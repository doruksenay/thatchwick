"""Yemek geliştirmesi içeriği (Project belgeleri 58, 58a, 60a-60c, 62): items.json, life.json ve yerelleştirme tablolarına yazar.

Kullanım:  python tools/content/yemek_gelistirme.py
Yeniden çalıştırılabilir: eklediği eşya, tarif, etkileşim ve grupları kimliğe göre baştan yazar.

Ne yazar:
- Malzeme grupları, ek baharatlar ve etkileri, konuşma satırları.
- Et, yumurta, süt ayrımı; kesim yan ürünleri; mezbaha, mutfak, piknik ve oyuncak eşyaları.
- Sofra etkileşimleri (masalarda), bulaşık, defter, risaleler.
- Yemek sınıfı ve öğrenme yolu (var olan yemeklere); yeni yemekler ve yeme verileri.
- İngilizce içerik metinleri (content.en.json) ve arayüz metinleri (ui.cooking.<dil>.json).
Eski kayıt: var olan kimlikler değişmez ("Et" artık dana etidir), bu yüzden kayıt çevirisi gerekmez.
"""

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
ITEMS = ROOT / "Assets/Resources/Content/items.json"
LIFE = ROOT / "Assets/Resources/Content/life.json"
LOC = ROOT / "Assets/Resources/Localization"

data = json.loads(ITEMS.read_text(encoding="utf-8"))
items = data["items"]
by_id = {i["id"]: i for i in items}

# Çakışma koruması: betiğin eklediği kimlikler, betikten önceki içerikte (git HEAD) başka bir şeye ait olmamalı.
try:
    import subprocess
    _base = json.loads(subprocess.run(["git", "show", "HEAD~0:Assets/Resources/Content/items.json"], cwd=ROOT, capture_output=True, check=True).stdout.decode("utf-8"))
    _mine = "dish" in json.dumps(_base.get("groups", "")) or "groups" in _base   # betik zaten işlenmişse koruma atlanır
    BASE_ITEMS = set() if _mine else {i["id"] for i in _base["items"]}
    BASE_RECIPES = set() if _mine else {r["id"] for r in _base["recipes"]}
    BASE_ACTS = set() if _mine else {(i["id"], x["id"]) for i in _base["items"] for x in i.get("interactions", []) or []}
except Exception:
    BASE_ITEMS, BASE_RECIPES, BASE_ACTS = set(), set(), set()
EN = {}   # content.en.json'a girecek anahtarlar
POT, OVEN, RACK, MORTAR, BENCH, LECTERN = "item.cooking_pot", "item.stone_oven", "item.drying_rack", "item.mortar_pestle", "item.workbench", "item.writing_lectern"


def put_item(obj, en):
    """Eşyayı ekler ya da yerinde günceller; İngilizce adını kaydeder."""
    assert obj["id"] not in BASE_ITEMS, "bu kimlik zaten başka bir eşyaya ait: " + obj["id"]
    cur = by_id.get(obj["id"])
    if cur is None:
        items.append(obj)
        by_id[obj["id"]] = obj
    else:
        keep = cur.get("interactions")
        cur.clear()
        cur.update(obj)
        if keep is not None and "interactions" not in obj:
            cur["interactions"] = keep
    EN[obj["id"] + ".name"] = en
    return by_id[obj["id"]]


def res(iid, tr, en, price, tags=None, spoil=0, **kw):
    obj = {"id": "item." + iid, "displayName": tr, "placement": "Resource", "category": "resource", "price": price}
    if tags:
        obj["tags"] = tags
    if spoil:
        obj["spoilDays"] = spoil
    obj.update(kw)
    return put_item(obj, en)


def rename(iid, tr, en):
    by_id["item." + iid]["displayName"] = tr
    EN["item." + iid + ".name"] = en


def act(item_id, inter, en, mood_en=None):
    """Etkileşimi eşyaya ekler (aynı kimlik varsa değiştirir)."""
    assert (item_id, inter["id"]) not in BASE_ACTS, f"bu etkileşim zaten var: {item_id}/{inter['id']}"
    it = by_id[item_id]
    lst = it.setdefault("interactions", [])
    lst[:] = [x for x in lst if x["id"] != inter["id"]]
    lst.append(inter)
    EN[f"{item_id}.act.{inter['id']}"] = en
    if mood_en:
        EN[f"{item_id}.act.{inter['id']}.mood"] = mood_en


def recipe(rid, station, produces, mats, tr, en, count=1, skill="Cooking", level=0, days=0.1, xp=10, **kw):
    r = {"id": rid, "station": station, "produces": "item." + produces, "count": count,
         "materials": [{"item": m if m.startswith("group.") else "item." + m, "count": c} for m, c in mats],
         "durationDays": days, "skill": skill, "skillXp": xp, "displayName": tr}
    if level:
        r["minLevel"] = level
    r.update(kw)
    assert rid not in BASE_RECIPES, "bu tarif kimliği zaten var: " + rid
    data["recipes"][:] = [x for x in data["recipes"] if x["id"] != rid]
    data["recipes"].append(r)
    EN[f"recipe.{rid}.name"] = en


def tag(item_id, t):
    tags = by_id[item_id].setdefault("tags", [])
    if t not in tags:
        tags.append(t)


# ---------------------------------------------------------------------------------------------------------------------
# 1. Et, yumurta, süt ayrımı (58 §8) ve kesim yan ürünleri (60a §2.2)
# ---------------------------------------------------------------------------------------------------------------------
rename("meat", "Dana eti", "Beef")
rename("poultry", "Tavuk eti", "Chicken")
rename("game_meat", "Karaca eti", "Venison")
rename("egg", "Tavuk yumurtası", "Hen's egg")
rename("milk", "İnek sütü", "Cow's milk")

RAW = ["food", "raw"]
res("mutton", "Kuzu eti", "Mutton", 9, RAW, 4)
res("goat_meat", "Keçi eti", "Goat meat", 6, RAW, 4)
res("pork", "Domuz eti", "Pork", 8, RAW, 4)
res("rabbit_meat", "Tavşan eti", "Rabbit meat", 6, RAW, 4)
res("duck_meat", "Ördek eti", "Duck meat", 7, RAW, 4)
res("goose_meat", "Kaz eti", "Goose meat", 9, RAW, 4)
res("pigeon_meat", "Güvercin eti", "Pigeon meat", 5, RAW, 4)
res("boar_meat", "Yaban domuzu eti", "Boar meat", 10, RAW, 4)
res("duck_egg", "Ördek yumurtası", "Duck egg", 3, ["food"], 12)
res("goose_egg", "Kaz yumurtası", "Goose egg", 5, ["food"], 12)
res("goat_milk", "Keçi sütü", "Goat's milk", 3, ["food"], 3)
res("goat_cheese", "Keçi peyniri", "Goat cheese", 9, ["food"], 20)
res("offal", "Sakatat", "Offal", 3, RAW, 2)
res("gut", "Bağırsak", "Gut", 2, ["craft"])
res("horn", "Boynuz", "Horn", 4, ["craft"])
res("rennet", "Şirden mayası", "Rennet", 6, ["craft"])
res("gut_string", "Kiriş", "Gut string", 6, ["craft"])
res("horn_comb", "Boynuz tarak", "Horn comb", 12, ["craft"])
res("sucuk", "Sucuk", "Sucuk sausage", 9, ["food", "preserved"])
res("vine_leaf", "Asma yaprağı", "Vine leaf", 1, ["food"], 6)


def roast(iid, tr, en, restore, mood=5, src="roast_meat"):
    base = next(x for x in by_id[POT]["interactions"] if x["id"] == src)
    inter = dict(base, id="roast_" + iid, displayName=tr, restore=restore, consume="item." + iid, moodValue=mood)
    act(POT, inter, en, "Meaty meal")


roast("mutton", "Kuzu eti kızart ve ye", "Roast and eat mutton", 78)
roast("goat_meat", "Keçi eti haşla ve ye", "Boil and eat goat meat", 68, 4)
roast("pork", "Domuz eti kızart ve ye", "Roast and eat pork", 78)
roast("rabbit_meat", "Tavşan eti kızart ve ye", "Roast and eat rabbit", 60, 4)
roast("duck_meat", "Ördek kızart ve ye", "Roast and eat duck", 64, 4)
roast("goose_meat", "Kaz kızart ve ye", "Roast and eat goose", 68)
roast("pigeon_meat", "Güvercin kızart ve ye", "Roast and eat pigeon", 52, 3)
roast("boar_meat", "Yaban domuzu eti kızart ve ye", "Roast and eat boar", 72)
roast("offal", "Ciğer kızart ve ye", "Fry and eat liver", 50, 2)
act(POT, {"id": "eat_duck_egg", "displayName": "Ördek yumurtası pişir ve ye", "need": "Hunger", "restore": 48, "durationDays": 0.04,
          "consume": "item.duck_egg", "skill": "Cooking", "skillXp": 15, "bladder": 4}, "Cook and eat a duck egg")
act(POT, {"id": "eat_goose_egg", "displayName": "Kaz yumurtası pişir ve ye", "need": "Hunger", "restore": 62, "durationDays": 0.04,
          "consume": "item.goose_egg", "skill": "Cooking", "skillXp": 15, "bladder": 4}, "Cook and eat a goose egg")
act(POT, {"id": "drink_goat_milk", "displayName": "Keçi sütü iç", "need": "Hunger", "restore": 25, "durationDays": 0.03,
          "consume": "item.goat_milk", "bladder": 15}, "Drink goat's milk")
act(POT, {"id": "eat_goat_cheese", "displayName": "Keçi peyniri ye", "need": "Hunger", "restore": 38, "durationDays": 0.06,
          "consume": "item.goat_cheese", "bladder": 4}, "Eat goat cheese")
act(POT, {"id": "eat_sucuk", "displayName": "Sucuk ye", "need": "Hunger", "restore": 55, "durationDays": 0.05,
          "consume": "item.sucuk", "bladder": 4}, "Eat sucuk")

BIG = lambda bones, tallow, offal, gut, horn=0: (
    [{"item": "item.bone", "count": bones}, {"item": "item.tallow", "count": tallow}, {"item": "item.offal", "count": offal}, {"item": "item.gut", "count": gut}]
    + ([{"item": "item.horn", "count": horn, "chance": 0.8}] if horn else []))
YOUNG = [{"item": "item.rennet", "count": 1, "youngOnly": True}]
ANIMALS = {
    "animal.chicken": ("item.poultry", 1, []),
    "animal.duck": ("item.duck_meat", 2, [{"item": "item.tallow", "count": 1}]),
    "animal.goose": ("item.goose_meat", 2, [{"item": "item.tallow", "count": 1}]),
    "animal.pigeon": ("item.pigeon_meat", 1, []),
    "animal.goat": ("item.goat_meat", 2, BIG(1, 1, 1, 1, 1) + YOUNG),
    "animal.angora_goat": ("item.goat_meat", 2, BIG(1, 1, 1, 1, 1) + YOUNG),
    "animal.sheep": ("item.mutton", 2, BIG(1, 1, 1, 1) + YOUNG),
    "animal.cow": ("item.meat", 4, BIG(3, 2, 2, 2, 2) + YOUNG),
    "animal.ox": ("item.meat", 4, BIG(3, 2, 2, 2, 2)),
    "animal.pig": ("item.pork", 3, BIG(2, 3, 1, 2)),
    "animal.rabbit": ("item.rabbit_meat", 1, []),
}
for a in data["animals"]:
    if a["id"] in ANIMALS:
        a["meat"], a["meatCount"], by = ANIMALS[a["id"]]
        if by:
            a["byproducts"] = by
        else:
            a.pop("byproducts", None)
    # Ördek ve kaz kendi yumurtasını, keçi kendi sütünü verir.
    if a["id"] == "animal.duck":
        a["produces"] = "item.duck_egg"
    if a["id"] == "animal.goose":
        a["produces"] = "item.goose_egg"
    if a["id"] == "animal.goat":
        a["produces"] = "item.goat_milk"

# Kesim barınaktan kalkar: mezbahada, oyuncunun seçtiği hayvanla yapılır (60a-Z3, 60b-M3).
for it in items:
    if it.get("interactions"):
        it["interactions"] = [x for x in it["interactions"] if not x.get("slaughter")]
# İç yağı artık kesimden çıkar; etten eritme tarifi kalkar (60a §2.2).
data["recipes"][:] = [r for r in data["recipes"] if r["id"] != "render_tallow"]

# ---------------------------------------------------------------------------------------------------------------------
# 2. Domates ve salça (60a-E1: bilinçli dönem istisnası), dönem ekşileri, baharatlar (58a §2)
# ---------------------------------------------------------------------------------------------------------------------
res("tomato", "Domates", "Tomato", 3, ["food"], 6)
res("seed_tomato", "Domates tohumu", "Tomato seed", 5, None, 0, level=6, plantsAs="item.crop_tomato")
garlic_crop = by_id["item.crop_garlic"]
crop = {"id": "item.crop_tomato", "displayName": "Domates ekini", "category": "plant", "growDays": 8, "tilled": True, "diesInWinter": True,
        "growSeasons": ["Spring", "Summer"], "cropFamily": "kabak", "site": "outdoor", "feed": "heavy", "likesFertilizer": ["manure", "compost"], "interactions": []}
for x in garlic_crop["interactions"]:
    y = json.loads(json.dumps(x).replace("item.seed_garlic", "item.seed_tomato").replace("item.garlic", "item.tomato"))
    if y["id"] == "harvest":
        y["produce"] = [{"item": "item.tomato", "count": 4}, {"item": "item.seed_tomato", "count": 1}, {"item": "item.seed_tomato", "count": 1, "chance": 0.5}]
    crop["interactions"].append(y)
put_item(crop, "Tomato plant")
for x in garlic_crop["interactions"]:
    for suffix in ("", ".mood"):
        k = f"item.crop_garlic.act.{x['id']}{suffix}"
        pass  # İngilizceleri aşağıda var olan tablodan kopyalanır (copy_en)
COPY_EN = [("item.crop_garlic", "item.crop_tomato")]
# Tohum çıkarma ve kalbur: sarımsağınkinden.
recipe("seed_tomato", "item.seed_maker", "seed_tomato", [("tomato", 1)], "Tohum çıkar: Domates", "Extract seeds: Tomato", 2, "Horticulture", 0, 0.05, 4)
_sieve = next(x for x in by_id["item.seed_maker"]["interactions"] if x["id"] == "sieve_garlic")
_sieve_tr = _sieve["displayName"]

act("item.seed_maker", json.loads(json.dumps(dict(_sieve, id="sieve_tomato", displayName=_sieve_tr.replace("Sarımsak", "Domates").replace("sarımsak", "domates")))
                                 .replace("item.seed_garlic", "item.seed_tomato").replace("item.garlic", "item.tomato")), "Sieve seed: Tomato")
res("tomato_puree", "Domates ezmesi", "Tomato purée", 4, ["food"], 3)
res("tomato_paste", "Salça", "Tomato paste", 9, ["food", "preserved"], 0)
res("verjuice", "Koruk suyu", "Verjuice", 4, ["condiment"])
res("pomegranate_molasses", "Nar ekşisi", "Pomegranate molasses", 8, ["condiment"])
res("plum_sour", "Erik ekşisi", "Sour plum paste", 4, ["condiment"])
res("sumac", "Sumak", "Sumac", 5, ["condiment", "merchant"])
res("cinnamon", "Tarçın", "Cinnamon", 14, ["spice", "merchantonly"])
for herb, tr, en in (("mint", "nane", "mint"), ("thyme", "kekik", "thyme"), ("dill", "dereotu", "dill"), ("sage", "adaçayı", "sage"), ("lavender", "lavanta", "lavender")):
    cap = tr[0].upper() + tr[1:]
    res(herb + "_dried", f"Kuru {tr} demeti", f"Dried {en} bunch", 4, ["herb"])
    res(herb + "_ground", f"{cap} (ufalanmış)", f"Rubbed {en}", 6, ["spice"])
    recipe("bundle_" + herb, RACK, herb + "_dried", [(herb, 2)], f"{cap} demetle ve kurut", f"Bundle and dry {en}", 2, "Herbalism", 0, 0.06, 6)
    recipe("rub_" + herb, MORTAR, herb + "_ground", [(herb + "_dried", 2)], f"Kuru {tr} ufala", f"Rub dried {en}", 2, "Herbalism", 0, 0.04, 5)
res("fenugreek_roasted", "Kavrulmuş çemen", "Roasted fenugreek", 5, ["spice"])
res("fenugreek_ground", "Çemen tozu", "Ground fenugreek", 7, ["spice"])
res("family_blend", "Aile karışımı", "Family blend", 12, ["spice"])
recipe("roast_fenugreek", POT, "fenugreek_roasted", [("fenugreek", 2)], "Çemen kavur", "Roast fenugreek", 2, "Cooking", 2, 0.05, 6)
recipe("grind_fenugreek", MORTAR, "fenugreek_ground", [("fenugreek_roasted", 2)], "Çemen döv", "Pound fenugreek", 2, "Herbalism", 0, 0.04, 5)
recipe("family_blend", MORTAR, "family_blend", [("black_pepper", 1), ("thyme_ground", 1), ("mint_ground", 1)], "Aile karışımı hazırla", "Mix the family blend", 2, "Cooking", 8, 0.05, 10)
recipe("tomato_puree", POT, "tomato_puree", [("tomato", 4), ("salt", 1)], "Domates ez ve kaynat", "Crush and boil tomatoes", 2, "Cooking", 3, 0.12, 10)
recipe("verjuice", POT, "verjuice", [("grape", 3)], "Koruk suyu sık", "Press verjuice", 2, "Cooking", 2, 0.06, 6)
recipe("pomegranate_molasses", POT, "pomegranate_molasses", [("pomegranate", 3)], "Nar ekşisi kaynat", "Boil down pomegranate molasses", 1, "Cooking", 4, 0.15, 10)
recipe("plum_sour", RACK, "plum_sour", [("plum", 3)], "Erik ekşisi kurut", "Dry sour plum paste", 2, "Cooking", 2, 0.1, 6)
recipe("sucuk", POT, "sucuk", [("group.red_meat", 2), ("gut", 1), ("garlic", 1), ("salt", 1)], "Sucuk doldur", "Stuff sucuk", 3, "Cooking", 4, 0.15, 14)
recipe("gut_string", RACK, "gut_string", [("gut", 2)], "Kiriş ger ve kurut", "Stretch and dry gut string", 1, "Leatherwork", 2, 0.08, 6)
recipe("horn_comb", BENCH, "horn_comb", [("horn", 1)], "Boynuz tarak oy", "Carve a horn comb", 1, "Carpentry", 3, 0.08, 8)
recipe("goat_cheese", POT, "goat_cheese", [("goat_milk", 2), ("rennet", 1)], "Keçi peyniri mayala", "Curdle goat cheese", 2, "Cooking", 3, 0.12, 10)
recipe("cheese_rennet", POT, "cheese", [("milk", 2), ("rennet", 1)], "Şirdenle peynir mayala", "Curdle cheese with rennet", 3, "Cooking", 3, 0.12, 10)

# Asma yaprağı: bağ bozumunda üzümle birlikte toplanır.
for x in by_id["item.vine_grape"]["interactions"]:
    if x["id"] == "pick":
        x["produce"] = [p for p in x["produce"] if p["item"] != "item.vine_leaf"] + [{"item": "item.vine_leaf", "count": 2, "chance": 0.7}]

EFFECTS = [
    ("eff_spice_mint", "Nane ferahlığı", "Mint freshness", "Yemekteki nane ferahlattı: biraz geç yorulur.", "The mint in the meal is refreshing: tires a little later.", 2, 0.5, {"energyDecay": 0.92}),
    ("eff_spice_thyme", "Kekik şifası", "Thyme's comfort", "Kekikli yemek içini ısıttı: soğuktan %10 az etkilenir.", "The thyme warmed them through: 10% less affected by cold.", 2, 0.5, {"coldResist": 0.1}),
    ("eff_spice_dill", "İştahı açıldı", "Appetite whetted", "Dereotu iştah açtı: %8 geç acıkır.", "The dill whetted the appetite: gets hungry 8% later.", 2, 0.5, {"hungerDecay": 0.92}),
    ("eff_spice_fenugreek", "Çemenli tokluk", "Fenugreek fullness", "Çemen uzun tok tutar: %15 geç acıkır.", "Fenugreek keeps you full: gets hungry 15% later.", 2, 0.75, {"hungerDecay": 0.85}),
    ("eff_spice_bay", "Defne kokusu", "Scent of bay", "Defneli yemeğin kokusu üstünde kaldı.", "The scent of the bay-leaf dish lingers.", 2, 0.5, {}),
    ("eff_spice_pepper", "Biberle ısındı", "Warmed by pepper", "Karabiber içini ısıttı: soğuktan %20 az etkilenir.", "The pepper warmed them: 20% less affected by cold.", 2, 0.5, {"coldResist": 0.2}),
    ("eff_spice_saffron", "Şahane sofra", "A splendid table", "Safranlı yemek: böylesi her gün yenmez.", "A saffron dish: not something you eat every day.", 5, 1.0, {}),
    ("eff_spice_garlic", "Sarımsaklı", "Garlicky", "Sarımsak hastalığa iyi gelir; kokusu da üstünde.", "Garlic keeps illness away; the smell stays too.", 1, 0.5, {"coldResist": 0.1}),
    ("eff_spice_cinnamon", "Tarçın keyfi", "Cinnamon cheer", "Tarçınlı tatlı keyif verdi.", "The cinnamon sweet lifted the spirits.", 3, 0.5, {}),
    ("eff_spice_lavender", "Lavantayla sakinledi", "Calmed by lavender", "Lavanta sakinleştirdi: %10 geç yorulur.", "Lavender is calming: tires 10% later.", 2, 0.75, {"energyDecay": 0.9}),
    ("eff_spice_sage", "Adaçayı yumuşaklığı", "Soothed by sage", "Adaçayı boğazı yumuşattı.", "The sage soothed the throat.", 2, 0.5, {}),
    ("eff_spice_fresh", "Bahar sofrası", "Spring table", "Taze otlu yemek bahar gibi geldi.", "A dish with fresh greens tastes of spring.", 2, 0.5, {}),
    ("eff_family_blend", "Aile karışımı", "Family blend", "Bu tat yalnızca bizim evde olur.", "This taste is only found at our house.", 4, 1.0, {"hungerDecay": 0.92}),
]
for eid, tr, en, dtr, den, value, days, extra in EFFECTS:
    data["effects"][:] = [e for e in data["effects"] if e["id"] != eid]
    data["effects"].append(dict({"id": eid, "displayName": tr, "description": dtr, "value": value, "days": days}, **extra))
    EN[f"effect.{eid}.name"] = en
    EN[f"effect.{eid}.desc"] = den

SWEETISH = ["sweet", "pastry", "drink", "dairy"]
SPICES = [
    ("mint", "eff_spice_mint", False, [], ["meat"], 0), ("mint_ground", "eff_spice_mint", True, [], [], 0),
    ("thyme", "eff_spice_thyme", False, [], ["sweet", "drink"], 0), ("thyme_ground", "eff_spice_thyme", True, [], ["sweet", "drink"], 0),
    ("dill", "eff_spice_dill", False, [], ["sweet", "drink", "meat"], 0), ("dill_ground", "eff_spice_dill", True, [], ["sweet", "drink"], 0),
    ("sage", "eff_spice_sage", False, [], ["sweet"], 0), ("sage_ground", "eff_spice_sage", True, [], ["sweet"], 0),
    ("lavender", "eff_spice_lavender", False, SWEETISH, [], 0), ("lavender_ground", "eff_spice_lavender", True, SWEETISH, [], 0),
    ("bay_leaf", "eff_spice_bay", True, ["soup", "meat", "fish", "vegetable", "grain"], [], 2),
    ("fenugreek_ground", "eff_spice_fenugreek", True, ["soup", "meat", "vegetable", "grain", "snack"], [], 0),
    ("saffron", "eff_spice_saffron", True, ["soup", "meat", "fish", "grain", "pastry", "sweet", "dairy"], [], 1),
    ("black_pepper", "eff_spice_pepper", True, [], ["sweet", "drink"], 0),
    ("garlic", "eff_spice_garlic", False, [], ["sweet", "drink", "pastry", "dairy"], 0),
    ("cinnamon", "eff_spice_cinnamon", True, ["sweet", "pastry", "drink", "grain", "dairy"], [], 1),
    ("wild_garlic", "eff_spice_fresh", False, ["soup", "vegetable", "dairy", "snack"], [], 0),
    ("cress", "eff_spice_fresh", False, ["soup", "vegetable", "fish", "snack"], [], 0),
    ("family_blend", "eff_family_blend", True, ["soup", "meat", "fish", "vegetable", "grain"], [], 1),
]
data["spices"] = [dict({"item": "item." + i, "effect": e, "processed": p}, **({"fits": f} if f else {}), **({"clashes": c} if c else {}), **({"smell": s} if s else {}))
                  for i, e, p, f, c, s in SPICES]

# ---------------------------------------------------------------------------------------------------------------------
# 3. Malzeme grupları (58 §5.1). Üye sırası harcama sırasıdır: en sıradan önce.
# ---------------------------------------------------------------------------------------------------------------------
G = lambda *m: [dict({"item": "item." + (x if isinstance(x, str) else x[0])}, **({} if isinstance(x, str) else dict(bonus=x[1], **({"kind": x[2]} if len(x) > 2 else {})))) for x in m]
GROUPS = [
    ("group.fish", "Balık (herhangi)", "Any fish", G(("fish_rudd", 0, "beyaz etli"), ("fish_bream", 0, "beyaz etli"), ("fish_tench", 0, "beyaz etli"), ("fish_perch", 0, "beyaz etli"),
        ("fish_carp", 0, "beyaz etli"), ("fish_bullhead", 0, "beyaz etli"), ("fish_pike", 0, "beyaz etli"), ("fish_catfish", 0, "beyaz etli"),
        ("fish_trout", 1, "beyaz etli"), ("fish_herring", 1, "yağlı"), ("fish_mackerel", 1, "yağlı"), ("fish_eel", 1, "yağlı"), ("fish_salmon", 2, "yağlı"))),
    ("group.shellfish", "Kabuklu (kerevit, midye)", "Shellfish", G("mussel", ("fish_crayfish", 1))),
    ("group.red_meat", "Kırmızı et (herhangi)", "Any red meat", G(("goat_meat", -1), "rabbit_meat", "pork", "meat", ("mutton", 1))),
    ("group.poultry", "Kümes eti (herhangi)", "Any poultry", G("poultry", "pigeon_meat", ("duck_meat", 1), ("goose_meat", 1))),
    ("group.game", "Av eti (herhangi)", "Any game", G(("game_meat", 1), ("boar_meat", 1))),
    ("group.fat", "Yağ (herhangi)", "Any cooking fat", G(("tallow", -2), "olive_oil", ("butter", 1), ("olive_oil_virgin", 2))),
    ("group.grain", "Tahıl (herhangi)", "Any grain", G(("barley", -1), ("oats", -1), "rye", "wheat")),
    ("group.root", "Kök sebze (herhangi)", "Any root vegetable", G("turnip", "radish", "beet", "parsnip", "carrot", "onion")),
    ("group.greens", "Yeşillik (herhangi)", "Any greens", G("cabbage", "leek", "lettuce", "spinach", "cress", "wild_garlic")),
    ("group.fruit", "Meyve (herhangi)", "Any fruit", G("apple", "pear", "plum", "berries", "grape", "cherry", "wild_strawberry", ("pomegranate", 1))),
    ("group.nut", "Kuruyemiş (herhangi)", "Any nut", G("nut", "chestnut", ("walnut", 1))),
    ("group.egg", "Yumurta (herhangi)", "Any egg", G("egg", ("duck_egg", 1), ("goose_egg", 1))),
    ("group.milk", "Süt (herhangi)", "Any milk", G("milk", "goat_milk")),
    ("group.cheese", "Peynir (herhangi)", "Any cheese", G("cheese", "brined_cheese", ("goat_cheese", 1), ("herb_cheese", 1), ("smoked_cheese", 1), ("tulum_cheese", 1), ("kasar", 1), ("aged_cheese", 2))),
    ("group.sweetener", "Tatlandırıcı (bal, pekmez)", "Any sweetener", G("pekmez", "honey", ("honey_strained", 1))),
    ("group.sour", "Ekşi (sirke, koruk, nar ekşisi)", "Any souring", G("vinegar", "verjuice", "plum_sour", ("sumac", 1), ("pomegranate_molasses", 1), ("aged_vinegar", 2))),
]
data["groups"] = []
for gid, tr, en, members in GROUPS:
    for m in members:
        assert m["item"] in by_id, m["item"]
    data["groups"].append({"id": gid, "displayName": tr, "members": members})
    EN[gid + ".name"] = en

# Eski tarifler gruba döner: "Et" isteyen tarif artık herhangi bir kırmızı etle pişer (58 §8).
TO_GROUP = {"item.meat": "group.red_meat", "item.poultry": "group.poultry", "item.egg": "group.egg", "item.milk": "group.milk", "item.game_meat": "group.game"}
KITCHEN_SKILLS = ("Cooking", "Baking", "Brewing")
KEEP_SPECIFIC = {"cheese_rennet", "tomato_puree"}
for r in data["recipes"]:
    if r.get("skill") in KITCHEN_SKILLS and r["id"] not in KEEP_SPECIFIC:
        for m in r.get("materials", []):
            m["item"] = TO_GROUP.get(m["item"], m["item"])
for it in items:
    for x in it.get("interactions", []) or []:
        if x.get("skill") in KITCHEN_SKILLS and x.get("produce"):
            for m in x.get("materials", []) or []:
                m["item"] = TO_GROUP.get(m["item"], m["item"])

# ---------------------------------------------------------------------------------------------------------------------
# 4. Eşyalar: mezbaha, mutfak, piknik, risaleler, oyuncaklar; sofra ve defter etkileşimleri
# ---------------------------------------------------------------------------------------------------------------------
put_item({"id": "item.slaughter_hook", "displayName": "Kesim kancası", "width": 1, "height": 1, "price": 30, "category": "barn", "level": 4, "styleSet": "Köylü",
          "site": "indoor", "belongs": ["storehouse", "barn"], "anywhere": True, "tags": ["slaughterhook"], "interactions": []}, "Slaughter hook")
put_item({"id": "item.meat_locker", "displayName": "Et dolabı", "width": 2, "height": 1, "price": 35, "category": "barn", "level": 4, "styleSet": "Köylü",
          "site": "indoor", "belongs": ["storehouse", "barn"], "anywhere": True, "tall": True, "tags": ["meatstore"], "interactions": []}, "Meat locker")
put_item({"id": "item.ice_trough", "displayName": "Buz teknesi", "width": 1, "height": 1, "price": 25, "category": "barn", "level": 5, "styleSet": "Köylü",
          "site": "indoor", "belongs": ["storehouse", "barn", "cellar"], "anywhere": True, "tags": ["meatcooler"]}, "Ice trough")
put_item({"id": "item.drain_channel", "displayName": "Akıtma oluğu", "width": 1, "height": 1, "price": 12, "category": "barn", "level": 4, "styleSet": "Köylü",
          "site": "indoor", "belongs": ["storehouse", "barn"], "anywhere": True, "comfort": 1}, "Drain channel")
# Deri askısı zaten var (item.hide_rack): mezbaha odasına o konur.
put_item({"id": "item.spice_rack", "displayName": "Baharat rafı", "category": "decor_wall", "price": 18, "level": 3, "placement": "Wall", "styleSet": "Köylü",
          "rooms": ["kitchen"], "wallBands": ["orta"], "wallWidth": 1, "beauty": 2, "finishGroup": "wood", "tags": ["spicerack"]}, "Spice rack")
put_item({"id": "item.paste_tray", "displayName": "Salça tepsisi", "width": 1, "height": 1, "price": 10, "category": "utility", "level": 4, "styleSet": "Köylü",
          "site": "outdoor", "belongs": ["outdoor"], "anywhere": True, "interactions": []}, "Paste tray")
recipe("tomato_paste", "item.paste_tray", "tomato_paste", [("tomato_puree", 2), ("salt", 1)], "Salçayı tepside güneşte kurut", "Sun-dry the paste on the tray", 2, "Cooking", 3, 0.1, 12)
res("picnic_basket", "Piknik sepeti", "Picnic basket", 20, ["craft"])
res("picnic_cloth", "Piknik örtüsü", "Picnic cloth", 10, ["craft"])
recipe("picnic_basket", BENCH, "picnic_basket", [("basket", 1)], "Piknik sepeti hazırla", "Fit out a picnic basket", 1, "Carpentry", 2, 0.06, 8)
for tid, tr, en in (("toy_pot_set", "Oyuncak tencere takımı", "Toy pot set"), ("toy_hammer", "Tahta çekiç", "Wooden toy hammer"),
                    ("toy_hoe", "Küçük çapa", "Little hoe"), ("toy_sheep", "Bez koyun", "Rag sheep")):
    put_item({"id": "item." + tid, "displayName": tr, "category": "kids", "price": 5, "placement": "Surface", "styleSet": "Köylü", "decorSize": "S", "weight": 1,
              "beauty": 1, "material": "cloth" if tid == "toy_sheep" else "wood", "fitsOn": ["masa", "raf"]}, en)

act("item.slaughter_hook", {"id": "slaughter", "displayName": "Listedeki hayvanı kes", "durationDays": 0.12, "skill": "AnimalHusbandry", "skillXp": 4,
                            "meal": "slaughter", "adultOnly": True}, "Slaughter the listed animal")
for holder in ("item.slaughter_hook", "item.meat_locker"):
    act(holder, {"id": "collect_meat", "displayName": "Mezbaha deposunu kilere taşı", "durationDays": 0.04, "skill": "AnimalHusbandry", "skillXp": 1,
                 "meal": "collectMeat"}, "Carry the slaughterhouse stock to the pantry")

TABLES = ["table_wood", "dining_table_long", "table_painted", "trestle_table", "walnut_table", "sini_table", "han_table", "catki_table", "yoruk_table", "stump_table", "barrel_table"]
for t in TABLES:
    tid = "item." + t
    tag(tid, "diningtable")
    act(tid, {"id": "eat_serving", "displayName": "Sofradan ye", "need": "Hunger", "restore": 55, "durationDays": 0.05, "bladder": 4, "meal": "eatServing"}, "Eat at the table")
    act(tid, {"id": "clear_table", "displayName": "Sofrayı topla", "durationDays": 0.03, "skill": "Decorating", "skillXp": 1, "meal": "clearTable"}, "Clear the table")
    # İhtiyaç doyurmaz: karakter kendi başına seçmesin (köpek her öğün dilenmeye alışır); yalnızca oyuncu emriyle.
    act(tid, {"id": "feed_scrap", "displayName": "Köpeğe bir lokma ver", "durationDays": 0.01, "skill": "AnimalHusbandry", "skillXp": 1, "meal": "feedScrap"}, "Give the dog a scrap")
for w in ("basin_ewer", "wash_tub", "water_bucket"):
    act("item." + w, {"id": "wash_dishes", "displayName": "Bulaşık yıka", "durationDays": 0.04, "skill": "Decorating", "skillXp": 1, "meal": "washDishes"}, "Wash the dishes")
for f in ("pantry_shelf", "aumbry", "root_cellar", "storage_jar", "ice_box", "granary"):
    tag("item." + f, "foodpoint")
for p in ("prep_counter", "kneading_trough"):
    tag("item." + p, "preptable")

act(LECTERN, {"id": "write_recipes", "displayName": "Bildiği tarifleri deftere yaz", "durationDays": 0.1, "skill": "Literacy", "skillXp": 6, "meal": "writeRecipes",
              "consume": "item.parchment"}, "Write known recipes into the cookbook")
act(LECTERN, {"id": "read_recipes", "displayName": "Defterden tarif öğren", "durationDays": 0.08, "skill": "Literacy", "skillXp": 4, "meal": "readRecipes"},
    "Learn a recipe from the cookbook")

# ---------------------------------------------------------------------------------------------------------------------
# 5. Yemekler: var olanların sınıfı ve öğrenme yolu; yeni yemekler (58 §7, 58 §4)
# ---------------------------------------------------------------------------------------------------------------------
CLASS = {
    "soup": ["soup_pea", "soup_bean", "soup_fish", "soup_chicken"],
    "meat": ["stew_meat", "stew_game"],
    "fish": ["wrap_fish"],
    "vegetable": ["stew_plain", "saute_mushroom", "sarma", "leek_olive_oil"],
    "grain": ["pilaf", "porridge_oats", "porridge_wheat", "wedding_dish"],
    "dairy": ["omelet", "kaygana", "breakfast"],
    "pastry": ["bread", "borek_leek", "kete_meat", "tart_apple", "simit", "bread_rye", "bread_maslin", "bazlama", "yufka", "bread_sourdough", "wedding_bread"],
    "sweet": ["hosaf", "helva"],
    "snack": ["miner_ration"],
}
BASIC = {"stew_plain", "soup_pea", "bread", "porridge_wheat", "porridge_oats", "omelet", "bazlama", "soup_bean"}
CLASS_OF = {"item." + d: c for c, ds in CLASS.items() for d in ds}
classified = set()
for r in data["recipes"]:
    if r["produces"] in CLASS_OF and r.get("skill") in ("Cooking", "Baking"):
        r["dish"] = CLASS_OF[r["produces"]]
        if r["produces"][5:] in BASIC:
            r["learn"] = "basic"
        classified.add(r["produces"])
for it in items:
    for x in it.get("interactions", []) or []:
        prod = (x.get("produce") or [{}])[0].get("item")
        if prod in CLASS_OF and x.get("skill") in ("Cooking", "Baking"):
            x["dish"] = CLASS_OF[prod]
            if prod[5:] in BASIC:
                x["learn"] = "basic"
            classified.add(prod)
missing = sorted(set(CLASS_OF) - classified)
assert not missing, "tarifi bulunamayan yemekler: " + ", ".join(missing)

RESTORE = {"soup": 55, "meat": 76, "fish": 66, "vegetable": 56, "grain": 66, "dairy": 55, "pastry": 60, "sweet": 40, "snack": 46, "drink": 20}
EFFECT = {"soup": "eff_warm_soup", "meat": "eff_hearty", "fish": "eff_fisher", "grain": "eff_full", "sweet": "eff_sweet", "pastry": "good_meal",
          "vegetable": "good_meal", "dairy": "eff_breakfast", "snack": "eff_travel", "drink": "eff_sweet"}
# (kimlik, Türkçe, İngilizce, sınıf, istasyon, beceri, seviye, malzemeler, adet, fiyat, öğrenme, [servis], [etki])
D = [
    # çorbalar
    ("soup_tarhana", "Tarhana çorbası", "Tarhana soup", "soup", POT, "Cooking", 2, [("tarhana", 1), ("group.fat", 1)], 3, 6, None),
    ("soup_cabbage", "Lahana çorbası", "Cabbage soup", "soup", POT, "Cooking", 1, [("cabbage", 2), ("onion", 1)], 3, 5, "basic"),
    ("soup_beet", "Pancar çorbası", "Beet soup", "soup", POT, "Cooking", 3, [("beet", 2), ("onion", 1), ("group.sour", 1)], 3, 7, None),
    ("soup_crayfish", "Kerevit çorbası", "Crayfish soup", "soup", POT, "Cooking", 5, [("group.shellfish", 2), ("onion", 1), ("butter", 1)], 3, 11, "villager:villager.kemal:60"),
    ("soup_bone", "Kemik suyu çorbası", "Bone broth soup", "soup", POT, "Cooking", 2, [("bone", 2), ("group.root", 2)], 4, 6, None, None, "eff_broth"),
    ("soup_tripe", "İşkembe çorbası", "Tripe soup", "soup", POT, "Cooking", 4, [("offal", 1), ("garlic", 1), ("group.sour", 1)], 3, 8, "villager:villager.serife:30"),
    ("soup_wild_garlic", "Yabani sarımsak çorbası", "Wild garlic soup", "soup", POT, "Cooking", 2, [("wild_garlic", 3), ("group.milk", 1)], 3, 6, None, None, "eff_forager"),
    # etli yemekler
    ("lamb_tandir", "Kuzu tandır", "Slow-roast lamb", "meat", OVEN, "Cooking", 6, [("mutton", 2), ("onion", 1), ("salt", 1)], 4, 16, "villager:villager.serife:60", "tray:stew"),
    ("beef_yahni", "Dana yahni", "Beef stew", "meat", POT, "Cooking", 4, [("meat", 2), ("onion", 2), ("group.root", 1)], 4, 12, None),
    ("pork_kavurma", "Domuz kavurma", "Fried pork", "meat", POT, "Cooking", 3, [("pork", 2), ("salt", 1)], 3, 11, None, "tray:fry"),
    ("goose_roast", "Kaz çevirme", "Roast goose", "meat", OVEN, "Cooking", 7, [("goose_meat", 2), ("apple", 1), ("salt", 1)], 4, 18, "request", "tray:fry", "eff_host"),
    ("rabbit_stew", "Tavşan yahnisi", "Rabbit stew", "meat", POT, "Cooking", 3, [("rabbit_meat", 2), ("onion", 1), ("carrot", 1)], 3, 10, None),
    ("venison_roast", "Karaca kızartması", "Roast venison", "meat", OVEN, "Cooking", 6, [("game_meat", 2), ("group.fat", 1), ("thyme", 1)], 4, 17, None, "tray:fry", "eff_hunter"),
    ("pigeon_stuffed", "Güvercin dolması", "Stuffed pigeon", "meat", OVEN, "Cooking", 5, [("pigeon_meat", 2), ("group.grain", 1), ("onion", 1)], 2, 13, "treatise", "tray:fry"),
    ("liver_fry", "Ciğer kavurma", "Fried liver", "meat", POT, "Cooking", 3, [("offal", 2), ("onion", 1), ("group.fat", 1)], 3, 8, None, "tray:fry"),
    ("goat_stew", "Keçi haşlama", "Boiled goat", "meat", POT, "Cooking", 2, [("goat_meat", 2), ("group.root", 2)], 4, 9, None),
    ("duck_plum", "Erikli ördek", "Duck with plums", "meat", OVEN, "Cooking", 5, [("duck_meat", 2), ("plum", 2)], 3, 14, "villager:villager.murat:60", "tray:fry"),
    ("boar_stew", "Yaban domuzu yahnisi", "Boar stew", "meat", POT, "Cooking", 6, [("boar_meat", 2), ("onion", 1), ("group.sour", 1)], 4, 16, None, None, "eff_hunter"),
    ("meatballs", "Köfte", "Meatballs", "meat", POT, "Cooking", 4, [("group.red_meat", 2), ("onion", 1), ("bread", 1)], 4, 11, None, "tray:fry"),
    ("sucuk_egg", "Sucuklu yumurta", "Eggs with sucuk", "meat", POT, "Cooking", 3, [("sucuk", 1), ("group.egg", 2)], 2, 12, None, "tray:fry", "eff_breakfast"),
    ("paste_stew", "Salçalı et güveci", "Meat stew with tomato paste", "meat", POT, "Cooking", 5, [("group.red_meat", 2), ("tomato_paste", 1), ("onion", 1), ("group.root", 1)], 4, 15, None),
    # balık
    ("fish_fry", "Balık kızartması", "Fried fish", "fish", POT, "Cooking", 3, [("group.fish", 2), ("flour", 1), ("group.fat", 1)], 2, 10, None, "tray:fish"),
    ("fish_grill", "Izgara balık", "Grilled fish", "fish", POT, "Cooking", 2, [("group.fish", 2), ("salt", 1)], 2, 9, None, "tray:fish"),
    ("fish_steam", "Balık buğulama", "Steamed fish", "fish", POT, "Cooking", 5, [("group.fish", 2), ("onion", 1), ("group.sour", 1)], 3, 12, "villager:villager.kemal:30", "crock:fish"),
    ("eel_pie", "Yılan balığı turtası", "Eel pie", "fish", OVEN, "Baking", 6, [("fish_eel", 1), ("flour", 2), ("butter", 1)], 3, 15, "treatise", "tray:pastry"),
    ("mussel_pilaf", "Midye pilavı", "Mussel pilaf", "fish", POT, "Cooking", 5, [("mussel", 3), ("group.grain", 2), ("onion", 1)], 3, 12, None, "sini:pilaf"),
    ("salt_fish_boil", "Tuzlu balık haşlama", "Boiled salt fish", "fish", POT, "Cooking", 2, [("salted_fish", 2), ("group.root", 2)], 3, 8, None, "crock:fish"),
    # sebze ve zeytinyağlı
    ("sarma_leaf", "Yaprak sarması", "Stuffed vine leaves", "vegetable", POT, "Cooking", 5, [("vine_leaf", 4), ("group.grain", 1), ("onion", 1), ("olive_oil", 1)], 4, 11, None, "tray:sarma", "eff_olive_oil"),
    ("broad_bean_oil", "Zeytinyağlı bakla", "Broad beans in olive oil", "vegetable", POT, "Cooking", 3, [("broad_bean", 3), ("onion", 1), ("olive_oil", 1)], 3, 8, None, None, "eff_olive_oil"),
    ("spinach_saute", "Ispanak kavurma", "Sautéed spinach", "vegetable", POT, "Cooking", 2, [("spinach", 3), ("onion", 1), ("group.fat", 1)], 3, 6, None),
    ("pumpkin_stuffed", "Su kabağı dolması", "Stuffed bottle gourd", "vegetable", POT, "Cooking", 4, [("pumpkin", 1), ("group.grain", 1), ("onion", 1)], 3, 8, None, "tray:sarma"),
    ("radish_salad", "Turp salatası", "Radish salad", "vegetable", POT, "Cooking", 1, [("radish", 2), ("group.sour", 1)], 2, 4, None, "bowl:salad", "eff_refresh"),
    ("cress_salad", "Su teresi salatası", "Watercress salad", "vegetable", POT, "Cooking", 1, [("cress", 3), ("olive_oil", 1)], 2, 5, None, "bowl:salad", "eff_refresh"),
    ("cabbage_paste", "Salçalı lahana", "Cabbage with tomato paste", "vegetable", POT, "Cooking", 3, [("cabbage", 2), ("tomato_paste", 1), ("onion", 1)], 3, 8, None),
    ("turnip_mash", "Şalgam ezmesi", "Mashed turnip", "vegetable", POT, "Cooking", 1, [("turnip", 3), ("butter", 1)], 3, 5, "basic", "bowl:soup"),
    # tahıl ve bakliyat
    ("saffron_pilaf", "Safranlı pilav", "Saffron pilaf", "grain", POT, "Cooking", 7, [("group.grain", 2), ("saffron", 1), ("butter", 1)], 4, 20, None, None, "eff_host"),
    ("porridge_barley", "Arpa lapası", "Barley porridge", "grain", POT, "Cooking", 1, [("barley", 2), ("group.milk", 1)], 3, 4, "basic", "bowl:soup"),
    ("pea_mash", "Bezelye ezmesi", "Pease pudding", "grain", POT, "Cooking", 2, [("dried_pea", 2), ("onion", 1), ("group.fat", 1)], 3, 6, None, "bowl:soup"),
    ("rye_gruel", "Çavdar bulamacı", "Rye gruel", "grain", POT, "Cooking", 1, [("rye_flour", 1), ("group.milk", 1)], 2, 4, None, "bowl:soup"),
    ("paste_pilaf", "Salçalı bulgur pilavı", "Bulgur pilaf with tomato paste", "grain", POT, "Cooking", 4, [("group.grain", 2), ("tomato_paste", 1), ("onion", 1)], 4, 10, None),
    ("milk_wheat", "Sütlü buğday", "Milky wheat", "grain", POT, "Cooking", 3, [("wheat", 2), ("group.milk", 1), ("group.sweetener", 1)], 3, 8, None, "bowl:sweet", "eff_sweet"),
    # yumurta, süt, kahvaltı
    ("duck_kaygana", "Ördek yumurtalı kaygana", "Duck-egg pancake", "dairy", POT, "Cooking", 3, [("duck_egg", 2), ("flour", 1), ("butter", 1)], 3, 9, None, "tray:pastry"),
    ("cokelek", "Çökelek", "Dry curd", "dairy", POT, "Cooking", 2, [("yogurt", 2)], 2, 6, "villager:villager.murat:30", "bowl:salad"),
    ("honey_milk", "Ballı süt", "Honey milk", "drink", POT, "Cooking", 1, [("group.milk", 1), ("group.sweetener", 1)], 2, 5, None),
    # hamur işi
    ("gozleme", "Gözleme", "Gözleme", "pastry", POT, "Cooking", 3, [("yufka", 2), ("group.cheese", 1), ("group.greens", 1)], 3, 9, None),
    ("borek_cheese", "Peynirli börek", "Cheese börek", "pastry", OVEN, "Baking", 4, [("yufka", 2), ("group.cheese", 2), ("group.egg", 1)], 4, 12, None),
    ("saffron_bun", "Safranlı bayram çöreği", "Saffron feast bun", "pastry", OVEN, "Baking", 8, [("flour", 2), ("saffron", 1), ("group.egg", 1), ("butter", 1)], 4, 22, None, None, "eff_host"),
    ("chestnut_bread", "Kestaneli ekmek", "Chestnut bread", "pastry", OVEN, "Baking", 4, [("flour", 2), ("chestnut", 2)], 3, 9, "villager:villager.hatice:30"),
    ("walnut_bun", "Cevizli çörek", "Walnut bun", "pastry", OVEN, "Baking", 5, [("flour", 2), ("walnut", 2), ("group.sweetener", 1)], 3, 11, None, None, "eff_sweet"),
    # tatlı
    ("kome", "Köme", "Köme (walnut strings)", "sweet", POT, "Cooking", 5, [("walnut", 3), ("pekmez", 1), ("flour", 1)], 3, 12, None),
    ("pestil", "Pestil", "Fruit leather", "sweet", POT, "Cooking", 3, [("group.fruit", 4)], 3, 7, None),
    ("honey_nuts", "Ballı fındık", "Honeyed hazelnuts", "sweet", POT, "Cooking", 2, [("nut", 3), ("honey", 1)], 3, 8, None),
    ("cherry_tart", "Kirazlı tart", "Cherry tart", "sweet", OVEN, "Baking", 5, [("flour", 2), ("cherry", 3), ("butter", 1)], 3, 12, "treatise", "tray:pastry"),
    ("chestnut_candy", "Kestane şekerlemesi", "Candied chestnuts", "sweet", POT, "Cooking", 6, [("chestnut", 3), ("honey", 1)], 3, 13, "villager:villager.hatice:60"),
    ("pear_hosaf", "Armut hoşafı", "Stewed pears", "sweet", POT, "Cooking", 2, [("pear", 3), ("group.sweetener", 1)], 3, 6, None, None, "eff_refresh"),
    # azık ve atıştırma
    ("olive_bread", "Zeytin ezmeli ekmek", "Bread with olive paste", "snack", POT, "Cooking", 1, [("bread", 1), ("olive_paste", 1)], 2, 7, None, None, "eff_olive_oil"),
    ("dried_fruit_ration", "Kuru meyveli azık", "Dried-fruit ration", "snack", POT, "Cooking", 2, [("dried_apple", 1), ("dried_plum", 1), ("group.nut", 2)], 3, 8, None),
]
NEW_DISHES = []
for row in D:
    did, tr, en, cls, station, skill, level, mats, count, price, learn = row[:11]
    serve = row[11] if len(row) > 11 else None
    effect = row[12] if len(row) > 12 else EFFECT[cls]
    for m, _ in mats:
        assert m.startswith("group.") or "item." + m in by_id, f"{did}: {m}"
    drink = cls == "drink"
    res(did, tr, en, price, ["food", "drink" if drink else "cooked"], 2 if cls in ("soup", "fish", "dairy", "drink") else 3 if cls != "snack" else 20)
    extra = {"dish": cls}
    if learn:
        extra["learn"] = learn
    if serve:
        extra["serve"] = serve
    verb_tr, verb_en = ("pişir", "Cook") if station == POT else ("fırınla", "Bake")
    recipe("cook_" + did, station, did, mats, f"{tr} {verb_tr}", f"{verb_en} {en[0].lower() + en[1:] if not en.startswith('Gözleme') and not en.startswith('Köme') and not en.startswith('Tarhana') else en}",
           count, skill, level, 0.10 + 0.01 * level, 10 + 2 * level, **extra)
    eat = {"id": "eat_" + did, "displayName": f"{tr} {'iç' if drink else 'ye'}", "need": "Hunger", "restore": RESTORE[cls] + (level if not drink else 0),
           "durationDays": 0.03 if drink else 0.05, "consume": "item." + did, "bladder": 15 if drink else 4}
    if effect:
        eat["moodlet"] = effect
    if cls == "sweet":
        eat["extraRestore"] = [{"need": "Fun", "amount": 10}]
    act(POT, eat, f"{'Drink' if drink else 'Eat'} {en[0].lower() + en[1:] if not en[:6] in ('Gözlem', 'Köme (', 'Tarhan') else en}")
    NEW_DISHES.append(did)

# Risaleler (58 §4): gezgin tüccardan; okuyan o yemekleri öğrenir.
TREATISES = [
    ("treatise_table", "Sofra risalesi", "Treatise on the table", ["pigeon_stuffed", "goose_roast"]),
    ("treatise_waters", "Su ürünleri risalesi", "Treatise on river fare", ["eel_pie"]),
    ("treatise_sweets", "Tatlılar risalesi", "Treatise on sweets", ["cherry_tart"]),
]
for tid, tr, en, dishes in TREATISES:
    res(tid, tr, en, 40, ["writing", "merchantonly"])
    act(LECTERN, {"id": "study_" + tid, "displayName": f"{tr} oku", "durationDays": 0.12, "skill": "Literacy", "skillXp": 8, "meal": "studyTreatise",
                  "consume": "item." + tid, "teaches": ["item." + d for d in dishes]}, f"Study the {en[0].lower() + en[1:]}")

# ---------------------------------------------------------------------------------------------------------------------
# 6. Yazma
# ---------------------------------------------------------------------------------------------------------------------
ITEMS.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="")

# Konuşma satırları (60b §3.3, 60c §2): life.json'a tek satırlık kayıtlar; metinler ui.cooking.<dil>.json'da.
T = []   # (kimlik, etiketler, [(tr, en), ...], ek)


def talk(tid, tags, *lines, **kw):
    T.append((tid, tags.split(), list(lines), kw))


W = "work "
talk("hold", W + "any", ("Şunu tutuversene.", "Hold this for me."), ("Tuttum.", "Got it."))
talk("more", W + "any", ("Biraz daha.", "A bit more."), ("Yeter mi?", "Enough?"), ("Hah, tamam.", "There, that's it."))
talk("quick", W + "any", ("Elin ne çabuk!", "What quick hands!"), ("Alıştık artık.", "Used to it by now."))
talk("rest", W + "any", ("Yoruldun mu?", "Tired?"), ("Bitsin, öyle dinlenirim.", "I'll rest when it's done."))
talk("two", W + "any", ("İki kişi ne çabuk bitiyor.", "Two make short work of it."), ("Hep böyle yapalım.", "Let's always do it this way."))
talk("salt", W + "kitchen", ("Tuzuna bak bakalım.", "Check the salt."), ("Bir tutam daha.", "A pinch more."))
talk("onion", W + "kitchen", ("Soğan gözümü yaktı.", "The onion stings my eyes."), ("Ağlama, pişince geçer.", "Don't cry, it passes once it's cooked."))
talk("scorch", W + "kitchen", ("Dibi tutmasın.", "Don't let it catch."), ("Karıştırıyorum, karıştırıyorum.", "I'm stirring, I'm stirring."))
talk("lid", W + "kitchen", ("Mis gibi koktu.", "Smells wonderful."), ("Daha dur, kapağı açma.", "Wait, leave the lid on."))
talk("mother", W + "kitchen", ("Bunu annem böyle yapmazdı.", "My mother never made it this way."), ("Annen de burada değil.", "Your mother isn't here, is she."))
talk("soil", W + "farm", ("Toprak bu yıl iyi.", "The soil is good this year."), ("Yağmur da yağarsa…", "If only the rain comes…"))
talk("back", W + "farm", ("Belim koptu.", "My back is broken."), ("Daha yarısındayız.", "We're only halfway."))
talk("ears", W + "farm", ("Şu başaklara bak!", "Look at those ears!"), ("Gözümüz aydın.", "A sight for sore eyes."))
talk("crows", W + "farm", ("Kargalar yine gelmiş.", "The crows are back."), ("Korkuluğu düzeltelim.", "Let's fix the scarecrow."))
talk("grumpy", W + "animal", ("Bu yine huysuz bugün.", "This one is grumpy again today."), ("Sen sert tutuyorsun.", "You're holding too hard."))
talk("milk", W + "animal", ("Sütü bollaşmış.", "She's giving more milk."), ("İyi bakınca veriyor.", "Look after them and they give."))
talk("name", W + "animal", ("Adını ne koyalım?", "What shall we name it?"), ("Sen koy.", "You choose."))
talk("measure", W + "craft", ("Ölçtün mü?", "Did you measure?"), ("İki kere.", "Twice."))
talk("straight", W + "craft", ("Düz tut, eğri gidiyor.", "Hold it straight, it's going crooked."), ("Senin gözün eğri.", "Your eye is crooked."))
talk("fine", W + "craft", ("Güzel oldu ha?", "Came out well, eh?"), ("Ustasına sor.", "Ask the master."))
talk("shoulder", W + "teach", ("Bak, bileğinle değil, omzunla.", "Look: not the wrist, the shoulder."), ("Hımm, şimdi oldu.", "Hmm, now it works."))
talk("no_rush", W + "teach", ("Acele etme, iş kaçmaz.", "Don't rush, the work won't run off."), ("Sen yaparken kolay duruyor.", "It looks easy when you do it."))
talk("learned", W + "teach", ("Ben de böyle öğrendim.", "This is how I learned too."), ("Kimden?", "From whom?"), ("Rahmetli babamdan.", "From my late father."))
talk("undo", W + "teach", ("Yanlış olmuş, bozalım.", "That's wrong, let's undo it."), ("Baştan mı?!", "From the start?!"))
talk("quiet", W + "spouse", ("Bugün pek sessizsin.", "You're quiet today."), ("Seni dinliyorum.", "I'm listening to you."))
talk("wood", W + "spouse", ("Kışa odun yetecek mi?", "Will the wood last the winter?"), ("Yeter, sen üzülme.", "It will, don't you worry."))
talk("faster", W + "spouse", ("Eskiden daha hızlıydın.", "You used to be faster."), ("Eskiden sen de daha az konuşurdun.", "You used to talk less."))
talk("why", W + "parentchild", ("Neden böyle yapıyoruz?", "Why do we do it this way?"), ("Çünkü dedem böyle yapardı.", "Because my grandfather did."))
talk("may_i", W + "parentchild", ("Ben de yapabilir miyim?", "Can I try too?"), ("Gel, elini ver.", "Come, give me your hand."))
talk("grow_up", W + "parentchild", ("Büyüyünce ben de usta olacağım.", "I'll be a master too when I grow up."), ("Olursun, olursun.", "You will, you will."))
talk("heavy", W + "sibling", ("Hep ağır tarafı bana veriyorsun.", "You always give me the heavy end."), ("Büyük sensin.", "You're the older one."))
talk("tell_mum", W + "sibling", ("Anneme söylerim.", "I'm telling mother."), ("Söyle.", "Go on, then."))
talk("lively", W + "cheerful", ("Bugün içim kıpır kıpır.", "I'm all lively today."), ("Belli, ıslığından.", "I can tell from the whistling."))
talk("eyes", W + "tired", ("Gözlerim kapanıyor.", "My eyes are closing."), ("Bitirelim, yatarsın.", "Let's finish, then you sleep."))
talk("knee", W + "weather", ("Hava dönecek, dizim sızlıyor.", "The weather will turn, my knee aches."), ("Senin diz hiç yanılmaz.", "Your knee is never wrong."))
talk("festival", W + "weather", ("Şenliğe ne giyeceksin?", "What will you wear to the festival?"), ("Önce şu iş bitsin.", "Let's finish this first."))
talk("storm", W + "weather", ("Dünkü fırtınada çatı uçacak sandım.", "I thought the roof would fly off last night."), ("Ben hiç uyumadım.", "I didn't sleep a wink."))
talk("baby", W + "weather", ("Bebek yine ağladı mı gece?", "Did the baby cry again last night?"), ("Sorma.", "Don't ask."))
talk("sour1", W + "sour", ("Sen karışma, ben yaparım.", "Stay out of it, I'll do it."), ("İyi, yap.", "Fine. Do it."))
talk("sour2", W + "sour", ("Yine mi sen?", "You again?"), ("Başka kimse yok.", "There's no one else."))
talk("meme", W + "any", ("Bu iş bizden sorulur.", "This is what we were born for."), ("Efsaneyiz.", "Absolute legends."), rare=True)
S1 = "smell_good "
talk("sg1", S1 + "anyone", ("Offff, çok iyi kokuyor!", "Ohhh, that smells so good!"), lines=1)
talk("sg2", S1 + "anyone", ("Bu ne güzel koku böyle?", "What is that lovely smell?"), lines=1)
talk("sg3", S1 + "anyone", ("Karnım guruldadı.", "My stomach just growled."), lines=1)
talk("sg4", S1 + "anyone", ("Bugün ne pişiyor?", "What's cooking today?"), lines=1)
talk("sg_child1", S1 + "child", ("Ne zaman yiyeceğiz?", "When do we eat?"), lines=1)
talk("sg_child2", S1 + "child", ("Acıktım!", "I'm hungry!"), lines=1)
talk("sg_spouse", S1 + "spouse", ("Bu kokuya işten kaçılır.", "A smell worth skipping work for."), lines=1)
talk("sg_elder", S1 + "elder", ("Annemin mutfağı böyle kokardı.", "My mother's kitchen smelled like this."), lines=1)
talk("sg_villager", S1 + "villager", ("Kimin evi bu? Ne güzel kokuyor.", "Whose house is this? It smells wonderful."), lines=1)
talk("sr1", "smell_reply_good", ("Sabret, az kaldı.", "Patience, nearly there."), lines=1)
talk("sr2", "smell_reply_good", ("Kapağı açma!", "Leave the lid alone!"), lines=1)
talk("sr3", "smell_reply_good", ("Sofrayı kurun bari.", "Lay the table, at least."), lines=1)
S2 = "smell_bad "
talk("sb1", S2 + "anyone", ("Bir şey mi yandı?", "Is something burning?"), lines=1)
talk("sb2", S2 + "anyone", ("Bu koku da ne?", "What is that smell?"), lines=1)
talk("sb3", S2 + "anyone", ("Pencereyi açın!", "Open the window!"), lines=1)
talk("sb_child", S2 + "child", ("Bunu yemek zorunda mıyım?", "Do I have to eat that?"), lines=1)
talk("sb_polite", S2 + "polite", ("Eline sağlık…", "Bless your hands…"), lines=1)
talk("sbr1", "smell_reply_bad", ("Dibi tutmuş biraz…", "It caught a little…"), lines=1)
talk("sbr2", "smell_reply_bad", ("Bir daha olmaz.", "It won't happen again."), lines=1)
talk("cat1", "cat_steal anyone", ("Hey!", "Hey!"), lines=1)
talk("cat2", "cat_steal anyone", ("Yine mi?!", "Again?!"), lines=1)
talk("cat3", "cat_steal anyone", ("Gözümün önünde…", "Right in front of me…"), lines=1)
talk("cat_child", "cat_steal child", ("Hahaha, kaptı!", "Hahaha, got it!"), lines=1)
talk("cat_owner1", "cat_steal owner", ("O benim balığımdı!", "That was my fish!"), lines=1)
talk("cat_owner2", "cat_steal owner", ("{pet}! Bırak onu!", "{pet}! Drop it!"), lines=1)
talk("dog1", "dog_scrap", ("Al bakalım, kimseye söyleme.", "Here you go, tell no one."), lines=1)
talk("dog2", "dog_scrap", ("Bu son, {pet}.", "This is the last one, {pet}."), lines=1)
talk("imit1", "imitate", ("Bak, ben de yapıyorum!", "Look, I'm doing it too!"), lines=1)
talk("imit2", "imitate", ("Ben ustayım!", "I'm the master!"), lines=1)
talk("idle1", "idle anyone", ("Hava da ne güzel…", "What fine weather…"), lines=1)
talk("idle2", "idle anyone", ("Bugün ne yapsam?", "What shall I do today?"), lines=1)
talk("idle3", "idle yawn", ("Esnemekten çenem yoruldu.", "My jaw is tired from yawning."), lines=1)
talk("idle4", "idle window", ("Yol bugün pek sakin.", "The road is quiet today."), lines=1)

life = LIFE.read_text(encoding="utf-8")
nl = "\r\n" if "\r\n" in life else "\n"
body = re.sub(r',\s*"talk": \[.*?\n  \]', "", life.rstrip(), flags=re.S)
assert body.endswith("}")
body = body[:-1].rstrip()
rows = []
for tid, tags, lines, kw in T:
    row = {"id": tid, "tags": tags, "lines": kw.get("lines", len(lines))}
    if kw.get("rare"):
        row["rare"] = True
    rows.append("    " + json.dumps(row, ensure_ascii=False))
LIFE.write_text(body + "," + nl + '  "talk": [' + nl + ("," + nl).join(rows) + nl + "  ]" + nl + "}" + nl, encoding="utf-8", newline="")
json.loads(LIFE.read_text(encoding="utf-8"))

# İngilizce içerik tablosu: yeni anahtarlar; domates ekininin etkileşimleri sarımsağınkinden kopyalanır.
path = LOC / "content.en.json"
en_table = json.loads(path.read_text(encoding="utf-8"))
for src, dst in COPY_EN:
    for k, v in list(en_table.items()):
        if k.startswith(src + ".act."):
            EN.setdefault(dst + k[len(src):], v)
EN["item.cooking_pot.act.roast_meat"] = "Roast and eat beef"
EN["item.cooking_pot.act.roast_poultry"] = "Roast and eat chicken"
EN["item.cooking_pot.act.roast_game"] = "Roast and eat venison"
for k in list(en_table):
    if ".act.slaughter" in k and not k.startswith("item.slaughter_hook") or k == "recipe.render_tallow.name":
        del en_table[k]
_sieve_en = en_table.get("item.seed_maker.act.sieve_garlic", "Sieve seed: Garlic")
EN["item.seed_maker.act.sieve_tomato"] = _sieve_en.replace("Garlic", "Tomato").replace("garlic", "tomato")
en_table.update(EN)
path.write_text(json.dumps(dict(sorted(en_table.items())), ensure_ascii=False, indent=2), encoding="utf-8", newline="")

# Konuşma metinleri arayüz alan dosyasına (ui.cooking.<dil>.json); öteki ui.cooking anahtarları korunur (yemek_ui.py yazar).
for lang, col in (("tr", 0), ("en", 1)):
    p = LOC / f"ui.cooking.{lang}.json"
    table = json.loads(p.read_text(encoding="utf-8")) if p.exists() else {}
    table = {k: v for k, v in table.items() if not k.startswith("talk.")}
    for tid, tags, lines, kw in T:
        for n, pair in enumerate(lines):
            table[f"talk.{tid}.{'abc'[n]}"] = pair[col]
    p.write_text(json.dumps(dict(sorted(table.items())), ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="")

meals = len({d for ds in CLASS.values() for d in ds}) + len([d for d in D if d[3] != "drink"])
print(f"{len(GROUPS)} grup, {len(SPICES)} baharat, {len(D)} yeni yemek (toplam {meals} öğün), {len(T)} konuşma satırı, {len(EN)} İngilizce metin.")
