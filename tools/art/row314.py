"""314: 012'nin 8 karakteri + 6 yeni köylü SE karelerini tek sırada (x3) çizer, baskın gövde rengini yazar."""
from collections import Counter
from PIL import Image

N = ["woman_a", "elder_m", "elder_f", "boy_a", "girl_a", "woodcutter_a", "farmer_f", "craftsman_a",
     "orhan", "hatice", "zeynep", "murat", "kerem", "ayse"]
S = 3
sheet = Image.new("RGBA", (60 * S * len(N), 60 * S), (51, 51, 51, 255))
for i, n in enumerate(N):
    im = Image.open(f"Assets/Art/Iso/_aday/iso_char_{n}_56_cand_se.png").convert("RGBA")
    sheet.alpha_composite(im.resize((60 * S, 60 * S), Image.NEAREST), (i * 60 * S, 0))
    body = im.crop((0, 20, 60, 45))
    c = Counter(p[:3] for p in body.getdata() if p[3] and max(p[:3]) > 40)
    print(n, "#%02x%02x%02x" % c.most_common(1)[0][0])
sheet.save("art/results/314-14li-sira.png")
