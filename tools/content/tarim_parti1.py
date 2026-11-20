#!/usr/bin/env python3
"""Tarım parti 1 (Project belgeleri 58 §4, 59, 59a): içerik göçü. Bir kez çalıştırıldı; yeniden çalıştırmak zararsızdır.

- Hasat aleti çapadan orağa geçer (çapa yalnızca çapalama). Yeni aletler: orak, tırpan (life.json).
- "Seçme tohumla hasat" kendir, çivit otu, kökboya ve sarı boya otunda keten veriyordu: kendi ürününü verir.
- Safran şafakta toplanırsa bir tel fazla (32 §8.10): bestHours.
- Tohum yapıcı işlevsizdi: "Tohum kalburu" olur, ürünü tohuma çevirir.
"""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
ITEMS = ROOT / "Assets/Resources/Content/items.json"
LIFE = ROOT / "Assets/Resources/Content/life.json"
EN = ROOT / "Assets/Resources/Localization/content.en.json"

d = json.loads(ITEMS.read_text(encoding="utf-8"))
life = json.loads(LIFE.read_text(encoding="utf-8"))
en = json.loads(EN.read_text(encoding="utf-8"))
items = {i["id"]: i for i in d["items"]}
seed_of = {i["plantsAs"]: i for i in d["items"] if i.get("plantsAs")}


def act(item, aid):
    return next((x for x in item.get("interactions", []) if x["id"] == aid), None)


# ---- 1. hasat aleti ve seçme tohum ----
for crop in [i for i in d["items"] if i.get("tilled")]:
    harvest = act(crop, "harvest")
    select = act(crop, "select_seed")
    for a in (harvest, select):
        if a and a.get("tool") == "hoe":
            a["tool"] = "sickle"
    seed = seed_of.get(crop["id"])
    if not (harvest and select and seed):
        continue
    main = harvest["produce"][0]
    want = [{"item": main["item"], "count": max(1, main["count"] - 1)}, {"item": seed["id"], "count": 3}]
    if [p["item"] for p in select["produce"]] != [p["item"] for p in want]:
        select["produce"] = want

# ---- 2. safran: şafakta bir tel fazla ----
saffron = act(items["item.crop_saffron"], "harvest")
saffron["bestHours"] = [5, 10]
saffron["bestHoursBonus"] = 1
saffron["displayName"] = "Safran topla (şafakta bir tel fazla)"
en["item.crop_saffron.act.harvest"] = "Pick saffron (one extra thread at dawn)"

# ---- 3. tohum kalburu ----
sieve = items["item.seed_maker"]
sieve["displayName"] = "Tohum kalburu"
en["item.seed_maker.name"] = "Seed sieve"
made = []
for crop in [i for i in d["items"] if i.get("tilled")]:
    seed = seed_of.get(crop["id"])
    harvest = act(crop, "harvest")
    if not seed or not harvest or crop["id"] == "item.crop_vetch":
        continue
    src = harvest["produce"][0]["item"]
    src = items[src].get("threshesTo", src)   # tahılda demet değil tane elenir
    if src == seed["id"] or src not in items:
        continue
    short = crop["id"].replace("item.crop_", "")
    aid = f"sieve_{short}"
    made.append({
        "id": aid,
        "displayName": f"Tohum ayır: {items[src]['displayName']} (1 → 2 tohum)",
        "durationDays": 0.03,
        "skill": "Farming",
        "skillXp": 3,
        "materials": [{"item": src, "count": 1}],
        "produce": [{"item": seed["id"], "count": 2}],
    })
    en[f"item.seed_maker.act.{aid}"] = f"Sieve seed: {en.get(src + '.name', src)} (1 → 2 seeds)"
sieve["interactions"] = made

# ---- 4. aletler ----
def tool(tid, name, tiers):
    return {"id": tid, "displayName": name, "tiers": [
        {"name": n, "akce": a, "firewood": f, "speed": s, "level": lv} for (n, a, f, s, lv) in tiers]}

have = {t["id"] for t in life["tools"]}
if "sickle" not in have:
    life["tools"].append(tool("sickle", "Orak", [("Demir orak", 45, 3, 1.2, 3), ("Çelik orak", 120, 6, 1.4, 6), ("Usta orağı", 260, 10, 1.6, 9)]))
if "scythe" not in have:
    # Tırpan taban kademede yoktur (0/3): ilk kademesi alınınca tahıl, lif ve otta her biçiş yandaki iki karoyu da biçer.
    life["tools"].append(tool("scythe", "Tırpan", [("Demir tırpan", 90, 5, 1.0, 4), ("Çelik tırpan", 180, 8, 1.15, 7), ("Usta tırpanı", 340, 12, 1.3, 9)]))
en.update({
    "tool.sickle.name": "Sickle", "tool.sickle.tier.0": "Iron sickle", "tool.sickle.tier.1": "Steel sickle", "tool.sickle.tier.2": "Master's sickle",
    "tool.scythe.name": "Scythe", "tool.scythe.tier.0": "Iron scythe", "tool.scythe.tier.1": "Steel scythe", "tool.scythe.tier.2": "Master's scythe",
})

# ---- yaz ----
ITEMS.write_text(json.dumps(d, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
unlocks = life.pop("skillUnlocks")
s = json.dumps(life, indent=2, ensure_ascii=False)
lines = ",\n".join("    " + json.dumps(u, ensure_ascii=False) for u in unlocks)
LIFE.write_text(s[:-2] + ',\n  "skillUnlocks": [\n' + lines + "\n  ]\n}\n", encoding="utf-8")
EN.write_text(json.dumps(en, ensure_ascii=False, indent=2, sort_keys=True), encoding="utf-8")
print("tohum kalburu işleri:", len(made))
