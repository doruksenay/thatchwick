using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Customization;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Heat;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Progress;
using Thatchwick.Core.Save;
using Thatchwick.Core.Time;
using Thatchwick.Core.Work;
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
        private Actor _maud;
        private string _lastFailure;
        private float _messageUntil;
        private string _message = "";
        private GUIStyle _labelStyle;
        private readonly System.Random _rng = new System.Random(11);
        private Population _population;
        private AppearanceCatalog _appearanceCatalog;
        private CharacterCreator _creator;
        private Household _household;
        private BuildMode _buildMode;
        private WallView _walls;
        private HeatSystem _heat;
        private PlantSystem _plants;
        private InteractionEffects _effects;
        private StorageService _storage;
        private Market _market;
        private AccountProgress _account;
        private MarketPanel _marketPanel;
        private int _stallId;
        private readonly List<(ItemInstance item, Transform view)> _plantViews = new List<(ItemInstance, Transform)>();
        private Vector2 _hudScroll;
        private static readonly string[] GirlNames = { "Edith", "Agnes", "Matilda", "Joan" };
        private static readonly string[] BoyNames = { "Wat", "Hob", "Piers", "Osric" };
        private int _nameCounter;

        /// <summary>Yeniden kurulan GameRoot bu kaydı yükler (Yükle düğmesi / Devam et).</summary>
        public static SaveData PendingLoad;
        private long _lastSavedDay = -1;
        private static string SavePath => System.IO.Path.Combine(Application.persistentDataPath, "thatchwick_save.json");
        public static bool HasSave => System.IO.File.Exists(SavePath);

        private float HudScale => Mathf.Max(1f, Screen.height / 540f);
        private Rect HudRect => new Rect(8f, 8f, 340f, Mathf.Min(560f, Screen.height / HudScale - 16f));

        private void Awake()
        {
            _content = ContentLoader.Load(out var contentErrors);
            foreach (var e in contentErrors) Debug.LogError($"İçerik hatası: {e}");
            Debug.Log($"İçerik yüklendi: {_content.Items.Count} eşya tanımı.");

            LoadedGame loaded = null;
            if (PendingLoad != null)
            {
                try { loaded = SaveGame.Load(PendingLoad, _content); }
                catch (System.Exception ex) { Debug.LogError($"Kayıt yüklenemedi: {ex.Message}"); }
                PendingLoad = null;
            }

            GridMap map;
            if (loaded != null)
            {
                foreach (var w in loaded.Warnings) Debug.LogWarning($"Kayıt: {w}");
                _world = loaded.World;
                map = _world.Map;
                _clock = loaded.Clock;
                _lastSavedDay = _clock.DayIndex;
                foreach (var item in _world.Items) if (item.Def.Id == "item.market_stall") _stallId = item.Id;
            }
            else
            {
                map = BuildDemoMap();
                _world = new WorldState(map);
                _clock = new GameClock();
                PlaceDemoItems();
                _world.Map.RestrictOwnership(20, 8, 31, 15); // başlangıç arsası: 12x8, genişletilebilir
            }

            var viewGo = new GameObject("WorldView");
            viewGo.transform.SetParent(transform, false);
            _view = viewGo.AddComponent<WorldView>();
            _view.Build(map);

            RebuildItemViews();
            _world.Changed += RebuildItemViews;

            foreach (var item in _world.Items)
                foreach (var tile in item.OccupiedTiles()) _view.ClearTerrainBlock(tile);
            _view.RefreshFloors();
            map.Changed += _view.RefreshFloors;
            map.Changed += () =>
            {
                if (_account == null || _account.HasFirst("first:room")) return;
                foreach (var _ in map.Rooms.IndoorRooms) { _account.Record("room", _clock.DayIndex, isWork: false); break; }
            };
            var wallsGo = new GameObject("WallView");
            wallsGo.transform.SetParent(transform, false);
            _walls = wallsGo.AddComponent<WallView>();
            _walls.Init(map);

            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = camGo.AddComponent<Camera>();
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.11f);

            _input = gameObject.AddComponent<WorldInput>();
            _input.Init(cam, Iso.MapBounds(map.Width, map.Height));
            _input.IsOverUi = IsOverHud;
            _input.Tapped += OnTapped;

            _household = loaded != null ? loaded.Household : new Household(100);
            _account = loaded != null ? loaded.Account : new AccountProgress();
            _account.LevelChanged += (from, to) => ShowMessage($"Hesap seviyesi {to}! Dükkânda yeni eşyalar açılmış olabilir.");
            _account.BadgeEarned += b => ShowMessage($"Rozet kazanıldı: {b.DisplayName} (+{b.RewardXp} XP)");
            if (loaded == null)
            {
                _household.AddItem("item.firewood", 10);
                _household.AddItem("item.sapling_oak", 2);
                _household.AddItem("item.turnip", 12);
                _household.AddItem("item.seed_turnip", 6);
            }
            _storage = new StorageService(_world, _household, _content);
            _effects = new InteractionEffects(_world, _household, _content, seed: 9) { Storage = _storage };
            _effects.Applied += (c, i, summary) => { ShowMessage($"{c.Name}: {summary}"); _account.Record(i.Id, _clock.DayIndex); };
            _plants = new PlantSystem(_world);
            _market = new Market(_content, _household, _storage, null, seed: 17);
            if (loaded != null) _market.RestoreState(loaded.MarketDemand, _clock.TotalDays, _clock.Season, loaded.MarketTakenOffers);
            else _market.Tick(0, _clock.TotalDays, _clock.Season);
            _market.OfferAccepted += o => _account.Record(o.IsContract ? "contract" : "barter", _clock.DayIndex);
            _marketPanel = new MarketPanel(_market, _household, _storage, _content, _world,
                () => _player?.Character, () => _player?.Runner, _stallId);
            _plants.PlantDied += (def, pos) => ShowMessage($"{def.DisplayName} kışın dondu");
            _heat = new HeatSystem(_world, _clock.Config, null, _clock.TotalDays);
            if (loaded != null) _heat.RestoreTemperatures(loaded.Temperatures);
            _buildMode = new BuildMode(_world, _household, new BuildService(_world, _household, _content) { Storage = _storage, AccountLevel = () => _account.Level }, _content, _view, _input, transform, _walls);
            _buildMode.Activity += a => _account.Record(a, _clock.DayIndex);

            _population = new Population(firstFreeId: 1, seed: 21);
            _population.NameGenerator = sex => (sex == Sex.Female ? GirlNames : BoyNames)[_nameCounter++ % 4];
            _population.BabyBorn += OnBabyBorn;

            _appearanceCatalog = ContentLoader.LoadAppearance(out var appearanceErrors);
            foreach (var e in appearanceErrors) Debug.LogError($"Görünüm hatası: {e}");
            _population.AppearanceCatalog = _appearanceCatalog;

            if (loaded != null)
            {
                foreach (var c in loaded.Characters)
                {
                    _population.Add(c);
                    var actor = CreateActorFor(c);
                    if (c.IsPlayerControlled && _player == null) _player = actor;
                    if (c.Name == "Maud" && _maud == null) _maud = actor;
                }
                _input.enabled = true;
                ShowMessage(loaded.Warnings.Count > 0 ? $"Kayıt yüklendi ({loaded.Warnings.Count} uyarı, konsola bak)" : "Kayıt yüklendi");
                return;
            }

            // Maud (NPC) rastgele görünümle hemen oluşur; oyuncunun karakteri yaratma ekranından gelir.
            var maud = new Character(2, "Maud", new GridPos(28, 9), false)
            {
                Sex = Sex.Female,
                Appearance = _appearanceCatalog.Randomize(_rng, Sex.Female)
            };
            maud.SetAge(36);
            maud.LifespanDays = maud.AgeConfig.RollLifespan(_rng);
            _population.Add(maud);
            _maud = CreateActorFor(maud);

            _creator = new CharacterCreator(_appearanceCatalog, seed: 5);
            _input.enabled = false;
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
            TryPlace("item.chopping_block", new GridPos(22, 15));
            TryPlace("item.chest_wood", new GridPos(24, 15));

            // Köy pazarı: çit geçidinin (32,15) doğusunda.
            _world.Map.SetBlocked(new GridPos(34, 14), false);
            _world.Map.SetBlocked(new GridPos(35, 14), false);
            _world.Map.SetBlocked(new GridPos(33, 15), false);
            _world.Map.SetBlocked(new GridPos(34, 15), false);
            if (_content.Items.TryGet("item.market_stall", out var stallDef))
                _stallId = _world.PlaceItem(stallDef, new GridPos(34, 14)).Id;

            // Arsanın dışındaki yabani meşeler (olgun): herkes kesebilir, fidanlarıyla arsaya ağaç dikilir.
            var rng = new System.Random(13);
            int placed = 0;
            for (int i = 0; i < 200 && placed < 14; i++)
            {
                var p = new GridPos(rng.Next(33, 44), rng.Next(4, 26));
                if (_world.Map.IsBlocked(p) || (p.Y == 15 && p.X <= 35) || !_content.Items.TryGet("item.tree_oak", out var oak)) continue;
                PlantSystem.SetGrown(_world.PlaceItem(oak, p));
                placed++;
            }
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
                case "item.crop_turnip": return new Color(0.45f, 0.7f, 0.35f);
                case "item.market_stall": return new Color(0.75f, 0.35f, 0.3f);
                case "item.chopping_block": return new Color(0.48f, 0.38f, 0.28f);
                case "item.table_wood": return new Color(0.62f, 0.42f, 0.24f);
                case "item.chair_wood": return new Color(0.7f, 0.5f, 0.3f);
                case "item.chest_wood": return new Color(0.5f, 0.33f, 0.2f);
                case "item.shelf_wood": return new Color(0.66f, 0.5f, 0.34f);
                case "item.candle": return new Color(1f, 0.95f, 0.7f);
                case "item.flower_pot": return new Color(0.85f, 0.4f, 0.5f);
                case "item.clay_cup": return new Color(0.8f, 0.5f, 0.35f);
                default: return new Color(0.6f, 0.45f, 0.3f);
            }
        }

        private readonly List<GameObject> _itemObjects = new List<GameObject>();

        // Eşya görünümlerini dünyadan yeniden kurar (yerleştirme/taşıma/satış sonrası). Yer tutucu küpler:
        // çok karolu eşya karo başına bir küp, dekor küçük küp olarak yuvasında durur.
        private void RebuildItemViews()
        {
            foreach (var go in _itemObjects) if (go != null) Destroy(go);
            _itemObjects.Clear();
            _itemViews.Clear();
            _plantViews.Clear();

            foreach (var item in _world.Items)
            {
                var color = ItemColor(item.Def.Id);
                if (item.IsDecor)
                {
                    var parent = _world.GetItem(item.ParentId.Value);
                    if (parent == null || !parent.TryGetSlotPosition(item.SlotId, out float gx, out float gy)) continue;
                    var pos = Iso.GridToWorld(gx, gy) + new Vector3(0f, 24f / 32f, 0f);
                    AddItemSprite(item, pos, color, 0.35f, Iso.SortOrder(Iso.GridToWorld(gx, gy).y) + 2);
                    continue;
                }
                if (item.IsPlant)
                {
                    var pos = _view.TileToWorld(item.Position);
                    AddItemSprite(item, pos, color, PlantScale(item), Iso.SortOrder(pos.y, 1), SpriteMaterials.Tree);
                    _plantViews.Add((item, _itemObjects[_itemObjects.Count - 1].transform));
                    continue;
                }
                foreach (var tile in item.OccupiedTiles())
                {
                    var pos = _view.TileToWorld(tile);
                    AddItemSprite(item, pos, color, 1f, Iso.SortOrder(pos.y));
                }
            }
        }

        private static float PlantScale(ItemInstance item) => 0.3f + 0.7f * item.GrowthFraction;

        private void AddItemSprite(ItemInstance item, Vector3 pos, Color color, float scale, int order, Sprite sprite = null)
        {
            var go = new GameObject(item.Def.Id);
            go.transform.SetParent(transform, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite != null ? sprite : SpriteMaterials.Cube(color);
            sr.sharedMaterial = SpriteMaterials.Unlit;
            sr.sortingOrder = order;
            _itemObjects.Add(go);
            if (!_itemViews.ContainsKey(item.Id)) _itemViews[item.Id] = go.transform;
        }

        private Actor CreateActorFor(Character character)
        {
            character.CanStarve = true;
            character.StageChanged += (c, from, to) => ShowMessage($"{c.Name}: {StageName(to)} oldu");
            character.Died += c => ShowMessage(c.CauseOfDeath == DeathCause.Cold ? $"{c.Name} donarak öldü"
                : c.CauseOfDeath == DeathCause.Starvation ? $"{c.Name} açlıktan öldü" : $"{c.Name} hayatını kaybetti");
            character.Skills.LevelChanged += (skill, from, to) => { ShowMessage($"{character.Name}: {SkillName(skill)} seviye {to}"); if (to > from) _account.OnSkillLevelUp(); };

            var go = new GameObject(character.Name);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PersonSprites.Get(character.Appearance);
            sr.sharedMaterial = SpriteMaterials.Unlit;
            go.transform.position = _view.TileToWorld(character.Position);

            var actor = new Actor
            {
                Character = character,
                Runner = new JobRunner(character, _world) { Effects = _effects },
                View = go.transform,
                Renderer = sr
            };
            _actors.Add(actor);
            return actor;
        }

        // Yeni doğan bebek: sahneye küçük bir karakter olarak eklenir (bebek evresinde yetişkin boyunun yarısı).
        private void OnBabyBorn(Character baby, Character mother, int? fatherId)
        {
            CreateActorFor(baby);
            ShowMessage($"{mother.Name} doğurdu: {baby.Name}");
        }

        private bool IsOverHud(Vector2 screenPos)
        {
            float s = HudScale;
            var guiPos = new Vector2(screenPos.x / s, (Screen.height - screenPos.y) / s);
            if (_buildMode != null && _buildMode.Active && _buildMode.PanelRect(s).Contains(guiPos)) return true;
            if (_marketPanel != null && _marketPanel.Active && _marketPanel.PanelRect(s).Contains(guiPos)) return true;
            return HudRect.Contains(guiPos);
        }

        // ---- kayıt ----

        private void SaveNow(bool quiet)
        {
            if (_player == null || _creator != null) return;   // oyun başlamadan kaydetme
            try
            {
                var data = SaveGame.Capture(_clock, _world, _household, _population.Members, _account, _heat, _market);
                System.IO.File.WriteAllText(SavePath, JsonUtility.ToJson(data));
                if (!quiet) ShowMessage("Kaydedildi");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Kaydetme hatası: {ex.Message}");
                if (!quiet) ShowMessage("Kaydedilemedi (konsola bak)");
            }
        }

        private void LoadSave()
        {
            try
            {
                PendingLoad = JsonUtility.FromJson<SaveData>(System.IO.File.ReadAllText(SavePath));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Kayıt okunamadı: {ex.Message}");
                ShowMessage("Kayıt okunamadı");
                return;
            }
            enabled = false;
            new GameObject("GameRoot").AddComponent<GameRoot>();
            Destroy(gameObject);
        }

        private void OnApplicationQuit() => SaveNow(quiet: true);
        private void OnApplicationPause(bool paused) { if (paused) SaveNow(quiet: true); }

        // Yapı modu ve pazar paneli aynı yeri kullanır: biri açılınca diğeri kapanır.
        private void ToggleBuild()
        {
            if (_marketPanel.Active) _marketPanel.Close();
            _buildMode.Toggle();
        }

        private void ToggleMarket()
        {
            if (_buildMode.Active) _buildMode.Toggle();
            _marketPanel.Toggle();
        }

        private void FinishCreation()
        {
            var character = _creator.Draft.CreateCharacter(1, new GridPos(24, 10), true, _appearanceCatalog, _rng);
            _creator.Release();
            _creator = null;
            _population.Add(character);
            _player = CreateActorFor(character);
            _input.enabled = true;
            ShowMessage($"{character.Name} köye geldi");
        }

        private void OnTapped(Vector2 screenPos)
        {
            var cam = Camera.main;
            if (cam == null || _player == null) return;

            var world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            var tile = _view.WorldToTile(world);

            if (_buildMode.Active) { _buildMode.OnTap(world); return; }

            ItemInstance tapped = _world.GetItemAt(_view.WorldToTile(world - new Vector3(0f, 24f / 32f, 0f))) ?? _world.GetItemAt(tile);

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

            if (_creator != null)
            {
                if (_creator.Finished) FinishCreation();
                return; // yaratma ekranı açıkken oyun saati ilerlemez
            }

            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.bKey.wasPressedThisFrame) ToggleBuild();
            if (kb != null && kb.pKey.wasPressedThisFrame) ToggleMarket();

            float dt = Mathf.Min(Time.deltaTime, MaxFrameSeconds);

            // Biyoloji ve iş ilerlemesi oyun saatinde işler; hız çarpanı GameClock'ta uygulanır.
            double gameDays = _clock.Advance(dt);
            if (_clock.DayIndex != _lastSavedDay) { _lastSavedDay = _clock.DayIndex; SaveNow(quiet: true); }
            _heat.Tick(gameDays, _clock.TotalDays);
            _plants.Tick(gameDays, _clock.Season);
            _market.Tick(gameDays, _clock.TotalDays, _clock.Season);
            foreach (var (plant, view) in _plantViews)
                if (view != null) { float s = PlantScale(plant); view.localScale = new Vector3(s, s, 1f); }
            foreach (var a in _actors) a.Character.AmbientTemperature = _heat.TemperatureAt(a.Character.Position);
            _population.Tick(gameDays);
            foreach (var actor in _actors.ToArray())
            {
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
            float scale = actor.Character.Stage == AgeStage.Baby ? 0.6f : (actor.Character.Stage == AgeStage.Child ? 0.8f : 1f);
            if (actor.Character.Stage != AgeStage.Baby && actor.Character.Stage != AgeStage.Child) scale *= actor.Character.Appearance.HeightPercent / 100f;
            actor.View.localScale = new Vector3(scale, scale, 1f);
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

            if (_creator != null)
            {
                _creator.OnGUI();
                if (HasSave)
                {
                    float cs = HudScale;
                    GUI.matrix = Matrix4x4.Scale(new Vector3(cs, cs, 1f));
                    if (GUI.Button(new Rect(Screen.width / cs - 238f, Screen.height / cs - 52f, 230f, 44f), "Kayıtlı oyuna devam et")) LoadSave();
                }
                return;
            }

            float s = HudScale;
            GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));

            DrawItemLabels(s);
            _buildMode.OnGUI(s);
            _marketPanel.OnGUI(s);

            GUILayout.BeginArea(HudRect, GUI.skin.box);
            _hudScroll = GUILayout.BeginScrollView(_hudScroll);
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
                    $"{c.Name} ({(c.IsDead ? "Öldü" : StageName(c.Stage))}{(c.IsPregnant ? ", hamile" : "")}, {c.AgeDays:0}g): Açlık {c.Needs.Get(NeedType.Hunger):0} · Enerji {c.Needs.Get(NeedType.Energy):0} · Isınma {c.Needs.Get(NeedType.Warmth):0}");
                if (!c.IsDead) GUILayout.Label($"   {ActivityText(actor.Runner)}");
                var skills = SkillsText(c);
                if (skills.Length > 0) GUILayout.Label($"   Beceri: {skills}");
            }

            if (_maud != null && !_maud.Character.IsDead && GUILayout.Button("Test: Maud hamile (2 günde doğurur)"))
            {
                _maud.Character.PregnancyDays = 2;
                if (!_maud.Character.TryStartPregnancy(_player.Character.Id)) ShowMessage("Maud şu an hamile kalamaz");
                else ShowMessage("Maud hamile");
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_buildMode.Active ? "Yapı modundan çık (B)" : "Yapı modu (B)")) ToggleBuild();
            if (GUILayout.Button(_marketPanel.Active ? "Pazarı kapat (P)" : "Pazar (P)")) ToggleMarket();
            GUILayout.EndHorizontal();
            GUILayout.Label($"Akçe: {_household.Akce}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Kaydet")) SaveNow(quiet: false);
            GUI.enabled = HasSave;
            if (GUILayout.Button("Yükle")) { LoadSave(); return; }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Yeni oyun (kayıt ilk gün dolunca üzerine yazılır)"))
            {
                enabled = false;
                new GameObject("GameRoot").AddComponent<GameRoot>();
                Destroy(gameObject);
                return;
            }
            int next = AccountProgress.XpForLevel(_account.Level + 1);
            GUILayout.Label($"Hesap Sv {_account.Level} · XP {_account.Xp}/{next} · bugünkü iş XP {_account.WorkXpToday(_clock.DayIndex)}/{AccountProgress.DailyWorkCap}");
            DrawHeatHud();
            GUILayout.Label($"Eşya tanımı: {_content.Items.Count}");
            if (Time.unscaledTime < _messageUntil) GUILayout.Label(_message);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawHeatHud()
        {
            string here = _player != null ? $"{_heat.TemperatureAt(_player.Character.Position):0}°C" : "-";
            GUILayout.Label($"Dış {_heat.OutdoorTemperature:0}°C · {_player?.Character.Name}: {here}");
            GUILayout.Label($"Odun {_household.Count(_heat.Config.FirewoodId)} · Kütük {_household.Count("item.log")} · Meşe fidanı {_household.Count("item.sapling_oak")}");
            GUILayout.Label($"Şalgam {_household.Count("item.turnip")} · Şalgam tohumu {_household.Count("item.seed_turnip")}");
            GUILayout.Label($"Depo {_storage.Used}/{_storage.Capacity}{(_storage.Free <= 0 ? " (dolu)" : "")}");
            foreach (var item in _world.Items)
            {
                if (item.IsDecor || item.Def.HeatOutput <= 0f) continue;
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{item.Def.DisplayName}: {(item.IsLit ? $"yanıyor {item.FuelDays:0.0}g" : "sönük")}");
                if (GUILayout.Button("Odun at", GUILayout.Width(70f)))
                {
                    bool ok = _heat.TryStoke(item.Id, _household);
                    if (ok) _account.Record("fire", _clock.DayIndex, isWork: false);
                    ShowMessage(ok ? "Odun atıldı" : (_household.Count(_heat.Config.FirewoodId) <= 0 ? "Odun yok (kes, yar ya da pazardan al)" : "Ocak dolu"));
                }
                GUILayout.EndHorizontal();
            }
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
                if (item.IsDecor) continue;
                Vector3 sp = cam.WorldToScreenPoint(_view.TileToWorld(item.Position) + new Vector3(0f, 1.5f, 0f));
                if (sp.z < 0f) continue;
                float x = sp.x / guiScale;
                float y = (Screen.height - sp.y) / guiScale;
                string label = item.Def.HeatOutput > 0f ? $"{item.Def.DisplayName} ({(item.IsLit ? "yanıyor" : "sönük")})"
                    : item.IsPlant ? (item.IsGrown ? $"{item.Def.DisplayName} (olgun)" : $"{item.Def.DisplayName} (%{item.GrowthFraction * 100f:0}{(PlantSystem.CanGrowIn(_clock.Season) ? "" : ", kışın büyümez")})")
                    : item.Def.DisplayName;
                GUI.Label(new Rect(x - 70f, y - 10f, 140f, 20f), label, _labelStyle);
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
