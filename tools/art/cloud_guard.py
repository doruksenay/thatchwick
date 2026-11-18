#!/usr/bin/env python3
"""Bulut işçisinin (art-worker.yml) commit'lerini push'tan önce denetler.

Ajanın kendisi push edemez (checkout kimlik bilgisi bırakmaz); iş akışı bu denetimden sonra push eder.
Böylece ajana gömülü bir talimat (PixelLab çıktısı, iş dosyası) main'e kod ya da sır gönderemez.

Kurallar:
  - Değişen her yol art/, Assets/Art/Iso/ ya da tools/art/ altında olmalı (CLAUDE.md'deki işçi yolları).
  - .meta dosyası yazılmaz (Unity üretir).
  - Assets/Art/Iso/ altındaki var olan dosya değiştirilmez ya da silinmez; yalnızca yeni dosya eklenir.
  - GUARD_SECRET_VARS ortam değişkeninde adları verilen sırların değerleri değişikliklerde geçmez.
  - tools/art/ değiştiyse sonuç main'e değil ayrı bir dala gider, insan PR'da inceler
    (yerel yoklayıcı main'deki tools/art betiklerini çalıştırır).

Kullanım:
  python tools/art/cloud_guard.py <base> [<head>]
Çıktı: "main" ya da "pr" (son satır). Çıkış kodu: 0 geçti, 2 reddedildi (nedenler stderr'de), 1 değişiklik yok.
"""
import os
import subprocess
import sys

ALLOWED = ("art/", "Assets/Art/Iso/", "tools/art/")
APPEND_ONLY = ("Assets/Art/Iso/",)
NEEDS_REVIEW = ("tools/art/",)


def git(*args, binary=False):
    out = subprocess.run(["git", *args], capture_output=True, check=True)
    return out.stdout if binary else out.stdout.decode("utf-8", "replace")


def main():
    if len(sys.argv) < 2:
        print(__doc__, file=sys.stderr)
        return 2
    base = sys.argv[1]
    head = sys.argv[2] if len(sys.argv) > 2 else "HEAD"

    raw = git("diff", "--no-renames", "--name-status", "-z", base, head)
    parts = [p for p in raw.split("\0") if p]
    changes = list(zip(parts[0::2], parts[1::2]))
    if not changes:
        print("Değişiklik yok.", file=sys.stderr)
        return 1

    problems = []
    review = False
    for status, path in changes:
        if not path.startswith(ALLOWED):
            problems.append(f"izin dışı yol: {path}")
        if path.endswith(".meta"):
            problems.append(f".meta yazılmaz: {path}")
        if path.startswith(APPEND_ONLY) and status != "A":
            problems.append(f"var olan dosya değiştirilemez/silinemez ({status}): {path}")
        if path.startswith(NEEDS_REVIEW):
            review = True

    secrets = []
    for name in os.environ.get("GUARD_SECRET_VARS", "").split(","):
        value = os.environ.get(name.strip(), "") if name.strip() else ""
        if len(value) >= 8:
            secrets.append((name.strip(), value.encode()))
    if secrets:
        patch = git("diff", "--text", base, head, binary=True)
        log = git("log", "--format=%B", f"{base}..{head}", binary=True)
        for name, value in secrets:
            if value in patch or value in log:
                problems.append(f"sır değeri değişikliklerde geçiyor ({name})")

    if problems:
        for p in problems:
            print("REDDEDİLDİ: " + p, file=sys.stderr)
        return 2
    for status, path in changes:
        print(f"  {status} {path}", file=sys.stderr)
    print("pr" if review else "main")
    return 0


if __name__ == "__main__":
    sys.exit(main())
