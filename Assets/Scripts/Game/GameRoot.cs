using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Customization;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Family;
using Thatchwick.Core.Heat;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Mood;
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
            public int Direction = IsoArt.DefaultDirection;
            public Sprite LastSprite;
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
        private FamilyService _family;
        private FamilyPanel _familyPanel;
        private Dynasty _dynasty = new Dynasty();
        private Actor _selected;          // haritaya dokununca komut alan kişi (aile içi iş verme)
        private bool _dynastyEnded;
        private int _stallId;
        private readonly List<(ItemInstance item, Transform view)> _plantViews = new List<(ItemInstance, Transform)>();
        private readonly List<(ItemInstance item, SpriteRenderer view)> _heatViews = new List<(ItemInstance, SpriteRenderer)>();
        private Vector2 _hudScroll;
        private AnimalSystem _animals;
        private RoomMoodSystem _roomMood;
        private sealed class AnimalView { public Transform T; public SpriteRenderer R; public int Dir = IsoArt.DefaultDirection; }
        private readonly Dictionary<int, AnimalView> _animalViews = new Dictionary<int, AnimalView>();

        // Etkileşim menüsü (Sims'teki pasta menüsü gibi): eşyaya dokununca seçenekler listelenir.
        private struct MenuOption { public ItemInstance Item; public InteractionDef Interaction; public string Blocked; }
        private readonly List<MenuOption> _menuOptions = new List<MenuOption>();
        private bool _menuOpen;
        private Rect _menuRect;
        private GridPos _menuTile;
        private Vector2 _menuScroll;
        private string _menuTitle;
        private static readonly string[] GirlNames = { "Edith", "Agnes", "Matilda", "Joan", "Isolde", "Mabel", "Cecily", "Alys" };
        private static readonly string[] BoyNames = { "Wat", "Hob", "Piers", "Osric", "Godric", "Tobias", "Alard", "Hugh" };
        private int _nameCounter;

        // Yaşam katmanı (Adım 18-43): tüm kurallar Core.Life'ta; burası kurar, her karede ilerletir, çizer.
        private LifeSystems _life;
        private BuildService _buildService;
        private StatusHud _statusHud;
        private VillagePanel _villagePanel;
        private readonly Dictionary<int, string> _villagerLocation = new Dictionary<int, string>();
        private double _scheduleTimer;

        // Adlandırılmış yerler (life.json'daki home/work/square anahtarları) harita karolarına çevrilir.
        private static readonly Dictionary<string, GridPos> Places = new Dictionary<string, GridPos>
        {
            { "home:1", new GridPos(38, 12) }, { "home:2", new GridPos(40, 12) }, { "home:3", new GridPos(38, 19) },
            { "home:4", new GridPos(41, 20) }, { "home:5", new GridPos(36, 11) }, { "home:6", new GridPos(40, 17) },
            { "work:smithy", new GridPos(39, 14) }, { "work:mill", new GridPos(41, 10) }, { "work:herb", new GridPos(37, 21) },
            { "work:pasture", new GridPos(42, 23) }, { "work:market", new GridPos(35, 15) }, { "work:pottery", new GridPos(40, 19) },
            { "square", new GridPos(36, 16) }
        };

        /// <summary>Yeniden kurulan GameRoot bu kaydı yükler (Yükle düğmesi / Devam et).</summary>
        public static SaveData PendingLoad;
        private long _lastSavedDay = -1;
        private static string SavePath => System.IO.Path.Combine(Application.persistentDataPath, "thatchwick_save.json");
        public static bool HasSave => System.IO.File.Exists(SavePath);

        private float HudScale => Mathf.Max(1f, Screen.height / 540f);
        private Rect HudRect => new Rect(8f, 8f, 340f, Mathf.Min(560f, Screen.height / HudScale - 16f));

        private void Awake()
        {
            IsoArt.Reset();
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
                _household.AddItem("item.plank", 6);
                _household.AddItem("item.nails", 4);
            }
            _storage = new StorageService(_world, _household, _content);
            _effects = new InteractionEffects(_world, _household, _content, seed: 9) { Storage = _storage };
            _effects.Applied += (c, i, summary) => { ShowMessage($"{c.Name}: {summary}"); _account.Record(i.Id, _clock.DayIndex); };
            _plants = new PlantSystem(_world);
            _animals = new AnimalSystem(_world, _content, seed: 23);
            if (loaded != null)
            {
                SaveGame.RestoreAnimals(loaded, _animals, _content);
                foreach (var w in loaded.Warnings) if (w.StartsWith("Hayvan")) Debug.LogWarning($"Kayıt: {w}");
            }
            _effects.Animals = _animals;
            _animals.Died += OnAnimalDied;
            _roomMood = new RoomMoodSystem(_world);
            _market = new Market(_content, _household, _storage, null, seed: 17);
            if (loaded != null) _market.RestoreState(loaded.MarketDemand, _clock.TotalDays, _clock.Season, loaded.MarketTakenOffers);
            else _market.Tick(0, _clock.TotalDays, _clock.Season);
            _market.OfferAccepted += o => _account.Record(o.IsContract ? "contract" : "barter", _clock.DayIndex);
            _marketPanel = new MarketPanel(_market, _household, _storage, _content, _world,
                () => _player?.Character, () => _player?.Runner, _stallId);
            _plants.PlantDied += (def, pos) =>
            {
                ShowMessage($"{def.DisplayName} kışın dondu");
                foreach (var c in _population.Members) if (!c.IsVillager) c.AddMoodlet(MoodCatalog.CropFrozen);
            };
            _heat = new HeatSystem(_world, _clock.Config, null, _clock.TotalDays);
            if (loaded != null) _heat.RestoreTemperatures(loaded.Temperatures);
            _buildService = new BuildService(_world, _household, _content) { Storage = _storage, AccountLevel = () => _account.Level, HousingResidents = _animals.Residents };
            _buildMode = new BuildMode(_world, _household, _buildService, _content, _view, _input, transform, _walls, _animals);
            _buildMode.Activity += a => _account.Record(a, _clock.DayIndex);

            _population = new Population(firstFreeId: 1, seed: 21);
            _population.NameGenerator = sex => (sex == Sex.Female ? GirlNames : BoyNames)[_nameCounter++ % (sex == Sex.Female ? GirlNames.Length : BoyNames.Length)];
            _population.BabyBorn += OnBabyBorn;
            _family = new FamilyService(_population, _household, _content, seed: 31);
            _family.Happened += OnFamilyEvent;
            _family.JoinedHousehold += OnJoinedHousehold;

            _life = new LifeSystems(_content, _world, _household, _population, _clock) { AccountLevel = () => _account.Level };
            _life.Attach(_family);
            _life.Attach(_plants);
            _life.Attach(_heat);
            _life.Attach(_market);
            _life.Attach(_buildService);
            _life.Log.Added += e => ShowMessage(e.Text, e.Level == NoticeLevel.Info ? 2.5f : 4f);
            _life.Suitors.SpawnPosition = () => NearestWalkable(Places["square"], 8);
            _life.Suitors.Arrived += c => CreateActorFor(c);
            _statusHud = new StatusHud(_life, _content, () => Commander?.Character, c => _actors.Find(a => a.Character == c)?.Runner);
            _villagePanel = new VillagePanel(_life, _content, _household, _family, _dynasty, () => Commander?.Character);

            _familyPanel = new FamilyPanel(_population, _family, _dynasty, () => Commander?.Character,
                c => _selected = _actors.Find(a => a.Character == c), c => _actors.Find(a => a.Character == c)?.Runner);

            _appearanceCatalog = ContentLoader.LoadAppearance(out var appearanceErrors);
            foreach (var e in appearanceErrors) Debug.LogError($"Görünüm hatası: {e}");
            _population.AppearanceCatalog = _appearanceCatalog;
            _life.Suitors.AppearanceCatalog = _appearanceCatalog;

            if (loaded != null)
            {
                foreach (var c in loaded.Characters)
                {
                    _population.Add(c);
                    var actor = CreateActorFor(c);
                    if (c.IsPlayerControlled && !c.IsDead && _player == null) _player = actor;
                    if (c.Name == "Maud" && _maud == null) _maud = actor;
                }
                foreach (var (a, b, v) in loaded.Affection) _family.SetAffection(a, b, v);
                _life.Restore(loaded.Life);
                SpawnNamedVillagers();
                if (loaded.Dynasty != null) _dynasty.Restore(loaded.Dynasty.HeadId, loaded.Dynasty.History, loaded.Dynasty.Ended);
                else if (_player != null) _dynasty.Found(_player.Character, _clock.TotalDays);
                _dynastyEnded = _dynasty.Ended || _player == null;
                _selected = _player;
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
            maud.SetTraits(LifeRules.RollTraits(_content, _rng, 1 + _rng.Next(2)));
            maud.LifespanDays += LifeRules.LifespanBonus(_content, maud.TraitIds);
            LifeRules.Recompute(maud, _content, _life.Village);
            _population.Add(maud);
            _maud = CreateActorFor(maud);

            // Adlı köylüler (demirci, değirmenci, otacı...) pazar çevresinde yaşar; sohbet ve hediyeyle yakınlaşılır.
            // Evlenmek isteyenler için talip havuzu (LifeSystems.Suitors) gerekirse yenisini getirir.
            SpawnNamedVillagers();
            _life.Weather.Seed = _rng.Next(1, int.MaxValue);

            _creator = new CharacterCreator(_appearanceCatalog, seed: 5) { Content = _content };
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
            // Keyif ve hijyen için başlangıç: leğen ve kütük sehpada değirmen oyunu.
            TryPlace("item.basin_ewer", new GridPos(30, 15));
            TryPlace("item.stump_table", new GridPos(28, 15));
            var stump = _world.GetItemAt(new GridPos(28, 15));
            if (stump != null && _content.Items.TryGet("item.morris_board", out var board)) _world.PlaceOnSurface(board, stump.Id, "a");

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
            _heatViews.Clear();

            foreach (var item in _world.Items)
            {
                var color = ItemColor(item.Def.Id);
                if (item.IsWallMounted)
                {
                    var wallArt = IsoArt.Item(item, out _);
                    AddItemSprite(item, Iso.WallMount(item.Position, item.Rotation), color, wallArt != null ? 1f : 0.3f,
                        Iso.WallMountOrder(item.Position, item.Rotation), wallArt);
                    continue;
                }
                if (item.IsDecor)
                {
                    var parent = _world.GetItem(item.ParentId.Value);
                    if (parent == null || !parent.TryGetSlotPosition(item.SlotId, out float gx, out float gy)) continue;
                    if (parent.IsWallMounted)
                    {
                        var onShelf = IsoArt.Item(item, out _);
                        AddItemSprite(item, Iso.GridToWorld(gx, gy) + new Vector3(0f, Iso.WallMountHeight + 0.1f, 0f), color,
                            onShelf != null ? 1f : 0.25f, Iso.WallMountOrder(parent.Position, parent.Rotation) + 1, onShelf);
                        continue;
                    }
                    // Gerçek masanın üst yüzü yer tutucu küpten biraz yüksek.
                    float topPx = IsoArt.Item(parent, out _) != null ? 27f : 24f;
                    var pos = Iso.GridToWorld(gx, gy) + new Vector3(0f, topPx / 32f, 0f);
                    var decorArt = IsoArt.Item(item, out _);
                    AddItemSprite(item, pos, color, decorArt != null ? 1f : 0.35f, Iso.SortOrder(Iso.GridToWorld(gx, gy).y) + 2, decorArt);
                    continue;
                }
                bool flip = false;
                var art = item.IsPlant ? null : IsoArt.Item(item, out flip);
                if (art != null)
                {
                    // Tek görsel, ayak izinin merkezinde (pivot = taban elmasının merkezi).
                    var center = Iso.GridToWorld(item.Position.X + item.Width * 0.5f, item.Position.Y + item.Height * 0.5f);
                    var sr = AddItemSprite(item, center, Color.white, 1f, Iso.SortOrder(center.y), art);
                    sr.color = Color.white;
                    sr.flipX = flip;
                    if (item.Def.HeatOutput > 0f) _heatViews.Add((item, sr));
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

        private SpriteRenderer AddItemSprite(ItemInstance item, Vector3 pos, Color color, float scale, int order, Sprite sprite = null)
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
            return sr;
        }

        /// <summary>İstenen yerin çevresinde boş (eşyasız, geçilebilir) ilk karo.</summary>
        private GridPos NearestWalkable(GridPos near, int maxRadius = 4)
        {
            for (int r = 0; r <= maxRadius; r++)
                for (int dy = -r; dy <= r; dy++)
                    for (int dx = -r; dx <= r; dx++)
                    {
                        var p = new GridPos(near.X + dx, near.Y + dy);
                        if (_world.Map.IsWalkable(p)) return p;
                    }
            return near;
        }

        private GridPos ResolvePlace(string key, int jitterSeed = 0)
        {
            if (string.IsNullOrEmpty(key) || !Places.TryGetValue(key, out var basePos)) basePos = Places["square"];
            if (jitterSeed != 0) basePos = new GridPos(basePos.X + (jitterSeed % 5) - 2, basePos.Y + (jitterSeed / 5 % 3) - 1);
            return NearestWalkable(basePos);
        }

        // life.json'daki adlı köylüleri (eksik olanları) karakter olarak ekler; görünüm ve ömür burada verilir.
        private void SpawnNamedVillagers()
        {
            foreach (var c in _life.Villagers.SpawnMissing(key => ResolvePlace(key)))
            {
                c.Appearance = _appearanceCatalog.Randomize(_rng, c.Sex);
                c.LifespanDays = c.AgeConfig.RollLifespan(_rng) + LifeRules.LifespanBonus(_content, c.TraitIds);
                CreateActorFor(c);
            }
        }

        // Köylüler gün saatine, havaya ve şenliğe göre evinden işine, meydana yürür.
        private void UpdateVillagerSchedules()
        {
            var weather = _life.Weather.Today(_clock.TotalDays);
            bool festival = _life.Festivals.TodaysFestival() != null;
            foreach (var a in _actors)
            {
                var c = a.Character;
                if (c.IsDead || !c.IsVillager) continue;
                var def = _life.Villagers.DefOf(c);
                if (def == null) continue;
                string where = _life.Villagers.LocationFor(def, _clock.DayFraction, weather, festival);
                if (_villagerLocation.TryGetValue(c.Id, out var last) && last == where) continue;
                _villagerLocation[c.Id] = where;
                a.Runner.Command(Job.MoveTo(ResolvePlace(where, where == "square" ? c.Id : 0)));
            }
        }

        // Köylünün ihtiyaçları köyde karşılanır (sabit); haneye katılınca hane gibi yaşar.
        private static void ApplyVillagerRules(Character c, JobRunner runner)
        {
            c.CanStarve = !c.IsVillager;
            c.MoodEnabled = !c.IsVillager;   // keyif, hijyen ve ruh hali hane üyelerinde
            c.Needs.SetDecayPerDay(NeedType.Hunger, c.IsVillager ? 0f : 120f);
            c.Needs.SetDecayPerDay(NeedType.Energy, c.IsVillager ? 0f : 90f);
            runner.Autonomous = !c.IsVillager && !c.IsPlayerControlled;
        }

        private void OnJoinedHousehold(Character c)
        {
            var actor = _actors.Find(a => a.Character == c);
            if (actor != null) ApplyVillagerRules(c, actor.Runner);
        }

        private void OnFamilyEvent(Character actor, Character target, SocialAction action, string message)
        {
            ShowMessage(message);
            switch (action)
            {
                case SocialAction.Feed: _account.Record("feed", _clock.DayIndex); break;
                case SocialAction.Propose: _account.Record("marriage", _clock.DayIndex, isWork: false); break;
                case SocialAction.Chat: _account.Record("chat", _clock.DayIndex, isWork: false); break;
            }
        }

        // Baş karakter ölünce hanedan sıradakine geçer; kimse yoksa hanedan biter.
        private void OnCharacterDied(Character c)
        {
            ShowMessage(c.CauseOfDeath == DeathCause.Cold ? $"{c.Name} donarak öldü"
                : c.CauseOfDeath == DeathCause.Starvation ? $"{c.Name} açlıktan öldü" : $"{c.Name} hayatını kaybetti");
            var result = _dynasty.OnDeath(c, _population.Members, _clock.TotalDays, _life.TradeOf(c));
            if (result == null) { if (_selected != null && _selected.Character == c) _selected = _player; return; }
            if (result.DynastyEnded) { _dynastyEnded = true; ShowMessage(result.Message, 6f); return; }
            var heir = _actors.Find(a => a.Character == result.Heir);
            if (heir != null)
            {
                _player = heir;
                _selected = heir;
                heir.Runner.Autonomous = false;
                heir.Runner.Cancel();
            }
            _account.Record("succession", _clock.DayIndex, isWork: false);
            ShowMessage(result.Message, 6f);
        }

        /// <summary>Dokunuş komutlarını alan kişi: aile panelinde seçilen, yoksa baş karakter.</summary>
        private Actor Commander => _selected != null && FamilyPanel.CanTakeCommands(_selected.Character) ? _selected : _player;

        private Actor CreateActorFor(Character character)
        {
            character.StageChanged += (c, from, to) => ShowMessage($"{c.Name}: {StageName(to)} oldu");
            character.Died += OnCharacterDied;
            character.Skills.LevelChanged += (skill, from, to) => { ShowMessage($"{character.Name}: {SkillName(skill)} seviye {to}"); if (to > from) _account.OnSkillLevelUp(); };

            var go = new GameObject(character.Name);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = IsoArt.Character(character, IsoArt.DefaultDirection) ?? PersonSprites.Get(character.Appearance);
            sr.sharedMaterial = SpriteMaterials.Unlit;
            go.transform.position = _view.TileToWorld(character.Position);

            var actor = new Actor
            {
                Character = character,
                Runner = new JobRunner(character, _world) { Effects = _effects, Social = _family },
                View = go.transform,
                Renderer = sr
            };
            _life.Attach(actor.Runner);
            ApplyVillagerRules(character, actor.Runner);
            _actors.Add(actor);
            return actor;
        }

        // Yeni doğan bebek: sahneye küçük bir karakter olarak eklenir (bebek evresinde yetişkin boyunun yarısı).
        private void OnBabyBorn(Character baby, Character mother, int? fatherId)
        {
            CreateActorFor(baby);
            MoodEvents.OnBirth(mother, fatherId.HasValue ? _population.Find(fatherId.Value) : null);
            ShowMessage($"{mother.Name} doğurdu: {baby.Name}");
        }

        private bool IsOverHud(Vector2 screenPos)
        {
            float s = HudScale;
            var guiPos = new Vector2(screenPos.x / s, (Screen.height - screenPos.y) / s);
            if (_buildMode != null && _buildMode.Active && _buildMode.PanelRect(s).Contains(guiPos)) return true;
            if (_marketPanel != null && _marketPanel.Active && _marketPanel.PanelRect(s).Contains(guiPos)) return true;
            if (_familyPanel != null && _familyPanel.Active && _familyPanel.PanelRect(s).Contains(guiPos)) return true;
            if (_villagePanel != null && _villagePanel.Active && _villagePanel.PanelRect(s).Contains(guiPos)) return true;
            if (_statusHud != null && _statusHud.IsOver(guiPos)) return true;
            if (_dynastyEnded) return true;
            if (_menuOpen && _menuRect.Contains(guiPos)) return true;
            return HudRect.Contains(guiPos);
        }

        private void DrawDynastyEnded(float s)
        {
            var r = new Rect(Screen.width / s * 0.5f - 170f, Screen.height / s * 0.5f - 70f, 340f, 140f);
            GUILayout.BeginArea(r, GUI.skin.box);
            GUILayout.Label("Hanedan sona erdi.");
            GUILayout.Label($"{_dynasty.History.Count} baş karakter, {_dynasty.CurrentGeneration} nesil sürdü.");
            if (GUILayout.Button("Yeni oyun"))
            {
                enabled = false;
                new GameObject("GameRoot").AddComponent<GameRoot>();
                Destroy(gameObject);
            }
            GUILayout.EndArea();
        }

        // ---- kayıt ----

        private void SaveNow(bool quiet)
        {
            if (_player == null || _creator != null || _dynastyEnded) return;   // oyun başlamadan ya da bittikten sonra kaydetme
            try
            {
                var data = SaveGame.Capture(_clock, _world, _household, _population.Members, _account, _heat, _market, _family, _dynasty, _animals, _life);
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
            if (_familyPanel.Active) _familyPanel.Close();
            if (_villagePanel.Active) _villagePanel.Close();
            _buildMode.Toggle();
        }

        private void ToggleMarket()
        {
            if (_buildMode.Active) _buildMode.Toggle();
            if (_familyPanel.Active) _familyPanel.Close();
            if (_villagePanel.Active) _villagePanel.Close();
            _marketPanel.Toggle();
        }

        private void ToggleVillage()
        {
            if (_buildMode.Active) _buildMode.Toggle();
            if (_marketPanel.Active) _marketPanel.Close();
            if (_familyPanel.Active) _familyPanel.Close();
            _villagePanel.Toggle();
        }

        /// <summary>Gerçek görseller ile yer tutucu çizimler arasında geçiş (karşılaştırma için).</summary>
        private void ToggleArt()
        {
            IsoArt.Enabled = !IsoArt.Enabled;
            _view.RefreshFloors();
            _walls.Rebuild();
            RebuildItemViews();
            foreach (var a in _actors) a.LastSprite = null;
            ShowMessage(IsoArt.Enabled ? $"Gerçek görseller ({IsoArt.TextureCount} dosya)" : "Yer tutucu çizimler");
        }

        private void ToggleFamily()
        {
            if (_buildMode.Active) _buildMode.Toggle();
            if (_marketPanel.Active) _marketPanel.Close();
            if (_villagePanel.Active) _villagePanel.Close();
            _familyPanel.Toggle();
        }

        private void FinishCreation()
        {
            var character = _creator.Draft.CreateCharacter(1, new GridPos(24, 10), true, _appearanceCatalog, _rng);
            _creator.Release();
            _creator = null;
            if (character.TraitIds.Count == 0) character.SetTraits(LifeRules.RollTraits(_content, _rng, 1 + _rng.Next(2)));
            character.LifespanDays += LifeRules.LifespanBonus(_content, character.TraitIds);
            LifeRules.Recompute(character, _content, _life.Village);
            _population.Add(character);
            _player = CreateActorFor(character);
            _selected = _player;
            _dynasty.Found(character, _clock.TotalDays);
            _dynasty.Name = $"{character.Name} hanedanı";
            _input.enabled = true;
            ShowMessage($"{character.Name} köye geldi");
        }

        private void OnTapped(Vector2 screenPos)
        {
            var cam = Camera.main;
            if (cam == null || Commander == null || _dynastyEnded) return;

            var world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            var tile = _view.WorldToTile(world);

            if (_buildMode.Active) { _menuOpen = false; _buildMode.OnTap(world); return; }
            if (_menuOpen) { _menuOpen = false; return; }   // menü dışına dokunmak menüyü kapatır

            CollectOptions(world);
            if (_menuOptions.Count == 0) { Commander.Runner.Command(Job.MoveTo(tile)); return; }
            if (_menuOptions.Count == 1) { RunOption(_menuOptions[0]); return; }

            // Birden çok seçenek: dokunulan yerde menü aç.
            float s = HudScale;
            float h = Mathf.Min(34f + 26f * (_menuOptions.Count + 1), 320f);
            float x = Mathf.Clamp(screenPos.x / s + 12f, 4f, Screen.width / s - 244f);
            float y = Mathf.Clamp((Screen.height - screenPos.y) / s - 20f, 4f, Screen.height / s - h - 4f);
            _menuRect = new Rect(x, y, 240f, h);
            _menuTile = tile;
            _menuScroll = Vector2.zero;
            _menuOpen = true;
        }

        // Dokunulan noktadaki eşyaların (duvar eşyası, yer eşyası ve üstündeki dekorlar) tüm etkileşimleri.
        private void CollectOptions(Vector3 world)
        {
            _menuOptions.Clear();
            var items = new List<ItemInstance>();
            var wall = BuildMode.PickWallItem(_world, world);
            var floor = _world.GetItemAt(_view.WorldToTile(world - new Vector3(0f, 24f / 32f, 0f))) ?? _world.GetItemAt(_view.WorldToTile(world));
            foreach (var root in new[] { wall, floor })
            {
                if (root == null) continue;
                items.Add(root);
                items.AddRange(_world.GetDecorOn(root.Id));
            }
            var names = new List<string>();
            foreach (var item in items)
            {
                if (item.Def.Interactions.Count > 0 && !names.Contains(item.Def.DisplayName)) names.Add(item.Def.DisplayName);
                foreach (var it in item.Def.Interactions)
                    _menuOptions.Add(new MenuOption { Item = item, Interaction = it, Blocked = Commander.Runner.WhyCannotStart(item, it) });
            }
            _menuTitle = string.Join(", ", names);
        }

        private void RunOption(MenuOption o)
        {
            Commander.Runner.Command(Job.UseItem(o.Item.Id, o.Interaction.Id));
            ShowMessage($"{Commander.Character.Name}: {o.Interaction.DisplayName} ({o.Item.Def.DisplayName})");
        }

        private void DrawInteractionMenu()
        {
            if (!_menuOpen) return;
            if (Commander == null) { _menuOpen = false; return; }
            GUILayout.BeginArea(_menuRect, GUI.skin.box);
            GUILayout.Label($"{Commander.Character.Name} · {_menuTitle}");
            _menuScroll = GUILayout.BeginScrollView(_menuScroll);
            foreach (var o in _menuOptions)
            {
                GUI.enabled = o.Blocked == null;
                string label = o.Blocked == null ? o.Interaction.DisplayName : $"{o.Interaction.DisplayName} ({o.Blocked})";
                if (GUILayout.Button(label)) { RunOption(o); _menuOpen = false; }
                GUI.enabled = true;
            }
            if (GUILayout.Button("Buraya yürü")) { Commander.Runner.Command(Job.MoveTo(_menuTile)); _menuOpen = false; }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void OnAnimalDied(Animal a)
        {
            ShowMessage(a.CauseOfDeath == AnimalDeath.Starvation ? $"{a.Name} açlıktan öldü (barınağa yem koy)" : $"{a.Name} yaşlılıktan öldü");
            foreach (var c in _population.Members) if (!c.IsVillager) c.AddMoodlet(MoodCatalog.AnimalDied);
        }

        private static Color AnimalColor(string id)
        {
            switch (id)
            {
                case "animal.chicken": return new Color(0.95f, 0.92f, 0.85f);
                case "animal.duck": return new Color(0.55f, 0.7f, 0.45f);
                case "animal.goat": return new Color(0.75f, 0.68f, 0.55f);
                case "animal.sheep": return new Color(0.92f, 0.9f, 0.86f);
                case "animal.cow": return new Color(0.55f, 0.4f, 0.3f);
                case "animal.dog": return new Color(0.6f, 0.45f, 0.3f);
                case "animal.cat": return new Color(0.85f, 0.55f, 0.25f);
                default: return new Color(0.95f, 0.8f, 0.2f);
            }
        }

        private static float AnimalScale(string id)
        {
            switch (id)
            {
                case "animal.chicken": case "animal.duck": case "animal.bees": return 0.22f;
                case "animal.cat": return 0.26f;
                case "animal.dog": return 0.3f;
                case "animal.cow": return 0.5f;
                default: return 0.4f;
            }
        }

        // Hayvan görünümleri: canlı hayvanlarla eşitlenir, yürüyüş ara değerlenir.
        private void UpdateAnimalViews()
        {
            var alive = new HashSet<int>();
            foreach (var a in _animals.Animals)
            {
                alive.Add(a.Id);
                if (!_animalViews.TryGetValue(a.Id, out var v))
                {
                    var go = new GameObject(a.Name);
                    go.transform.SetParent(transform, false);
                    v = new AnimalView { T = go.transform, R = go.AddComponent<SpriteRenderer>() };
                    v.R.sharedMaterial = SpriteMaterials.Unlit;
                    _animalViews[a.Id] = v;
                }
                Vector3 pos = _view.TileToWorld(a.Position);
                if (a.NextTile.HasValue)
                {
                    var to = _view.TileToWorld(a.NextTile.Value);
                    v.Dir = IsoArt.DirectionIndex(new Vector2(to.x - pos.x, to.y - pos.y));
                    pos = Vector3.Lerp(pos, to, a.StepProgress);
                }
                v.T.position = pos;
                var real = IsoArt.Animal(a.Species.Id, v.Dir);
                float scale = a.IsMature ? 1f : 0.7f;
                if (real != null) { v.R.sprite = real; v.R.color = Color.white; }
                else { v.R.sprite = SpriteMaterials.Cube(AnimalColor(a.Species.Id)); v.R.color = Color.white; scale *= AnimalScale(a.Species.Id); }
                v.T.localScale = new Vector3(scale, scale, 1f);
                v.R.sortingOrder = Iso.SortOrder(pos.y, 1);
            }
            var gone = new List<int>();
            foreach (var kv in _animalViews) if (!alive.Contains(kv.Key)) gone.Add(kv.Key);
            foreach (var id in gone) { if (_animalViews[id].T != null) Destroy(_animalViews[id].T.gameObject); _animalViews.Remove(id); }
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
            if (kb != null && kb.fKey.wasPressedThisFrame) ToggleFamily();
            if (kb != null && kb.gKey.wasPressedThisFrame) ToggleArt();
            if (kb != null && kb.vKey.wasPressedThisFrame) ToggleVillage();
            if (kb != null && kb.nKey.wasPressedThisFrame) _statusHud.ToggleLog();

            float dt = Mathf.Min(Time.deltaTime, MaxFrameSeconds);

            // Biyoloji ve iş ilerlemesi oyun saatinde işler; hız çarpanı GameClock'ta uygulanır.
            double gameDays = _clock.Advance(dt);
            if (_clock.DayIndex != _lastSavedDay) { _lastSavedDay = _clock.DayIndex; SaveNow(quiet: true); }
            Simulate(gameDays);
        }

        /// <summary>Editörde oyun testi için zamanı ileri sarar (küçük adımlarla gerçek simülasyon).</summary>
        private void FastForward(double days)
        {
            const double step = 0.02;
            for (double t = 0; t < days && !_dynastyEnded; t += step)
            {
                _clock.Restore(_clock.TotalDays + step, _clock.RequestedSpeed);
                Simulate(step);
            }
            ShowMessage($"{days:0} gün ileri sarıldı");
        }

        private void Simulate(double gameDays)
        {
            _heat.Tick(gameDays, _clock.TotalDays);
            _plants.Tick(gameDays, _clock.Season);
            _market.Tick(gameDays, _clock.TotalDays, _clock.Season);
            _animals.Tick(gameDays);
            _roomMood.Tick(gameDays, _population.Members);
            UpdateAnimalViews();
            foreach (var (plant, view) in _plantViews)
                if (view != null) { float s = PlantScale(plant); view.localScale = new Vector3(s, s, 1f); }
            foreach (var (heatItem, heatView) in _heatViews)
                if (heatView != null) { var lit = IsoArt.Item(heatItem, out _); if (lit != null && heatView.sprite != lit) heatView.sprite = lit; }
            foreach (var a in _actors)
                a.Character.AmbientTemperature = a.Character.IsVillager ? 15f : _heat.TemperatureAt(a.Character.Position);
            _population.Tick(gameDays);
            _life.Tick(gameDays);
            _scheduleTimer += gameDays;
            if (_scheduleTimer >= 0.03) { _scheduleTimer = 0; UpdateVillagerSchedules(); }
            foreach (var actor in _actors.ToArray())
            {
                if (actor.Character.IsDead)
                {
                    // Ölen yaşamaz: sahneden kalkar, yerinde mezarı (GraveService) durur.
                    if (actor.View != null) Destroy(actor.View.gameObject);
                    _actors.Remove(actor);
                    _villagerLocation.Remove(actor.Character.Id);
                    if (_selected == actor) _selected = _player == actor ? null : _player;
                    if (_maud == actor) _maud = null;
                    continue;
                }
                if (actor.Character.Stage != AgeStage.Baby) actor.Runner.Tick(gameDays);   // bebek iş yapmaz; büyükler besler
                UpdateView(actor);
            }

            if (Commander == null) return;
            var failure = Commander.Runner.LastFailure;
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
            {
                var to = _view.TileToWorld(next.Value);
                actor.Direction = IsoArt.DirectionIndex(new Vector2(to.x - pos.x, to.y - pos.y));
                pos = Vector3.Lerp(pos, to, actor.Runner.StepProgress);
            }
            actor.View.position = pos;
            // Gerçek görsel: yürüdüğü yöne bakar, yaş evresi değişince seti değişir.
            var real = IsoArt.Character(actor.Character, actor.Direction);
            var wanted = real != null ? real : PersonSprites.Get(actor.Character.Appearance);
            if (wanted != actor.LastSprite) { actor.Renderer.sprite = wanted; actor.LastSprite = wanted; }
            // Ekranda daha aşağıdaki (öndeki) nesne üste çizilir; aynı karoda karakter eşyanın önünde.
            actor.Renderer.sortingOrder = Iso.SortOrder(pos.y, 1);
            float scale;
            if (real != null)   // gerçek setler zaten çocuk/yetişkin boyunda çizildi; bebek çocuk setinin küçüğü
                scale = actor.Character.Stage == AgeStage.Baby ? 0.6f : 1f;
            else
            {
                scale = actor.Character.Stage == AgeStage.Baby ? 0.6f : (actor.Character.Stage == AgeStage.Child ? 0.8f : 1f);
                if (actor.Character.Stage != AgeStage.Baby && actor.Character.Stage != AgeStage.Child) scale *= actor.Character.Appearance.HeightPercent / 100f;
            }
            actor.View.localScale = new Vector3(scale, scale, 1f);
        }

        private void ShowMessage(string text, float seconds = 2.5f)
        {
            _message = text;
            _messageUntil = Time.unscaledTime + seconds;
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

        /// <summary>"İyi (+15): Dinlenmiş, Uyumlu ev"</summary>
        public static string MoodText(Character c, int maxNames = 3)
        {
            var names = new List<string>();
            foreach (var m in c.Mood.Active) if (names.Count < maxNames) names.Add(m.Def.DisplayName);
            string more = c.Mood.Active.Count > maxNames ? " ..." : "";
            string total = c.Mood.Total > 0 ? $"+{c.Mood.Total}" : c.Mood.Total.ToString();
            return names.Count == 0 ? $"{MoodRules.Name(c.Mood.Status)}" : $"{MoodRules.Name(c.Mood.Status)} ({total}): {string.Join(", ", names)}{more}";
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
            _familyPanel.OnGUI(s);
            _villagePanel.OnGUI(s);
            DrawBalloons(s);
            _statusHud.OnGUI(s);
            DrawInteractionMenu();
            if (_dynastyEnded) DrawDynastyEnded(s);

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
                if (c.ComfortNeedsActive && !c.IsDead)
                    GUILayout.Label($"   Keyif {c.Needs.Get(NeedType.Fun):0} · Hijyen {c.Needs.Get(NeedType.Hygiene):0} · Ruh hali: {MoodText(c)}");
                if (!c.IsDead) GUILayout.Label($"   {ActivityText(actor.Runner)}");
                var skills = SkillsText(c);
                if (skills.Length > 0) GUILayout.Label($"   Beceri: {skills}");
            }

            if (_maud != null && _player != null && !_maud.Character.IsDead && GUILayout.Button("Test: Maud hamile (2 günde doğurur)"))
            {
                _maud.Character.PregnancyDays = 2;
                if (!_maud.Character.TryStartPregnancy(_player.Character.Id)) ShowMessage("Maud şu an hamile kalamaz");
                else ShowMessage("Maud hamile");
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_buildMode.Active ? "Yapı modundan çık (B)" : "Yapı modu (B)")) ToggleBuild();
            if (GUILayout.Button(_marketPanel.Active ? "Pazarı kapat (P)" : "Pazar (P)")) ToggleMarket();
            if (GUILayout.Button(_familyPanel.Active ? "Aileyi kapat (F)" : "Aile (F)")) ToggleFamily();
            if (GUILayout.Button(_villagePanel.Active ? "Köyü kapat (V)" : "Köy (V)")) ToggleVillage();
            if (GUILayout.Button(IsoArt.Enabled ? "Görsel: gerçek (G)" : "Görsel: yer tutucu (G)")) ToggleArt();
            GUILayout.EndHorizontal();
            GUILayout.Label($"{_dynasty.Name} · {_dynasty.CurrentGeneration}. nesil · Baş: {_player?.Character.Name} · Komut alan: {Commander?.Character.Name}");
            GUILayout.Label($"Akçe: {_household.Akce}");
            GUILayout.BeginHorizontal();
            if (Application.isEditor && GUILayout.Button("Test: +5 gün")) FastForward(5);
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

        // Karakterlerin üstünde ne yaptıkları ve nedeni ("Yemek yiyor — çünkü acıktı").
        private void DrawBalloons(float guiScale)
        {
            var cam = Camera.main;
            if (cam == null) return;
            foreach (var actor in _actors)
            {
                if (actor.Character.IsDead || actor.Character.IsVillager || actor.View == null) continue;
                var sp = cam.WorldToScreenPoint(actor.View.position + new Vector3(0f, 1.7f, 0f));
                _statusHud.DrawBalloon(guiScale, sp, actor.Character, actor.Runner);
            }
        }

        private void DrawHeatHud()
        {
            string here = _player != null ? $"{_heat.TemperatureAt(_player.Character.Position):0}°C" : "-";
            GUILayout.Label($"Dış {_heat.OutdoorTemperature:0}°C · {_player?.Character.Name}: {here}");
            GUILayout.Label($"Odun {_household.Count(_heat.Config.FirewoodId)} · Kütük {_household.Count("item.log")} · Meşe fidanı {_household.Count("item.sapling_oak")}");
            GUILayout.Label($"Şalgam {_household.Count("item.turnip")} · Şalgam tohumu {_household.Count("item.seed_turnip")} · Tahta {_household.Count("item.plank")} · Çivi {_household.Count("item.nails")}");
            GUILayout.Label($"Yem {_household.Count("item.feed")} · Yumurta {_household.Count("item.egg")} · Süt {_household.Count("item.milk")} · Yün {_household.Count("item.wool")} · Bal {_household.Count("item.honey")} · Hayvan {_animals.Animals.Count}");
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
                if (item.IsDecor || item.IsWallMounted) continue;
                Vector3 sp = cam.WorldToScreenPoint(_view.TileToWorld(item.Position) + new Vector3(0f, 1.5f, 0f));
                if (sp.z < 0f) continue;
                float x = sp.x / guiScale;
                float y = (Screen.height - sp.y) / guiScale;
                string label = item.Def.HeatOutput > 0f ? $"{item.Def.DisplayName} ({(item.IsLit ? "yanıyor" : "sönük")})"
                    : item.IsPlant ? (item.IsGrown ? $"{item.Def.DisplayName} (olgun)" : $"{item.Def.DisplayName} (%{item.GrowthFraction * 100f:0}{(PlantSystem.CanGrowIn(_clock.Season) ? "" : ", kışın büyümez")})")
                    : item.Def.IsHousing ? $"{item.Def.DisplayName} ({_animals.Residents(item.Id)}/{item.Def.HousingCapacity}, yem {item.FeedStock:0.#}{(item.StockTotal > 0 ? $", ürün {item.StockTotal}" : "")})"
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
