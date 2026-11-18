#!/usr/bin/env python3
"""Toplu işçi: PixelLab'i Claude olmadan, doğrudan REST API ile çağırır (2026-11-18 pilotu).

Neden: her asset işini bir Claude Code oturumu yürütünce token'ın çoğu üretim dışı işe (okuma, betik yazma,
rapor) gidiyor ve Claude kullanım sınırı üretimi durdurabiliyor. Parametreleri belli olan işler bu betikle
token harcamadan üretilir; Claude yalnızca tarifi yazar ve sonuçları toplu inceler.

Hangi işler: iş dosyasında `worker: batch` olan ve "## Parametreler (toplu işçi)" bölümünde ```json bloğu
bulunan işler. Bulut ajanı bu işleri almaz (`queue_ready.py`).

Kullanım (PIXELLAB_API_TOKEN ortam değişkeni gerekir; Pillow gerekir):
  python tools/art/batch_worker.py probe                 # bakiye okur, hiçbir şey üretmez (0 gen)
  python tools/art/batch_worker.py plan  357 353         # ne gönderileceğini yazar, API çağırmaz (0 gen)
  python tools/art/batch_worker.py run   357 353 341     # üretir, doğrular, teslim eder, rapor yazar
Commit ve push yapmaz; sonucu `git add` ile kullanıcı ya da iş akışı gönderir.

Parametre bloğu (örnek):
  {
    "tool": "map_object",
    "family": "esya",
    "common": {"view": "high top-down", "detail": "low detail", "shading": "basic shading",
               "outline": "single color outline", "suffix": ", medieval peasant village object, ..."},
    "max_attempts": 2,
    "items": [
      {"name": "boulder_small", "out": "Assets/Art/Iso/_aday/iso_terrain_boulder_small_se.png",
       "canvas": [64, 56], "guide": {"n": 1, "m": 1, "axis": "x", "height": 12},
       "description": "small rounded grey field stone ..."}
    ]
  }
Kurallar 301 ile aynıdır: kılavuz + maske ile stil eşleme, ikinci deneme yalnızca otomatik ölçülebilen
hatalarda (boş, maske dışına >20 px taşma, ayak izinin bir yanı %80'den az, tabana değmiyor, kılavuz rengi
kalıntısı, yarı saydam piksel, alt satıra değme) ve seed=1 ile; tanınırlık gibi göz kararları raporda
"incele" olarak kalır. Var olan dosyanın üzerine yazılmaz. art/budget.md frenleri (rezerv 200, iş başına
cost_max) uygulanır.
"""
import base64
import datetime
import glob
import io
import json
import os
import re
import subprocess
import sys
import time
import urllib.error
import urllib.request

from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
API = os.environ.get("PIXELLAB_API_BASE", "https://api.pixellab.ai/v2")
QUEUE = os.path.join(ROOT, "art", "queue")
RESULTS = os.path.join(ROOT, "art", "results")
LEDGER = os.path.join(ROOT, "art", "ledger")
TMP = os.path.join(ROOT, "art", "tmp")
GUIDE_COLOR = (0x78, 0x60, 0x46)
RESERVE = 200.0
POLL_EVERY, POLL_TIMEOUT = 8, 600
MAX_INFLIGHT = 4          # aynı anda en çok bu kadar nesne (Tier 2 sınırı 10; bulut şeritleri de kullanıyor)
PY = sys.executable


# ----------------------------------------------------------------------------- API
def token():
    t = os.environ.get("PIXELLAB_API_TOKEN", "").strip()
    if not t:
        sys.exit("PIXELLAB_API_TOKEN ortam değişkeni yok. PowerShell: $env:PIXELLAB_API_TOKEN = \"...\"")
    return t


def http(method, path, body=None, raw=False, retry429=True):
    url = path if path.startswith("http") else API + path
    data = json.dumps(body).encode() if body is not None else None
    req = urllib.request.Request(url, data=data, method=method, headers={
        "Authorization": "Bearer " + token(),
        "Content-Type": "application/json",
        "User-Agent": "thatchwick-batch-worker",
    })
    try:
        with urllib.request.urlopen(req, timeout=120) as r:
            payload = r.read()
            return payload if raw else (json.loads(payload) if payload else {})
    except urllib.error.HTTPError as e:
        text = e.read().decode("utf-8", "replace")[:800]
        if e.code == 423:                         # nesne hâlâ üretiliyor: hata değil
            raise Pending(text)
        if e.code == 429 and retry429:          # eşzamanlı iş sınırı (Tier 2: 10, bulut şeritleriyle ortak)
            for _ in range(8):
                print(f"  429 eşzamanlılık sınırı, 30 sn bekleniyor: {text[:100]}")
                time.sleep(30)
                try:
                    return http(method, path, body, raw, retry429=False)
                except RuntimeError as e2:
                    if "HTTP 429" not in str(e2):
                        raise
            raise RuntimeError(f"{method} {path} -> 429 sürüyor")
        raise RuntimeError(f"{method} {path} -> HTTP {e.code}: {text}")


class Pending(Exception):
    pass


def balance():
    """GET /balance: ham yanıt ve içinden okunabilen abonelik generation sayısı."""
    raw = http("GET", "/balance")
    found = []

    def walk(x, key=""):
        if isinstance(x, dict):
            for k, v in x.items():
                walk(v, k)
        elif isinstance(x, list):
            for v in x:
                walk(v, key)
        elif isinstance(x, (int, float)) and "generation" in key.lower() and "total" not in key.lower():
            found.append((key, float(x)))
    walk(raw)
    gens = None
    for k, v in found:
        if any(w in k.lower() for w in ("remaining", "left", "available", "balance")):
            gens = v
            break
    if gens is None and found:
        gens = found[0][1]
    return raw, gens


def b64img(img, palette=False):
    im = img.convert("RGBA")
    if palette:                                   # kayıpsız palet PNG (301 notu: base64 bozulması)
        im = im.quantize(colors=256, method=Image.Quantize.FASTOCTREE) if len(set(im.getdata())) <= 256 else im
    buf = io.BytesIO()
    im.save(buf, format="PNG")
    return {"type": "base64", "base64": base64.b64encode(buf.getvalue()).decode(), "format": "png"}


def submit_map_object(item, common, guide, mask, seed):
    w, h = item["canvas"]
    body = {
        "description": (item["description"] + common.get("suffix", ""))[:2000],
        "image_size": {"width": w, "height": h},
        "view": common.get("view", "high top-down"),
        "outline": common.get("outline", "single color outline"),
        "shading": common.get("shading", "basic shading"),
        "detail": common.get("detail", "low detail"),
        "background_image": b64img(guide, palette=True),
        "inpainting": {"type": "mask", "mask_image": b64img(mask.convert("RGBA"))},
        "seed": seed,
    }
    r = http("POST", "/map-objects", body)
    return r["object_id"], r.get("background_job_id"), r.get("usage")


def fetch_map_object(object_id):
    """Tamamlanana kadar yoklar; PNG baytlarını ve son meta veriyi döndürür."""
    t0 = time.time()
    while True:
        try:
            meta = http("GET", f"/map-objects/{object_id}")
        except Pending:
            if time.time() - t0 > POLL_TIMEOUT:
                raise RuntimeError(f"map object {object_id} {POLL_TIMEOUT} sn'de bitmedi")
            time.sleep(POLL_EVERY)
            continue
        status = str(meta.get("status", "")).lower()
        url = meta.get("download_url") or meta.get("image_url") or meta.get("url")
        if url:
            if url.startswith("/"):
                url = API.rsplit("/v2", 1)[0] + url
            try:
                data = http("GET", url, raw=True)
            except RuntimeError:                  # bazı indirme adresleri imzalıdır; yetkisiz dene
                with urllib.request.urlopen(urllib.request.Request(url, headers={"User-Agent": "thatchwick"}), timeout=120) as r:
                    data = r.read()
            return data, meta
        img = meta.get("image") or (meta.get("last_response") or {}).get("image")
        if isinstance(img, dict) and img.get("base64"):
            return base64.b64decode(img["base64"]), meta
        if status in ("failed", "error", "cancelled"):
            raise RuntimeError(f"map object {object_id} başarısız: {json.dumps(meta)[:400]}")
        if time.time() - t0 > POLL_TIMEOUT:
            raise RuntimeError(f"map object {object_id} {POLL_TIMEOUT} sn'de bitmedi: {json.dumps(meta)[:300]}")
        time.sleep(POLL_EVERY)


# ----------------------------------------------------------------------------- iş dosyası
def job_path(jid):
    m = glob.glob(os.path.join(QUEUE, f"{jid}-*.md"))
    if len(m) != 1:
        sys.exit(f"iş bulunamadı ya da birden çok: {jid}")
    return m[0]


def front(path):
    text = open(path, encoding="utf-8").read()
    head, sep, rest = text.partition("---")
    blk, sep2, body = rest.partition("---")
    fm = {}
    for line in blk.splitlines():
        m = re.match(r"^([A-Za-z_]+):\s*(.*?)\s*(#.*)?$", line)
        if m:
            fm[m.group(1)] = m.group(2).strip().strip('"')
    return text, fm, body


def params(body, jid):
    m = re.search(r"## Parametreler \(toplu işçi\).*?```json\s*(\{.*?\})\s*```", body, re.S)
    if not m:
        sys.exit(f"{jid}: '## Parametreler (toplu işçi)' json bloğu yok")
    return json.loads(m.group(1))


def set_status(path, status, note):
    text = open(path, encoding="utf-8").read()
    head, sep, rest = text.partition("---")
    blk, sep2, body = rest.partition("---")
    blk = re.sub(r"(?m)^status:[^\n]*", f"status: {status}", blk, count=1)
    blk = re.sub(r"(?m)^claimed_at:[^\n]*\n", "", blk)
    marker = "<!-- İşçi bundan sonrasına"
    i = body.find(marker)
    line = f"- {datetime.date.today()} (toplu işçi): {note}\n\n"
    body = body[:i] + line + body[i:] if i >= 0 else body.rstrip("\n") + "\n" + line
    open(path, "w", encoding="utf-8", newline="").write(head + sep + blk + sep2 + body)


# ----------------------------------------------------------------------------- kılavuz ve denetim
def make_guide(jid, item):
    g = item["guide"]
    w, h = item["canvas"]
    d = os.path.join(TMP, jid)
    os.makedirs(d, exist_ok=True)
    pre = os.path.join(d, item["name"])
    out = subprocess.run([PY, os.path.join(ROOT, "tools", "art", "footprint_guide.py"), "--w", str(w), "--h", str(h),
                          "--n", str(g.get("n", 1)), "--m", str(g.get("m", 1)), "--axis", g.get("axis", "x"),
                          "--height", str(g["height"]), "--out", pre], capture_output=True, text=True)
    if out.returncode:
        raise RuntimeError(f"footprint_guide: {out.stderr or out.stdout}")
    info = open(pre + "_info.txt", encoding="utf-8").read()
    xs = re.search(r"footprint x (\d+)-(\d+), y (\d+)-(\d+)", info)
    x0, x1, y0, y1 = map(int, xs.groups())
    piv = re.search(r"pivot x=([\d.]+), pivot y \(alttan\)=([\d.]+)", info)
    return (Image.open(pre + "_guide.png").convert("RGBA"), Image.open(pre + "_mask.png").convert("L"),
            (x0, y0, x1 - x0 + 1, y1 - y0 + 1), (float(piv.group(1)), float(piv.group(2))))


def measure(img, mask, fp, fill=True):
    """Otomatik ölçülebilen 301 ölçütleri. Dönüş: (ölçüler, ikinci deneme gerektiren hatalar)."""
    im = img.convert("RGBA")
    W, H = im.size
    px = im.load()
    mk = mask.load()
    opaque = [(x, y) for y in range(H) for x in range(W) if px[x, y][3] > 0]
    m = {"boyut": f"{W}x{H}"}
    errs = []
    if not opaque:
        return m, ["boş çıktı"]
    semi = sum(1 for x, y in opaque if px[x, y][3] < 255)
    xs = [p[0] for p in opaque]
    ys = [p[1] for p in opaque]
    m["kutu"] = f"x{min(xs)}-{max(xs)}, y{min(ys)}-{max(ys)}"
    m["boy"] = max(ys) - min(ys) + 1
    m["renk"] = len({px[x, y][:3] for x, y in opaque})
    m["yarı saydam"] = semi
    out_mask = sum(1 for x, y in opaque if mk[x, y] == 0)
    m["maske dışı"] = out_mask
    guide_left = sum(1 for x, y in opaque if sum(abs(a - b) for a, b in zip(px[x, y][:3], GUIDE_COLOR)) <= 12)
    m["kılavuz kalıntısı"] = guide_left
    fx, fy, fw, fh = fp
    cx, cy = fx + fw / 2, fy + fh / 2
    rows = range(int(cy), fy + fh)        # taban elmasının alt yarısı
    left = [x for x, y in opaque if fy <= y < fy + fh and x < cx]
    right = [x for x, y in opaque if fy <= y < fy + fh and x >= cx]
    cov_l = (cx - min(left)) / (fw / 2) if left else 0.0
    cov_r = (max(right) + 1 - cx) / (fw / 2) if right else 0.0
    m["ayak izi kaplaması"] = f"sol %{cov_l * 100:.0f}, sağ %{cov_r * 100:.0f}"
    touches = any(y in rows for _, y in opaque)
    m["tabana değiyor"] = "evet" if touches else "hayır"
    m["alt satıra değme"] = "evet" if max(ys) >= H - 1 else "hayır"
    if semi:
        errs.append(f"{semi} yarı saydam piksel")
    if out_mask > 20:
        errs.append(f"maske dışına {out_mask} px taşma")
    if fill and (cov_l < 0.8 or cov_r < 0.8):     # ince/doğal eşyada ("fill": false) kapalı
        errs.append("ayak izinin bir yanı %80'den az")
    if not touches:
        errs.append("tabana değmiyor")
    if guide_left > 10:
        errs.append(f"kılavuz rengi kalıntısı {guide_left} px")
    if max(ys) >= H - 1:
        errs.append("alt satıra değiyor (kırpılmış)")
    return m, errs


def style_check(path, family):
    r = subprocess.run([PY, os.path.join(ROOT, "tools", "art", "style.py"), "check", "--family", family, path],
                       capture_output=True, text=True)
    last = [l for l in r.stdout.splitlines() if l.startswith("| ") and not l.startswith("| Dosya") and "---" not in l]
    row = last[-1] if last else ""
    level = "KALDI" if "KALDI" in row else ("UYARI" if "UYARI" in row else "GEÇTİ")
    return level, row, ("ailesinden çok doygun" in row)


def style_fit(src, dst, family):
    r = subprocess.run([PY, os.path.join(ROOT, "tools", "art", "style.py"), "fit", "--family", family, src, dst],
                       capture_output=True, text=True)
    m = re.search(r"doygunluk x([\d.]+)", r.stdout)
    return m.group(1) if m else "?"


# ----------------------------------------------------------------------------- akış
def plan_job(jid):
    path = job_path(jid)
    text, fm, body = front(path)
    if fm.get("worker") != "batch":
        sys.exit(f"{jid}: 'worker: batch' değil; bu işi bulut ajanı yapar")
    if fm.get("approved_by", "").split("#")[0].strip() not in ("user", "auto"):
        sys.exit(f"{jid}: onay yok (approved_by: {fm.get('approved_by')}); kullanıcı 'user' yapınca çalışır")
    p = params(body, jid)
    if p.get("tool") != "map_object":
        sys.exit(f"{jid}: şimdilik yalnızca tool=map_object destekleniyor")
    return path, fm, p


def run_job(jid, dry=False, reuse=None):
    reuse = reuse or {}
    path, fm, p = plan_job(jid)
    family = p.get("family") or fm.get("family", "esya")
    common = p.get("common", {})
    items = p["items"]
    attempts = int(p.get("max_attempts", 2))
    cost_max = float(fm.get("cost_max", "0") or 0)
    print(f"\n== {jid} ({len(items)} parça, cost_max {cost_max:g}, aile {family})")
    done_before = [it for it in items if os.path.exists(os.path.join(ROOT, it["out"]))]
    for it in done_before:
        print(f"  {it['name']}: zaten teslim edilmiş ({it['out']}), atlanıyor")
    items = [it for it in items if it not in done_before]
    if not items:
        print("  yapılacak parça yok"); return 0.0
    if dry:
        for it in items:
            g = it["guide"]
            print(f"  {it['name']}: tuval {it['canvas']}, kılavuz n={g.get('n', 1)} m={g.get('m', 1)} "
                  f"{g.get('axis', 'x')} boy {g['height']} -> {it['out']}")
            print(f"     \"{it['description'] + common.get('suffix', '')}\"")
        return None

    raw0, bal0 = balance()
    if bal0 is not None and bal0 - cost_max < RESERVE:
        sys.exit(f"{jid}: rezerv freni: bakiye {bal0:g} - cost_max {cost_max:g} < {RESERVE:g}")
    set_status(path, "running", "başladı")
    tmp = os.path.join(TMP, jid)
    red = os.path.join(RESULTS, f"{jid}-red")
    state = {it["name"]: {"item": it, "tries": []} for it in items}
    for it in items:
        st = state[it["name"]]
        st["guide"], st["mask"], st["fp"], st["pivot"] = make_guide(jid, it)

    def collect(todo, attempt):
        for s in [x for x in todo if "pending" in x]:
            if "pending" not in s:
                continue
            oid, usage = s.pop("pending")
            name = s["item"]["name"]
            try:
                data, meta = fetch_map_object(oid)
            except RuntimeError as e:
                s["tries"].append({"hata": str(e), "object_id": oid})
                print(f"   {name}: {e}")
                continue
            os.makedirs(tmp, exist_ok=True)
            rawp = os.path.join(tmp, f"{name}_d{attempt + 1}.png")
            open(rawp, "wb").write(data)
            img = Image.open(rawp).convert("RGBA")
            if img.size != tuple(s["item"]["canvas"]):
                img = img.crop((0, 0) + tuple(s["item"]["canvas"])) if img.width >= s["item"]["canvas"][0] else img
            m, errs = measure(img, s["mask"], s["fp"], s["item"].get("fill", True))
            fam = s["item"].get("family", family)
            level, row, sat_only = style_check(rawp, fam)
            fitted = None
            if level == "KALDI" and sat_only:              # style.py: kilitsiz ailede doygunluk düzeltmesi
                fp_ = os.path.join(tmp, f"{name}_d{attempt + 1}_fit.png")
                if os.path.exists(fp_):
                    os.remove(fp_)
                k = style_fit(rawp, fp_, fam)
                level2, row2, _ = style_check(fp_, fam)
                if level2 != "KALDI":
                    fitted, level, row = (fp_, k), level2, row2
            if level == "KALDI":
                errs.append("stil kapısı KALDI")
            s["tries"].append({"object_id": oid, "raw": rawp, "ölçü": m, "hatalar": errs, "stil": level,
                               "stil satırı": row, "fit": fitted, "usage": usage})
            print(f"   {name}: {'TUTTU' if not errs else 'tutmadı: ' + '; '.join(errs)} (stil {level})")
            if not errs:
                s["ok"] = True

    spent_calls = 0
    for attempt in range(attempts):
        todo = [s for s in state.values() if not s.get("ok") and len(s["tries"]) == attempt]
        if not todo:
            break
        print(f"  deneme {attempt + 1}: {len(todo)} parça gönderiliyor")
        for idx, s in enumerate(todo):                     # dalga dalga gönder (eşzamanlılık sınırı)
            if s["item"]["name"] in reuse and attempt == 0:
                s["pending"] = (reuse[s["item"]["name"]], None)
                print(f"   {s['item']['name']}: önceki nesne kullanılıyor ({reuse[s['item']['name']]})")
                continue
            if sum(1 for x in todo if "pending" in x and x["pending"][1] != "reused") >= MAX_INFLIGHT:
                collect(todo, attempt)
            try:
                oid, _, usage = submit_map_object(s["item"], common, s["guide"], s["mask"], seed=attempt)
                spent_calls += 1
                s["pending"] = (oid, usage)
            except RuntimeError as e:
                s["tries"].append({"hata": str(e)})
                print(f"   {s['item']['name']}: gönderilemedi: {e}")
        collect(todo, attempt)

    raw1, bal1 = balance()
    spent = (bal0 - bal1) if (bal0 is not None and bal1 is not None) else float(spent_calls)

    # teslim
    delivered, sheet_files = [], []
    os.makedirs(red, exist_ok=True)
    for s in state.values():
        it = s["item"]
        for i, t in enumerate(s["tries"]):
            if not t.get("raw"):
                continue
            good = s.get("ok") and i == len(s["tries"]) - 1
            src = t["fit"][0] if t.get("fit") else t["raw"]
            if good:
                dst = os.path.join(ROOT, it["out"])
                os.makedirs(os.path.dirname(dst), exist_ok=True)
                Image.open(src).convert("RGBA").save(dst)
                delivered.append(it["out"])
                sheet_files.append(dst)
            else:
                Image.open(t["raw"]).save(os.path.join(red, f"{it['name']}_d{i + 1}.png"))
    sheet = os.path.join(RESULTS, f"{jid}-yanyana.png")
    if sheet_files and not os.path.exists(sheet):
        subprocess.run([PY, os.path.join(ROOT, "tools", "art", "style.py"), "sheet", "--family", family,
                        "--out", sheet] + sheet_files, capture_output=True)

    # rapor, defter, durum
    today = datetime.date.today().isoformat()
    rep = [f"unity-import: pending\n", f"# {jid} {fm.get('title', '')} (toplu işçi, {today})\n",
           f"Claude kullanılmadı: `tools/art/batch_worker.py`, PixelLab REST `POST /map-objects` (kılavuz + maske, 301 yöntemi).",
           f"Harcama: **{spent:g} generation** (bakiye {bal0} -> {bal1}, `GET /balance`). Çağrı: {spent_calls}. "
           f"Teslim: {len(delivered)}/{len(items)}. Yan yana: [{jid}-yanyana.png]({jid}-yanyana.png)."
           if sheet_files else f"Harcama: **{spent:g} generation**. Teslim: 0/{len(items)}.",
           "Masaüstü kopyası: yerel (betik yapmaz).\n",
           "## Parçalar\n", "| Parça | Deneme | Sonuç | Stil | Ölçüler | Hata |", "|---|---|---|---|---|---|"]
    for s in state.values():
        it = s["item"]
        for i, t in enumerate(s["tries"]):
            res = "TESLİM" if (s.get("ok") and i == len(s["tries"]) - 1) else "red"
            ms = ", ".join(f"{k} {v}" for k, v in (t.get("ölçü") or {}).items())
            fit = f" (doygunluk x{t['fit'][1]})" if t.get("fit") else ""
            rep.append(f"| {it['name']} | {i + 1} | {res} | {t.get('stil', '-')}{fit} | {ms} | "
                       f"{'; '.join(t.get('hatalar', [])) or t.get('hata', '')} |")
    rep += ["\n## Teslim", "| Dosya | Pivot (x, y ALTTAN) |", "|---|---|"]
    for s in state.values():
        if s.get("ok"):
            rep.append(f"| {s['item']['out']} | {s['pivot'][0]:g}, {s['pivot'][1]:g} |")
    rep += ["\n## İncele (otomatik ölçülemeyen)",
            "- Tanınırlık, istenmeyen ayrıntı (zemin lekesi, gölge, ekstra nesne), ışık yönü, ayna uygunluğu: yan yana sayfasında kullanıcı karar verir.",
            f"\n<details><summary>GET /balance ham yanıt</summary>\n\n```\n{json.dumps(raw1, ensure_ascii=False)[:1500]}\n```\n</details>\n"]
    title = re.sub(r"[^a-z0-9]+", "-", os.path.basename(path)[len(jid) + 1:-3].lower()).strip("-")
    open(os.path.join(RESULTS, f"{jid}-{title}.md"), "w", encoding="utf-8").write("\n".join(rep))
    os.makedirs(LEDGER, exist_ok=True)
    lp = os.path.join(LEDGER, f"{jid}.md")
    if not os.path.exists(lp):
        open(lp, "w", encoding="utf-8").write("| Tarih | İş | Araç | Harcanan | Kalan (get_balance) |\n|---|---|---|---|---|\n")
    with open(lp, "a", encoding="utf-8") as f:
        f.write(f"| {today} | {jid} {fm.get('title', '')[:40]} (toplu işçi) | POST /map-objects x{spent_calls} | {spent:g} | {bal1} |\n")
    if delivered:
        set_status(path, "review", f"{len(delivered)}/{len(items)} teslim, {spent:g} gen, Claude'suz. Rapor: art/results/{jid}-{title}.md")
    else:
        set_status(path, "failed", f"teslim yok, {spent:g} gen. Rapor: art/results/{jid}-{title}.md")
    print(f"  bitti: {len(delivered)}/{len(items)} teslim, {spent:g} gen; rapor art/results/{jid}-{title}.md")
    return spent


def main():
    if len(sys.argv) < 2 or sys.argv[1] not in ("probe", "plan", "run"):
        sys.exit(__doc__)
    cmd, args = sys.argv[1], sys.argv[2:]
    jobs = [a for a in args if "=" not in a]
    reuse = dict(a.split("=", 1) for a in args if "=" in a)   # ad=object_id: önceki çalışmanın nesnesini yeniden üretmeden al
    if cmd == "probe":
        raw, gens = balance()
        print(json.dumps(raw, ensure_ascii=False, indent=2))
        print(f"\nOkunan abonelik generation: {gens}")
        return
    if not jobs:
        sys.exit("iş numarası ver: ör. 357 353")
    for j in jobs:
        plan_job(j)                                       # hepsi geçerli mi, başlamadan denetle
    if cmd == "plan":
        for j in jobs:
            run_job(j, dry=True)
        return
    total = 0.0
    for j in jobs:
        total += run_job(j, reuse=reuse) or 0.0
    print(f"\nToplam {total:g} generation. Sonra: git add art Assets/Art/Iso && git commit && git push")


if __name__ == "__main__":
    main()
