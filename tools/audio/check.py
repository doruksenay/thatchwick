"""Otomatik kapı: teknik hatası olan adaylar incelemeye gelmez (Project 45 bölüm 5, 45a bölüm 4).

Aday başına: süre, sessizlik, gerçek tepe, yükseklik, döngü dikişi.
Set başına: benzerlik (neredeyse aynı adaylar) ve aile referansından uzaklık.
Eşikler audio/style/families.json'da; pilotlardan sonra ayarlanır.
"""
from __future__ import annotations

import json

import numpy as np

import post
from common import STYLE, read_json, write_json

SR = post.SR


# ---------------------------------------------------------------- öznitelik

def _mel_fb(n_fft: int, sr: int, n_mels: int = 40) -> np.ndarray:
    def hz2mel(f): return 2595 * np.log10(1 + f / 700)
    def mel2hz(m): return 700 * (10 ** (m / 2595) - 1)
    mels = np.linspace(hz2mel(30), hz2mel(sr / 2), n_mels + 2)
    hz = mel2hz(mels)
    bins = np.floor((n_fft + 1) * hz / sr).astype(int)
    fb = np.zeros((n_mels, n_fft // 2 + 1))
    for i in range(1, n_mels + 1):
        l, c, r = bins[i - 1], bins[i], bins[i + 1]
        if c > l:
            fb[i - 1, l:c] = (np.arange(l, c) - l) / (c - l)
        if r > c:
            fb[i - 1, c:r] = (r - np.arange(c, r)) / (r - c)
    return fb


_FB = None


def features(a: np.ndarray, sr: int = SR) -> np.ndarray:
    """Ses izi: log-mel ortalama + sapma (40+40) ve 16 noktalı zarf şekli."""
    global _FB
    x = a.mean(axis=1) if a.ndim == 2 else a
    n_fft, hop = 1024, 256
    if len(x) < n_fft:
        x = np.pad(x, (0, n_fft - len(x)))
    frames = np.lib.stride_tricks.sliding_window_view(x, n_fft)[::hop] * np.hanning(n_fft)
    spec = np.abs(np.fft.rfft(frames, axis=1)) ** 2
    if _FB is None:
        _FB = _mel_fb(n_fft, sr)
    logmel = np.log10(spec @ _FB.T + 1e-10)
    env = np.sqrt(np.mean(frames ** 2, axis=1))
    env = np.interp(np.linspace(0, len(env) - 1, 16), np.arange(len(env)), env)
    env = env / (env.max() + 1e-9)
    return np.concatenate([logmel.mean(0), logmel.std(0), env * 3]).astype(np.float32)


def distance(f1: np.ndarray, f2: np.ndarray) -> float:
    """0 = aynı, ~1 = ilgisiz (merkezlenmiş kosinüs uzaklığı)."""
    a, b = f1 - f1.mean(), f2 - f2.mean()
    return float(1 - np.dot(a, b) / (np.linalg.norm(a) * np.linalg.norm(b) + 1e-9))


# ---------------------------------------------------------------- aday

def seam_score(a: np.ndarray, sr: int = SR) -> float:
    """Döngü dikişi (dB): son 20 ms ile ilk 20 ms arasındaki RMS farkı + örnek sıçraması."""
    n = int(0.02 * sr)
    if len(a) < 4 * n:
        return 99.0
    jump = float(np.max(np.abs(a[0] - a[-1])))
    return abs(post.rms_db(a[-n:]) - post.rms_db(a[:n])) + jump * 40


def check_candidate(a: np.ndarray, fam: dict, loop: bool, sr: int = SR) -> dict:
    dur = len(a) / sr
    m = {
        "duration_s": round(dur, 3),
        "lufs": round(post.loudness(a, sr), 1),
        "true_peak_db": round(post.true_peak_db(a), 1),
        "rms_db": round(post.rms_db(a), 1),
    }
    flags = []
    lo, hi = fam.get("duration_range", [0.05, 30])
    if dur < lo or dur > hi * 1.05:
        flags.append(f"süre aralık dışı ({dur:.2f} sn, aile {lo}-{hi})")
    if m["rms_db"] < -60:
        flags.append("neredeyse sessiz")
    if m["true_peak_db"] > -0.5:
        flags.append(f"tepe çok yüksek ({m['true_peak_db']} dBTP)")
    if abs(m["lufs"] - fam.get("lufs", -18)) > 3 and dur >= 0.45:
        if m["true_peak_db"] >= -1.6:
            # ani vuruşlu seslerde tepe sınırı hedefe ulaşmayı engeller: hata değil, bilgi
            m["note"] = f"tepe sınırında, yükseklik {m['lufs']} (hedef {fam.get('lufs')})"
        else:
            flags.append(f"yükseklik hedeften uzak ({m['lufs']} / {fam.get('lufs')})")
    if loop:
        s = seam_score(a, sr)
        m["seam"] = round(s, 2)
        if s > 3.0:
            flags.append(f"döngü dikişi duyulabilir ({s:.1f})")
    m["flags"] = flags
    m["pass"] = not flags
    return m


# ---------------------------------------------------------------- set

def ref_path(family: str):
    return STYLE / "refs" / f"{family}.json"


def load_ref(family: str):
    d = read_json(ref_path(family), None)
    return None if d is None else np.array(d["features"], dtype=np.float32)


def save_ref(family: str, feats: list[np.ndarray], source: str) -> None:
    mean = np.mean(np.stack(feats), axis=0)
    write_json(ref_path(family), {"source": source, "features": [round(float(v), 5) for v in mean]})


def check_set(names: list[str], feats: list[np.ndarray], fam: dict, family: str) -> dict:
    """Benzerlik kapısı: 'çeşitli ama aynı aileden' (45a bölüm 4)."""
    dup_t = float(fam.get("dup_threshold", 0.035))
    out_t = float(fam.get("outlier_threshold", 0.55))
    n = len(feats)
    dmat = [[0.0] * n for _ in range(n)]
    for i in range(n):
        for j in range(i + 1, n):
            dmat[i][j] = dmat[j][i] = round(distance(feats[i], feats[j]), 4)
    res = {"pairs_dup": [], "nearest": {}, "ref_distance": {}, "flags": {}}
    dropped = set()
    for i in range(n):
        for j in range(i + 1, n):
            if dmat[i][j] < dup_t and i not in dropped and j not in dropped:
                res["pairs_dup"].append([names[i], names[j], dmat[i][j]])
                dropped.add(j)  # ikinciyi düşür
                res["flags"].setdefault(names[j], []).append(f"{names[i]} ile neredeyse aynı ({dmat[i][j]})")
    for i in range(n):
        others = [dmat[i][j] for j in range(n) if j != i]
        res["nearest"][names[i]] = min(others) if others else None
    ref = load_ref(family)
    if ref is not None:
        for i in range(n):
            d = round(distance(feats[i], ref), 4)
            res["ref_distance"][names[i]] = d
            if d > out_t:
                res["flags"].setdefault(names[i], []).append(f"aile referansından uzak ({d})")
    return res


def stt_flags(text: str) -> list[str]:
    """Köy dili denetimi: yazıya dökülen metinde yasak parça var mı?"""
    cfg = json.loads((STYLE / "koy-dili.json").read_text(encoding="utf-8"))
    low = text.lower()
    hits = [b for b in cfg["banned_substrings"] if b in low.replace(" ", "")]
    return [f"STT'de yasak parça: {', '.join(hits)} (\"{text}\")"] if hits else []
