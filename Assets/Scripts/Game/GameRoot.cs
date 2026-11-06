using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Game
{
    /// <summary>
    /// Geçici oynanabilir test sahnesi: ızgara dünya, kamera, tek bir karakter
    /// (dokunulan karoya A* ile yürür) ve oyun saati HUD'u. Sahneyi koddan kurar;
    /// GameBootstrap oyun başlayınca bunu otomatik oluşturur.
    /// </summary>
    public sealed class GameRoot : MonoBehaviour
    {
        private const int MapWidth = 48;
        private const int MapHeight = 32;
        private const float WalkTilesPerSecond = 3f;
        private const float MaxFrameSeconds = 0.1f;

        private GridMap _map;
        private WorldView _view;
        private WorldInput _input;
        private GameClock _clock;
        private ContentDatabase _content;
        private Character _player;
        private Transform _walker;
        private readonly List<GridPos> _path = new List<GridPos>();
        private int _pathIndex;
        private float _messageUntil;
        private string _message = "";

        private float HudScale => Mathf.Max(1f, Screen.height / 540f);
        private Rect HudRect => new Rect(8f, 8f, 250f, 175f);

        private void Awake()
        {
            _map = BuildDemoMap();
            _clock = new GameClock();

            _content = ContentLoader.Load(out var contentErrors);
            foreach (var e in contentErrors) Debug.LogError($"İçerik hatası: {e}");
            Debug.Log($"İçerik yüklendi: {_content.Items.Count} eşya tanımı.");

            var viewGo = new GameObject("WorldView");
            viewGo.transform.SetParent(transform, false);
            _view = viewGo.AddComponent<WorldView>();
            _view.Build(_map);

            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = camGo.AddComponent<Camera>();
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.11f);

            _input = gameObject.AddComponent<WorldInput>();
            _input.Init(cam, new Vector2(MapWidth, MapHeight));
            _input.IsOverUi = IsOverHud;
            _input.Tapped += OnTapped;

            var start = new GridPos(MapWidth / 2, MapHeight / 2 - 6);
            _player = new Character(1, "Aldric", start, isPlayerControlled: true);

            var walkerGo = new GameObject("Walker");
            walkerGo.transform.SetParent(transform, false);
            var sr = walkerGo.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteMaterials.WhiteSquare;
            sr.sharedMaterial = SpriteMaterials.Unlit;
            sr.color = new Color(0.95f, 0.62f, 0.2f);
            sr.sortingOrder = 2;
            walkerGo.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            _walker = walkerGo.transform;
            _walker.position = _view.TileToWorld(start);
        }

        // Test haritası: gölet (dikdörtgen) ve dağınık engeller.
        private static GridMap BuildDemoMap()
        {
            var map = new GridMap(MapWidth, MapHeight);
            for (int x = 10; x < 22; x++)
                for (int y = 12; y < 20; y++)
                    map.SetBlocked(new GridPos(x, y), true);

            for (int y = 4; y < 26; y++)
                if (y != 15) map.SetBlocked(new GridPos(32, y), true); // (32,15) bir geçit

            var rng = new System.Random(7);
            for (int i = 0; i < 60; i++)
            {
                var p = new GridPos(rng.Next(MapWidth), rng.Next(MapHeight));
                if (p.X == MapWidth / 2 && p.Y == MapHeight / 2 - 6) continue;
                map.SetBlocked(p, true);
            }
            return map;
        }

        private bool IsOverHud(Vector2 screenPos)
        {
            float s = HudScale;
            var guiPos = new Vector2(screenPos.x / s, (Screen.height - screenPos.y) / s);
            return HudRect.Contains(guiPos);
        }

        private void OnTapped(Vector2 screenPos)
        {
            var cam = Camera.main;
            if (cam == null) return;

            var world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            var target = _view.WorldToTile(world);

            var path = Pathfinder.FindPath(_map, _player.Position, target);
            if (path == null)
            {
                ShowMessage("Oraya ulaşılamıyor");
                return;
            }

            _path.Clear();
            _path.AddRange(path);
            _pathIndex = 1;
        }

        // Play modundayken kod yeniden derlenirse (ör. git pull) private alanlar sıfırlanır
        // ama sahne çalışmaya devam eder. Bu durumda kendimizi temiz bir kopyayla değiştiririz.
        private bool RecoverIfLost()
        {
            if (_clock != null) return false;
            enabled = false;
            new GameObject("GameRoot").AddComponent<GameRoot>();
            Destroy(gameObject);
            return true;
        }

        private void Update()
        {
            if (RecoverIfLost()) return;

            float dt = Mathf.Min(Time.deltaTime, MaxFrameSeconds);

            // Biyoloji oyun saatinde işler; hız çarpanı GameClock'ta uygulanır.
            double gameDays = _clock.Advance(dt);
            _player.Tick(gameDays);

            Walk(dt * _clock.EffectiveSpeed);
        }

        private void Walk(float scaledSeconds)
        {
            if (_pathIndex >= _path.Count) return;

            float budget = WalkTilesPerSecond * scaledSeconds;
            while (budget > 0f && _pathIndex < _path.Count)
            {
                Vector3 target = _view.TileToWorld(_path[_pathIndex]);
                Vector3 pos = _walker.position;
                float dist = Vector3.Distance(pos, target);

                if (dist <= budget)
                {
                    _walker.position = target;
                    _player.Position = _path[_pathIndex];
                    budget -= dist;
                    _pathIndex++;
                }
                else
                {
                    _walker.position = Vector3.MoveTowards(pos, target, budget);
                    budget = 0f;
                }
            }
        }

        private void ShowMessage(string text)
        {
            _message = text;
            _messageUntil = Time.unscaledTime + 1.5f;
        }

        private static string SeasonName(Season s)
        {
            switch (s)
            {
                case Season.Spring: return "İlkbahar";
                case Season.Summer: return "Yaz";
                case Season.Autumn: return "Sonbahar";
                default: return "Kış";
            }
        }

        private void OnGUI()
        {
            if (_clock == null) return;

            float s = HudScale;
            GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));

            GUILayout.BeginArea(HudRect, GUI.skin.box);
            GUILayout.Label(
                $"Yıl {_clock.YearIndex + 1} · {SeasonName(_clock.Season)} · Gün {_clock.DayOfSeason + 1}/{_clock.Config.DaysPerSeason}");

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Hız {_clock.RequestedSpeed}x", GUILayout.Width(60f));
            foreach (int speed in GameClock.AllowedSpeeds)
            {
                if (GUILayout.Button($"{speed}x")) _clock.SetSpeed(speed);
            }
            GUILayout.EndHorizontal();

            GUILayout.Label($"Açlık  {_player.Needs.Get(NeedType.Hunger):0}");
            GUILayout.Label($"Enerji {_player.Needs.Get(NeedType.Energy):0}");
            GUILayout.Label($"Eşya tanımı: {_content.Items.Count}");
            if (Time.unscaledTime < _messageUntil) GUILayout.Label(_message);
            GUILayout.EndArea();
        }
    }

    internal static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (Object.FindFirstObjectByType<GameRoot>() != null) return;
            new GameObject("GameRoot").AddComponent<GameRoot>();
        }
    }
}
