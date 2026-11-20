"""Ses kuyruğu işçisi: hazır işleri ElevenLabs'te üretir, son işlemden ve kapıdan geçirir, incelemeye bırakır.

Kullanım (repo kökünden):
  python tools/audio/worker.py --list              hazır işleri ve tahmini krediyi göster
  python tools/audio/worker.py --once              hazır işlerin hepsini çalıştır
  python tools/audio/worker.py --once --git        aynısı + sahiplenme ve sonuç commit'leri (yoklayıcı böyle çağırır)
  python tools/audio/worker.py --job s101          tek iş
  python tools/audio/worker.py --once --mock       kredisiz deneme (sentetik ses)

Belge: Project 45 (hat), 45a (doğallık).
"""
from __future__ import annotations

import argparse
import itertools
import random
import re
import shutil
import sys
import traceback
import zlib
from pathlib import Path

import check
import el_api
import koy_dili
import post
from common import (BUDGET_FILE, ROOT, VOICES_FILE, WORK, Job, all_jobs, family_of, freq_counts, git_commit_push,
                    host, job_by_id, load_env, load_families, now_iso, read_json, ready_jobs, review_copy_dir,
                    write_json)

SOUND_KINDS = ("sfx", "stinger", "spot")
BED_KINDS = ("ambience",)


# ---------------------------------------------------------------- yardımcılar

def prompt_of(job: Job) -> str:
    p = job.get("prompt")
    if p:
        return str(p).strip()
    lines = [l for l in job.body.splitlines() if l.strip() and not l.lstrip().startswith(("#", "|", ">"))]
    return " ".join(l.strip() for l in lines).strip()


def full_prompt(fam: dict, text: str, extra: str = "") -> str:
    parts = [fam.get("prefix", "").strip(), text.strip()]
    if extra:
        parts.append(extra)
    return " ".join(p for p in parts if p)


def axis_variants(axes: dict | None, n: int, seed: int) -> list[str]:
    """Eksenlerden dengeli n kombinasyon (45a bölüm 2.1)."""
    if not axes:
        return [""] * n
    keys = list(axes.keys())
    combos = list(itertools.product(*[axes[k] for k in keys]))
    r = random.Random(seed)
    r.shuffle(combos)
    out = []
    while len(out) < n:
        out.extend(combos)
    return ["Details: " + ", ".join(f"{v}" for v in c) + "." for c in out[:n]]


def table_rows(body: str) -> list[tuple[str, str]]:
    """'| anahtar | metin |' satırları (başlık ve ayraç hariç)."""
    rows = []
    for line in body.splitlines():
        s = line.strip()
        if not s.startswith("|") or set(s.replace("|", "").strip()) <= set("-: "):
            continue
        cells = [c.strip() for c in s.strip("|").split("|")]
        if len(cells) < 2 or cells[0].lower() in ("anahtar", "key"):
            continue
        rows.append((cells[0], cells[1]))
    return rows


def estimate(job: Job) -> int:
    """Kredi tahmini (families.json -> credit)."""
    c = load_families()["credit"]
    kind = job.get("kind")
    gen, _ = freq_counts(job)
    if kind in SOUND_KINDS + BED_KINDS:
        layers = job.get("layers")
        if layers:
            tot = 0
            for L in layers:
                d = L.get("duration_s", job.get("duration_s"))
                tot += int(L.get("generate", gen)) * (d * c["sfx_per_sec"] if d else c["sfx_auto_duration"])
            return int(tot)
        d = job.get("duration_s")
        return int(gen * (d * c["sfx_per_sec"] if d else c["sfx_auto_duration"]))
    if kind == "voice_design":
        return int(gen * c["voice_design"])
    if kind == "voice_lines":
        chars = sum(len(t) for _, t in table_rows(job.body))
        stt = sum(1 for _ in table_rows(job.body)) * gen * c["stt_per_min"] / 20  # ~3 sn klip
        return int(chars * gen * c["tts_per_char"] + stt)
    if kind == "voice_syllables":
        n = int(job.get("count", 150))
        return int(n * 4 * int(job.get("takes", 2)) * c["tts_per_char"])
    if kind == "music":
        ms = job.get("length_ms") or 120000
        return int(gen * ms / 60000 * c["music_per_min"])
    return 0


def log_budget(job: Job, est: int, before, after) -> None:
    if not BUDGET_FILE.exists():
        BUDGET_FILE.write_text("# Ses kredi günlüğü\n\n| tarih | iş | tür | tahmin | harcanan | kalan |\n|---|---|---|---|---|---|\n",
                               encoding="utf-8")
    spent = (before - after) if (before is not None and after is not None) else "?"
    with BUDGET_FILE.open("a", encoding="utf-8", newline="\n") as f:
        f.write(f"| {now_iso()} | {job.id} | {job.get('kind')} | {est} | {spent} | {after if after is not None else '?'} |\n")


# ---------------------------------------------------------------- üretim

class Gen:
    def __init__(self, api, mock: bool):
        self.api = api
        self.mock = mock
        self.cfg = load_families()["api"]

    def _fmt_retry(self, fn, fmt_key: str, **kw):
        fmt = self.cfg[fmt_key]
        try:
            return fn(output_format=fmt, **kw)
        except el_api.ApiError as e:
            if e.status in (400, 403, 422) and fmt != "mp3_44100_128":
                print(f"  biçim {fmt} reddedildi, mp3_44100_128 deneniyor")
                return fn(output_format="mp3_44100_128", **kw)
            raise

    def sound(self, text, duration, influence, loop, model) -> bytes:
        try:
            return self._fmt_retry(self.api.sound, "sfx_format", text=text, model_id=model, duration=duration,
                                   prompt_influence=influence, loop=loop)
        except el_api.ApiError as e:
            if loop and e.status in (400, 422):  # model döngü parametresini tanımıyor: kendimiz kapatırız
                return self._fmt_retry(self.api.sound, "sfx_format", text=text, model_id=model, duration=duration,
                                       prompt_influence=influence, loop=False)
            raise

    def tts(self, voice_id, text, seed) -> bytes:
        return self._fmt_retry(self.api.tts, "tts_format", voice_id=voice_id, text=text,
                               model_id=self.cfg["tts_model"], seed=seed)

    def music(self, prompt, plan, length_ms, instrumental) -> bytes:
        return self._fmt_retry(self.api.music, "music_format", prompt=prompt, plan=plan, length_ms=length_ms,
                               instrumental=instrumental, model_id=self.cfg.get("music_model"))


def _save(raw_dir: Path, name: str, data: bytes) -> Path:
    raw_dir.mkdir(parents=True, exist_ok=True)
    ext = ".wav" if data[:4] == b"RIFF" else ".mp3"
    p = raw_dir / f"{name}{ext}"
    p.write_bytes(data)
    return p


def generate(job: Job, g: Gen) -> dict:
    """Ham adayları üretir. Dönen: {grup_adı: [(aday_adı, ham_yol, ek_bilgi)]}"""
    kind = job.get("kind")
    fam = family_of(job)
    raw = WORK / job.id / "raw"
    gen, _ = freq_counts(job)
    seed = int(job.get("seed", zlib.crc32(job.id.encode()) % 100000))
    groups: dict[str, list] = {}

    if kind in SOUND_KINDS + BED_KINDS:
        loop = bool(job.get("loop", False))
        model = job.get("model") or (g.cfg["sfx_loop_model"] if loop else g.cfg["sfx_model"])
        infl = job.get("prompt_influence", fam.get("prompt_influence"))
        jitter = float(job.get("duration_jitter", 0.1 if kind in SOUND_KINDS else 0.0))
        layers = job.get("layers") or [{"name": "main", "prompt": prompt_of(job)}]
        for L in layers:
            n = int(L.get("generate", gen))
            dur0 = L.get("duration_s", job.get("duration_s"))
            variants = axis_variants(L.get("axes", job.get("axes")), n, seed + len(groups))
            rr = random.Random(seed)
            items = []
            for i in range(n):
                dur = dur0 * rr.uniform(1 - jitter, 1 + jitter) if (dur0 and jitter) else dur0
                text = full_prompt(fam, L.get("prompt") or prompt_of(job), variants[i])
                name = f"{L['name']}_c{i + 1}" if len(layers) > 1 else f"c{i + 1}"
                print(f"  {name}: {text[-90:]}")
                p = _save(raw, name, g.sound(text, dur, infl, loop, model))
                items.append((name, p, {"prompt": text, "duration": dur, "model": model}))
            groups[L["name"]] = items

    elif kind == "voice_design":
        items, previews = [], {}
        desc = job.get("description") or prompt_of(job)
        k = 0
        for _ in range(max(1, gen)):
            for pv in g.api.voice_design(desc, g.cfg["voice_design_model"], job.get("sample_text")):
                k += 1
                name = f"c{k}"
                p = _save(raw, name, pv["audio"])
                previews[name] = pv["generated_voice_id"]
                items.append((name, p, {"generated_voice_id": pv["generated_voice_id"]}))
        job.meta["previews"] = previews
        groups["main"] = items

    elif kind == "voice_lines":
        vid = voice_id(job)
        for key, text in table_rows(job.body):
            items = []
            for i in range(gen):
                name = f"{key}_c{i + 1}"
                p = _save(raw, name, g.tts(vid, text, seed + i))
                items.append((name, p, {"text": text, "model": g.cfg["tts_model"]}))
            groups[key] = items

    elif kind == "voice_syllables":
        vid = voice_id(job)
        cfg = koy_dili._cfg()
        syl = koy_dili.bank(int(job.get("count", 150)), seed=int(job.get("bank_seed", 1)))
        chunks = koy_dili.chunk_text(syl, cfg["per_chunk"], cfg["separator"])
        items = []
        for t in range(int(job.get("takes", 2))):
            for ci, text in enumerate(chunks):
                p = _save(raw, f"chunk{ci + 1}_t{t + 1}", g.tts(vid, text, seed + 31 * t + ci))
                part = syl[ci * cfg["per_chunk"]:(ci + 1) * cfg["per_chunk"]]
                items.append((f"chunk{ci + 1}_t{t + 1}", p, {"syllables": part, "take": t + 1}))
        groups["chunks"] = items

    elif kind == "music":
        plan = job.get("composition_plan")
        if not plan and job.get("plan_file"):
            plan = read_json(ROOT / job.get("plan_file"), None)
        text = full_prompt(fam, prompt_of(job)) if not plan else None
        items = []
        for i in range(gen):
            name = f"c{i + 1}"
            p = _save(raw, name, g.music(text, plan, job.get("length_ms"), bool(job.get("instrumental", True))))
            items.append((name, p, {"prompt": text, "model": g.cfg.get("music_model") or "music_v1"}))
        groups["main"] = items
    else:
        raise ValueError(f"bilinmeyen tür: {kind}")
    return groups


def voice_id(job: Job) -> str:
    voices = read_json(VOICES_FILE, {})
    v = job.get("voice")
    if v not in voices:
        raise RuntimeError(f"ses '{v}' audio/voices.json'da yok; önce voice_design işini kabul et")
    return voices[v]["voice_id"]


# ---------------------------------------------------------------- son işlem + kapı

def finish(job: Job, groups: dict, g: Gen) -> dict:
    fam = family_of(job)
    loop = bool(job.get("loop", False))
    cand_dir = WORK / job.id / "cand"
    master_dir = WORK / job.id / "master"
    if cand_dir.exists():
        shutil.rmtree(cand_dir)
    report = {"job": job.id, "kind": job.get("kind"), "family": fam["name"], "created": now_iso(), "groups": {}}

    if job.get("kind") == "voice_syllables":
        groups = _slice_syllables(groups["chunks"], report)

    for gname, items in groups.items():
        names, feats, cands = [], [], {}
        for name, path, info in items:
            a = path if not isinstance(path, Path) else post.decode(path)
            a = post.process(a, fam, loop=loop)
            m = check.check_candidate(a, fam, loop)
            m.update({k: v for k, v in info.items() if k not in ("syllables",)})
            if job.get("kind") == "voice_lines" and job.get("stt", True):
                try:
                    txt = g.api.transcribe(path.read_bytes(), path.name, g.cfg["stt_model"])
                    m["stt"] = txt
                    sf = check.stt_flags(txt)
                    if sf:
                        m["flags"] += sf
                        m["pass"] = False
                except Exception as e:  # STT çalışmazsa aday düşmez, not düşülür
                    m["stt_error"] = str(e)[:200]
            post.write_master(a, master_dir / f"{name}.wav")
            post.encode_ogg(a, cand_dir / f"{name}.ogg", q=int(fam.get("vorbis_q", 4)))
            cands[name] = m
            names.append(name)
            feats.append(check.features(a))
        sres = check.check_set(names, feats, fam, fam["name"]) if len(names) > 1 or check.load_ref(fam["name"]) is not None else {}
        for nm, fl in (sres.get("flags") or {}).items():
            cands[nm]["flags"] = cands[nm]["flags"] + fl
            # benzerlik ve referans uyarıları adayı düşürmez; incelemede işaretlenir (pilotla eşik ayarlanır)
            cands[nm]["similarity_warning"] = True
        report["groups"][gname] = {"candidates": cands, "set": {k: v for k, v in sres.items() if k != "flags"}}
    write_json(WORK / job.id / "report.json", report)
    return report


def _slice_syllables(items, report) -> dict:
    """Hece dizisi kayıtlarını hecelere böler; sayı tutmayan parçayı atlar."""
    out: dict[str, list] = {}
    misses = []
    for name, path, info in items:
        a = post.to_mono(post.decode(path))
        segs = post.slice_on_silence(a)
        syl = info["syllables"]
        if len(segs) != len(syl):
            misses.append(f"{name}: {len(segs)} parça / {len(syl)} hece")
            continue
        for s, seg in zip(syl, segs):
            out.setdefault(f"syl_{s}", []).append((f"syl_{s}_t{info['take']}", seg, {"syllable": s}))
    report["slice_misses"] = misses
    return out


def copy_for_review(job: Job) -> None:
    dst = review_copy_dir()
    if not dst:
        return
    d = dst / job.id
    if d.exists():
        shutil.rmtree(d)
    shutil.copytree(WORK / job.id / "cand", d)


# ---------------------------------------------------------------- ana akış

def run_job(job: Job, api, mock: bool, use_git: bool) -> None:
    est = estimate(job)
    cost_max = job.get("cost_max")
    if cost_max is not None and est > int(cost_max):
        job.meta["status"] = "blocked"
        job.meta["error"] = f"tahmin {est} kredi > cost_max {cost_max}"
        job.save()
        print(f"[{job.id}] durdu: {job.meta['error']}")
        return
    try:
        before = api.credits_left()
    except Exception as e:  # anahtarın "User: read" izni yoksa kredi okunamaz; üretim yine sürer
        print(f"[{job.id}] kalan kredi okunamadı ({type(e).__name__}); cost_max sınırıyla devam")
        before = None
    reserve = int(load_families()["credit"]["reserve_ratio"] * (before or 0))
    if before is not None and before - est < reserve:
        print(f"[{job.id}] kredi yetmez (kalan {before}, gerekli {est}, yedek {reserve}); atlandı")
        return
    job.meta.update({"status": "claimed", "claimed_by": host(), "claimed_at": now_iso()})
    job.save()
    if use_git:
        git_commit_push([str(job.path.relative_to(ROOT))], f"ses: {job.id} sahiplenildi")
    print(f"[{job.id}] üretiliyor ({job.get('kind')}, tahmin {est} kredi)")
    try:
        g = Gen(api, mock)
        groups = generate(job, g)
        report = finish(job, groups, g)
        total = sum(len(gr["candidates"]) for gr in report["groups"].values())
        passed = sum(1 for gr in report["groups"].values() for m in gr["candidates"].values() if m["pass"])
        try:
            after = api.credits_left()
        except Exception:
            after = None
        job.meta.update({"status": "review" if passed else "blocked", "candidates": total, "passed": passed,
                         "credits_spent": (before - after) if before is not None and after is not None else None,
                         "generated_at": now_iso()})
        if passed:
            job.meta.pop("error", None)
        else:
            job.meta["error"] = "hiçbir aday kapıdan geçmedi; prompt'u değiştir"
        job.save()
        copy_for_review(job)
        log_budget(job, est, before, after)
        print(f"[{job.id}] {passed}/{total} aday kapıdan geçti → {job.meta['status']}")
    except Exception as e:
        job.meta.update({"status": "blocked", "error": f"{type(e).__name__}: {str(e)[:300]}"})
        job.save()
        traceback.print_exc()
    if use_git:
        git_commit_push([str(job.path.relative_to(ROOT)), str(BUDGET_FILE.relative_to(ROOT))],
                        f"ses: {job.id} → {job.meta['status']}")


def main(argv=None) -> int:
    ap = argparse.ArgumentParser(description="Thatchwick ses kuyruğu işçisi")
    ap.add_argument("--list", action="store_true")
    ap.add_argument("--once", action="store_true")
    ap.add_argument("--job")
    ap.add_argument("--max", type=int, default=50)
    ap.add_argument("--mock", action="store_true", help="kredisiz sentetik ses")
    ap.add_argument("--git", action="store_true", help="sahiplenme/sonuç commit'leri")
    ap.add_argument("--worker", default="yerel")
    a = ap.parse_args(argv)
    load_env()

    if a.list:
        jobs = ready_jobs(a.worker)
        tot = 0
        for j in jobs:
            e = estimate(j)
            tot += e
            print(f"{j.id:10} {j.get('kind'):16} {j.get('family'):9} freq={j.get('freq', '-')} ~{e} kredi")
        print(f"toplam ~{tot} kredi, {len(jobs)} iş")
        ids = {j.id for j in jobs}
        waiting = [j for j in all_jobs() if j.get("status") == "ready" and j.id not in ids]
        if waiting:
            print("bekleyen (onay ya da bağımlılık):", ", ".join(j.id for j in waiting))
        return 0

    api = el_api.client(a.mock)
    if a.job:
        j = job_by_id(a.job)
        if not j:
            print("iş yok:", a.job)
            return 1
        run_job(j, api, a.mock, a.git)
        return 0
    if a.once:
        for j in ready_jobs(a.worker)[: a.max]:
            run_job(j, api, a.mock, a.git)
        return 0
    ap.print_help()
    return 0


if __name__ == "__main__":
    sys.exit(main())
