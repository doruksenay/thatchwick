#!/usr/bin/env python3
"""
Eşya yerleşim metadata'sı (belge 44, Aşama B): items.json'a görsel (yön/eksen, pivot, görsel boyutu), ön yüz,
iç/dış, ait olduğu oda türleri, meslek etiketleri ve ilişki kuralı yazar; dükkân şablonlarının eksik eşyalarını
(dönem kurallarına uygun) ekler. Var olan alanlara dokunmaz, yalnızca bu alanları yeniden kurar (tekrar çalıştırılabilir).

Görseller: IsoArt'ın eski el tablosu (kabul edilmiş eşlemeler, aynen) + Assets/Art/Iso(_aday) altındaki eşlenmemiş
görseller (reddedilen işlerin çıktıları hariç; pivot ve boyut art/results tablolarından). Eksik değerler kategori
varsayılanıyla doldurulur; emin olunamayanlar sonda listelenir.

Çalıştırma: python tools/layout/item_meta.py [--check]
"""
import glob
import json
import os
import re
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
ITEMS = os.path.join(ROOT, "Assets", "Resources", "Content", "items.json")

# ---------------------------------------------------------------- yeni eşyalar (Ek tablosu, dönem: ortaçağ öncesi icat yok)
NEW_ITEMS = [
    # id, ad, w, h, kategori, fiyat, oda (menü), stil
    ("item.shop_counter", "Satış tezgâhı", 2, 1, "surfaces", 30, ["living"], "Tüccar"),
    ("item.anvil", "Örs", 1, 1, "work", 60, ["outdoor"], "Zanaatkâr"),
    ("item.bellows", "Körük", 1, 1, "work", 25, ["outdoor"], "Zanaatkâr"),
    ("item.quench_tub", "Su teknesi (demirci)", 1, 1, "work", 12, ["outdoor"], "Zanaatkâr"),
    ("item.iron_pile", "Nal ve demir yığını", 1, 1, "work", 10, ["outdoor"], "Zanaatkâr"),
    ("item.lumber_pile", "Kereste yığını", 2, 1, "storage", 14, ["outdoor"], "Köylü"),
    ("item.unfinished_furniture", "Yarım kalmış dolap", 1, 1, "work", 0, ["outdoor"], "Zanaatkâr"),
    ("item.kneading_trough", "Hamur teknesi", 2, 1, "kitchen", 22, ["kitchen"], "Köylü"),
    ("item.flour_sack", "Un çuvalı", 1, 1, "storage", 6, ["kitchen"], "Köylü"),
    ("item.grain_sack", "Tahıl çuvalı", 1, 1, "storage", 6, ["kitchen"], "Köylü"),
    ("item.bread_rack", "Ekmek rafı", 1, 1, "kitchen", 18, ["kitchen"], "Köylü"),
    ("item.writing_lectern", "Yazı kürsüsü (rahle)", 1, 1, "work", 30, ["living"], "Tüccar"),
    ("item.scroll_rack", "Rulo rafı", 1, 1, "storage", 24, ["living"], "Tüccar"),
    ("item.scroll_chest", "Rulo sandığı", 1, 1, "storage", 20, ["living"], "Tüccar"),
    ("item.cutting_table", "Kesim masası", 2, 1, "surfaces", 26, ["living"], "Zanaatkâr"),
    ("item.cloth_bolt_shelf", "Kumaş topları rafı", 2, 1, "storage", 28, ["living"], "Zanaatkâr"),
    ("item.garment_stand", "Elbise askısı", 1, 1, "storage", 12, ["bedroom"], "Zanaatkâr"),
    ("item.yarn_basket", "İplik ve yumak sepeti", 1, 1, "decor", 6, ["living"], "Köylü"),
    ("item.clay_trough", "Kil teknesi", 1, 1, "work", 10, ["outdoor"], "Köylü"),
    ("item.pot_drying_rack", "Kuruyan çömlek rafı", 2, 1, "work", 16, ["outdoor"], "Köylü"),
    ("item.fish_basket", "Balık sepeti", 1, 1, "storage", 6, ["kitchen"], "Köylü"),
    ("item.hide_rack", "Deri askısı", 1, 1, "work", 14, ["outdoor"], "Zanaatkâr"),
    ("item.stone_blocks", "Taş blokları", 1, 1, "work", 8, ["outdoor"], "Köylü"),
    ("item.wicker_basket", "Hasır sepet", 1, 1, "storage", 5, ["kitchen"], "Köylü"),
    ("item.bedside_stand", "Başucu sehpası", 1, 1, "storage", 12, ["bedroom"], "Köylü"),
    ("item.wash_tub", "Çamaşır leğeni", 1, 1, "hygiene", 8, ["outdoor"], "Köylü"),
    # dış mekân ve bahçe (Aşama D)
    ("item.shop_sign", "Dükkân tabelası", 1, 1, "village", 0, ["outdoor"], None),
    ("item.signpost", "Yol tabelası", 1, 1, "village", 0, ["outdoor"], None),
    ("item.notice_post", "İlan tahtası", 1, 1, "village", 0, ["outdoor"], None),
    ("item.clothes_line", "Çamaşır ipi", 2, 1, "garden", 8, ["outdoor"], "Köylü"),
    ("item.hitching_post", "At bağlama direği", 1, 1, "garden", 10, ["outdoor"], "Köylü"),
    ("item.cloth_drying_line", "Kurumaya asılmış boyalı kumaşlar", 2, 1, "garden", 0, ["outdoor"], None),
    ("item.net_drying_rack", "Ağ kurutma askısı", 2, 1, "garden", 0, ["outdoor"], None),
    ("item.pottery_display", "Çömlek sergisi", 1, 1, "village", 0, ["outdoor"], None),
    ("item.smith_display", "Nal ve alet sergisi", 1, 1, "village", 0, ["outdoor"], None),
    ("item.flower_clump", "Çiçek öbeği", 1, 1, "garden", 4, ["outdoor"], "Köylü"),
    ("item.shrub", "Çalı", 1, 1, "garden", 0, ["outdoor"], None),
    ("item.reeds", "Saz öbeği", 1, 1, "garden", 0, ["outdoor"], None),
    ("item.grass_tuft", "Çim öbeği", 1, 1, "garden", 0, ["outdoor"], None),
]
# Çim öbeği ve çakıl yolu kapatmasın: halı gibi serilir (yürünebilir).
NEW_RUG_LIKE = {"item.grass_tuft"}
NEW_SURFACE = [
    ("item.ink_quill", "Mürekkep hokkası ve tüy kalem", "decor", 6),
    ("item.wax_seal", "Mühür ve mum", "decor", 5),
    ("item.letter_box", "Mektup kutusu", "decor", 8),
    ("item.shears_chalk", "Makas, ölçü ipi ve tebeşir", "decor", 6),
]
NEW_WALL = [
    ("item.chisel_rack", "Keski askısı", "work", 14),
]

# ---------------------------------------------------------------- ön yüz
FRONT = {
    "sit": "chair_wood chair_carved chair_painted armchair_cushion x_chair throne_chair child_chair bench_long bench_wood han_bench "
           "garden_bench bleached_bench settle_bench chest_bench sedir sedir_long cushioned_divan",
    "open": "chest_wood chest_iron carved_chest dowry_chest bed_end_chest treasure_chest toy_chest spice_chest shelf_wood pantry_shelf "
            "potion_shelf aumbry clothes_press yukluk ice_box scroll_rack cloth_bolt_shelf bread_rack scroll_chest bedside_stand",
    "work": "hearth_stone stone_oven fireplace_large forge workbench saddler_bench stonecutter loom spinning_wheel pottery_wheel glass_bench "
            "glass_furnace chandler_table prep_counter alembic kiln writing_lectern cutting_table anvil kneading_trough "
            "market_stall seed_maker bloomery",
    "stand": "basin_ewer outhouse bath_hut flush_privy kitchen_spout",
}
FRONT_OF = {f"item.{n}": use for use, names in FRONT.items() for n in names.split()}
# Yalnızca bu dönüşler (görsel tek yöne çıkar): merdivenler ve kiler kapağı arkaya doğru yükselir (dönüş 1 = +x, aynası 0 = +y).
ONLY_ROT = {f"item.{n}": [0, 1] for n in "stairs_up stairs_down stairs_up_stone stairs_down_stone cellar_hatch ladder".split()}
# Sol/sağ simetrik olmayan (aynası yanlış görünür) eşya yok sayıldı; şüpheliler sonda listelenir.
NO_MIRROR = set()

# ---------------------------------------------------------------- iç/dış
OUTDOOR_CATS = {"plant", "forage", "farm", "garden"}
OUTDOOR_IDS = set(f"item.{n}" for n in (
    "outhouse bath_hut woodshed granary ice_house smokehouse tanning_rack lime_kiln kiln sawmill water_wheel bonfire well "
    "market_stall ox_cart cellar_hatch kite hoop chopping_block compost_heap scarecrow threshing_floor threshing_sledge "
    "charcoal_clamp roasting_heap quarry pond fish_trap hunting_blind weather_vane glass_furnace rain_barrel cistern fountain "
    "water_tower underground_cistern noria windmill fulling_mill trip_hammer drain_pit olive_crusher olive_press grape_tub "
    "wash_tub clothes_line hitching_post cloth_drying_line net_drying_rack pottery_display smith_display flower_clump shrub "
    "reeds grass_tuft shop_sign signpost notice_post").split())
INDOOR_IN_OUTCAT = {"item.cat_basket", "item.mushroom_bed"}
ANY_IDS = set(f"item.{n}" for n in (
    "forge bloomery workbench whetstone stonecutter loom spinning_wheel pottery_wheel dye_vat saddler_bench seed_maker alembic "
    "brew_barrel chandler_table churn sieve glass_bench barrel storage_jar seed_crate water_barrel water_bucket feed_trough "
    "water_trough hay_bale barn_lantern animal_bell milking_stool saddle_rack stump_table bench_wood stool_wood barrel_table lute "
    "rope_coil ice_box salting_barrel quern millstone harvest_heap stairs_up stairs_down stairs_up_stone stairs_down_stone ladder "
    "towel_rack brazier firewood_basket anvil bellows quench_tub iron_pile lumber_pile unfinished_furniture clay_trough "
    "hide_rack stone_blocks wicker_basket grain_sack flour_sack fish_basket beehive root_cellar collection_box feed_chute pot_drying_rack "
    "nest_box stall pig_pen horse_stall salt_lick feed_mixer water_gate canal irrigation_ditch bucket_pulley water_channel "
    "coop barn pigsty kennel granary").split()) - OUTDOOR_IDS
WALL_ANY = set(f"item.{n}" for n in "notice_board door_wreath name_plaque horseshoe_display torch_wall oil_lamp_wall spring_garland winter_greenery".split())

# ---------------------------------------------------------------- her yer (bilinçli): ışıklar ve merdivenler
ANYWHERE = set(f"item.{n}" for n in (
    "candlestick torch_wall oil_lamp_wall sconce_brass hanging_lantern iron_chandelier brass_lantern barn_lantern brazier "
    "stairs_up stairs_down stairs_up_stone stairs_down_stone ladder notice_board").split())

# ---------------------------------------------------------------- meslek etiketleri
TRADES = {
    "tavern": "hearth_stone copper_pots prep_counter brew_barrel barrel trestle_table han_table han_bench bench_long tankard_rack "
              "plate_rack singer_stand cooking_pot firewood_basket barrel_table storage_jar pantry_shelf bench_wood spoon_rack "
              "settle_bench chair_wood stool_wood green_bottle_shelf peg_rail ceiling_herbs beam_hook",
    "general": "shop_counter pantry_shelf storage_jar seed_crate barrel wicker_basket grain_sack flour_sack chest_wood shelf_wood "
               "firewood_basket market_stall stool_wood wall_shelf wall_shelf_oak long_shelf_iron peg_rail",
    "scribe": "writing_lectern shelf_wood scroll_rack scroll_chest chest_wood table_painted table_wood chair_carved chair_wood "
              "stool_wood rug_runner wall_shelf wall_shelf_oak notice_board",
    "herb": "potion_shelf drying_rack table_wood bed_straw basin_ewer alembic shelf_wood ceiling_herbs herb_bundle_wall stool_wood "
            "chest_wood beam_hook wall_shelf storage_jar",
    "smithy": "forge bloomery anvil bellows quench_tub whetstone iron_pile trip_hammer tool_rack_wall water_barrel firewood_basket "
              "barrel horseshoe_display",
    "carpenter": "workbench lumber_pile unfinished_furniture tool_rack_wall chair_wood chair_carved chair_painted table_wood "
                 "table_painted chest_bench chest_wood settle_bench bench_wood stool_wood x_chair firewood_basket sawmill "
                 "dowry_chest carved_chest bed_end_chest shelf_wood peg_rail",
    "mill": "millstone quern stone_oven kneading_trough flour_sack grain_sack bread_rack seed_crate barrel table_wood "
            "prep_counter firewood_basket water_wheel",
    "tailor": "loom spinning_wheel dye_vat clothes_press cutting_table cloth_bolt_shelf garment_stand yarn_basket chest_wood "
              "stool_wood felt_rug felt_rug_small felt_rug_large peg_rail wall_shelf",
    "potter": "pottery_wheel kiln clay_trough pot_drying_rack storage_jar shelf_wood barrel_tub stool_wood wall_shelf",
    "fisher": "fish_basket salting_barrel barrel oar_rack net_curtain dried_fish_string table_wood shelf_wood fish_trap stool_wood "
              "first_fish_scale",
    "saddler": "saddler_bench saddle_rack saddle_peg hide_rack tool_rack_wall table_wood barrel barrel_tub tanning_rack stool_wood",
    "mason": "stonecutter stone_blocks chisel_rack workbench whetstone table_wood barrel lime_kiln",
    "animals": "feed_trough water_trough hay_bale milking_stool saddle_rack barn_lantern animal_bell salt_lick",
}
TRADES_OF = {}
for t, names in TRADES.items():
    for n in names.split():
        TRADES_OF.setdefault(f"item.{n}", []).append(t)

# ---------------------------------------------------------------- oda türleri
ROOM_MAP = {"bedroom": ["bedroom", "inn_room"], "kitchen": ["kitchen"], "living": ["living"], "kids": ["kids"],
            "outdoor": ["outdoor"], "barn": ["barn"], "cellar": ["cellar"], "pantry": ["pantry"]}
EXTRA_ROOMS = {
    "seating": ["dining", "living"], "surfaces": ["dining"],
}
STORE_ROOMS = set(f"item.{n}" for n in "barrel storage_jar seed_crate salting_barrel brine_crock pantry_shelf grain_sack flour_sack "
                  "wicker_basket chest_wood chest_iron ice_box brew_barrel honey_strainer root_cellar fish_basket".split())
INN_ROOM = set(f"item.{n}" for n in "bed_straw bed_frame chest_wood basin_ewer chamber_pot stool_wood chair_wood bedside_stand "
               "rug_rush rush_mat felt_rug_small wall_shelf peg_rail".split())
NO_ROOMS_DEFAULT = {
    "item.chair_carved": ["kitchen", "dining", "living"], "item.bench_wood": ["dining", "living", "outdoor"],
    "item.game_table": ["living", "dining"], "item.salting_barrel": ["pantry", "cellar"], "item.market_stall": ["outdoor"],
    "item.root_cellar": ["outdoor"], "item.toy_wooden": ["kids", "living"],
}
NEAR_HEARTH = {"item.cooking_pot", "item.copper_pots", "item.firewood_basket", "item.cezve"}
HEARTH_TAG = {"item.hearth_stone", "item.stone_oven", "item.fireplace_large", "item.forge", "item.bloomery"}
FOCUS_TAG = {"item.singer_stand", "item.bonfire", "item.well", "item.fountain"}

# ---------------------------------------------------------------- görseller
def load_results():
    """art/results: dosya adı -> (genişlik, yükseklik, pivotX, pivotY)."""
    out = {}
    rx = re.compile(r"\|\s*`?(iso_[a-z0-9_]+?)(?:\.png)?`?\s*\|\s*(\d+)\s*[x×]\s*(\d+)\s*\|\s*(\d+(?:[.,]\d+)?)\s*(?:/\s*(\d+(?:[.,]\d+)?))?\s*\|\s*(\d+(?:[.,]\d+)?)?")
    for f in sorted(glob.glob(os.path.join(ROOT, "art", "results", "*.md"))):
        for line in open(f, encoding="utf-8"):
            m = rx.match(line.strip())
            if not m:
                continue
            name, w, h, px, py_slash, py = m.groups()
            pyv = py_slash or py
            if pyv is None:
                continue
            out[name] = (int(w), int(h), float(px.replace(",", ".")), float(pyv.replace(",", ".")))
    return out


def rejected_outputs():
    bad = set()
    for f in glob.glob(os.path.join(ROOT, "art", "queue", "*.md")):
        txt = open(f, encoding="utf-8").read()
        m = re.search(r"^status:\s*(\w+)", txt, re.M)
        if m and m.group(1) == "rejected":
            for o in re.findall(r"^\s*-\s*(Assets/\S+\.png)", txt, re.M):
                bad.add(os.path.splitext(os.path.basename(o))[0])
    return bad


def parse_isoart():
    # Eski el tablosu IsoArt.cs'ten items.json'a taşındı (Aşama B). Taşıma bir kez yapılır; sonraki çalıştırmalarda
    # items.json'daki "art" alanları korunur, yalnızca görseli olmayan eşyalar için yeni görseller aranır.
    src = open(os.path.join(ROOT, "Assets", "Scripts", "Game", "IsoArt.cs"), encoding="utf-8").read()
    table = {}
    for m in re.finditer(r'\["(item\.[a-z0-9_]+)"\] = new ItemArt \{([^}]*)\}', src):
        body = m.group(2)
        g = lambda k: (re.search(k + r' = "([^"]+)"', body) or [None, None])[1]
        n = lambda k: float(re.search(k + r" = ([\d.]+)", body).group(1))
        table[m.group(1)] = {"alongX": g("AlongX"), "alongY": g("AlongY"), "lit": g("Lit"), "pivotX": n("PivotX"), "pivotY": n("PivotY")}
    plants = dict(re.findall(r'\["(item\.[a-z0-9_]+)"\] = "(iso_tree_[a-z0-9_]+)"', src))
    return table, plants


def axis_of(name):
    """Görselin uzun ekseni: x ("/", sw_ne) ya da y ("\\", se/se_nw); tek karoda None."""
    if re.search(r"_\dx\d", name) is None and "_2w" not in name:
        return None
    return "x" if "sw_ne" in name else "y"


def default_pivot(w, h, placement):
    if placement == "Surface":
        return 16, 7
    if placement == "Wall":
        return (48, 32) if w > 1 else (32, 32)
    big = max(w, h)
    return {1: (40, 24), 2: (56, 32) if w * h == 2 else (72, 40), 3: (72, 40) if w * h == 3 else (88, 48)}.get(big, (104, 56))


def scan_sprites(bad):
    files = set()
    for d in ("Assets/Art/Iso", "Assets/Art/Iso/_aday"):
        for f in os.listdir(os.path.join(ROOT, d)):
            if f.endswith(".png"):
                files.add(os.path.splitext(f)[0])
    files -= bad
    return files


def version(name):
    m = re.search(r"_v(\d+)$", name)
    return int(m.group(1)) if m else 1


def main():
    check = "--check" in sys.argv
    raw = open(ITEMS, encoding="utf-8").read()
    data = json.loads(raw)
    items = data["items"]
    by_id = {i["id"]: i for i in items}
    results = load_results()
    bad = rejected_outputs()
    sprites = scan_sprites(bad)
    all_files = scan_sprites(set())          # eski tablo kullanıcı onaylı eşlemedir: red listesine bakılmaz
    table, plants = parse_isoart()
    uncertain = []

    # ---- yeni eşyalar
    for (iid, name, w, h, cat, price, rooms, style) in NEW_ITEMS:
        if iid in by_id:
            continue
        d = {"id": iid, "displayName": name, "width": w, "height": h, "price": price, "category": cat, "rooms": rooms, "level": 3}
        if style:
            d["styleSet"] = style
        if price == 0:
            d["tags"] = ["townonly"]
        if iid in NEW_RUG_LIKE:
            d["placement"] = "Rug"
        if cat == "surfaces":
            d["surfaceSlots"] = [{"id": "a", "u": 0.3, "v": 0.5, "size": "M"}, {"id": "b", "u": 0.7, "v": 0.5, "size": "M"}]
            d["maxLoad"] = 16
        items.append(d); by_id[iid] = d
    for (iid, name, cat, price) in NEW_SURFACE:
        if iid not in by_id:
            d = {"id": iid, "displayName": name, "placement": "Surface", "price": price, "category": cat, "decorSize": "S",
                 "fitsOn": ["masa", "raf"], "beauty": 1, "level": 3}
            items.append(d); by_id[iid] = d
    for (iid, name, cat, price) in NEW_WALL:
        if iid not in by_id:
            d = {"id": iid, "displayName": name, "placement": "Wall", "price": price, "category": cat, "level": 3}
            items.append(d); by_id[iid] = d

    # ---- görsel eşlemesi: önce eski tablo (aynen), sonra eşlenmemiş görseller
    art = {}
    for iid, a in table.items():
        a = {k: v for k, v in a.items() if v is not None}
        missing = [v for k, v in a.items() if isinstance(v, str) and v not in all_files]
        if missing:
            uncertain.append(f"{iid}: eski tablodaki görsel henüz yok ({', '.join(missing)}), eşlenmedi")
            if all(isinstance(v, str) and v in missing for k, v in a.items() if isinstance(v, str)):
                continue
            a = {k: v for k, v in a.items() if v not in missing}
        art[iid] = a
    used = set(v for a in art.values() for k, v in a.items() if isinstance(v, str))
    for iid, name in plants.items():
        art.setdefault(iid, {})["grown"] = name
        used.add(name)
    cands = {}
    for s in sorted(sprites - used):
        m = re.match(r"^iso_(item|wall_item)_(.+?)(?:_(\d)x(\d))?(?:_(\d)w)?(_lit)?_(se_nw|sw_ne|se|sw)(?:_v(\d+))?$", s)
        if m:
            kind, short, _, _, _, lit, _, _ = m.groups()
            if re.search(r"_(b|f)$", short):          # alternatif aday (bloomery_b ...): ana aday seçilir
                continue
            iid = "item." + short
            if iid in by_id:
                cands.setdefault((iid, "lit" if lit else "main"), []).append(s)
            continue
        m = re.match(r"^iso_(tree|plant)_([a-z]+)_(young|grown|fruit)_se(?:_v(\d+))?$", s)
        if m:
            kind, short, stage, _ = m.groups()
            iid = {"coffee": "item.bush_coffee", "grape": "item.vine_grape", "olive": "item.tree_olive"}.get(short, "item.tree_" + short)
            if iid in by_id:
                cands.setdefault((iid, stage), []).append(s)
            continue
        m = re.match(r"^iso_forage_([a-z_]+?)(?:_v(\d+))?_se$", s)
        if m:
            iid = "item.forage_" + m.group(1)
            if iid in by_id:
                cands.setdefault((iid, "main"), []).append(s)
    for (iid, slot), names in cands.items():
        name = max(names, key=version)
        d = by_id[iid]
        if d.get("art"):
            continue
        a = art.setdefault(iid, {})
        if slot in ("young", "grown", "fruit"):
            a.setdefault(slot, name)
            continue
        if slot == "lit":
            a.setdefault("lit", name)
            continue
        if a.get("alongX") or a.get("alongY"):
            continue                                   # eski tabloda eşlenmiş: dokunma
        w, h = d.get("width", 1) or 1, d.get("height", 1) or 1
        placement = d.get("placement") or "Floor"
        dm = re.search(r"_(\d)x(\d)", name)
        sw, sh = (int(dm.group(1)), int(dm.group(2))) if dm else (1, 1)
        if placement not in ("Wall", "Surface") and (sw * sh != w * h or max(sw, sh) != max(w, h)):
            uncertain.append(f"{iid}: görsel {name} {sw}x{sh}, ayak izi {w}x{h}: eşlenmedi (görsel boyutu ile ayak izi uyuşmuyor)")
            continue
        ax = axis_of(name)
        if ax is None or w == h or placement in ("Wall", "Surface"):
            a["alongX"] = name
        else:
            even_axis = "x" if w > h else "y"
            a["alongX" if ax == even_axis else "alongY"] = name
        if name in results:
            pw, ph, px, py = results[name]
            a["pivotX"], a["pivotY"], a["pxW"], a["pxH"] = px, py, pw, ph
        else:
            px, py = default_pivot(w, h, placement)
            a["pivotX"], a["pivotY"] = px, py
            uncertain.append(f"{iid}: {name} pivotu raporda yok, varsayılan {px},{py}")
    # Eski tablonun görsel boyutları raporlardan
    for iid, a in art.items():
        n = a.get("alongX") or a.get("alongY")
        if n and n in results and "pxW" not in a:
            a["pxW"], a["pxH"] = results[n][0], results[n][1]

    # ---- metadata
    for d in items:
        iid = d["id"]
        placement = d.get("placement") or "Floor"
        if placement == "Resource":
            continue
        cat = d.get("category") or "furniture"
        kept_art = d.get("art")
        for k in ("art", "front", "frontDepth", "site", "belongs", "trades", "anywhere", "relation"):
            d.pop(k, None)
        # görsel: items.json'da zaten varsa korunur (elle düzeltilmiş olabilir)
        a = None if kept_art else art.get(iid)
        if kept_art:
            d["art"] = kept_art
        if a:
            a = dict(a)
            a["mirror"] = iid not in NO_MIRROR
            front = FRONT_OF.get(iid)
            if front:
                # Ön yüzlü eşya: yön görselleri. Tek karoda _se → n, _sw → w; çok karoda "\\" ekseni → w, "/" ekseni → n.
                w, h = d.get("width", 1) or 1, d.get("height", 1) or 1
                for key in ("alongX", "alongY"):
                    n = a.pop(key, None)
                    if not n:
                        continue
                    if w == h:
                        a["w" if re.search(r"_sw(_v\d+)?$", n) else "n"] = n
                    else:
                        a["n" if axis_of(n) == "x" else "w"] = n
            if iid in ONLY_ROT:
                a["rotations"] = ONLY_ROT[iid]
            d["art"] = a
        # ön yüz ve ilişki
        if FRONT_OF.get(iid):
            d["front"] = FRONT_OF[iid]
            if d["front"] == "sit":
                d["relation"] = "faceTable"
        if iid in NEAR_HEARTH:
            d["relation"] = "nearHearth"
        if iid == "item.bedside_stand":
            d["relation"] = "bedside"
        tags = d.pop("tags", None) or []
        if iid in HEARTH_TAG and "hearth" not in tags:
            tags.append("hearth")
        if iid in FOCUS_TAG and "focus" not in tags:
            tags.append("focus")
        if tags:
            d["tags"] = tags
        # iç/dış
        if placement == "Wall":
            site = "any" if iid in WALL_ANY else "indoor"
        elif placement in ("Surface",):
            site = None
        elif placement in ("Rug", "Ceiling"):
            site = "outdoor" if iid in OUTDOOR_IDS else "indoor"
        elif iid in OUTDOOR_IDS or (cat in OUTDOOR_CATS and iid not in INDOOR_IN_OUTCAT and iid not in ANY_IDS):
            site = "outdoor"
        elif iid in ANY_IDS:
            site = "any"
        else:
            site = "indoor"
        if site:
            d["site"] = site
        # ait olduğu yerler
        if placement != "Surface" and cat not in ("plant", "forage"):
            if iid in ANYWHERE:
                d["anywhere"] = True
            else:
                rooms = d.get("rooms") or NO_ROOMS_DEFAULT.get(iid, [])
                b = []
                for r in rooms:
                    for x in ROOM_MAP.get(r, []):
                        if x not in b:
                            b.append(x)
                if "kitchen" in rooms or "living" in rooms:
                    for x in EXTRA_ROOMS.get(cat, []):
                        if x not in b:
                            b.append(x)
                if iid in STORE_ROOMS:
                    for x in ("pantry", "cellar", "storehouse"):
                        if x not in b:
                            b.append(x)
                if iid in INN_ROOM and "inn_room" not in b:
                    b.append("inn_room")
                if cat == "work" and "workshop" not in b:
                    b.append("workshop")
                if site == "outdoor" and "outdoor" not in b:
                    b.append("outdoor")
                if not b and not TRADES_OF.get(iid):
                    uncertain.append(f"{iid}: oda türü yok (rooms boş), kural dışı kalır")
                if b:
                    d["belongs"] = b
            if TRADES_OF.get(iid):
                d["trades"] = TRADES_OF[iid]
        # Sabit sıra (tekrar çalıştırınca aynı dosya): yerleşim alanları en sonda.
        for k in ("tags", "art", "front", "relation", "site", "belongs", "trades", "anywhere"):
            if k in d:
                d[k] = d.pop(k)
    unknown_trade = [i for i in TRADES_OF if i not in by_id]
    if unknown_trade:
        uncertain.append("meslek tablosunda tanımsız eşya: " + ", ".join(unknown_trade))

    out = json.dumps(data, indent=2, ensure_ascii=False) + "\n"
    if check:
        print("değişiklik var" if out != raw else "güncel")
        return 1 if out != raw else 0
    open(ITEMS, "w", encoding="utf-8").write(out)
    mapped = sum(1 for d in items if d.get("art"))
    print(f"eşya {len(items)}, görselli {mapped}, yeni eşlenen {len(cands)}")
    for u in uncertain:
        print("EMİN DEĞİL:", u)
    return 0


if __name__ == "__main__":
    sys.exit(main())
