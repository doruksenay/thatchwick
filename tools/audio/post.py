"""Son işlem: çöz → (mono) → kırp → yükseklik eşitle → tepe sınırla → döngü dikişi → OGG.

Gereken: ffmpeg (PATH'te), numpy, scipy, soundfile, pyloudnorm.
Belge: Project 45 bölüm 1 ve 5.
"""
from __future__ import annotations

import subprocess
import tempfile
from pathlib import Path

import numpy as np
import soundfile as sf
from scipy.signal import resample_poly

SR = 48000


def decode(path: Path, sr: int = SR) -> np.ndarray:
    """Her biçimi (mp3, wav, ogg) float32 (n, kanal) diziye çevirir."""
    with tempfile.TemporaryDirectory() as td:
        out = Path(td) / "d.wav"
        subprocess.run(["ffmpeg", "-v", "error", "-y", "-i", str(path), "-ar", str(sr), "-c:a", "pcm_f32le", str(out)],
                       check=True)
        data, _ = sf.read(out, dtype="float32", always_2d=True)
    return data


def to_mono(a: np.ndarray) -> np.ndarray:
    return a.mean(axis=1, keepdims=True) if a.shape[1] > 1 else a


def to_stereo(a: np.ndarray) -> np.ndarray:
    return np.repeat(a, 2, axis=1) if a.shape[1] == 1 else a[:, :2]


def db(x: float) -> float:
    return 20 * np.log10(max(x, 1e-12))


def peak_db(a: np.ndarray) -> float:
    return db(float(np.max(np.abs(a)))) if a.size else -120.0


def true_peak_db(a: np.ndarray) -> float:
    if a.size == 0:
        return -120.0
    up = resample_poly(a, 4, 1, axis=0)
    return db(float(np.max(np.abs(up))))


def rms_db(a: np.ndarray) -> float:
    return db(float(np.sqrt(np.mean(np.square(a))))) if a.size else -120.0


def loudness(a: np.ndarray, sr: int = SR) -> float:
    """Tümleşik yükseklik (LUFS). 0,4 sn'den kısa seslerde RMS (dBFS) yaklaşığı."""
    if a.shape[0] < int(0.45 * sr):
        return rms_db(a)
    import pyloudnorm as pyln
    meter = pyln.Meter(sr)
    val = meter.integrated_loudness(a if a.shape[1] > 1 else a[:, 0])
    return float(val) if np.isfinite(val) else rms_db(a)


def trim(a: np.ndarray, sr: int = SR, rel_db: float = -48.0, pre_ms: float = 4, post_ms: float = 40) -> np.ndarray:
    """Baştaki ve sondaki sessizliği kırpar (tepeye göre bağıl eşik)."""
    env = np.max(np.abs(a), axis=1)
    if env.size == 0 or env.max() <= 0:
        return a
    thr = env.max() * 10 ** (rel_db / 20)
    idx = np.where(env > thr)[0]
    s = max(0, idx[0] - int(pre_ms / 1000 * sr))
    e = min(len(env), idx[-1] + int(post_ms / 1000 * sr))
    return a[s:e]


def fades(a: np.ndarray, sr: int = SR, fin_ms: float = 3, fout_ms: float = 15) -> np.ndarray:
    a = a.copy()
    n_in = min(len(a) // 4, int(fin_ms / 1000 * sr))
    n_out = min(len(a) // 4, int(fout_ms / 1000 * sr))
    if n_in > 0:
        a[:n_in] *= np.linspace(0, 1, n_in, dtype=np.float32)[:, None]
    if n_out > 0:
        a[-n_out:] *= np.linspace(1, 0, n_out, dtype=np.float32)[:, None]
    return a


def normalize(a: np.ndarray, target_lufs: float, sr: int = SR, ceiling_db: float = -1.0) -> np.ndarray:
    cur = loudness(a, sr)
    if cur <= -90:
        return a
    out = a * (10 ** ((target_lufs - cur) / 20))
    tp = true_peak_db(out)
    if tp > ceiling_db:  # tepe sınırı: yumuşak değil, düz kısma (kırpılma olmasın diye)
        out = out * (10 ** ((ceiling_db - tp) / 20))
    return out.astype(np.float32)


def make_loop(a: np.ndarray, sr: int = SR, xfade_s: float = 0.6) -> np.ndarray:
    """Sonu başa çapraz geçişle bindirir; çıkan dosya dikişsiz döner (eşit güç)."""
    n = len(a)
    L = int(min(xfade_s * sr, n * 0.15))
    if L < 32:
        return a
    t = np.linspace(0, 1, L, dtype=np.float32)[:, None]
    fin, fout = np.sin(t * np.pi / 2), np.cos(t * np.pi / 2)
    out = a[: n - L].copy()
    out[:L] = a[:L] * fin + a[n - L:] * fout
    return out


def slice_on_silence(a: np.ndarray, sr: int = SR, rel_db: float = -38.0, min_gap_ms: float = 90,
                     min_len_ms: float = 60) -> list[np.ndarray]:
    """Hece dizisini sessizliklerden böler (voice_syllables işleri)."""
    env = np.max(np.abs(a), axis=1)
    if env.size == 0 or env.max() <= 0:
        return []
    win = max(1, int(0.005 * sr))
    sm = np.convolve(env, np.ones(win) / win, mode="same")
    loud = sm > env.max() * 10 ** (rel_db / 20)
    segs, start, quiet = [], None, 0
    gap = int(min_gap_ms / 1000 * sr)
    for i, v in enumerate(loud):
        if v:
            if start is None:
                start = i
            quiet = 0
        elif start is not None:
            quiet += 1
            if quiet >= gap:
                end = i - quiet + 1
                if end - start >= int(min_len_ms / 1000 * sr):
                    segs.append(a[max(0, start - int(0.008 * sr)): end + int(0.02 * sr)])
                start, quiet = None, 0
    if start is not None and len(a) - start >= int(min_len_ms / 1000 * sr):
        segs.append(a[start:])
    return segs


def process(a: np.ndarray, fam: dict, loop: bool = False, sr: int = SR) -> np.ndarray:
    a = to_mono(a) if fam.get("mono", True) else to_stereo(a)
    if loop:
        a = make_loop(a, sr)
    elif fam.get("trim", True):
        a = fades(trim(a, sr), sr)
    else:
        a = fades(a, sr, fin_ms=20, fout_ms=400)
    return normalize(a, float(fam.get("lufs", -18)), sr)


def write_master(a: np.ndarray, path: Path, sr: int = SR) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    sf.write(path, a, sr, subtype="PCM_24")


def encode_ogg(a: np.ndarray, path: Path, q: int = 4, sr: int = SR) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory() as td:
        tmp = Path(td) / "e.wav"
        sf.write(tmp, a, sr, subtype="FLOAT")
        # 44,1 kHz: mobilde yeterli ve dosya küçük
        subprocess.run(["ffmpeg", "-v", "error", "-y", "-i", str(tmp), "-ar", "44100", "-c:a", "libvorbis",
                        "-q:a", str(q), str(path)], check=True)
