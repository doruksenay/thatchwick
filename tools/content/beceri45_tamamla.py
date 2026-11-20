#!/usr/bin/env python3
"""
Beceri tamamlama (Project belgesi 45, 9. bölüm): skillUnlocks tablosundaki planlanmış satırların içerik tarafı.
Yeni eşyalar, tarifler, beceri işleri (skillWork), kilit taşıma (seviye ve lockSkill) ve tablo bayrakları.
Bir kez çalıştırılır; ikinci çalıştırmada var olan kimlikleri atlar (idempotent).

    python3 tools/content/beceri45_tamamla.py
"""
import json, os, sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
ITEMS = os.path.join(ROOT, "Assets/Resources/Content/items.json")
LIFE = os.path.join(ROOT, "Assets/Resources/Content/life.json")

d = json.load(open(ITEMS, encoding="utf-8"))
life = json.load(open(LIFE, encoding="utf-8"))
ids = {i["id"]: i for i in d["items"]}
recipes = {r["id"]: r for r in d["recipes"]}
parts = {p["id"]: p for p in d["buildParts"]}
NEW_ITEMS, NEW_PLACEABLES = [], []
NEW_XP = []   # eklenen tarif ve etkileşimler: XP/gün oranı mevcut en iyi kaynağı aşmasın (SkillTempo, belge 45 5.2)


def best_ratio():
    best = {}
    for it in d["items"]:
        for x in it.get("interactions", []):
            if x.get("skill") and x.get("durationDays", 0) > 0 and x.get("skillXp", 0) > 0:
                best[x["skill"]] = max(best.get(x["skill"], 0), x["skillXp"] / x["durationDays"])
    for r in d["recipes"]:
        sk = r.get("skill", "Carpentry")
        if r.get("durationDays", 0) > 0 and r.get("skillXp", 0) > 0:
            best[sk] = max(best.get(sk, 0), r["skillXp"] / r["durationDays"])
    return best


BEST = best_ratio()
# Doğrudan kaynağı olmayan becerilerin dolaylı hızı (SkillTempo.Indirect) üstüne çıkılmasın.
BEST.update({k: v for k, v in {"Carting": 16, "Salvage": 22, "Stewardship": 18, "Decorating": 36}.items() if k not in BEST})


def res(iid, name, price, tags=None, **kw):
    """Kaynak (envanter) eşyası."""
    if iid in ids:
        return ids[iid]
    it = {"id": iid, "displayName": name, "placement": "Resource", "category": "resource", "price": price}
    if tags:
        it["tags"] = tags
    it.update(kw)
    d["items"].append(it); ids[iid] = it; NEW_ITEMS.append(iid)
    return it


def placeable(iid, name, price, category, level, lock=None, **kw):
    if iid in ids:
        return ids[iid]
    it = {"id": iid, "displayName": name, "category": category, "price": price, "level": level}
    if lock:
        it["lockSkill"] = lock
    it.update(kw)
    NEW_XP.extend(it.get("interactions", []))
    d["items"].append(it); ids[iid] = it; NEW_PLACEABLES.append(iid)
    return it


def recipe(rid, station, produces, mats, skill, minlvl, name, count=1, dur=0.12, xp=None):
    assert station in ids, station
    assert produces in ids, produces
    for m in mats:
        assert m[0] in ids, (rid, m[0])
    r = recipes.get(rid)
    if r is None:
        r = {"id": rid, "station": station, "produces": produces, "materials": [{"item": m, "count": c} for m, c in mats],
             "durationDays": dur, "skill": skill, "skillXp": xp if xp is not None else 8 + 2 * minlvl, "displayName": name}
        if count > 1:
            r["count"] = count
        d["recipes"].append(r); recipes[rid] = r; NEW_XP.append(r)
    if minlvl > 1:
        r["minLevel"] = minlvl
    return r


def inter(iid, idef):
    it = ids[iid]
    lst = it.setdefault("interactions", [])
    for x in lst:
        if x["id"] == idef["id"]:
            return x
    lst.append(idef)
    NEW_XP.append(idef)
    return idef


def lock(iid, level, skill=None):
    it = ids[iid]
    it["level"] = level
    if skill:
        it["lockSkill"] = skill


def min_level(rid, level):
    recipes[rid]["minLevel"] = level


def set_inter_min(iid, inter_id, level):
    for x in ids[iid].get("interactions", []):
        if x["id"] == inter_id:
            x["minLevel"] = level
            return
    raise KeyError((iid, inter_id))


def W(iid, wid, name, skill, lvl, dur=0.04, xp=4, **kw):
    x = {"id": wid, "displayName": name, "durationDays": dur, "skill": skill, "skillXp": xp, "skillWork": kw.pop("work", wid)}
    if lvl > 1:
        x["minLevel"] = lvl
    x.update(kw)
    return inter(iid, x)


crops = [k for k, i in ids.items() if k.startswith("item.crop_")]
fruit_trees = [k for k, i in ids.items() if i.get("category") == "plant" and i.get("fruitSeasons") and k != "item.bush_coffee"]
forest_trees = ["item.tree_oak", "item.tree_birch", "item.tree_pine", "item.tree_willow"]
forage = [k for k, i in ids.items() if i.get("category") == "forage" and k != "item.forage_sand"]

# ================= Çiftçilik =================
for s in ["item.seed_onion", "item.seed_carrot", "item.seed_pea", "item.seed_broad_bean"]:
    if s in ids: lock(s, 2)
for s in ["item.seed_wheat", "item.seed_rye", "item.seed_barley", "item.seed_flax", "item.seed_oats"]:
    if s in ids: lock(s, 3)
for c in crops:
    harvest = next((x for x in ids[c].get("interactions", []) if x["id"] == "harvest"), None)
    W(c, "inspect_field", "Tarlayı incele", "Farming", 2)
    if harvest:
        main = harvest["produce"][0]
        seeds = [p for p in harvest["produce"] if p["item"].startswith("item.seed_")]
        if seeds:
            inter(c, {"id": "select_seed", "displayName": "Seçme tohumla hasat et (en iyi başaktan)", "durationDays": 0.07,
                      "skill": "Farming", "skillXp": 16, "minLevel": 4, "requiresGrown": True, "removeItem": True, "tool": "hoe",
                      "produce": [{"item": main["item"], "count": max(1, main.get("count", 1) - 1)}, {"item": seeds[0]["item"], "count": 3}]})
    W(c, "bulk_sow", "Toplu ek (çevredeki 3×3 tarla)", "Farming", 5, dur=0.1, xp=6)
    W(c, "smell_soil", "Toprağı kokla (hasat tahmini)", "Farming", 6)
    W(c, "imece", "İmece çağır", "Farming", 9, dur=0.05, xp=10)
    W(c, "soil_check", "Toprağa bak", "SoilCare", 2, dur=0.03, xp=3)
    if any(x["id"] == "manure" for x in ids[c].get("interactions", [])):
        set_inter_min(c, "manure", 3)

# ================= Bahçıvanlık =================
res("item.dill", "Dereotu", 3, ["herb"])
res("item.fenugreek", "Çemen", 3, ["herb", "spice"])
res("item.lavender", "Lavanta", 4, ["herb"])
placeable("item.crop_herb_bed", "Mutfak otu yatağı", 0, "plant", 1, growDays=6, tilled=True, diesInWinter=True,
          growSeasons=["Spring", "Summer"], cropFamily="ot", site="outdoor",
          interactions=[{"id": "harvest", "displayName": "Ot kes", "durationDays": 0.05, "skill": "Horticulture", "skillXp": 12, "requiresGrown": True,
                         "removeItem": True, "produce": [{"item": "item.herb", "count": 2}, {"item": "item.dill", "count": 1}, {"item": "item.fenugreek", "count": 1},
                                                         {"item": "item.lavender", "count": 1}, {"item": "item.seed_herb_bed", "count": 1}]},
                        {"id": "water", "displayName": "Sula", "durationDays": 0.02, "skill": "Horticulture", "skillXp": 2, "water": True, "tool": "can"}])
res("item.seed_herb_bed", "Mutfak otu tohumu", 4, plantsAs="item.crop_herb_bed", level=2, lockSkill="Horticulture")
placeable("item.crop_saffron", "Safran çiğdemi", 0, "plant", 1, growDays=10, tilled=True, diesInWinter=True, growSeasons=["Autumn"], cropFamily="cigdem", site="outdoor",
          interactions=[{"id": "harvest", "displayName": "Safran topla (sabah)", "durationDays": 0.08, "skill": "Horticulture", "skillXp": 20, "requiresGrown": True,
                         "removeItem": True, "produce": [{"item": "item.saffron", "count": 1}, {"item": "item.saffron_corm", "count": 1}, {"item": "item.saffron_corm", "count": 1, "chance": 0.5}]},
                        {"id": "water", "displayName": "Sula", "durationDays": 0.02, "skill": "Horticulture", "skillXp": 2, "water": True, "tool": "can"}])
res("item.saffron_corm", "Safran soğanı", 15, plantsAs="item.crop_saffron", level=4, lockSkill="Horticulture")
lock("item.sapling_grape", 4, "Horticulture")
lock("item.sapling_coffee", 7, "Horticulture")
lock("item.sapling_olive", 7, "Horticulture")
for t in fruit_trees:
    W(t, "prune_tree", "Buda (meyve çabuk olgunlaşır)", "Horticulture", 3, dur=0.06, xp=8, work="prune", requiresGrown=True)
    W(t, "graft", "Aşıla (envanterdeki fidanla)", "Horticulture", 5, dur=0.08, xp=15, requiresGrown=True)
    W(t, "heritage", "Yadigâr ağaç ilan et", "Horticulture", 9, dur=0.05, xp=10, requiresGrown=True)
placeable("item.flower_bed", "Çiçek tarhı", 20, "garden", 6, "Horticulture", width=2, height=1, styleSet="Köylü", rooms=["outdoor"], beauty=5,
          site="outdoor", belongs=["outdoor"],
          interactions=[{"id": "stroll", "displayName": "Bahçede gezin", "need": "Fun", "restore": 20, "durationDays": 0.06, "skill": "Horticulture",
                         "skillXp": 4, "skillWork": "stroll", "minLevel": 6}])

SEEDM = "item.seed_maker"
sm = next(r for r in d["recipes"] if r["station"] == SEEDM)
recipe("seed_herb_bed", SEEDM, "item.seed_herb_bed", [("item.herb", 2)], sm.get("skill", "Farming"), 2, "Ot tohumu ayır", count=2, dur=sm["durationDays"])
recipe("seed_saffron", SEEDM, "item.saffron_corm", [("item.saffron", 1)], "Horticulture", 4, "Safran soğanı ayır", dur=sm["durationDays"])

# ================= Toprak bakımı =================
res("item.bone", "Kemik", 1, ["raw"])
res("item.bone_meal", "Kemik unu", 4, ["fertilizer"])
recipe("bone_meal", "item.mortar_pestle", "item.bone_meal", [("item.bone", 3)], "SoilCare", 4, "Kemik unu döv", count=2)
res("item.coffee_spent", "Kahve telvesi", 0, ["fertilizer"])
placeable("item.compost_heap", "Kompost yığını", 15, "farm", 6, "SoilCare", styleSet="Köylü", rooms=["outdoor"], site="outdoor", belongs=["outdoor"],
          interactions=[{"id": "compost_pomace", "displayName": "Pirinayı komposta göm (3)", "durationDays": 0.05, "skill": "SoilCare", "skillXp": 6, "minLevel": 6,
                         "materials": [{"item": "item.pomace", "count": 3}], "produce": [{"item": "item.compost", "count": 2}]},
                        {"id": "compost_grounds", "displayName": "Telveyi komposta göm (3)", "durationDays": 0.04, "skill": "SoilCare", "skillXp": 4, "minLevel": 6,
                         "materials": [{"item": "item.coffee_spent", "count": 3}], "produce": [{"item": "item.compost", "count": 1}]}])
for cz in ["cezve_brew", "cezve_brew_fine"]:
    x = next((i for i in ids["item.cezve"]["interactions"] if i["id"] == cz), None)
    if x is not None:
        x.setdefault("produce", [])
        if not any(p["item"] == "item.coffee_spent" for p in x["produce"]):
            x["produce"].append({"item": "item.coffee_spent", "count": 1})
placeable("item.crop_vetch", "Fiğ (yeşil gübre)", 0, "plant", 1, growDays=5, tilled=True, diesInWinter=True, growSeasons=["Spring", "Summer", "Autumn"],
          cropFamily="fig", site="outdoor",
          interactions=[{"id": "green_manure", "displayName": "Toprağa göm (verim +)", "durationDays": 0.05, "skill": "SoilCare", "skillXp": 12, "requiresGrown": True,
                         "removeItem": True, "skillWork": "green_manure", "produce": [{"item": "item.seed_vetch", "count": 1}]},
                        {"id": "water", "displayName": "Sula", "durationDays": 0.02, "skill": "SoilCare", "skillXp": 2, "water": True, "tool": "can"},
                        {"id": "harvest", "displayName": "Ot olarak biç", "durationDays": 0.05, "skill": "SoilCare", "skillXp": 6, "requiresGrown": True,
                         "removeItem": True, "produce": [{"item": "item.feed", "count": 2}, {"item": "item.seed_vetch", "count": 1}]}])
res("item.seed_vetch", "Fiğ tohumu", 2, plantsAs="item.crop_vetch", level=7, lockSkill="SoilCare")
res("item.fish_meal", "Balık unu", 4, ["fertilizer"])
for c in crops + ["item.crop_herb_bed", "item.crop_saffron", "item.crop_vetch"]:
    inter(c, {"id": "bone_meal", "displayName": "Kemik unu ser", "durationDays": 0.03, "consume": "item.bone_meal", "fertilize": 0.5, "skill": "SoilCare", "skillXp": 4, "minLevel": 4})
    inter(c, {"id": "fish_meal", "displayName": "Balık unu ser", "durationDays": 0.03, "consume": "item.fish_meal", "fertilize": 0.4, "skill": "SoilCare", "skillXp": 3})
    inter(c, {"id": "lime", "displayName": "Kireçle (sönmüş kireç)", "durationDays": 0.04, "consume": "item.quicklime", "fertilize": 0.35, "skill": "SoilCare", "skillXp": 6, "minLevel": 9})
    W(c, "black_earth", "Kara toprak hazırla (3×3 kalıcı)", "SoilCare", 10, dur=0.2, xp=30, materials=[{"item": "item.compost", "count": 4}, {"item": "item.manure", "count": 4}])

# ================= Toplayıcılık =================
for f in ["item.forage_linden", "item.forage_chamomile", "item.forage_sage", "item.forage_mint", "item.forage_rosehip"]:
    if f in ids: set_inter_min(f, "gather", 3)
for x in ids["item.forage_hive"].get("interactions", []):
    x["minLevel"] = 4
set_inter_min("item.forage_pinecone", "gather", 6)
res("item.thyme", "Kekik", 3, ["herb", "spice"])
g = next(x for x in ids["item.forage_herbs"]["interactions"] if x["id"] == "gather")
if not any(p["item"] == "item.thyme" for p in g["produce"]):
    g["produce"].append({"item": "item.thyme", "count": 1, "minLevel": 6})
res("item.truffle", "Yer mantarı", 30, ["food"], spoilDays=6)
placeable("item.forage_truffle", "Yer mantarı izi", 0, "forage", 1, forageSeasons=["Autumn"], forageMax=1, site="outdoor",
          interactions=[{"id": "gather", "displayName": "Kazıp çıkar", "durationDays": 0.08, "skill": "Foraging", "skillXp": 20, "minLevel": 8,
                         "removeItem": True, "produce": [{"item": "item.truffle", "count": 1}]}])
forage.append("item.forage_truffle")
for f in forage:
    W(f, "track", "İz sür (nadir düğüm)", "Foraging", 7, dur=0.05, xp=6)
    W(f, "forage_regrow", "Ne zaman yeniden çıkar?", "Foraging", 9, dur=0.02, xp=2)

# ================= Odunculuk =================
res("item.tar", "Katran", 6, ["craft"])
res("item.fine_lumber", "Kaliteli kereste", 12, ["craft"])
for t in forest_trees:
    W(t, "branch", "Dal buda (kuru dal)", "Woodcutting", 2, dur=0.05, xp=5, requiresGrown=True, tool="axe", produce=[{"item": "item.firewood", "count": 1}])
W("item.tree_pine", "resin", "Reçine topla", "Woodcutting", 6, dur=0.06, xp=8, requiresGrown=True, produce=[{"item": "item.resin", "count": 1}])
recipe("tar", "item.kiln", "item.tar", [("item.log", 2), ("item.resin", 2)], "Woodcutting", 6, "Katran kaynat", count=2, dur=0.2)
recipe("fine_lumber", "item.workbench", "item.fine_lumber", [("item.log", 2)], "Woodcutting", 8, "Kaliteli kereste biç", dur=0.15)

# ================= Hava bilgisi =================
W("item.weather_vane", "sky_look", "Gökyüzüne bak", "WeatherLore", 2, dur=0.02, xp=4)
res("item.folk_almanac", "Halk takvimi", 40, ["book"])
recipe("folk_almanac", "item.writing_lectern", "item.folk_almanac", [("item.parchment", 2), ("item.ink", 1)], "WeatherLore", 10, "Halk takvimi yaz", dur=0.3)
placeable("item.storm_bell", "Fırtına çanı", 45, "farm", 8, "WeatherLore", styleSet="Zanaatkâr", rooms=["outdoor"], site="outdoor", belongs=["outdoor"])
recipe("storm_bell", "item.forge", "item.storm_bell", [("item.iron_bar", 3), ("item.plank", 2)], "WeatherLore", 8, "Fırtına çanı dök", dur=0.25)

# ================= Hayvancılık =================
lock("item.churn", 3, "AnimalHusbandry")
lock("item.beehive", 4, "AnimalHusbandry")
set_inter_min("item.beehive", "cut_comb", 4)
res("item.aged_cheese", "Olgun peynir", 18, ["food"], spoilDays=80)
placeable("item.cheese_rack", "Peynir olgunlaştırma rafı", 30, "kitchen", 6, "AnimalHusbandry", styleSet="Köylü", rooms=["kitchen", "cellar"],
          autoProcess={"item": "item.cheese", "count": 2, "produces": "item.aged_cheese", "producesCount": 1}, tags=["passive"], site="indoor", belongs=["kitchen", "cellar"])
placeable("item.brood_basket", "Kuluçka sepeti", 12, "barn", 6, "AnimalHusbandry", styleSet="Köylü", rooms=["outdoor", "barn"], site="any", belongs=["outdoor", "barn"])
res("item.mulberry_leaf", "Dut yaprağı", 1, ["raw"])
res("item.cocoon", "İpek kozası", 6, ["raw"])
res("item.silk_thread", "İpek ipliği", 15, ["craft"])
res("item.silk", "İpek kumaş", 45, ["cloth"])
placeable("item.silkworm_rack", "İpek böceği rafı", 40, "barn", 8, "AnimalHusbandry", styleSet="Zanaatkâr", rooms=["barn", "workshop"],
          autoProcess={"item": "item.mulberry_leaf", "count": 4, "produces": "item.cocoon", "producesCount": 1}, tags=["passive"], site="indoor", belongs=["barn", "workshop"])

# ================= Avcılık =================
W("item.hunting_blind", "hunt_track", "İz oku", "Hunting", 2, dur=0.04, xp=5)
W("item.hunting_blind", "watch_wild", "Gözle (Doğa Defteri)", "Hunting", 9, dur=0.15, xp=12)

# ================= Balıkçılık =================
res("item.bait", "Balık yemi", 1)
recipe("bait", "item.cooking_pot", "item.bait", [("item.bread", 1)], "Fishing", 2, "Yem yoğur", count=4, dur=0.04)
lock("item.fish_trap", 3, "Fishing")
res("item.fishing_net", "Büyük ağ", 25, ["tool"])
recipe("fishing_net", "item.loom", "item.fishing_net", [("item.yarn", 4)], "Fishing", 4, "Ağ ör", dur=0.2)
placeable("item.crayfish_trap", "Kerevit tuzağı", 20, "farm", 5, "Fishing", styleSet="Köylü", rooms=["outdoor"], fishTrap=True, site="outdoor", belongs=["outdoor"])
res("item.fish_oil", "Balık yağı", 6, ["fuel", "light"])
recipe("fish_oil", "item.cooking_pot", "item.fish_oil", [("item.fish_carp", 2)], "Fishing", 6, "Balık yağı kaynat", dur=0.12)
inter("item.pond", {"id": "ice_fish", "displayName": "Buz deliği aç, balık tut", "durationDays": 0.15, "skill": "Fishing", "skillXp": 14, "minLevel": 7,
                    "requiresFrost": True, "tool": "rod", "produce": [{"item": "item.fish_pike", "count": 1, "chance": 0.5}, {"item": "item.fish_trout", "count": 1, "chance": 0.6}]})
W("item.pond", "stock_pond", "Göleti stokla (2 sazan)", "Fishing", 8, dur=0.08, xp=12, materials=[{"item": "item.fish_carp", "count": 2}])
recipe("seed_vetch", SEEDM, "item.seed_vetch", [("item.feed", 2)], "SoilCare", 7, "Fiğ tohumu ayır", count=2, dur=0.05)
recipe("fish_meal", "item.mortar_pestle", "item.fish_meal", [("item.fish_carp", 2)], "Fishing", 9, "Balık unu döv", count=3, dur=0.1)
res("item.legend_fish", "Göllerin efsane balığı", 120, ["food", "trophy"])
inter("item.pond", {"id": "swim", "displayName": "Yüz", "need": "Fun", "restore": 25, "durationDays": 0.08, "skill": "Athletics", "skillXp": 8, "minLevel": 6,
                    "wets": 1.0, "extraRestore": [{"need": "Hygiene", "amount": 15}]})

# ================= Marangozluk =================
for rid in ["bed_frame", "box_bed", "wall_shelf", "display_shelf"]:
    if rid in recipes: recipes[rid]["minLevel"] = max(recipes[rid].get("minLevel", 1), 3 if "bed" in rid else 2)
set_inter_min("item.workbench", "repair_wood", 3)
set_inter_min("item.forge", "repair_metal", 3)
for p in parts.values():
    if p.get("kind") in ("door", "window") and p.get("level") in (2, 3):
        p["level"] = 4
for it in d["items"]:
    for u in it.get("upgrades", []):
        if u["id"] == "carving":
            u["minLevel"] = 6
res("item.cart_wheel", "Araba tekerleği", 20, ["craft"])
recipe("cart_wheel", "item.workbench", "item.cart_wheel", [("item.plank", 3), ("item.iron_bar", 1)], "Carpentry", 7, "Araba tekerleği yap", dur=0.2)
# Çatkı (5) ve Yörük (6) takımları: üçer parça (sanat 455d işinde).
CAT_TABLE, CAT_CHAIR, CAT_CHEST = ids["item.table_wood"]["category"], ids["item.chair_wood"]["category"], ids["item.chest_wood"]["category"]
SETS = [("catki", "Çatkı", 5, [("table", "masası", 4, 3), ("chair", "sandalyesi", 3, 2), ("chest", "sandığı", 4, 2)]),
        ("yoruk", "Yörük", 6, [("table", "sofrası", 3, 2), ("chair", "minderi", 0, 3), ("chest", "yüklüğü", 5, 3)])]
for key, style, lvl, pieces in SETS:
    for kind, noun, planks, extra in pieces:
        iid = f"item.{key}_{kind}"
        cat = {"table": CAT_TABLE, "chair": CAT_CHAIR, "chest": CAT_CHEST}[kind]
        mats = [("item.plank", planks)] if planks else []
        mats += [("item.felt", 2)] if key == "yoruk" and kind == "chair" else [("item.nails", 1)]
        extra_kw = {"comfort": 3} if kind == "chair" else {"storage": 20} if kind == "chest" else {}
        placeable(iid, f"{style} {noun}", 0, cat, lvl, "Carpentry", styleSet=style, rooms=["living", "bedroom"], finishGroup="wood" if key == "catki" else "cloth",
                  beauty=2 + extra, site="indoor", belongs=["living", "bedroom"], width=2 if kind == "table" else 1, height=1, **extra_kw)
        recipe(f"{key}_{kind}", "item.workbench", iid, mats, "Carpentry", lvl, f"{style} {noun} yap", dur=0.15)
# Tüccar (8) ve Asil (9) kademeleri: Marangozluk kilitli mobilya.
CARP_CATS = {CAT_TABLE, CAT_CHAIR, CAT_CHEST, "sleep", "hygiene", "seating", "surface", "storage"}
moved = 0
for it in d["items"]:
    if it.get("placement") == "Resource" or it.get("lockSkill"):
        continue
    if it.get("category") in CARP_CATS and it.get("styleSet") in ("Tüccar", "Asil"):
        made_by = next((r for r in d["recipes"] if r["produces"] == it["id"]), None)
        if made_by and made_by.get("skill", "Carpentry") != "Carpentry":
            continue
        it["level"] = max(int(it.get("level", 1)), 8 if it["styleSet"] == "Tüccar" else 9)
        moved += 1

# ================= Demircilik =================
res("item.hinge", "Menteşe", 4, ["craft"])
recipe("hinge", "item.forge", "item.hinge", [("item.iron_bar", 1)], "Smithing", 2, "Menteşe döv", count=3, dur=0.08)
if "cezve" in recipes: min_level("cezve", 4)
lock("item.copper_pots", 4, "Smithing")
lock("item.bloomery", 4, "Smithing")
res("item.tin_brooch", "Cam taşlı kalay broş", 60, ["jewelry"])
recipe("tin_brooch", "item.forge", "item.tin_brooch", [("item.tin", 1), ("item.glass", 1)], "Smithing", 9, "Taşlı takı işle", dur=0.2)

# ================= Dokumacılık =================
lock("item.dye_vat", 4, "Weaving")
if "felt" in recipes: min_level("felt", 4)
res("item.tunic", "Yün cepken", 20, ["clothing"])
recipe("tunic", "item.loom", "item.tunic", [("item.cloth", 2), ("item.yarn", 1)], "Weaving", 5, "Cepken dik", dur=0.15)
res("item.linen_shirt", "Keten gömlek", 25, ["clothing"])
recipe("linen_shirt", "item.loom", "item.linen_shirt", [("item.linen", 2), ("item.linen_yarn", 1)], "Weaving", 6, "Keten gömlek dik", dur=0.15)
recipe("silk_thread", "item.spinning_wheel", "item.silk_thread", [("item.cocoon", 2)], "Weaving", 7, "İpek çek", dur=0.12)
recipe("silk", "item.loom", "item.silk", [("item.silk_thread", 2)], "Weaving", 7, "İpek dokuma", dur=0.25)
res("item.quilt", "Yorgan", 40, ["bedding"])
recipe("quilt", "item.loom", "item.quilt", [("item.cloth", 3), ("item.wool", 4)], "Weaving", 9, "Yorgan dik", dur=0.25)
res("item.dowry_linen", "İşlemeli çeyiz örtüsü", 50, ["dowry"])
recipe("dowry_linen", "item.loom", "item.dowry_linen", [("item.linen", 3), ("item.dye_red", 1), ("item.linen_yarn", 2)], "Weaving", 9, "Çeyiz örtüsü işle", dur=0.3)

# ================= Saraçlık =================
res("item.tulum", "Tulum (keçi derisi)", 15, ["craft"])
recipe("tulum", "item.saddler_bench", "item.tulum", [("item.leather", 2)], "Leatherwork", 5, "Tulum dik", dur=0.15)
res("item.tulum_cheese", "Tulum peyniri", 22, ["food"], spoilDays=90)
inter("item.cheese_press", {"id": "make_tulum_cheese", "displayName": "Tulum peyniri bas (2 peynir, 1 tulum)", "durationDays": 0.2, "skill": "Cooking", "skillXp": 15,
                            "minLevel": 3, "materials": [{"item": "item.cheese", "count": 2}, {"item": "item.tulum", "count": 1}], "produce": [{"item": "item.tulum_cheese", "count": 3}]})
if "fur" in recipes: min_level("fur", 6)
res("item.boots", "Çizme", 30, ["clothing"])
recipe("boots", "item.saddler_bench", "item.boots", [("item.leather", 2), ("item.nails", 1)], "Leatherwork", 6, "Çizme dik", dur=0.15)
res("item.leather_vest", "Deri yelek", 30, ["clothing"])
recipe("leather_vest", "item.saddler_bench", "item.leather_vest", [("item.leather", 3)], "Leatherwork", 7, "Deri yelek dik", dur=0.15)
res("item.harness", "Koşum takımı", 35, ["craft"])
recipe("harness", "item.saddler_bench", "item.harness", [("item.leather", 3), ("item.iron_bar", 1)], "Leatherwork", 8, "Koşum takımı dik", dur=0.2)
res("item.race_saddle", "Yarış eyeri", 90, ["saddle"])
recipe("race_saddle", "item.saddler_bench", "item.race_saddle", [("item.leather", 3), ("item.fur", 1), ("item.fine_lumber", 1)], "Leatherwork", 9, "Yarış eyeri dik", dur=0.25)

# ================= Çömlekçilik =================
lock("item.flower_pot", 3, "Pottery")
if "marbles" in recipes: min_level("marbles", 3)
if "brick" in recipes: min_level("brick", 4)
if "roof_tiles" in recipes: min_level("roof_tiles", 4)
res("item.glass_pane", "Pencere camı", 10, ["craft"])
recipe("glass_pane", "item.glass_bench", "item.glass_pane", [("item.glass", 1)], "Pottery", 7, "Pencere camı kes", count=2, dur=0.1)
res("item.glazed_tile", "Sırlı çini", 8, ["craft"])
recipe("glazed_tile", "item.kiln", "item.glazed_tile", [("item.clay", 2), ("item.dye_blue", 1), ("item.firewood", 1)], "Pottery", 9, "Çini sırla", count=4, dur=0.25)

# ================= Mumculuk =================
recipe("torch", "item.chandler_table", "item.torch", [("item.tallow", 1), ("item.firewood", 1)], "Chandlery", 3, "Meşale sar", count=3, dur=0.06)
res("item.scented_candle", "Lavantalı mum", 8, ["light"])
recipe("scented_candle", "item.chandler_table", "item.scented_candle", [("item.beeswax", 2), ("item.lavender", 1), ("item.linen_yarn", 1)], "Chandlery", 6, "Kokulu mum dök", dur=0.1)
res("item.bay_leaf", "Defne yaprağı", 2, ["herb"])
res("item.bay_soap", "Defne sabunu", 10, ["soap"])
recipe("bay_soap", "item.chandler_table", "item.bay_soap", [("item.olive_oil", 1), ("item.bay_leaf", 2)], "Chandlery", 6, "Defne sabunu kaynat", dur=0.12)
res("item.mine_candle", "Uzun yanan maden mumu", 6, ["light"])
recipe("mine_candle", "item.chandler_table", "item.mine_candle", [("item.tallow", 3), ("item.yarn", 1)], "Chandlery", 7, "Maden mumu dök", dur=0.1)
res("item.festival_candle", "Şenlik mumu", 10, ["light"])
recipe("festival_candle", "item.chandler_table", "item.festival_candle", [("item.beeswax", 3), ("item.dye_red", 1), ("item.linen_yarn", 1)], "Chandlery", 8, "Şenlik mumu dök", dur=0.12)
res("item.sealing_wax", "Mühür mumu", 6, ["craft"])
recipe("sealing_wax", "item.chandler_table", "item.sealing_wax", [("item.beeswax", 1), ("item.resin", 1), ("item.dye_red", 1)], "Chandlery", 9, "Mühür mumu kaynat", count=2, dur=0.08)

# ================= Taş ustalığı =================
if "cut_stone" in recipes: min_level("cut_stone", 2)
if "floor_cobble" in parts: parts["floor_cobble"]["level"] = 2
lock("item.lime_kiln", 6, "Masonry")
for r in d["recipes"]:
    if r["produces"] == "item.mortar":
        r["minLevel"] = 6
lock("item.cistern", 7, "Masonry")
if "cistern" in recipes: min_level("cistern", 7)
res("item.marble", "Mermer", 20, ["stone"])
placeable("item.gravestone", "Yazıtlı mezar taşı", 0, "garden", 8, "Masonry", styleSet="Zanaatkâr", rooms=["outdoor"], beauty=2, site="outdoor", belongs=["outdoor"],
          interactions=[{"id": "remember", "displayName": "Anmak için dur", "need": "Fun", "restore": 5, "durationDays": 0.04}])
recipe("gravestone", "item.stonecutter", "item.gravestone", [("item.marble", 2)], "Masonry", 8, "Mezar taşı yontup yaz", dur=0.3)
for st in ["item.stairs_up_stone", "item.stairs_down_stone"]:
    lock(st, 9, "Masonry")

# ================= Madencilik, Mühendislik =================
res("item.master_lantern", "Usta madencinin feneri", 50, ["light", "tool"])
recipe("master_lantern", "item.forge", "item.master_lantern", [("item.iron_bar", 2), ("item.glass", 1), ("item.mine_candle", 1)], "Mining", 10, "Usta fenerini yap", dur=0.2)
res("item.hund_rail", "Hund rayı (maden arabası)", 30, ["tool"])
recipe("hund_rail", "item.workbench", "item.hund_rail", [("item.plank", 4), ("item.iron_bar", 1)], "Engineering", 6, "Hund rayı kur", dur=0.2)
res("item.windlass", "Bocurgat ve kova dolabı", 30, ["tool"])
recipe("windlass", "item.workbench", "item.windlass", [("item.plank", 4), ("item.iron_bar", 1), ("item.yarn", 2)], "Engineering", 7, "Bocurgat kur", dur=0.2)
lock("item.bellows", 8, "Engineering")

# ================= Geri kazanım =================
res("item.patch_quilt", "Yamalı yorgan", 25, ["bedding"])
recipe("patch_quilt", "item.loom", "item.patch_quilt", [("item.cloth", 2), ("item.yarn", 2)], "Salvage", 8, "Yamalı yorgan dik", dur=0.2)
res("item.old_keepsake", "Eski hatıra", 15, ["memento"])

# ================= Aşçılık, Fırıncılık, İçecekçilik, Aktarlık =================
W("item.cooking_pot", "feast", "Ziyafet sofrası kur (4 çeşit)", "Cooking", 5, dur=0.2, xp=20)
W("item.cooking_pot", "taste", "Tadına bak", "Cooking", 7, dur=0.02, xp=3)
res("item.herb_cheese", "Otlu peynir", 14, ["food"], spoilDays=40)
inter("item.cheese_press", {"id": "make_herb_cheese", "displayName": "Otlu peynir yap (3 süt, 1 ot)", "durationDays": 0.25, "skill": "Cooking", "skillXp": 18, "minLevel": 7,
                            "materials": [{"item": "item.milk", "count": 3}, {"item": "item.herb", "count": 1}], "produce": [{"item": "item.herb_cheese", "count": 2}]})
res("item.wedding_dish", "Düğün yemeği (keşkek)", 30, ["food"], spoilDays=3)
recipe("wedding_dish", "item.cooking_pot", "item.wedding_dish", [("item.meat", 2), ("item.flour", 2), ("item.butter", 1)], "Cooking", 9, "Keşkek döv", count=2, dur=0.3)
for iid, name, price in [("item.bazlama", "Bazlama", 3), ("item.yufka", "Yufka", 2), ("item.bread_sourdough", "Ekşi mayalı ekmek", 5), ("item.wedding_bread", "Düğün ekmeği", 12)]:
    res(iid, name, price, ["food"], spoilDays=6)
res("item.sourdough", "Ekşi maya", 1, ["craft"])
OVEN = "item.stone_oven"
inter(OVEN, {"id": "make_bazlama", "displayName": "Bazlama yap (1 un)", "durationDays": 0.08, "skill": "Baking", "skillXp": 8, "minLevel": 2,
             "materials": [{"item": "item.flour", "count": 1}], "produce": [{"item": "item.bazlama", "count": 3}]})
inter(OVEN, {"id": "make_yufka", "displayName": "Yufka aç (1 un)", "durationDays": 0.08, "skill": "Baking", "skillXp": 8, "minLevel": 2,
             "materials": [{"item": "item.flour", "count": 1}], "produce": [{"item": "item.yufka", "count": 4}]})
inter(OVEN, {"id": "start_sourdough", "displayName": "Ekşi maya başlat (1 un)", "durationDays": 0.05, "skill": "Baking", "skillXp": 6, "minLevel": 4,
             "materials": [{"item": "item.flour", "count": 1}], "produce": [{"item": "item.sourdough", "count": 1}]})
inter(OVEN, {"id": "bake_sourdough", "displayName": "Ekşi mayalı ekmek pişir", "durationDays": 0.22, "skill": "Baking", "skillXp": 16, "minLevel": 6,
             "materials": [{"item": "item.flour", "count": 2}, {"item": "item.sourdough", "count": 1}, {"item": "item.firewood", "count": 1}],
             "produce": [{"item": "item.bread_sourdough", "count": 3}, {"item": "item.sourdough", "count": 1}]})
inter(OVEN, {"id": "bake_wedding_bread", "displayName": "Düğün ekmeği pişir", "durationDays": 0.25, "skill": "Baking", "skillXp": 20, "minLevel": 7,
             "materials": [{"item": "item.flour", "count": 3}, {"item": "item.honey", "count": 1}, {"item": "item.egg", "count": 1}, {"item": "item.firewood", "count": 1}],
             "produce": [{"item": "item.wedding_bread", "count": 2}]})
inter(OVEN, {"id": "bake_village", "displayName": "Köy fırını: toplu ekmek (6 un)", "durationDays": 0.35, "skill": "Baking", "skillXp": 30, "minLevel": 9,
             "materials": [{"item": "item.flour", "count": 6}, {"item": "item.firewood", "count": 1}], "produce": [{"item": "item.bread", "count": 11}]})
for tea, nm in [("item.tea_linden", "Ihlamur"), ("item.tea_sage", "Adaçayı"), ("item.tea_chamomile", "Papatya"), ("item.tea_mint", "Nane"), ("item.tea_rosehip", "Kuşburnu")]:
    inter("item.cooking_pot", {"id": "brew_" + tea.split("_", 1)[1], "displayName": f"{nm} çayı demle ve iç", "need": "Fun", "restore": 15, "durationDays": 0.05,
                               "skill": "Brewing", "skillXp": 6, "minLevel": 2, "consume": tea, "bladder": 12})
res("item.pomegranate", "Nar", 4, ["food", "merchantonly"])
res("item.pomegranate_sherbet", "Nar şerbeti", 8, ["drink"])
recipe("pomegranate_sherbet", "item.cooking_pot", "item.pomegranate_sherbet", [("item.pomegranate", 3), ("item.honey", 1)], "Brewing", 4, "Nar şerbeti yap", count=2, dur=0.08)
for x in ids.get("item.grape_tub", {}).get("interactions", []):
    if x["id"] == "tread_grapes":
        x["minLevel"] = 6
for r in d["recipes"]:
    if r["id"] == "tread_grapes":
        r["minLevel"] = 6
res("item.aged_vinegar", "Eski sirke", 20, ["spice"])
recipe("aged_vinegar", "item.brew_barrel", "item.aged_vinegar", [("item.vinegar", 3)], "Brewing", 9, "Sirkeyi yıllandır", dur=0.5)
res("item.cough_syrup", "Öksürük şurubu", 12, ["remedy"])
recipe("cough_syrup", "item.alembic", "item.cough_syrup", [("item.honey", 1), ("item.linden", 2), ("item.herb", 1)], "Herbalism", 5, "Öksürük şurubu kaynat", count=2, dur=0.12)
res("item.insect_repellent", "Böcek kovucu", 8, ["remedy"])
recipe("insect_repellent", "item.alembic", "item.insect_repellent", [("item.herb", 2), ("item.resin", 1)], "Herbalism", 6, "Böcek kovucu damıt", count=2, dur=0.1)
res("item.sleep_syrup", "Uyku şurubu", 15, ["remedy"])
recipe("sleep_syrup", "item.alembic", "item.sleep_syrup", [("item.tea_chamomile", 2), ("item.honey", 1), ("item.lavender", 1)], "Herbalism", 9, "Uyku şurubu kaynat", dur=0.15)
res("item.saffron_remedy", "Safranlı ilaç", 30, ["remedy"])
recipe("saffron_remedy", "item.alembic", "item.saffron_remedy", [("item.saffron", 1), ("item.honey", 1), ("item.herb", 1)], "Herbalism", 9, "Safranlı ilaç hazırla", dur=0.15)
# Şuruplar içilir: ihtiyaç ve ruh hali (kaynak eşya, envanterden; içme etkileşimi istasyonda).
inter("item.alembic", {"id": "drink_cough_syrup", "displayName": "Öksürük şurubu iç", "need": "Energy", "restore": 10, "durationDays": 0.02, "consume": "item.cough_syrup", "heal": 1.0})
inter("item.alembic", {"id": "drink_sleep_syrup", "displayName": "Uyku şurubu iç", "need": "Energy", "restore": 30, "durationDays": 0.02, "consume": "item.sleep_syrup"})
inter("item.alembic", {"id": "drink_saffron_remedy", "displayName": "Safranlı ilaç iç", "need": "Fun", "restore": 20, "durationDays": 0.02, "consume": "item.saffron_remedy", "heal": 2.0})

# Yeni yiyecekler sofrada yenir (tencere başı; "Peynir ye" gibi).
for food, nm, rest in [("item.truffle", "Yer mantarı", 30), ("item.aged_cheese", "Olgun peynir", 40), ("item.tulum_cheese", "Tulum peyniri", 40),
                       ("item.herb_cheese", "Otlu peynir", 38), ("item.wedding_dish", "Keşkek", 70), ("item.bazlama", "Bazlama", 35), ("item.yufka", "Yufka", 25),
                       ("item.bread_sourdough", "Ekşi mayalı ekmek", 50), ("item.wedding_bread", "Düğün ekmeği", 55), ("item.pomegranate", "Nar", 20),
                       ("item.legend_fish", "Efsane balık", 90)]:
    inter("item.cooking_pot", {"id": "eat_" + food.split(".", 1)[1], "displayName": f"{nm} ye", "need": "Hunger", "restore": rest, "durationDays": 0.05,
                               "consume": food, "bladder": 4, "moodId": "mood.meal", "moodText": nm, "moodValue": 2 if rest >= 50 else 1, "moodDays": 1.0})
inter("item.cooking_pot", {"id": "drink_pomegranate_sherbet", "displayName": "Nar şerbeti iç", "need": "Fun", "restore": 15, "durationDays": 0.03,
                           "consume": "item.pomegranate_sherbet", "bladder": 12})

# ================= Okuryazarlık, Kâhyalık =================
LEC = "item.writing_lectern"
W(LEC, "chronicle", "Kroniği yaz", "Literacy", 6, dur=0.15, xp=14, consume="item.parchment")
W(LEC, "contract", "Sözleşme yaz (köy işi ödülü +%15)", "Literacy", 7, dur=0.1, xp=10)
recipe("manuscript", LEC, "item.book", [("item.parchment", 3), ("item.ink", 1)], "Literacy", 8, "El yazması kopyala", dur=0.4)
W(LEC, "ledger", "Hesap defterine bak", "Literacy", 9, dur=0.05, xp=6)
res("item.dynasty_chronicle", "Hanedan kroniği", 100, ["book"])
recipe("dynasty_chronicle", LEC, "item.dynasty_chronicle", [("item.parchment", 4), ("item.ink", 2), ("item.leather", 1)], "Literacy", 10, "Hanedan kroniğini yaz", dur=0.5)
W(LEC, "weekly_plan", "Haftalık plan çıkar", "Stewardship", 4, dur=0.05, xp=8)
W(LEC, "imece_steward", "İmece düzenle", "Stewardship", 7, dur=0.05, xp=12, work="imece")

# ================= Müzik =================
lock("item.lute", 3, "Music")
placeable("item.drum", "Davul", 0, "decor", 6, "Music", styleSet="Köylü", rooms=["living", "outdoor"], beauty=2, site="any", belongs=["living", "outdoor"],
          interactions=[{"id": "play_music", "displayName": "Davul çal", "need": "Fun", "restore": 30, "durationDays": 0.08, "skill": "Music", "skillXp": 10, "moodlet": "had_fun"}])
recipe("drum", "item.workbench", "item.drum", [("item.leather", 1), ("item.plank", 2)], "Music", 6, "Davul ger", dur=0.15)
for k, it in ids.items():
    if it.get("housing") and it.get("housing") not in ("hive",):
        inter(k, {"id": "play_animals", "displayName": "Hayvanlara çal", "durationDays": 0.06, "skill": "Music", "skillXp": 8, "minLevel": 8,
                  "requiresAnimal": True, "befriendAnimals": 4})

# ================= Ev düzenleme =================
for k, it in ids.items():
    if it.get("category") == "sleep" and k not in ("item.cradle",):
        W(k, "tidy", "Odayı toparla", "Decorating", 3, dur=0.05, xp=5)
        W(k, "guest_room", "Misafir odası hazırla", "Decorating", 8, dur=0.1, xp=12)
for k, it in ids.items():
    if it.get("category") == "decor_wall" and int(it.get("width", 1)) >= 2:
        it["level"] = max(int(it.get("level", 1)), 6)

# ================= Arabacılık =================
res("item.wheelbarrow", "El arabası", 20, ["tool"])
recipe("wheelbarrow", "item.workbench", "item.wheelbarrow", [("item.plank", 3), ("item.nails", 1)], "Carting", 1, "El arabası yap", dur=0.12)
res("item.horse_cart", "Hafif at arabası", 60, ["tool"])
recipe("horse_cart", "item.workbench", "item.horse_cart", [("item.plank", 6), ("item.nails", 2), ("item.iron_bar", 1)], "Carting", 3, "At arabası yap", dur=0.25)
res("item.sled", "Kış kızağı", 25, ["tool"])
recipe("sled", "item.workbench", "item.sled", [("item.plank", 4), ("item.iron_bar", 1)], "Carting", 7, "Kızak yap", dur=0.15)
res("item.covered_cart", "Üstü örtülü araba", 90, ["tool"])
recipe("covered_cart", "item.workbench", "item.covered_cart", [("item.plank", 6), ("item.cloth", 3), ("item.cart_wheel", 2)], "Carting", 9, "Örtülü araba yap", dur=0.3)

# XP tavanı: yeni iş, becerinin mevcut en iyi kaynağından hızlı XP vermesin (oran %90'ını aşmasın).
for x in NEW_XP:
    sk = x.get("skill", "Carpentry")
    cap = BEST.get(sk)
    if cap and x.get("durationDays", 0) > 0 and x.get("skillXp", 0) / x["durationDays"] > 0.9 * cap:
        x["skillXp"] = max(1, int(0.9 * cap * x["durationDays"]))
    # İçerik etkileşimleri (öbür eşyaların planlı etkileşimleri) için de aynı tavan.
for c in crops + ["item.crop_herb_bed", "item.crop_saffron", "item.crop_vetch"]:
    pass

# Satılabilir: yeni kaynakların hepsi pazarda değerli; ham madde etiketi olmayanlar zanaat ürünü.
json.dump(d, open(ITEMS, "w", encoding="utf-8"), indent=2, ensure_ascii=False)
open(ITEMS, "a", encoding="utf-8").write("\n")

# ================= skillUnlocks: uygulananları işaretle, metinleri uygulamaya uydur =================
RETEXT = {
    ("Farming", 10, "Ustalık eseri: Soy tohumu"): "Ustalık: soy tohumu (her hasatta bir tohum fazla)",
    ("SoilCare", 5, "Nadasa bırak"): "Nadas: boş toprak iki kat hızlı dinlenir",
    ("SoilCare", 8, "Verim katmanı"): "Verim katmanı (yapı modunda renkli verim haritası)",
    ("Horticulture", 8, "Aile tarifi: aile aşısı"): "Aile tarifi: aile aşısı",
    ("Carpentry", 7, "Merdiven çeşitleri"): None,
    ("Carpentry", 7, "Araba tekerleği"): "Araba tekerleği",
    ("Smithing", 9, "Taşlı takı, gümüş işleme"): "Taşlı takı (cam taşlı kalay broş)",
    ("Weaving", 5, "Giysi dik"): "Giysi dik (cepken)",
    ("Mining", 9, "Damarı dinle"): "Damarı dinle: çatırtı bir gün önceden duyulur",
    ("Salvage", 3, "Ayıkla (toplu söküm)"): "Ayıklama: söküm her malzemeden bir fazla",
    ("Salvage", 6, "Kırık aleti kurtar"): "Kırık aleti kurtar (hasarlı eşya da malzeme verir)",
    ("Salvage", 7, "Yıkık eşyadan malzeme"): "Yıkık eşyadan tam malzeme",
    ("Teaching", 4, "Aynı anda iki öğrenci"): "Aynı derste iki konu",
    ("Teaching", 8, "Aile tarifini yazmak hızlanır"): "Tarif yazılınca okuyabilen herkes öğrenir",
    ("Teaching", 10, "Ustalık: köy okulu önderliği"): "Ustalık: köy okulu (çocuklar her gün okuryazarlık)",
    ("Parenting", 8, "Çocukluk izi şansı +"): "Oyuncak alışkanlığı %25 hızlı (çocukluk izi)",
    ("Parenting", 10, "Ustalık: çocuğun ikinci özelliği iki aday arasından seç"): "Ustalık: çocuk ikinci huyunu usta ebeveynden alır",
    ("Charm", 10, "Ustalık: köy meclisinde oy"): "Ustalık: köy meclisinde söz (itibar kazancı +%10)",
    ("Engineering", 10, "Ustalık: köy değirmeni önderliği"): "Ustalık: değirmen projesi %40 ucuz",
    ("Athletics", 8, "Ağır yük taşı"): "Ağır yük: kesim, kazı, taş işi %10 hızlı",
    ("Athletics", 4, "Ağaca tırman"): "Ağaca tırman: meyve toplama %25 hızlı",
    ("Woodcutting", 4, "Yönlü devir"): "Yönlü devir: ağaç kesimi %15 hızlı",
    ("Literacy", 1, "Yazıları söker"): "Yazıları söker (kitap oku)",
    ("Hunting", 7, "Kurt avı"): "Kurt avı",
}
kept = []
for u in life["skillUnlocks"]:
    key = None
    for (sk, lv, prefix), new in RETEXT.items():
        if u["skill"] == sk and u["level"] == lv and u["text"].startswith(prefix):
            key = (sk, lv, prefix)
            if new is None:
                u = None
            else:
                u["text"] = new
            break
    if u is None:
        continue
    kept.append(u)
life["skillUnlocks"] = kept

# Kodu olmayan (hâlâ planlı) satırlar: oyun katmanı işleriyle birlikte bu listeden düşer.
STILL_PLANNED = set()
for u in life["skillUnlocks"]:
    if (u["skill"], u["level"], u["text"]) in STILL_PLANNED:
        continue
    u.pop("planned", None)


def dump_life(obj, path):
    unlocks = obj.pop("skillUnlocks")
    s = json.dumps(obj, indent=2, ensure_ascii=False)
    lines = ",\n".join("    " + json.dumps(u, ensure_ascii=False) for u in unlocks)
    s = s[:-2] + ',\n  "skillUnlocks": [\n' + lines + "\n  ]\n}\n"
    open(path, "w", encoding="utf-8").write(s)
    obj["skillUnlocks"] = unlocks


dump_life(life, LIFE)
print(f"yeni kaynak {len(NEW_ITEMS)}, yeni yerleştirilen {len(NEW_PLACEABLES)}, Tüccar/Asil taşınan {moved}")
print("RESOURCES", " ".join(NEW_ITEMS))
print("PLACEABLES", " ".join(NEW_PLACEABLES))
