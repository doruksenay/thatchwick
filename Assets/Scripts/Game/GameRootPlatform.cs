using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Thatchwick.Core.Progress;
using Thatchwick.Core.Save;
using Thatchwick.Game.Platform;

namespace Thatchwick.Game
{
    /// <summary>
    /// Yayın altyapısının GameRoot'a bağlandığı yer (20.11.2026): platform (mağaza), hata bildirimi, başarımlar,
    /// masaüstü görüntü ayarları ve Esc menüsü. GameRoot.cs'e yalnızca birkaç çağrı eklenir (Awake, Update, kayıt,
    /// çıkış); gerisi burada durur ki paralel dallar aynı satırlarda çakışmasın.
    /// </summary>
    public sealed partial class GameRoot
    {
        // ---------------- açılış ve kare ----------------

        /// <summary>Awake'in ilk işi: hata toplayıcı, platform, profil. Sahne yeniden kurulunca yeniden çağrılır (zararsız).</summary>
        private void InitPlatform()
        {
            ErrorReporter.Init();
            PlatformServices.Init();
            ErrorReporter.RefreshHeader();
            ApplySavedDisplay();
            LoadAchievements();
            if (ErrorReporter.PreviousSessionUnclean && !_uncleanNoticeShown)
            {
                _uncleanNoticeShown = true;
                _pendingNotice = T("ui.error.prev_session");
            }
        }

        private static bool _uncleanNoticeShown;   // uygulama ömründe bir kez (sahne yeniden kurulsa da)
        private string _pendingNotice;
        private float _nextErrorNotice;
        private float _nextAchievementCheck;
        private static volatile string _saveError;   // arka plan kayıt yazımının hatası (ana iş parçacığı okur)
        private float _saveRetryAt;

        /// <summary>Update'in ilk işi. Oyun başlamadan (başlık, karakter yaratma) da çalışır.</summary>
        private void TickPlatform()
        {
            PlatformServices.Current.Tick();

            // Yeni bir hata görüldüyse oyuncuya tek satır (en çok 30 saniyede bir; hata fırtınasında ekranı kaplamasın).
            int fresh = ErrorReporter.TakeUnseen();
            if (fresh > 0 && Time.unscaledTime >= _nextErrorNotice)
            {
                _nextErrorNotice = Time.unscaledTime + 30f;
                _pendingNotice = T("ui.error.notice");
            }
            if (_pendingNotice != null && Time.unscaledTime > 1f)
            {
                ShowMessage(_pendingNotice, 7f);
                _pendingNotice = null;
            }

            // Arka plandaki otomatik kayıt yazılamadıysa (disk dolu, izin yok): oyuncu bilsin, bir kez de yeniden dene.
            string saveError = _saveError;
            if (saveError != null)
            {
                _saveError = null;
                ErrorReporter.Report("save", "Otomatik kayıt yazılamadı: " + saveError);
                ShowMessage(T("ui.save.failed"), 8f);
                _saveRetryAt = Time.unscaledTime + 20f;
            }
            if (_saveRetryAt > 0f && Time.unscaledTime >= _saveRetryAt)
            {
                _saveRetryAt = 0f;
                SaveNow(quiet: true);   // bu kez beklemeden: hata sürüyorsa SaveNow kendisi bildirir
            }

            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame) OnEscape();

            if (Time.unscaledTime >= _nextAchievementCheck)
            {
                _nextAchievementCheck = Time.unscaledTime + 2f;
                CheckAchievements();
            }
        }

        /// <summary>
        /// Esc: açık başarım listesini ya da ayarları kapatır; hiçbir pencere yokken oyun içi menüyü (ayarlar) açar.
        /// Başka bir panel açıkken karışmaz (paneller kendi kapatma düğmesini kullanır).
        /// </summary>
        private void OnEscape()
        {
            if (AchievementsVisible) { _achievementsPanel.Visible = false; return; }
            if (LedgerVisible) { _ledger.Visible = false; return; }
            if (CloseUiWindows()) return;
            if (_settingsOpen) { _settingsOpen = false; PlayerPrefs.Save(); return; }
            if (_creator != null || _player == null || _dynastyEnded) return;
            if (_vivView != null && _vivView.Active) return;   // canlılık evi Esc'yi kendisi kullanır
            if (!MenuOpen()) _settingsOpen = true;
        }

        /// <summary>Uygulama kapanırken ve arka plana alınırken (GameRoot.OnApplicationQuit / OnApplicationPause).</summary>
        private void OnSessionPaused(bool quitting)
        {
            SaveAchievements();
            PlayerPrefs.Save();
            ErrorReporter.SessionEnded();
            if (quitting) PlatformServices.Shutdown();
        }

        /// <summary>Oyun içinden başlık ekranına dönüş: önce kaydeder, sonra GameRoot'u yeniden kurar.</summary>
        private void ReturnToTitle()
        {
            SaveNow(quiet: true);
            SaveAchievements();
            enabled = false;
            new GameObject("GameRoot").AddComponent<GameRoot>();
            Destroy(gameObject);
        }

        // ---------------- başarımlar ----------------

        private const string AchievementFileName = "thatchwick_profile.json";
        private AchievementService _achievements;
        private Ui.AchievementsPanel _achievementsPanel;
        private bool _platformSynced;

        private static string AchievementPath => Path.Combine(Application.persistentDataPath, AchievementFileName);
        private bool AchievementsVisible => _achievementsPanel != null && _achievementsPanel.Visible;

        private void LoadAchievements()
        {
            AchievementProfile profile = null;
            try
            {
                // Kayıtlarla aynı güvenli okuma: ana dosya bozuksa yedeğe düşer.
                if (SaveFiles.TryRead(AchievementPath, text => !string.IsNullOrEmpty(text) && text.TrimStart().StartsWith("{", StringComparison.Ordinal), out string json, out _))
                    profile = JsonUtility.FromJson<AchievementProfile>(json);
            }
            catch (Exception ex) { ErrorReporter.Report("achievements", ex); }
            _achievements = new AchievementService(AchievementCatalog.Default, profile);
            _achievements.Unlocked += OnAchievementUnlocked;
            _platformSynced = false;
        }

        private void SaveAchievements()
        {
            if (_achievements == null || !_achievements.Dirty) return;
            try
            {
                SaveFiles.WriteAtomic(AchievementPath, JsonUtility.ToJson(_achievements.ToProfile()));
                _achievements.MarkSaved();
            }
            catch (Exception ex) { ErrorReporter.Report("achievements", ex); }
        }

        private void OnAchievementUnlocked(AchievementDef def)
        {
            PlatformServices.Current.UnlockAchievement(def.PlatformName);
            ShowMessage(T("ui.ach.unlocked", ("name", T(def.NameKey))), 6f);
            _achievementsPanel?.Refresh();
        }

        private void CheckAchievements()
        {
            if (_achievements == null) return;
            // Yerelde açık olanlar mağaza tarafında da açık olsun (çevrimdışı açılanlar, yeniden kurulum): bir kez.
            if (!_platformSynced && PlatformServices.Current.Connected)
            {
                _platformSynced = true;
                foreach (var def in _achievements.Defs)
                    if (_achievements.IsUnlocked(def.Id)) PlatformServices.Current.UnlockAchievement(def.PlatformName);
            }
            if (_player == null || _creator != null || _clock == null || _account == null) { SaveAchievements(); return; }

            var snapshot = new AchievementSnapshot
            {
                Counter = _account.Counter,
                Level = _account.Level,
                Generation = _dynasty != null ? _dynasty.CurrentGeneration : 1,
                Years = (int)(_clock.TotalDays / Math.Max(1, _clock.Config.DaysPerYear)),
                Heirlooms = _life != null && _life.Legacy != null ? _life.Legacy.Heirlooms.Count : 0
            };
            _achievements.Evaluate(snapshot);
            SaveAchievements();
        }

        private void ToggleAchievements()
        {
            if (_achievementsPanel == null)
            {
                var host = EnsureUi();
                if (host == null) { ShowMessage(T("ui.ach.unavailable")); return; }
                _achievementsPanel = new Ui.AchievementsPanel(host, AchievementRows,
                    () => T("ui.ach.title", ("done", _achievements.UnlockedCount), ("total", _achievements.Defs.Count)),
                    () => T("ui.ach.close"));
            }
            // Ayarlar açık kalır (zaman durur; MenuOpen): IMGUI ayar penceresi başarım penceresi açıkken çizilmez
            // (IMGUI, UI Toolkit'in üstüne çizildiği için), kapanınca ayarlara geri dönülür.
            _achievementsPanel.Visible = !_achievementsPanel.Visible;
        }

        private IReadOnlyList<Ui.AchievementsPanel.Row> AchievementRows()
        {
            var rows = new List<Ui.AchievementsPanel.Row>();
            if (_achievements == null) return rows;
            foreach (var def in _achievements.Defs)
            {
                bool open = _achievements.IsUnlocked(def.Id);
                bool secret = def.Hidden && !open;
                rows.Add(new Ui.AchievementsPanel.Row
                {
                    Name = secret ? T("ui.ach.hidden_name") : T(def.NameKey),
                    Description = secret ? T("ui.ach.hidden_desc") : T(def.DescKey),
                    Progress = open ? T("ui.ach.done")
                        : def.Target > 1 ? Math.Min(_achievements.Best(def.Id), def.Target) + " / " + def.Target : "",
                    Unlocked = open
                });
            }
            return rows;
        }

        /// <summary>UI Toolkit kökü (HUD ve pencereler ortak kullanır); kurulamazsa null.</summary>
        private Ui.UiHost EnsureUi()
        {
            if (_ui != null) return _ui;
            var host = new Ui.UiHost(transform);
            if (!host.Ready) { host.Destroy(); return null; }
            return _ui = host;
        }

        // ---------------- görüntü ayarları (masaüstü) ----------------

        private const string DisplayModePref = "display.fullscreen", DisplayWidthPref = "display.width",
            DisplayHeightPref = "display.height", DisplayVsyncPref = "display.vsync";

        /// <summary>Görüntü ayarları yalnızca masaüstü oyuncusunda anlamlıdır (mobil ve editör kendi boyutunu kullanır).</summary>
        private static bool DisplaySettingsAvailable => !Application.isMobilePlatform && !Application.isEditor;

        private static void ApplySavedDisplay()
        {
            if (!DisplaySettingsAvailable || !PlayerPrefs.HasKey(DisplayModePref)) return;
            bool full = PlayerPrefs.GetInt(DisplayModePref, 1) == 1;
            int w = PlayerPrefs.GetInt(DisplayWidthPref, Screen.width), h = PlayerPrefs.GetInt(DisplayHeightPref, Screen.height);
            if (w >= DisplayOptions.MinWidth && h >= DisplayOptions.MinHeight)
                Screen.SetResolution(w, h, full ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            QualitySettings.vSyncCount = PlayerPrefs.GetInt(DisplayVsyncPref, 1) == 1 ? 1 : 0;
        }

        private static void SetDisplay(int width, int height, bool fullscreen)
        {
            Screen.SetResolution(width, height, fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            PlayerPrefs.SetInt(DisplayModePref, fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(DisplayWidthPref, width);
            PlayerPrefs.SetInt(DisplayHeightPref, height);
        }

        private static List<(int width, int height)> DisplayChoices()
        {
            var available = new List<(int, int)>();
            var all = Screen.resolutions;
            if (all != null) foreach (var r in all) available.Add((r.width, r.height));
            return DisplayOptions.Choices(available, (Screen.width, Screen.height));
        }

        /// <summary>Ayarlar ekranının görüntü satırları (var olan IMGUI ayar penceresine ek).</summary>
        private void DrawDisplaySettings()
        {
            if (!DisplaySettingsAvailable) return;
            bool full = Screen.fullScreenMode != FullScreenMode.Windowed;
            var current = (Screen.width, Screen.height);

            GUILayout.BeginHorizontal();
            GUILayout.Label(T("ui.settings.display"), GUILayout.Width(110f));
            if (GUILayout.Toggle(full, T("ui.settings.fullscreen"), GUI.skin.button, GUILayout.Width(110f)) && !full) SetDisplay(current.width, current.height, true);
            if (GUILayout.Toggle(!full, T("ui.settings.windowed"), GUI.skin.button, GUILayout.Width(110f)) && full) SetDisplay(current.width, current.height, false);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(T("ui.settings.resolution"), GUILayout.Width(110f));
            var choices = DisplayChoices();
            if (GUILayout.Button("<", GUILayout.Width(30f)))
            {
                var next = DisplayOptions.Step(choices, current, -1);
                if (next != current) SetDisplay(next.width, next.height, full);
            }
            GUILayout.Label(current.width + " × " + current.height, GUILayout.Width(110f));
            if (GUILayout.Button(">", GUILayout.Width(30f)))
            {
                var next = DisplayOptions.Step(choices, current, +1);
                if (next != current) SetDisplay(next.width, next.height, full);
            }
            bool vsync = QualitySettings.vSyncCount > 0;
            bool wantVsync = GUILayout.Toggle(vsync, T("ui.settings.vsync"), GUI.skin.button, GUILayout.Width(90f));
            if (wantVsync != vsync)
            {
                QualitySettings.vSyncCount = wantVsync ? 1 : 0;
                PlayerPrefs.SetInt(DisplayVsyncPref, wantVsync ? 1 : 0);
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>Ayarlar ekranının alt satırları: başarımlar, hata raporu, oyun içindeyken ana menü ve çıkış.</summary>
        private void DrawSystemSettings()
        {
            GUILayout.BeginHorizontal();
            if (_achievements != null && GUILayout.Button(T("ui.ach.button", ("done", _achievements.UnlockedCount), ("total", _achievements.Defs.Count)), GUILayout.Height(26f)))
                ToggleAchievements();
            if (GUILayout.Button(T("ui.error.copy"), GUILayout.Height(26f)))
            {
                GUIUtility.systemCopyBuffer = ErrorReporter.BuildReport();
                ShowMessage(T("ui.error.copied"), 4f);
            }
            if (!Application.isMobilePlatform && GUILayout.Button(T("ui.error.open_folder"), GUILayout.Height(26f)))
                Application.OpenURL("file://" + ErrorReporter.LogFolder);
            GUILayout.EndHorizontal();

            if (ErrorReporter.Total > 0)
                GUILayout.Label(T("ui.error.count", ("count", ErrorReporter.Total), ("distinct", ErrorReporter.Distinct)));

            bool inGame = !_title && _creator == null && _player != null;
            if (inGame)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(T("ui.hud.save"), GUILayout.Height(26f))) SaveNow(quiet: false);
                if (GUILayout.Button(T("ui.menu.to_title"), GUILayout.Height(26f))) { _settingsOpen = false; ReturnToTitle(); }
                if (!Application.isMobilePlatform && !Application.isEditor && GUILayout.Button(T("ui.menu.quit"), GUILayout.Height(26f)))
                {
                    SaveNow(quiet: true);
                    Application.Quit();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Label(VersionLabel);
        }

        private static string VersionLabel => "Thatchwick " + Application.version;
    }
}
