using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Game
{
    /// <summary>
    /// Geçici oynanabilir test sahnesi: ızgara dünya, kamera, eşyalar, oyuncu karakteri
    /// (dokununca yürür ya da eşyayı kullanır), otonom bir NPC ve oyun saati HUD'u.
    /// Sahneyi koddan kurar; GameBootstrap oyun başlayınca bunu otomatik oluşturur.
    /// </summary>
    public sealed class GameRoot : MonoBehaviour
    {
        private const int MapWidth = 48;
        private const int MapHeight = 32;
        private const float MaxFrameSeconds = 0.1f;

        private sealed class Actor
        {
            public Character Character;
            public JobRunner Runner;
            public Transform View;
            public SpriteRenderer Renderer;
        }

        private WorldState _world;
        private WorldView _view;
        private WorldInput _input;
        private GameClock _clock;
        private ContentDatabase _content;
        private readonly List<Actor> _actors = new List<Actor>();
        private readonly Dictionary<int, Transform> _itemViews = new Dictionary<int, Transform>();
        private Actor _player;
        private string _lastFailure;
        private float _messageUntil;
        private string _message = "";
        private GUIStyle _labelStyle;
        private readonly System.Random _rng = new System.Random(11);

        private float HudScale => Mathf.Max(1f, Screen.height / 540f);
        private Rect HudRect => new Rect(8f, 8f, 330f, 230f);

        private void Awake()
        {
            var map = BuildDemoMap();
            _world = new WorldState(map);
            _clock = new GameClock();

            _content = ContentLoader.Load(out var contentErrors);
            foreach (var e in contentErrors) Debug.LogError($"İçerik hatası: {e}");
            Debug.Log($"İçerik yüklendi: {_content.Items.Count} eşya tanımı.");

            PlaceDemoItems();

            var viewGo = new GameObject("WorldView");
            viewGo.transform.SetParent(transform, false);
            _view = viewGo.AddComponent<WorldView>();
            _view.Build(map);

            CreateItemViews();

            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = camGo.AddComponent<Camera>();
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.11f);

            _input = gameObject.AddComponent<WorldInput>();
            _input.Init(cam, Iso.MapBounds(MapWidth, MapHeight));
            _input.IsOverUi = IsOverHud;
            _input.Tapped += OnTapped;

            _player = CreateActor(1, "Aldric", new GridPos(24, 10), true, new Color(0.95f, 0.62f, 0.2f));
            CreateActor(2, "Maud", new GridPos(28, 9), false, new Color(0.9f, 0.5f, 0.75f));
        }

        // Test haritası: gölet (dikdörtgen), bir çit hattı ve dağınık engeller.
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
                map.SetBlocked(new GridPos(rng.Next(MapWidth), rng.Next(MapHeight)), true);

            return map;
        }

        // Başlangıç alanını (oyuncu, NPC, eşyalar) açık bırakır ve eşyaları yerleştirir.
        private void PlaceDemoItems()
        {
            for (int x = 22; x <= 31; x++)
                for (int y = 8; y <= 16; y++)
                    _world.Map.SetBlocked(new GridPos(x, y), false);

            TryPlace("item.bed_straw", new GridPos(26, 13));
            TryPlace("item.hearth_stone", new GridPos(28, 13));
            TryPlace("item.cooking_pot", new GridPos(30, 13));
            TryPlace("item.stool_wood", new GridPos(26, 15));
        }

        private void TryPlace(string id, GridPos pos)
        {
            if (!_content.Items.TryGet(id, out var def))
            {
                Debug.LogError($"Eşya tanımı yok: {id}");
                return;
            }
            _world.PlaceItem(def, pos);
        }

        private static Color ItemColor(string id)
        {
            switch (id)
            {
                case "item.bed_straw": return new Color(0.75f, 0.6f, 0.35f);
                case "item.hearth_stone": return new Color(0.85f, 0.35f, 0.2f);
                case "item.cooking_pot": return new Color(0.55f, 0.55f, 0.6f);
                default: return new Color(0.6f, 0.45f, 0.3f);
            }
        }

        private void CreateItemViews()
        {
            foreach (var item in _world.Items)
            {
                var go = new GameObject(item.Def.Id);
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteMaterials.Cube(ItemColor(item.Def.Id));
                sr.sharedMaterial = SpriteMaterials.Unlit;
                var pos = _view.TileToWorld(item.Position);
                go.transform.position = pos;
                sr.sortingOrder = Iso.SortOrder(pos.y);
                _itemViews[item.Id] = go.transform;
            }
        }

        private Actor CreateActor(int id, string name, GridPos start, bool player, Color color)
        {
            var character = new Character(id, name, start, player);
            character.SetAge(36); // Test: yetişkin başlar
            character.LifespanDays = character.AgeConfig.RollLifespan(_rng);
            character.StageChanged += (c, from, to) => ShowMessage($"{c.Name}: {StageName(to)} oldu");
            character.Died += c => ShowMessage($"{c.Name} hayatını kaybetti");
            character.Skills.LevelChanged += (skill, from, to) => ShowMessage($"{character.Name}: {SkillName(skill)} seviye {to}");
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteMaterials.Pillar(color);
            sr.sharedMaterial = SpriteMaterials.Unlit;
            go.transform.position = _view.TileToWorld(start);

            var actor = new Actor
            {
                Character = character,
                Runner = new JobRunner(character, _world),
                View = go.transform,
                Renderer = sr
            };
            _actors.Add(actor);
            return actor;
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
            if (cam == null || _player == null) return;

            var world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            var tile = _view.WorldToTile(world);

            ItemInstance tapped = null;
            foreach (var item in _world.Items)
            {
                if (item.Position == tile) { tapped = item; break; }
            }

            if (tapped != null && tapped.Def.Interactions.Count > 0)
            {
                var interaction = tapped.Def.Interactions[0];
                _player.Runner.Command(Job.UseItem(tapped.Id, interaction.Id));
                ShowMessage($"{interaction.DisplayName}: {tapped.Def.DisplayName}");
            }
            else
            {
                _player.Runner.Command(Job.MoveTo(tile));
            }
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

            // Biyoloji ve iş ilerlemesi oyun saatinde işler; hız çarpanı GameClock'ta uygulanır.
            double gameDays = _clock.Advance(dt);
            foreach (var actor in _actors)
            {
                actor.Character.Tick(gameDays);
                if (actor.Character.IsDead)
                {
                    actor.Renderer.color = new Color(1f, 1f, 1f, 0.35f);
                    continue;
                }
                actor.Runner.Tick(gameDays);
                UpdateView(actor);
            }

            var failure = _player.Runner.LastFailure;
            if (failure != _lastFailure)
            {
                _lastFailure = failure;
                if (failure != null) ShowMessage(failure);
            }
        }

        // Karakteri bulunduğu karodan sonraki karoya doğru ara değerler (düzgün yürüme).
        private void UpdateView(Actor actor)
        {
            Vector3 pos = _view.TileToWorld(actor.Character.Position);
            var next = actor.Runner.NextTile;
            if (next.HasValue)
                pos = Vector3.Lerp(pos, _view.TileToWorld(next.Value), actor.Runner.StepProgress);
            actor.View.position = pos;
            // Ekranda daha aşağıdaki (öndeki) nesne üste çizilir; aynı karoda karakter eşyanın önünde.
            actor.Renderer.sortingOrder = Iso.SortOrder(pos.y, 1);
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

        private static string SkillName(SkillType s)
        {
            switch (s)
            {
                case SkillType.Farming: return "Çiftçilik";
                case SkillType.Woodcutting: return "Odunculuk";
                case SkillType.Cooking: return "Yemek";
                default: return "Marangozluk";
            }
        }

        private static string SkillsText(Character c)
        {
            var parts = new System.Text.StringBuilder();
            foreach (SkillType t in System.Enum.GetValues(typeof(SkillType)))
            {
                int level = c.Skills.GetLevel(t);
                if (level <= 1 && c.Skills.GetXp(t) <= 0) continue;
                if (parts.Length > 0) parts.Append(" · ");
                parts.Append($"{SkillName(t)} {level}");
            }
            return parts.Length == 0 ? "" : parts.ToString();
        }

        private static string StageName(AgeStage s)
        {
            switch (s)
            {
                case AgeStage.Baby: return "Bebek";
                case AgeStage.Child: return "Çocuk";
                case AgeStage.Youth: return "Genç";
                case AgeStage.Adult: return "Yetişkin";
                default: return "Yaşlı";
            }
        }

        private static string ActivityText(JobRunner runner)
        {
            if (runner.IsIdle) return "Boşta";
            return runner.IsUsingItem ? "Kullanıyor" : "Yürüyor";
        }

        private void OnGUI()
        {
            if (_clock == null) return;

            float s = HudScale;
            GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));

            DrawItemLabels(s);

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

            foreach (var actor in _actors)
            {
                var c = actor.Character;
                GUILayout.Label(
                    $"{c.Name} ({(c.IsDead ? "Öldü" : StageName(c.Stage))}, {c.AgeDays:0}g): Açlık {c.Needs.Get(NeedType.Hunger):0} · Enerji {c.Needs.Get(NeedType.Energy):0}");
                if (!c.IsDead) GUILayout.Label($"   {ActivityText(actor.Runner)}");
                var skills = SkillsText(c);
                if (skills.Length > 0) GUILayout.Label($"   Beceri: {skills}");
            }

            GUILayout.Label($"Eşya tanımı: {_content.Items.Count}");
            if (Time.unscaledTime < _messageUntil) GUILayout.Label(_message);
            GUILayout.EndArea();
        }

        // Eşya adları, dünyada eşyanın üstünde küçük etiket olarak (test için).
        private void DrawItemLabels(float guiScale)
        {
            var cam = Camera.main;
            if (cam == null) return;

            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 11
                };
            }

            foreach (var item in _world.Items)
            {
                Vector3 sp = cam.WorldToScreenPoint(_view.TileToWorld(item.Position) + new Vector3(0f, 1.5f, 0f));
                if (sp.z < 0f) continue;
                float x = sp.x / guiScale;
                float y = (Screen.height - sp.y) / guiScale;
                GUI.Label(new Rect(x - 60f, y - 10f, 120f, 20f), item.Def.DisplayName, _labelStyle);
            }
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
