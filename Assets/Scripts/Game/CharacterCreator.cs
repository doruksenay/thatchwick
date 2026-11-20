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
        private static string L(string key) => GameLocalization.T(key);
        private enum Tab { Identity, Personality, Voice, Body, Face, Hair, Marks, Clothes }
        private static string[] TabLabels => new[] { L("ui.creator.tab.identity"), L("ui.creator.tab.personality"), L("ui.creator.tab.voice"), L("ui.creator.tab.body"), L("ui.creator.tab.face"), L("ui.creator.tab.hair"), L("ui.creator.tab.marks"), L("ui.creator.tab.clothes") };
        // Görünüm sekmeleri (zar ve kilit bunlarda anlamlı).
        private static readonly Tab[] LookTabs = { Tab.Voice, Tab.Body, Tab.Face, Tab.Hair, Tab.Marks, Tab.Clothes };

        private readonly AppearanceCatalog _catalog;
        private readonly System.Random _rng;
        private readonly CharacterDraft _draft;
        private Sprite _preview;
        private bool _previewLayered;   // katman karesi LayeredCharacterArt önbelleğine aittir, burada silinmez
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

        private static string[] SexLabels => new[] { L("ui.creator.sex.female"), L("ui.creator.sex.male") };
        private static string[] StageLabels => new[] { L("ui.creator.stage.young"), L("ui.creator.stage.adult") };
        private static string[] BuildLabels => new[] { L("ui.creator.build.slim"), L("ui.creator.build.medium"), L("ui.creator.build.sturdy"), L("ui.creator.build.plump") };
        private static string[] FrameLabels => new[] { L("ui.creator.frame.masc"), L("ui.creator.frame.mid"), L("ui.creator.frame.fem") };
        private static string[] VoiceLabels => new[] { L("ui.creator.voice.deep"), L("ui.creator.voice.warm"), L("ui.creator.voice.clear"), L("ui.creator.voice.thin") };
        private static string[] AccidentLabels => new[] { L("ui.creator.acc.off"), L("ui.creator.acc.rare"), L("ui.creator.acc.normal") };
        private static string[] FocusLabels => new[] { L("ui.creator.focus.none"), L("ui.creator.focus.farming"), L("ui.creator.focus.woodcutting"), L("ui.creator.focus.cooking"), L("ui.creator.focus.carpentry") };

        // ---- Ayna kipi (belge 01 ve 14): var olan karakterin görünüşünü ve adını değiştirir ----
        private readonly Character _mirrorTarget;
        private readonly Appearance _mirrorOriginal;
        private static readonly Tab[] MirrorTabs = { Tab.Identity, Tab.Voice, Tab.Face, Tab.Hair, Tab.Marks, Tab.Clothes };
        public bool IsMirror => _mirrorTarget != null;
        public Character MirrorTarget => _mirrorTarget;
        /// <summary>Ayna kipinde "Vazgeç" seçildi.</summary>
        public bool Cancelled { get; private set; }
        /// <summary>Elmas bakiyesi (Ayna kipi).</summary>
        public Func<int> DiamondBalance { get; set; }
        /// <summary>Evde ayna var mı (Ayna kipi; karar 18.11.2026).</summary>
        public Func<bool> HasHouseMirror { get; set; }

        /// <summary>Ayna: verilen karakterin görünüşü taslağa kopyalanır; genetik alanlar ve beden yapısı değişmez.</summary>
        public CharacterCreator(AppearanceCatalog catalog, Character target, int seed = 1)
        {
            _catalog = catalog;
            _rng = new System.Random(seed);
            _mirrorTarget = target;
            _mirrorOriginal = target.Appearance.Clone();
            _draft = CharacterDraft.CreateRandom(catalog, _rng, target.Name);
            _draft.SetSex(target.Sex, catalog);
            _draft.StartStage = target.Stage;
            _draft.Appearance = target.Appearance.Clone();
            _ink = catalog.InkColors.Count > 0 ? catalog.InkColors[0] : new Rgb(40, 50, 90);
            _tab = Tab.Hair;
        }

        /// <summary>Ayna'nın değiştiremediği alanlar her karede aslına döner (zar ve sekmeler bunlara dokunsa da).</summary>
        private void KeepMirrorLocks()
        {
            var a = _draft.Appearance; var o = _mirrorOriginal;
            a.SkinColor = o.SkinColor; a.EyeColor = o.EyeColor; a.EyeColorRight = o.EyeColorRight;
            a.FaceShapeId = o.FaceShapeId; a.EyeShapeId = o.EyeShapeId; a.NoseId = o.NoseId; a.EarsId = o.EarsId;
            a.Build = o.Build; a.Frame = o.Frame; a.HeightPercent = o.HeightPercent;
        }

        private MirrorQuote MirrorQuoteNow() =>
            MirrorRules.Quote(_mirrorTarget, _draft.Appearance, _draft.Name, _catalog, HasHouseMirror == null || HasHouseMirror());

        public CharacterCreator(AppearanceCatalog catalog, int seed = 1)
        {
            _catalog = catalog;
            _rng = new System.Random(seed);
            _draft = CharacterDraft.CreateRandom(catalog, _rng, "");
            _draft.SetSex(Sex.Male, catalog);
            _draft.Appearance = catalog.Randomize(_rng, Sex.Male);
            _ink = catalog.InkColors.Count > 0 ? catalog.InkColors[0] : new Rgb(40, 50, 90);
        }

        /// <summary>Varsayılan ad genel ad havuzundan gelir (belge 61b §5); oyuncu değiştirebilir.</summary>
        public void SetDefaultName(string name) { if (!string.IsNullOrWhiteSpace(name)) _draft.Name = name; }

        public void Release()
        {
            if (!_previewLayered) PersonSprites.Destroy(_preview);
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
                _titleStyle = new GUIStyle(UiSkin.InkLabel(GUI.skin.label)) { fontSize = 16, fontStyle = FontStyle.Bold };
                _smallStyle = new GUIStyle(UiSkin.InkLabel(GUI.skin.label)) { fontSize = 11, wordWrap = true };
                _centerStyle = new GUIStyle(UiSkin.InkLabel(GUI.skin.label)) { alignment = TextAnchor.MiddleCenter };
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
            GUILayout.Label(IsMirror ? GameLocalization.T("ui.mirror.title", ("name", _mirrorTarget.Name)) : L("ui.creator.title"), _titleStyle);
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
                if (!_previewLayered) PersonSprites.Destroy(_preview);
                // Katman görselleri (belge 43) varsa önizleme oyundaki gerçek karakterdir; yoksa prosedürel yer tutucu.
                _preview = LayeredCharacterArt.Frame(a, _draft.Sex, _draft.StartStage, "idle", "se", 0);
                _previewLayered = _preview != null;
                if (!_previewLayered) _preview = PersonSprites.Create(a);
                _previewKey = key;
            }

            float scale = 6f;
            var area = GUILayoutUtility.GetRect(210f, 300f);
            GUI.color = new Color(0.25f, 0.4f, 0.25f, 1f);
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = Color.white;
            float w = 24f * scale, h = 48f * scale;
            if (_previewLayered)
            {
                // Katman karesi gerçek ölçüde (84 px'lik tuval): tam sayı büyütmeyle alana sığdır.
                float k = Mathf.Max(1f, Mathf.Floor(Mathf.Min(area.width / _preview.texture.width, (area.height - 10f) / _preview.texture.height)));
                w = _preview.texture.width * k; h = _preview.texture.height * k;
            }
            var r = new Rect(area.x + (area.width - w) / 2f, area.yMax - 10f - h, w, h);
            GUI.DrawTexture(r, _preview.texture, ScaleMode.StretchToFill, true);

            GUILayout.Space(4f);
            GUILayout.Label($"{_draft.Name} · {(_draft.Sex == Sex.Female ? L("ui.creator.sex.female") : L("ui.creator.sex.male"))} · {AgeStages.Name(_draft.StartStage)}", _smallStyle);
            if (!IsMirror) GUILayout.Label(_draft.EffectiveDynastyName, _smallStyle);

            GUILayout.BeginHorizontal();
            if (!IsMirror && GUILayout.Button(L("ui.creator.roll_all"))) RandomizeAll();
            GUI.enabled = _undo.Count > 0;
            if (GUILayout.Button(L("ui.creator.undo"), GUILayout.Width(70f))) Undo();
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.Label(_locked.Count > 0 ? L("ui.creator.locked_note") : L("ui.creator.lock_hint"), _smallStyle);
            GUILayout.EndVertical();
        }

        private void DrawOptionsColumn(float panelHeight)
        {
            GUILayout.BeginVertical();

            // Sekmeler: iki sıra.
            if (IsMirror)
            {
                int mi = Mathf.Max(0, System.Array.IndexOf(MirrorTabs, _tab));
                int mn = GUILayout.SelectionGrid(mi, MirrorTabs.Select(x => TabLabels[(int)x]).ToArray(), 3);
                if (mn != mi || _tab != MirrorTabs[mn]) { _tab = MirrorTabs[mn]; _scroll = Vector2.zero; }
            }
            else
            {
                int t = (int)_tab;
                t = GUILayout.SelectionGrid(t, TabLabels, 4);
                if (t != (int)_tab) { _tab = (Tab)t; _scroll = Vector2.zero; }
            }

            if (LookTabs.Contains(_tab))
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(L("ui.creator.roll_tab"), GUILayout.Width(150f))) RandomizeTab(_tab);
                bool locked = _locked.Contains(_tab);
                bool nowLocked = GUILayout.Toggle(locked, " " + L("ui.creator.lock"));
                if (nowLocked != locked) { if (nowLocked) _locked.Add(_tab); else _locked.Remove(_tab); }
                GUILayout.EndHorizontal();
            }

            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(Mathf.Max(200f, panelHeight - 170f)));
            switch (_tab)
            {
                case Tab.Identity: if (IsMirror) DrawMirrorIdentity(); else DrawIdentity(); break;
                case Tab.Personality: DrawPersonality(); break;
                case Tab.Voice: DrawVoice(); break;
                case Tab.Body: DrawBody(); break;
                case Tab.Face: DrawFace(); break;
                case Tab.Hair: DrawHair(); break;
                case Tab.Marks: DrawMarks(); break;
                case Tab.Clothes: DrawClothes(); break;
            }
            GUILayout.EndScrollView();
            if (IsMirror) { KeepMirrorLocks(); DrawMirrorFooter(); GUILayout.EndVertical(); return; }

            foreach (var e in _errors)
            {
                var prev = GUI.color;
                GUI.color = new Color(1f, 0.5f, 0.5f);
                GUILayout.Label("• " + e, _smallStyle);
                GUI.color = prev;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(L("ui.creator.start"), GUILayout.Height(30f)))
            {
                _errors = _draft.ValidateNewGame(_catalog);
                if (Content != null) _errors.AddRange(_draft.ValidateTraits(Content));
                if (_errors.Count == 0) Finished = true;
                else if (_draft.Accidents == AccidentSetting.NotSet) _tab = Tab.Identity;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        // ---- Ayna kipi ----

        private void DrawMirrorIdentity()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(L("ui.creator.name"), GUILayout.Width(100f));
            _draft.Name = GUILayout.TextField(_draft.Name ?? "", CharacterDraft.MaxNameLength);
            GUILayout.EndHorizontal();
            GUILayout.Label("   " + GameLocalization.T("ui.mirror.rename_note", ("cost", MirrorRules.RenameDiamonds)), _smallStyle);
            GUILayout.Label("   " + GameLocalization.T("ui.mirror.locked_note"), _smallStyle);
        }

        private void DrawMirrorFooter()
        {
            var q = MirrorQuoteNow();
            int balance = DiamondBalance != null ? DiamondBalance() : 0;
            bool nameOk = (_draft.Name ?? "").Trim().Length >= CharacterDraft.MinNameLength;
            bool changed = q.Diamonds > 0 || q.UsesFreeMirror;
            var prev = GUI.color;
            GUI.color = new Color(1f, 0.5f, 0.5f);
            foreach (var e in q.Errors) GUILayout.Label("• " + e, _smallStyle);
            if (!nameOk) GUILayout.Label("• " + GameLocalization.T("ui.mirror.name_short"), _smallStyle);
            if (q.CanApply && q.Diamonds > balance) GUILayout.Label("• " + GameLocalization.T("ui.mirror.not_enough"), _smallStyle);
            GUI.color = prev;

            string price = !changed ? GameLocalization.T("ui.mirror.no_change")
                : q.Diamonds > 0 ? GameLocalization.T("ui.mirror.cost", ("cost", q.Diamonds), ("balance", balance))
                : GameLocalization.T("ui.mirror.free");
            GUILayout.Label(price);

            GUILayout.BeginHorizontal();
            GUI.enabled = changed && q.CanApply && nameOk && q.Diamonds <= balance;
            if (GUILayout.Button(GameLocalization.T("ui.mirror.apply"), GUILayout.Height(30f))) Finished = true;
            GUI.enabled = true;
            if (GUILayout.Button(GameLocalization.T("ui.mirror.cancel"), GUILayout.Height(30f), GUILayout.Width(110f))) Cancelled = true;
            GUILayout.EndHorizontal();
        }

        // ---- sekmeler ----

        private void DrawIdentity()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(L("ui.creator.name"), GUILayout.Width(100f));
            _draft.Name = GUILayout.TextField(_draft.Name ?? "", CharacterDraft.MaxNameLength);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(L("ui.creator.dynasty"), GUILayout.Width(100f));
            _draft.DynastyName = GUILayout.TextField(_draft.DynastyName ?? "", CharacterDraft.MaxDynastyLength);
            GUILayout.EndHorizontal();
            GUILayout.Label("   " + GameLocalization.T("ui.creator.dynasty_blank", ("name", _draft.EffectiveDynastyName)), _smallStyle);

            int sexIndex = _draft.Sex == Sex.Female ? 0 : 1;
            int newSex = Row(L("ui.creator.sex"), () => GUILayout.Toolbar(sexIndex, SexLabels));
            if (newSex != sexIndex) _draft.SetSex(newSex == 0 ? Sex.Female : Sex.Male, _catalog);
            GUILayout.Label("   " + L("ui.creator.gender_note"), _smallStyle);

            int stageIndex = _draft.StartStage == AgeStage.YoungAdult ? 0 : 1;
            int newStage = Row(L("ui.creator.start_stage"), () => GUILayout.Toolbar(stageIndex, StageLabels));
            _draft.StartStage = newStage == 0 ? AgeStage.YoungAdult : AgeStage.Adult;

            _draft.BirthSeason = Row(L("ui.creator.birth_season"), () => GUILayout.Toolbar(Mathf.Clamp(_draft.BirthSeason, 0, 3), Likes.SeasonNames));
            GUILayout.BeginHorizontal();
            GUILayout.Label(L("ui.creator.birth_day"), GUILayout.Width(100f));
            if (GUILayout.Button("<", GUILayout.Width(28f))) _draft.BirthDayOfSeason = (_draft.BirthDayOfSeason - 1 + DaysPerSeason) % DaysPerSeason;
            GUILayout.Label(GameLocalization.T("ui.creator.day_n", ("n", _draft.BirthDayOfSeason + 1)), _centerStyle, GUILayout.Width(80f));
            if (GUILayout.Button(">", GUILayout.Width(28f))) _draft.BirthDayOfSeason = (_draft.BirthDayOfSeason + 1) % DaysPerSeason;
            GUILayout.EndHorizontal();

            Section(L("ui.creator.accidents_sec"));
            int acc = _draft.Accidents == AccidentSetting.NotSet ? -1 : (int)_draft.Accidents - 1;
            int newAcc = Row(L("ui.creator.accident"), () => GUILayout.Toolbar(acc, AccidentLabels));
            if (newAcc >= 0) _draft.Accidents = (AccidentSetting)(newAcc + 1);
            string accText;
            switch (_draft.Accidents)
            {
                case AccidentSetting.Off: accText = L("ui.creator.acc_off_text"); break;
                case AccidentSetting.Rare: accText = L("ui.creator.acc_rare_text"); break;
                case AccidentSetting.Normal: accText = L("ui.creator.acc_normal_text"); break;
                default: accText = L("ui.creator.acc_pick_text"); break;
            }
            GUILayout.Label("   " + accText, _smallStyle);
        }

        private void DrawPersonality()
        {
            int focusIndex = _draft.SkillFocus.HasValue ? 1 + (int)_draft.SkillFocus.Value : 0;
            int newFocus = Row(L("ui.creator.focus"), () => GUILayout.Toolbar(Mathf.Min(focusIndex, FocusLabels.Length - 1), FocusLabels));
            _draft.SkillFocus = newFocus == 0 ? (SkillType?)null : (SkillType)(newFocus - 1);
            GUILayout.Label("   " + L("ui.creator.focus_note"), _smallStyle);

            // Yatkınlık (28 karar 1, 45 karar 3): en çok 1 beceri; o becerilerde XP x1,5 ve günlük doygunluk eşiği yüksek.
            Section(GameLocalization.T("ui.creator.aptitudes", ("n", _draft.Aptitudes.Count), ("max", CharacterDraft.MaxFounderAptitudes)));
            var all = (SkillType[])System.Enum.GetValues(typeof(SkillType));
            for (int i = 0; i < all.Length; i += 4)
            {
                GUILayout.BeginHorizontal();
                for (int j = i; j < Mathf.Min(i + 4, all.Length); j++)
                {
                    bool on = _draft.Aptitudes.Contains(all[j]);
                    if (GUILayout.Toggle(on, SkillNames.Name(all[j]), GUILayout.Width(120f)) != on) _draft.ToggleAptitude(all[j]);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Label(L("ui.creator.aptitude_note"), _smallStyle);

            if (Content != null && Content.Traits.Count > 0)
            {
                Section(GameLocalization.T("ui.creator.traits", ("n", _draft.TraitIds.Count), ("max", Character.MaxTraits)));
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
                GUILayout.Label(_traitStatus.Length > 0 ? _traitStatus : L("ui.creator.traits_note"), _smallStyle);
            }

            Section(L("ui.creator.likes"));
            var foods = FoodNames();
            if (foods.Count > 0)
            {
                _draft.LikedFood = TextCycle(L("ui.creator.liked_food"), foods, _draft.LikedFood, true);
                _draft.DislikedFood = TextCycle(L("ui.creator.disliked_food"), foods, _draft.DislikedFood, true);
            }
            _draft.LikedColor = TextCycle(L("ui.creator.liked_color"), Likes.Colors.ToList(), _draft.LikedColor, true);
            _draft.LikedHobby = TextCycle(L("ui.creator.hobby"), Likes.Hobbies.ToList(), _draft.LikedHobby, true);
            GUILayout.Label(L("ui.creator.likes_note"), _smallStyle);
        }

        private void DrawVoice()
        {
            var a = _draft.Appearance;
            a.Voice = (VoiceTimbre)Row(L("ui.creator.timbre"), () => GUILayout.Toolbar((int)a.Voice, VoiceLabels));
            GUILayout.BeginHorizontal();
            GUILayout.Label(GameLocalization.T("ui.creator.pitch", ("n", (a.VoicePitch > 0 ? "+" : "") + a.VoicePitch)), GUILayout.Width(100f));
            a.VoicePitch = Mathf.RoundToInt(GUILayout.HorizontalSlider(a.VoicePitch, Appearance.MinPitch, Appearance.MaxPitch, GUILayout.Width(200f)));
            GUILayout.EndHorizontal();
            a.LaughId = OptionRow(L("ui.creator.laugh"), _catalog.Laughs, a.LaughId);
            GUILayout.Label(L("ui.creator.voice_note"), _smallStyle);
        }

        private void DrawBody()
        {
            var a = _draft.Appearance;
            a.Build = (BodyBuild)Row(L("ui.creator.build"), () => GUILayout.Toolbar((int)a.Build, BuildLabels));
            a.Frame = (BodyFrame)Row(L("ui.creator.frame"), () => GUILayout.Toolbar((int)a.Frame, FrameLabels));
            GUILayout.Label(L("ui.creator.build_note"), _smallStyle);
            a.SkinColor = ColorRow("skin", L("ui.creator.skin"), a.SkinColor, _catalog.SkinTones, false);
        }

        private void DrawFace()
        {
            var a = _draft.Appearance;
            a.FaceShapeId = OptionRow(L("ui.creator.face_shape"), _catalog.FaceShapes, a.FaceShapeId);
            a.EyeShapeId = OptionRow(L("ui.creator.eye_shape"), _catalog.EyeShapes, a.EyeShapeId);
            a.EyeColor = ColorRow("eye", L("ui.creator.eye_color"), a.EyeColor, _catalog.EyeColors, true);
            bool hetero = a.EyeColorRight.HasValue;
            bool nowHetero = GUILayout.Toggle(hetero, " " + L("ui.creator.heterochromia"));
            if (nowHetero && !hetero) a.EyeColorRight = _catalog.EyeColors.FirstOrDefault(e => e != a.EyeColor);
            if (!nowHetero) a.EyeColorRight = null;
            if (a.EyeColorRight.HasValue)
                a.EyeColorRight = ColorRow("eyeR", L("ui.creator.right_eye"), a.EyeColorRight.Value, _catalog.EyeColors, true);
            a.BrowId = OptionRow(L("ui.creator.brow"), _catalog.Brows, a.BrowId);
            a.BrowFollowsHair = GUILayout.Toggle(a.BrowFollowsHair, " " + L("ui.creator.brow_follows"));
            if (!a.BrowFollowsHair) a.BrowColor = ColorRow("brow", L("ui.creator.brow_color"), a.BrowColor, _catalog.HairColors, true);
            a.NoseId = OptionRow(L("ui.creator.nose"), _catalog.Noses, a.NoseId);
            a.MouthId = OptionRow(L("ui.creator.mouth"), _catalog.Mouths, a.MouthId);
            a.EarsId = OptionRow(L("ui.creator.ears"), _catalog.Ears, a.EarsId);
            a.LashesId = OptionRow(L("ui.creator.lashes"), _catalog.Lashes, a.LashesId);
            GUILayout.Label(L("ui.creator.face_note"), _smallStyle);
        }

        private void DrawHair()
        {
            var a = _draft.Appearance;
            // Modüler saç (belge 43b): önce hazır stil; "Ayrıntılı" kütle, ön, eklenti ve süsü ayrı açar.
            PresetRow(a);
            _hairDetailed = GUILayout.Toggle(_hairDetailed, L("ui.creator.hair_detailed"));
            if (_hairDetailed)
            {
                a.HairStyleId = OptionRow(L("ui.creator.hair_style"), _catalog.HairStyles, a.HairStyleId);
                _catalog.FitHair(a);
                a.BangsId = OptionRow(L("ui.creator.bangs"), _catalog.FittingModules(_catalog.Bangs, a.HairStyleId).ToList(), a.BangsId);
                a.HairExtraId = OptionRow(L("ui.creator.hair_extra"), _catalog.FittingModules(_catalog.HairExtras, a.HairStyleId).ToList(), a.HairExtraId);
            }
            a.HairOrnamentId = OptionRow(L("ui.creator.hair_ornament"), _catalog.FittingModules(_catalog.HairOrnaments, a.HairStyleId).ToList(), a.HairOrnamentId);
            if (a.HairOrnamentId != Appearance.None)
                a.HairOrnamentColor = ColorRow("orn", L("ui.creator.ornament_color"), a.HairOrnamentColor, _catalog.ClothColors, true);
            a.HairColor = ColorRow("hair", L("ui.creator.hair_color"), a.HairColor, _catalog.HairColors, true);
            a.MustacheId = OptionRow(L("ui.creator.mustache"), _catalog.Mustaches, a.MustacheId);
            a.BeardId = OptionRow(L("ui.creator.beard"), _catalog.Beards, a.BeardId);
            a.SideburnsId = OptionRow(L("ui.creator.sideburns"), _catalog.Sideburns, a.SideburnsId);
            a.StubbleLevel = Row(L("ui.creator.stubble"), () => GUILayout.Toolbar(a.StubbleLevel, StubbleLabels));
            if (a.MustacheId != Appearance.None || a.BeardId != Appearance.None || a.SideburnsId != Appearance.None || a.StubbleLevel > 0)
            {
                bool same = a.FacialHairColor == a.HairColor;
                bool nowSame = GUILayout.Toggle(same, " " + L("ui.creator.beard_same"));
                if (nowSame) a.FacialHairColor = a.HairColor;
                else
                {
                    if (same) a.FacialHairColor = _catalog.HairColors.FirstOrDefault(h => h != a.HairColor);
                    a.FacialHairColor = ColorRow("beard", L("ui.creator.beard_color"), a.FacialHairColor, _catalog.HairColors, true);
                }
            }
            GUILayout.Label(L("ui.creator.custom_color_note"), _smallStyle);
        }

        private void DrawMarks()
        {
            var a = _draft.Appearance;

            Section(GameLocalization.T("ui.creator.skin_details", ("n", MarkRules.Count(a.Marks, MarkKind.SkinDetail)), ("max", MarkRules.MaxSkinDetails)));
            ToggleMarks(MarkKind.SkinDetail, _catalog.SkinDetails, MarkRules.MaxSkinDetails, o => FirstTag(o.Tag, "face"));

            int chosenScars = a.Marks.Count(m => m.Kind == MarkKind.Scar && m.Source == MarkSource.Chosen);
            Section(GameLocalization.T("ui.creator.scars", ("n", chosenScars), ("max", MarkRules.MaxChosenScars)));
            ListMarks(MarkKind.Scar, _catalog.ScarPatterns, _catalog.ScarZones);
            if (chosenScars < MarkRules.MaxChosenScars && _catalog.ScarPatterns.Count > 0 && _catalog.ScarZones.Count > 0)
            {
                GUILayout.BeginHorizontal();
                _scarPattern = Cycle(_catalog.ScarPatterns, _scarPattern, 100f);
                _scarZone = Cycle(_catalog.ScarZones, _scarZone, 100f);
                if (GUILayout.Button(L("ui.creator.add"), GUILayout.Width(50f)))
                {
                    string zone = _catalog.ScarZones[_scarZone].Id;
                    a.Marks.RemoveAll(m => m.Kind == MarkKind.Scar && m.Source == MarkSource.Chosen && m.Zone == zone);
                    a.Marks.Add(new Mark { Kind = MarkKind.Scar, Id = _catalog.ScarPatterns[_scarPattern].Id, Zone = zone, Fade = 2, Source = MarkSource.Chosen });
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Label(L("ui.creator.scars_note"), _smallStyle);

            Section(L("ui.creator.tattoos"));
            ListMarks(MarkKind.Tattoo, _catalog.TattooPatterns, _catalog.TattooZones);
            if (_catalog.TattooPatterns.Count > 0 && _catalog.TattooZones.Count > 0)
            {
                GUILayout.BeginHorizontal();
                _tattooPattern = Cycle(_catalog.TattooPatterns, _tattooPattern, 100f);
                _tattooZone = Cycle(_catalog.TattooZones, _tattooZone, 100f);
                if (GUILayout.Button(L("ui.creator.add"), GUILayout.Width(50f)))
                {
                    string zone = _catalog.TattooZones[_tattooZone].Id;
                    a.Marks.RemoveAll(m => m.Kind == MarkKind.Tattoo && m.Zone == zone);
                    a.Marks.Add(new Mark { Kind = MarkKind.Tattoo, Id = _catalog.TattooPatterns[_tattooPattern].Id, Zone = zone, Color = _ink });
                }
                GUILayout.EndHorizontal();
                if (_catalog.InkColors.Count > 0) _ink = ColorRow("ink", L("ui.creator.ink"), _ink, _catalog.InkColors, false);
            }

            Section(GameLocalization.T("ui.creator.jewelry", ("n", MarkRules.Count(a.Marks, MarkKind.Jewelry)), ("max", MarkRules.MaxJewelry)));
            ToggleMarks(MarkKind.Jewelry, _catalog.Jewelry, MarkRules.MaxJewelry, o => FirstTag(o.Tag, "ear"));

            Section(GameLocalization.T("ui.creator.makeup", ("n", MarkRules.Count(a.Marks, MarkKind.Makeup)), ("max", MarkRules.MaxMakeup)));
            ToggleMarks(MarkKind.Makeup, _catalog.Makeup, MarkRules.MaxMakeup, _ => "face");
            foreach (var m in a.Marks.Where(x => x.Kind == MarkKind.Makeup).ToList())
                if (_catalog.MakeupColors.Count > 0) m.Color = ColorRow("mk_" + m.Id, Label(_catalog.Makeup, m.Id), m.Color, _catalog.MakeupColors, false);
        }

        private void DrawClothes()
        {
            var a = _draft.Appearance;
            a.OutfitId = OptionRow(L("ui.creator.top"), _catalog.Outfits, a.OutfitId);
            a.PatternId = OptionRow(L("ui.creator.pattern"), _catalog.Patterns, a.PatternId);
            a.OutfitColor = ColorRow("outfit", L("ui.creator.main_color"), a.OutfitColor, _catalog.ClothColors, true);
            a.TrimColor = ColorRow("trim", L("ui.creator.trim"), a.TrimColor, _catalog.ClothColors, true);
            a.BottomId = OptionRow(L("ui.creator.bottom"), _catalog.Bottoms, a.BottomId);
            a.ShoesId = OptionRow(L("ui.creator.shoes"), _catalog.Shoes, a.ShoesId);
            a.OuterId = OptionRow(L("ui.creator.outer"), _catalog.Outers, a.OuterId);
            if (a.OuterId != Appearance.None) a.OuterColor = ColorRow("outer", L("ui.creator.outer_color"), a.OuterColor, _catalog.ClothColors, true);
            a.HatId = OptionRow(L("ui.creator.hat"), _catalog.Hats, a.HatId);
            if (a.HatId != Appearance.None) a.HatColor = ColorRow("hat", L("ui.creator.hat_color"), a.HatColor, _catalog.ClothColors, true);
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
                    a.NoseId = r.NoseId; a.MouthId = r.MouthId; a.EarsId = r.EarsId; a.LashesId = r.LashesId;
                    break;
                case Tab.Hair:
                    a.HairStyleId = r.HairStyleId; a.BangsId = r.BangsId; a.HairExtraId = r.HairExtraId; a.HairColor = r.HairColor;
                    a.HairOrnamentId = r.HairOrnamentId; a.HairOrnamentColor = r.HairOrnamentColor;
                    a.MustacheId = r.MustacheId; a.BeardId = r.BeardId; a.SideburnsId = r.SideburnsId; a.StubbleLevel = r.StubbleLevel;
                    a.FacialHairColor = r.FacialHairColor;
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
                    string[] fades = { L("ui.creator.fade.fresh"), L("ui.creator.fade.faded"), L("ui.creator.fade.old") };
                    m.Fade = GUILayout.Toolbar(m.Fade, fades, GUILayout.Width(150f));
                }
                if (GUILayout.Button(L("ui.creator.remove"), GUILayout.Width(60f))) a.Marks.Remove(m);
                GUILayout.EndHorizontal();
            }
        }

        private static string[] StubbleLabels => new[] { L("ui.creator.stubble.none"), L("ui.creator.stubble.light"), L("ui.creator.stubble.medium"), L("ui.creator.stubble.dense") };
        private bool _hairDetailed;
        private List<OptionDef> _presetOptions;

        /// <summary>Hazır saç stilleri (belge 43b): seçince kütle, ön ve eklenti birlikte değişir.</summary>
        private void PresetRow(Appearance a)
        {
            if (_catalog.HairPresets.Count == 0) return;
            if (_presetOptions == null)
                _presetOptions = _catalog.HairPresets.Select(p => new OptionDef { Id = p.Id, DisplayName = p.DisplayName, Group = p.Group }).ToList();
            var current = _catalog.PresetOf(a);
            if (current == null)
            {
                // Ayrıntılı modda yapılmış birleşim hazır stillerde yoksa "Özel" görünür; oklar ilk hazır stile döner.
                GUILayout.BeginHorizontal();
                GUILayout.Label(L("ui.creator.hair"), GUILayout.Width(100f));
                if (GUILayout.Button("<", GUILayout.Width(28f))) _catalog.ApplyPreset(a, _catalog.HairPresets[_catalog.HairPresets.Count - 1]);
                GUILayout.Label(L("ui.creator.custom"), _centerStyle, GUILayout.Width(130f));
                if (GUILayout.Button(">", GUILayout.Width(28f))) _catalog.ApplyPreset(a, _catalog.HairPresets[0]);
                GUILayout.EndHorizontal();
                return;
            }
            string chosen = OptionRow(L("ui.creator.hair"), _presetOptions, current.Id);
            if (chosen != current.Id) _catalog.ApplyPreset(a, _catalog.HairPresets.First(p => p.Id == chosen));
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
                    // Renk kutusu gerçek renkle düz çizilir: eskiden bordo düğme dokusu backgroundColor ile çarpılıyor, sarı, gri
                    // ve mavi de koyu kahve görünüyordu (tester raporu 65 A5). Seçili renk kalın mürekkep çerçeveyle gösterilir.
                    bool selected = palette[i] == current;
                    var cell = GUILayoutUtility.GetRect(20f, 18f, GUILayout.Width(20f), GUILayout.Height(18f));
                    if (Event.current.type == EventType.Repaint)
                    {
                        var prev = GUI.color;
                        GUI.color = selected ? UiSkin.Ink : new Color(0f, 0f, 0f, 0.35f);
                        GUI.DrawTexture(cell, Texture2D.whiteTexture);
                        GUI.color = ToColor(palette[i]);
                        float inset = selected ? 3f : 1f;
                        GUI.DrawTexture(new Rect(cell.x + inset, cell.y + inset, cell.width - 2 * inset, cell.height - 2 * inset), Texture2D.whiteTexture);
                        GUI.color = prev;
                    }
                    if (GUI.Button(cell, GUIContent.none, GUIStyle.none)) current = palette[i];
                }
                GUILayout.EndHorizontal();
            }

            if (allowCustom)
            {
                bool open = _customOpen.Contains(id);
                bool newOpen = GUILayout.Toggle(open, " " + GameLocalization.T("ui.creator.custom_color", ("hex", current.ToHex())));
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
