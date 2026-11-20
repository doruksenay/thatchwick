"""Mutfak aletleri (Project belgeleri 63, 63a): yeni kaplar ve aletler, pişirme yöntemleri, bulaşık, tarifler, metinler.

Kullanım:  python tools/content/mutfak_aletleri.py
Yeniden çalıştırılabilir: eklediği eşya ve tarifleri baştan yazar, var olan etkileşimlere cook ve needTools alanlarını
yeniden koyar.

- Pişirme ocakta olur: kazandaki pişirme işleri taş ocağa ve büyük şömineye taşınır. Kazanda yalnızca "ye" kalır.
- "Pişirme kazanı" Demir kazan olur (kademe 2, taşınan kap); bakır kazan seti ve bakır tencere kademe 3.
- Her pişirme işi bir yöntem alır (boil, fry, stew, grill, bake, ember, raw); yöntem kabı ister.
- Bıçak, oklava, fırın küreği zorunlu; kesme tahtası, kaşık, kepçe, kevgir yardımcı (63a-K1).
"""

import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
ITEMS = ROOT / "Assets/Resources/Content/items.json"
EN = ROOT / "Assets/Resources/Localization/content.en.json"

POT, COPPER_SET = "item.cooking_pot", "item.copper_pots"
HEARTHS = ["item.hearth_stone", "item.fireplace_large"]
OVEN = "item.stone_oven"


def art(name, px=16.0, py=8.0):
    return {"alongX": name, "pivotX": px, "pivotY": py, "mirror": True, "pending": True}


def decor(id_, tr, en, price, level, style, size, weight, material, fits, equip, tier=0, beauty=1):
    d = {
        "id": id_, "displayName": tr, "category": "kitchen", "price": price, "level": level, "placement": "Surface",
        "styleSet": style, "rooms": ["kitchen"], "decorSize": size, "weight": weight, "beauty": beauty,
        "material": material, "fitsOn": fits, "equip": equip,
    }
    if tier:
        d["equipTier"] = tier
    d["art"] = art(id_.replace("item.", "iso_item_") + "_se")
    return d, en


ON_HOOK = ["kanca", "raf", "masa", "ocak"]
ON_SHELF = ["raf", "masa", "ocak"]

NEW = [
    decor("item.pot_clay", "Kil tencere", "Clay pot", 6, 1, "Köylü", "M", 3, "clay", ON_SHELF, "vessel:boil", 1),
    decor("item.griddle_iron", "Demir sac", "Iron griddle", 18, 2, "Köylü", "M", 4, "metal", ON_HOOK, "vessel:fry", 2),
    decor("item.pan_copper", "Bakır tava", "Copper frying pan", 45, 6, "Tüccar", "M", 3, "metal", ON_HOOK, "vessel:fry", 3, 2),
    decor("item.guvec_plain", "Sırsız güveç", "Plain earthenware casserole", 5, 1, "Köylü", "M", 3, "clay", ON_SHELF, "vessel:stew", 1),
    decor("item.guvec_glazed", "Sırlı güveç", "Glazed casserole", 14, 3, "Zanaatkâr", "M", 3, "clay", ON_SHELF, "vessel:stew", 2, 2),
    decor("item.guvec_fine", "Çini sırlı güveç", "Fine glazed casserole", 40, 7, "Tüccar", "M", 3, "clay", ON_SHELF, "vessel:stew", 3, 3),
    decor("item.spit_iron", "Demir şiş", "Iron spits", 10, 2, "Köylü", "M", 2, "metal", ON_HOOK, "vessel:grill", 2),
    decor("item.spit_steel", "Çelik şiş ve ızgara", "Steel skewers and grate", 38, 6, "Tüccar", "M", 3, "metal", ON_HOOK, "vessel:grill", 3, 2),
    decor("item.baking_tray_clay", "Kil fırın tepsisi", "Clay baking dish", 4, 1, "Köylü", "M", 2, "clay", ON_SHELF, "vessel:bake", 1),
    decor("item.baking_tray_copper", "Bakır fırın tepsisi", "Copper baking tray", 30, 5, "Tüccar", "M", 3, "metal", ON_HOOK, "vessel:bake", 3, 2),
    decor("item.trivet_iron", "Sacayağı", "Iron trivet", 6, 1, "Köylü", "S", 2, "metal", ON_SHELF, None),
    decor("item.kitchen_knife", "Mutfak bıçağı", "Kitchen knife", 8, 1, "Köylü", "S", 1, "metal", ON_HOOK, "tool:knife"),
    decor("item.cutting_board", "Kesme tahtası", "Chopping board", 4, 1, "Köylü", "S", 1, "wood", ON_HOOK, "tool:board"),
    decor("item.wooden_spoon", "Tahta kaşık", "Wooden spoon", 2, 1, "Köylü", "S", 1, "wood", ON_HOOK, "tool:spoon"),
    decor("item.rolling_board", "Oklava ve hamur tahtası", "Rolling pin and board", 6, 2, "Köylü", "M", 2, "wood", ON_SHELF, "tool:pin"),
    decor("item.colander", "Kevgir", "Colander", 12, 3, "Zanaatkâr", "M", 2, "metal", ON_HOOK, "tool:colander"),
]
# Sacayağı rolsüz (dışarıda pişirmenin eşyası; kural sonra): equip yok.
NEW = [(dict((k, v) for k, v in d.items() if v is not None), en) for d, en in NEW]

FLOOR = [
    ({
        "id": "item.baker_peel", "displayName": "Fırın küreği", "category": "kitchen", "price": 6, "level": 3,
        "styleSet": "Köylü", "rooms": ["kitchen"], "finishGroup": "wood", "site": "indoor", "belongs": ["kitchen"],
        "equip": "tool:peel", "art": art("iso_item_baker_peel_se", 16.0, 10.0),
    }, "Baker's peel"),
    ({
        "id": "item.dish_basket", "displayName": "Bulaşık sepeti", "category": "kitchen", "price": 8, "level": 1,
        "styleSet": "Köylü", "rooms": ["kitchen"], "finishGroup": "wood", "site": "any", "belongs": ["kitchen", "outdoor"],
        "equip": "basket", "art": art("iso_item_dish_basket_empty_se", 16.0, 10.0),
    }, "Dish basket"),
    ({
        "id": "item.dish_tub", "displayName": "Bulaşık leğeni", "category": "kitchen", "price": 10, "level": 1,
        "styleSet": "Köylü", "rooms": ["kitchen", "outdoor"], "finishGroup": "wood", "site": "any", "belongs": ["kitchen", "outdoor"],
        "washPlace": "tub", "art": art("iso_item_dish_tub_se", 16.0, 10.0),
        "interactions": [{"id": "wash_dishes", "displayName": "Bulaşığı yıka", "durationDays": 0.05, "washDishes": True,
                          "fetchWater": True, "skill": "Decorating", "skillXp": 1}],
    }, "Washing-up tub"),
    ({
        "id": "item.washing_stone", "displayName": "Yıkama taşı", "category": "garden", "price": 6, "level": 1,
        "styleSet": "Köylü", "rooms": ["outdoor"], "finishGroup": "stone", "site": "outdoor", "belongs": ["outdoor"],
        "washPlace": "stone", "art": art("iso_item_washing_stone_se", 16.0, 8.0),
        "interactions": [{"id": "wash_dishes", "displayName": "Bulaşığı kıyıda yıka", "durationDays": 0.05, "washDishes": True,
                          "skill": "Decorating", "skillXp": 1}],
    }, "Washing stone"),
]
NEW_EN_ACTS = {
    "item.dish_tub": {"wash_dishes": "Wash the dishes"},
    "item.washing_stone": {"wash_dishes": "Wash the dishes at the water"},
}

# Var olan eşyalara rol (63a §2, §5.1).
ROLES = {
    POT: {"displayName": "Demir kazan", "equip": "vessel:boil", "equipTier": 2},
    COPPER_SET: {"displayName": "Bakır kazan", "equip": "vessel:boil", "equipTier": 3},
    "item.copper_pot": {"equip": "vessel:boil", "equipTier": 3},
    "item.ladle": {"equip": "tool:ladle"},
    "item.wooden_bowl": {"equip": "tableware"}, "item.wooden_plate": {"equip": "tableware"},
    "item.clay_plate": {"equip": "tableware"}, "item.tin_plate": {"equip": "tableware"},
    "item.tile_bowl": {"equip": "tableware"},
    "item.hearth_stone": {"cookFire": True}, "item.fireplace_large": {"cookFire": True},
    "item.kitchen_spout": {"washPlace": "spout"},
    "item.wash_tub": {"washPlace": "tub"},
}
ROLE_EN = {POT: "Iron cauldron", COPPER_SET: "Copper cauldron"}
WASH_ACTS = {
    "item.kitchen_spout": {"id": "wash_dishes", "displayName": "Bulaşığı yalakta yıka", "durationDays": 0.04, "washDishes": True,
                           "piped": True, "skill": "Decorating", "skillXp": 1},
    "item.wash_tub": {"id": "wash_dishes", "displayName": "Bulaşığı leğende yıka", "durationDays": 0.05, "washDishes": True,
                      "fetchWater": True, "skill": "Decorating", "skillXp": 1},
}
WASH_EN = {"item.kitchen_spout": "Wash the dishes at the trough", "item.wash_tub": "Wash the dishes in the tub"}

RECIPES = [
    # (kimlik, istasyon, ürün, malzemeler, beceri, seviye, süre, xp, tr, en)
    ("kit_pot_clay", "item.pottery_wheel", "item.pot_clay", [("item.clay", 2)], "Pottery", 0, 0.06, 6, "Kil tencere yap", "Make a clay pot"),
    ("kit_guvec_plain", "item.pottery_wheel", "item.guvec_plain", [("item.clay", 2)], "Pottery", 1, 0.06, 6, "Sırsız güveç yap", "Make a plain casserole"),
    ("kit_guvec_glazed", "item.pottery_wheel", "item.guvec_glazed", [("item.clay", 2), ("item.ash", 1)], "Pottery", 3, 0.08, 10, "Sırlı güveç yap", "Make a glazed casserole"),
    ("kit_guvec_fine", "item.pottery_wheel", "item.guvec_fine", [("item.clay", 2), ("item.glazed_tile", 1)], "Pottery", 6, 0.1, 16, "Çini sırlı güveç yap", "Make a fine glazed casserole"),
    ("kit_tray_clay", "item.pottery_wheel", "item.baking_tray_clay", [("item.clay", 1)], "Pottery", 1, 0.05, 5, "Kil fırın tepsisi yap", "Make a clay baking dish"),
    ("kit_cauldron", "item.forge", POT, [("item.iron_bar", 2)], "Smithing", 2, 0.12, 14, "Demir kazan döv", "Forge an iron cauldron"),
    ("kit_griddle", "item.forge", "item.griddle_iron", [("item.iron_bar", 1)], "Smithing", 2, 0.08, 10, "Demir sac döv", "Forge an iron griddle"),
    ("kit_pan_copper", "item.forge", "item.pan_copper", [("item.copper", 2), ("item.tin", 1)], "Smithing", 6, 0.12, 18, "Bakır tava döv, kalayla", "Forge and tin a copper pan"),
    ("kit_spit_iron", "item.forge", "item.spit_iron", [("item.iron_bar", 1)], "Smithing", 1, 0.06, 8, "Demir şiş döv", "Forge iron spits"),
    ("kit_spit_steel", "item.forge", "item.spit_steel", [("item.iron_bar", 2), ("item.charcoal", 2)], "Smithing", 6, 0.12, 18, "Çelik şiş ve ızgara döv", "Forge steel skewers and a grate"),
    ("kit_tray_copper", "item.forge", "item.baking_tray_copper", [("item.copper", 1), ("item.tin", 1)], "Smithing", 5, 0.1, 14, "Bakır fırın tepsisi döv", "Forge a copper baking tray"),
    ("kit_trivet", "item.forge", "item.trivet_iron", [("item.iron_bar", 1)], "Smithing", 1, 0.05, 6, "Sacayağı döv", "Forge a trivet"),
    ("kit_knife", "item.forge", "item.kitchen_knife", [("item.iron_bar", 1), ("item.plank", 1)], "Smithing", 1, 0.06, 8, "Mutfak bıçağı döv", "Forge a kitchen knife"),
    ("kit_colander", "item.forge", "item.colander", [("item.copper", 1)], "Smithing", 3, 0.08, 10, "Kevgir döv", "Forge a colander"),
    ("kit_board", "item.workbench", "item.cutting_board", [("item.plank", 1)], "Carpentry", 0, 0.04, 4, "Kesme tahtası yap", "Make a chopping board"),
    ("kit_spoon", "item.workbench", "item.wooden_spoon", [("item.plank", 1)], "Carpentry", 0, 0.03, 3, "Tahta kaşık oy", "Carve a wooden spoon"),
    ("kit_rolling", "item.workbench", "item.rolling_board", [("item.plank", 2)], "Carpentry", 1, 0.05, 5, "Oklava ve hamur tahtası yap", "Make a rolling pin and board"),
    ("kit_peel", "item.workbench", "item.baker_peel", [("item.plank", 2)], "Carpentry", 1, 0.05, 5, "Fırın küreği yap", "Make a baker's peel"),
    ("kit_dish_tub", "item.workbench", "item.dish_tub", [("item.plank", 3), ("item.nails", 1)], "Carpentry", 2, 0.08, 8, "Bulaşık leğeni yap", "Make a washing-up tub"),
    ("kit_dish_basket", "item.workbench", "item.dish_basket", [("item.withy", 3)], "Weaving", 0, 0.05, 5, "Bulaşık sepeti ör", "Weave a dish basket"),
    ("kit_washing_stone", "item.stonecutter", "item.washing_stone", [("item.stone", 2)], "Masonry", 1, 0.06, 6, "Yıkama taşı yont", "Dress a washing stone"),
]

# ---- pişirme yöntemi ----
STEW = {"cook_stew", "make_stew_meat", "make_stew_game"}
FRY = {"cook_omelet", "make_saute_mushroom", "make_kaygana", "make_helva", "roast_coffee"}
GRILL = {"roast_meat", "roast_poultry", "roast_game"}
EMBER = {"eat_chestnut"}
RAW = {"make_breakfast", "make_wrap_fish", "miner_ration", "bait", "pastirma_pepper"}
NO_COOK = {"feast", "taste"}
CHOP = {"item.turnip", "item.cabbage", "item.leek", "item.onion", "item.carrot", "item.garlic", "item.mushroom", "item.meat",
        "item.poultry", "item.game_meat", "item.cheese", "item.parsnip", "item.beet", "item.radish", "item.apple"}
PIN = {"make_borek_leek", "make_kete_meat", "make_tart_apple", "make_simit", "make_yufka", "make_bazlama", "bake_wedding_bread"}
TRAY = {"make_borek_leek", "make_kete_meat", "make_tart_apple", "bake"}
STRAIN = {"brew_coffee_cloth", "make_hosaf", "make_pekmez", "make_pekmez_grape"}


def consumed(i):
    ids = set()
    if i.get("consume"):
        ids.add(i["consume"])
    for m in i.get("materials", []) or []:
        ids.add(m["item"])
    return ids


def method_for(key, i):
    if key in NO_COOK:
        return None
    if key in STEW:
        return "stew"
    if key in FRY or key.startswith("cook_fish_"):
        return "fry"
    if key in GRILL:
        return "grill"
    if key in EMBER:
        return "ember"
    if key in RAW:
        return "raw"
    return "boil"


def tools_for(key, method, ins):
    tools = []
    if method in ("bake",):
        if key in PIN:
            tools.append("tool:pin")
        if "item.firewood" in ins:
            tools.append("tool:peel")
        if key in TRAY:
            tools.append("vessel:bake")
        return tools
    if ins & CHOP or key.startswith("cook_fish_") or key in {"make_soup_fish", "make_wrap_fish"}:
        tools.append("tool:knife")
    if key in STRAIN:
        tools.append("tool:colander")
    return tools


def is_cook(i):
    return i.get("skill") in ("Cooking", "Brewing") and i["id"] not in NO_COOK


def main():
    data = json.loads(ITEMS.read_text(encoding="utf-8"))
    items = data["items"]
    by = {it["id"]: it for it in items}
    added_ids = {d["id"] for d, _ in NEW} | {d["id"] for d, _ in FLOOR}

    # 1. Yeni eşyalar (yeniden çalıştırmada baştan yazılır, kazanın hemen arkasına).
    items[:] = [it for it in items if it["id"] not in added_ids]
    at = next(k for k, it in enumerate(items) if it["id"] == POT) + 1
    for d, _ in NEW + FLOOR:
        items.insert(at, d)
        at += 1
    by = {it["id"]: it for it in items}

    # 2. Roller.
    for iid, role in ROLES.items():
        by[iid].update(role)
    for iid, act in WASH_ACTS.items():
        acts = by[iid].setdefault("interactions", [])
        acts[:] = [a for a in acts if a["id"] != act["id"]] + [dict(act)]

    # 3. Pişirme kazandan ocaklara (kazanda yalnızca yeme kalır).
    pot = by[POT]
    keep, move = [], []
    for i in pot["interactions"]:
        (move if is_cook(i) else keep).append(i)
    if move:
        pot["interactions"] = keep
        for h in HEARTHS:
            acts = by[h].setdefault("interactions", [])
            ids = {a["id"] for a in move}
            acts[:] = [a for a in acts if a["id"] not in ids] + [json.loads(json.dumps(a)) for a in move]
    copper = by[COPPER_SET]
    copper["interactions"] = [i for i in copper.get("interactions", []) if not is_cook(i)]

    # 4. Yöntem ve gerekenler: ocaklardaki ve fırındaki pişirme işleri.
    for h in HEARTHS:
        for i in by[h]["interactions"]:
            if not is_cook(i):
                continue
            m = method_for(i["id"], i)
            i.pop("cook", None); i.pop("needTools", None)
            if m:
                i["cook"] = m
                t = tools_for(i["id"], m, consumed(i))
                if t:
                    i["needTools"] = t
    for i in by[OVEN]["interactions"]:
        if i.get("skill") not in ("Cooking", "Baking"):
            continue
        i["cook"] = "bake"
        i.pop("needTools", None)
        t = tools_for(i["id"], "bake", consumed(i))
        if t:
            i["needTools"] = t

    # 5. Tarifler: kazan tarifleri taş ocağa (ve şömineye) taşınır; yeni alet tarifleri.
    recipes = data["recipes"]
    recipes[:] = [r for r in recipes if not r["id"].startswith("kit_") and not r["id"].endswith("__fire")]
    extra = []
    for r in recipes:
        if r["station"] in (POT, HEARTHS[0]):
            r["station"] = HEARTHS[0]
            if r.get("skill") in ("Cooking", "Brewing") or r["id"] in ("render_tallow", "fish_oil"):
                m = method_for(r["id"], r)
                r["cook"] = m
                r.pop("needTools", None)
                t = tools_for(r["id"], m, consumed(r))
                if t:
                    r["needTools"] = t
            twin = json.loads(json.dumps(r))
            twin["id"] = r["id"] + "__fire"
            twin["station"] = HEARTHS[1]
            extra.append(twin)
    recipes.extend(extra)
    for rid, st, prod, mats, skill, lvl, dur, xp, tr, _en in RECIPES:
        r = {"id": rid, "station": st, "produces": prod, "materials": [{"item": a, "count": b} for a, b in mats],
             "durationDays": dur, "skill": skill, "skillXp": xp, "displayName": tr}
        if lvl:
            r["minLevel"] = lvl
        recipes.append(r)

    ITEMS.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    # 6. İngilizce metinler.
    en = json.loads(EN.read_text(encoding="utf-8"))
    for d, name in NEW + FLOOR:
        en[d["id"] + ".name"] = name
    for iid, acts in NEW_EN_ACTS.items():
        for aid, text in acts.items():
            en[f"{iid}.act.{aid}"] = text
    for iid, text in WASH_EN.items():
        en[f"{iid}.act.wash_dishes"] = text
    for iid, name in ROLE_EN.items():
        en[iid + ".name"] = name
    # Taşınan etkileşimlerin metni (ad ve ruh hali metni) ocaklara kopyalanır, kazandan silinir.
    def act_of(key, prefix):
        rest = key[len(prefix):]
        return rest.split(".", 1)[0]

    # Yeniden çalıştırmada taşıma çoktan yapılmış olabilir: ocaktaki bütün pişirme işleri sayılır.
    moved_ids = {i["id"] for i in move} | {i["id"] for i in by[HEARTHS[0]]["interactions"] if is_cook(i)}
    for k in list(en):
        if k.startswith(POT + ".act.") and act_of(k, POT + ".act.") in moved_ids:
            for h in HEARTHS:
                en[h + k[len(POT):]] = en[k]
    for k in [k for k in en if k.startswith(POT + ".act.")]:
        if act_of(k, POT + ".act.") not in {a["id"] for a in pot["interactions"]}:
            del en[k]
    for k in [k for k in en if k.startswith(COPPER_SET + ".act.")]:
        if act_of(k, COPPER_SET + ".act.") not in {a["id"] for a in copper["interactions"]}:
            del en[k]
    recipe_en = {r[0]: r[9] for r in RECIPES}
    for k in [k for k in en if k.startswith("recipe.kit_")]:
        del en[k]
    for rid, text in recipe_en.items():
        en[f"recipe.{rid}.name"] = text
    for r in recipes:
        if r["id"].endswith("__fire"):
            base = f"recipe.{r['id'][:-6]}.name"
            if base in en:
                en[f"recipe.{r['id']}.name"] = en[base]
    EN.write_text(json.dumps(dict(sorted(en.items())), ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"{len(NEW) + len(FLOOR)} yeni eşya, {len(move)} pişirme işi ocaklara, {len(RECIPES)} tarif, {len(extra)} ocak tarifi ikizi.")


if __name__ == "__main__":
    main()
