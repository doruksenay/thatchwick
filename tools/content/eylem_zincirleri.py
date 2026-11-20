"""İzlenen eylem zincirleri (Project belgeleri 60, 60a-60c): life.json'a "chains" bölümünü ve adım metinlerini yazar.

Kullanım:  python tools/content/eylem_zincirleri.py
Yeniden çalıştırılabilir: var olan "chains" bölümünü ve "chain.step.*" anahtarlarını baştan yazar.

Zincir, işin süresini ve sonucunu değiştirmez; yalnızca görünür adımlara böler. Adımın karesi (pose) henüz yoksa
görünüm etkileşimin eski simgesine düşer, bu yüzden veriler görsellerden önce girebilir.
Eşleme sırası listedeki sıradır: ilk tutan zincir kazanır (özel olan üstte).
"""

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
LIFE = ROOT / "Assets/Resources/Content/life.json"
LOC = ROOT / "Assets/Resources/Localization"

# (kimlik, Türkçe, İngilizce). Arayüz anahtarı: chain.step.<kimlik>
LABELS = [
    # ortak
    ("gather", "Malzemeyi alıyor", "Getting the ingredients"),
    ("kindle", "Ateşi tutuşturuyor", "Kindling the fire"),
    ("whistle", "Islık çalıyor", "Whistling"),
    ("sigh", "İç çekiyor", "Sighing"),
    ("fumble", "Eline yüzüne bulaştırıyor", "Fumbling"),
    ("flourish", "Hüner gösteriyor", "Showing off"),
    ("admire", "İşine bakıyor", "Admiring the work"),
    ("tidy_up", "Ortalığı topluyor", "Tidying up"),
    # mutfak
    ("chop_food", "Doğruyor", "Chopping"),
    ("chop_on_knee", "Dizinde doğruyor", "Chopping on the knee"),
    ("fill_tub", "Leğene su dolduruyor", "Filling the tub"),
    ("unload_basket", "Sepeti boşaltıyor", "Unloading the basket"),
    ("wash_dishes", "Bulaşık yıkıyor", "Washing up"),
    ("scrub_dishes", "Bulaşığı ovalıyor", "Scrubbing the dishes"),
    ("stack_dishes", "Temizleri diziyor", "Stacking the clean dishes"),
    ("stir", "Karıştırıyor", "Stirring"),
    ("taste", "Tadına bakıyor", "Tasting"),
    ("serve_up", "Kaba alıyor", "Dishing up"),
    ("knead", "Hamur yoğuruyor", "Kneading"),
    ("shape_dough", "Şekil veriyor", "Shaping the dough"),
    ("load_oven", "Fırına sürüyor", "Loading the oven"),
    ("watch_oven", "Fırını bekliyor", "Minding the oven"),
    ("unload_oven", "Fırından çıkarıyor", "Taking it out of the oven"),
    ("pour_grain", "Tahılı döküyor", "Pouring the grain"),
    ("grind", "Öğütüyor", "Grinding"),
    ("bag_flour", "Çuvala alıyor", "Bagging"),
    ("slice", "Dilimliyor", "Slicing"),
    ("layer_barrel", "Fıçıya diziyor", "Packing the barrel"),
    ("salt", "Tuzluyor", "Salting"),
    ("seal", "Kapağı kapatıyor", "Sealing"),
    ("string_up", "İpe diziyor", "Stringing up"),
    ("hang_up", "Asıyor", "Hanging up"),
    ("smoke", "Tütsülüyor", "Smoking"),
    ("pour_milk", "Sütü döküyor", "Pouring the milk"),
    ("churn", "Yayık dövüyor", "Churning"),
    ("press_curd", "Bastırıyor", "Pressing"),
    ("fill_cask", "Fıçıyı dolduruyor", "Filling the cask"),
    ("tread", "Üzüm çiğniyor", "Treading grapes"),
    ("measure_herb", "Otu ölçüyor", "Measuring herbs"),
    ("pound", "Havanda dövüyor", "Pounding"),
    ("distil", "Damıtıyor", "Distilling"),
    ("bottle", "Şişeliyor", "Bottling"),
    ("brew_cup", "Demliyor", "Brewing"),
    ("pour_cup", "Fincana döküyor", "Pouring a cup"),
    # ihtiyaçlar
    ("take_food", "Yiyeceği alıyor", "Taking the food"),
    ("eat", "Yiyor", "Eating"),
    ("wipe_mouth", "Ağzını siliyor", "Wiping up"),
    ("fill_cup", "Bardağı dolduruyor", "Filling a cup"),
    ("drink", "İçiyor", "Drinking"),
    ("night_clothes", "Uyku takımını giyiyor", "Changing for bed"),
    ("lie_down", "Yatıyor", "Lying down"),
    ("sleep", "Uyuyor", "Sleeping"),
    ("wake_stretch", "Uyanıp geriniyor", "Waking and stretching"),
    ("day_clothes", "Giyiniyor", "Getting dressed"),
    ("pour_bath", "Suyu döküyor", "Pouring the water"),
    ("undress", "Soyunuyor", "Undressing"),
    ("wash", "Yıkanıyor", "Washing"),
    ("dry_off", "Kurulanıyor", "Drying off"),
    ("splash_face", "Yüzünü yıkıyor", "Washing face and hands"),
    ("shake_hands", "Elini silkeliyor", "Shaking hands dry"),
    ("stoke", "Odun atıyor", "Stoking the fire"),
    ("warm_hands", "Ellerini ısıtıyor", "Warming hands"),
    ("look_around", "Etrafa bakıyor", "Looking around"),
    ("relieve", "İşini görüyor", "Relieving"),
    ("step_in", "İçeri giriyor", "Stepping in"),
    ("step_out", "Çıkıyor", "Stepping out"),
    # zanaat
    ("fetch_wood", "Tahtayı getiriyor", "Fetching timber"),
    ("measure", "Ölçüp çiziyor", "Measuring and marking"),
    ("saw", "Biçiyor", "Sawing"),
    ("plane", "Rendeliyor", "Planing"),
    ("hammer", "Çakıyor", "Hammering"),
    ("sand", "Zımparalıyor", "Sanding"),
    ("spread_cloth", "Kumaşı seriyor", "Spreading the cloth"),
    ("cut_cloth", "Kesiyor", "Cutting"),
    ("thread_needle", "İğneye iplik geçiriyor", "Threading the needle"),
    ("stitch", "Dikiyor", "Stitching"),
    ("try_on", "Tutup bakıyor", "Holding it up"),
    ("card_wool", "Yün tarıyor", "Carding wool"),
    ("spin", "Eğiriyor", "Spinning"),
    ("wind", "Çileyi sarıyor", "Winding the skein"),
    ("warp", "Çözgüyü geriyor", "Setting the warp"),
    ("weave", "Dokuyor", "Weaving"),
    ("stretch_hide", "Deriyi geriyor", "Stretching the hide"),
    ("scrape", "Kazıyor", "Scraping"),
    ("cut_leather", "Deriyi kesiyor", "Cutting leather"),
    ("awl", "Biz ile deliyor", "Punching with the awl"),
    ("oil_leather", "Yağlıyor", "Oiling"),
    ("add_coal", "Kömür atıyor", "Adding charcoal"),
    ("bellows", "Körük çekiyor", "Working the bellows"),
    ("heat_iron", "Demiri kızdırıyor", "Heating the iron"),
    ("forge", "Örste dövüyor", "Forging"),
    ("quench", "Suya daldırıyor", "Quenching"),
    ("wedge_clay", "Kili yoğuruyor", "Wedging clay"),
    ("throw_pot", "Çarkta şekil veriyor", "Throwing on the wheel"),
    ("load_kiln", "Fırına diziyor", "Loading the kiln"),
    ("fire_kiln", "Fırını yakıyor", "Firing the kiln"),
    ("gather_glass", "Camı pipoya alıyor", "Gathering glass"),
    ("blow_glass", "Cam üflüyor", "Blowing glass"),
    ("cool_glass", "Soğutuyor", "Cooling"),
    ("mark_stone", "Taşı işaretliyor", "Marking the stone"),
    ("chisel", "Yontuyor", "Chiselling"),
    ("blow_dust", "Tozunu üflüyor", "Blowing off the dust"),
    ("melt_tallow", "Yağı eritiyor", "Melting tallow"),
    ("dip_wick", "Fitili daldırıyor", "Dipping the wick"),
    ("heat_dye", "Boyayı ısıtıyor", "Heating the dye"),
    ("dip_cloth", "Kumaşı daldırıyor", "Dipping the cloth"),
    ("cut_quill", "Kalemi yontuyor", "Cutting the quill"),
    ("write", "Yazıyor", "Writing"),
    ("blot", "Kumla kurutuyor", "Blotting with sand"),
    ("inspect", "İnceliyor", "Inspecting"),
    ("mend", "Onarıyor", "Mending"),
    ("split_log", "Odun yarıyor", "Splitting logs"),
    ("stack", "İstifliyor", "Stacking"),
    # tarla, orman, hayvan
    ("bend_cut", "Biçiyor", "Reaping"),
    ("bundle", "Demetliyor", "Bundling"),
    ("basket", "Sepete koyuyor", "Filling the basket"),
    ("fill_bucket", "Kovayı dolduruyor", "Filling the bucket"),
    ("pour_water", "Suluyor", "Watering"),
    ("scoop", "Sepete alıyor", "Scooping"),
    ("spread", "Serpiyor", "Spreading"),
    ("reach_fruit", "Dallara uzanıyor", "Reaching into the branches"),
    ("notch", "Çentik açıyor", "Cutting a notch"),
    ("chop_tree", "Kesiyor", "Chopping"),
    ("stand_back", "Geri çekiliyor", "Standing back"),
    ("limb", "Dalları buduyor", "Limbing"),
    ("thresh", "Harman dövüyor", "Threshing"),
    ("winnow", "Savuruyor", "Winnowing"),
    ("carry_feed", "Yemi taşıyor", "Carrying feed"),
    ("pour_feed", "Yemliğe döküyor", "Filling the trough"),
    ("crouch", "Eğiliyor", "Crouching down"),
    ("collect", "Topluyor", "Collecting"),
    ("offer_hand", "Elini koklatıyor", "Offering a hand"),
    ("stroke", "Okşuyor", "Stroking"),
]

# Adım: (kimlik, poz, pay, seçenekler)
def S(id_, pose, share=1, **kw):
    d = {"id": id_, "pose": pose}
    if share != 1:
        d["share"] = share
    d.update(kw)
    return d


WHISTLE = S("whistle", "whistle", 1, when="moodGood", sfx="whistle.tune")
SIGH = S("sigh", "sigh", 0.5, when="moodBad")
FUMBLE = S("fumble", "fumble", 0.5, when="skillLow")
FLOURISH = S("flourish", "flourish", 0.5, when="skillHigh")

CHAINS = [
    # ---- ihtiyaçlar ----
    ("bath", [{"flag": "bath"}], [
        S("pour_bath", "pour", 1, when="carriedWater", sfx="work.water"),
        S("undress", "undress", 1, sfx="cloth.rustle"),
        S("wash", "bathe", 5, undress=True, sfx="bath.splash"),
        WHISTLE | {"undress": True},
        S("dry_off", "dry_off", 1, undress=True),
        S("day_clothes", "dress", 1, sfx="cloth.rustle"),
    ]),
    ("sleep", [{"flag": "sleep"}], [
        S("night_clothes", "dress", 1, sfx="cloth.rustle"),
        S("lie_down", "lie_sleep", 0.5),
        S("sleep", "lie_sleep", 14),
        S("wake_stretch", "wake_stretch", 0.5),
        S("day_clothes", "dress", 1, sfx="cloth.rustle"),
    ]),
    ("drink_brewed", [{"station": "item.cezve"}, {"station": "item.teapot"}], [
        S("kindle", "kindle", 1, sfx="fire.kindle"),
        S("brew_cup", "stir", 3, sfx="kitchen.stir"),
        S("pour_cup", "pour", 1),
        S("drink", "drink", 3, held="consume"),
    ]),
    ("drink", [{"flag": "drink"}], [
        S("fill_cup", "pour", 1),
        S("drink", "drink", 3, held="consume"),
    ]),
    ("eat_from_bowl", [{"flag": "eatContents"}], [
        S("take_food", "reach", 1),
        S("eat", "eat", 5),
        S("wipe_mouth", "wipe_mouth", 0.5),
    ]),
    ("eat", [{"flag": "eat"}], [
        S("take_food", "reach", 1, held="consume"),
        S("eat", "eat", 5, held="consume"),
        S("wipe_mouth", "wipe_mouth", 0.5),
    ]),
    ("warm", [{"need": "Warmth", "category": "heat_light"}], [
        S("stoke", "kindle", 1, when="fire", sfx="fire.kindle"),
        S("warm_hands", "warm_hands", 5),
    ]),
    ("relieve_outdoors", [{"flag": "toilet", "category": "plant"}, {"flag": "toilet", "category": "forage"}], [
        S("look_around", "look_around", 1),
        S("relieve", "relieve", 3),
        S("step_out", "step_out", 1),
    ]),
    ("toilet", [{"flag": "toilet"}], [
        S("step_in", "step_in", 1),
        S("relieve", "relieve", 3),
        S("step_out", "step_out", 1),
    ]),
    ("wash_hands", [{"need": "Hygiene"}], [
        S("splash_face", "wash", 3, sfx="work.water"),
        S("shake_hands", "shake_hands", 1),
    ]),

    # ---- mutfak ----
    # Ocakta pişirme (63, 63a): kap ve aletler ayak işiyle (JobRunner.Errands) gelir; ateş sönükse tutuşturulur.
    ("cook_pot", [{"flag": "cook", "category": "heat_light"}], [
        S("kindle", "kindle", 1, when="unlit", sfx="fire.kindle"),
        S("gather", "reach", 1, held="consume"),
        S("chop_on_knee", "chop_knife", 1, when="noBoard", sfx="kitchen.chop"),
        S("chop_food", "chop_knife", 2, sfx="kitchen.chop"),
        FUMBLE,
        S("stir", "stir", 4, sfx="kitchen.stir"),
        WHISTLE,
        SIGH,
        S("taste", "taste", 1),
        FLOURISH,
        S("serve_up", "ladle", 1, held="produce", sfx="kitchen.ladle"),
    ]),
    ("bake", [{"station": "item.stone_oven", "skill": "Baking"}], [
        S("gather", "reach", 1, held="consume"),
        S("knead", "knead", 3, sfx="kitchen.knead"),
        FUMBLE,
        S("shape_dough", "knead", 1),
        S("kindle", "kindle", 1, sfx="fire.kindle"),
        S("load_oven", "peel", 1),
        S("watch_oven", "wait", 3),
        WHISTLE,
        S("unload_oven", "peel", 1, held="produce"),
    ]),
    ("mill", [{"station": "item.millstone"}, {"station": "item.quern"}], [
        S("pour_grain", "pour", 1, held="consume"),
        S("grind", "pull", 5),
        S("bag_flour", "reach", 1, held="produce"),
    ]),
    ("preserve", [{"station": "item.salting_barrel"}, {"station": "item.brine_crock"}], [
        S("slice", "chop_knife", 2, sfx="kitchen.chop"),
        S("layer_barrel", "reach", 2),
        S("salt", "sprinkle", 1),
        S("seal", "reach", 1),
    ]),
    ("smoke", [{"station": "item.smokehouse"}], [
        S("hang_up", "reach", 2, held="consume"),
        S("kindle", "kindle", 1, sfx="fire.kindle"),
        S("smoke", "wait", 4),
        S("gather", "reach", 1, held="produce"),
    ]),
    ("dry_rack", [{"station": "item.drying_rack"}], [
        S("slice", "chop_knife", 2, sfx="kitchen.chop"),
        S("string_up", "craft_hand", 2),
        S("hang_up", "reach", 2),
    ]),
    ("dairy", [{"station": "item.churn"}, {"station": "item.cheese_press"}], [
        S("pour_milk", "pour", 1, held="consume"),
        S("churn", "churn", 5),
        S("press_curd", "knead", 1, held="produce"),
    ]),
    ("grape_tub", [{"station": "item.grape_tub"}], [
        S("fill_cask", "pour", 1, held="consume"),
        S("tread", "tread", 5),
        WHISTLE,
    ]),
    ("brew", [{"station": "item.brew_barrel"}], [
        S("fill_cask", "pour", 2, held="consume"),
        S("stir", "stir", 3),
        S("seal", "reach", 1),
    ]),
    ("apothecary", [{"station": "item.alembic"}, {"station": "item.mortar_pestle"}], [
        S("measure_herb", "reach", 1, held="consume"),
        S("pound", "pound", 3),
        S("distil", "wait", 3),
        S("bottle", "pour", 1, held="produce"),
    ]),

    # ---- zanaat ----
    ("woodcut", [{"station": "item.chopping_block"}], [
        S("fetch_wood", "carry", 1, held="consume"),
        S("split_log", "chop", 5, sfx="work.chop"),
        S("stack", "reach", 1, held="produce"),
    ]),
    ("carpentry", [{"station": "item.workbench", "flag": "craft"}], [
        S("fetch_wood", "carry", 1),
        S("measure", "craft_hand", 1),
        S("saw", "saw", 2),
        FUMBLE,
        S("plane", "plane", 2),
        S("hammer", "craft_hammer", 2, sfx="work.hammer"),
        WHISTLE,
        S("sand", "craft_hand", 1),
        FLOURISH,
        S("admire", "admire", 1, held="produce"),
    ]),
    ("sew", [{"station": "item.sewing_table", "flag": "craft"}], [
        S("spread_cloth", "reach", 1),
        S("measure", "craft_hand", 1),
        S("cut_cloth", "craft_hand", 1),
        S("thread_needle", "sew", 0.5),
        FUMBLE,
        S("stitch", "sew", 5),
        WHISTLE,
        S("try_on", "admire", 1, held="produce"),
    ]),
    ("spin", [{"station": "item.spinning_wheel"}], [
        S("card_wool", "craft_hand", 2),
        S("spin", "spin", 5),
        S("wind", "craft_hand", 1, held="produce"),
    ]),
    ("weave", [{"station": "item.loom", "flag": "craft"}], [
        S("warp", "craft_hand", 2),
        S("weave", "weave", 6),
        WHISTLE,
        S("cut_cloth", "craft_hand", 1, held="produce"),
    ]),
    ("tan", [{"station": "item.tanning_rack"}], [
        S("stretch_hide", "pull", 2, held="consume"),
        S("scrape", "scrape", 5),
        S("oil_leather", "craft_hand", 1),
    ]),
    ("leather", [{"station": "item.saddler_bench", "flag": "craft"}], [
        S("cut_leather", "craft_hand", 2),
        S("awl", "craft_hand", 2),
        S("stitch", "sew", 4),
        S("oil_leather", "craft_hand", 1),
        S("admire", "admire", 1, held="produce"),
    ]),
    ("smith", [{"station": "item.forge"}, {"station": "item.bloomery"}], [
        S("add_coal", "reach", 1),
        S("bellows", "bellows", 2),
        S("heat_iron", "wait", 1),
        S("forge", "craft_hammer", 4, sfx="work.hammer"),
        FUMBLE,
        S("quench", "pour", 1),
        FLOURISH,
        S("inspect", "admire", 1, held="produce"),
    ]),
    ("pottery", [{"station": "item.pottery_wheel"}], [
        S("wedge_clay", "knead", 2),
        S("throw_pot", "wheel_throw", 5),
        FUMBLE,
        S("admire", "admire", 1, held="produce"),
    ]),
    ("kiln", [{"station": "item.kiln"}], [
        S("load_kiln", "reach", 2),
        S("fire_kiln", "kindle", 1),
        S("watch_oven", "wait", 4),
        S("unload_oven", "reach", 1, held="produce"),
    ]),
    ("glass", [{"station": "item.glass_bench"}, {"station": "item.glass_furnace"}], [
        S("bellows", "bellows", 1),
        S("gather_glass", "reach", 1),
        S("blow_glass", "blow_glass", 4),
        S("cool_glass", "wait", 1),
        S("admire", "admire", 1, held="produce"),
    ]),
    ("stonecut", [{"station": "item.stonecutter"}], [
        S("mark_stone", "craft_hand", 1),
        S("chisel", "chisel", 5, sfx="work.hammer"),
        S("blow_dust", "blow_dust", 0.5),
        S("admire", "admire", 1, held="produce"),
    ]),
    ("chandler", [{"station": "item.chandler_table"}], [
        S("melt_tallow", "stir", 2),
        S("dip_wick", "dip", 4),
        S("hang_up", "reach", 1, held="produce"),
    ]),
    ("dye", [{"station": "item.dye_vat"}], [
        S("heat_dye", "kindle", 1),
        S("dip_cloth", "dip", 2, held="consume"),
        S("stir", "stir", 3),
        S("hang_up", "reach", 1, held="produce"),
    ]),
    ("scribe", [{"station": "item.writing_lectern"}], [
        S("cut_quill", "craft_hand", 1),
        S("write", "write", 5),
        S("blot", "sprinkle", 1),
    ]),
    ("repair", [{"flag": "repair"}], [
        S("inspect", "admire", 1),
        S("mend", "craft_hammer", 4, sfx="work.hammer"),
        S("admire", "admire", 1),
    ]),

    # ---- tarla, orman, hayvan ----
    ("thresh", [{"station": "item.threshing_floor"}], [
        S("spread", "spread", 1, held="consume"),
        S("thresh", "thresh", 4),
        S("winnow", "sieve", 2),
        S("bag_flour", "reach", 1, held="produce"),
    ]),
    ("fell", [{"flag": "harvest", "skill": "Woodcutting"}], [
        S("notch", "chop", 1, sfx="work.chop"),
        S("chop_tree", "chop", 5, sfx="work.chop"),
        S("stand_back", "stand_back", 0.5),
        S("limb", "chop", 2),
    ]),
    ("harvest", [{"flag": "harvest", "skill": "Farming"}], [
        S("bend_cut", "reap", 4, sfx="work.harvest"),
        S("bundle", "bend", 1),
        S("basket", "reach", 1, held="produce"),
    ]),
    ("pick_fruit", [{"flag": "harvest", "skill": "Horticulture"}, {"flag": "harvest", "skill": "Foraging"}], [
        S("reach_fruit", "pick", 4, sfx="work.harvest"),
        S("basket", "bend", 1, held="produce"),
    ]),
    ("water_crop", [{"flag": "water"}], [
        S("fill_bucket", "bucket_carry", 1, sfx="work.water"),
        S("pour_water", "pour", 3, sfx="work.water"),
    ]),
    ("fertilize", [{"flag": "fertilize"}], [
        S("scoop", "bend", 1, held="consume"),
        S("spread", "spread", 3),
    ]),
    ("feed_animals", [{"flag": "feed"}], [
        S("carry_feed", "carry", 1, held="consume"),
        S("pour_feed", "pour", 2),
    ]),
    ("collect_produce", [{"flag": "collect"}], [
        S("crouch", "bend", 1),
        S("collect", "milk", 4),
        S("basket", "reach", 1),
    ]),
    # Bulaşık (63a §4): kirliler sepette gelir; yalakta ayakta, leğende ve kıyıda çömelerek yıkanır.
    ("wash_dishes_stand", [{"flag": "washDishes", "station": "item.kitchen_spout"}], [
        S("unload_basket", "reach", 1),
        S("wash_dishes", "wash_dishes", 5, sfx="bath.splash"),
        WHISTLE,
        S("stack_dishes", "reach", 1),
    ]),
    ("wash_dishes", [{"flag": "washDishes"}], [
        S("fill_tub", "pour", 1, when="carriedWater", sfx="work.water"),
        S("unload_basket", "reach", 1),
        S("scrub_dishes", "scrub", 5, sfx="bath.splash"),
        WHISTLE,
        SIGH,
        S("stack_dishes", "reach", 1),
    ]),
    ("pet_animals", [{"flag": "pet"}], [
        S("offer_hand", "reach", 1),
        S("stroke", "love", 4),
    ]),
]


# ---------------------------------------------------------------------------------------------------------------------
# Parti 2-10 (Project belgesi 62, 64-yemek): adım yeri, kapanış adımı, etki anı; sofra, mezbaha ve eşya dışı işler.
# ---------------------------------------------------------------------------------------------------------------------

# (zincir, adım) → ek alanlar. at: adımın yeri (storage, prep, table, water, fire); closing: iş kesilince de oynar;
# take: malzeme bu adımda envanterden ayrılır.
STEP_EXTRAS = {
    ("cook_pot", "gather"): {"at": "storage", "take": True},
    ("cook_pot", "chop_food"): {"at": "prep"},
    ("bake", "gather"): {"at": "storage", "take": True},
    ("bake", "knead"): {"at": "prep"},
    ("bake", "shape_dough"): {"at": "prep"},
    ("bath", "dry_off"): {"closing": True},
    ("bath", "day_clothes"): {"closing": True},
    ("sleep", "wake_stretch"): {"closing": True},
    ("sleep", "day_clothes"): {"closing": True},
}

LABELS += [
    # sofra ve mezbaha
    ("sit_down", "Oturuyor", "Sitting down"),
    ("take_portion", "Tabağına alıyor", "Helping themselves"),
    ("eat_at_table", "Sofrada yiyor", "Eating at the table"),
    ("chat_at_table", "Sofrada sohbet ediyor", "Chatting at the table"),
    ("get_up", "Kalkıyor", "Getting up"),
    ("stack_plates", "Tabakları topluyor", "Stacking the plates"),
    ("carry_dishes", "Bulaşığı götürüyor", "Carrying the dishes"),
    ("wipe_table", "Masayı siliyor", "Wiping the table"),
    ("lead_animal", "Hayvanı içeri götürüyor", "Leading the animal in"),
    ("door_closes", "Kapı kapalı", "The door is closed"),
    ("come_out", "Dışarı çıkıyor", "Coming out"),
    ("rinse_hands", "Ellerini yıkıyor", "Washing their hands"),
    ("open_locker", "Et dolabını açıyor", "Opening the meat locker"),
    ("carry_meat", "Eti kilere taşıyor", "Carrying the meat to the pantry"),
    ("offer_scrap", "Köpeğe bir lokma veriyor", "Slipping the dog a scrap"),
    ("pick_eggs", "Yumurtaları topluyor", "Gathering the eggs"),
    ("sit_stool", "Tabureye oturuyor", "Sitting on the stool"),
    ("milk_animal", "Sağıyor", "Milking"),
    ("smoke_hive", "Kovanı dumanlıyor", "Smoking the hive"),
    ("cut_comb", "Peteği kesiyor", "Cutting the comb"),
    # eşya dışı işler
    ("greet", "Selam veriyor", "Greeting"),
    ("talk", "Konuşuyor", "Talking"),
    ("laugh_together", "Gülüşüyorlar", "Laughing together"),
    ("part", "Ayrılıyor", "Taking their leave"),
    ("pick_up_baby", "Bebeği kucağına alıyor", "Picking up the baby"),
    ("tend_baby", "Bebekle ilgileniyor", "Tending the baby"),
    ("put_down_baby", "Bebeği yatırıyor", "Laying the baby down"),
    ("tell", "Anlatıyor", "Telling"),
    ("examine", "Bakıyor, yokluyor", "Examining"),
    ("treat", "Tedavi ediyor", "Treating"),
    ("reassure", "Yüreklendiriyor", "Reassuring"),
    ("draw_near", "Yaklaşıyor", "Drawing near"),
    ("speak_heart", "Gönlünü açıyor", "Speaking from the heart"),
    ("spread_cloth", "Örtüyü seriyor", "Spreading the cloth"),
    ("picnic_eat", "Piknik yapıyor", "Having a picnic"),
    ("pack_basket", "Sepeti topluyor", "Packing the basket"),
    ("stroll", "Yürüyorlar", "Strolling"),
    ("pause_view", "Durup bakıyorlar", "Pausing for the view"),
    ("cast_line", "Oltayı atıyor", "Casting the line"),
    ("wait_bite", "Balığın vurmasını bekliyor", "Waiting for a bite"),
    ("land_fish", "Balığı çekiyor", "Landing the fish"),
    ("lend_hand", "Yardım ediyor", "Lending a hand"),
    ("invite", "Yemeğe çağırıyor", "Inviting to dinner"),
    ("offer_hand", "Elini uzatıyor", "Offering a hand"),
    ("stroke", "Seviyor", "Stroking"),
    ("brush_coat", "Tımar ediyor", "Brushing the coat"),
    ("check_hooves", "Tırnaklarına bakıyor", "Checking the hooves"),
    ("lift_saddle", "Eyeri kaldırıyor", "Lifting the saddle"),
    ("buckle", "Kolanı sıkıyor", "Tightening the girth"),
    ("show_command", "Komutu gösteriyor", "Showing the command"),
    ("reward", "Ödüllendiriyor", "Rewarding"),
    ("break_soil", "Toprağı kırıyor", "Breaking the soil"),
    ("turn_soil", "Toprağı çeviriyor", "Turning the soil"),
    ("level_soil", "Düzlüyor", "Levelling"),
    ("open_furrow", "Çizi açıyor", "Opening a furrow"),
    ("drop_seed", "Tohumu bırakıyor", "Dropping the seed"),
    ("cover_seed", "Üstünü kapatıyor", "Covering it over"),
    ("wade_in", "Suya giriyor", "Wading in"),
    ("swing_pick", "Kazmayı vuruyor", "Swinging the pick"),
    ("clear_rubble", "Molozu çekiyor", "Clearing the rubble"),
    ("set_prop", "Direği dikiyor", "Setting the prop"),
    ("wedge_prop", "Kamayı çakıyor", "Wedging it tight"),
    ("fill_pail", "Kovayı dolduruyor", "Filling the pail"),
    ("haul_pail", "Suyu dışarı taşıyor", "Hauling the water out"),
    ("mark_ice", "Buzu çiziyor", "Scoring the ice"),
    ("saw_ice", "Buzu kesiyor", "Sawing the ice"),
    ("patch_road", "Yolu onarıyor", "Mending the road"),
    ("release_fish", "Balığı salıyor", "Releasing the fish"),
]


def _coop_matches():
    """Yumurta, sağım ve bal ayrı zincir (belge 63 §3.3): barınak türüne göre eşya kimliğiyle eşlenir."""
    items = json.loads((ROOT / "Assets/Resources/Content/items.json").read_text(encoding="utf-8"))["items"]
    by = {"eggs": [], "milk": [], "honey": []}
    for it in items:
        if not any(i.get("collect") for i in it.get("interactions", []) or []):
            continue
        h = it.get("housing")
        kind = "eggs" if h in ("coop", "dovecote") else "milk" if h == "barn" else "honey" if h == "hive" else None
        if kind:
            by[kind].append({"station": it["id"], "flag": "collect"})
    return by


def _extend():
    for cid, _, steps in CHAINS:
        for st in steps:
            extra = STEP_EXTRAS.get((cid, st["id"]))
            if extra:
                st.update(extra)
    coop = _coop_matches()
    new = [
        ("eat_serving", [{"flag": "meal", "idPrefix": "eat_serving"}], [
            S("sit_down", "sit", 1),
            S("take_portion", "reach", 1, sfx="kitchen.ladle"),
            S("eat_at_table", "sit_eat", 5),
            S("chat_at_table", "sit", 1, when="moodGood"),
            S("get_up", "sit", 0.5),
        ]),
        ("clear_table", [{"flag": "meal", "idPrefix": "clear_table"}], [
            S("stack_plates", "reach", 2),
            S("carry_dishes", "carry", 2),
            S("wipe_table", "reach", 1),
        ]),
        ("slaughter", [{"flag": "meal", "idPrefix": "slaughter"}], [
            S("lead_animal", "carry", 2),
            S("door_closes", "wait", 6),
            S("come_out", "step_out", 1),
            S("rinse_hands", "wash", 1, sfx="work.water"),
        ]),
        ("collect_meat", [{"flag": "meal", "idPrefix": "collect_meat"}], [
            S("open_locker", "reach", 1),
            S("carry_meat", "carry", 3, at="storage"),
        ]),
        ("feed_scrap", [{"flag": "meal", "idPrefix": "feed_scrap"}], [
            S("offer_scrap", "reach", 1),
        ]),
    ]
    if coop["eggs"]:
        new.append(("collect_eggs", coop["eggs"], [S("crouch", "bend", 1), S("pick_eggs", "bend", 3), S("basket", "reach", 1)]))
    if coop["milk"]:
        new.append(("collect_milk", coop["milk"], [S("sit_stool", "sit", 1), S("milk_animal", "milk", 4), S("basket", "carry", 1)]))
    if coop["honey"]:
        new.append(("collect_honey", coop["honey"], [S("smoke_hive", "reach", 1), S("cut_comb", "chop_knife", 3), S("basket", "reach", 1)]))
    # Özel zincirler genel "eat", "wash_hands" ve "collect_produce" kurallarından önce gelmeli: en başa eklenir.
    CHAINS[0:0] = new
    jobs = [
        ("soc_chat", ["social:Chat", "social:Compliment", "social:Joke", "social:Listen", "social:TellAnimalTalk", "social:Persuade", "social:Mediate"], [
            S("greet", "social", 1), S("talk", "social", 4), S("laugh_together", "social", 1, when="moodGood"), S("part", "social", 0.5)]),
        ("soc_baby", ["social:Feed", "social:Lullaby", "social:PlayWithBaby", "social:Soothe", "social:SingLullaby", "social:FirstSteps"], [
            S("pick_up_baby", "care", 1), S("tend_baby", "care", 4), S("put_down_baby", "care", 1)]),
        ("soc_tell", ["social:TellStory", "social:TeachLetters", "social:ElderLesson", "social:VillageLesson", "social:ShowWork", "social:TalkTeen"], [
            S("sit_down", "sit", 1), S("tell", "social", 5), S("get_up", "sit", 0.5)]),
        ("soc_heal", ["social:Bandage", "social:SetBone", "social:Diagnose", "social:Midwife", "social:VisitPatient", "social:ElderCare"], [
            S("examine", "care", 2), S("treat", "care", 4), S("reassure", "social", 1)]),
        ("soc_heart", ["social:Propose", "social:TryForBaby"], [S("draw_near", "love", 1), S("speak_heart", "love", 3)]),
        ("soc_picnic", ["social:Picnic"], [
            S("spread_cloth", "bend", 1), S("sit_down", "sit", 1), S("picnic_eat", "sit_eat", 5), S("chat_at_table", "sit", 2), S("pack_basket", "bend", 1)]),
        ("soc_walk", ["social:EveningWalk"], [S("stroll", "walk2", 5), S("pause_view", "admire", 1)]),
        ("soc_fish", ["social:FishTogether"], [S("cast_line", "fish", 1), S("wait_bite", "fish", 5), S("land_fish", "fish", 1)]),
        ("soc_fireside", ["social:Fireside"], [S("sit_down", "sit", 1), S("warm_hands", "warm_hands", 4), S("tell", "social", 2)]),
        ("soc_help", ["social:HelpWork"], [S("lend_hand", "work", 1)]),
        ("soc_invite", ["social:InviteDinner"], [S("greet", "social", 1), S("invite", "social", 2)]),
        ("ani_pet", ["animal:Pet"], [S("offer_hand", "reach", 1), S("stroke", "love", 4)]),
        ("ani_brush", ["animal:Brush"], [S("brush_coat", "care", 4), S("check_hooves", "bend", 1)]),
        ("ani_saddle", ["animal:Saddle", "animal:Unsaddle"], [S("lift_saddle", "carry", 1), S("buckle", "bend", 2)]),
        ("ani_train", ["animal:Train"], [S("show_command", "social", 3), S("reward", "reach", 1)]),
        ("tile_till", ["tile:Till"], [S("break_soil", "hoe", 2, sfx="work.hoe"), S("turn_soil", "hoe", 3, sfx="work.hoe"), FUMBLE, S("level_soil", "hoe", 1)]),
        ("tile_sow", ["tile:Sow"], [S("open_furrow", "hoe", 1), S("drop_seed", "sow", 3), S("cover_seed", "bend", 1)]),
        ("tile_fish", ["tile:Fish"], [S("cast_line", "fish", 1), S("wait_bite", "fish", 5), WHISTLE, S("land_fish", "fish", 1)]),
        ("tile_bathe", ["tile:Bathe"], [S("undress", "undress", 1, sfx="cloth.rustle"), S("wade_in", "step_in", 1, undress=True), S("wash", "bathe", 4, undress=True, sfx="bath.splash"),
                                       S("dry_off", "dry_off", 1, undress=True), S("day_clothes", "dress", 1, sfx="cloth.rustle")]),
        ("tile_dig", ["tile:Dig"], [S("swing_pick", "mine_dig", 5), S("clear_rubble", "mine_bucket", 1)]),
        ("tile_support", ["tile:Support"], [S("set_prop", "mine_support", 3), S("wedge_prop", "craft_hammer", 1)]),
        ("tile_bail", ["tile:Bail"], [S("fill_pail", "mine_bucket", 2), S("haul_pail", "carry", 2)]),
        ("tile_ice", ["tile:CutIce"], [S("mark_ice", "bend", 1), S("saw_ice", "saw", 4)]),
        ("tile_repair", ["tile:Repair"], [S("patch_road", "hoe", 3)]),
        ("tile_stock", ["tile:Stock"], [S("release_fish", "bend", 2)]),
    ]
    for cid, keys, steps in jobs:
        CHAINS.append((cid, {"jobs": keys}, steps))


_extend()


def chain_line(cid, match, steps):
    head = {"id": cid, "jobs": match["jobs"]} if isinstance(match, dict) else {"id": cid, "match": match}
    head["steps"] = steps
    return "    " + json.dumps(head, ensure_ascii=False)


def write_life():
    text = LIFE.read_text(encoding="utf-8")
    nl = "\r\n" if "\r\n" in text else "\n"
    body = text.rstrip()
    if not body.endswith("}"):
        raise SystemExit("life.json beklenen biçimde bitmiyor")
    # Var olan bölümü çıkar (yeniden çalıştırma).
    body = re.sub(r',\s*"chains": \[.*?\n  \]', "", body, flags=re.S)
    body = body[:-1].rstrip()
    block = "," + nl + '  "chains": [' + nl + ("," + nl).join(chain_line(*c) for c in CHAINS) + nl + "  ]" + nl + "}" + nl
    LIFE.write_text(body + block, encoding="utf-8", newline="")
    json.loads(LIFE.read_text(encoding="utf-8"))  # geçerli JSON mu


def write_labels():
    """Adım metinleri kendi alan dosyasına yazılır (ui.chains.<dil>.json); ana ui tablosundaki eski anahtarlar taşınır."""
    used = {s["id"] for _, _, steps in CHAINS for s in steps}
    known = {k for k, _, _ in LABELS}
    missing = sorted(used - known)
    if missing:
        raise SystemExit("Metni olmayan adımlar: " + ", ".join(missing))
    for lang, col in (("tr", 1), ("en", 2)):
        main = LOC / f"ui.{lang}.json"
        raw = main.read_text(encoding="utf-8")
        nl = "\r\n" if "\r\n" in raw else "\n"
        table = json.loads(raw)
        if any(k.startswith("chain.step.") for k in table):
            table = {k: v for k, v in table.items() if not k.startswith("chain.step.")}
            out = json.dumps(dict(sorted(table.items())), ensure_ascii=False, indent=2)
            main.write_text(out.replace("\n", nl) + nl, encoding="utf-8", newline="")
        area = {"chain.step." + row[0]: row[col] for row in LABELS if row[0] in used}
        out = json.dumps(dict(sorted(area.items())), ensure_ascii=False, indent=2)
        (LOC / f"ui.chains.{lang}.json").write_text(out.replace("\n", nl) + nl, encoding="utf-8", newline="")


if __name__ == "__main__":
    ids = [c[0] for c in CHAINS]
    assert len(ids) == len(set(ids)), "yinelenen zincir kimliği"
    write_life()
    write_labels()
    print(f"{len(CHAINS)} zincir, {len({s['id'] for _, _, st in CHAINS for s in st})} adım metni yazıldı.")
