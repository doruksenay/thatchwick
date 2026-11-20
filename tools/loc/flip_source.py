"""Tek seferlik geçiş: içerik JSON'larının metinlerini Türkçeden İngilizceye çevirir.

Ne zaman: açık özellik dalları main'e birleşince (yoksa her dalda çakışma çıkar).
Ne yapar:
  1. items.json / life.json / appearance.json içindeki Türkçe metinleri content.tr.json'a yazar.
  2. Aynı alanlara content.en.json'daki İngilizce metni koyar (kaynak dil İngilizce olur).
  3. Sonra elle: Assets/Scripts/Core/Localization/ContentLocalizer.cs'te JsonLanguage = Languages.English,
     tools/loc/check.py'de JSON_LANGUAGE = "en".

Kullanım:
    python tools/loc/check.py              # önce eksik çeviri kalmamalı
    python tools/loc/flip_source.py --uygula
"""

import json
import sys

from content_keys import APPEARANCE_GROUPS, CONTENT, LOC, extract, load_content, read_table, write_table


def _set(obj, field, key, en, tr_out):
    text = obj.get(field)
    if isinstance(text, str) and text.strip():
        tr_out[key] = text
        obj[field] = en[key]


def flip(items, life, appearance, en):
    """JSON nesnelerini yerinde değiştirir; Türkçe tabloyu döndürür."""
    tr = {}
    for it in items.get("items", []) or []:
        _set(it, "displayName", f"{it['id']}.name", en, tr)
        for act in it.get("interactions", []) or []:
            k = f"{it['id']}.act.{act['id']}"
            _set(act, "displayName", k, en, tr)
            _set(act, "moodText", k + ".mood", en, tr)
    for r in items.get("recipes", []) or []:
        _set(r, "displayName", f"recipe.{r['id']}.name", en, tr)
    for a in items.get("animals", []) or []:
        _set(a, "displayName", f"{a['id']}.name", en, tr)
    for p in items.get("buildParts", []) or []:
        _set(p, "displayName", f"part.{p['id']}.name", en, tr)
    for e in items.get("effects", []) or []:
        _set(e, "displayName", f"effect.{e['id']}.name", en, tr)
        _set(e, "description", f"effect.{e['id']}.desc", en, tr)
    for group in ("traits", "branches", "projects", "festivals", "goals"):
        for o in life.get(group, []) or []:
            _set(o, "displayName", f"{o['id']}.name", en, tr)
            _set(o, "description", f"{o['id']}.desc", en, tr)
    for t in life.get("tools", []) or []:
        _set(t, "displayName", f"tool.{t['id']}.name", en, tr)
        for i, tier in enumerate(t.get("tiers", []) or []):
            _set(tier, "name", f"tool.{t['id']}.tier.{i}", en, tr)
    for t in life.get("templates", []) or []:
        _set(t, "displayName", f"{t['id']}.name", en, tr)
    for v in life.get("villagers", []) or []:
        _set(v, "roleName", f"{v['id']}.role", en, tr)
        for ev in v.get("events", []) or []:
            _set(ev, "text", f"{v['id']}.event.{ev['id']}", en, tr)
    for group in APPEARANCE_GROUPS:
        for o in appearance.get(group, []) or []:
            _set(o, "displayName", f"look.{group}.{o['id']}", en, tr)
    return tr


def main(argv):
    items, life, appearance = load_content()
    en = read_table(LOC / "content.en.json")
    missing = [k for k, _, _ in extract(items, life, appearance)
               if not en.get(k, "").strip() or en[k].startswith("TODO:")]
    if missing:
        print(f"{len(missing)} metnin İngilizcesi eksik; önce tools/loc/check.py temiz olmalı.")
        return 1
    if "--uygula" not in argv:
        print("Kuru çalıştırma: hiçbir dosya değişmedi. Uygulamak için --uygula.")
        return 0

    tr = flip(items, life, appearance, en)
    for name, data in (("items.json", items), ("life.json", life), ("appearance.json", appearance)):
        (CONTENT / name).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    write_table(LOC / "content.tr.json", tr)
    print(f"{len(tr)} Türkçe metin content.tr.json'a taşındı; JSON'lar artık İngilizce.")
    print("Şimdi ContentLocalizer.JsonLanguage ve check.py JSON_LANGUAGE değerlerini 'en' yap.")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
