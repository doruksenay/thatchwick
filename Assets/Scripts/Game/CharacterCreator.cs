using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Customization;
using Thatchwick.Core.Life;

namespace Thatchwick.Game
{
    /// <summary>
    /// Karakter yaratma ekranı 2.0 (IMGUI, doküman 14). Sekmeler: Kimlik, Kişilik, Ses, Beden, Yüz, Saç,
    /// İz ve süs, Kıyafet. Sekme başına zar ve kilit, genel zar (kilitli sekmeler korunur), geri al.
    /// Cinsiyet hiçbir seçeneği kilitlemez; boy yoktur. Geçici arayüz: PixelLab panelleri (iş 212) gelince
    /// aynı düzene giydirilir (tools/art/ui_layouts.json "yaratma").
    /// </summary>
    public sealed class CharacterCreator
    {
        private enum Tab { Identity, Personality, Voice, Body, Face, Hair, Marks, Clothes }
        private static readonly string[] TabLabels = { "Kimlik", "Kişilik", "Ses", "Beden", "Yüz", "Saç", "İz ve süs", "Kıyafet" };
        // Görünüm sekmeleri (zar ve kilit bunlarda anlamlı).
        private static readonly Tab[] LookTabs = { Tab.Voice, Tab.Body, Tab.Face, Tab.Hair, Tab.Marks, Tab.Clothes };

        private readonly AppearanceCatalog _catalog;
        private readonly System.Random _rng;
        private readonly CharacterDraft _draft;
        private Sprite _preview;
        private string _previewKey;
        private Vector2 _scroll;
        private List<string> _errors = new List<string>();
        private readonly HashSet<string> _customOpen = new HashSet<string>();
        private readonly HashSet<Tab> _locked = new HashSet<Tab>();
        private readonly Stack<Appearance> _undo = new Stack<Appearance>();
        private Tab _tab = Tab.Identity;
        private GUIStyle _titleStyle, _smallStyle, _centerStyle;

        // İz ve süs sekmesi için yeni işaret taslağı.
        private int _scarPattern, _scarZone, _tattooPattern, _tattooZone;
        private Rgb _ink;

        /// <summary>Özellik ve yemek listesi için içerik (GameRoot verir); null ise o bölümler gösterilmez.</summary>
        public Thatchwick.Core.Content.ContentDatabase Content { get; set; }
        /// <summary>Doğum günü seçimi için mevsim uzunluğu.</summary>
        public int DaysPerSeason { get; set; } = 10;
        private string _traitStatus = "";

        public bool Finished { get; private set; }
        public CharacterDraft Draft => _draft;

        private static readonly string[] SexLabels = { "Kadın", "Erkek" };
        private static readonly string[] StageLabels = { "Genç (30 gün)", "Yetişkin (40 gün)" };
        private static readonly string[] BuildLabels = { "İnce", "Orta", "Gürbüz", "Dolgun" };
        private static readonly string[] FrameLabels = { "Erkeksi", "Arada", "Kadınsı" };
        private static readonly string[] VoiceLabels = { "Kalın", "Sıcak", "Berrak", "İnce" };
        private static readonly string[] AccidentLabels = { "Kapalı", "Seyrek", "Normal" };
        private static readonly string[] FocusLabels = { "Yok", "Çiftçilik", "Odunculuk", "Yemek", "Marangozluk" };

        public CharacterCreator(AppearanceCatalog catalog, int seed = 1)
        {
            _catalog = catalog;
            _rng = new System.Random(seed);
            _draft = CharacterDraft.CreateRandom(catalog, _rng, "Aldric");
            _draft.SetSex(Sex.Male, catalog);
            _draft.Appearance = catalog.Randomize(_rng, Sex.Male);
            _ink = catalog.InkColors.Count > 0 ? catalog.InkColors[0] : new Rgb(40, 50, 90);
        }

        public void Release()
        {
            PersonSprites.Destroy(_preview);
            _preview = null;
        }

        public void OnGUI()
        {
            float s = Mathf.Max(1f, Screen.height / 540f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));
            float sw = Screen.width / s;
            float sh = Screen.height / s;

            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
                _smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true };
                _centerStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            }

            // Geri al: bu karede görünüm değişirse önceki hali yığına at.
            var before = _draft.Appearance.Clone();
            string keyBefore = FullKey(before);
            _undoing = false;

            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
            GUI.color = Color.white;

            float pw = Mathf.Min(sw - 16f, 820f);
            float ph = Mathf.Min(sh - 16f, 520f);
            var panel = new Rect((sw - pw) / 2f, (sh - ph) / 2f, pw, ph);
            GUI.Box(panel, GUIContent.none);

            GUILayout.BeginArea(new Rect(panel.x + 10f, panel.y + 8f, panel.width - 20f, panel.height - 16f));
            GUILayout.Label("Karakter yarat", _titleStyle);
            GUILayout.BeginHorizontal();
            DrawPreviewColumn();
            DrawOptionsColumn(ph);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            // Aynı durumu iki kez yığına atma; yığın en çok 40 adım tutar.
            if (!_undoing && FullKey(_draft.Appearance) != keyBefore && Event.current.type != EventType.Repaint)
            {
                if (_undo.Count == 0 || FullKey(_undo.Peek()) != keyBefore) _undo.Push(before);
                if (_undo.Count > 40) TrimUndo();
            }
        }

        private bool _undoing;

        private static string FullKey(Appearance a) => $"{a.Key()}|{a.Voice}|{a.VoicePitch}|{a.LaughId}";

        private void TrimUndo()
        {
            var keep = _undo.Take(30).Reverse().ToList();
            _undo.Clear();
            foreach (var a in keep) _undo.Push(a);
        }

        private void DrawPreviewColumn()
        {
            GUILayout.BeginVertical(GUILayout.Width(220f));
            var a = _draft.Appearance;

            string key = a.Key();
            if (_preview == null || key != _previewKey)
            {
                PersonSprites.Destroy(_preview);
                _preview = PersonSprites.Create(a);
                _previewKey = key;
            }

            float scale = 6f;
            var area = GUILayoutUtility.GetRect(210f, 300f);
            GUI.color = new Color(0.25f, 0.4f, 0.25f, 1f);
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = Color.white;
            float w = 24f * scale, h = 48f * scale;
            var r = new Rect(area.x + (area.width - w) / 2f, area.yMax - 10f - h, w, h);
            GUI.DrawTexture(r, _preview.texture, ScaleMode.StretchToFill, true);

            GUILayout.Space(4f);
            GUILayout.Label($"{_draft.Name} · {(_draft.Sex == Sex.Female ? "Kadın" : "Erkek")} · {(_draft.StartStage == AgeStage.Youth ? "Genç" : "Yetişkin")}", _smallStyle);
            GUILayout.Label(_draft.EffectiveDynastyName, _smallStyle);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Zar at (hepsi)")) RandomizeAll();
            GUI.enabled = _undo.Count > 0;
            if (GUILayout.Button("Geri al", GUILayout.Width(70f))) Undo();
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.Label(_locked.Count > 0 ? "Kilitli sekmeler zarda korunur." : "Sekmeyi kilitlersen zar onu değiştirmez.", _smallStyle);
            GUILayout.EndVertical();
        }

        private void DrawOptionsColumn(float panelHeight)
        {
            GUILayout.BeginVertical();

            // Sekmeler: iki sıra.
            int t = (int)_tab;
            t = GUILayout.SelectionGrid(t, TabLabels, 4);
            if (t != (int)_tab) { _tab = (Tab)t; _scroll = Vector2.zero; }

            if (LookTabs.Contains(_tab))
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Bu sekmeye zar at", GUILayout.Width(150f))) RandomizeTab(_tab);
                bool locked = _locked.Contains(_tab);
                bool nowLocked = GUILayout.Toggle(locked, " Kilitle");
                if (nowLocked != locked) { if (nowLocked) _locked.Add(_tab); else _locked.Remove(_tab); }
                GUILayout.EndHorizontal();
            }

            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(Mathf.Max(200f, panelHeight - 170f)));
            switch (_tab)
            {
                case Tab.Identity: DrawIdentity(); break;
                case Tab.Personality: DrawPersonality(); break;
                case Tab.Voice: DrawVoice(); break;
                case Tab.Body: DrawBody(); break;
                case Tab.Face: DrawFace(); break;
                case Tab.Hair: DrawHair(); break;
                case Tab.Marks: DrawMarks(); break;
                case Tab.Clothes: DrawClothes(); break;
            }
            GUILayout.EndScrollView();

            foreach (var e in _errors)
            {
                var prev = GUI.color;
                GUI.color = new Color(1f, 0.5f, 0.5f);
                GUILayout.Label("• " + e, _smallStyle);
                GUI.color = prev;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Başla", GUILayout.Height(30f)))
            {
                _errors = _draft.ValidateNewGame(_catalog);
                if (Content != null) _errors.AddRange(_draft.ValidateTraits(Content));
                if (_errors.Count == 0) Finished = true;
                else if (_draft.Accidents == AccidentSetting.NotSet) _tab = Tab.Identity;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        // ---- sekmeler ----

        private void DrawIdentity()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("İsim", GUILayout.Width(100f));
            _draft.Name = GUILayout.TextField(_draft.Name ?? "", CharacterDraft.MaxNameLength);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Hanedan", GUILayout.Width(100f));
            _draft.DynastyName = GUILayout.TextField(_draft.DynastyName ?? "", CharacterDraft.MaxDynastyLength);
            GUILayout.EndHorizontal();
            GUILayout.Label("   Boş bırakırsan: " + _draft.EffectiveDynastyName, _smallStyle);

            int sexIndex = _draft.Sex == Sex.Female ? 0 : 1;
            int newSex = Row("Cinsiyet", () => GUILayout.Toolbar(sexIndex, SexLabels));
            if (newSex != sexIndex) _draft.SetSex(newSex == 0 ? Sex.Female : Sex.Male, _catalog);
            GUILayout.Label("   Bütün saç, sakal ve kıyafetler herkese açık. Hamilelik yalnızca kadında.", _smallStyle);

            int stageIndex = _draft.StartStage == AgeStage.Youth ? 0 : 1;
            int newStage = Row("Başlangıç", () => GUILayout.Toolbar(stageIndex, StageLabels));
            _draft.StartStage = newStage == 0 ? AgeStage.Youth : AgeStage.Adult;

            _draft.BirthSeason = Row("Doğum mevsimi", () => GUILayout.Toolbar(Mathf.Clamp(_draft.BirthSeason, 0, 3), Likes.SeasonNames));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Doğum günü", GUILayout.Width(100f));
            if (GUILayout.Button("<", GUILayout.Width(28f))) _draft.BirthDayOfSeason = (_draft.BirthDayOfSeason - 1 + DaysPerSeason) % DaysPerSeason;
            GUILayout.Label($"{_draft.BirthDayOfSeason + 1}. gün", _centerStyle, GUILayout.Width(80f));
            if (GUILayout.Button(">", GUILayout.Width(28f))) _draft.BirthDayOfSeason = (_draft.BirthDayOfSeason + 1) % DaysPerSeason;
            GUILayout.EndHorizontal();

            Section("Kazalar");
            int acc = _draft.Accidents == AccidentSetting.NotSet ? -1 : (int)_draft.Accidents - 1;
            int newAcc = Row("Kaza ayarı", () => GUILayout.Toolbar(acc, AccidentLabels));
            if (newAcc >= 0) _draft.Accidents = (AccidentSetting)(newAcc + 1);
            string accText;
            switch (_draft.Accidents)
            {
                case AccidentSetting.Off: accText = "Kaza ve yara izi olmaz."; break;
                case AccidentSetting.Rare: accText = "Ara sıra küçük kazalar. Usta olan ve daha önce yaralanan daha az kaza geçirir; iz kalması gittikçe azalır."; break;
                case AccidentSetting.Normal: accText = "Riskli işlerde kaza olur; iyileşen yara iz bırakabilir. Usta ve hekim riski azaltır."; break;
                default: accText = "Seç: yeni oyunda zorunlu. Ayarlar menüsünden sonra değiştirilebilir."; break;
            }
            GUILayout.Label("   " + accText, _smallStyle);
        }

        private void DrawPersonality()
        {
            int focusIndex = _draft.SkillFocus.HasValue ? 1 + (int)_draft.SkillFocus.Value : 0;
            int newFocus = Row("Beceri eğilimi", () => GUILayout.Toolbar(Mathf.Min(focusIndex, FocusLabels.Length - 1), FocusLabels));
            _draft.SkillFocus = newFocus == 0 ? (SkillType?)null : (SkillType)(newFocus - 1);
            GUILayout.Label("   Eğilim seçilen beceriyle seviye 2 başlatır.", _smallStyle);

            if (Content != null && Content.Traits.Count > 0)
            {
                Section($"Özellikler ({_draft.TraitIds.Count}/{Character.MaxTraits})");
                foreach (var t in Content.Traits.All)
                {
                    bool on = _draft.TraitIds.Contains(t.Id);
                    GUILayout.BeginHorizontal();
                    bool now = GUILayout.Toggle(on, t.DisplayName, GUILayout.Width(150f));
                    GUILayout.Label(t.Description ?? "", _smallStyle);
                    GUILayout.EndHorizontal();
                    if (now != on)
                        _traitStatus = _draft.ToggleTrait(Content, t.Id, out var why) ? "" : why;
                }
                GUILayout.Label(_traitStatus.Length > 0 ? _traitStatus : "   Hiç seçmezsen rastgele verilir.", _smallStyle);
            }

            Section("Sevdikleri");
            var foods = FoodNames();
            if (foods.Count > 0)
            {
                _draft.LikedFood = TextCycle("Sevdiği yemek", foods, _draft.LikedFood, true);
                _draft.DislikedFood = TextCycle("Sevmediği", foods, _draft.DislikedFood, true);
            }
            _draft.LikedColor = TextCycle("Sevdiği renk", Likes.Colors.ToList(), _draft.LikedColor, true);
            _draft.LikedHobby = TextCycle("Uğraşı", Likes.Hobbies.ToList(), _draft.LikedHobby, true);
            GUILayout.Label("   Sevdiği yemek ve uğraş keyif verir; sevmediği yemek keyfini kaçırır.", _smallStyle);
        }

        private void DrawVoice()
        {
            var a = _draft.Appearance;
            a.Voice = (VoiceTimbre)Row("Tını", () => GUILayout.Toolbar((int)a.Voice, VoiceLabels));
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Perde {(a.VoicePitch > 0 ? "+" : "")}{a.VoicePitch}", GUILayout.Width(100f));
            a.VoicePitch = Mathf.RoundToInt(GUILayout.HorizontalSlider(a.VoicePitch, Appearance.MinPitch, Appearance.MaxPitch, GUILayout.Width(200f)));
            GUILayout.EndHorizontal();
            a.LaughId = OptionRow("Gülüş", _catalog.Laughs, a.LaughId);
            GUILayout.Label("   Sesler daha sonra eklenecek; seçimin kaydedilir ve geldiğinde kullanılır.", _smallStyle);
        }

        private void DrawBody()
        {
            var a = _draft.Appearance;
            a.Build = (BodyBuild)Row("Beden", () => GUILayout.Toolbar((int)a.Build, BuildLabels));
            a.Frame = (BodyFrame)Row("Çerçeve", () => GUILayout.Toolbar((int)a.Frame, FrameLabels));
            GUILayout.Label("   Beden yapısı yaşamla değişir (yemek ve iş); Ayna'da seçilmez.", _smallStyle);
            a.SkinColor = ColorRow("skin", "Ten", a.SkinColor, _catalog.SkinTones, false);
        }

        private void DrawFace()
        {
            var a = _draft.Appearance;
            a.FaceShapeId = OptionRow("Yüz şekli", _catalog.FaceShapes, a.FaceShapeId);
            a.EyeShapeId = OptionRow("Göz şekli", _catalog.EyeShapes, a.EyeShapeId);
            a.EyeColor = ColorRow("eye", "Göz rengi", a.EyeColor, _catalog.EyeColors, true);
            bool hetero = a.EyeColorRight.HasValue;
            bool nowHetero = GUILayout.Toggle(hetero, " İki göz farklı renk");
            if (nowHetero && !hetero) a.EyeColorRight = _catalog.EyeColors.FirstOrDefault(e => e != a.EyeColor);
            if (!nowHetero) a.EyeColorRight = null;
            if (a.EyeColorRight.HasValue)
                a.EyeColorRight = ColorRow("eyeR", "Sağ göz", a.EyeColorRight.Value, _catalog.EyeColors, true);
            a.BrowId = OptionRow("Kaş", _catalog.Brows, a.BrowId);
            a.BrowFollowsHair = GUILayout.Toggle(a.BrowFollowsHair, " Kaş rengi saçla aynı");
            if (!a.BrowFollowsHair) a.BrowColor = ColorRow("brow", "Kaş rengi", a.BrowColor, _catalog.HairColors, true);
            a.NoseId = OptionRow("Burun", _catalog.Noses, a.NoseId);
            a.MouthId = OptionRow("Ağız", _catalog.Mouths, a.MouthId);
            a.EarsId = OptionRow("Kulak", _catalog.Ears, a.EarsId);
            GUILayout.Label("   Ten, göz, yüz, burun ve kulak çocuklara geçer; sonradan Ayna'da değişmez.", _smallStyle);
        }

        private void DrawHair()
        {
            var a = _draft.Appearance;
            a.HairStyleId = OptionRow("Saç", _catalog.HairStyles, a.HairStyleId);
            a.BangsId = OptionRow("Perçem", _catalog.Bangs, a.BangsId);
            a.HairColor = ColorRow("hair", "Saç rengi", a.HairColor, _catalog.HairColors, true);
            a.MustacheId = OptionRow("Bıyık", _catalog.Mustaches, a.MustacheId);
            a.BeardId = OptionRow("Sakal", _catalog.Beards, a.BeardId);
            if (a.MustacheId != Appearance.None || a.BeardId != Appearance.None)
            {
                bool same = a.FacialHairColor == a.HairColor;
                bool nowSame = GUILayout.Toggle(same, " Sakal rengi saçla aynı");
                if (nowSame) a.FacialHairColor = a.HairColor;
                else
                {
                    if (same) a.FacialHairColor = _catalog.HairColors.FirstOrDefault(h => h != a.HairColor);
                    a.FacialHairColor = ColorRow("beard", "Sakal rengi", a.FacialHairColor, _catalog.HairColors, true);
                }
            }
            GUILayout.Label("   Özel renk boya sayılır, çocuklara geçmez.", _smallStyle);
        }

        private void DrawMarks()
        {
            var a = _draft.Appearance;

            Section($"Ten detayları ({MarkRules.Count(a.Marks, MarkKind.SkinDetail)}/{MarkRules.MaxSkinDetails})");
            ToggleMarks(MarkKind.SkinDetail, _catalog.SkinDetails, MarkRules.MaxSkinDetails, o => FirstTag(o.Tag, "face"));

            int chosenScars = a.Marks.Count(m => m.Kind == MarkKind.Scar && m.Source == MarkSource.Chosen);
            Section($"Yara izleri ({chosenScars}/{MarkRules.MaxChosenScars})");
            ListMarks(MarkKind.Scar, _catalog.ScarPatterns, _catalog.ScarZones);
            if (chosenScars < MarkRules.MaxChosenScars && _catalog.ScarPatterns.Count > 0 && _catalog.ScarZones.Count > 0)
            {
                GUILayout.BeginHorizontal();
                _scarPattern = Cycle(_catalog.ScarPatterns, _scarPattern, 100f);
                _scarZone = Cycle(_catalog.ScarZones, _scarZone, 100f);
                if (GUILayout.Button("Ekle", GUILayout.Width(50f)))
                {
                    string zone = _catalog.ScarZones[_scarZone].Id;
                    a.Marks.RemoveAll(m => m.Kind == MarkKind.Scar && m.Source == MarkSource.Chosen && m.Zone == zone);
                    a.Marks.Add(new Mark { Kind = MarkKind.Scar, Id = _catalog.ScarPatterns[_scarPattern].Id, Zone = zone, Fade = 2, Source = MarkSource.Chosen });
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Label("   Oyunda kazadan da iz kalabilir; çok yaralanan karakterin izi giderek azalır.", _smallStyle);

            Section("Dövmeler (bölge başına bir)");
            ListMarks(MarkKind.Tattoo, _catalog.TattooPatterns, _catalog.TattooZones);
            if (_catalog.TattooPatterns.Count > 0 && _catalog.TattooZones.Count > 0)
            {
                GUILayout.BeginHorizontal();
                _tattooPattern = Cycle(_catalog.TattooPatterns, _tattooPattern, 100f);
                _tattooZone = Cycle(_catalog.TattooZones, _tattooZone, 100f);
                if (GUILayout.Button("Ekle", GUILayout.Width(50f)))
                {
                    string zone = _catalog.TattooZones[_tattooZone].Id;
                    a.Marks.RemoveAll(m => m.Kind == MarkKind.Tattoo && m.Zone == zone);
                    a.Marks.Add(new Mark { Kind = MarkKind.Tattoo, Id = _catalog.TattooPatterns[_tattooPattern].Id, Zone = zone, Color = _ink });
                }
                GUILayout.EndHorizontal();
                if (_catalog.InkColors.Count > 0) _ink = ColorRow("ink", "Mürekkep", _ink, _catalog.InkColors, false);
            }

            Section($"Takılar ({MarkRules.Count(a.Marks, MarkKind.Jewelry)}/{MarkRules.MaxJewelry})");
            ToggleMarks(MarkKind.Jewelry, _catalog.Jewelry, MarkRules.MaxJewelry, o => FirstTag(o.Tag, "ear"));

            Section($"Makyaj ({MarkRules.Count(a.Marks, MarkKind.Makeup)}/{MarkRules.MaxMakeup})");
            ToggleMarks(MarkKind.Makeup, _catalog.Makeup, MarkRules.MaxMakeup, _ => "face");
            foreach (var m in a.Marks.Where(x => x.Kind == MarkKind.Makeup).ToList())
                if (_catalog.MakeupColors.Count > 0) m.Color = ColorRow("mk_" + m.Id, Label(_catalog.Makeup, m.Id), m.Color, _catalog.MakeupColors, false);
        }

        private void DrawClothes()
        {
            var a = _draft.Appearance;
            a.OutfitId = OptionRow("Üst", _catalog.Outfits, a.OutfitId);
            a.PatternId = OptionRow("Desen", _catalog.Patterns, a.PatternId);
            a.OutfitColor = ColorRow("outfit", "Ana renk", a.OutfitColor, _catalog.ClothColors, true);
            a.TrimColor = ColorRow("trim", "Kenar", a.TrimColor, _catalog.ClothColors, true);
            a.BottomId = OptionRow("Alt", _catalog.Bottoms, a.BottomId);
            a.ShoesId = OptionRow("Ayakkabı", _catalog.Shoes, a.ShoesId);
            a.OuterId = OptionRow("Dış giysi", _catalog.Outers, a.OuterId);
            if (a.OuterId != Appearance.None) a.OuterColor = ColorRow("outer", "Dış renk", a.OuterColor, _catalog.ClothColors, true);
            a.HatId = OptionRow("Başlık", _catalog.Hats, a.HatId);
            if (a.HatId != Appearance.None) a.HatColor = ColorRow("hat", "Başlık rengi", a.HatColor, _catalog.ClothColors, true);
        }

        // ---- zar ve geri al ----

        private void RandomizeAll()
        {
            foreach (var t in LookTabs) if (!_locked.Contains(t)) RandomizeTab(t);
            _errors = new List<string>();
        }

        private void RandomizeTab(Tab tab)
        {
            var r = _catalog.Randomize(_rng, _draft.Sex);
            var a = _draft.Appearance;
            switch (tab)
            {
                case Tab.Voice: a.Voice = r.Voice; a.VoicePitch = r.VoicePitch; a.LaughId = r.LaughId; break;
                case Tab.Body: a.Build = r.Build; a.Frame = r.Frame; a.SkinColor = r.SkinColor; break;
                case Tab.Face:
                    a.FaceShapeId = r.FaceShapeId; a.EyeShapeId = r.EyeShapeId; a.EyeColor = r.EyeColor; a.EyeColorRight = r.EyeColorRight;
                    a.BrowId = r.BrowId; a.BrowFollowsHair = r.BrowFollowsHair; a.BrowColor = r.BrowColor;
                    a.NoseId = r.NoseId; a.MouthId = r.MouthId; a.EarsId = r.EarsId;
                    break;
                case Tab.Hair:
                    a.HairStyleId = r.HairStyleId; a.BangsId = r.BangsId; a.HairColor = r.HairColor;
                    a.MustacheId = r.MustacheId; a.BeardId = r.BeardId; a.FacialHairColor = r.FacialHairColor;
                    break;
                case Tab.Marks:
                    a.Marks.Clear();
                    foreach (var m in r.Marks) a.Marks.Add(m.Clone());
                    break;
                case Tab.Clothes:
                    a.OutfitId = r.OutfitId; a.PatternId = r.PatternId; a.OutfitColor = r.OutfitColor; a.TrimColor = r.TrimColor;
                    a.BottomId = r.BottomId; a.ShoesId = r.ShoesId; a.OuterId = r.OuterId; a.OuterColor = r.OuterColor;
                    a.HatId = r.HatId; a.HatColor = r.HatColor;
                    break;
            }
        }

        private void Undo()
        {
            if (_undo.Count == 0) return;
            _draft.Appearance = _undo.Pop();
            _undoing = true;
        }

        // ---- yardımcılar ----

        private List<string> FoodNames()
        {
            if (Content == null) return new List<string>();
            return Content.Items.All.Where(i => i.HasTag("food") || i.HasTag("meal")).Select(i => i.Id).Distinct().ToList();
        }

        private string FoodLabel(string id) =>
            Content != null && Content.Items.TryGet(id, out var def) ? def.DisplayName : id;

        private void Section(string title)
        {
            GUILayout.Space(6f);
            GUILayout.Label(title, _titleStyle);
        }

        private int Row(string label, Func<int> draw)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(100f));
            int v = draw();
            GUILayout.EndHorizontal();
            return v;
        }

        private static string FirstTag(string tag, string fallback)
        {
            if (string.IsNullOrEmpty(tag)) return fallback;
            int comma = tag.IndexOf(',');
            return comma < 0 ? tag : tag.Substring(0, comma);
        }

        private static string Label(List<OptionDef> list, string id) => list.FirstOrDefault(o => o.Id == id)?.DisplayName ?? id;

        private int Cycle(List<OptionDef> list, int index, float width)
        {
            index = Mathf.Clamp(index, 0, list.Count - 1);
            if (GUILayout.Button("<", GUILayout.Width(22f))) index = (index - 1 + list.Count) % list.Count;
            GUILayout.Label(list[index].DisplayName, _centerStyle, GUILayout.Width(width));
            if (GUILayout.Button(">", GUILayout.Width(22f))) index = (index + 1) % list.Count;
            return index;
        }

        private string TextCycle(string label, List<string> values, string current, bool allowNone)
        {
            var list = new List<string>();
            if (allowNone) list.Add("");
            list.AddRange(values);
            int index = Math.Max(0, list.IndexOf(current ?? ""));
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(100f));
            if (GUILayout.Button("<", GUILayout.Width(28f))) index = (index - 1 + list.Count) % list.Count;
            string shown = list[index].Length == 0 ? "—" : FoodLabelOrSelf(list[index]);
            GUILayout.Label(shown, _centerStyle, GUILayout.Width(130f));
            if (GUILayout.Button(">", GUILayout.Width(28f))) index = (index + 1) % list.Count;
            GUILayout.EndHorizontal();
            return list[index];
        }

        private string FoodLabelOrSelf(string id) => Content != null && Content.Items.Contains(id) ? FoodLabel(id) : id;

        private void ToggleMarks(MarkKind kind, List<OptionDef> options, int max, Func<OptionDef, string> zoneFor)
        {
            var a = _draft.Appearance;
            int count = MarkRules.Count(a.Marks, kind);
            int perRow = 3, i = 0;
            GUILayout.BeginHorizontal();
            foreach (var o in options)
            {
                if (o.Id == Appearance.None) continue;
                if (i > 0 && i % perRow == 0) { GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); }
                i++;
                bool on = a.Marks.Any(m => m.Kind == kind && m.Id == o.Id);
                GUI.enabled = on || count < max;
                bool now = GUILayout.Toggle(on, " " + o.DisplayName, GUILayout.Width(150f));
                GUI.enabled = true;
                if (now == on) continue;
                if (now) a.Marks.Add(new Mark { Kind = kind, Id = o.Id, Zone = zoneFor(o), Color = DefaultColor(kind) });
                else a.Marks.RemoveAll(m => m.Kind == kind && m.Id == o.Id);
                count = MarkRules.Count(a.Marks, kind);
            }
            GUILayout.EndHorizontal();
        }

        private Rgb DefaultColor(MarkKind kind) =>
            kind == MarkKind.Makeup && _catalog.MakeupColors.Count > 0 ? _catalog.MakeupColors[0] : new Rgb(0, 0, 0);

        private void ListMarks(MarkKind kind, List<OptionDef> patterns, List<OptionDef> zones)
        {
            var a = _draft.Appearance;
            foreach (var m in a.Marks.Where(x => x.Kind == kind && x.Source == MarkSource.Chosen).ToList())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"   {Label(patterns, m.Id)} · {Label(zones, m.Zone)}", GUILayout.Width(220f));
                if (kind == MarkKind.Scar)
                {
                    string[] fades = { "Taze", "Solmuş", "Eski" };
                    m.Fade = GUILayout.Toolbar(m.Fade, fades, GUILayout.Width(150f));
                }
                if (GUILayout.Button("Kaldır", GUILayout.Width(60f))) a.Marks.Remove(m);
                GUILayout.EndHorizontal();
            }
        }

        private string OptionRow(string label, List<OptionDef> list, string currentId)
        {
            if (list.Count == 0) return currentId;
            int index = list.FindIndex(o => o.Id == currentId);
            if (index < 0) index = 0;

            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(100f));
            if (GUILayout.Button("<", GUILayout.Width(28f))) index = (index - 1 + list.Count) % list.Count;
            GUILayout.Label(list[index].DisplayName, _centerStyle, GUILayout.Width(130f));
            if (GUILayout.Button(">", GUILayout.Width(28f))) index = (index + 1) % list.Count;
            GUILayout.Label($"{index + 1}/{list.Count}", _smallStyle);
            GUILayout.EndHorizontal();
            return list[index].Id;
        }

        private static Color ToColor(Rgb c) => new Color32(c.R, c.G, c.B, 255);

        private Rgb ColorRow(string id, string label, Rgb current, List<Rgb> palette, bool allowCustom)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(100f));

            int perRow = 16;
            GUILayout.BeginVertical();
            for (int start = 0; start < palette.Count; start += perRow)
            {
                GUILayout.BeginHorizontal();
                for (int i = start; i < Math.Min(start + perRow, palette.Count); i++)
                {
                    var prev = GUI.backgroundColor;
                    GUI.backgroundColor = ToColor(palette[i]);
                    bool selected = palette[i] == current;
                    if (GUILayout.Button(selected ? "*" : "", GUILayout.Width(20f), GUILayout.Height(18f)))
                        current = palette[i];
                    GUI.backgroundColor = prev;
                }
                GUILayout.EndHorizontal();
            }

            if (allowCustom)
            {
                bool open = _customOpen.Contains(id);
                bool newOpen = GUILayout.Toggle(open, $" Özel renk ({current.ToHex()})");
                if (newOpen != open)
                {
                    if (newOpen) _customOpen.Add(id); else _customOpen.Remove(id);
                }
                if (newOpen)
                {
                    float r = Slider("R", current.R);
                    float g = Slider("G", current.G);
                    float b = Slider("B", current.B);
                    current = new Rgb((byte)Mathf.RoundToInt(r), (byte)Mathf.RoundToInt(g), (byte)Mathf.RoundToInt(b));
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            return current;
        }

        private static float Slider(string label, float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(16f));
            value = GUILayout.HorizontalSlider(value, 0f, 255f, GUILayout.Width(200f));
            GUILayout.EndHorizontal();
            return value;
        }
    }
}
