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

# Adlandırma denetimi (Project belgesi 61b §8.1; aynısı AdlandirmaTests'te). Sözcük başı eşleşir; tuhaflık satırları muaf.
BANNED = ("hund dren şaperon kupelasyon kalamin galen tentür jonglör goblen kapitone aplike lambri şindıl parapet serpantin "
          "füme sote tunik tuniğ ceket pantolon pileli şapka kapüşon broş kolye küvet şömine karyola kanopi paravan aplik "
          "parke liken otomat rozet moodlet xp bonus envanter kasaba şehir şehr rota jüri mastıf çikolata izabel bordo haki "
          "hijyen enerjisi").split()
DUPLICATE_ALLOWED = {"Akıtma oluğu"}   # iki oturumda iki kez eklenmiş aynı düzenek; birleştirme kararı bekliyor
PLACEHOLDER = re.compile(r"\{[^}]*\}")
WORD = re.compile(r"[a-zçğıöşüâîû]+")


def tr_lower(s):
    return s.replace("I", "ı").replace("İ", "i").lower()


def naming_problems(items, entries):
    """Aynı ada sahip eşyalar ve yasaklı sözcükler (içerik + ui*.tr.json)."""
    names = {}
    for it in items.get("items", []):
        if not it["id"].startswith("item.forage_"):
            names.setdefault(it.get("displayName"), []).append(it["id"])
    dups = [(n, ids) for n, ids in names.items() if len(ids) > 1 and n not in DUPLICATE_ALLOWED]
    texts = [(k, t) for k, t, _ in entries]
    for path in sorted(LOC.glob("ui*.tr.json")):
        texts += [(k, v) for k, v in read_table(path).items() if not k.startswith("core.oddity.")]
    banned = []
    for k, t in texts:
        for w in WORD.findall(tr_lower(PLACEHOLDER.sub(" ", t or ""))):
            banned += [(k, w, b, t) for b in BANNED if w.startswith(b)]
    return dups, banned


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
    dups, banned = naming_problems(items, entries)
    for n, ids in dups:
        print(f"  AYNI AD  {n}: {', '.join(ids)}")
    for k, w, b, t in banned:
        print(f"  YASAK  {k}: '{w}' ({b}): {t}")
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

    return 1 if missing or numbers or stale or dups or banned else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
