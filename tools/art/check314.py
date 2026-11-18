"""314: validate_iso, style check ve yan yana sayfası (referans: 012 sekizlisi dahil)."""
import subprocess
import sys

N = ["orhan", "hatice", "zeynep", "murat", "kerem", "ayse"]
A = "Assets/Art/Iso/_aday/iso_char_%s_56_cand_%s.png"


def run(args):
    r = subprocess.run([sys.executable, "-W", "ignore"] + args, capture_output=True, text=True)
    print((r.stdout + r.stderr).strip()[-1500:])
    print("rc", r.returncode)


for n in N:
    run(["tools/art/validate_iso.py", A % (n, "se"), "--kind", "object", "--max-colors", "32"])
run(["tools/art/style.py", "check", "--family", "karakter"] + [A % (n, "se") for n in N])
run(["tools/art/style.py", "sheet", "--family", "karakter", "--out", "art/results/314-yanyana.png"]
    + [A % (n, "se") for n in N])
