"""Kabul edilen adayları oyuna yükseltir: Assets/Resources/Audio/…, soundbank.json, manifest.json, voices.json.

Kullanım:
  python tools/audio/promote.py            kabul edilmiş (accepted) bütün işleri yükselt
  python tools/audio/promote.py --git      aynısı + commit/push
  python tools/audio/promote.py --mock     voice_design kabulünde ElevenLabs'e gitmeden sahte voice_id

İş dosyasındaki 'pick':
  [c2, c5]          seçilen adaylar (inceleme sayfası yazar)
  all_passed        kapıdan geçen bütün adaylar (hece bankası için varsayılan)
Belge: Project 45 bölüm 7.
"""
from __future__ import annotations

import argparse
import shutil
import sys

import check
import el_api
import post
from common import (BANK_FILE, MANIFEST_FILE, ROOT, UNITY_AUDIO, VOICES_FILE, WORK, Job, all_jobs, family_of,
                    git_commit_push, load_env, masters_dir, now_iso, read_json, write_json)


def safe(key: str) -> str:
    return key.replace(".", "_").replace("/", "_").replace(" ", "_")


def load_bank() -> dict:
    return read_json(BANK_FILE, {"version": 1, "entries": []})


def upsert(bank: dict, entry: dict, append: bool) -> None:
    for i, e in enumerate(bank["entries"]):
        if e["key"] == entry["key"]:
            if append:
                e["clips"] = e.get("clips", []) + [c for c in entry.get("clips", []) if c not in e.get("clips", [])]
                for L in entry.get("layers", []):
                    tgt = next((x for x in e.setdefault("layers", []) if x["name"] == L["name"]), None)
                    if tgt:
                        tgt["clips"] += [c for c in L["clips"] if c not in tgt["clips"]]
                    else:
                        e["layers"].append(L)
            else:
                bank["entries"][i] = entry
            return
    bank["entries"].append(entry)
    bank["entries"].sort(key=lambda e: e["key"])


def base_entry(job: Job, key: str, fam: dict) -> dict:
    g = lambda k, d: job.get(k, fam.get(k, d))
    return {
        "key": key,
        "group": fam["group"],
        "family": fam["name"],
        "freq": str(job.get("freq", "B")),
        "loop": bool(job.get("loop", False)),
        "clips": [],
        "layers": [],
        "pitchVar": float(g("pitch_var", 0.05)),
        "volVarDb": float(g("vol_var_db", 2.0)),
        "timingJitterMs": float(g("timing_jitter_ms", 0)),
        "cooldownMs": float(g("cooldown_ms", 60)),
        "maxVoices": int(g("max_voices", 3)),
        "volumeDb": float(job.get("volume_db", 0.0)),
        "context": str(job.get("context", "")),
        "spotOf": str(job.get("spot_of", "")),
        "spotIntervalMin": float((job.get("spot_interval_s") or [0, 0])[0]),
        "spotIntervalMax": float((job.get("spot_interval_s") or [0, 0])[1]),
        "todWeights": [float(x) for x in (job.get("tod_weights") or [1, 1, 1, 1])],      # şafak, gündüz, akşam, gece
        "seasonWeights": [float(x) for x in (job.get("season_weights") or [1, 1, 1, 1])],  # ilkbahar, yaz, sonbahar, kış
        "syllables": [],
    }


def picked(job: Job, report: dict) -> dict[str, list[str]]:
    """{grup: [aday adları]}"""
    pick = job.get("pick")
    if pick in (["all"], ["all_passed"], "all"):
        pick = "all_passed"
    if job.get("kind") == "voice_syllables" and not pick:
        pick = "all_passed"
    out: dict[str, list[str]] = {}
    for gname, gr in report["groups"].items():
        cands = gr["candidates"]
        if pick == "all_passed":
            ok = [n for n, m in cands.items() if m["pass"]]
            if job.get("kind") == "voice_syllables":
                ok = ok[:1]  # hece başına en iyi (ilk geçen) kayıt
            out[gname] = ok
        else:
            out[gname] = [n for n in (pick or []) if n in cands]
    return {k: v for k, v in out.items() if v}


def sequence_combos(sel: dict[str, list[str]], layer_names: list[str], limit: int) -> list[list[str]]:
    """Sıralı katmanlardan (ör. kepçe + dökülüş) dengeli birleşimler: her seçilen aday en az bir kez kullanılır."""
    import itertools
    pools = [sel.get(L, []) for L in layer_names]
    if not all(pools):
        raise ValueError("sıralı katmanın her parçasından en az bir aday seçilmeli")
    combos = list(itertools.product(*pools))
    n = max(max(len(p) for p in pools), min(limit, len(combos)))
    # çapraz sırayla al: (a1,b1), (a2,b2)… önce, kalanlar sonra
    first = [tuple(p[i % len(p)] for p in pools) for i in range(max(len(p) for p in pools))]
    out = list(dict.fromkeys(first + combos))
    return [list(c) for c in out[:n]]


def place_sequence(job: Job, sel, layer_names, key, group_dir, manifest, fam) -> list[str]:
    """sequence: true ise katmanlar üst üste değil art arda çalar; her birleşim tek dosya olur."""
    import numpy as np
    gap = float(job.get("sequence_gap_s", 0.12))
    clips = []
    for i, combo in enumerate(sequence_combos(sel, layer_names, int(job.get("keep", 4)) * 2)):
        parts = []
        for j, cname in enumerate(combo):
            a = post.decode(WORK / job.id / "cand" / f"{cname}.ogg")
            if j:
                parts.append(np.zeros((int(post.SR * gap),) + a.shape[1:], dtype=a.dtype))
            parts.append(a)
        out_name = f"{safe(key)}_{i + 1}"
        dst = group_dir / f"{out_name}.ogg"
        dst.parent.mkdir(parents=True, exist_ok=True)
        post.encode_ogg(np.concatenate(parts), dst, q=int(fam.get("vorbis_q", 4)))
        rel = str(dst.relative_to(ROOT)).replace("\\", "/")
        manifest["files"] = [f for f in manifest["files"] if f["path"] != rel]
        manifest["files"].append({"path": rel, "job": job.id, "candidate": "+".join(combo), "kind": job.get("kind"),
                                  "prompt": " | ".join(L.get("prompt", "") for L in job.get("layers") or []),
                                  "model": job.get("model"), "date": now_iso(), "credits_job": job.get("credits_spent"),
                                  "license": "ElevenLabs ücretli plan ticari lisans"})
        clips.append(str(dst.relative_to(ROOT / "Assets" / "Resources")).replace("\\", "/").rsplit(".", 1)[0])
    return clips


def promote_voice_design(job: Job, mock: bool) -> None:
    pick = job.get("pick") or []
    if len(pick) != 1:
        raise ValueError("voice_design için tek önizleme seçilmeli (pick: [cN])")
    gen_id = (job.get("previews") or {}).get(pick[0])
    if not gen_id:
        raise ValueError(f"{pick[0]} için generated_voice_id yok")
    vkey = job.get("voice_key") or job.id
    api = el_api.client(mock)
    vid = api.create_voice(f"Thatchwick {vkey}", str(job.get("description", ""))[:900], gen_id)
    voices = read_json(VOICES_FILE, {})
    voices[vkey] = {"voice_id": vid, "generated_voice_id": gen_id, "job": job.id, "created": now_iso(),
                    "description": job.get("description", "")}
    write_json(VOICES_FILE, voices)
    src = WORK / job.id / "master" / f"{pick[0]}.wav"
    if src.exists():
        d = masters_dir() / "voices"
        d.mkdir(parents=True, exist_ok=True)
        shutil.copy2(src, d / f"{vkey}_preview.wav")
    print(f"[{job.id}] ses oluşturuldu: {vkey} → {vid}")


def promote_job(job: Job, mock: bool) -> list[str]:
    """Yükseltir; değişen yolları döndürür."""
    report = read_json(WORK / job.id / "report.json", None)
    if job.get("kind") == "voice_design":
        promote_voice_design(job, mock)
        job.meta.update({"status": "done", "promoted_at": now_iso()})
        job.save()
        return [str(VOICES_FILE.relative_to(ROOT)), str(job.path.relative_to(ROOT))]
    if report is None:
        raise RuntimeError(f"{job.id}: rapor yok (.audio_work/{job.id}); bu makinede üretilmemiş olabilir")
    fam = family_of(job)
    sel = picked(job, report)
    if not sel:
        raise ValueError(f"{job.id}: seçili aday yok (pick)")
    bank = load_bank()
    manifest = read_json(MANIFEST_FILE, {"files": []})
    key = str(job.get("key") or job.id)
    group_dir = UNITY_AUDIO / fam["group"] / fam["name"]
    mdir = masters_dir() / fam["name"]
    feats_for_ref = []
    kind = job.get("kind")

    def place(cname: str, out_name: str) -> str:
        src = WORK / job.id / "cand" / f"{cname}.ogg"
        dst = group_dir / f"{out_name}.ogg"
        dst.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(src, dst)
        m = WORK / job.id / "master" / f"{cname}.wav"
        if m.exists():
            mdir.mkdir(parents=True, exist_ok=True)
            shutil.copy2(m, mdir / f"{out_name}.wav")
        if job.get("pilot"):
            # bulutta ana kopya yok (aday dalına yalnız OGG gider): referans izi OGG'den çıkarılır
            feats_for_ref.append(check.features(post.decode(m if m.exists() else src)))
        info = next(gr["candidates"][cname] for gr in report["groups"].values() if cname in gr["candidates"])
        manifest["files"] = [f for f in manifest["files"] if f["path"] != str(dst.relative_to(ROOT)).replace("\\", "/")]
        manifest["files"].append({
            "path": str(dst.relative_to(ROOT)).replace("\\", "/"), "job": job.id, "candidate": cname, "kind": kind,
            "prompt": info.get("prompt") or info.get("text") or info.get("syllable"),
            "model": info.get("model") or job.get("model"), "date": now_iso(), "credits_job": job.get("credits_spent"),
            "license": "ElevenLabs ücretli plan; müzikte Indie Game koşulu (Project 43 bölüm 3.3)" if kind == "music"
                       else "ElevenLabs ücretli plan ticari lisans"})
        return str(dst.relative_to(ROOT / "Assets" / "Resources")).replace("\\", "/").rsplit(".", 1)[0]

    if kind == "voice_lines":
        for gname, names in sel.items():
            e = base_entry(job, f"voice.{job.get('voice')}.{gname}", fam)
            e["clips"] = [place(n, f"{safe(e['key'])}_{i + 1}") for i, n in enumerate(names)]
            upsert(bank, e, bool(job.get("append", False)))
    elif kind == "voice_syllables":
        e = base_entry(job, f"voice.{job.get('voice')}.syl", fam)
        for gname, names in sorted(sel.items()):
            syl = gname[len("syl_"):]
            e["clips"].append(place(names[0], f"{safe(e['key'])}_{syl}"))
            e["syllables"].append(syl)
        upsert(bank, e, False)
    else:
        e = base_entry(job, key, fam)
        layer_names = [L["name"] for L in (job.get("layers") or [])]
        if len(layer_names) > 1 and job.get("sequence"):
            e["clips"] = place_sequence(job, sel, layer_names, key, group_dir, manifest, fam)
        elif len(layer_names) > 1:
            for L in layer_names:
                names = sel.get(L, [])
                e["layers"].append({"name": L, "clips": [place(n, f"{safe(key)}__{L}_{i + 1}") for i, n in enumerate(names)]})
        else:
            names = next(iter(sel.values()))
            e["clips"] = [place(n, f"{safe(key)}_{i + 1}") for i, n in enumerate(names)]
        upsert(bank, e, bool(job.get("append", False)))

    if feats_for_ref:
        check.save_ref(fam["name"], feats_for_ref, job.id)
    bank["updated"] = now_iso()
    write_json(BANK_FILE, bank)
    write_json(MANIFEST_FILE, manifest)
    job.meta.update({"status": "done", "promoted_at": now_iso()})
    job.save()
    print(f"[{job.id}] yükseltildi: {sum(len(v) for v in sel.values())} dosya → {group_dir.relative_to(ROOT)}")
    return [str(UNITY_AUDIO.relative_to(ROOT)), str(MANIFEST_FILE.relative_to(ROOT)),
            str(job.path.relative_to(ROOT)), "audio/style/refs"]


def main(argv=None) -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--git", action="store_true")
    ap.add_argument("--mock", action="store_true")
    ap.add_argument("--job")
    a = ap.parse_args(argv)
    load_env()
    changed, done = set(), []
    for j in all_jobs():
        if j.get("status") != "accepted" or (a.job and j.id != a.job):
            continue
        try:
            changed.update(promote_job(j, a.mock))
            done.append(j.id)
        except Exception as e:
            print(f"[{j.id}] yükseltilemedi: {e}", file=sys.stderr)
    if a.git and done:
        paths = [p for p in changed if (ROOT / p).exists()]
        git_commit_push(paths, f"ses: yükseltildi {', '.join(done)}")
    if not done:
        print("yükseltilecek kabul edilmiş iş yok")
    return 0


if __name__ == "__main__":
    sys.exit(main())
