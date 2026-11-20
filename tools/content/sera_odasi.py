#!/usr/bin/env python3
"""
Sera oda tipi (Project belgesi 45c ek): seraya "ait" eşyalar (tag "greenhouse"), fidan rafı ve saksı tezgâhı.
Kod: HomeRooms.cs (RoomKind.Greenhouse). Var olan kimlikleri atlar (idempotent).

    python3 tools/content/sera_odasi.py
"""
import json, os

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
ITEMS = os.path.join(ROOT, "Assets/Resources/Content/items.json")
d = json.load(open(ITEMS, encoding="utf-8"))
ids = {i["id"]: i for i in d["items"]}
recipes = {r["id"]: r for r in d["recipes"]}

# Seraya ait sayılan saksı ve bitki eşyaları (tag; belongs'a dokunulmaz: köylü evi yerleşim kuralları belongs'a bakar).
GREEN = [k for k, i in ids.items() if (k.startswith("item.pot_") or k.startswith("item.herb_pot_")) and i.get("placement") != "Resource"]
GREEN += ["item.flower_pot", "item.plant_large", "item.fern_trough", "item.ivy_wall", "item.window_flower_box",
          "item.glass_garden", "item.glass_garden_large", "item.flower_bed"]


def tag(iid, t):
    it = ids[iid]
    tags = it.setdefault("tags", [])
    if t not in tags:
        tags.append(t)


for k in GREEN:
    if k in ids:
        tag(k, "greenhouse")
# Çiçek tarhı seranın toprak zeminine de kurulur.
if "item.flower_bed" in ids:
    ids["item.flower_bed"]["site"] = "any"

NEW = []


def placeable(iid, **kw):
    if iid in ids:
        return ids[iid]
    it = {"id": iid}
    it.update(kw)
    d["items"].append(it); ids[iid] = it; NEW.append(iid)
    return it


placeable("item.seedling_rack", displayName="Fidan rafı", category="garden", price=25, level=3, lockSkill="Horticulture", styleSet="Köylü",
          rooms=["living", "workshop"], finishGroup="wood", site="indoor", tags=["greenhouse", "greenhouse_work"], beauty=3,
          interactions=[{"id": "tend_seedlings", "displayName": "Fidelere bak", "need": "Fun", "restore": 12, "durationDays": 0.05,
                         "skill": "Horticulture", "skillXp": 4}])
placeable("item.potting_bench", displayName="Saksı tezgâhı", category="garden", price=35, level=4, lockSkill="Horticulture", styleSet="Zanaatkâr",
          rooms=["living", "workshop"], finishGroup="wood", site="indoor", width=2, height=1, tags=["greenhouse", "greenhouse_work"], beauty=1)


def recipe(rid, produces, mats, minlvl, name, dur=0.06, xp=6):
    if rid in recipes:
        return
    for m, _ in mats:
        assert m in ids, m
    r = {"id": rid, "station": "item.potting_bench", "produces": produces, "materials": [{"item": m, "count": c} for m, c in mats],
         "durationDays": dur, "skill": "Horticulture", "skillXp": xp, "displayName": name}
    if minlvl > 1:
        r["minLevel"] = minlvl
    d["recipes"].append(r); recipes[rid] = r


recipe("pot_lavender", "item.pot_lavender", [("item.clay", 1), ("item.compost", 1), ("item.lavender", 1)], 4, "Lavanta saksıla")
recipe("herb_pot_thyme", "item.herb_pot_thyme", [("item.clay", 1), ("item.compost", 1), ("item.thyme", 1)], 4, "Kekik saksıla")
recipe("pot_mint", "item.pot_mint", [("item.clay", 1), ("item.compost", 1), ("item.mint", 1)], 4, "Nane saksıla")

json.dump(d, open(ITEMS, "w", encoding="utf-8"), indent=2, ensure_ascii=False)
open(ITEMS, "a", encoding="utf-8").write("\n")
print("seraya ait:", len([k for k in GREEN if k in ids]), "yeni:", NEW)
