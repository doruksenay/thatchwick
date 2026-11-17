using UnityEngine;
using Thatchwick.Core.Localization;

namespace Thatchwick.Game
{
    /// <summary>
    /// Geçici dil seçici: ekranın sol altında küçük bir düğme. Ayarlar ekranı gelince (yol haritası Adım 27)
    /// oraya taşınacak. Sahneye elle eklenmez; oyun açılırken kendini kurar (GameRoot'a dokunmadan).
    /// </summary>
    public sealed class LanguageMenu : MonoBehaviour
    {
        private bool _open;
        private bool _restartNote;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindAnyObjectByType<LanguageMenu>() != null) return;
            var go = new GameObject("LanguageMenu");
            DontDestroyOnLoad(go);
            go.AddComponent<LanguageMenu>();
        }

        private void OnGUI()
        {
            var loc = GameLocalization.Current;
            const float w = 120f, h = 24f, pad = 6f;
            float x = pad, y = Screen.height - h - pad;

            if (_open)
            {
                var langs = Languages.Released;
                float listH = langs.Length * (h + 2f);
                GUI.Box(new Rect(x, y - listH - 4f, w, listH + 2f), GUIContent.none);
                for (int i = 0; i < langs.Length; i++)
                {
                    var r = new Rect(x + 2f, y - listH - 2f + i * (h + 2f), w - 4f, h);
                    string label = (langs[i] == loc.Language ? "• " : "  ") + Languages.NativeName(langs[i]);
                    if (GUI.Button(r, label))
                    {
                        if (langs[i] != loc.Language)
                        {
                            GameLocalization.SetLanguage(langs[i]);
                            _restartNote = true;
                        }
                        _open = false;
                    }
                }
            }

            string current = loc.Get("ui.language.button") + ": " + Languages.NativeName(loc.Language);
            if (GUI.Button(new Rect(x, y, w + 40f, h), current)) _open = !_open;

            if (_restartNote)
            {
                // Dil değişti: içerik adları (eşya, tarif ...) oyun yeniden açılınca değişir.
                var note = GameLocalization.Current.Get("ui.language.restart_note");
                var r = new Rect(x + w + 46f, y, 380f, h);
                GUI.Box(r, note);
                if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition)) _restartNote = false;
            }
        }
    }
}
