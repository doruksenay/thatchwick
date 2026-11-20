using System.Linq;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Clothing;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Build;
using Thatchwick.Core.Work;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;
using Xunit;

namespace Thatchwick.Core.Tests
{
    /// <summary>Terzilik ve giyim, Parti 2 (belge 48 §6, §9-12; belge 51 §2): örnek, takım, işlev, takı, eskime, çamaşır, giyim uyumu, kayıt.</summary>
    public class GiyimSistemiTests
    {
        private static ContentDatabase _db;
        private static ContentDatabase Db()
        {
            if (_db != null) return _db;
            var db = ContentFilesTests.LoadDb(out _);
            db.Clothing = TerzilikVeriTests.LoadCatalog(db, out _);
            return _db = db;
        }

        private static ClothingService Svc() => new ClothingService(Db().Clothing) { Moods = Db().Moods };

        private static Character Person(int id = 1, double age = 35, Sex sex = Sex.Female)
        {
            var c = new Character(id, "K" + id, new GridPos(0, 0), true) { Sex = sex, CanStarve = false, MoodEnabled = true };
            c.SetAge(age);
            return c;
        }

        // ---------- örnek ve takım ----------

        [Fact]
        public void Ornek_VarsayilanRenkVeKesim_Giyilir()
        {
            var s = Svc();
            var c = Person();
            var t = s.Create("long_tunic");
            Assert.Equal(DyePalette.WhiteWool, t.Color.Id);
            Assert.Equal("long", t.Cuts[CutKind.Sleeve]);
            Assert.Equal("round", t.Cuts[CutKind.Collar]);
            Assert.Equal(EquipError.None, s.Equip(c, t, OutfitKind.Daily));
            Assert.Equal(c.Id, t.OwnerId);
            s.Dress(c, default);
            Assert.Contains(t, s.Worn(c));
            Assert.True(c.Mood.Has(ClothingService.NewClothes));
        }

        [Fact]
        public void Yas_ErgenAcikKesimVeGomleksizYelekGiyemez()
        {
            var s = Svc();
            var teen = Person(2, 26);
            var dress = s.Create("plain_entari");
            dress.Cuts[CutKind.Collar] = "boat";
            Assert.Equal(EquipError.AdultCut, s.Equip(teen, dress, OutfitKind.Daily));
            dress.Cuts[CutKind.Collar] = "round";
            Assert.Equal(EquipError.None, s.Equip(teen, dress, OutfitKind.Daily));
            var vest = s.Create("felt_vest");
            Assert.Equal(EquipError.BareArmed, s.Equip(teen, vest, OutfitKind.Work));
            Assert.Equal(EquipError.None, s.Equip(teen, vest, OutfitKind.Daily));   // entarinin üstüne
            s.Unequip(teen, dress.Id, OutfitKind.Daily);
            Assert.False(s.WardrobeOf(teen).Get(OutfitKind.Daily).Slots.ContainsKey(ClothingSlot.Work));
            var adult = Person(3, 35);
            Assert.Equal(EquipError.None, s.Equip(adult, s.Create("felt_vest"), OutfitKind.Work));
            var wed = s.Create("wedding_kaftan");
            Assert.Equal(EquipError.Age, s.Equip(teen, wed, OutfitKind.Festive));
        }

        [Fact]
        public void Yas_CocukVeBebek()
        {
            var s = Svc();
            var child = Person(4, 15);
            Assert.Equal(EquipError.Age, s.Equip(child, s.Create("long_cloak"), OutfitKind.Daily));
            Assert.Equal(EquipError.None, s.Equip(child, s.Create("child_cloak"), OutfitKind.Daily));
            var baby = Person(5, 2);
            Assert.Equal(EquipError.Age, s.Equip(baby, s.Create("tunic"), OutfitKind.Daily));
            Assert.Equal(EquipError.None, s.Equip(baby, s.Create("swaddle"), OutfitKind.Daily));
            Assert.Equal(EquipError.None, s.Equip(baby, s.CreateJewelry("evil_eye"), OutfitKind.Daily));
            Assert.Equal(EquipError.Age, s.Equip(baby, s.CreateJewelry("glass_beads"), OutfitKind.Daily));
        }

        [Fact]
        public void Takim_DurumaGoreSecilir()
        {
            var s = Svc();
            var c = Person();
            foreach (var k in new[] { OutfitKind.Daily, OutfitKind.Work, OutfitKind.Sleep, OutfitKind.Winter, OutfitKind.Festive })
                s.Equip(c, s.Create("tunic"), k);
            Assert.Equal(OutfitKind.Daily, s.Choose(c, new ClothingContext { Temperature = 20 }));
            Assert.Equal(OutfitKind.Winter, s.Choose(c, new ClothingContext { Outdoors = true, Temperature = 0 }));
            Assert.Equal(OutfitKind.Work, s.Choose(c, new ClothingContext { Working = true, Temperature = 20 }));
            Assert.Equal(OutfitKind.Festive, s.Choose(c, new ClothingContext { Festival = true, Working = true }));
            c.Asleep = true;
            Assert.Equal(OutfitKind.Sleep, s.Choose(c, new ClothingContext { Festival = true }));
            c.Asleep = false;
            // takımı boşsa Günlük
            var d = Person(7);
            s.Equip(d, s.Create("tunic"), OutfitKind.Daily);
            Assert.Equal(OutfitKind.Daily, s.Choose(d, new ClothingContext { Working = true }));
        }

        [Fact]
        public void Takim_GebelikGunlukYerine_YagmurdaPelerin()
        {
            var s = Svc();
            var c = Person();
            s.Equip(c, s.Create("plain_entari"), OutfitKind.Daily);
            s.Equip(c, s.Create("maternity_entari"), OutfitKind.Maternity);
            Assert.True(c.TryStartPregnancy(9));
            Assert.Equal(OutfitKind.Maternity, s.Choose(c, default));
            s.WardrobeOf(c).AutoMaternity = false;
            Assert.Equal(OutfitKind.Daily, s.Choose(c, default));
            var cloak = s.Create("kepenek");
            s.Give(cloak, c);
            s.WardrobeOf(c).RainCloak = cloak.Id;
            Assert.DoesNotContain(cloak, s.Worn(c, raining: false));
            Assert.Contains(cloak, s.Worn(c, raining: true));
        }

        // ---------- işlev ----------

        [Fact]
        public void Islev_IsiMalzemeYuvaKalite_ElmasSaymaz()
        {
            var s = Svc();
            var c = Person();
            Assert.Equal(0f, s.ColdResist(c));
            var cloak = s.Create("long_cloak");
            s.Equip(c, cloak, OutfitKind.Daily);
            s.Dress(c, default);
            float w0 = s.WarmthPoints(c);
            Assert.Equal((double)(3f * 1.5f), (double)(w0), 3);   // yün × dış
            cloak.Quality = 2;
            Assert.Equal((double)(w0 * 1.5f), (double)(s.WarmthPoints(c)), 3);
            var fur = s.Create("fur_lined_cloak");
            s.Equip(c, fur, OutfitKind.Daily);
            Assert.True(s.WarmthPoints(c) > w0);
            fur.Diamond = true;
            Assert.Equal(0f, s.WarmthPoints(c));
            Assert.True(s.ColdResist(c) <= ClothingService.MaxColdResist);
        }

        [Fact]
        public void Islev_KepenekYagmurdaIslatmaz_KarakterSoakRain()
        {
            var s = Svc();
            var c = Person();
            var k = s.Create("kepenek");
            s.Equip(c, k, OutfitKind.Daily);
            Assert.Equal(1f, s.RainGuard(c));
            c.RainGuard = s.RainGuard(c);
            c.SoakRain(0.5f);
            Assert.Equal(0f, c.Wetness);
            c.RainGuard = 0.5f;
            c.SoakRain(0.5f);
            Assert.Equal((double)(0.25f), (double)(c.Wetness), 3);
        }

        [Fact]
        public void Islev_IsTehlikeleri()
        {
            var s = Svc();
            var c = Person();
            Assert.Equal(1.0, s.HazardFactor(c, "bee"));
            s.Equip(c, s.Create("beekeeper_veil"), OutfitKind.Daily);
            Assert.Equal(0.25, s.HazardFactor(c, "bee"));
            s.Equip(c, s.Create("work_gloves"), OutfitKind.Daily);
            Assert.Equal(0.5, s.HazardFactor(c, "thorns"));
        }

        [Fact]
        public void Islev_GiysiIsisiKarakterSogukDirencineEklenir()
        {
            var a = Person(1); var b = Person(2);
            a.AmbientTemperature = b.AmbientTemperature = -5f;
            b.ClothingColdResist = 0.4f;
            a.Tick(0.2); b.Tick(0.2);
            Assert.True(b.Needs.Get(NeedType.Warmth) > a.Needs.Get(NeedType.Warmth));
        }

        // ---------- takı ----------

        /// <summary>Altın tozu ve değerli taş takıları (karar 20.11.2026): temel takının etkisini daha güçlü taşır.</summary>
        [Fact]
        public void AltinVeTasliTakilar_EtkileriGuclu()
        {
            var db = Db();
            foreach (var id in new[] { "jw_ring_gold", "jw_ring_gem", "jw_earring_gold", "jw_earring_gem", "jw_necklace_gem", "jw_evil_eye_gem" })
            {
                var recipe = db.Items.Get("item." + id);
                Assert.NotNull(db.Clothing.JewelryByItem("item." + id));
                Assert.True(recipe.Price > 100);
            }
            var s = Svc();
            var plain = Person(1); var rich = Person(2);
            Assert.Equal(EquipError.None, s.WearJewelry(plain, s.FromItem("item.jw_ring")));
            Assert.Equal(EquipError.None, s.WearJewelry(plain, s.FromItem("item.jw_earring")));
            Assert.Equal(EquipError.None, s.WearJewelry(plain, s.FromItem("item.jw_evil_eye")));
            Assert.Equal(EquipError.None, s.WearJewelry(rich, s.FromItem("item.jw_ring_gem")));
            Assert.Equal(EquipError.None, s.WearJewelry(rich, s.FromItem("item.jw_earring_gold")));
            Assert.Equal(EquipError.None, s.WearJewelry(rich, s.FromItem("item.jw_evil_eye_gem")));
            Assert.Equal(EquipError.None, s.WearJewelry(rich, s.FromItem("item.jw_necklace_gem")));

            Assert.Equal(0.04, s.TradeBonus(plain), 6);    // bakır yüzük: basamak 1
            Assert.Equal(0.10, s.TradeBonus(rich), 6);     // taşlı altın: etkili basamak 4
            Assert.Equal(0.06, s.CourtBonus(plain), 6);
            Assert.Equal(0.12, s.CourtBonus(rich), 6);     // altın küpe: basamak 3
            Assert.Equal(0.9, s.AccidentFactor(plain), 6);
            Assert.Equal(0.8, s.AccidentFactor(rich), 6);
            Assert.Equal(0, s.JewelryJoy(plain));
            Assert.Equal(ClothingService.MaxJewelryJoy, s.JewelryJoy(rich));   // gerdanlık 3 + altınlar, tavan 4

            s.UpdateMoods(rich, false); s.UpdateMoods(plain, false);
            Assert.True(rich.Mood.Has(ClothingService.JewelryFineMood));
            Assert.False(plain.Mood.Has(ClothingService.JewelryJoyMood));

            // Çıkarılan takı çekmeceye döner, başkası takabilir.
            var ring = s.JewelryOwnedBy(rich).First(i => s.JewelryOf(i).Place == JewelryPlace.Hand);
            s.TakeOffJewelry(rich, ring);
            Assert.Contains(ring, s.UnwornJewelry());
            Assert.Equal(0.0, s.TradeBonus(rich), 6);
            Assert.Equal(EquipError.None, s.WearJewelry(plain, ring));
            Assert.Equal(0.14, s.TradeBonus(plain), 6);    // bakır 0,04 + taşlı altın 0,10
        }

        [Fact]
        public void NazarBoncugu_KazaOlasiliginiDusurur()
        {
            var inj = new InjurySystem(new Population(), 5);
            var c = Person();
            var chop = new InteractionDef { Id = "chop", Skill = SkillType.Woodcutting };
            var def = inj.Defs.First();
            double before = inj.Chance(c, chop, def, false);
            inj.LuckFactor = _ => 0.8;
            Assert.Equal(before * 0.8, inj.Chance(c, chop, def, false), 9);
        }

        [Fact]
        public void Taki_EnCok5_IkiYuzuk_KeyifTavani4()
        {
            var s = Svc();
            var c = Person();
            Assert.Equal(EquipError.None, s.Equip(c, s.CreateJewelry("ring"), OutfitKind.Daily));
            Assert.Equal(EquipError.None, s.Equip(c, s.CreateJewelry("ring"), OutfitKind.Daily));
            Assert.Equal(EquipError.JewelryLimit, s.Equip(c, s.CreateJewelry("ring"), OutfitKind.Daily));
            Assert.Equal(EquipError.None, s.Equip(c, s.CreateJewelry("earring", 2), OutfitKind.Daily));
            Assert.Equal(EquipError.None, s.Equip(c, s.CreateJewelry("glass_beads"), OutfitKind.Daily));
            Assert.Equal(EquipError.None, s.Equip(c, s.CreateJewelry("nose_ring"), OutfitKind.Daily));
            Assert.Equal(EquipError.JewelryLimit, s.Equip(c, s.CreateJewelry("bracelet"), OutfitKind.Daily));
            // broş ve nazar boncuğu sınıra girmez
            Assert.Equal(EquipError.None, s.Equip(c, s.CreateJewelry("brooch"), OutfitKind.Daily));
            Assert.Equal(EquipError.None, s.Equip(c, s.CreateJewelry("evil_eye"), OutfitKind.Daily));
            // aynı yerdeki değiştirilir (boyun)
            Assert.Equal(EquipError.None, s.Equip(c, s.CreateJewelry("cord_necklace", 1, gem: true), OutfitKind.Daily));
            Assert.Equal(1 + 2, s.JewelryJoy(c));   // burun 1 + taşlı ip kolye 2
            Assert.Equal(0.09, s.CourtBonus(c), 6);
            Assert.Equal(0.08, s.TradeBonus(c), 6);
            Assert.Equal(0.9, s.AccidentFactor(c));
            Assert.True(s.JewelryJoy(c) <= ClothingService.MaxJewelryJoy);
            s.UpdateMoods(c, false);
            Assert.True(c.Mood.Has(ClothingService.JewelryMood));
        }

        [Fact]
        public void Taki_MuhurYuzuguHaneReisiyleGecer()
        {
            var s = Svc();
            var old = Person(1, 75); var heir = Person(2, 40);
            var seal = s.CreateJewelry("seal_ring", 2);
            s.Equip(old, seal, OutfitKind.Daily);
            Assert.True(s.HasSeal(old));
            Assert.Equal(1, s.PassSeal(old, heir));
            Assert.Equal(heir.Id, seal.OwnerId);
            Assert.True(seal.Heirloom);
            s.Dress(heir, default);
            Assert.True(s.HasSeal(heir));
            Assert.False(s.HasSeal(old));
        }

        // ---------- eskime, yama, söküm, kir, çamaşır ----------

        [Fact]
        public void Eskime_GunlukBir_IsIki_CocukBucukKat_SapsizSolar()
        {
            var s = Svc();
            var c = Person();
            c.Duty = DutyKind.Farm;
            var daily = s.Create("tunic"); var work = s.Create("work_tunic");
            daily.Color = new ClothColor(DyePalette.Madder, ClothTone.Dark);
            daily.Fixed = false;
            s.Equip(c, daily, OutfitKind.Daily); s.Equip(c, work, OutfitKind.Work);
            for (int i = 0; i < 10; i++) s.DailyWear(c);
            Assert.Equal((double)(90f), (double)(daily.Durability), 3);
            Assert.Equal((double)(80f), (double)(work.Durability), 3);
            Assert.Equal(ClothTone.Mid, daily.Color.Tone);   // 10 giyişte bir ton
            var kid = Person(3, 15);
            var kt = s.Create("child_tunic");
            s.Equip(kid, kt, OutfitKind.Daily);
            s.DailyWear(kid);
            Assert.Equal((double)(98.5f), (double)(kt.Durability), 3);
        }

        [Fact]
        public void Yama_KirkPuan_Yirtik_Sokum()
        {
            var s = Svc();
            var t = s.Create("tunic");
            t.Durability = 20f;
            Assert.True(t.NeedsPatch);
            Assert.True(s.Patch(t));
            Assert.Equal(60f, t.Durability);
            Assert.Equal(1, t.Patches);
            t.Durability = 0f;
            Assert.True(t.Torn);
            Assert.False(s.Patch(t));
            var c = Person();
            Assert.Equal(EquipError.Torn, s.Equip(c, t, OutfitKind.Daily));
            Assert.Equal(2, s.Unpick(t.Id, thrifty: false));     // 2 kumaş × 2 parça × %50
            Assert.Null(s.Get(t.Id));
            var cloak = s.Create("long_cloak");
            Assert.Equal(4, s.Unpick(cloak.Id, thrifty: true));   // 3 × 2 × %75
        }

        [Fact]
        public void Kir_IsKirletir_OnlukKoruru_Yikama()
        {
            var s = Svc();
            var c = Person();
            var t = s.Create("tunic");
            s.Equip(c, t, OutfitKind.Daily);
            s.Dress(c, default);
            for (int i = 0; i < 4; i++) s.AddDirt(c, SkillType.Mining);
            Assert.True(t.IsDirty);
            s.UpdateMoods(c, false);
            Assert.True(c.Mood.Has(ClothingService.DirtyClothes));
            // giyiliyken sepette değil; çıkarınca sepette
            Assert.Empty(s.Basket(new[] { c }));
            s.Unequip(c, t.Id, OutfitKind.Daily);
            Assert.Single(s.Basket(new[] { c }));
            Assert.Equal(1, s.WashAll(new[] { c }, soap: false));
            Assert.Equal((double)(12f), (double)(t.Dirt), 3);   // kül suyu %30 bırakır
            s.Wash(t, soap: true);
            Assert.Equal(0f, t.Dirt);
            // bez önlük yemekte kiri tutar
            var cook = Person(2);
            var shirt = s.Create("linen_undershirt");
            s.Equip(cook, shirt, OutfitKind.Daily);
            s.Equip(cook, s.Create("cloth_apron"), OutfitKind.Daily);
            s.Dress(cook, default);
            s.AddDirt(cook, SkillType.Cooking);
            Assert.Equal(0f, shirt.Dirt);
        }

        // ---------- evre ----------

        [Fact]
        public void Evre_KuculenGiysiSandiga_KardeseSakla()
        {
            var s = Svc();
            var kid = Person(1, 15);
            var ct = s.Create("child_tunic");
            s.Equip(kid, ct, OutfitKind.Daily);
            kid.SetAge(25);   // Ergen
            var outgrown = s.OnStageChanged(kid);
            Assert.Contains(ct, outgrown);
            Assert.True(ct.Outgrown);
            Assert.Equal(0, ct.OwnerId);
            var sib = Person(2, 14);
            Assert.True(s.KeepForSibling(ct, sib));
            Assert.Equal(EquipError.None, s.Equip(sib, ct, OutfitKind.Daily));
        }

        [Fact]
        public void Evre_IlkBayramlik_Buyudum()
        {
            var s = Svc();
            var teen = Person(1, 25);
            var suit = s.Create("feast_entari");
            Assert.True(s.GiveFirstFeast(teen, suit));
            Assert.True(teen.Mood.Has(ClothingService.GrownUp));
            Assert.True(s.WardrobeOf(teen).Has(OutfitKind.Festive));
        }

        // ---------- giyim uyumu ----------

        [Fact]
        public void Uyum_HaddiniAsmak_GundeBirTepki_ElmasSayilmaz()
        {
            var s = Svc();
            var c = Person();
            var v = Person(50); v.IsVillager = true;
            var silk = s.Create("kaftan");   // Tüccar
            s.Equip(c, silk, OutfitKind.Daily);
            s.Dress(c, default);
            Assert.Equal(OutfitReaction.Overreach, s.TryReact(c, v, 0, ClothingTier.Peasant, false, 10.2));
            Assert.True(c.Mood.Has(ClothingService.Ashamed));
            Assert.Equal(OutfitReaction.None, s.TryReact(c, v, 0, ClothingTier.Peasant, false, 10.6));   // aynı gün
            c.Mood.Remove(ClothingService.Ashamed);
            silk.Diamond = true;
            Assert.Equal(OutfitReaction.None, s.Evaluate(c, ClothingTier.Peasant, false, 11.1));
        }

        [Fact]
        public void Uyum_Ovgu_YakinlikUzatir_CocukMuaf_Bayramda()
        {
            var s = Svc();
            var c = Person();
            var v = Person(50); v.IsVillager = true;
            var t = s.Create("long_tunic", quality: 2);   // Zanaatkâr, kaliteli
            s.Equip(c, t, OutfitKind.Daily);
            s.Dress(c, default);
            Assert.Equal(OutfitReaction.Praise, s.TryReact(c, v, 80, ClothingTier.Peasant, false, 20.0));
            Assert.True(c.Mood.Has(ClothingService.Confident));
            Assert.Equal(OutfitReaction.None, s.Evaluate(c, ClothingTier.Noble, false, 21.0) == OutfitReaction.Praise ? OutfitReaction.Praise : OutfitReaction.None);
            // bayramda iş takımı pasaklılıktır
            s.Equip(c, s.Create("work_tunic"), OutfitKind.Work);
            s.Dress(c, new ClothingContext { Working = true });
            Assert.Equal(OutfitReaction.Sloppy, s.Evaluate(c, ClothingTier.Peasant, true, 22.0));
            // çocuk tepki almaz
            var kid = Person(3, 15);
            s.Equip(kid, s.Create("kaftan"), OutfitKind.Daily);
            Assert.Equal(OutfitReaction.None, s.Evaluate(kid, ClothingTier.Peasant, false, 22.0));
        }

        [Fact]
        public void Uyum_MahcupEvdeUstunuDegistirir()
        {
            var s = Svc();
            var c = Person();
            var v = Person(50); v.IsVillager = true;
            var kaftan = s.Create("kaftan");
            var shirt = s.Create("linen_undershirt");
            s.Equip(c, shirt, OutfitKind.Daily);
            s.Equip(c, s.Create("salvar"), OutfitKind.Daily);
            s.Equip(c, kaftan, OutfitKind.Daily);   // gövdede gömleğin yerine geçer
            s.Dress(c, default);
            s.TryReact(c, v, 0, ClothingTier.Peasant, false, 5.0);
            Assert.True(s.ChangeAfterRemark(c, ClothingTier.Peasant));
            Assert.False(c.Mood.Has(ClothingService.Ashamed));
            Assert.DoesNotContain(kaftan, s.Worn(c));
        }

        // ---------- moodlet ----------

        [Fact]
        public void Moodlet_KendiDiktigim_AtalarimVeGebelikBonusu()
        {
            var s = Svc();
            var c = Person();
            var mine = s.Create("plain_entari", maker: c, ownLabor: true);
            s.Equip(c, mine, OutfitKind.Daily);
            s.Dress(c, default);
            s.UpdateMoods(c, false);
            Assert.True(c.Mood.Has(ClothingService.SelfSewn));
            Assert.False(c.Mood.Has(ClothingService.OurLabor));   // kendi diktiği daha güçlü olanı alır
            var h = s.Create("long_veil");
            h.Heirloom = true;
            s.Equip(c, h, OutfitKind.Daily);
            s.UpdateMoods(c, false);
            Assert.True(c.Mood.Has(ClothingService.Ancestors));
            // gebelik: entari "Rahatım", kuşakla "Rahatım +"; giyilmezse hiçbir eksi yok
            Assert.True(c.TryStartPregnancy(9));
            s.Equip(c, s.Create("maternity_entari"), OutfitKind.Maternity);
            s.Dress(c, default);
            s.UpdateMoods(c, false);
            Assert.True(c.Mood.Has(ClothingService.Comfy));
            s.Equip(c, s.Create("belly_band"), OutfitKind.Maternity);
            s.UpdateMoods(c, false);
            Assert.True(c.Mood.Has(ClothingService.ComfyPlus));
            Assert.False(c.Mood.Has(ClothingService.Comfy));
            var other = Person(2);
            Assert.True(other.TryStartPregnancy(9));
            int before = other.Mood.Total;
            s.UpdateMoods(other, false);
            Assert.Equal(before, other.Mood.Total);
            Assert.True(other.IsPregnant);
        }

        // ---------- kayıt ----------

        [Fact]
        public void Kayit_OrnekVeTakimlarGeriGelir()
        {
            var s = Svc();
            var c = Person();
            var t = s.Create("kaftan", new ClothColor("oxblood", ClothTone.Single), 3, c, 4.5, true);
            t.Cuts[CutKind.Slit] = "side";
            t.Trim = new ClothColor(DyePalette.Saffron, ClothTone.Light);
            t.EmbroideryMotif = "lale"; t.EmbroideryPlace = "yaka"; t.EmbroideryThread = "ipek";
            t.Durability = 55f; t.Dirt = 12f; t.Patches = 1; t.Heirloom = true; t.HeirloomNote = "Ninemin kaftanı";
            s.Equip(c, t, OutfitKind.Festive);
            var r = s.CreateJewelry("bracelet", 3, true);
            s.Equip(c, r, OutfitKind.Festive);
            s.WardrobeOf(c).RainCloak = t.Id;
            var save = s.Export();
            var json = System.Text.Json.JsonSerializer.Serialize(save, new System.Text.Json.JsonSerializerOptions { IncludeFields = true });
            var back = System.Text.Json.JsonSerializer.Deserialize<ClothingSave>(json, new System.Text.Json.JsonSerializerOptions { IncludeFields = true });
            var s2 = Svc();
            s2.Import(back);
            var t2 = s2.Get(t.Id);
            Assert.Equal(t.Color, t2.Color);
            Assert.Equal(t.Trim, t2.Trim);
            Assert.Equal("side", t2.Cuts[CutKind.Slit]);
            Assert.Equal(3, t2.Quality);
            Assert.Equal(c.Name, t2.MakerName);
            Assert.True(t2.OwnLabor && t2.Heirloom);
            Assert.Equal("Ninemin kaftanı", t2.HeirloomNote);
            Assert.Equal(55f, t2.Durability);
            Assert.Equal("lale", t2.EmbroideryMotif);
            Assert.Equal(t.Id, s2.WardrobeOf(c).Get(OutfitKind.Festive).Slots[ClothingSlot.Body]);
            Assert.Contains(r.Id, s2.WardrobeOf(c).Get(OutfitKind.Festive).Jewelry);
            Assert.True(s2.Get(r.Id).Gem);
            Assert.Equal(t.Id, s2.WardrobeOf(c).RainCloak);
            Assert.NotEqual(t.Id, s2.Create("tunic").Id);   // kimlik sayacı da geri gelir
        }

        // ---------- yaşam sistemiyle ----------

        private static (LifeSystems life, Household house, Population pop, WorldState w) Life()
        {
            var db = Db();
            var w = new WorldState(new GridMap(10, 10));
            var house = new Household(100);
            var pop = new Population();
            var life = new LifeSystems(db, w, house, pop, new GameClock());
            return (life, house, pop, w);
        }

        [Fact]
        public void Yasam_DikilenGiysiOrnekOlur_KendiEmegimiz()
        {
            var (life, house, pop, w) = Life();
            var c = Person();
            c.Skills.RestoreXp(SkillType.Tailoring, SkillRules.XpForLevel(5));
            var table = w.PlaceItem(Db().Items.Get("item.sewing_table"), new GridPos(2, 2));
            var craft = table.Def.Interactions.First(i => i.IsCraft && i.Produce.Any(p => p.ItemId == "item.tunic"));
            house.AddItem("item.tunic", 1);   // tarifin ürettiği
            life.OnInteractionApplied(c, craft);
            Assert.Equal(0, house.Count("item.tunic"));
            var made = life.Clothing.All.Single();
            Assert.Equal("tunic", made.ModelId);
            Assert.Equal(c.Name, made.MakerName);
            Assert.True(made.OwnLabor);
            Assert.Equal(1, made.Quality);
        }

        [Fact]
        public void Yasam_YeniUyeyeSadeGiysi_HepsiniYika()
        {
            var (life, house, pop, w) = Life();
            var c = Person();
            pop.Add(c);
            life.GiveStarterClothes(c);
            Assert.True(life.Clothing.HasWardrobe(c));
            Assert.NotEmpty(life.Clothing.Worn(c));
            var dirty = life.Clothing.Create("tunic");
            dirty.Dirt = 50f;
            house.AddItem("item.soap", 1);
            Assert.Equal(1, life.WashAllClothes(c));
            Assert.Equal(0f, dirty.Dirt);
            Assert.Equal(0, house.Count("item.soap"));
            Assert.True(life.TrySting(c, roll: 0.0));   // başlıksız acemi sokulur
        }

        [Fact]
        public void Yasam_AriciBasligiSokmayiAzaltir()
        {
            var (life, _, pop, _) = Life();
            var c = Person();
            pop.Add(c);
            life.Clothing.Equip(c, life.Clothing.Create("beekeeper_veil"), OutfitKind.Daily);
            life.Clothing.Dress(c, default);
            Assert.False(life.TrySting(c, roll: LifeSystems.BeeStingChance * 0.5));
        }
    }
}
