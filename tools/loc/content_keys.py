"""İçerik JSON'larındaki oyuncuya görünen metinleri yerelleştirme anahtarlarıyla çıkarır.

Anahtar şeması Core'daki ContentKeys ile birebir aynıdır (Assets/Scripts/Core/Localization/ContentKeys.cs).
Bir tarafı değiştirirsen ötekini de değiştir; ContentLocalizationTests ikisinin uyumunu denetler.

Kullanım (modül olarak):
    from content_keys import extract
    for key, text, context in extract(items, life, appearance): ...
"""

import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
CONTENT = ROOT / "Assets" / "Resources" / "Content"
LOC = ROOT / "Assets" / "Resources" / "Localization"

APPEARANCE_GROUPS = [
    "hairStyles",
    "bangs",
    "mustaches",
    "beards",
    "facialHair",
    "faceShapes",
    "eyeShapes",
    "brows",
    "noses",
    "mouths",
    "ears",
    "faceMarks",
    "skinDetails",
    "scarPatterns",
    "scarZones",
    "tattooPatterns",
    "tattooZones",
    "jewelry",
    "makeup",
    "outfits",
    "bottoms",
    "shoes",
    "outers",
    "hats",
    "patterns",
    "laughs",
]


def _s(v):
    return v if isinstance(v, str) and v.strip() else None


def extract(items, life, appearance):
    """(anahtar, metin, bağlam) üçlüleri döndürür. Boş metinler atlanır."""
    out = []

    def add(key, text, ctx):
        t = _s(text)
        if t is not None:
            out.append((key, t, ctx))

    for it in items.get("items", []) or []:
        iid = it["id"]
        add(f"{iid}.name", it.get("displayName"), f"eşya ({it.get('category', 'furniture')})")
        for act in it.get("interactions", []) or []:
            k = f"{iid}.act.{act['id']}"
            add(k, act.get("displayName"), f"etkileşim, eşya: {it.get('displayName')}")
            add(k + ".mood", act.get("moodText"), f"ruh hali metni, eşya: {it.get('displayName')}")
    for r in items.get("recipes", []) or []:
        add(f"recipe.{r['id']}.name", r.get("displayName"), f"tarif, ürün: {r.get('produces')}")
    for a in items.get("animals", []) or []:
        add(f"{a['id']}.name", a.get("displayName"), "hayvan türü")
    for p in items.get("buildParts", []) or []:
        add(f"part.{p['id']}.name", p.get("displayName"), f"yapı parçası ({p.get('kind')})")
    for e in items.get("effects", []) or []:
        add(f"effect.{e['id']}.name", e.get("displayName"), "yemek/içki etkisi adı")
        add(f"effect.{e['id']}.desc", e.get("description"), "yemek/içki etkisi açıklaması")

    for t in life.get("traits", []) or []:
        add(f"{t['id']}.name", t.get("displayName"), "karakter özelliği")
        add(f"{t['id']}.desc", t.get("description"), "karakter özelliği açıklaması")
    for b in life.get("branches", []) or []:
        add(f"{b['id']}.name", b.get("displayName"), f"meslek dalı ({b.get('skill')})")
        add(f"{b['id']}.desc", b.get("description"), "meslek dalı açıklaması")
    for p in life.get("projects", []) or []:
        add(f"{p['id']}.name", p.get("displayName"), "köy projesi")
        add(f"{p['id']}.desc", p.get("description"), "köy projesi açıklaması")
    for f in life.get("festivals", []) or []:
        add(f"{f['id']}.name", f.get("displayName"), "festival")
        add(f"{f['id']}.desc", f.get("description"), "festival açıklaması")
    for g in life.get("goals", []) or []:
        add(f"{g['id']}.name", g.get("displayName"), "hanedan hedefi")
        add(f"{g['id']}.desc", g.get("description"), "hanedan hedefi açıklaması")
    for t in life.get("tools", []) or []:
        add(f"tool.{t['id']}.name", t.get("displayName"), "alet")
        for i, tier in enumerate(t.get("tiers", []) or []):
            add(f"tool.{t['id']}.tier.{i}", tier.get("name"), f"alet kademesi ({t.get('displayName')})")
    for t in life.get("templates", []) or []:
        add(f"{t['id']}.name", t.get("displayName"), "oda şablonu")
    for v in life.get("villagers", []) or []:
        # Köylü adları çevrilmez (dönem/kültür kararına bağlı); yalnızca meslek adı ve olay metinleri.
        add(f"{v['id']}.role", v.get("roleName"), "köylünün mesleği")
        for ev in v.get("events", []) or []:
            add(f"{v['id']}.event.{ev['id']}", ev.get("text"), f"köylü ilişki olayı ({v.get('name')})")

    for group in APPEARANCE_GROUPS:
        for o in appearance.get(group, []) or []:
            add(f"look.{group}.{o['id']}", o.get("displayName"), f"görünüm ({group})")

    return out


def load_content(root=CONTENT):
    def rd(name):
        p = Path(root) / name
        return json.loads(p.read_text(encoding="utf-8")) if p.exists() else {}
    return rd("items.json"), rd("life.json"), rd("appearance.json")


def read_table(path):
    p = Path(path)
    return json.loads(p.read_text(encoding="utf-8")) if p.exists() else {}


def write_table(path, table):
    """Anahtarları sıralı, UTF-8, satır başına bir giriş olacak biçimde yazar (diff dostu)."""
    p = Path(path)
    p.parent.mkdir(parents=True, exist_ok=True)
    keys = sorted(table)
    lines = ["{"]
    for i, k in enumerate(keys):
        comma = "," if i < len(keys) - 1 else ""
        lines.append(f"  {json.dumps(k, ensure_ascii=False)}: {json.dumps(table[k], ensure_ascii=False)}{comma}")
    lines.append("}")
    p.write_text("\n".join(lines) + "\n", encoding="utf-8")
