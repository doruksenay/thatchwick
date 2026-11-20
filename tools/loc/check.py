"""Yerelleştirme denetimi: içerikteki her oyuncu metninin kaynak dil (İngilizce) tablosunda karşılığı var mı?

Kullanım:
    python tools/loc/check.py            # eksikleri listeler; eksik varsa çıkış kodu 1
    python tools/loc/check.py --todo     # eksikleri İngilizce tabloya "TODO: <Türkçe metin>" olarak ekler
    python tools/loc/check.py --kaynak   # çeviriler güncellendikten sonra Türkçe metinleri "çevrildi" diye kaydeder

ESKİ: Türkçe metni, İngilizcesi yazıldıktan sonra değişmiş anahtarlar (tools/loc/en_kaynak.json'daki metinle
karşılaştırılır). İngilizceyi gözden geçir, sonra --kaynak ile kaydet.

Yeni eşya, tarif, beceri dalı ... eklediğinde İngilizce adını Assets/Resources/Localization/content.en.json'a yaz.
Aynı denetim CI'da ContentLocalizationTests ile de çalışır.
"""

import re
import sys

from pathlib import Path

from content_keys import LOC, extract, load_content, read_table, write_table

SOURCE_LOG = Path(__file__).resolve().parent / "en_kaynak.json"

JSON_LANGUAGE = "tr"  # Core'daki ContentLocalizer.JsonLanguage ile aynı olmalı
NUM = re.compile(r"\d+")


def main(argv):
    items, life, appearance = load_content()
    entries = extract(items, life, appearance)
    en_path = LOC / "content.en.json"
    en = read_table(en_path)

    missing = [(k, t, c) for k, t, c in entries if not en.get(k, "").strip() or en[k].startswith("TODO:")]
    numbers = [(k, t, en[k]) for k, t, _ in entries
               if k in en and not en[k].startswith("TODO:") and sorted(NUM.findall(t)) != sorted(NUM.findall(en[k]))]
    keys = {k for k, _, _ in entries}
    orphans = [k for k in en if k not in keys]
    log = read_table(SOURCE_LOG)
    stale = [(k, log[k], t) for k, t, _ in entries if k in log and k in en and log[k] != t]

    print(f"İçerik metni: {len(entries)}  İngilizce tablo: {len(en)}  eksik: {len(missing)}  "
          f"sayısı tutmayan: {len(numbers)}  eski: {len(stale)}  kullanılmayan: {len(orphans)}")
    for k, t, c in missing:
        print(f"  EKSİK  {k}  ({c}): {t}")
    for k, t, e in numbers:
        print(f"  SAYI   {k}: '{t}' <> '{e}'")
    for k, was, now in stale:
        print(f"  ESKİ   {k}: '{was}' -> '{now}'  (İngilizce: '{en[k]}')")
    if orphans:
        print("  (Kullanılmayan anahtarlar başka bir dalın içeriği olabilir; silmeden önce dallar birleşsin.)")

    if "--todo" in argv and missing:
        for k, t, _ in missing:
            if not en.get(k, "").strip():
                en[k] = f"TODO: {t}"
        write_table(en_path, en)
        print(f"{en_path.name}: {len(missing)} TODO eklendi.")

    if "--kaynak" in argv:
        write_table(SOURCE_LOG, {k: t for k, t, _ in entries if k in en and not en[k].startswith("TODO:")})
        print(f"{SOURCE_LOG.name}: çevrilmiş metinlerin Türkçesi kaydedildi.")
        stale = []

    return 1 if missing or numbers or stale else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
