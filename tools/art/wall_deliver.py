"""325 teslim: geçen kalas dosyalarını Assets/Art/Iso'ya üzerine yazmadan kopyalar."""
import os
import shutil

for n in ("sw_ne", "se_nw"):
    s = "art/tmp325/out/iso_wall_wall_plank_%s.png" % n
    d = "Assets/Art/Iso/iso_wall_wall_plank_%s.png" % n
    if not os.path.exists(d):
        shutil.copyfile(s, d)
        print("kopyalandı", d)
if os.path.exists("tools/art/upload_tmp.py"):
    os.remove("tools/art/upload_tmp.py")
