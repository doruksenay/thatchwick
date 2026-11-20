"""Belge 52 (ulu ağaçlar ve ağaç evler) içeriği: ulu ağaçlar, ağaç ev iskeletleri, tarifler ve İngilizce metinler.

Kullanım: python tools/content/agac_evleri.py
Betik kendini tekrar çalıştırmaya dayanıklıdır: aynı kimlikleri siler ve yeniden yazar. Çakışmada main'in
items.json'u üstüne yeniden çalıştırılır.
"""

import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "loc"))
from content_keys import write_table  # noqa: E402

ROOT = Path(__file__).resolve().parents[2]
ITEMS = ROOT / "Assets" / "Resources" / "Content" / "items.json"
EN = ROOT / "Assets" / "Resources" / "Localization" / "content.en.json"

KINDS = [
    # anahtar, Türkçe, İngilizce, Marangozluk, malzemeler, süre, XP, satış fiyatı
    ("nest", "Dal Yuvası", "Branch Nest", 2, [("item.plank", 8), ("item.nails", 4), ("item.linen_yarn", 2)], 0.20, 30, 30),
    ("lookout", "Gözcü Kulesi", "Lookout Tower", 4, [("item.plank", 14), ("item.nails", 6), ("item.linen_yarn", 2), ("item.log", 2)], 0.30, 45, 52),
    ("fort", "Tahta Kale", "Wooden Fort", 5, [("item.plank", 18), ("item.nails", 8), ("item.cloth", 1)], 0.35, 55, 80),
    ("hut", "Saz Damlı Kulübe", "Thatched Hideout", 6, [("item.plank", 16), ("item.nails", 8), ("item.straw", 6), ("item.linen", 2)], 0.40, 65, 75),
    ("kiosk", "Ağaç Köşkü", "Tree Pavilion", 8, [("item.plank", 24), ("item.nails", 12), ("item.roof_tiles", 6), ("item.iron_bar", 1)], 0.50, 90, 120),
]

TREES = [
    # kimlik, Türkçe, İngilizce, boy, etiketler, toplama ürünleri (ürün, adet, şans, mevsimler)
    ("item.great_oak", "Ulu meşe", "Great oak", 2, ["kiosk5"],
     [("item.oak_gall", 2, 1.0, ["Summer", "Autumn"]), ("item.viv_bark", 1, 0.5, None)], "Mazı ve kabuk topla", "Gather galls and bark"),
    ("item.great_linden", "Ulu ıhlamur", "Great linden", 2, [],
     [("item.linden", 3, 1.0, ["Summer"]), ("item.viv_moss", 1, 0.5, None)], "Ihlamur çiçeği topla", "Gather linden blossom"),
    ("item.great_walnut", "Ulu ceviz", "Great walnut", 2, [],
     [("item.walnut", 3, 1.0, ["Autumn"]), ("item.viv_bark", 1, 0.5, None)], "Ceviz topla", "Gather walnuts"),
    ("item.great_chestnut", "Ulu kestane", "Great chestnut", 2, [],
     [("item.chestnut", 4, 1.0, ["Autumn"]), ("item.viv_moss", 1, 0.5, None)], "Kestane topla", "Gather chestnuts"),
    ("item.great_plane", "Ulu çınar", "Great plane tree", 3, ["kiosk5", "inn_kiosk"],
     [("item.viv_bark", 1, 1.0, None), ("item.viv_moss", 1, 0.5, None)], "Kabuk ve yosun topla", "Gather bark and moss"),
]


def act(id_, tr, action, dur, **kw):
    a = {"id": id_, "displayName": tr, "durationDays": dur, "treeAction": action}
    a.update(kw)
    return a


def tree_interactions(kiosk_ok, inn, gather_tr, gather_produce):
    acts = []
    if not inn:
        for key, tr, _, _, _, _, _, _ in KINDS:
            acts.append(act(f"found_{key}", f"Ağaç ev kur: {tr}", "found", 0.12, skill="Carpentry", skillXp=20,
                            materials=[{"item": f"item.th_kit_{key}", "count": 1}]))
        acts += [
            act("build", "Ağaç evin yapımına devam et", "build", 0.15, skill="Carpentry", skillXp=20),
            act("repair", "Ağaç evi onar", "repair", 0.10, skill="Carpentry", skillXp=10,
                materials=[{"item": "item.plank", "count": 2}, {"item": "item.nails", "count": 2}]),
            act("play", "Ağaç evde oyna", "play", 0.12, need="Fun", restore=40, extraRestore=[{"need": "Social", "amount": 15}]),
            act("lookout", "Gözcülük oyna", "lookout", 0.10, need="Fun", restore=30, skill="WeatherLore", skillXp=4),
            act("siege", "Kale kuşatması oyna", "siege", 0.12, need="Fun", restore=45, skill="Athletics", skillXp=4,
                extraRestore=[{"need": "Social", "amount": 20}]),
            act("sleep", "Kulübede uyu", "sleep", 0.30, need="Energy", restore=70),
            act("stash", "Hazine sakla", "stash", 0.04),
            act("dig", "Hazine ara", "dig", 0.05),
            act("inn_toggle", "Köşkte aşevi aç ya da kapat", "innToggle", 0.02),
            act("call_down", "Küsen çocuğu aşağı çağır", "callDown", 0.05, skill="Parenting", skillXp=4),
        ]
    produce = []
    for item, n, chance, seasons in gather_produce:
        p = {"item": item, "count": n}
        if chance < 1.0:
            p["chance"] = chance
        if seasons:
            p["seasons"] = seasons
        produce.append(p)
    acts += [
        act("carve", "Adını gövdeye kazı", "carve", 0.04),
        act("gather", gather_tr, "gather", 0.06, skill="Foraging", skillXp=5, produce=produce),
        act("shade", "Gölgesinde otur", "shade", 0.08, need="Fun", restore=15, extraRestore=[{"need": "Energy", "amount": 5}]),
    ]
    if inn:
        acts += [
            act("inn_soup", "Aşevinde çorba iç (5 akçe)", "dine", 0.06, need="Hunger", restore=45),
            act("inn_eat", "Aşevinde yahni ye (8 akçe)", "dine", 0.08, need="Hunger", restore=70, extraRestore=[{"need": "Fun", "amount": 10}]),
            act("inn_roast", "Aşevinde ziyafet çek (14 akçe)", "dine", 0.10, need="Hunger", restore=90,
                extraRestore=[{"need": "Fun", "amount": 20}, {"need": "Social", "amount": 10}]),
            act("inn_charity", "Hayır sofrasına otur", "charity", 0.08, need="Hunger", restore=45),
            act("donate_10", "Aşevine bağış yap (10 akçe)", "donate", 0.02),
            act("donate_50", "Aşevine bağış yap (50 akçe)", "donate", 0.02),
            act("donate_100", "Aşevine bağış yap (100 akçe)", "donate", 0.02),
            act("donate_food", "Aşevine yiyecek bağışla (en çok 5)", "donateFood", 0.03),
        ]
    return acts


EN_ACTS = {
    "found_nest": "Build a treehouse: Branch Nest",
    "found_lookout": "Build a treehouse: Lookout Tower",
    "found_fort": "Build a treehouse: Wooden Fort",
    "found_hut": "Build a treehouse: Thatched Hideout",
    "found_kiosk": "Build a treehouse: Tree Pavilion",
    "build": "Keep building the treehouse",
    "repair": "Repair the treehouse",
    "play": "Play in the treehouse",
    "lookout": "Play lookout",
    "siege": "Play castle siege",
    "sleep": "Sleep in the hideout",
    "stash": "Hide a treasure",
    "dig": "Look for treasure",
    "inn_toggle": "Open or close a kitchen in the pavilion",
    "carve": "Carve your name in the trunk",
    "shade": "Sit in its shade",
    "call_down": "Call the sulking child down",
    "inn_soup": "Have soup at the kitchen (5 akçe)",
    "inn_eat": "Have stew at the kitchen (8 akçe)",
    "inn_roast": "Feast at the kitchen (14 akçe)",
    "donate_food": "Donate food to the kitchen (up to 5)",
    "inn_charity": "Sit at the charity table",
    "donate_10": "Donate to the kitchen (10 akçe)",
    "donate_50": "Donate to the kitchen (50 akçe)",
    "donate_100": "Donate to the kitchen (100 akçe)",
}


def main():
    data = json.loads(ITEMS.read_text(encoding="utf-8"))
    en = json.loads(EN.read_text(encoding="utf-8"))
    new_ids = {t[0] for t in TREES} | {f"item.th_kit_{k[0]}" for k in KINDS} | {"item.walnut"}
    data["items"] = [i for i in data["items"] if i["id"] not in new_ids]
    data["recipes"] = [r for r in data["recipes"] if not r["id"].startswith("th_kit_")]

    for iid, tr, eng, size, tags, produce, gtr, gen in TREES:
        inn = "inn_kiosk" in tags
        data["items"].append({
            "id": iid, "displayName": tr, "category": "village", "price": 0, "width": size, "height": size,
            "site": "outdoor", "belongs": ["outdoor"], "tags": ["great_tree"] + tags,
            "interactions": tree_interactions(True, inn, gtr, produce),
        })
        en[f"{iid}.name"] = eng
        for a in tree_interactions(True, inn, gtr, produce):
            en[f"{iid}.act.{a['id']}"] = gen if a["id"] == "gather" else EN_ACTS[a["id"]]

    data["items"].append({"id": "item.walnut", "displayName": "Ceviz", "placement": "Resource", "price": 3,
                          "category": "resource", "tags": ["food"], "spoilDays": 60})
    en["item.walnut.name"] = "Walnuts"
    # Ceviz kırılıp yenir (kestane gibi tencerede değil, ama aynı yerde: ocak başı tenceresi).
    for i in data["items"]:
        if i["id"] == "item.cooking_pot":
            i["interactions"] = [a for a in i["interactions"] if a["id"] != "eat_walnut"]
            i["interactions"].append({"id": "eat_walnut", "displayName": "Ceviz kır ve ye", "need": "Hunger", "restore": 30,
                                      "durationDays": 0.05, "consume": "item.walnut", "moodId": "mood.meal",
                                      "moodText": "Taze ceviz", "moodValue": 3, "moodDays": 1.0, "bladder": 3})
    en["item.cooking_pot.act.eat_walnut"] = "Crack and eat walnuts"
    en["item.cooking_pot.act.eat_walnut.mood"] = "Fresh walnuts"

    for key, tr, eng, level, mats, dur, xp, price in KINDS:
        kid = f"item.th_kit_{key}"
        data["items"].append({"id": kid, "displayName": f"{tr} iskeleti", "placement": "Resource", "price": price,
                              "category": "resource", "tags": ["craft", "treehouse_kit"]})
        en[f"{kid}.name"] = f"{eng} frame"
        rid = f"th_kit_{key}"
        data["recipes"].append({"id": rid, "station": "item.workbench", "produces": kid,
                                "materials": [{"item": m, "count": n} for m, n in mats], "minLevel": level,
                                "durationDays": dur, "skillXp": xp, "displayName": f"Ağaç ev iskeleti: {tr}"})
        en[f"recipe.{rid}.name"] = f"Treehouse frame: {eng}"

    ITEMS.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    write_table(EN, en)
    print("ulu ağaç:", len(TREES), "iskelet:", len(KINDS))


if __name__ == "__main__":
    main()
