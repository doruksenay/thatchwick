#!/usr/bin/env python3
"""Yeni sabit Türkçe metin denetimi (CLAUDE.md > Dil ve yerelleştirme).

Bir dalın `main`'e göre farkına bakar: Assets/Scripts/Core ve Assets/Scripts/Game altındaki .cs dosyalarına
eklenen satırlarda, oyuncuya gidebilecek Türkçe harfli sabit metin (çğıöşü) var mı? Silinen satırlardaki aynı
metinler düşülür: kodu taşımak ya da dosya bölmek sayılmaz, yalnızca net yeni metin sayılır.

Neden fark tabanlı: dosya başına sayaç (eski taban dosyası) kırmızı bir PR birleşince `main`'i ve sonraki bütün
PR'ları kırıyordu. Bu denetim yalnızca PR'ın kendi eklediğine bakar; `main`'de çalışmaz.

Kullanım:
  python3 tools/loc/sabit_metin.py                 # origin/main ile ortak atadan bu yana (commit edilmemişler dahil)
  python3 tools/loc/sabit_metin.py --base HEAD^1   # CI: birleştirme commit'inin ana dal tarafı

Bilerek sabit kalan metin (ad havuzu gibi) satır sonuna `// loc-yok: <neden>` yazılarak geçirilir.
Çıkış kodu: 0 temiz, 1 yeni sabit metin var, 2 kullanım/git hatası.
"""
import argparse
import collections
import re
import subprocess
import sys

PATHS = ["Assets/Scripts/Core", "Assets/Scripts/Game"]
LITERAL = re.compile(r'"[^"\n]*[çğıöşüÇĞİÖŞÜ][^"\n]*"')
# Oyuncuya gitmeyen iletiler: geliştirici günlüğü, içerik doğrulama, istisna, hata raporu, editör öznitelikleri.
DEVELOPER_ONLY = ("Debug.Log", "errors.Add(", "Warnings.Add(", "warnings.Add(", "throw new", "ErrorReporter.Report(",
                  "[Tooltip(", "[Header(", "[Obsolete(")
EXEMPT = "// loc-yok:"


def literals(line):
    """Satırdaki sayılan sabit metinler (yorum, geliştirici iletisi ve muaf satır sayılmaz)."""
    t = line.strip()
    if not t or t.startswith(("//", "*", "/*")):
        return []
    if EXEMPT in t or any(skip in t for skip in DEVELOPER_ONLY):
        return []
    cut = t.find(" // ")   # satır sonu açıklaması
    if cut >= 0:
        t = t[:cut]
    return LITERAL.findall(t)


def scan(diff_text):
    """Birleşik fark metninden (git diff -U0) net yeni metinleri çıkarır: [(dosya, satır no, metin)]."""
    added = []                       # (dosya, satır, metin)
    removed = collections.Counter()  # metin -> adet
    path, line_no = None, 0
    for raw in diff_text.splitlines():
        if raw.startswith("+++ "):
            path = raw[6:] if raw.startswith("+++ b/") else None
            continue
        if raw.startswith("--- "):
            continue
        if raw.startswith("@@"):
            m = re.match(r"@@ -\d+(?:,\d+)? \+(\d+)", raw)
            line_no = int(m.group(1)) if m else 0
            continue
        if raw.startswith("+"):
            if path and path.endswith(".cs"):
                for lit in literals(raw[1:]):
                    added.append((path, line_no, lit))
            line_no += 1
        elif raw.startswith("-"):
            for lit in literals(raw[1:]):
                removed[lit] += 1
    fresh = []
    for path, no, lit in added:
        if removed[lit] > 0:
            removed[lit] -= 1   # taşınmış ya da yeniden biçimlenmiş satır
        else:
            fresh.append((path, no, lit))
    return fresh


def git(*args):
    return subprocess.run(["git", *args], capture_output=True, text=True, encoding="utf-8", errors="replace")


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__.split("\n")[0])
    ap.add_argument("--base", help="karşılaştırılacak commit (varsayılan: origin/main ile ortak ata)")
    args = ap.parse_args(argv)

    base = args.base
    if not base:
        r = git("merge-base", "HEAD", "origin/main")
        if r.returncode != 0 or not r.stdout.strip():
            print("sabit_metin: origin/main bulunamadı (git fetch origin main); denetim atlandı.", file=sys.stderr)
            return 0
        base = r.stdout.strip()
    r = git("diff", "-U0", "--no-color", "--no-ext-diff", base, "--", *PATHS)
    if r.returncode != 0:
        print("sabit_metin: git diff başarısız:\n" + r.stderr, file=sys.stderr)
        return 2

    fresh = scan(r.stdout)
    if not fresh:
        print("Yeni sabit Türkçe metin yok.")
        return 0
    print(f"Koda {len(fresh)} yeni sabit Türkçe metin eklenmiş:")
    for path, no, lit in fresh:
        print(f"  {path}:{no}: {lit}")
        print(f"::error file={path},line={no}::Yeni sabit Türkçe metin: anahtara taşı (CLAUDE.md > Dil ve yerelleştirme)")
    print("\nOyuncuya giden metin anahtarla yazılır: Game'de GameLocalization.T(\"ui...\"), Core'da LocText.Of(...) ya da\n"
          "dizgi döndüren imzalarda CoreText.T(...). Yeni anahtar ui.<alan>.en.json ve ui.<alan>.tr.json dosyasına girer.\n"
          "Metin oyuncuya gitmiyorsa ya da bilerek sabitse satır sonuna `// loc-yok: <neden>` yaz.")
    return 1


if __name__ == "__main__":
    sys.exit(main())
