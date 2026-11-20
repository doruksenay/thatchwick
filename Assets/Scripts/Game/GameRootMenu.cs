using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Audio;
using Thatchwick.Core.Localization;
using Thatchwick.Core.Progress;
using Thatchwick.Core.Save;
using Thatchwick.Game.Audio;

namespace Thatchwick.Game
{
    /// <summary>
    /// Başlık ekranı (üç kayıt yuvası), ayarlar (dil, ses) ve ilk gün rehberi. Geçici IMGUI; metinler anahtarlı.
    /// Akış: oyun açılınca başlık ekranı gelir; yuva seçilir (devam ya da yeni oyun), sonra eski akış sürer
    /// (yerleşim adı, karakter yaratma). Oyun içindeki "Yeni oyun" da başlık ekranına döner.
    /// </summary>
    public sealed partial class GameRoot
    {
        private const string SlotPref = "save_slot";
        /// <summary>Etkin kayıt yuvası (1-3). Kayıt ve yükleme bu yuvanın dosyasına gider.</summary>
        public static int SaveSlot
        {
            get => SaveSlots.Clamp(PlayerPrefs.GetInt(SlotPref, 1));
            set { PlayerPrefs.SetInt(SlotPref, SaveSlots.Clamp(value)); PlayerPrefs.Save(); }
        }

        private static string SlotPath(int slot) => System.IO.Path.Combine(Application.persistentDataPath, SaveSlots.FileName(slot));

        private bool _title;                 // başlık ekranı açık (yeni açılış ya da "Yeni oyun")
        private SaveSlots.Info[] _slotInfo;  // yuva özetleri (başlık ekranı açılırken bir kez okunur)
        private int _confirmSlot;            // üzerine yazma ya da silme onayı bekleyen yuva
        private bool _confirmDelete;
        private bool _settingsOpen;

        private void OpenTitle()
        {
            _title = true;
            _confirmSlot = 0;
            _slotInfo = new SaveSlots.Info[SaveSlots.Count + 1];
            for (int i = 1; i <= SaveSlots.Count; i++) _slotInfo[i] = ReadSlot(i);
        }

        private SaveSlots.Info ReadSlot(int slot)
        {
            try
            {
                if (!SaveFiles.TryRead(SlotPath(slot), IsReadableSave, out string json, out _)) return null;
                return SaveSlots.Describe(JsonUtility.FromJson<SaveData>(json), _clock != null ? _clock.Config.DaysPerYear : 40);
            }
            catch (System.Exception) { return null; }
        }

        private void DrawTitle()
        {
            if (AchievementsVisible) return;   // başarım penceresi UI Toolkit'te; IMGUI başlık onun üstünü örtmesin
            float s = Mathf.Max(1f, Screen.height / 540f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));
            float sw = Screen.width / s, sh = Screen.height / s;
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
            GUI.color = Color.white;

            float pw = Mathf.Min(sw - 16f, 520f), ph = Mathf.Min(sh - 16f, 460f);
            GUILayout.BeginArea(new Rect((sw - pw) / 2f, (sh - ph) / 2f, pw, ph), GUI.skin.box);
            var title = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            var sub = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            GUILayout.Label("Thatchwick", title);                       // ad ve alt başlık hiçbir dilde çevrilmez (belge 21)
            GUILayout.Label("Generations of a Village", sub);
            GUILayout.Space(8f);

            if (_settingsOpen) { DrawSettings(); GUILayout.EndArea(); GUI.matrix = Matrix4x4.identity; return; }

            for (int i = 1; i <= SaveSlots.Count; i++) DrawSlotRow(i);

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(T("ui.menu.settings"), GUILayout.Height(28f))) _settingsOpen = true;
            if (!Application.isMobilePlatform && !Application.isEditor && GUILayout.Button(T("ui.menu.quit"), GUILayout.Height(28f), GUILayout.Width(120f))) Application.Quit();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUI.matrix = Matrix4x4.identity;
        }

        private void DrawSlotRow(int slot)
        {
            var info = _slotInfo != null ? _slotInfo[slot] : null;
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();
            string head = GameLocalization.T("ui.menu.slot", ("n", slot));
            if (info == null) GUILayout.Label(head + " · " + T("ui.menu.slot_empty"));
            else
            {
                string name = string.IsNullOrEmpty(info.Dynasty) ? "?" : info.Dynasty;
                string place = string.IsNullOrEmpty(info.Settlement) ? "" : " · " + info.Settlement;
                GUILayout.Label(head + " · " + name + place + "\n" + GameLocalization.T(info.Ended ? "ui.menu.slot_ended" : "ui.menu.slot_line",
                    ("year", info.Year), ("day", info.DayOfYear), ("gen", info.Generation)));
            }
            GUILayout.FlexibleSpace();
            if (_confirmSlot == slot)
            {
                GUILayout.Label(T(_confirmDelete ? "ui.menu.confirm_delete" : "ui.menu.confirm_overwrite"), GUILayout.Width(150f));
                if (GUILayout.Button(T("ui.menu.yes"), GUILayout.Width(52f)))
                {
                    if (_confirmDelete) { DeleteSlot(slot); _confirmSlot = 0; }
                    else StartNewGame(slot);
                }
                if (GUILayout.Button(T("ui.menu.no"), GUILayout.Width(52f))) _confirmSlot = 0;
            }
            else
            {
                if (info != null && !info.Ended && GUILayout.Button(T("ui.menu.continue"), GUILayout.Width(96f), GUILayout.Height(30f))) { SaveSlot = slot; LoadSave(); }
                if (GUILayout.Button(T("ui.menu.new_game"), GUILayout.Width(96f), GUILayout.Height(30f)))
                {
                    if (info == null) StartNewGame(slot);
                    else { _confirmSlot = slot; _confirmDelete = false; }
                }
                if (info != null && GUILayout.Button(T("ui.menu.delete"), GUILayout.Width(52f), GUILayout.Height(30f))) { _confirmSlot = slot; _confirmDelete = true; }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        /// <summary>Seçilen yuvada yeni oyun: başlık kapanır, yerleşim adı ve karakter yaratma ekranı gelir.</summary>
        private void StartNewGame(int slot)
        {
            SaveSlot = slot;
            _hasSave = false;      // yaratma ekranındaki "kayıtlı oyuna devam et" yalnızca bu yuvanın kaydı içindi
            _confirmSlot = 0;
            _title = false;
            _settingsOpen = false;
        }

        private void DeleteSlot(int slot)
        {
            string path = SlotPath(slot);
            foreach (var p in new[] { path, path + ".bak", path + ".tmp" })
            {
                try { if (System.IO.File.Exists(p)) System.IO.File.Delete(p); }
                catch (System.Exception ex) { Debug.LogWarning($"Kayıt silinemedi: {ex.Message}"); }
            }
            if (_slotInfo != null) _slotInfo[slot] = ReadSlot(slot);
            if (slot == SaveSlot) _hasSave = false;
        }

        // ---- ayarlar: dil ve ses (PlayerPrefs) ----

        private static readonly (AudioGroup group, string key)[] VolumeRows =
        {
            (AudioGroup.Music, "ui.settings.music"), (AudioGroup.Ambience, "ui.settings.ambience"), (AudioGroup.Sfx, "ui.settings.sfx"),
            (AudioGroup.Voice, "ui.settings.voice"), (AudioGroup.Ui, "ui.settings.ui")
        };

        private static string VolumePref(AudioGroup g) => "vol_" + g;

        /// <summary>Kayıtlı ses düzeylerini uygular (açılışta bir kez).</summary>
        private static void ApplySavedVolumes()
        {
            var audio = AudioService.Instance;
            if (audio == null) return;
            foreach (var (g, _) in VolumeRows)
                if (PlayerPrefs.HasKey(VolumePref(g))) audio.SetVolume(g, Mathf.Clamp01(PlayerPrefs.GetFloat(VolumePref(g))));
        }

        private void DrawSettings()
        {
            GUILayout.Label(T("ui.settings.title"));
            GUILayout.BeginHorizontal();
            GUILayout.Label(T("ui.language.button"), GUILayout.Width(110f));
            foreach (var code in Languages.Released)
            {
                bool on = GameLocalization.Current.Language == code;
                if (GUILayout.Toggle(on, Languages.NativeName(code), GUI.skin.button, GUILayout.Width(110f)) && !on) GameLocalization.SetLanguage(code);
            }
            GUILayout.EndHorizontal();
            GUILayout.Label(T("ui.language.restart_note"));

            var audio = AudioService.Instance;
            if (audio != null)
                foreach (var (g, key) in VolumeRows)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(T(key), GUILayout.Width(110f));
                    float cur = audio.GetVolume(g);
                    float now = GUILayout.HorizontalSlider(cur, 0f, 1f, GUILayout.Width(220f));
                    GUILayout.Label(Mathf.RoundToInt(now * 100f) + "%", GUILayout.Width(44f));
                    GUILayout.EndHorizontal();
                    if (Mathf.Abs(now - cur) > 0.005f) { audio.SetVolume(g, now); PlayerPrefs.SetFloat(VolumePref(g), now); }
                }
            DrawDisplaySettings();   // masaüstü: tam ekran, çözünürlük, dikey eşitleme (GameRootPlatform.cs)
            GUILayout.FlexibleSpace();
            DrawSystemSettings();    // başarımlar, hata raporu, ana menü ve çıkış, sürüm
            if (GUILayout.Button(T("ui.settings.back"), GUILayout.Height(28f))) { _settingsOpen = false; PlayerPrefs.Save(); }
        }

        /// <summary>Oyun içinde ayarlar penceresi (üst şeritteki düğmeden).</summary>
        private void DrawSettingsWindow(float s)
        {
            if (AchievementsVisible) return;   // başarım penceresi (UI Toolkit) açıkken üstünü örtmesin
            float sw = Screen.width / s, sh = Screen.height / s;
            float wh = Mathf.Min(sh - 16f, 430f);
            GUILayout.BeginArea(new Rect((sw - 460f) / 2f, (sh - wh) / 2f, 460f, wh), GUI.skin.box);
            DrawSettings();
            GUILayout.EndArea();
        }

        // ---- ilk gün rehberi ----

        private void DrawGuide(float s)
        {
            if (_account == null || _creator != null || _title || !FirstDayGuide.IsActive(_account)) return;
            if (FirstDayGuide.CloseIfComplete(_account)) { ShowMessage(T("ui.guide.done"), 5f); return; }
            if (MenuOpen()) return;   // paneller açıkken yer kaplamasın
            float sw = Screen.width / s;
            GUILayout.BeginArea(new Rect(sw - 268f, 64f, 260f, 190f), GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label(GameLocalization.T("ui.guide.title", ("done", FirstDayGuide.DoneCount(_account)), ("total", FirstDayGuide.Steps.Count)));
            if (GUILayout.Button("×", GUILayout.Width(24f))) FirstDayGuide.Dismiss(_account);
            GUILayout.EndHorizontal();
            bool nextShown = false;
            var wrap = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 11 };
            foreach (var step in FirstDayGuide.Steps)
            {
                bool done = FirstDayGuide.IsDone(step, _account);
                var prev = GUI.color;
                if (done) GUI.color = new Color(1f, 1f, 1f, 0.5f);
                GUILayout.Label((done ? "✓ " : "○ ") + T("ui.guide." + step.Id), wrap);
                GUI.color = prev;
                if (!done && !nextShown) { nextShown = true; GUILayout.Label("   " + T("ui.guide." + step.Id + ".how"), wrap); }
            }
            GUILayout.EndArea();
        }
    }
}
