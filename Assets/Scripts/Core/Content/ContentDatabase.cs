using System;
using System.Collections.Generic;
using Thatchwick.Core.Life;

namespace Thatchwick.Core.Content
{
    /// <summary>Tüm içerik kayıtlarının toplandığı yer ve doğrulama.</summary>
    public sealed class ContentDatabase
    {
        public ContentRegistry<ItemDef> Items { get; } = new ContentRegistry<ItemDef>();
        public ContentRegistry<TraitDef> Traits { get; } = new ContentRegistry<TraitDef>();
        public ContentRegistry<BranchDef> Branches { get; } = new ContentRegistry<BranchDef>();
        public ContentRegistry<ProjectDef> Projects { get; } = new ContentRegistry<ProjectDef>();
        public ContentRegistry<VillagerDef> Villagers { get; } = new ContentRegistry<VillagerDef>();
        public ContentRegistry<FestivalDef> Festivals { get; } = new ContentRegistry<FestivalDef>();
        public ContentRegistry<GoalDef> Goals { get; } = new ContentRegistry<GoalDef>();
        public ContentRegistry<ToolDef> Tools { get; } = new ContentRegistry<ToolDef>();
        public ContentRegistry<RoomTemplateDef> Templates { get; } = new ContentRegistry<RoomTemplateDef>();
        public ContentRegistry<AnimalSpeciesDef> Animals { get; } = new ContentRegistry<AnimalSpeciesDef>();
        /// <summary>Yapı modu malzeme ve parça kataloğu (duvar, kapı, pencere, zemin, çatı, kaplama).</summary>
        public ContentRegistry<BuildPartDef> Parts { get; } = new ContentRegistry<BuildPartDef>();
        /// <summary>Beceri seviyesi → açılan tablosu (belge 45; life.json "skillUnlocks").</summary>
        public List<SkillUnlockDef> SkillUnlocks { get; } = new List<SkillUnlockDef>();
        /// <summary>Etkileşimlerin başvurduğu moodlet kataloğu (doğrulama için).</summary>
        public Mood.MoodCatalog Moods { get; set; } = Mood.MoodCatalog.Default;

        /// <summary>
        /// İçerik hatalarını okunur metin olarak döner (boşsa içerik geçerli).
        /// Veri yüklenirken ve CI'da çalıştırılır; hatalı içerik oyunu sessizce bozmasın.
        /// </summary>
        public List<string> Validate()
        {
            var errors = new List<string>();

            foreach (var item in Items.All)
            {
                if (string.IsNullOrWhiteSpace(item.DisplayName))
                    errors.Add($"{item.Id}: görünen ad boş.");

                if (item.Width < 1 || item.Width > ItemDef.MaxSize || item.Height < 1 || item.Height > ItemDef.MaxSize)
                    errors.Add($"{item.Id}: boyut 1-{ItemDef.MaxSize} arasında olmalı.");
                if (item.Price < 0) errors.Add($"{item.Id}: fiyat negatif olamaz.");
                if (item.HeatOutput < 0f) errors.Add($"{item.Id}: ısı gücü negatif olamaz.");
                if (item.HeatOutput > 0f && item.Placement != ItemPlacement.Floor) errors.Add($"{item.Id}: ısı kaynağı yere konan eşya olmalı.");
                if (item.UnlockLevel < 1 || item.UnlockLevel > 20) errors.Add($"{item.Id}: açılma seviyesi 1-20 arasında olmalı.");
                if (item.Storage < 0) errors.Add($"{item.Id}: depo kapasitesi negatif olamaz.");
                if (item.Storage > 0 && item.Placement != ItemPlacement.Floor) errors.Add($"{item.Id}: depo eşyası yere konan eşya olmalı.");
                if (item.GrowDays < 0f) errors.Add($"{item.Id}: büyüme süresi negatif olamaz.");
                if ((item.NeedsTilledSoil || item.DiesInWinter) && !item.IsPlant) errors.Add($"{item.Id}: tilled/diesInWinter yalnızca bitkide kullanılır.");
                if (item.IsPlant && item.Placement != ItemPlacement.Floor) errors.Add($"{item.Id}: bitki yere konan eşya olmalı.");
                if (item.PlantsAs != null)
                {
                    if (item.Placement != ItemPlacement.Resource) errors.Add($"{item.Id}: dikilebilen fidan bir kaynak olmalı.");
                    if (!Items.TryGet(item.PlantsAs, out var plant)) errors.Add($"{item.Id}: dikilince oluşacak '{item.PlantsAs}' bulunamadı.");
                    else if (!plant.IsPlant) errors.Add($"{item.Id}: '{item.PlantsAs}' bir bitki değil (growDays yok).");
                }
                if (item.Placement == ItemPlacement.Resource && (item.Width != 1 || item.Height != 1 || item.SurfaceSlots.Count > 0 || item.Interactions.Count > 0))
                    errors.Add($"{item.Id}: kaynak 1x1 olmalı, yuvası ve etkileşimi olamaz.");
                if (item.Placement == ItemPlacement.Wall)
                {
                    if (item.Width != 1 || item.Height != 1) errors.Add($"{item.Id}: duvar eşyası 1x1 olmalı (genişlik wallWidth ile verilir).");
                    if (item.IsHousing) errors.Add($"{item.Id}: duvar eşyası barınak olamaz.");
                }
                if (item.IsCeiling && (item.Width != 1 || item.Height != 1 || item.IsHousing || item.IsStairs))
                    errors.Add($"{item.Id}: tavan eşyası 1x1 olmalı.");
                // Ev dekorasyonu (27).
                if (item.WallWidth < 1 || item.WallWidth > 3) errors.Add($"{item.Id}: wallWidth 1-3 arasında olmalı.");
                if (item.WallWidth > 1 && item.Placement != ItemPlacement.Wall) errors.Add($"{item.Id}: wallWidth yalnızca duvar eşyasında kullanılır.");
                if (item.WallBands.Count > 0 && item.Placement != ItemPlacement.Wall) errors.Add($"{item.Id}: wallBands yalnızca duvar eşyasında kullanılır.");
                if (item.Weight < 1 || item.Weight > 12) errors.Add($"{item.Id}: yük 1-12 arasında olmalı.");
                if (item.MaxLoad < 0 || item.MaxLoad > 100) errors.Add($"{item.Id}: maxLoad 0-100 arasında olmalı.");
                if (item.MaxLoad > 0 && item.SurfaceSlots.Count == 0) errors.Add($"{item.Id}: maxLoad yalnızca yuvası olan eşyada kullanılır.");
                if (item.Beauty < -10 || item.Beauty > 30) errors.Add($"{item.Id}: güzellik -10 ile 30 arasında olmalı.");
                if (item.Material != null && Array.IndexOf(Decor.Materials, item.Material) < 0) errors.Add($"{item.Id}: bilinmeyen malzeme '{item.Material}'.");
                foreach (var f in item.FitsOn)
                    if (Array.IndexOf(Decor.SlotKinds, f) < 0) errors.Add($"{item.Id}: bilinmeyen yuva türü '{f}' (fitsOn).");
                if (item.FitsOn.Count > 0 && item.Placement != ItemPlacement.Surface) errors.Add($"{item.Id}: fitsOn yalnızca dekorda (Surface) kullanılır.");
                if (item.TarnishDays > 0f && !item.IsMirror) errors.Add($"{item.Id}: kararma yalnızca aynada olur.");
                if (item.IsMirror && item.Placement == ItemPlacement.Resource) errors.Add($"{item.Id}: ayna yerleştirilebilir olmalı.");
                // D2: tarz, doldurulan dekor, pencere, örtü.
                if (item.StyleTheme != null && Array.FindIndex(Decor.Themes, t => t.id == item.StyleTheme) < 0)
                    errors.Add($"{item.Id}: bilinmeyen tarz '{item.StyleTheme}'.");
                if (item.FillCount < 0 || item.FillCount > 12) errors.Add($"{item.Id}: fillCount 0-12 arasında olmalı.");
                if ((item.FillCount > 0) != (item.FillWith.Count > 0)) errors.Add($"{item.Id}: fillWith ve fillCount birlikte yazılmalı.");
                if (item.IsFillDecor)
                {
                    // Yer eşyası da doldurulabilir: çeyiz sandığı dokumalarla dolar (D3b).
                    if (item.Placement == ItemPlacement.Resource || item.Placement == ItemPlacement.Rug || item.Placement == ItemPlacement.Ceiling)
                        errors.Add($"{item.Id}: yalnızca dekor, duvar ya da yer eşyası doldurulur.");
                    foreach (var f in item.FillWith)
                        if (!Items.TryGet(f, out var fd)) errors.Add($"{item.Id}: doldurulacak ürün '{f}' bulunamadı.");
                        else if (fd.Placement != ItemPlacement.Resource && !item.HasTag("display")) errors.Add($"{item.Id}: doldurulacak ürün '{f}' bir kaynak (envanter ürünü) olmalı.");
                }
                foreach (var it in item.Interactions)
                    if ((it.FillDecor || it.EatContents) && !item.IsFillDecor) errors.Add($"{item.Id}/{it.Id}: doldurma ve yeme yalnızca doldurulan dekorda olur.");
                if (item.WindowMount && item.Placement != ItemPlacement.Wall) errors.Add($"{item.Id}: window yalnızca duvar eşyasında kullanılır.");
                if (item.IsKeepsake && item.Price > 0) errors.Add($"{item.Id}: hatıra satın alınmaz; price 0 olmalı.");
                if (item.IsSeasonal && item.Placement == ItemPlacement.Resource) errors.Add($"{item.Id}: mevsim süsü yerleştirilen bir eşya olmalı.");
                if (item.WindowMount && item.WallWidth != 1) errors.Add($"{item.Id}: pencere eşyası 1 kenar olmalı.");
                if (item.Insulation < 0f || item.Insulation > 1f) errors.Add($"{item.Id}: insulation 0-1 arasında olmalı.");
                if (item.RequiresNearby != null)
                {
                    if (Items.TryGet(item.RequiresNearby, out var nearDef)) item.RequiresNearbyName = nearDef.DisplayName;
                    else errors.Add($"{item.Id}: requiresNear bilinmeyen eşya '{item.RequiresNearby}'.");
                }
                if (item.Insulation > 0f && item.Placement != ItemPlacement.Wall) errors.Add($"{item.Id}: insulation yalnızca duvar eşyasında kullanılır.");
                if (item.FitsKind(Decor.Cover) && item.EffectiveFits.Count > 1) errors.Add($"{item.Id}: örtü yalnızca örtü yuvasına konur.");
                if (item.SurfaceSlots.FindAll(sl => sl.Kind == Decor.Cover).Count > 1) errors.Add($"{item.Id}: en çok bir örtü yuvası olur.");
                if (item.IsHousing)
                {
                    if (item.HousingCapacity < 1) errors.Add($"{item.Id}: barınak kapasitesi en az 1 olmalı.");
                    if (item.Placement != ItemPlacement.Floor) errors.Add($"{item.Id}: barınak yere konan eşya olmalı.");
                }
                else if (item.HousingCapacity != 0) errors.Add($"{item.Id}: capacity yalnızca barınakta kullanılır.");
                if (item.Placement == ItemPlacement.Surface)
                {
                    if (item.Width != 1 || item.Height != 1) errors.Add($"{item.Id}: dekor 1x1 olmalı.");
                    if (item.SurfaceSlots.Count > 0) errors.Add($"{item.Id}: dekorun kendi yüzey yuvası olamaz.");
                }
                if (item.Comfort < 0 || item.Comfort > 50) errors.Add($"{item.Id}: konfor 0-50 arasında olmalı.");
                if (item.SpoilDays < 0f) errors.Add($"{item.Id}: bozulma süresi negatif olamaz.");
                if (item.SpoilDays > 0f && !item.HasTag("food") && !item.HasTag("ice")) errors.Add($"{item.Id}: bozulma yalnızca yiyecek (ya da buz) etiketli eşyada olur.");
                if (item.Preserves < 0f || item.Preserves > 1f) errors.Add($"{item.Id}: kiler çarpanı 0-1 arasında olmalı.");
                if (item.ForageMax < 0) errors.Add($"{item.Id}: toplama sınırı negatif olamaz.");
                if (item.IsForage && (item.Placement != ItemPlacement.Floor || !item.IsPlant && item.Interactions.Count == 0)) errors.Add($"{item.Id}: toplanabilir eşya yere konan ve etkileşimli olmalı.");
                if (item.GrowSeasons.Count > 0 && !item.IsPlant) errors.Add($"{item.Id}: growSeasons yalnızca bitkide kullanılır.");
                var slotIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (var slot in item.SurfaceSlots)
                {
                    if (string.IsNullOrWhiteSpace(slot.Id)) { errors.Add($"{item.Id}: kimliksiz yuva."); continue; }
                    if (!slotIds.Add(slot.Id)) errors.Add($"{item.Id}/{slot.Id}: yuva kimliği tekrar ediyor.");
                    if (slot.U < 0f || slot.U > 1f || slot.V < 0f || slot.V > 1f) errors.Add($"{item.Id}/{slot.Id}: U ve V 0-1 arasında olmalı.");
                    if (slot.Kind != null && Array.IndexOf(Decor.SlotKinds, slot.Kind) < 0) errors.Add($"{item.Id}/{slot.Id}: bilinmeyen yuva türü '{slot.Kind}'.");
                }

                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (var it in item.Interactions)
                {
                    string where = $"{item.Id}/{it.Id}";
                    if (string.IsNullOrWhiteSpace(it.Id))
                    {
                        errors.Add($"{item.Id}: kimliksiz etkileşim.");
                        continue;
                    }
                    if (!seen.Add(it.Id)) errors.Add($"{where}: etkileşim kimliği tekrar ediyor.");
                    bool pureWork = it.HasEffects || it.Skill.HasValue || it.Moodlet != null;
                    if (pureWork ? (it.Restore < 0f || it.Restore > 100f) : (it.Restore <= 0f || it.Restore > 100f))
                        errors.Add($"{where}: Restore 0-100 arasında olmalı (iş etkileşimi değilse sıfırdan büyük).");
                    if (it.RequiresGrown && !item.IsPlant) errors.Add($"{where}: requiresGrown yalnızca bitkide kullanılır.");
                    if (it.ConsumeItem != null && !Items.Contains(it.ConsumeItem)) errors.Add($"{where}: harcanan '{it.ConsumeItem}' bulunamadı.");
                    foreach (var c in it.Consume)
                    {
                        if (!Items.TryGet(c.ItemId, out var mat)) errors.Add($"{where}: malzeme '{c.ItemId}' bulunamadı.");
                        else if (mat.Placement != ItemPlacement.Resource) errors.Add($"{where}: malzeme '{c.ItemId}' bir kaynak olmalı.");
                        if (c.Count < 1) errors.Add($"{where}: malzeme adedi en az 1 olmalı.");
                    }
                    if (it.Moodlet != null && (Moods == null || !Moods.Contains(it.Moodlet))) errors.Add($"{where}: moodlet '{it.Moodlet}' bulunamadı.");
                    if (it.MinSkillLevel < 0 || it.MinSkillLevel > Characters.SkillRules.MaxLevel) errors.Add($"{where}: minLevel 0-{Characters.SkillRules.MaxLevel} arasında olmalı.");
                    if (it.MinSkillLevel > 0 && !it.Skill.HasValue) errors.Add($"{where}: minLevel için beceri gerekli.");
                    if ((it.AddFeed > 0 && !item.IsTrough || it.Collect || it.RequiresAnimal) && !item.IsHousing) errors.Add($"{where}: yem/toplama/hayvan yalnızca barınakta kullanılır.");
                    if (it.AddFeed > 0 && it.AllConsumed().Count == 0) errors.Add($"{where}: yem koymak için malzeme (consume) gerekli.");
                    if (it.AddFeed < 0) errors.Add($"{where}: addFeed negatif olamaz.");
                    foreach (var er in it.ExtraRestore)
                        if (er.Amount <= 0f || er.Amount > 100f) errors.Add($"{where}: ek doygunluk 0-100 arasında olmalı.");
                    if (it.RequiresLit && item.HeatOutput <= 0f) errors.Add($"{where}: requiresLit yalnızca ısı kaynağında kullanılır.");
                    if (!string.IsNullOrEmpty(it.MoodId) && (it.MoodValue == 0 || it.MoodDays <= 0.0)) errors.Add($"{where}: ruh hali etkisi için moodValue ve moodDays gerekli.");
                    if (!string.IsNullOrEmpty(it.Tool) && !Tools.Contains(it.Tool)) errors.Add($"{where}: bilinmeyen alet '{it.Tool}'.");
                    foreach (var p in it.Produce)
                    {
                        if (!Items.Contains(p.ItemId)) errors.Add($"{where}: üretilen '{p.ItemId}' bulunamadı.");
                        if (p.Count < 1) errors.Add($"{where}: üretim adedi en az 1 olmalı.");
                        if (p.Chance <= 0f || p.Chance > 1f) errors.Add($"{where}: üretim olasılığı 0-1 arasında olmalı.");
                        if (p.MinLevel < 0 || p.MaxLevel < 0 || (p.MaxLevel > 0 && p.MinLevel > p.MaxLevel)) errors.Add($"{where}: üretim seviye aralığı geçersiz.");
                        if ((p.MinLevel > 0 || p.MaxLevel > 0) && !it.Skill.HasValue) errors.Add($"{where}: seviye sınırlı üretim için etkileşimde beceri gerekli.");
                    }
                    if (it.DurationDays <= 0.0) errors.Add($"{where}: süre sıfırdan büyük olmalı.");
                    if (it.Skill.HasValue && it.SkillXp <= 0f) errors.Add($"{where}: beceri verilmiş ama skillXp sıfırdan büyük değil.");
                    if (!it.Skill.HasValue && it.SkillXp != 0f) errors.Add($"{where}: skillXp var ama beceri yok.");
                }
            }

            foreach (var a in Animals.All)
            {
                if (string.IsNullOrWhiteSpace(a.DisplayName)) errors.Add($"{a.Id}: görünen ad boş.");
                if (a.Price < 0) errors.Add($"{a.Id}: fiyat negatif olamaz.");
                if (a.FeedPerDay < 0f) errors.Add($"{a.Id}: yem negatif olamaz.");
                if (a.Produces != null && !Items.Contains(a.Produces)) errors.Add($"{a.Id}: ürün '{a.Produces}' bulunamadı.");
                if (a.Meat != null && !Items.Contains(a.Meat)) errors.Add($"{a.Id}: et '{a.Meat}' bulunamadı.");
                if (a.MatureDays < 0f || a.MatureDays >= a.LifespanDays) errors.Add($"{a.Id}: erişkinlik yaşı 0 ile ömür arasında olmalı.");
                bool housed = false;
                foreach (var item in Items.All) if (item.HousingKind == a.Housing) { housed = true; break; }
                if (!housed) errors.Add($"{a.Id}: '{a.Housing}' türünde barınak eşyası yok.");
            }

            ValidateParts(errors);
            ValidateFarm(errors);
            ValidateLife(errors);
            return errors;
        }

        private void ValidateFarm(List<string> errors)
        {
            var roles = new HashSet<string> { AnimalRoles.Water, AnimalRoles.Bedding, AnimalRoles.Collector, AnimalRoles.Feeder, AnimalRoles.Bell };
            foreach (var item in Items.All)
            {
                if (item.AnimalRole != null && !roles.Contains(item.AnimalRole)) errors.Add($"{item.Id}: bilinmeyen hayvan rolü '{item.AnimalRole}'.");
                if (item.AnimalComfort < 0 || item.AnimalComfort > 10) errors.Add($"{item.Id}: hayvan konforu 0-10 arasında olmalı.");
                if (item.RequiresIndoor && item.Placement != ItemPlacement.Floor) errors.Add($"{item.Id}: requiresIndoor yalnızca yere konan eşyada.");
                if (item.IsTrough && item.IsHousing) errors.Add($"{item.Id}: yemlik aynı zamanda barınak olamaz.");
                foreach (var it in item.Interactions)
                    if (it.AddIce > 0f && !item.IsIceBox) errors.Add($"{item.Id}.{it.Id}: buz yalnızca buzluğa konur.");
                if (item.NeedsWaterSource && item.WaterRadius <= 0) errors.Add($"{item.Id}: needsSource yalnızca sulayan eşyada.");
                if (item.IsFixture && WaterNetworkNode(item)) errors.Add($"{item.Id}: tesisat (waterUse) ağ parçası olamaz: suyu iletmez.");
                if (item.WaterUse > 12) errors.Add($"{item.Id}: waterUse en çok 12 (kurşun boru kapasitesi).");
                if (item.IsTap && !item.IsFixture) errors.Add($"{item.Id}: tap yalnızca tesisatta (waterUse > 0).");
                if (item.CisternCapacity > 0 && item.WaterSource) errors.Add($"{item.Id}: sarnıç ayrıca waterSource olmaz.");
                if (item.WaterFlow != 0 && !item.WaterSource) errors.Add($"{item.Id}: waterFlow yalnızca su kaynağında.");
                foreach (var it in item.Interactions)
                    if (it.NeedsPipedWater && !item.IsFixture) errors.Add($"{item.Id}.{it.Id}: piped yalnızca tesisatta.");
                if (item.AutoProcess != null)
                {
                    if (!Items.Contains(item.AutoProcess.Item)) errors.Add($"{item.Id}: otomatik işin girdisi '{item.AutoProcess.Item}' yok.");
                    if (!Items.Contains(item.AutoProcess.Produces)) errors.Add($"{item.Id}: otomatik işin ürünü '{item.AutoProcess.Produces}' yok.");
                }
            }
            foreach (var a in Animals.All)
            {
                if (a.Hide != null && !Items.Contains(a.Hide)) errors.Add($"{a.Id}: deri '{a.Hide}' bulunamadı.");
                if (a.Rideable && (a.RideSpeed < 1f || a.RideSpeed > 4f)) errors.Add($"{a.Id}: binek hızı 1-4 arasında olmalı.");
                if (a.Space <= 0f) errors.Add($"{a.Id}: hayvan başına alan pozitif olmalı.");
            }
        }

        private void ValidateParts(List<string> errors)
        {
            foreach (var p in Parts.All)
                if (p.MaterialItem != null && !Items.Contains(p.MaterialItem)) errors.Add($"{p.Id}: malzeme '{p.MaterialItem}' bulunamadı.");
            var defaults = new Dictionary<string, int>(StringComparer.Ordinal);
            var finishGroups = new HashSet<string>(StringComparer.Ordinal);
            foreach (var p in Parts.All)
            {
                if (string.IsNullOrWhiteSpace(p.DisplayName)) errors.Add($"{p.Id}: görünen ad boş.");
                if (p.Price < 0) errors.Add($"{p.Id}: fiyat negatif olamaz.");
                if (p.UnlockLevel < 1 || p.UnlockLevel > 20) errors.Add($"{p.Id}: açılma seviyesi 1-20 arasında olmalı.");
                if (!p.TryGetRgb(out _, out _, out _)) errors.Add($"{p.Id}: renk #RRGGBB olmalı.");
                if (p.HeatLoss <= 0f || p.HeatLoss > 3f) errors.Add($"{p.Id}: ısı kaybı çarpanı 0-3 arasında olmalı.");
                if (p.Comfort < 0 || p.Comfort > 10) errors.Add($"{p.Id}: konfor 0-10 arasında olmalı.");
                foreach (var lv in p.Levels)
                    if (!World.GridMap.IsValidLevel(lv)) errors.Add($"{p.Id}: geçersiz kat {lv}.");
                if (p.Kind == BuildPartKind.ItemFinish)
                {
                    if (string.IsNullOrEmpty(p.Group)) errors.Add($"{p.Id}: mobilya kaplamasının grubu (group) yok.");
                    else finishGroups.Add(p.Group);
                    if (p.PriceFactor < 0.5f || p.PriceFactor > 3f) errors.Add($"{p.Id}: fiyat çarpanı 0,5-3 arasında olmalı.");
                }
                else if (p.Group != null) errors.Add($"{p.Id}: group yalnızca mobilya kaplamasında kullanılır.");
                // Varsayılan tür başına birdir; zeminde temel tür, kaplamada grup başına.
                if (p.IsDefault)
                {
                    string key = p.Kind == BuildPartKind.Floor ? $"{p.Kind}:{p.Base}" : p.Kind == BuildPartKind.ItemFinish ? $"{p.Kind}:{p.Group}" : p.Kind.ToString();
                    defaults.TryGetValue(key, out int n);
                    defaults[key] = n + 1;
                }
            }
            foreach (var kv in defaults)
                if (kv.Value > 1) errors.Add($"Yapı parçası: {kv.Key} için birden çok varsayılan var.");
            foreach (var item in Items.All)
                if (item.FinishGroup != null && !finishGroups.Contains(item.FinishGroup))
                    errors.Add($"{item.Id}: '{item.FinishGroup}' kaplama grubunda hiç kaplama yok.");
            foreach (var item in Items.All)
                if (item.AccentGroup != null && !finishGroups.Contains(item.AccentGroup))
                    errors.Add($"{item.Id}: '{item.AccentGroup}' ikinci kaplama grubunda hiç kaplama yok.");
            foreach (var item in Items.All)
            {
                // döküntü yığını: "Topla"; saz hasır: "Ot serp" (belge 45)
                bool onlyCollect = item.Interactions.TrueForAll(i => i.CollectContents || i.HomeAction == "strew");
                if (item.IsRug && (!onlyCollect || item.SurfaceSlots.Count > 0 || item.IsStairs || item.IsHousing || item.HeatOutput > 0f || item.Storage > 0))
                    errors.Add($"{item.Id}: halı etkileşimsiz, yuvasız düz bir eşya olmalı.");
            }
        }

        private void ValidateLife(List<string> errors)
        {
            // Dal isteyen tarifler (D3: Camcı): dal var mı, adı kartta ve uyarıda görünsün.
            foreach (var item in Items.All)
                foreach (var it in item.Interactions)
                {
                    if (it.RequiresBranch == null) continue;
                    if (!Branches.TryGet(it.RequiresBranch, out var br)) errors.Add($"{item.Id}/{it.Id}: gereken dal '{it.RequiresBranch}' bulunamadı.");
                    else it.RequiresBranchName = br.DisplayName;
                }
            foreach (var t in Traits.All)
            {
                if (string.IsNullOrWhiteSpace(t.DisplayName)) errors.Add($"{t.Id}: görünen ad boş.");
                foreach (var c in t.Conflicts) if (!Traits.Contains(c)) errors.Add($"{t.Id}: çakışan özellik '{c}' bulunamadı.");
            }
            foreach (var b in Branches.All)
            {
                if (b.Level != 5 && b.Level != 10) errors.Add($"{b.Id}: dal seviyesi 5 ya da 10 olmalı.");
                if (b.Requires != null)
                {
                    if (!Branches.TryGet(b.Requires, out var first)) errors.Add($"{b.Id}: gereken dal '{b.Requires}' bulunamadı.");
                    else if (first.Skill != b.Skill || first.Level >= b.Level) errors.Add($"{b.Id}: gereken dal aynı beceride ve daha düşük seviyede olmalı.");
                }
                else if (b.Level == 10) errors.Add($"{b.Id}: 10. seviye dal bir 5. seviye dala bağlı olmalı (requires).");
            }
            foreach (var p in Projects.All)
            {
                if (p.Akce < 0) errors.Add($"{p.Id}: akçe maliyeti negatif olamaz.");
                if (p.Akce == 0 && p.Costs.Count == 0) errors.Add($"{p.Id}: proje maliyetsiz olamaz.");
                foreach (var c in p.Costs) if (!Items.Contains(c.ItemId)) errors.Add($"{p.Id}: maliyet kalemi '{c.ItemId}' bulunamadı.");
            }
            foreach (var v in Villagers.All)
            {
                if (string.IsNullOrWhiteSpace(v.Name)) errors.Add($"{v.Id}: köylü adı boş.");
                if (v.Age < 24 || v.Age > 100) errors.Add($"{v.Id}: yaş 24-100 oyun günü arasında olmalı.");
                foreach (var tr in v.Traits) if (!Traits.Contains(tr)) errors.Add($"{v.Id}: özellik '{tr}' bulunamadı.");
                foreach (var list in new[] { v.Loves, v.Likes, v.Dislikes })
                    foreach (var g in list)
                        if (!g.StartsWith("tag:") && !Items.Contains(g)) errors.Add($"{v.Id}: hediye zevki '{g}' bulunamadı.");
                foreach (var e in v.Events)
                {
                    if (e.MinAffection < 1 || e.MinAffection > 100) errors.Add($"{v.Id}/{e.Id}: yakınlık eşiği 1-100 olmalı.");
                    if (e.RewardItem != null && !Items.Contains(e.RewardItem)) errors.Add($"{v.Id}/{e.Id}: ödül eşyası '{e.RewardItem}' bulunamadı.");
                }
            }
            foreach (var i in Items.All)
                foreach (var vid in i.Variants)
                {
                    if (!Items.TryGet(vid, out var v)) { errors.Add($"{i.Id}: varyant '{vid}' bulunamadı."); continue; }
                    if (v.Width != i.Width || v.Height != i.Height || v.Placement != i.Placement) errors.Add($"{i.Id}: varyant '{vid}' aynı boyut ve yerleşimde olmalı.");
                    if (!v.Variants.Contains(i.Id)) errors.Add($"{i.Id}: varyant '{vid}' karşılıklı değil.");
                }
            foreach (var f in Festivals.All)
                if (f.Day < 0 || f.Day > 11) errors.Add($"{f.Id}: şenlik günü 0-11 arasında olmalı.");
            foreach (var g in Goals.All)
            {
                if (g.Target <= 0) errors.Add($"{g.Id}: hedef değeri sıfırdan büyük olmalı.");
                if (string.IsNullOrWhiteSpace(g.Metric)) errors.Add($"{g.Id}: ölçüt boş.");
                else if (Array.IndexOf(GoalDef.Metrics, g.Metric) < 0) errors.Add($"{g.Id}: bilinmeyen ölçüt '{g.Metric}'.");
            }
            foreach (var t in Tools.All)
            {
                if (t.Tiers.Count == 0) errors.Add($"{t.Id}: alet kademesi yok.");
                foreach (var tier in t.Tiers) if (tier.Akce < 0 || tier.Firewood < 0) errors.Add($"{t.Id}: kademe maliyeti negatif olamaz.");
            }
            foreach (var t in Templates.All)
            {
                if (t.Width < 2 || t.Height < 2 || t.Width > 12 || t.Height > 12) errors.Add($"{t.Id}: şablon boyutu 2-12 arasında olmalı.");
                int sideLen = (t.DoorSide == TemplateSide.North || t.DoorSide == TemplateSide.South) ? t.Width : t.Height;
                if (t.DoorOffset < 0 || t.DoorOffset >= sideLen) errors.Add($"{t.Id}: kapı konumu duvar dışında.");
                foreach (var w in t.Windows) if (w < 0 || w >= sideLen) errors.Add($"{t.Id}: pencere konumu duvar dışında.");
                if (!string.IsNullOrWhiteSpace(t.Floor) && !(Enum.TryParse<World.FloorType>(t.Floor, true, out var fl) && Enum.IsDefined(typeof(World.FloorType), fl)))
                    errors.Add($"{t.Id}: bilinmeyen zemin '{t.Floor}'.");
                foreach (var i in t.Items)
                {
                    if (!Items.Contains(i.ItemId)) errors.Add($"{t.Id}: şablon eşyası '{i.ItemId}' bulunamadı.");
                    if (i.X < 0 || i.Y < 0 || i.X >= t.Width || i.Y >= t.Height) errors.Add($"{t.Id}: '{i.ItemId}' şablon dışında.");
                }
            }
        }
        private static bool WaterNetworkNode(ItemDef d) => d.WaterSource || d.NeedsWaterSource || d.PowerSource || d.WaterGate || d.IsCistern || d.WaterHead;
    }
}
