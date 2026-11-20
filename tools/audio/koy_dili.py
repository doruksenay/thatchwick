"""Köy dili: anlamsız hece üretici (Project 14 bölüm 11, 45a bölüm 2.5).

Deterministiktir: aynı tohum her seferinde aynı heceleri verir. Hece bankası işleri (kind: voice_syllables)
ve replik işleri bu modülü kullanır. Oyun içindeki birleştirme C# tarafında (GibberishComposer).
"""
from __future__ import annotations

import json
import random

from common import STYLE


def _cfg() -> dict:
    return json.loads((STYLE / "koy-dili.json").read_text(encoding="utf-8"))


def _allowed(syl: str, cfg: dict) -> bool:
    s = syl.lower()
    return not any(b in s for b in cfg["banned_substrings"])


def inventory(cfg: dict | None = None) -> list[str]:
    """Kurallara uyan bütün heceler (sıralı, tekrarsız)."""
    cfg = cfg or _cfg()
    out = []
    for o in cfg["onsets"]:
        for v in cfg["vowels"]:
            for c in cfg["codas"]:
                syl = f"{o}{v}{c}"
                if _allowed(syl, cfg) and syl not in out:
                    out.append(syl)
    return out


def bank(n: int, seed: int = 1) -> list[str]:
    """Ses başına hece bankası: envanterden dengeli, tohumlu n hece."""
    inv = inventory()
    r = random.Random(seed)
    r.shuffle(inv)
    # açık heceler (sessizle bitmeyen) konuşmayı yumuşatır: en az yarısı açık olsun
    open_ = [s for s in inv if s[-1] in "aeiouıöü"]
    closed = [s for s in inv if s[-1] not in "aeiouıöü"]
    k_open = min(len(open_), max(n * 2 // 3, n - len(closed)))
    pick = open_[:k_open] + closed[: n - k_open]
    return pick[:n]


def chunk_text(syllables: list[str], per_chunk: int, sep: str) -> list[str]:
    """TTS'e gönderilecek metinler: heceler arası duraklı (dilimleme için)."""
    chunks = []
    for i in range(0, len(syllables), per_chunk):
        part = syllables[i:i + per_chunk]
        chunks.append(sep.join(s.capitalize() for s in part) + ".")
    return chunks


def phrase(n_syllables: int, seed: int) -> str:
    """Replik işleri için anlamsız cümlecik (ör. 'Mori talu, venna!')."""
    inv = bank(80, seed=7)
    r = random.Random(seed)
    words, cur = [], []
    for _ in range(n_syllables):
        cur.append(r.choice(inv))
        if len(cur) >= r.choice((2, 2, 3)):
            words.append("".join(cur))
            cur = []
    if cur:
        words.append("".join(cur))
    text = " ".join(words)
    if not is_clean(text):
        return phrase(n_syllables, seed * 7919 + 1)  # birleşim yasak parçaya denk geldi: yeni tohum
    return text.capitalize()


def is_clean(text: str) -> bool:
    """Birleşmiş metinde (boşluksuz da) yasak parça var mı?"""
    cfg = _cfg()
    flat = text.lower().replace(" ", "")
    words = text.lower().replace(",", " ").replace("!", " ").replace("?", " ").split()
    return (not any(b in flat for b in cfg["banned_substrings"])
            and all(_allowed(w, cfg) for w in words)
            and not any(w in cfg.get("banned_words", []) for w in words))


if __name__ == "__main__":
    inv = inventory()
    print(len(inv), "hece;", ", ".join(bank(24, 3)))
    print(phrase(6, 1), "|", phrase(6, 2))
