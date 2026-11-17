#!/usr/bin/env python3
"""Kuyruktaki işleri otomatik akış için onaylar (kullanıcının çalıştırdığı komut).

Kullanıcı kararı (2026-11-17): "Bu projede istenen tüm asset'ler PixelLab'e üretilmeli ve kuyruğa atılmalı."
Harcama onayı kullanıcıya aittir. Bu yüzden onay satırlarını işçi ya da Claude oturumu değil,
kullanıcı bu betikle değiştirir.

Yaptığı:
  - status pending/blocked ve approved_by pending/auto olan her iş -> approved_by: user
  - Zemin oturumunda çıktısı teslim edilmiş blocked işler (çıktı dosyaları Assets'te varsa) -> status: review
  - Çıktısı repoda olmayan bu blocked işler -> status: pending
  - accepted, rejected, review, running, failed işlere dokunmaz

Kullanım:
  python tools/art/approve_all.py            # yalnızca ne değişeceğini gösterir
  python tools/art/approve_all.py --uygula   # değiştirir; sonra git add/commit/push
"""
import argparse
import glob
import os
import re

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
QUEUE = os.path.join(ROOT, "art", "queue")
TODAY_NOTE = "otomatik akış: kullanıcı onayı (tools/art/approve_all.py)"
DELIVERED_BLOCKED = {"001", "002", "003", "005", "006"}   # zemin oturumunda teslim edildi (2026-11-08)
REOPEN_BLOCKED = set()                                    # çıktısı eksik blocked iş: pending yapılır


def outputs(head):
    outs, on = [], False
    for line in head.splitlines():
        if line.startswith("outputs:"):
            on = True
            continue
        if on:
            m = re.match(r"^\s+-\s+(\S+)", line)
            if not m:
                break
            outs.append(m.group(1))
    return outs


def exists(path):
    base = re.split(r"[{<*]", path)[0]
    if glob.glob(os.path.join(ROOT, base) + "*"):
        return True
    # numaralı set (ör. _00.png ... _15.png ya da _0.png ...): öneki ara
    stem = re.sub(r"_\d+\.png$", "_", base)
    return stem != base and bool(glob.glob(os.path.join(ROOT, stem) + "*"))


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--uygula", action="store_true", help="değişiklikleri yaz")
    a = ap.parse_args()
    changes = []
    for path in sorted(glob.glob(os.path.join(QUEUE, "*.md"))):
        text = open(path, encoding="utf-8").read()
        if not text.startswith("---"):
            continue
        _, head, rest = text.split("---", 2)
        jid = os.path.basename(path).split("-", 1)[0]
        st = re.search(r"^status:\s*(\S+)", head, re.M)
        ap_ = re.search(r"^approved_by:\s*(\S+)", head, re.M)
        if not st:
            continue
        status, approved = st.group(1), ap_.group(1) if ap_ else ""
        new_head, new_rest, what = head, rest, []
        if status in ("pending", "blocked") and approved in ("pending", "auto", ""):
            new_head = re.sub(r"^approved_by:.*$", f"approved_by: user        # {TODAY_NOTE}", new_head, count=1, flags=re.M)
            what.append(f"approved_by {approved or '-'} -> user")
        if status == "blocked" and jid in DELIVERED_BLOCKED:
            if all(exists(o) for o in outputs(head)):
                new_head = re.sub(r"^status:.*$", "status: review", new_head, count=1, flags=re.M)
                new_rest = rest.rstrip() + "\n\nOTOMATİK AKIŞ: çıktılar zemin oturumunda teslim edilmişti (Assets'te var); `blocked` yerine `review`. Kabul ya da red kullanıcının.\n"
                what.append("status blocked -> review (çıktılar var)")
            else:
                new_head = re.sub(r"^status:.*$", "status: pending", new_head, count=1, flags=re.M)
                new_rest = rest.rstrip() + "\n\nOTOMATİK AKIŞ: çıktılar repoda yok; `pending` yapıldı, işçi üretir.\n"
                what.append("status blocked -> pending (çıktılar eksik)")
        if status == "blocked" and jid in REOPEN_BLOCKED:
            new_head = re.sub(r"^status:.*$", "status: pending", new_head, count=1, flags=re.M)
            new_rest = rest.rstrip() + "\n\nOTOMATİK AKIŞ: bu işin çıktısı repoya gelmemiş; `pending` yapıldı, işçi üretir.\n"
            what.append("status blocked -> pending")
        if what:
            changes.append((jid, os.path.basename(path), what))
            if a.uygula:
                with open(path, "w", encoding="utf-8", newline="\n") as f:
                    f.write("---" + new_head + "---" + new_rest)
    for jid, name, what in changes:
        print(f"{jid:>5}  {name:<48} {'; '.join(what)}")
    print(f"\n{len(changes)} iş {'güncellendi' if a.uygula else 'güncellenecek (uygulamak için --uygula)'}.")


if __name__ == "__main__":
    main()
