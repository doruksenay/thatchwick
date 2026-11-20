#!/usr/bin/env python3
"""Tarım parti 2 (Project belgesi 59 §1-3): içerik göçü. Yeniden çalıştırmak zararsızdır.

- Hasat döngüleri: bezelye ve bakla devşirme (3 toplama), marul, ıspanak ve mutfak otu yatağı kes-yine-gelir (3 kesim),
  safran ve kökboya kalıcı yatak. Yeniden veren ekinin olağan hasadı tohum vermez: tohum sararınca ("Sök") ya da
  "Seçme tohumla hasat" ile gelir.
- Sırık: bezelye sırık ister; "Sırık dik" ve kütükten ekin sırığı.
- Sararma: her tarla ekinine "Sök" (yan ürünü kod aileye göre verir).
- Dev ürün: şalgam, lahana, su kabağı, turp, pancar için tarladaki 2×2 dev ürün ve sergilik eşya.
- "Bereketli hasat" moodlet'i yalnız şalgamdaydı: bütün tarla ekinlerinin hasadına.
"""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
ITEMS = ROOT / "Assets/Resources/Content/items.json"
LIFE = ROOT / "Assets/Resources/Content/life.json"
EN = ROOT / "Assets/Resources/Localization/content.en.json"

d = json.loads(ITEMS.read_text(encoding="utf-8"))
en = json.loads(EN.read_text(encoding="utf-8"))
items = {i["id"]: i for i in d["items"]}
seed_of = {i["plantsAs"]: i for i in d["items"] if i.get("plantsAs")}


def act(item, aid):
    return next((x for x in item.get("interactions", []) if x["id"] == aid), None)


def add_item(obj, en_name):
    if obj["id"] not in items:
        d["items"].append(obj)
        items[obj["id"]] = obj
    else:
        items[obj["id"]].update(obj)
    en[obj["id"] + ".name"] = en_name


# ---- 1. hasat döngüleri ----
CYCLES = {
    # ekin: (döngü, en çok toplama, yeniden olgunlaşma günü, hasat ürünü, Türkçe ad, İngilizce ad)
    "item.crop_pea": ("repeat", 3, 2, [("item.pea", 2, 1)], "Devşir (bitki kalır)", "Pick (plant keeps bearing)"),
    "item.crop_broad_bean": ("repeat", 3, 2, [("item.broad_bean", 2, 1)], "Devşir (bitki kalır)", "Pick (plant keeps bearing)"),
    "item.crop_lettuce": ("cut", 3, 2, [("item.lettuce", 2, 1)], "Yaprak kes (yine gelir)", "Cut leaves (grows back)"),
    "item.crop_spinach": ("cut", 3, 2, [("item.spinach", 2, 1)], "Yaprak kes (yine gelir)", "Cut leaves (grows back)"),
    "item.crop_herb_bed": ("cut", 3, 2, [("item.herb", 2, 1), ("item.dill", 1, 1), ("item.fenugreek", 1, 1), ("item.lavender", 1, 1)],
                           "Ot kes (yine gelir)", "Cut herbs (grows back)"),
    # Safran yılda bir, sonbaharda açar; soğanı yatakta yavaş çoğalır.
    "item.crop_saffron": ("perennial", 0, 6, [("item.saffron", 1, 1), ("item.saffron_corm", 1, 0.5)], None, None),
    # Kökboya: kök alınır, yatak kalır ve yeniden köklenir.
    "item.crop_madder": ("perennial", 0, 10, [("item.madder_root", 3, 1), ("item.madder_root", 1, 0.5), ("item.seed_madder", 1, 0.5)],
                         "Kök al (yatak kalır)", "Dig roots (bed stays)"),
}
for cid, (cycle, max_h, regrow, rows, tr, eng) in CYCLES.items():
    crop = items[cid]
    crop["cycle"] = cycle
    if max_h:
        crop["maxHarvests"] = max_h
    crop["regrowDays"] = regrow
    if cycle == "perennial":
        crop.pop("diesInWinter", None)   # kalıcı yatak kışı toprakta geçirir
    harvest = act(crop, "harvest")
    harvest["produce"] = [({"item": i, "count": c} if ch >= 1 else {"item": i, "count": c, "chance": ch}) for (i, c, ch) in rows]
    if tr:
        harvest["displayName"] = tr
        en[f"{cid}.act.harvest"] = eng

# ---- 2. sırık ----
add_item({"id": "item.crop_stake", "displayName": "Ekin sırığı", "placement": "Resource", "price": 1, "category": "resource"}, "Crop stake")
block = items["item.chopping_block"]
if not act(block, "make_stakes"):
    block["interactions"].append({
        "id": "make_stakes", "displayName": "Ekin sırığı yont (1 kütük → 6 sırık)", "durationDays": 0.04,
        "skill": "Woodcutting", "skillXp": 4, "consume": "item.log", "produce": [{"item": "item.crop_stake", "count": 6}], "tool": "axe",
    })
en["item.chopping_block.act.make_stakes"] = "Whittle crop stakes (1 log → 6 stakes)"
for cid in ["item.crop_pea"]:
    crop = items[cid]
    crop["needsStake"] = True
    if not act(crop, "stake"):
        idx = next(i for i, x in enumerate(crop["interactions"]) if x["id"] == "water") + 1
        crop["interactions"].insert(idx, {
            "id": "stake", "displayName": "Sırık dik (sırıksız ürün yarım)", "durationDays": 0.03,
            "skill": "Farming", "skillXp": 3, "stake": True, "consume": "item.crop_stake",
        })
    en[f"{cid}.act.stake"] = "Set a stake (half yield without one)"

# ---- 3. sararmış ekini sök; bereketli hasat ----
for crop in [i for i in d["items"] if i.get("tilled")]:
    harvest = act(crop, "harvest")
    if harvest and seed_of.get(crop["id"]) and crop["id"] != "item.crop_vetch":
        harvest["moodlet"] = "good_harvest"
    if not act(crop, "uproot"):
        idx = next((i for i, x in enumerate(crop["interactions"]) if x["id"] == "harvest"), 0) + 1
        crop["interactions"].insert(idx, {
            "id": "uproot", "displayName": "Sök (sararmış ekin, yan ürün verir)", "durationDays": 0.04,
            "skill": "Farming", "skillXp": 4, "requiresWithered": True, "removeItem": True,
        })
    en[f"{crop['id']}.act.uproot"] = "Pull up (withered crop, gives a by-product)"

# ---- 4. dev ürün ----
GIANTS = {
    # ekin kısa adı: (ürün, Türkçe, İngilizce)
    "turnip": ("item.turnip", "Dev şalgam", "Giant turnip"),
    "cabbage": ("item.cabbage", "Dev lahana", "Giant cabbage"),
    "pumpkin": ("item.pumpkin", "Dev su kabağı", "Giant gourd"),
    "radish": ("item.radish", "Dev turp", "Giant radish"),
    "beet": ("item.beet", "Dev pancar", "Giant beet"),
}
for short, (main, tr, eng) in GIANTS.items():
    crop = items[f"item.crop_{short}"]
    seed = seed_of[crop["id"]]["id"]
    field_id, show_id = f"item.giant_crop_{short}", f"item.giant_{short}"
    add_item({
        "id": show_id, "displayName": f"{tr} (sergilik)", "placement": "Resource", "category": "resource",
        "price": max(20, items[main].get("price", 2) * 10), "tags": ["giant"],
    }, f"{eng} (show piece)")
    items[show_id].pop("spoilDays", None)   # bozulma yalnızca yiyecekte; sergilik ürünün ömrü şenlik partisinde (57) bağlanır
    add_item({
        "id": field_id, "displayName": tr, "category": "plant", "width": 2, "height": 2, "site": "outdoor",
        "interactions": [{
            "id": "harvest_giant", "displayName": "Dev ürünü kaldır", "durationDays": 0.15,
            "skill": "Farming", "skillXp": 40, "removeItem": True, "tool": "axe", "moodlet": "good_harvest",
            "produce": [
                {"item": show_id, "count": 1},
                {"item": main, "count": 10},
                {"item": main, "count": 2, "chance": 0.5},
                {"item": main, "count": 2, "chance": 0.5},
                {"item": seed, "count": 2},
            ],
        }],
        "art": {"alongX": f"iso_crop_giant_{short}_2x2_se", "pivotX": 64.0, "pivotY": 32.0, "mirror": True, "pending": True},
    }, eng)
    en[f"{field_id}.act.harvest_giant"] = "Lift the giant crop"
    crop["giantAs"] = field_id

# ---- yaz ----
ITEMS.write_text(json.dumps(d, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
EN.write_text(json.dumps(en, ensure_ascii=False, indent=2, sort_keys=True), encoding="utf-8")

# life.json: yalnızca Çiftçilik 8 pasifinin metni değişir (dosya biçimi korunur).
life = LIFE.read_text(encoding="utf-8")
old = '"text": "İri ürün şansı (Hasat Şenliği yarışması)"'
new = '"text": "Dev ürün şansı iki kat (2×2 olgun ve sulu tarla; Hasat Şenliği yarışması)"'
if old in life:
    LIFE.write_text(life.replace(old, new), encoding="utf-8")
print("döngü:", len(CYCLES), "dev ürün:", len(GIANTS))
