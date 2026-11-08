using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Customization;

namespace Thatchwick.Game
{
    /// <summary>
    /// Karakter yaratma ekranı (IMGUI): isim, cinsiyet, başlangıç evresi, beceri eğilimi, vücut,
    /// boy, ten, göz, saç (tür + renk), sakal, yüz işareti, kıyafet (tür + iki renk) ve başlık.
    /// Sol tarafta canlı önizleme. Geçici arayüz; asıl arayüz Adım 27'de.
    /// </summary>
    public sealed class CharacterCreator
    {
        private readonly AppearanceCatalog _catalog;
        private readonly System.Random _rng;
        private readonly CharacterDraft _draft;
        private Sprite _preview;
        private string _previewKey;
        private Vector2 _scroll;
        private List<string> _errors = new List<string>();
        private readonly HashSet<string> _customOpen = new HashSet<string>();
        private GUIStyle _titleStyle;
        private GUIStyle _smallStyle;

        public bool Finished { get; private set; }
        public CharacterDraft Draft => _draft;

        private static readonly string[] SexLabels = { "Kadın", "Erkek" };
        private static readonly string[] StageLabels = { "Genç (30 gün)", "Yetişkin (40 gün)" };
        private static readonly string[] BuildLabels = { "İnce", "Orta", "Gürbüz" };
        private static readonly string[] FocusLabels = { "Yok", "Çiftçilik", "Odunculuk", "Yemek", "Marangozluk" };

        public CharacterCreator(AppearanceCatalog catalog, int seed = 1)
        {
            _catalog = catalog;
            _rng = new System.Random(seed);
            _draft = CharacterDraft.CreateRandom(catalog, _rng, "Aldric");
            _draft.SetSex(Sex.Male, catalog);
            _draft.Appearance = catalog.Randomize(_rng, Sex.Male);
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
                _titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
                _smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            }

            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
            GUI.color = Color.white;

            float pw = Mathf.Min(sw - 16f, 780f);
            float ph = Mathf.Min(sh - 16f, 500f);
            var panel = new Rect((sw - pw) / 2f, (sh - ph) / 2f, pw, ph);
            GUI.Box(panel, GUIContent.none);

            GUILayout.BeginArea(new Rect(panel.x + 10f, panel.y + 8f, panel.width - 20f, panel.height - 16f));
            GUILayout.Label("Karakter yarat", _titleStyle);
            GUILayout.BeginHorizontal();

            DrawPreviewColumn(ph);
            DrawOptionsColumn(ph);

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawPreviewColumn(float panelHeight)
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
            float boy = a.HeightPercent / 100f;
            var area = GUILayoutUtility.GetRect(210f, 300f);
            GUI.color = new Color(0.25f, 0.4f, 0.25f, 1f);
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = Color.white;
            float w = 24f * scale * boy;
            float h = 48f * scale * boy;
            var r = new Rect(area.x + (area.width - w) / 2f, area.yMax - 10f - h, w, h);
            GUI.DrawTexture(r, _preview.texture, ScaleMode.StretchToFill, true);

            GUILayout.Space(4f);
            GUILayout.Label($"{_draft.Name} · {(_draft.Sex == Sex.Female ? "Kadın" : "Erkek")} · {(_draft.StartStage == AgeStage.Youth ? "Genç" : "Yetişkin")}", _smallStyle);

            if (GUILayout.Button("Rastgele görünüm"))
            {
                _draft.Appearance = _catalog.Randomize(_rng, _draft.Sex);
                _errors = new List<string>();
            }
            GUILayout.EndVertical();
        }

        private void DrawOptionsColumn(float panelHeight)
        {
            GUILayout.BeginVertical();
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(Mathf.Max(200f, panelHeight - 130f)));
            var a = _draft.Appearance;

            // Kimlik
            Section("Kimlik");
            GUILayout.BeginHorizontal();
            GUILayout.Label("İsim", GUILayout.Width(90f));
            _draft.Name = GUILayout.TextField(_draft.Name ?? "", CharacterDraft.MaxNameLength);
            GUILayout.EndHorizontal();

            int sexIndex = _draft.Sex == Sex.Female ? 0 : 1;
            int newSex = Row("Cinsiyet", () => GUILayout.Toolbar(sexIndex, SexLabels));
            if (newSex != sexIndex) _draft.SetSex(newSex == 0 ? Sex.Female : Sex.Male, _catalog);

            int stageIndex = _draft.StartStage == AgeStage.Youth ? 0 : 1;
            int newStage = Row("Başlangıç", () => GUILayout.Toolbar(stageIndex, StageLabels));
            _draft.StartStage = newStage == 0 ? AgeStage.Youth : AgeStage.Adult;

            int focusIndex = _draft.SkillFocus.HasValue ? 1 + (int)_draft.SkillFocus.Value : 0;
            int newFocus = Row("Beceri eğilimi", () => GUILayout.Toolbar(focusIndex, FocusLabels));
            _draft.SkillFocus = newFocus == 0 ? (SkillType?)null : (SkillType)(newFocus - 1);
            GUILayout.Label("   Eğilim seçilen beceriyle seviye 2 başlatır.", _smallStyle);

            // Beden
            Section("Beden");
            int newBuild = Row("Vücut", () => GUILayout.Toolbar((int)a.Build, BuildLabels));
            a.Build = (BodyBuild)newBuild;
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Boy %{a.HeightPercent}", GUILayout.Width(90f));
            a.HeightPercent = Mathf.RoundToInt(GUILayout.HorizontalSlider(a.HeightPercent, AppearanceCatalog.MinHeight, AppearanceCatalog.MaxHeight));
            GUILayout.EndHorizontal();
            a.SkinColor = ColorRow("skin", "Ten", a.SkinColor, _catalog.SkinTones, false);

            // Yüz
            Section("Yüz");
            a.EyeColor = ColorRow("eye", "Göz rengi", a.EyeColor, _catalog.EyeColors, true);
            a.FaceMarkId = OptionRow("Yüz işareti", _catalog.FaceMarks, a.FaceMarkId);

            // Saç
            Section("Saç ve sakal");
            a.HairStyleId = OptionRow("Saç stili", _catalog.HairStyles, a.HairStyleId);
            a.HairColor = ColorRow("hair", "Saç rengi", a.HairColor, _catalog.HairColors, true);
            if (_draft.Sex == Sex.Male)
            {
                a.FacialHairId = OptionRow("Sakal/bıyık", _catalog.FacialHair, a.FacialHairId);
                a.FacialHairColor = a.HairColor; // sakal rengi saçı izler
            }
            else
            {
                a.FacialHairId = Appearance.None;
            }

            // Kıyafet
            Section("Kıyafet");
            a.OutfitId = OptionRow("Kıyafet", _catalog.Outfits, a.OutfitId);
            a.OutfitColor = ColorRow("outfit", "Ana renk", a.OutfitColor, _catalog.ClothColors, true);
            a.TrimColor = ColorRow("trim", "Kenar/pantolon", a.TrimColor, _catalog.ClothColors, true);
            a.HatId = OptionRow("Başlık", _catalog.Hats, a.HatId);
            if (a.HatId != Appearance.None)
                a.HatColor = ColorRow("hat", "Başlık rengi", a.HatColor, _catalog.ClothColors, true);

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
                _errors = _draft.Validate(_catalog);
                if (_errors.Count == 0) Finished = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        // ---- arayüz yardımcıları ----

        private void Section(string title)
        {
            GUILayout.Space(6f);
            GUILayout.Label(title, _titleStyle);
        }

        private int Row(string label, Func<int> draw)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(90f));
            int v = draw();
            GUILayout.EndHorizontal();
            return v;
        }

        private string OptionRow(string label, List<OptionDef> all, string currentId)
        {
            var list = _catalog.For(all, _draft.Sex).ToList();
            if (list.Count == 0) return currentId;
            int index = list.FindIndex(o => o.Id == currentId);
            if (index < 0) index = 0;

            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(90f));
            if (GUILayout.Button("<", GUILayout.Width(28f))) index = (index - 1 + list.Count) % list.Count;
            GUILayout.Label(list[index].DisplayName, new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter }, GUILayout.Width(130f));
            if (GUILayout.Button(">", GUILayout.Width(28f))) index = (index + 1) % list.Count;
            GUILayout.Label($"{index + 1}/{list.Count}", _smallStyle);
            GUILayout.EndHorizontal();
            return list[index].Id;
        }

        private static Color ToColor(Rgb c) => new Color32(c.R, c.G, c.B, 255);

        private Rgb ColorRow(string id, string label, Rgb current, List<Rgb> palette, bool allowCustom)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(90f));

            int perRow = 14;
            GUILayout.BeginVertical();
            for (int start = 0; start < palette.Count; start += perRow)
            {
                GUILayout.BeginHorizontal();
                for (int i = start; i < Math.Min(start + perRow, palette.Count); i++)
                {
                    var prev = GUI.backgroundColor;
                    GUI.backgroundColor = ToColor(palette[i]);
                    bool selected = palette[i] == current;
                    if (GUILayout.Button(selected ? "*" : "", GUILayout.Width(22f), GUILayout.Height(20f)))
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
