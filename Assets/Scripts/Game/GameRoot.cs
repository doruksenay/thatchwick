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
using Thatchwick.Core.Mining;
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
        private const int MapWidth = Thatchwick.Core.World.Regions.WorldWidth;   // çiftlik (96x72, 64x64 arsa) + köy yolu + şehir (belge 39-40)
        private const int MapHeight = Thatchwick.Core.World.Regions.WorldHeight;
        private const int LegacyMapWidth = 48;   // eski kayıtların haritası
        // Bölgeli dünyada (çiftlik + şehir) etkin bölge; kamera ve zemin çizimi buna kilitli.
        private bool _regional;
        private Thatchwick.Core.World.Region _region;
        private float _fadeUntil;
        private const float MaxFrameSeconds = 0.1f;

        private sealed class Actor
        {
            public Character Character;
            public JobRunner Runner;
            public Transform View;
            public SpriteRenderer Renderer;
            public SpriteRenderer Shadow;
            public int Direction = IsoArt.DefaultDirection;
            public Sprite LastSprite;
            // Kırlaşma, kırışıklık, sargı: günde bir hesaplanır (AgingLook).
            public LookState Look;
            public int LookDay = -1;
            public string LookInjury;
            // Yürüme sallantısı (yürüme animasyonu gelene kadar): adım evresi ve önceki karedeki konum.
            public float WalkPhase;
            public Vector3 LastPos;
            // Animasyon partisi 1: son adım (toz için), iş vuruşu ve işin simgesi (parçacık türü).
            public int LastStep;
            public float LastBeat;
            public object WorkJob;
            public string WorkIcon;
            public Sprite ActionFrame;
        }

        /// <summary>Etkileşim karesi döngüsü: 8 kare yaklaşık 0,9 saniyede (kişiye göre evre).</summary>
        private static float ActionCycle(int seed) => Mathf.Repeat(Time.time / 0.9f + seed * 0.17f, 1f);

        /// <summary>Eylem simgesi (ActivityDescriber) → 018c-e kare anahtarları, sırayla denenir.</summary>
        private static readonly Dictionary<string, string[]> ActionKeys = new Dictionary<string, string[]>
        {
            ["eat"] = new[] { "eat2", "eat" },
            ["cook"] = new[] { "cook" },
            ["craft"] = new[] { "craft_hammer", "craft_hand" },
            ["chop"] = new[] { "chop" },
            ["farm"] = new[] { "hoe", "sow" },
            ["work"] = new[] { "work" },
        };

        private Sprite ActionFrameFor(Actor actor, string icon, int seed)
        {
            if (icon == null || !ActionKeys.TryGetValue(icon, out var keys)) return null;
            float cyc = ActionCycle(seed);
            foreach (var k in keys)
            {
                var f = IsoArt.CharacterAction(actor.Character, k, actor.Direction, cyc);
                if (f != null) return f;
            }
            return null;
        }

        /// <summary>Yürürken iki piksellik adım sallantısı; bir adım yaklaşık bu kadar dünya birimi.</summary>
        private const float WalkBobPixels = 2f, WalkStepUnits = 0.55f, ArtPixelsPerUnit = 32f;
        private static readonly Vector3 ShadowOffset = new Vector3(0.06f, -0.03f, 0f);

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
        private Ambience _ambience;
        private WeatherFx _weatherFx;
        private Juice _juice;
        /// <summary>Son kurulumda görünen eşyalar: yeni gelen eşya "pop" ile belirir.</summary>
        private readonly HashSet<int> _seenItemIds = new HashSet<int>();
        private bool _seenItemsReady;
        /// <summary>HUD'da yuvarlanarak sayılan akçe ve son değişimin rengi.</summary>
        private float _akceShown = float.NaN;
        private float _akceFlashUntil;
        private bool _akceFlashUp;
        private readonly List<Ambience.LightSource> _lights = new List<Ambience.LightSource>();
        private bool _ambienceSnap = true;
        private GUIStyle _alertStyle;
        private bool _labelsAll;          // L: bütün eşya etiketleri (dokunmatikte fare üstü yok)
        private bool _hudExpanded;        // Tab: sol panelin ayrıntıları (ihtiyaçlar, beceriler, ayarlar)
        // Rastgelelik: her oyunun ana tohumu kayıtta saklanır; alt sistemler bundan türetir (bkz. Core.Seeds).
        private System.Random _rng;
        private int _seed;
        private int _rngEpoch;
        // Dünya/harita değişince görünümler aynı karede en çok bir kez yeniden kurulur (LateUpdate).
        private bool _itemsDirty;
        private bool _floorsDirty;
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
        private StoragePanel _storagePanel;
        private MinePanel _minePanel;
        private KeepsakePanel _keepsakePanel;
        private bool _buntingShown;
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
        private sealed class AnimalView { public Transform T; public SpriteRenderer R; public SpriteRenderer Shadow; public int Dir = IsoArt.DefaultDirection; }
        private readonly Dictionary<int, AnimalView> _animalViews = new Dictionary<int, AnimalView>();

        // Etkileşim menüsü (Sims'teki pasta menüsü gibi): eşyaya dokununca seçenekler listelenir.
        // Eşya etkileşimi ya da özel seçenek (hayvan işi, çapalama, çan): Label/Run doluysa özel seçenektir.
        private struct MenuOption { public ItemInstance Item; public InteractionDef Interaction; public string Blocked; public string Label; public System.Action Run; }
        private AnimalCare _animalCare;
        private FarmWork _farmWork;
        private MineSystem _mines;
        private OxCart _cart;
        private FarmAutomation _automation;
        private WaterWorks _water;                    // su şebekesi ve tesisat (ark, boru, sarnıç)
        private DutyPlanner _duties;
        private double HourOfDay => _clock.DayFraction * 24.0;
        private bool _hoeMode;      // çapa kipi: boş çime dokununca çapalar
        private readonly List<MenuOption> _menuOptions = new List<MenuOption>();
        private bool _menuOpen;
        private Rect _menuRect;
        private GridPos _menuTile;
        private Vector2 _menuScroll;
        private string _menuTitle;
        // Hayali yerleşim: iki kültürün adları tek havuzda (karar 17.11.2026).
        private static readonly string[] GirlNames = { "Edith", "Agnes", "Matilda", "Joan", "Isolde", "Mabel", "Cecily", "Alys", "Elif", "Gülbahar", "Selcan", "Esma", "Nergis", "Ayla" };
        private static readonly string[] BoyNames = { "Wat", "Hob", "Piers", "Osric", "Godric", "Tobias", "Alard", "Hugh", "Tolga", "Baran", "Kaya", "Alp", "Oğuz", "Doğan" };
        private int _nameCounter;

        // Yaşam katmanı (Adım 18-43): tüm kurallar Core.Life'ta; burası kurar, her karede ilerletir, çizer.
        private LifeSystems _life;
        private BuildService _buildService;
        private StatusHud _statusHud;
        private VillagePanel _villagePanel;
        private readonly Dictionary<int, string> _villagerLocation = new Dictionary<int, string>();
        private double _scheduleTimer;

        // Adlandırılmış yerler (life.json'daki home/work/square anahtarları) harita karolarına çevrilir.
        // Köy arsanın doğusunda (x70-78). Eski kayıtların (48x32) haritasında LegacyPlaces kullanılır.
        private static Dictionary<string, GridPos> Places = new Dictionary<string, GridPos>
        {
            { "home:1", new GridPos(74, 22) }, { "home:2", new GridPos(76, 22) }, { "home:3", new GridPos(74, 29) },
            { "home:4", new GridPos(77, 30) }, { "home:5", new GridPos(72, 21) }, { "home:6", new GridPos(76, 27) },
            { "work:smithy", new GridPos(75, 24) }, { "work:mill", new GridPos(77, 20) }, { "work:herb", new GridPos(73, 31) },
            { "work:pasture", new GridPos(78, 33) }, { "work:market", new GridPos(71, 25) }, { "work:pottery", new GridPos(76, 29) },
            { "square", new GridPos(72, 26) }
        };

        private static readonly Dictionary<string, GridPos> LegacyPlaces = new Dictionary<string, GridPos>
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
        // Menüde "devam et"/"Yükle" yalnızca okunabilir bir kayıt varken görünür; Awake'te ve her başarılı kayıttan sonra güncellenir.
        private bool _hasSave;

        private float HudScale => Mathf.Max(1f, Screen.height / 540f);
        private Rect HudRect => new Rect(8f, 8f, 340f, _hudExpanded
            ? Mathf.Min(560f, Screen.height / HudScale - 16f)
            : (_clock != null && Time.unscaledTime < _messageUntil ? 112f : 88f));

        private void Awake()
        {
            // Kare hızı açıkça sabit: mobilde 30 (pil), masaüstünde 60. Oyun hızı kareden bağımsızdır (Time.deltaTime).
            Application.targetFrameRate = Application.isMobilePlatform ? 30 : 60;
            WaitPendingWrite();   // önceki sahnenin arka plan kaydı bitmeden kayıt okunmasın
            IsoArt.Reset();
            _content = ContentLoader.Load(out var contentErrors);
            foreach (var e in contentErrors) Debug.LogError($"İçerik hatası: {e}");
            Debug.Log($"İçerik yüklendi: {_content.Items.Count} eşya tanımı.");

            LoadedGame loaded = null;
            if (PendingLoad != null)
            {
                try { loaded = SaveGame.Load(PendingLoad, _content); }
                catch (System.Exception ex)
                {
                    // Yeni oyun başlar; ama bozuk kayıt ilk otomatik kayıtla silinmesin diye kenara kopyalanır.
                    Debug.LogError($"Kayıt yüklenemedi: {ex.Message}");
                    string kept = SaveFiles.Quarantine(SavePath);
                    ShowMessage(kept != null ? "Kayıt açılamadı. Dosya .corrupt olarak saklandı; yeni oyun başladı." : "Kayıt açılamadı; yeni oyun başladı.", 8f);
                }
                PendingLoad = null;
            }
            _hasSave = SaveFiles.TryRead(SavePath, IsReadableSave, out _, out _);

            // Eski kayıtta tohum yoktur (0): yeni bir ana tohum verilir, o oturumda pazar eski sabit tohumuyla kalır.
            bool legacySeed = loaded != null && loaded.Seed == 0;
            _seed = loaded != null && loaded.Seed != 0 ? loaded.Seed : Thatchwick.Core.Seeds.NewMaster();
            _rngEpoch = loaded != null ? loaded.RngEpoch + 1 : 0;
            _rng = new System.Random(Thatchwick.Core.Seeds.Derive(_seed, 1, _rngEpoch));

            GridMap map;
            if (loaded != null)
            {
                foreach (var w in loaded.Warnings) Debug.LogWarning($"Kayıt: {w}");
                _world = loaded.World;
                map = _world.Map;
                Places = map.Width <= LegacyMapWidth ? LegacyPlaces : Places;   // eski kayıt: eski küçük harita
                if (Thatchwick.Core.World.Regions.IsRegional(map)) { Thatchwick.Core.World.Regions.ApplyPortals(map); Places = Thatchwick.Core.World.Regions.Places(); }
                ClearLegacyObstacles(map);
                _clock = loaded.Clock;
                _lastSavedDay = _clock.DayIndex;
                foreach (var item in _world.Items) if (item.Def.Id == "item.market_stall") _stallId = item.Id;
            }
            else
            {
                map = BuildDemoMap();
                Places = Thatchwick.Core.World.Regions.Places();
                _world = new WorldState(map);
                _clock = new GameClock();
                PlaceDemoItems();
                _world.Map.RestrictOwnership(4, 4, 67, 67); // başlangıç arsası: 64x64
            }

            var viewGo = new GameObject("WorldView");
            viewGo.transform.SetParent(transform, false);
            _view = viewGo.AddComponent<WorldView>();
            _view.Content = _content;
            _regional = Thatchwick.Core.World.Regions.IsRegional(map);
            var farm = Thatchwick.Core.World.Regions.Farm;
            if (_regional) { _region = farm; _view.Build(map, new RectInt(farm.X, farm.Y, farm.W, farm.H)); }
            else _view.Build(map);

            RebuildItemViews();
            _world.Changed += () => _itemsDirty = true;
            _view.LevelChanged += () => _itemsDirty = true;   // görünen kat değişince eşyalar yeniden kurulur
            // Duvar değişince kesitte hangi duvarın alçaldığı değişebilir: asılı eşyalar yeniden çizilir (Yukarı'da gerekmez).
            _world.Map.Changed += () => { if (_walls != null && _walls.Mode != WallMode.Up) _itemsDirty = true; };

            foreach (var item in _world.Items)
                foreach (var tile in item.OccupiedTiles()) _view.ClearTerrainBlock(tile);
            _view.RefreshFloors();
            map.Changed += () => _floorsDirty = true;
            map.RoadsChanged += () => _floorsDirty = true;
            map.ConduitsChanged += () => _floorsDirty = true;
            _world.Changed += () => { if (map.HasConduits) _floorsDirty = true; };    // kapak, kaynak: arkın ıslaklığı
            map.Changed += () =>
            {
                if (_account == null || _account.HasFirst("first:room")) return;
                foreach (var _ in map.Rooms.IndoorRooms) { _account.Record("room", _clock.DayIndex, isWork: false); break; }
            };
            var wallsGo = new GameObject("WallView");
            wallsGo.transform.SetParent(transform, false);
            _walls = wallsGo.AddComponent<WallView>();
            _walls.Init(map, _view, _content);
            _walls.ModeChanged += () => _itemsDirty = true;   // Yukarı / Kesit / Aşağı: asılı eşyalar gizlenir ya da görünür

            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = camGo.AddComponent<Camera>();
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = WorldView.OutsideColor;   // harita kenarındaki solma bu renge karışır

            _ambience = gameObject.AddComponent<Ambience>();
            _ambience.Init(cam);
            _weatherFx = gameObject.AddComponent<WeatherFx>();
            _weatherFx.Init(cam);
            _juice = gameObject.AddComponent<Juice>();

            _input = gameObject.AddComponent<WorldInput>();
            _input.Init(cam, _regional ? Iso.RectBounds(farm.X, farm.Y, farm.W, farm.H) : Iso.MapBounds(map.Width, map.Height));
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
                _household.AddItem("item.compost", 3);
                _household.AddItem("item.salt", 4);
            }
            _storage = new StorageService(_world, _household, _content);
            _effects = new InteractionEffects(_world, _household, _content, seed: Thatchwick.Core.Seeds.Derive(_seed, 2, _rngEpoch)) { Storage = _storage, FamilyYieldChance = s => _life.Upbringing.Recipes.ExtraYield(s) };
            _effects.Applied += (c, i, summary) =>
            {
                if (c.IsVillager) return;
                // Yemek, uyku, yıkanma gibi ihtiyaç işleri (çoğu otonom) hesap XP'si vermez; yalnızca sayaç (rozet) ilerler.
                bool work = !i.RestoresNeed || i.Skill.HasValue;
                if (work) { ShowMessage($"{c.Name}: {summary}"); _account.Record(i.Id, _clock.DayIndex); }
                else _account.Count("act:" + i.Id);
            };
            _plants = new PlantSystem(_world, Thatchwick.Core.Seeds.Derive(_seed, 7, _rngEpoch));
            _animals = new AnimalSystem(_world, _content, seed: Thatchwick.Core.Seeds.Derive(_seed, 3, _rngEpoch)) { SeasonNow = () => _clock.Season, HealthSeed = Thatchwick.Core.Seeds.Derive(_seed, 31, _rngEpoch) };
            if (loaded != null)
            {
                SaveGame.RestoreAnimals(loaded, _animals, _content);
                foreach (var w in loaded.Warnings) if (w.StartsWith("Hayvan")) Debug.LogWarning($"Kayıt: {w}");
            }
            _effects.Animals = _animals;
            _effects.Plants = _plants;
            _plants.HiveActive = hive => _animals.Residents(hive.Id) > 0;   // kovanda koloni varken tozlaştırır
            if (loaded != null) SaveGame.RestoreSoil(loaded, _plants);
            _plants.FamilyOf = id => _content.Items.TryGet(id, out var cd) ? cd.CropFamily : null;   // nöbet aileye bakar
            _mines = new MineSystem(_world, _household, Thatchwick.Core.Seeds.Derive(_seed, 53, 0));   // kazı katı tohumdan: yüklemede aynı
            if (loaded != null) SaveGame.RestoreMines(loaded, _mines);
            _animals.Died += OnAnimalDied;
            _roomMood = new RoomMoodSystem(_world);
            // Pazar teklifleri gün ve tohumdan yeniden üretilir: yüklemeden sonra aynı kalsın diye epoch'suz türetilir.
            _market = new Market(_content, _household, _storage, null, seed: legacySeed ? 17 : Thatchwick.Core.Seeds.Derive(_seed, 4));
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
            _plants.PlantLost += (def, pos, why) =>
            {
                if (why == PlantLoss.Crows) ShowMessage($"Kuzgunlar {def.DisplayName} yedi (korkuluk dikmeyi düşün)");
                else if (why == PlantLoss.Storm) ShowMessage($"Fırtına {def.DisplayName} ekinini yıprattı");
            };
            _heat = new HeatSystem(_world, _clock.Config, null, _clock.TotalDays)
            {
                // Malzeme yalıtımı: taş duvar az, kalas çok kaçırır; çatı stili üst katın çatı kaybını değiştirir.
                // Malzemenin ısı kaybı × perde ya da kumaş duvar örtüsü (27 §4.5).
                EdgeLossFactor = e => (_buildService?.EdgePart(_world.Map.GetEdge(e), _world.Map.GetEdgeStyle(e))?.HeatLoss ?? 1f)
                    * Thatchwick.Core.Build.DecorSystem.EdgeInsulation(_world, e),
                RoofLossFactor = t => BuildParts.Resolve(_content, BuildPartKind.Roof, _world.Map.GetRoofStyle(t) == GridMap.NoRoof ? null : _world.Map.GetRoofStyle(t))?.HeatLoss ?? 1f
            };
            if (loaded != null) _heat.RestoreTemperatures(loaded.Temperatures);
            _buildService = new BuildService(_world, _household, _content) { Storage = _storage, AccountLevel = () => _account.Level, HousingResidents = _animals.Residents, Plants = _plants };
            _buildMode = new BuildMode(_world, _household, _buildService, _content, _view, _input, transform, _walls, _animals);
            _buildMode.Activity += a => _account.Record(a, _clock.DayIndex);

            _population = new Population(firstFreeId: 1, seed: Thatchwick.Core.Seeds.Derive(_seed, 5, _rngEpoch));
            // Yerleştirme denetimi karakterleri bilsin: kimsenin üstüne eşya, ekin ya da mezar konmaz.
            _world.CharacterPositions = LivingPositions;
            _population.NameGenerator = NextName;
            _population.BabyBorn += OnBabyBorn;
            _population.BirthPlace = m => Population.ShelterNear(_world, m.Position);   // helada, tarlada doğan bebek eve
            _family = new FamilyService(_population, _household, _content, seed: Thatchwick.Core.Seeds.Derive(_seed, 6, _rngEpoch));
            _family.Happened += OnFamilyEvent;
            _family.JoinedHousehold += OnJoinedHousehold;

            _life = new LifeSystems(_content, _world, _household, _population, _clock, Thatchwick.Core.Seeds.Derive(_seed, 8, _rngEpoch))
            {
                AccountLevel = () => _account.Level,
                AccountXp = () => _account.Xp
            };
            // Kilitler beceride (karar 28-4): hanenin o becerideki en iyi seviyesi.
            _life.SkillLocks = true;
            _buildService.SkillLevel = _life.BestSkill;
            _animals.SkillLevel = _life.BestSkill;
            _life.Attach(_family);
            _life.Attach(_plants);
            _life.Attach(_heat);
            _life.Attach(_market);
            _life.Attach(_buildService);
            _life.Attach(_effects);
            _life.Attach(_animals);   // attan düşen binici yaralanır, at ürkünce bildirim (Project belgesi 29)
            // Kurt savunması dünyadan okunur: çitli ağıl ve açık havada yanan ateş (belge 31-4).
            if (_life.Wolves != null) { _life.Wolves.Fenced = () => WildWorld.HasPen(_world.Map); _life.Wolves.HasFire = () => WildWorld.HasOutdoorFire(_world); _life.Wolves.ExtraDefense = () => _life.Upbringing.Companions.WolfDefenseBonus(); }   // Sürü Dili (belge 31-8)
            _life.AttachWild(_world.Map);   // haritada dolaşan yaban hayvanları
            // Miras: kurucu (köydeki soy), yadigâr envantere girmez, satılınca geçmişi silinir, komşu haneye giden köylü olur.
            _life.FounderId = () => _dynasty.History.Count > 0 ? _dynasty.History[0].CharacterId : (int?)null;
            _buildService.StoreBlocker = _life.Legacy.StoreBlocker;
            _buildService.PlacedItemSold += _life.Legacy.OnItemSold;
            _life.Legacy.MarriedOutHappened += OnMarriedOut;
            _buildMode.Legacy = _life.Legacy;
            _buildMode.Comfort = _life.Mood;
            _buildMode.Maker = () => _player?.Character;
            _buildMode.Today = () => _clock.TotalDays;
            _buildMode.Keepsakes = _life.Keepsakes;
            _buildMode.Season = () => _clock.Season;
            SetupFarm();
            _life.Log.Added += e => ShowMessage(e.Text, e.Level == NoticeLevel.Info ? 2.5f : 4f);
            _life.Suitors.SpawnPosition = () => NearestWalkable(Places["square"], 8);
            _life.SquarePosition = () => NearestWalkable(Places["square"], 8);   // bunalımda "Meydana kaçtı"
            _life.Suitors.Arrived += c => CreateActorFor(c);
            _statusHud = new StatusHud(_life, _content, () => Commander?.Character, c => _actors.Find(a => a.Character == c)?.Runner, () => _plants.Summary());
            _villagePanel = new VillagePanel(_life, _content, _household, _family, _dynasty, () => Commander?.Character);

            _familyPanel = new FamilyPanel(_population, _family, _dynasty, () => Commander?.Character,
                c => _selected = _actors.Find(a => a.Character == c), c => _actors.Find(a => a.Character == c)?.Runner)
            { Duties = _duties, Legacy = _life.Legacy, Today = () => _clock.TotalDays, Upbringing = _life.Upbringing, House = _household };

            _appearanceCatalog = ContentLoader.LoadAppearance(out var appearanceErrors);
            foreach (var e in appearanceErrors) Debug.LogError($"Görünüm hatası: {e}");
            _population.AppearanceCatalog = _appearanceCatalog;
            _life.Suitors.AppearanceCatalog = _appearanceCatalog;
            // Ölen adlı köylünün (demirci ...) halefi mevsim başında gelir: görünüm ve sahne nesnesi burada verilir.
            _life.Villagers.PlaceResolver = key => ResolvePlace(key);
            _life.Villagers.SuccessorArrived += (c, def) =>
            {
                GiveRandomLook(c);
                CreateActorFor(c);
            };

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
                if (loaded.Dynasty != null)
                {
                    _dynasty.Restore(loaded.Dynasty.HeadId, loaded.Dynasty.History, loaded.Dynasty.Ended);
                    _dynasty.Name = loaded.Dynasty.Name;
                    _dynasty.SettlementName = loaded.Dynasty.SettlementName;
                }
                else if (_player != null) _dynasty.Found(_player.Character, _clock.TotalDays);
                _dynastyEnded = _dynasty.Ended || _player == null;
                _selected = _player;
                _input.enabled = true;
                ShowMessage(loaded.Warnings.Count > 0 ? $"Kayıt yüklendi ({loaded.Warnings.Count} uyarı, konsola bak)" : "Kayıt yüklendi");
                return;
            }

            // Maud (NPC) rastgele görünümle hemen oluşur; oyuncunun karakteri yaratma ekranından gelir.
            var maud = new Character(2, "Maud", new GridPos(33, 31), false) { Sex = Sex.Female };
            GiveRandomLook(maud);
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

            _creator = new CharacterCreator(_appearanceCatalog, seed: 5) { Content = _content, DaysPerSeason = _clock.Config.DaysPerSeason };
            _namingSettlement = true;                       // önce yerleşimin adı, sonra karakter
            _settlementDraft = Settlement.DefaultName;
            _input.enabled = false;
        }

        /// <summary>
        /// Eski kayıtlardaki test engellerini kaldırır: zemin katında eşya olmadan engelli duran karolar.
        /// Arazi engelini yalnızca eski test haritası koyuyordu (mezar kazısı kendi engelini hemen geri alır).
        /// </summary>
        private void ClearLegacyObstacles(GridMap map)
        {
            int cleared = 0;
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                {
                    var p = new GridPos(x, y);
                    if (!map.IsBlocked(p) || _world.GetItemAt(p) != null) continue;
                    map.SetBlocked(p, false);
                    cleared++;
                }
            if (cleared > 0) Debug.Log($"Kayıt: {cleared} eski test engeli kaldırıldı.");
        }

        // Test haritası: kaynaklı doğal gölet (hep dolu; böcekli, içinde sazan, kerevit ve alabalık).
        private static GridMap BuildDemoMap()
        {
            var map = new GridMap(MapWidth, MapHeight);
            for (int x = 8; x < 20; x++)
                for (int y = 44; y < 52; y++)
                    map.SetFloor(new GridPos(x, y), FloorType.Water);
            var natural = new GridPos(8, 44);
            map.SetSpring(natural);
            map.RestorePond(natural, 1f, spring: true, insects: true, filledDays: 30, new[]
            {
                new KeyValuePair<string, int>("item.fish_carp", 8),
                new KeyValuePair<string, int>("item.fish_crayfish", 10),
                new KeyValuePair<string, int>("item.fish_trout", 6),
                new KeyValuePair<string, int>("item.fish_pike", 2)
            });

            BuildStarterHouse(map);
            // Çiftlik yolu doğu kenarına, şehir geçidine kadar uzar; şehir haritası kurulur (belge 39).
            for (int x = 73; x <= 95; x++) map.SetRoad(new GridPos(x, 40), RoadType.Path);
            map.SetRoad(new GridPos(94, 41), RoadType.Path); map.SetRoad(new GridPos(95, 41), RoadType.Path);
            Thatchwick.Core.World.Regions.BuildTerrain(map);
            Thatchwick.Core.World.Regions.Naturalize(map);
            Thatchwick.Core.World.Regions.Apply(map);

            // Yol bulma testinden kalan çit hattı (x=32) ve 60 rastgele engel kaldırıldı (görsel analiz, 18.11.2026):
            // dünyada karşılıkları yoktu, haritada koyu lekeler olarak görünüyordu. Kaya ve çalı görselleri gelince
            // (art/queue/357) engeller eşya olarak dönecek.
            return map;
        }

        // Başlangıç eşyalarını yerleştirir.
        // Başlangıç evi (BuildStarterHouse): iç 28-36 x 26-32; zemin kat mutfak ve oturma, üst kat yatak odaları, bodrum kiler.
        // Oyuncu (31,31), Maud (33,31), kapı içi (32,32) ve merdiven uçları boş kalır.
        private void PlaceDemoItems()
        {
            // --- zemin kat: merdivenler, mutfak, yemek ve oturma ---
            Put("item.stairs_down", 29, 27);              // bodruma (giriş 29,26; iniş 29,29 kat -1)
            Put("item.stairs_up", 35, 27);                // üst kata (giriş 35,26; iniş 35,29 kat 1)
            Put("item.pantry_shelf", 30, 26);
            Put("item.hearth_stone", 32, 26);
            Put("item.cooking_pot", 33, 26);
            Put("item.water_barrel", 36, 26);
            Put("item.rug_wool", 31, 30);
            Put("item.table_wood", 31, 29);
            Put("item.chair_wood", 30, 29);
            Put("item.chair_wood", 33, 29);
            Put("item.basin_ewer", 28, 30);
            Put("item.candlestick", 28, 32);
            Put("item.armchair_cushion", 36, 31);
            Put("item.chest_wood", 36, 32);
            Put("item.stool_wood", 34, 32);
            PutOn("item.candle", 31, 29, 0, "a");
            PutOn("item.flower_pot", 31, 29, 0, "c");
            PutOn("item.clay_cup", 31, 29, 0, "b");

            // --- üst kat: üç yatak, sandıklar, kilimler, okuma köşesi ---
            Put("item.bed_straw", 28, 26, 1);
            Put("item.chest_wood", 30, 26, 1);
            Put("item.rug_wool_small", 28, 27, 1);
            Put("item.bed_straw", 32, 26, 1);
            Put("item.chest_wood", 34, 26, 1);
            Put("item.bed_straw", 28, 32, 1);
            Put("item.rug_woven_large", 30, 29, 1);
            Put("item.shelf_wood", 31, 32, 1);
            Put("item.armchair_cushion", 36, 30, 1);
            Put("item.candlestick", 36, 32, 1);
            Put("item.stool_wood", 33, 32, 1);
            PutOn("item.clay_cup", 31, 32, 1, "a");

            // --- bodrum: kiler ve atölye ---
            Put("item.barrel", 28, 26, -1);
            Put("item.barrel", 28, 27, -1);
            Put("item.salting_barrel", 31, 26, -1);
            Put("item.pantry_shelf", 32, 26, -1);
            Put("item.barrel", 35, 26, -1);
            Put("item.seed_crate", 36, 26, -1);
            Put("item.brew_barrel", 36, 28, -1);
            Put("item.barrel_tub", 36, 30, -1);
            Put("item.workbench", 28, 32, -1);
            Put("item.shelf_wood", 33, 32, -1);
            Put("item.barn_lantern", 32, 30, -1);

            // --- avlu ---
            Put("item.well", 25, 30);
            Put("item.chopping_block", 24, 27);
            Put("item.rain_barrel", 38, 26);
            Put("item.hay_bale", 24, 34);
            Put("item.garden_bench", 29, 35);
            Put("item.stool_wood", 34, 35);
            Put("item.stump_table", 35, 35);
            var stump = _world.GetItemAt(new GridPos(35, 35));
            if (stump != null && _content.Items.TryGet("item.morris_board", out var board)) _world.PlaceOnSurface(board, stump.Id, "a");
            Put("item.scarecrow", 48, 32);                // tarla alanı: 42-56 x 26-38 (boş bırakıldı)

            // --- ağaçlar: meyve bahçesi (kuzeybatı), gölet söğütleri, arsa kenarı ---
            string[] fruit = { "item.tree_apple", "item.tree_pear", "item.tree_cherry", "item.tree_plum" };
            int k = 0;
            for (int y = 10; y <= 19; y += 3)
                for (int x = 9; x <= 21; x += 4) PutGrown(fruit[k++ % fruit.Length], x, y);
            PutGrown("item.forage_berry_bush", 24, 10);
            PutGrown("item.forage_berry_bush", 26, 12);
            PutGrown("item.tree_willow", 7, 42);
            PutGrown("item.tree_willow", 21, 50);
            PutGrown("item.tree_willow", 13, 54);
            var edgeRng = new System.Random(64);
            for (int i = 0; i < 40; i++)
            {
                int side = edgeRng.Next(4), t = 6 + edgeRng.Next(58);
                int ex = side == 0 ? 5 + edgeRng.Next(2) : side == 1 ? 65 + edgeRng.Next(2) : t;
                int ey = side == 2 ? 5 + edgeRng.Next(2) : side == 3 ? 65 + edgeRng.Next(2) : t;
                if (_world.Map.GetRoad(new GridPos(ex, ey)) != RoadType.None) continue;
                PutGrown(edgeRng.Next(3) == 0 ? "item.tree_pine" : "item.tree_birch", ex, ey, quiet: true);
            }

            // Köy yolu ve şehir: dükkân içleri (meslek eşyaları, duvar ve yüzey dekoru), meydan, evler, ağaçlar.
            foreach (var ti in Thatchwick.Core.World.Regions.Items())
            {
                var at = new GridPos(ti.X, ti.Y);
                if (ti.Grown)
                {
                    // Ağaç ve çalı yalnızca açık toprağa: yol, meydan taşı, su, köprü ve bina içi atlanır.
                    if (_world.Map.GetRoad(at) != RoadType.None || _world.Map.GetFloor(at) != FloorType.Grass) continue;
                    PutGrown(ti.Id, ti.X, ti.Y, quiet: true);
                    continue;
                }
                if (ti.Slot != null) { PutOn(ti.Id, ti.X, ti.Y, 0, ti.Slot); continue; }
                if (ti.WallSide >= 0) { PutWall(ti.Id, at, ti.WallSide); continue; }
                Put(ti.Id, ti.X, ti.Y);
                if (ti.Id == "item.market_stall") { var st = _world.GetItemAt(at); if (st != null) _stallId = st.Id; }
            }

            // Arsanın dışındaki yabani meşeler (olgun): herkes kesebilir, fidanlarıyla arsaya ağaç dikilir.
            var rng = new System.Random(13);
            int placed = 0;
            for (int i = 0; i < 400 && placed < 24; i++)
            {
                var p = new GridPos(rng.Next(70, 94), rng.Next(36, 70));
                if (_world.Map.IsBlocked(p) || _world.Map.GetRoad(p) != RoadType.None || !_content.Items.TryGet("item.tree_oak", out var oak)) continue;
                PlantSystem.SetGrown(_world.PlaceItem(oak, p));
                placed++;
            }
        }

        // Başlangıç evi: iki katlı 9x7 ev (iç 28-36 x 26-32), ahşap zemin; altında taş döşeli bodrum.
        // Güneyde kapı (32), her katta pencereler; çatı en üst katın duvarlarından otomatik. Merdiven boşluğu üst katta açık.
        private static void BuildStarterHouse(GridMap map)
        {
            const int x0 = 28, x1 = 36, y0 = 26, y1 = 32;
            for (int z = -1; z <= 1; z++)
            {
                for (int x = x0; x <= x1; x++)
                    for (int y = y0; y <= y1; y++)
                    {
                        var p = new GridPos(x, y, z);
                        if (z != 0) map.SetTile(p, true);
                        map.SetFloor(p, z < 0 ? FloorType.Stone : FloorType.Wood);
                    }
                bool cellar = z < 0;
                for (int x = x0; x <= x1; x++)
                {
                    bool win = !cellar && (x == 30 || x == 34);
                    map.SetEdge(new Edge(x, y0, EdgeAxis.Horizontal, z), win ? EdgeKind.Window : EdgeKind.Wall);
                    var south = z == 0 && x == 32 ? EdgeKind.Door : win ? EdgeKind.Window : EdgeKind.Wall;
                    map.SetEdge(new Edge(x, y1 + 1, EdgeAxis.Horizontal, z), south);
                }
                for (int y = y0; y <= y1; y++)
                {
                    bool win = !cellar && y == 29;
                    map.SetEdge(new Edge(x0, y, EdgeAxis.Vertical, z), win ? EdgeKind.Window : EdgeKind.Wall);
                    map.SetEdge(new Edge(x1 + 1, y, EdgeAxis.Vertical, z), win ? EdgeKind.Window : EdgeKind.Wall);
                }
            }
            // Merdiven boşluğu: üst kata çıkan merdivenin (35,27-28) üstü açık.
            map.SetTile(new GridPos(35, 27, 1), false);
            map.SetTile(new GridPos(35, 28, 1), false);

            // Kapıdan güneye patika, sonra doğuya köye giden yol.
            for (int y = 33; y <= 40; y++) map.SetRoad(new GridPos(32, y), RoadType.Path);
            for (int x = 33; x <= 72; x++) map.SetRoad(new GridPos(x, 40), RoadType.Path);
        }

        private void Put(string id, int x, int y, int z = 0, int rotation = 0)
        {
            if (!_content.Items.TryGet(id, out var def)) { Debug.LogError($"Eşya tanımı yok: {id}"); return; }
            try { _world.PlaceItem(def, new GridPos(x, y, z), rotation); }
            catch (System.Exception ex) { Debug.LogWarning($"Başlangıç eşyası konamadı {id} ({x},{y},{z}): {ex.Message}"); }
        }

        private void PutGrown(string id, int x, int y, bool quiet = false)
        {
            if (!_content.Items.TryGet(id, out var def)) return;
            var check = _world.CanPlaceFloor(def, new GridPos(x, y), 0);
            if (!check.Ok) { if (!quiet) Debug.LogWarning($"Başlangıç ağacı konamadı {id} ({x},{y}): {check.Reason}"); return; }
            PlantSystem.SetGrown(_world.PlaceItem(def, new GridPos(x, y)));
        }

        private void PutWall(string id, GridPos tile, int side)
        {
            if (!_content.Items.TryGet(id, out var def)) { Debug.LogError($"Eşya tanımı yok: {id}"); return; }
            try { _world.PlaceOnWall(def, tile, side); }
            catch (System.Exception ex) { Debug.LogWarning($"Başlangıç duvar eşyası konamadı {id} {tile}: {ex.Message}"); }
        }

        private void PutOn(string id, int x, int y, int z, string slot)
        {
            var surface = _world.GetItemAt(new GridPos(x, y, z));
            if (surface == null || !_content.Items.TryGet(id, out var def)) return;
            try { _world.PlaceOnSurface(def, surface.Id, slot); }
            catch (System.Exception ex) { Debug.LogWarning($"Başlangıç dekoru konamadı {id}: {ex.Message}"); }
        }

        private const float RegionFadeSeconds = 0.7f;

        // Komşu bölgenin eşyaları kenardaki boşluğun üstünde görünmesin: yalnızca etkin bölgenin eşyaları çizilir.
        private void HideOtherRegions()
        {
            if (_region == null) return;
            // Eşya parçaları, gölgeler, süsler: dünya konumundan karoya çevrilip bölgeye bakılır.
            foreach (var go in _itemObjects)
            {
                if (go == null) continue;
                var g = Iso.WorldToGrid(go.transform.position, 0);
                go.SetActive(_region.Contains(new GridPos(Mathf.FloorToInt(g.x), Mathf.FloorToInt(g.y))));
            }
        }

        private void HideOtherRegionActors()
        {
            if (_region == null) return;
            foreach (var a in _actors)
            {
                if (a.View == null || a.Character == null) continue;
                bool here = _region.Contains(a.Character.Position);
                if (a.View.gameObject.activeSelf != here) a.View.gameObject.SetActive(here);
            }
        }

        /// <summary>
        /// Seçili karakter başka bir haritaya (çiftlik, şehir) geçtiyse: zemin o bölge için kurulur, kamera oraya kilitlenir
        /// ve kısa bir kararma olur. Diğer bölgelerdeki hayat sürer (tek dünya, ekran dışı).
        /// </summary>
        private void UpdateRegion()
        {
            var a = _selected ?? _player;
            if (a == null || a.Character == null) return;
            var r = Thatchwick.Core.World.Regions.At(a.Character.Position);
            if (r == null || r == _region) return;
            _region = r;
            _view.SetArea(new RectInt(r.X, r.Y, r.W, r.H));
            _input.SetBounds(Iso.RectBounds(r.X, r.Y, r.W, r.H));
            if (a.View != null) _input.JumpTo(a.View.position);
            _itemsDirty = true;
            _fadeUntil = Time.unscaledTime + RegionFadeSeconds;
            ShowMessage(r.Name);
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
            RebuildItemViewsCore();
            // Yeni gelen eşya (satın alınan, konan, hasatla düşen) kısa bir ezilip esnemeyle belirir.
            if (_juice != null && _seenItemsReady)
                foreach (var kv in _itemViews)
                    if (!_seenItemIds.Contains(kv.Key) && kv.Value != null)
                    {
                        _juice.Pop(kv.Value);
                        _juice.Emit(Juice.Kind.Dust, kv.Value.position, 4, Iso.SortOrder(kv.Value.position.y, 2, 0));
                    }
            _seenItemIds.Clear();
            foreach (var kv in _itemViews) _seenItemIds.Add(kv.Key);
            _seenItemsReady = true;
        }

        // Şenlik hazırlığı (27 §9): yaklaşan şenlikte meydanda iki direk arasında renkli bayrak dizisi. Görsel gelene kadar elmaslar.
        private static readonly Color[] BuntingColors =
        {
            new Color(0.78f, 0.22f, 0.18f), new Color(0.9f, 0.75f, 0.25f), new Color(0.22f, 0.4f, 0.7f), new Color(0.35f, 0.6f, 0.3f)
        };

        private void AddFestiveBunting()
        {
            _buntingShown = _life != null && _life.FestivePrep != null;
            if (!_buntingShown || !_view.IsLevelVisible(0)) return;
            var sq = Places["square"];
            var a = Iso.GridToWorld(sq.X - 2.5f, sq.Y + 0.5f, 0) + new Vector3(0f, 1.1f, 0f);
            var b = Iso.GridToWorld(sq.X + 3.5f, sq.Y + 0.5f, 0) + new Vector3(0f, 1.1f, 0f);
            const int flags = 11;
            for (int i = 0; i <= flags; i++)
            {
                float t = i / (float)flags;
                var p = Vector3.Lerp(a, b, t) - new Vector3(0f, 0.35f * Mathf.Sin(t * Mathf.PI), 0f);   // sarkan ip
                var go = new GameObject("festive_bunting");
                go.transform.SetParent(transform, false);
                go.transform.position = p;
                go.transform.localScale = new Vector3(0.16f, i == 0 || i == flags ? 0.5f : 0.22f, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteMaterials.Diamond;
                sr.sharedMaterial = SpriteMaterials.Unlit;
                sr.color = i == 0 || i == flags ? new Color(0.45f, 0.32f, 0.2f) : BuntingColors[i % BuntingColors.Length];
                sr.sortingOrder = Iso.SortOrder(p.y - 1.1f, 3, 0);
                _itemObjects.Add(go);
            }
        }

        private void RebuildItemViewsCore()
        {
            foreach (var go in _itemObjects) if (go != null) Destroy(go);
            _itemObjects.Clear();
            AddFestiveBunting();
            _itemViews.Clear();
            _plantViews.Clear();
            _heatViews.Clear();

            foreach (var item in _world.Items)
            {
                if (!_view.IsLevelVisible(item.Position.Z)) continue;   // görünmeyen katın eşyası çizilmez
                int level = item.Position.Z;
                var color = ItemColor(item.Def.Id);
                if (item.IsWallMounted)
                {
                    if (WallItemFacesAway(item) && !WallItemHidden(item)) continue;   // Yukarı'da ön duvarın iç yüzü: duvarın arkasında kalır
                    if (WallItemHidden(item))
                    {
                        // Alçak duvar (Kesit'te ön duvar, Aşağı'da hepsi): eşya havada kalmasın; duvarın tepesinde küçük işaret.
                        var mark = Iso.WallMountSpan(item.Position, item.Rotation, item.WallSpan, item.WallBand)
                            - new Vector3(0f, Iso.BandHeight(item.WallBand) - LoweredWallTop, 0f);
                        var msr = AddItemSprite(item, mark, color, 0.12f * Mathf.Max(1, item.WallSpan), Iso.WallMountOrder(item.Position, item.Rotation) + 1, SpriteMaterials.Diamond);
                        msr.color = new Color(color.r, color.g, color.b, 0.75f);
                        continue;
                    }
                    // Bantlı duvar (27): alt, orta, üst yükseklik; geniş eşya kenarlarının ortasında, genişliğince büyük.
                    var wallArt = IsoArt.Item(item, out _);
                    float wscale = (wallArt != null ? 1f : 0.3f) * (item.WallSpan > 1 && wallArt == null ? item.WallSpan : 1f);
                    var wsr = AddItemSprite(item, Iso.WallMountSpan(item.Position, item.Rotation, item.WallSpan, item.WallBand), color, wscale,
                        Iso.WallMountOrder(item.Position, item.Rotation) + item.WallBand, wallArt);
                    wsr.color = ConditionTint(item, wallArt != null ? Color.white : color);
                    continue;
                }
                if (item.IsCeiling)
                {
                    // Duvarlar Aşağı'dayken tavan eşyası da gizlenir (alçak duvarların üstünde havada kalmasın).
                    if (_walls != null && _walls.Mode == WallMode.Down) continue;
                    // Tavan ve kiriş (27): karonun ortasında, tavan yüksekliğinde; kat ya da çatı görünürken zaten çizilmez.
                    var cpos = _view.TileToWorld(item.Position) + new Vector3(0f, Iso.CeilingHeight, 0f);
                    var ceilArt = IsoArt.Item(item, out _);
                    var csr = AddItemSprite(item, cpos, color, ceilArt != null ? 1f : 0.3f, Iso.SortOrder(_view.TileToWorld(item.Position).y, 3, level), ceilArt);
                    csr.color = ConditionTint(item, ceilArt != null ? Color.white : color);
                    continue;
                }
                if (item.IsDecor)
                {
                    var parent = _world.GetItem(item.ParentId.Value);
                    if (parent == null || !parent.TryGetSlotPosition(item.SlotId, out float gx, out float gy)) continue;
                    if (parent.IsWallMounted)
                    {
                        if (WallItemHidden(parent) || WallItemFacesAway(parent)) continue;   // gizli ya da duvar arkasındaki rafın üstündekiler
                        var onShelf = IsoArt.Item(item, out _);
                        var dsr = AddItemSprite(item, Iso.GridToWorld(gx, gy, level) + new Vector3(0f, Iso.BandHeight(parent.WallBand) + 0.1f, 0f), color,
                            onShelf != null ? 1f : 0.25f, Iso.WallMountOrder(parent.Position, parent.Rotation) + parent.WallBand + 1, onShelf);
                        dsr.color = ConditionTint(item, onShelf != null ? Color.white : color);
                        continue;
                    }
                    if (parent.IsCeiling)
                    {
                        if (_walls != null && _walls.Mode == WallMode.Down) continue;
                        // Kiriş kancasında asılı: kancanın biraz altında.
                        var onHook = IsoArt.Item(item, out _);
                        AddItemSprite(item, _view.TileToWorld(parent.Position) + new Vector3(0f, Iso.CeilingHeight - 0.35f, 0f), color,
                            onHook != null ? 1f : 0.25f, Iso.SortOrder(_view.TileToWorld(parent.Position).y, 4, level), onHook);
                        continue;
                    }
                    // Gerçek masanın üst yüzü yer tutucu küpten biraz yüksek.
                    float topPx = IsoArt.Item(parent, out _) != null ? 27f : 24f;
                    var slotDef = parent.Def.FindSlot(item.SlotId);
                    if (slotDef != null && slotDef.IsCover)
                    {
                        // Masa ya da sedir örtüsü (27 D2): eşyanın bütün üstüne serili düz elmaslar, diğer dekorun altında.
                        var coverColor = ConditionTint(item, color);
                        // Tek karoluk yüzeyin (kütük sehpa) üstü karodan dar: örtü taşmasın.
                        float coverScale = parent.Width * parent.Height <= 1 ? 0.62f : 0.95f;
                        foreach (var t in parent.OccupiedTiles())
                        {
                            var cp = _view.TileToWorld(t) + new Vector3(0f, topPx / 32f - 0.02f, 0f);
                            var csr2 = AddItemSprite(item, cp, coverColor, coverScale, Iso.SortOrder(_view.TileToWorld(t).y, 0, level) + 1, SpriteMaterials.Diamond);
                            csr2.color = coverColor;
                        }
                        continue;
                    }
                    var pos = Iso.GridToWorld(gx, gy, level) + new Vector3(0f, topPx / 32f, 0f);
                    var decorArt = IsoArt.Item(item, out _);
                    var dsr2 = AddItemSprite(item, pos, color, decorArt != null ? 1f : 0.35f, Iso.SortOrder(Iso.GridToWorld(gx, gy, level).y, 0, level) + 2, decorArt);
                    if (item.Def.IsFillDecor) dsr2.color = FillTint(item, decorArt != null ? Color.white : color);
                    continue;
                }
                if (item.IsRug)
                {
                    // Halı: yere serili düz elmaslar (zeminin hemen üstünde, eşyaların altında). Halılar üst üste serilir (27):
                    // karodaki sırası çizim sırasını belirler. Döküntü yığını küçük, koyu bir leke.
                    bool debris = item.Def.Id == Thatchwick.Core.Content.Decor.Debris;
                    bool rugFlip = false;
                    var rugArt = debris ? null : IsoArt.Item(item, out rugFlip);
                    if (rugArt != null)
                    {
                        // Gerçek halı görseli (390): tek düz görsel, ayak izinin merkezinde; sırası en alttaki karoya göre.
                        var rc = Iso.GridToWorld(item.Position.X + item.Width * 0.5f, item.Position.Y + item.Height * 0.5f, level);
                        int rugRank = 0;
                        foreach (var tile in item.OccupiedTiles()) rugRank = Mathf.Max(rugRank, _world.RugsAt(tile).IndexOf(item));
                        var rart = AddItemSprite(item, rc, Color.white, 1f, -9990 + Mathf.Clamp(rugRank, 0, 9) + level * Iso.LevelSortStep, rugArt);
                        rart.color = ConditionTint(item, FinishTint(item, Color.white, 0.45f));
                        rart.flipX = rugFlip;
                        continue;
                    }
                    var rugColor = debris ? new Color(0.35f, 0.27f, 0.2f) : ConditionTint(item, FinishTint(item, ItemColor(item.Def.Id), 1f));
                    foreach (var tile in item.OccupiedTiles())
                    {
                        int rank = _world.RugsAt(tile).IndexOf(item);
                        var rsr = AddItemSprite(item, _view.TileToWorld(tile), rugColor, debris ? 0.55f : 0.92f,
                            -9990 + Mathf.Clamp(rank, 0, 9) + level * Iso.LevelSortStep, SpriteMaterials.Diamond);
                        rsr.color = rugColor;
                    }
                    continue;
                }
                bool flip = false;
                var art = item.IsPlant ? null : IsoArt.Item(item, out flip);
                if (art != null)
                {
                    // Tek görsel, ayak izinin merkezinde (pivot = taban elmasının merkezi).
                    var center = Iso.GridToWorld(item.Position.X + item.Width * 0.5f, item.Position.Y + item.Height * 0.5f, level);
                    var sr = AddItemSprite(item, center, Color.white, 1f, Iso.SortOrder(center.y, 0, level), art);
                    AddItemShadow(item, center, level);
                    sr.color = FinishTint(item, Color.white, 0.45f);   // kaplama: gerçek görsel gelene kadar hafif renk
                    sr.flipX = flip;
                    if (item.Def.HeatOutput > 0f) _heatViews.Add((item, sr));
                    continue;
                }
                if (item.IsPlant)
                {
                    var pos = _view.TileToWorld(item.Position);
                    var plantArt = IsoArt.Plant(item);
                    var psr = AddItemSprite(item, pos, plantArt != null ? Color.white : color, PlantScale(item), Iso.SortOrder(pos.y, 1, level), plantArt ?? SpriteMaterials.Tree);
                    var sway = SpriteMaterials.Sway;   // rüzgârda sallanma (vertex shader; yoksa düz)
                    if (sway != null) psr.sharedMaterial = sway;
                    // Ağaç gölgesi: ışık sol üstten, gölge taç boyunca sağ alta uzanır; büyüdükçe büyür.
                    AddShadowObject(pos + new Vector3(0.18f, -0.06f, 0f) * PlantScale(item), 0.95f * PlantScale(item), level);
                    _plantViews.Add((item, _itemObjects[_itemObjects.Count - 1].transform));
                    continue;
                }
                var tinted = FinishTint(item, color, 1f);
                AddItemShadow(item, Iso.GridToWorld(item.Position.X + item.Width * 0.5f, item.Position.Y + item.Height * 0.5f, level), level);
                foreach (var tile in item.OccupiedTiles())
                {
                    var pos = _view.TileToWorld(tile);
                    AddItemSprite(item, pos, tinted, 1f, Iso.SortOrder(pos.y, 0, level));
                }
            }
        }

        // Hasarlı eşya koyu ve soluk, onarılmış hafif soluk, kararmış ayna gri (27-ev-dekorasyonu).
        /// <summary>Kesit modunda alçak duvarın tepesi (dünya birimi; SpriteMaterials.LowWallHeightPx / 32).</summary>
        private const float LoweredWallTop = 0.34f;

        // Duvar eşyası alçak duvarda mı (gizlenir): geniş eşyanın herhangi bir kenarı alçaksa hepsi.
        private bool WallItemHidden(ItemInstance item)
        {
            if (_walls == null || !item.IsWallMounted) return false;
            foreach (var seg in item.WallSegments()) if (_walls.IsLowered(seg.edge)) return true;
            return false;
        }

        // Duvar eşyası kameraya bakmayan yüzde mi (ön duvarın oda içi yüzü): Yukarı modunda duvarın arkasında kalır, çizilmez.
        // Kameraya bakan yüz kenarın A yüzüdür (WallView); eşya karosu B yüzündeyse arkaya bakar.
        private static bool WallItemFacesAway(ItemInstance item)
        {
            if (!item.IsWallMounted) return false;
            foreach (var seg in item.WallSegments()) if (seg.tile == seg.edge.SideB) return true;
            return false;
        }

        // Doldurulan dekor (27 D2): boşken soluk, bayatta kahverengimsi, çürükte yeşilimsi (görsel gelene kadar).
        private Color FillTint(ItemInstance item, Color baseColor)
        {
            var c = ConditionTint(item, baseColor);
            if (!item.HasContents) return Color.Lerp(c, new Color(0.55f, 0.5f, 0.45f), 0.45f);
            int f = Thatchwick.Core.Build.DecorSystem.WorstFreshness(item, _content);
            if (f == Thatchwick.Core.Content.Decor.Rotten) return Color.Lerp(c, new Color(0.35f, 0.45f, 0.2f), 0.6f);
            if (f == Thatchwick.Core.Content.Decor.Stale) return Color.Lerp(c, new Color(0.55f, 0.42f, 0.28f), 0.3f);
            return c;
        }

        private Color ConditionTint(ItemInstance item, Color baseColor)
        {
            var c = baseColor;
            if (item.Condition == Thatchwick.Core.Content.ItemCondition.Damaged) c = Color.Lerp(c, new Color(0.35f, 0.3f, 0.28f), 0.55f);
            else if (item.Condition == Thatchwick.Core.Content.ItemCondition.Repaired) c = Color.Lerp(c, new Color(0.7f, 0.65f, 0.6f), 0.2f);
            if (item.IsTarnished) c = Color.Lerp(c, new Color(0.4f, 0.42f, 0.38f), 0.5f);
            if (_clock != null && Thatchwick.Core.Build.DecorSystem.IsWilted(item, _clock.Season))
                c = Color.Lerp(c, new Color(0.5f, 0.38f, 0.22f), 0.55f);    // solmuş mevsim süsü (D3b): kurumuş kahverengi
            return c;
        }

        // Mobilya kaplamasının rengi (amount: 1 = tam kaplama rengi, küçük = beyaza karışık); kaplama yoksa baseColor.
        private Color FinishTint(ItemInstance item, Color baseColor, float amount)
        {
            if (item.FinishId == null || !_content.Parts.TryGet(item.FinishId, out var part)) return baseColor;
            var c = SpriteMaterials.PartColor(part, baseColor);
            return amount >= 1f ? c : Color.Lerp(Color.white, c, amount);
        }

        // Bitki etiketi: büyüme yüzdesi, ekinde sulu/susuz, meyve ağacında meyve durumu.
        private string PlantLabel(ItemInstance item)
        {
            string name = item.Def.DisplayName;
            if (item.IsGrown)
            {
                if (item.Def.IsFruitTree) return item.FruitReady ? (item.Def.Id == "item.tree_oak" ? $"{name} (mazı hazır)" : $"{name} (meyve hazır)") : $"{name} (olgun)";
                return $"{name} (olgun)";
            }
            string extra = PlantSystem.CanGrowIn(_clock.Season) || item.Position.Z < 0 ? "" : ", kışın büyümez";
            if (item.Def.IsCrop) extra += item.Watered ? ", sulu" : ", susuz";
            return $"{name} (%{item.GrowthFraction * 100f:0}{extra})";
        }

        private static float PlantScale(ItemInstance item) => 0.3f + 0.7f * item.GrowthFraction;

        // ---------------- gölgeler (görsel analiz, 18.11.2026) ----------------

        /// <summary>Gölgenin saydamlığı. Gölgeler zeminin ve halıların üstünde, eşyaların altında çizilir.</summary>
        private const float ShadowAlpha = 0.26f;
        private static int ShadowOrder(int level) => -9980 + level * Iso.LevelSortStep;

        /// <summary>Karakter ve hayvanın altındaki gölge: onunla birlikte hareket eden çocuk nesne.</summary>
        private static SpriteRenderer NewShadow(Transform parent, Vector3 local, float size)
        {
            var go = new GameObject("Shadow");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = local;
            go.transform.localScale = new Vector3(size, size, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteMaterials.Shadow;
            sr.sharedMaterial = SpriteMaterials.Unlit;
            sr.color = new Color(0f, 0f, 0f, ShadowAlpha);
            sr.sortingOrder = ShadowOrder(0);
            return sr;
        }

        /// <summary>Eşyanın ayak izi kadar gölge (halılar ayrı çizilir, gölgesizdir).</summary>
        private void AddItemShadow(ItemInstance item, Vector3 footprintCenter, int level)
        {
            float size = (item.Width + item.Height) * 0.5f * 0.92f;
            AddShadowObject(footprintCenter + new Vector3(0.08f, -0.04f, 0f), size, level);
        }

        private void AddShadowObject(Vector3 pos, float size, int level)
        {
            var go = new GameObject("Shadow");
            go.transform.SetParent(transform, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size, size, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteMaterials.Shadow;
            sr.sharedMaterial = SpriteMaterials.Unlit;
            sr.color = new Color(0f, 0f, 0f, ShadowAlpha);
            sr.sortingOrder = ShadowOrder(level);
            _itemObjects.Add(go);
        }

        // ---------------- gün ışığı ----------------

        /// <summary>Isı vermeyen ama gece parlayan ışıklar (mum, fener, kandil).</summary>
        private static readonly HashSet<string> LightItemIds = new HashSet<string>
        {
            "item.candle", "item.candlestick", "item.torch_wall", "item.oil_lamp_wall", "item.sconce_brass", "item.barn_lantern"
        };

        private void UpdateAmbience()
        {
            if (_ambience == null || _clock == null || _life == null) return;
            var weather = _life.Weather.Today(_clock.TotalDays);
            var target = DayLight.Ambient(HourOfDay, _clock.Season, weather);
            bool building = _buildMode != null && _buildMode.Active;
            if (_weatherFx != null) _weatherFx.Set(weather, building);
            _lights.Clear();
            if (_ambience.Night > 0.02f || _ambienceSnap)
            {
                foreach (var item in _world.Items)
                {
                    bool fire = item.Def.HeatOutput > 0f;
                    if (fire ? !item.IsLit : !LightItemIds.Contains(item.Def.Id)) continue;
                    if (!_view.IsLevelVisible(item.Position.Z) || !_itemViews.TryGetValue(item.Id, out var t) || t == null) continue;
                    _lights.Add(new Ambience.LightSource
                    {
                        Position = t.position + new Vector3(0f, fire ? 0.1f : 0.25f, 0f),
                        Strength = fire ? 0.6f + item.Def.HeatOutput / 140f : 0.5f,
                        Fire = fire,
                        Key = item.Id
                    });
                }
            }
            _ambience.Apply(target, building, _lights, _ambienceSnap);
            _ambienceSnap = false;
        }

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

        // En yakın yürünebilir karo; tercihen meydana yürüyerek ulaşılabilen (eşyaların arasına kapanmış bir cep değil).
        private GridPos NearestWalkable(GridPos near, int maxRadius = 4)
        {
            GridPos? enclosed = null;
            for (int r = 0; r <= maxRadius; r++)
                for (int dy = -r; dy <= r; dy++)
                    for (int dx = -r; dx <= r; dx++)
                    {
                        if (System.Math.Max(System.Math.Abs(dx), System.Math.Abs(dy)) != r) continue;
                        var p = new GridPos(near.X + dx, near.Y + dy);
                        if (!_world.Map.IsWalkable(p)) continue;
                        if (ReachesSquare(p)) return p;
                        if (!enclosed.HasValue) enclosed = p;
                    }
            return enclosed ?? near;
        }

        private bool ReachesSquare(GridPos p)
        {
            var square = Places["square"];
            var goals = new List<GridPos>(9);
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                    goals.Add(new GridPos(square.X + dx, square.Y + dy));
            return Pathfinder.FindPathToAny(_world.Map, p, goals) != null;
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
                GiveRandomLook(c);
                c.LifespanDays = c.AgeConfig.RollLifespan(_rng) + LifeRules.LifespanBonus(_content, c.TraitIds);
                CreateActorFor(c);
            }
        }

        // Yeni doğan ya da gelen kişinin adı: listeden sıradaki, ama yaşayan biriyle aynı ad verilmez (yüklemeden sonra sayaç
        // sıfırlansa da). Liste tükenirse aynı ada sıra eki konur ("Edith II").
        private string NextName(Sex sex)
        {
            var names = sex == Sex.Female ? GirlNames : BoyNames;
            var taken = new HashSet<string>();
            foreach (var c in _population.Members) if (!c.IsDead) taken.Add(c.Name);
            for (int i = 0; i < names.Length; i++)
            {
                string n = names[_nameCounter++ % names.Length];
                if (!taken.Contains(n)) return n;
            }
            string baseName = names[_nameCounter++ % names.Length];
            for (int k = 2; ; k++)
            {
                string n = $"{baseName} {Roman(k)}";
                if (!taken.Contains(n)) return n;
            }
        }

        private static string Roman(int n) => n switch { 2 => "II", 3 => "III", 4 => "IV", 5 => "V", 6 => "VI", 7 => "VII", 8 => "VIII", 9 => "IX", _ => n.ToString() };

        // Yapı denetimleri için: herkesin durduğu karo ve yürürken adım attığı sonraki karo (eşya ayağının altına konmasın).
        private IEnumerable<GridPos> LivingPositions()
        {
            foreach (var c in _population.Members)
                if (!c.IsDead) yield return c.Position;
            foreach (var a in _actors)
                if (a.Character != null && !a.Character.IsDead && a.Runner?.NextTile is GridPos next) yield return next;
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
                if (def == null)
                {
                    // Komşu haneye evlenen ya da orada doğan köylü: eşinin (yoksa annesinin) programına uyar.
                    Character anchor = c.SpouseId.HasValue ? _population.Find(c.SpouseId.Value) : null;
                    if ((anchor == null || anchor.IsDead || _life.Villagers.DefOf(anchor) == null) && c.BirthMotherId.HasValue)
                        anchor = _population.Find(c.BirthMotherId.Value);
                    def = anchor != null && !anchor.IsDead ? _life.Villagers.DefOf(anchor) : null;
                    if (def == null && anchor != null && !anchor.IsDead && anchor.SpouseId.HasValue)
                        def = _life.Villagers.DefOf(_population.Find(anchor.SpouseId.Value));
                    if (def == null) continue;
                }
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
            runner.Autonomous = !c.IsVillager;   // Sims tarzı özgür irade: oyuncu karakteri de kuyruğu boşken ihtiyacını kendisi giderir; komut her zaman önce gelir
        }

        // Komşu haneye evlenen üye köylü olur: komut almaz, eşinin programına uyar.
        private void OnMarriedOut(Character member, Character spouse)
        {
            var actor = _actors.Find(a => a.Character == member);
            if (actor != null)
            {
                actor.Runner.Cancel();
                ApplyVillagerRules(member, actor.Runner);
            }
            if (_selected != null && _selected.Character == member) _selected = _player;
            _villagerLocation.Remove(member.Id);
            ShowMessage($"{member.Name}, {spouse.Name} ile evlenip onun hanesine gitti.", 5f);
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
                case SocialAction.Chat:
                case SocialAction.TellStory: _account.Record("chat", _clock.DayIndex, isWork: false); break;
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
            _life.OnSuccession(_dynasty, result.Heir);
            var heir = _actors.Find(a => a.Character == result.Heir);
            if (heir != null)
            {
                _player = heir;
                _selected = heir;
                heir.Runner.Autonomous = true;   // varis de özgür iradeyle yaşar
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
            var shadow = NewShadow(go.transform, ShadowOffset, 0.42f);

            var actor = new Actor
            {
                Character = character,
                Runner = new JobRunner(character, _world)
                {
                    Effects = _effects, Social = _family, Animals = _animalCare, TileWork = _farmWork,
                    WaterNetworkAccess = () => _water != null ? _water.AllWaterAccess() : WaterNetwork.WaterAccess(_world),
                    PipedWater = it => _water != null && _water.IsSupplied(it),
                    WalkSpeed = () => _animals.RideSpeed(character.Id), DutyProvider = _duties.Next,
                    DutySpeed = () => SkillNames.Factor(_life.BestSkill(SkillType.Stewardship), 0.03)
                },
                View = go.transform,
                Renderer = sr,
                Shadow = shadow
            };
            _life.Attach(actor.Runner);
            character.Accident += c => { if (!c.IsVillager) ShowMessage($"{c.Name} yetişemedi... (tuvalet kazası)"); };
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
            if (_storagePanel != null && _storagePanel.Active && _storagePanel.PanelRect(s).Contains(guiPos)) return true;
            if (_minePanel != null && _minePanel.Active && _minePanel.PanelRect(s).Contains(guiPos)) return true;
            if (_keepsakePanel != null && _keepsakePanel.Active && _keepsakePanel.PanelRect(s).Contains(guiPos)) return true;
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

        // Arka planda süren dosya yazımı: yazımlar sırayla yapılır, yeni bir yazım ya da yükleme öncekini bekler.
        private static System.Threading.Tasks.Task _pendingWrite = System.Threading.Tasks.Task.CompletedTask;

        private static void WaitPendingWrite()
        {
            try { _pendingWrite.Wait(); }
            catch (System.Exception ex) { Debug.LogError($"Kaydetme hatası: {ex.Message}"); }
        }

        /// <param name="background">Günlük otomatik kayıt: durum ana iş parçacığında alınır, dosya arka planda yazılır
        /// (oyun takılmaz). Elle kayıt, uygulamadan çıkış ve duraklatma beklemeden yazar.</param>
        private void SaveNow(bool quiet, bool background = false)
        {
            if (_player == null || _creator != null || _dynastyEnded) return;   // oyun başlamadan ya da bittikten sonra kaydetme
            try
            {
                var data = SaveGame.Capture(_clock, _world, _household, _population.Members, _account, _heat, _market, _family, _dynasty, _animals, _life, _plants, _seed, _rngEpoch, _mines);
                string json = JsonUtility.ToJson(data);
                string path = SavePath;
                // Önce geçici dosyaya yazılır, eski kayıt .bak olarak kalır: yazma yarıda kesilse de bir kayıt sağlam kalır.
                if (background)
                {
                    _pendingWrite = _pendingWrite.ContinueWith(_ =>
                    {
                        try { SaveFiles.WriteAtomic(path, json); }
                        catch (System.Exception ex) { Debug.LogError($"Kaydetme hatası: {ex.Message}"); }
                    }, System.Threading.Tasks.TaskScheduler.Default);
                    _hasSave = true;
                    return;
                }
                WaitPendingWrite();
                SaveFiles.WriteAtomic(path, json);
                _hasSave = true;
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
            WaitPendingWrite();
            try
            {
                // Ana dosya yoksa, okunamıyorsa ya da oyuna yüklenemiyorsa yedeğe düşer. Yükleme burada bir kez denenir:
                // JSON'u geçerli ama içeriği bozuk dosya yeni sahnede sessizce "yeni oyun"a dönmesin.
                if (!SaveFiles.TryRead(SavePath, CanLoadSave, out string json, out bool usedFallback))
                {
                    Debug.LogError("Kayıt okunamadı: geçerli bir kayıt ya da yedek yok.");
                    string kept = SaveFiles.Quarantine(SavePath);
                    ShowMessage(kept != null ? "Kayıt açılamadı; dosya .corrupt olarak saklandı." : "Kayıt okunamadı", 6f);
                    return;
                }
                if (usedFallback) Debug.LogWarning("Ana kayıt okunamadı; yedekten yüklendi.");
                PendingLoad = JsonUtility.FromJson<SaveData>(json);
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

        private bool CanLoadSave(string json)
        {
            try
            {
                var data = JsonUtility.FromJson<SaveData>(json);
                if (data == null) return false;
                SaveGame.Load(data, _content);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Kayıt yüklenemiyor: {ex.Message}");
                return false;
            }
        }

        private static bool IsReadableSave(string json)
        {
            try
            {
                var data = JsonUtility.FromJson<SaveData>(json);
                return data != null && data.map != null && data.map.width > 0 && data.map.height > 0;
            }
            catch (System.Exception)
            {
                return false;
            }
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

        // Köylü, talip ve NPC: rastgele görünüm ve ona uyan gizli genom (çocuklarına geçer).
        private void GiveRandomLook(Character c)
        {
            c.Appearance = _appearanceCatalog.Randomize(_rng, c.Sex);
            c.Genome = Genetics.FromAppearance(c.Appearance, _appearanceCatalog, _rng);
        }

        private LookState LookFor(Actor actor)
        {
            int day = (int)_clock.TotalDays;
            string injury = actor.Character.Injury?.Zone;
            if (actor.Look == null || actor.LookDay != day || actor.LookInjury != injury)
            {
                actor.Look = AgingLook.For(actor.Character, _appearanceCatalog);
                actor.LookDay = day;
                actor.LookInjury = injury;
            }
            return actor.Look;
        }

        // ---- yerleşim adı (yeni oyunun ilk adımı) ----
        private bool _namingSettlement;
        private string _settlementDraft = Settlement.DefaultName;
        private string _settlementError;

        private void DrawSettlementNaming()
        {
            float s = HudScale;
            GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));
            var r = new Rect(Screen.width / s * 0.5f - 180f, Screen.height / s * 0.5f - 90f, 360f, 180f);
            GUILayout.BeginArea(r, GUI.skin.box);
            GUILayout.Label("Hanen hangi yerleşimde yaşayacak?");
            GUILayout.Label("Hayali bir ortaçağ köyü: adını sen koy.");
            _settlementDraft = GUILayout.TextField(_settlementDraft ?? "", Settlement.MaxLength);
            if (!string.IsNullOrEmpty(_settlementError)) GUILayout.Label(_settlementError);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Rastgele ad")) { _settlementDraft = Settlement.Random(_rng); _settlementError = null; }
            if (GUILayout.Button("Devam"))
            {
                _settlementError = Settlement.Validate(_settlementDraft);
                if (_settlementError == null)
                {
                    _dynasty.SettlementName = Settlement.Clean(_settlementDraft);
                    _namingSettlement = false;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUI.matrix = Matrix4x4.identity;
        }

        private void FinishCreation()
        {
            var draft = _creator.Draft;
            var character = draft.CreateCharacter(1, new GridPos(31, 31), true, _appearanceCatalog, _rng);
            _creator.Release();
            _creator = null;
            if (character.TraitIds.Count == 0) character.SetTraits(LifeRules.RollTraits(_content, _rng, 1 + _rng.Next(2)));
            character.LifespanDays += LifeRules.LifespanBonus(_content, character.TraitIds);
            LifeRules.Recompute(character, _content, _life.Village);
            _population.Add(character);
            _player = CreateActorFor(character);
            _selected = _player;
            _dynasty.Found(character, _clock.TotalDays);
            _dynasty.Name = draft.EffectiveDynastyName;
            _life.Injuries.Setting = draft.Accidents;
            _input.enabled = true;
            ShowMessage($"{character.Name} {_dynasty.SettlementName} yerleşimine geldi");
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
            // Çapa kipi: boş çime dokunmak orayı çapalar, yıpranmış yola dokunmak onarır (sıraya eklenir).
            if (_hoeMode && _menuOptions.Count == 0)
            {
                var action = _world.Map.GetRoad(tile) != RoadType.None ? TileAction.Repair : TileAction.Till;
                string why = _farmWork.CanDo(Commander.Character, tile, action);
                if (why != null) { ShowMessage(why); return; }
                if (!Commander.Runner.Queue.TryEnqueue(Job.OnTile(tile, action))) ShowMessage("İş sırası dolu.");
                return;
            }
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
            var wall = BuildMode.PickWallItem(_world, world, _view);
            var floor = _world.GetItemAt(_view.WorldToTile(world - new Vector3(0f, 24f / 32f, 0f))) ?? _world.GetItemAt(_view.WorldToTile(world));
            foreach (var root in new[] { wall, floor })
            {
                if (root == null) continue;
                items.Add(root);
                items.AddRange(_world.GetDecorOn(root.Id));
            }
            var names = new List<string>();
            var tile = _view.WorldToTile(world);
            var visitor = _life?.Roamers?.At(tile);
            if (visitor != null) AddWildOptions(visitor, names);
            var animal = AnimalAt(tile);
            if (animal != null)
            {
                names.Add($"{animal.Name} ({animal.Species.DisplayName}, {AnimalProgress.LineageName(animal.Lineage)} · sv {animal.Level}{(animal.LegendaryTitle != null ? " · " + animal.LegendaryTitle : "")}, dostluk {animal.Friendship:0}{(animal.IsMature ? "" : ", yavru")}{(animal.IsSick || animal.IsCold ? ", " + animal.HealthText.ToLowerInvariant() : "")}{(animal.Species.Rideable ? $", dayanıklılık {animal.Stamina:0}" : "")})");
                AddAnimalOptions(animal);
            }
            foreach (var item in items)
            {
                if ((item.Def.Interactions.Count > 0 || item.Def.AnimalRole == AnimalRoles.Bell || item.Def.WaterGate) && !names.Contains(item.Def.DisplayName)) names.Add(item.Def.DisplayName);
                foreach (var it in item.Def.Interactions)
                    _menuOptions.Add(new MenuOption { Item = item, Interaction = it, Blocked = Commander.Runner.WhyCannotStart(item, it) });
                if (item.Def.WaterGate)
                {
                    var gate = item;
                    _menuOptions.Add(new MenuOption
                    {
                        Label = gate.Toggled ? "Kapağı aç (su geçsin)" : "Kapağı kapat (suyu kes)",
                        Run = () => { gate.Toggled = !gate.Toggled; _water?.Invalidate(); _floorsDirty = true; ShowMessage(gate.Toggled ? "Su kapağı kapandı" : "Su kapağı açıldı"); }
                    });
                }
                if (item.Def.AnimalRole == AnimalRoles.Bell)
                    _menuOptions.Add(new MenuOption
                    {
                        Label = _animals.Released ? "Çanı çal: hayvanları içeri çağır" : "Çanı çal: hayvanları otlağa salıver",
                        Run = () => { _animals.Released = !_animals.Released; ShowMessage(_animals.Released ? "Hayvanlar gündüz otlağa çıkar" : "Hayvanlar barınakta kalır"); }
                    });
            }
            // Su karosu (gölet, dere): yıkan, balık tut, balık at; donmuşsa buz kes. Kazılan gölet yağmur bekler.
            if (items.Count == 0 && animal == null && _world.Map.IsWater(tile))
            {
                var pond = _world.Map.PondAt(tile);
                if (!pond.IsFilled) names.Add($"Boş gölet (su %{pond.Level * 100f:0}, yağmurla dolar)");
                else
                {
                    var fishText = new List<string>();
                    foreach (var f in pond.Fish) fishText.Add($"{f.Value} {(_content.Items.TryGet(f.Key, out var fd) ? fd.DisplayName : f.Key)}");
                    names.Add($"{(_world.Map.WaterFrozen ? "Donmuş gölet" : "Gölet")}{(pond.Spring ? " (kaynaklı)" : $" (su %{pond.Level * 100f:0})")}"
                        + $"{(pond.Insects ? ", böcekli" : ", böcek yok")}, balık: {(fishText.Count > 0 ? string.Join(", ", fishText) : "yok")}");
                }
                var waterTile = tile;
                foreach (var species in _farmWork.FishSpecies())
                {
                    int have = _household.Count(species);
                    if (have <= 0) continue;
                    var sp = species;
                    _menuOptions.Add(new MenuOption
                    {
                        Label = $"Gölete at: {_content.Items.Get(sp).DisplayName} ({have})",
                        Blocked = _farmWork.CanDo(Commander.Character, waterTile, TileAction.Stock, sp),
                        Run = () => Commander.Runner.Command(Job.OnTile(waterTile, TileAction.Stock, sp))
                    });
                }
                foreach (var (label, act) in new[] { ("Suya gir, yıkan", TileAction.Bathe), ("Kıyıdan balık tut", TileAction.Fish), ("Buz kes", TileAction.CutIce) })
                {
                    if (act == TileAction.CutIce && !_world.Map.WaterFrozen) continue;
                    var a = act;
                    _menuOptions.Add(new MenuOption
                    {
                        Label = label,
                        Blocked = _farmWork.CanDo(Commander.Character, waterTile, a),
                        Run = () => Commander.Runner.Command(Job.OnTile(waterTile, a))
                    });
                }
            }
            AddMineOptions(tile, names);
            _menuTitle = string.Join(", ", names);
        }

        // Arsa kazısı (kat -2): kayaya dokununca kaz ya da işaretle; açık karoda destek, ışık ve su.
        private void AddMineOptions(GridPos tile, List<string> names)
        {
            if (tile.Z > -2) return;
            var m = _mines.Get(MineSystem.PlotId);
            var t = m?.Get(tile.X, tile.Y);
            if (t == null) return;
            var c = Commander.Character;
            var target = tile;
            if (!t.IsOpen)
            {
                string vein = m.TouchesOpen(tile.X, tile.Y) && t.Ore != null ? $", damar: {(_content.Items.TryGet(t.Ore, out var od) ? od.DisplayName : t.Ore)}" : "";
                names.Add($"{MineSystem.RockName(t.Rock)} (sertlik {MineSystem.Hardness(t.Rock)}{vein}){(t.Creaking ? ", ÇATIRDIYOR" : "")}");
                _menuOptions.Add(new MenuOption { Label = "Kaz", Blocked = _farmWork.CanDo(c, target, TileAction.Dig), Run = () => { if (!Commander.Runner.Queue.TryEnqueue(Job.OnTile(target, TileAction.Dig))) ShowMessage("İş sırası dolu."); } });
                _menuOptions.Add(new MenuOption { Label = t.Marked ? "İşareti kaldır" : "Kazılacak diye işaretle (görevli kazar)", Run = () => { _mines.Mark(m, target.X, target.Y, !t.Marked); _floorsDirty = true; } });
                return;
            }
            names.Add($"Kazı · su derinliği {m.WaterDepth} (yarın {_mines.TomorrowDepth(m)}){(t.Support != SupportKind.None ? ", destekli" : "")}");
            _menuOptions.Add(new MenuOption { Label = "Ahşap destek kur (2 tahta)", Blocked = _farmWork.CanDo(c, target, TileAction.Support), Run = () => Commander.Runner.Command(Job.OnTile(target, TileAction.Support)) });
            _menuOptions.Add(new MenuOption { Label = "Taş kemer kur (3 yontma taş)", Blocked = _farmWork.CanDo(c, target, TileAction.Support, "stone"), Run = () => Commander.Runner.Command(Job.OnTile(target, TileAction.Support, "stone")) });
            foreach (var light in new[] { "item.tallow_candle", "item.beeswax_candle", "item.clay_lamp", "item.torch" })
            {
                if (_household.Count(light) <= 0) continue;
                var l = light;
                _menuOptions.Add(new MenuOption { Label = $"Işık koy: {_content.Items.Get(l).DisplayName}", Blocked = _farmWork.CanDo(c, target, TileAction.Light, l), Run = () => Commander.Runner.Command(Job.OnTile(target, TileAction.Light, l)) });
            }
            if (m.Water > 0) _menuOptions.Add(new MenuOption { Label = "Kovayla su boşalt", Blocked = _farmWork.CanDo(c, target, TileAction.Bail), Run = () => Commander.Runner.Command(Job.OnTile(target, TileAction.Bail)) });
        }

        private void RunOption(MenuOption o)
        {
            if (o.Run != null) { if (o.Blocked == null) o.Run(); else ShowMessage(o.Blocked); return; }
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
                string name = o.Label ?? o.Interaction.DisplayName;
                string label = o.Blocked == null ? name : $"{name} ({o.Blocked})";
                if (GUILayout.Button(label)) { RunOption(o); _menuOpen = false; }
                GUI.enabled = true;
            }
            if (GUILayout.Button("Buraya yürü")) { Commander.Runner.Command(Job.MoveTo(_menuTile)); _menuOpen = false; }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        // Çiftlik ve hayvancılık: barınak odası konforu, otlak, binek, mekanizmalar, görevli aile üyeleri.
        private void SetupFarm()
        {
            _animals.Temperature = p => _heat.TemperatureAt(p);
            _animals.CanGoOut = () => HourOfDay >= 6.0 && HourOfDay < 20.0 && _life.CalmToday;   // gündüz ve fırtına yokken otlakta
            _animals.RiderPosition = id =>
            {
                var c = _population.Find(id);
                return c != null && !c.IsDead ? c.Position : (GridPos?)null;
            };
            _animals.FindCharacter = id => _population.Find(id);
            // Yavrulama ustalığı: hanenin en iyi hayvancısı (dal ustalığı bir seviye sayılır).
            _animals.HusbandryLevel = () =>
            {
                int best = 1;
                foreach (var c in _population.Members)
                    if (!c.IsDead && !c.IsVillager) best = System.Math.Max(best, c.Skills.GetLevel(SkillType.AnimalHusbandry) + c.Modifiers.QualityLevels);
                return best;
            };
            _animals.Born += (mother, baby) =>
            {
                ShowMessage($"{mother.Species.DisplayName} yavruladı: {baby.Name}");
                foreach (var c in _population.Members) if (!c.IsVillager && !c.IsDead) c.AddMoodlet(MoodCatalog.NewFoal);
            };
            _effects.Season = () => _clock.Season;
            _effects.Household = () => _population.Members;
            _market.Trader = () => Commander?.Character;
            _animals.FellSick += a => ShowMessage($"{a.Name} hastalandı: barınağına hayvan merhemi sür, sıcak ve altlıklı tut");
            _animals.Recovered += a => ShowMessage($"{a.Name} iyileşti");
            _animals.Dismounted += (a, riderId) => ShowMessage($"{_population.Find(riderId)?.Name} attan indi ({a.Name} kapıda bekliyor)");
            _animalCare = new AnimalCare(_animals, _household);
            _life?.Attach(_animals, _animalCare);   // can yoldaşı bağı (belge 31-8)
            _animalCare.Done += (c, a, act) =>
            {
                switch (act)
                {
                    case AnimalAction.Mount: ShowMessage($"{c.Name} {a.Name} sırtında (hız x{_animals.RideSpeed(c.Id):0.#})"); _account.Record("ride", _clock.DayIndex); break;
                    case AnimalAction.Saddle: ShowMessage($"{a.Name} eyerlendi"); break;
                    case AnimalAction.Unsaddle: ShowMessage($"{a.Name}: eyer çıkarıldı"); break;
                    case AnimalAction.Brush: ShowMessage($"{c.Name} {a.Name} tımar etti (dostluk {a.Friendship:0})"); _account.Record("brush", _clock.DayIndex); break;
                    default: ShowMessage($"{c.Name} {a.Name} sevdi (dostluk {a.Friendship:0})"); _account.Record("pet_animal", _clock.DayIndex); break;
                }
                if (act == AnimalAction.Pet || act == AnimalAction.Brush) c.AddMoodlet(MoodCatalog.PetLove);
            };
            _storagePanel = new StoragePanel(_life.Spoilage, _household, _content);
            _farmWork = new FarmWork(_world)
            {
                HoeSpeed = c => _life.Tools.SpeedFor("hoe"), House = _household, Content = _content,
                Season = () => _clock.Season, TemperatureAt = p => _heat.TemperatureAt(p), RodSpeed = c => _life.Tools.SpeedFor("rod")
            };
            _farmWork.Build = _buildService;
            _life.Attach(_farmWork);   // kıyıdan ilk balık hatırası (D3b)
            _farmWork.Plants = _plants;
            _farmWork.Mine = new MineWork(_mines);
            _farmWork.Sown += (c, t, seed) => _account.Count("act:sow");
            SetupMines();
            _farmWork.Worked += (c, t, note) => { ShowMessage($"{c.Name}: {note}"); _account.Count("act:water"); };
            _farmWork.Tilled += (c, t) => _account.Count("act:till");
            _farmWork.Repaired += (c, t) => { ShowMessage($"{c.Name} yolu onardı"); _account.Count("act:repair_road"); };
            _effects.TemperatureAt = p => _heat.TemperatureAt(p);   // buz kesmek için don
            _water = new WaterWorks(_world);
            _effects.PipedWater = _water.IsSupplied;
            _view.WetConduit = p => _water.Report.IsWet(p);
            _floorsDirty = true;
            _automation = new FarmAutomation(_world, _household)
            {
                Storage = _storage, Plants = _plants, Build = _buildService, Content = _content, TemperatureAt = p => _heat.TemperatureAt(p),
                DraftAnimals = _animals.DraftAnimals                          // düven: çeki hayvanı (at, öküz)
            };
            _cart = new OxCart(_world, _household, _market, _content)
            {
                DraftAnimals = _animals.DraftAnimals,
                Carter = () => BestMember(SkillType.Carting)
            };
            _duties = new DutyPlanner(_world, _animals, _effects, _animalCare)
            {
                WorkHours = () => HourOfDay >= 7.0 && HourOfDay < 19.0,
                Now = () => _clock.TotalDays,
                TileWork = _farmWork,
                Mines = _mines
            };
            _world.Map.WearMultiplier = RoadWear();
            _life.NewDay += day =>
            {
                if (day % _clock.Config.DaysPerSeason == 0) _itemsDirty = true;   // mevsim döndü: solan mevsim süsleri yeniden boyanır (D3b)
                if ((_life.FestivePrep != null) != _buntingShown) _itemsDirty = true;   // şenlik yaklaştı ya da geçti: meydan bayrakları
                _world.Map.OnNewDayRoads();                                   // kullanılmayan patika çimlenir
                _world.Map.WearMultiplier = RoadWear();                        // çamurlu günde çok, taş ustası varken az aşınır
                _plants.Config.FallowRecovery = (float)(0.02 * SkillNames.Factor(_life.BestSkill(SkillType.SoilCare), 0.1));   // toprak bakımı
                var r = _animals.OnNewDay(_clock.Season, _household, _storage);
                if (r.Collected > 0) ShowMessage($"Toplama sandığı: +{r.Collected} ürün depoya alındı");
                if (r.Births > 0) ShowMessage(r.Births == 1 ? "Ahırda bir yavru doğdu" : $"Ahırda {r.Births} yavru doğdu");
                // Kâhya: görevleri bekleyen işe göre dağıtır.
                foreach (var (who, from, to) in _duties.AutoAssign(_population.Members))
                    ShowMessage($"Kâhya: {who.Name} {(to == DutyKind.None ? "görevden alındı" : DutyPlanner.DutyName(to) + " görevine")}");
                // Kazı: çürüyen destek, su, çatırdama ve göçük (her biri bir gün önceden görünür).
                _mines.NewDay();
                MineShifts();
                _floorsDirty = true;
                int cartEarned = _cart.OnNewDay(_clock.DayIndex, out var cartSold);
                if (cartEarned > 0)
                {
                    int units = 0; foreach (var kv in cartSold) units += kv.Value;
                    ShowMessage($"Öküz arabası pazara indi: {units} mal satıldı, +{cartEarned} akçe");
                }
                if (r.FeedAdded > 0) ShowMessage($"Yem oluğu yemliklere {r.FeedAdded} yem doldurdu");
                // Göletler: yağmur doldurur, sıcakta buharlaşır; böcek gelir, balıklar ürer ya da kurur.
                var todays = _life.Weather.Today(_clock.TotalDays);
                float rain = todays == WeatherType.Storm ? GridMap.StormFill : todays == WeatherType.Rain ? GridMap.RainFill : 0f;
                foreach (var note in _world.Map.OnNewDayPonds(rain, _heat.OutdoorTemperature, _clock.Season == Season.Winter, _pondRng,
                    breeds: id => !(_content.Items.TryGet(id, out var fd) && fd.HasTag("nobreed")),
                    predator: id => _content.Items.TryGet(id, out var fd) && fd.HasTag("predator")))
                    ShowMessage(note);
                // Gübre için tavan yok: depo dolduysa oyuncuyu uyar (sat, tarlaya ser, depoyu büyüt).
                int manure = _household.Count("item.manure");
                if (_storage.Free == 0 && manure >= 5)
                    ShowMessage($"Depo dolu, {manure} gübre yer kaplıyor: pazarda sat, tarlaya ser ya da depoyu büyüt (yoksa yiyecek alınamaz).");
                var water = _water.OnNewDay(_life.RainyToday);
                _floorsDirty = true;
                if (water.DryFixtures > 0) ShowMessage($"Su gelmeyen tesisat: {water.DryFixtures} (yapı modunda su yolunu denetle)");
                var made = _automation.OnNewDay(_clock.Season);
                if (made.Count > 0)
                {
                    var parts = new List<string>();
                    foreach (var kv in made)
                        parts.Add(kv.Key.StartsWith("ekim:") ? $"{kv.Value} tohum ekildi" : $"+{kv.Value} {(_content.Items.TryGet(kv.Key, out var d) ? d.DisplayName : kv.Key)}");
                    ShowMessage("Düzenekler: " + string.Join(", ", parts));
                }
            };
            SetupSkills();
        }

        // Hanenin bu beceride en iyisi (canlı ve hanede); yoksa null.
        private Character BestMember(SkillType skill)
        {
            Character best = null;
            foreach (var c in _population.Members)
            {
                if (c.IsDead || c.IsVillager) continue;
                if (best == null || c.Skills.GetLevel(skill) > best.Skills.GetLevel(skill)) best = c;
            }
            return best;
        }

        // Kazı motorunu oyuna bağlar: kazma kademesi, depo, göçük yarası, yapı modundan kuyu inişi, hazine odası, kaya rengi.
        private void SetupMines()
        {
            _mines.PickTier = c => _life.Tools.Tier("pickaxe");
            _mines.PickSpeed = c => _life.Tools.SpeedFor("pickaxe");
            _mines.Store = (id, n) => _storage.AddUpTo(id, n);
            _mines.CarriedLight = c => _household.Count("item.horn_lantern") > 0 ? 3 : 0;
            _mines.CharactersAt = p =>
            {
                var list = new List<Character>();
                foreach (var a in _actors) if (!a.Character.IsDead && a.Character.Position == p) list.Add(a.Character);
                return list;
            };
            _mines.CaughtInCaveIn += (c, m) =>
            {
                var def = System.Linq.Enumerable.FirstOrDefault(_life.Injuries.Defs, d => d.Id == "cave_in");
                if (def != null && _life.Injuries.Setting != AccidentSetting.Off) _life.Injuries.Injure(c, def);
                c.AddMoodlet(MoodCatalog.CaveInSurvived);
                // Göçükten çıkar: en yakın açık karoya.
                c.Position = NearestWalkable(c.Position, 6);
            };
            _mines.CaveIn += (m, tiles) => { ShowMessage($"Göçük! {tiles.Count} karo molozla doldu (destek kur, molozu yeniden kaz)", 5f); _floorsDirty = true; };
            _mines.Creaked += (m, n) => ShowMessage($"Kazıda {n} karo çatırdıyor: bugün destek kurulmazsa yarın çöker", 5f);
            _mines.SupportRotted += (m, x, y, gone) => ShowMessage(gone ? "Çürüyen bir çatkı çöktü (yenisini kur)" : "Bir ahşap çatkı çürüyor (1 tahta ile yenile)");
            _mines.RelicFound += (c, relic) => { ShowMessage($"{c.Name} bir kalıntı buldu: {(_content.Items.TryGet(relic, out var rd) ? rd.DisplayName : relic)}", 4f); c.AddMoodlet(MoodCatalog.VeinFound); };
            _mines.Dug += (c, m, x, y, got) => { _floorsDirty = true; if (c != null) c.AddMoodlet(MoodCatalog.Dusty); };
            _buildService.DeepDigBlocker = p => _mines.ShaftBlocker(_life.BestSkill(SkillType.Mining));
            _keepsakePanel = new KeepsakePanel(_life.Keepsakes, _world, _clock);
            _minePanel = new MinePanel(_mines, _household, _content)
            {
                Actor = () => Commander?.Character,
                OpenBlocker = () => _life.BestSkill(SkillType.Mining) < 1 ? "Madencilik gerekir." : null
            };
            _minePanel.Message += m => ShowMessage(m, 4f);
            _buildService.DeepShaftOpened += p => { _mines.OpenShaft(p); _floorsDirty = true; };
            _buildService.ProtectedItem = it => _life.Legacy.HeirloomOf(it.Id) != null && MineSystem.InTreasureRoom(_world, it);
            _view.RockTint = p =>
            {
                var m = _mines.Get(MineSystem.PlotId);
                var t = m?.Get(p.X, p.Y);
                if (t == null || t.IsOpen) return null;
                if (!m.TouchesOpen(p.X, p.Y)) return new Color32(46, 38, 33, 255);         // görünmeyen kaya (sis)
                if (t.Creaking) return new Color32(150, 60, 50, 255);
                if (t.Marked) return new Color32(170, 150, 80, 255);
                if (t.Ore != null) return new Color32(110, 96, 120, 255);                  // damar izi
                switch (t.Rock)
                {
                    case RockKind.Soil: return new Color32(98, 78, 58, 255);
                    case RockKind.Soft: return new Color32(104, 96, 86, 255);
                    case RockKind.Hard: return new Color32(78, 76, 80, 255);
                    case RockKind.Rubble: return new Color32(120, 110, 98, 255);
                    default: return new Color32(90, 88, 86, 255);
                }
            };
        }

        // Ocakta sabah vardiyası (ekran dışı): maden görevli yetişkinler işaretli karoları kazar.
        private void MineShifts()
        {
            var mine = _mines.Get(MineSystem.MineId);
            if (mine == null) return;
            foreach (var c in _population.Members)
            {
                if (c.IsDead || c.IsVillager || c.Duty != DutyKind.Mine || !DutyPlanner.CanTakeDuty(c, DutyKind.Mine)) continue;
                int dug = _mines.Shift(mine, c, 0.25);
                if (dug > 0) ShowMessage($"{c.Name} ocakta {dug} karo kazdı");
            }
        }

        // İkinci dalga beceriler (çiftlik sistemleri kurulduktan sonra bağlanır).
        private void SetupSkills()
        {
            // İkinci dalga beceriler: hanenin en iyisi düzeneklere, depoya, söküme ve yollara etki eder.
            _automation.EngineeringLevel = () => _life.BestSkill(SkillType.Engineering);
            _storage.CartingLevel = () => _life.BestSkill(SkillType.Carting);
            _buildService.SalvageLevel = () => _life.BestSkill(SkillType.Salvage);
            _buildService.Salvaged += list =>
            {
                int units = 0;
                var parts = new List<string>();
                foreach (var kv in list) { units += kv.Value; parts.Add($"+{kv.Value} {(_content.Items.TryGet(kv.Key, out var d) ? d.DisplayName : kv.Key)}"); }
                Commander?.Character.GainSkillXp(SkillType.Salvage, 3 * units);
                ShowMessage("Geri kazanım: " + string.Join(", ", parts));
            };
            _buildService.Furnished += item => Commander?.Character.GainSkillXp(SkillType.Decorating, 2);
            _duties.Issued += (c, job) => _player?.Character.GainSkillXp(SkillType.Stewardship, 1);   // hane reisi görevleri yönetir
        }

        // Yol aşınması: yağmurda x1,5; taş ustalığı seviye başına %4 azaltır.
        private float RoadWear() => (float)((_life.RainyToday ? 1.5 : 1.0) / SkillNames.Factor(_life.BestSkill(SkillType.Masonry), 0.04));

        // Dokunulan karodaki hayvan (yakın olanı); yoksa null.
        private Animal AnimalAt(GridPos tile)
        {
            Animal best = null;
            foreach (var a in _animals.Animals)
            {
                if (a.IsDead || a.Position.Z != tile.Z) continue;
                if (Mathf.Abs(a.Position.X - tile.X) + Mathf.Abs(a.Position.Y - tile.Y) > 1) continue;
                if (best == null || a.Position == tile) best = a;
            }
            return best;
        }

        // Hayvan seçenekleri: sev / tımar, eyerle, eyeri çıkar, bin.
        private void AddAnimalOptions(Animal a)
        {
            var c = Commander;
            void Opt(AnimalAction act, string label)
            {
                string why = _animalCare.CanDo(c.Character, a.Id, act);
                _menuOptions.Add(new MenuOption
                {
                    Label = $"{label} ({a.Name})", Blocked = why,
                    Run = () => c.Runner.Command(Job.WithAnimal(a.Id, act))
                });
            }
            Opt(a.Species.Rideable ? AnimalAction.Brush : AnimalAction.Pet, a.Species.Rideable ? "Tımar et" : "Sev");
            if (a.Species.Rideable)
            {
                if (a.Saddle == null) Opt(AnimalAction.Saddle, "Eyerle");
                else
                {
                    Opt(AnimalAction.Mount, "Bin");
                    Opt(AnimalAction.Unsaddle, "Eyeri çıkar");
                }
            }
        }

        // Yaban ziyaretçisi seçenekleri: sakin yaklaş (sev), yayla vur (av yayı gerekir; köpek yardım eder).
        private void AddWildOptions(WildVisitor v, List<string> names)
        {
            var c = Commander;
            names.Add($"{v.Species.Name}{(v.Young ? " yavrusu" : "")} (yaban)");
            if (c == null) return;
            int id = v.Id;
            _menuOptions.Add(new MenuOption
            {
                Label = $"Sakin yaklaş, sev ({v.Species.Name})",
                Blocked = v.Species.Pettable ? null : $"{v.Species.Name} sevilmez; uzaktan izle.",
                Run = () => ShowMessage(_life.Roamers.TryPet(id, c.Character))
            });
            if (v.Species.Huntable)
                _menuOptions.Add(new MenuOption
                {
                    Label = $"Yayla vur ({v.Species.Name})",
                    Blocked = _household.Count("item.hunting_bow") <= 0 ? "Av yayı gerekli (pazardan)." : c.Character.Stage < AgeStage.Teen ? "Çocuk avlanmaz." : null,
                    Run = () =>
                    {
                        bool hound = false;
                        foreach (var a in _animals.Animals) if (!a.IsDead && a.Species.Id == "animal.dog" && a.IsMature && a.ShyDays <= 0) { hound = true; break; }
                        var r = _life.Roamers.Shoot(id, c.Character, _household, hound);
                        if (r.Success && r.SpeciesId == "wild.wolf") _life.Wolves?.OnWolfHunted();
                        ShowMessage(r.Message);
                    }
                });
        }

        private sealed class WildView { public Transform T; public SpriteRenderer R; public int Dir = IsoArt.DefaultDirection; }
        private readonly Dictionary<int, WildView> _wildViews = new Dictionary<int, WildView>();

        // Yaban ziyaretçileri: görseli gelene kadar renkli yer tutucu küp.
        private void UpdateWildViews()
        {
            var roam = _life?.Roamers;
            if (roam == null) return;
            var alive = new HashSet<int>();
            foreach (var v in roam.Visitors)
            {
                alive.Add(v.Id);
                if (!_wildViews.TryGetValue(v.Id, out var w))
                {
                    var go = new GameObject(v.Species.Name);
                    go.transform.SetParent(transform, false);
                    w = new WildView { T = go.transform, R = go.AddComponent<SpriteRenderer>() };
                    w.R.sharedMaterial = SpriteMaterials.Unlit;
                    _wildViews[v.Id] = w;
                }
                Vector3 pos = _view.TileToWorld(v.Position);
                if (v.NextTile.HasValue)
                {
                    var to = _view.TileToWorld(v.NextTile.Value);
                    w.Dir = IsoArt.DirectionIndex(new Vector2(to.x - pos.x, to.y - pos.y));
                    pos = Vector3.Lerp(pos, to, (float)v.StepProgress);
                }
                w.T.position = pos;
                w.R.enabled = _view.IsLevelVisible(v.Position.Z);
                var real = IsoArt.Animal(v.Species.Id, w.Dir);
                float scale = v.Young ? 0.7f : 1f;
                if (real != null) w.R.sprite = real;
                else { w.R.sprite = SpriteMaterials.Cube(AnimalColor(v.Species.Id)); scale *= AnimalScale(v.Species.Id); }
                w.T.localScale = new Vector3(scale, scale, 1f);
                w.R.sortingOrder = Iso.SortOrder(pos.y, 1, v.Position.Z);
            }
            var gone = new List<int>();
            foreach (var kv in _wildViews) if (!alive.Contains(kv.Key)) gone.Add(kv.Key);
            foreach (var id in gone) { if (_wildViews[id].T != null) Destroy(_wildViews[id].T.gameObject); _wildViews.Remove(id); }
        }

        private void OnAnimalDied(Animal a)
        {
            ShowMessage(a.CauseOfDeath == AnimalDeath.Starvation ? $"{a.Name} açlıktan öldü (barınağa yem koy)"
                : a.CauseOfDeath == AnimalDeath.Sickness ? $"{a.Name} hastalıktan öldü (merhem sürülmedi)"
                : $"{a.Name} yaşlılıktan öldü");
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
                case "animal.ox": return new Color(0.42f, 0.32f, 0.25f);
                case "wild.rabbit": case "wild.squirrel": case "wild.roe_deer": return new Color(0.6f, 0.45f, 0.3f);
                case "wild.fox": return new Color(0.85f, 0.45f, 0.15f);
                case "wild.boar": case "wild.hedgehog": case "wild.rat": return new Color(0.35f, 0.3f, 0.27f);
                case "wild.crow": case "wild.owl": case "wild.sparrow": case "wild.stork": return new Color(0.2f, 0.2f, 0.22f);
                case "wild.frog": return new Color(0.4f, 0.6f, 0.3f);
                case "wild.butterfly": return new Color(0.95f, 0.85f, 0.3f);
                case "wild.firefly": return new Color(1f, 1f, 0.5f);
                case "animal.horse": return new Color(0.45f, 0.3f, 0.2f);
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
                case "animal.cow": case "animal.ox": return 0.5f;
                case "wild.butterfly": case "wild.firefly": case "wild.sparrow": case "wild.frog": return 0.12f;
                case "wild.rabbit": case "wild.squirrel": case "wild.hedgehog": case "wild.rat": case "wild.crow": return 0.18f;
                case "wild.owl": case "wild.fox": case "wild.stork": return 0.26f;
                case "wild.roe_deer": case "wild.boar": return 0.38f;
                case "animal.horse": return 0.55f;
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
                    v.Shadow = NewShadow(go.transform, new Vector3(0.06f, -0.03f, 0f), 0.6f);
                    _animalViews[a.Id] = v;
                }
                Vector3 pos = _view.TileToWorld(a.Position);
                var rider = a.RiderId != 0 ? _actors.Find(x => x.Character.Id == a.RiderId) : null;
                if (rider != null && rider.View != null) pos = rider.View.position - new Vector3(0f, 0.02f, 0f);   // binicinin altında, onunla birlikte
                else if (a.NextTile.HasValue)
                {
                    var to = _view.TileToWorld(a.NextTile.Value);
                    v.Dir = IsoArt.DirectionIndex(new Vector2(to.x - pos.x, to.y - pos.y));
                    pos = Vector3.Lerp(pos, to, a.StepProgress);
                }
                v.T.position = pos;
                v.R.enabled = _view.IsLevelVisible(a.Position.Z);
                var real = IsoArt.Animal(a.Species.Id, v.Dir);
                float scale = a.IsMature ? 1f : 0.7f;
                if (real != null) { v.R.sprite = real; v.R.color = Color.white; }
                else { v.R.sprite = SpriteMaterials.Cube(AnimalColor(a.Species.Id)); v.R.color = Color.white; scale *= AnimalScale(a.Species.Id); }
                v.T.localScale = new Vector3(scale, scale, 1f);
                v.R.sortingOrder = Iso.SortOrder(pos.y, 1, a.Position.Z);
                if (v.Shadow != null) { v.Shadow.enabled = v.R.enabled && rider == null; v.Shadow.sortingOrder = ShadowOrder(a.Position.Z); }
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
            if (kb != null && kb.kKey.wasPressedThisFrame) _storagePanel.Toggle();
            if (kb != null && kb.mKey.wasPressedThisFrame) _minePanel.Toggle();
            if (kb != null && kb.jKey.wasPressedThisFrame) _keepsakePanel?.Toggle();
            if (kb != null && kb.hKey.wasPressedThisFrame) { _hoeMode = !_hoeMode; ShowMessage(_hoeMode ? "Çapa kipi: boş çime dokun, çapalasın" : "Çapa kipi kapalı"); }
            if (kb != null && kb.vKey.wasPressedThisFrame) ToggleVillage();
            if (kb != null && kb.nKey.wasPressedThisFrame) _statusHud.ToggleLog();
            if (kb != null && kb.lKey.wasPressedThisFrame) _labelsAll = !_labelsAll;
            if (kb != null && kb.tabKey.wasPressedThisFrame) _hudExpanded = !_hudExpanded;

            // Canlı modda görünen kat, komut alan kişinin bulunduğu kata uyar; yapı modunda oyuncu seçer.
            if (_buildMode != null && !_buildMode.Active && Commander != null) _view.ViewLevel = Commander.Character.Position.Z;

            float dt = Mathf.Min(Time.deltaTime, MaxFrameSeconds);

            // Biyoloji ve iş ilerlemesi oyun saatinde işler; hız çarpanı GameClock'ta uygulanır.
            double gameDays = _clock.Advance(dt);
            if (_clock.DayIndex != _lastSavedDay) { _lastSavedDay = _clock.DayIndex; SaveNow(quiet: true, background: true); }
            Simulate(gameDays);
        }

        // Aynı karede kaç eşya ya da duvar değişirse değişsin (kışın donan ekinler, oda kurma, sabah çıkan yabani bitkiler)
        // zemin ve eşya görünümleri bir kez yeniden kurulur.
        private void LateUpdate()
        {
            if (_view == null || _world == null) return;
            if (_regional) UpdateRegion();
            if (_floorsDirty) { _floorsDirty = false; _view.RefreshFloors(); }
            if (_itemsDirty) { _itemsDirty = false; RebuildItemViews(); if (_regional) HideOtherRegions(); }
            if (_regional) HideOtherRegionActors();
            if (_creator == null) UpdateAmbience();
            UpdateJuice();
        }

        /// <summary>
        /// Animasyon partisi 1 (0 gen): ocak dumanı ve kıvılcımı, düşen yaprak, rüzgâr gücü, akçe sayacı.
        /// Karakterin nefesi ve iş vuruşu <see cref="UpdateView"/>'da.
        /// </summary>
        private void UpdateJuice()
        {
            if (_juice == null || _clock == null) return;
            _juice.Paused = _creator != null;
            _juice.SpeedScale = _clock.EffectiveSpeed;
            if (_juice.Paused) return;
            float dt = Mathf.Min(Time.deltaTime, 0.1f);
            var weather = _life != null ? _life.Weather.Today(_clock.TotalDays) : WeatherType.Clear;
            bool building = _buildMode != null && _buildMode.Active;

            // Rüzgâr: piksel cinsinden en büyük kayma. Fırtına ağaçları belirgin eğer; yapı modunda durur.
            float wind = weather == WeatherType.Storm ? 3f : weather == WeatherType.Rain ? 1.6f : weather == WeatherType.Snow ? 0.6f : 1f;
            if (_clock.Season == Season.Autumn) wind += 0.5f;
            Shader.SetGlobalFloat("_TwWind", building ? 0f : wind);

            // Yanan ocak: duman yükselir, arada kıvılcım. Yağmurda duman daha sık (ıslak odun).
            foreach (var (heatItem, heatView) in _heatViews)
            {
                if (heatView == null || !heatItem.IsLit) continue;
                var top = heatView.transform.position + new Vector3(0f, 0.55f, 0f);
                int order = heatView.sortingOrder + 2;
                float smokeRate = weather == WeatherType.Rain || weather == WeatherType.Storm ? 1.6f : 1.0f;
                if (Random01() < dt * smokeRate) _juice.Emit(Juice.Kind.Smoke, top + new Vector3(0f, 0.15f, 0f), 1, order);
                if (Random01() < dt * 1.4f) _juice.Emit(Juice.Kind.Spark, top - new Vector3(0f, 0.25f, 0f), 1, order);
            }

            // Ağaçlardan yaprak: sonbaharda sık, fırtınada her mevsim (kışın çıplak dal, yaprak yok).
            float leafRate = _clock.Season == Season.Autumn ? 0.08f : (weather == WeatherType.Storm && _clock.Season != Season.Winter ? 0.05f : 0f);
            if (leafRate > 0f)
            {
                _juice.LeafColor = _clock.Season == Season.Autumn
                    ? (Random01() < 0.5f ? new Color(0.84f, 0.5f, 0.18f, 1f) : new Color(0.72f, 0.32f, 0.14f, 1f))
                    : new Color(0.42f, 0.6f, 0.26f, 1f);
                foreach (var (plant, view) in _plantViews)
                {
                    if (view == null || plant.Def.IsCrop) continue;
                    float s = PlantScale(plant);
                    if (s < 0.6f || Random01() >= dt * leafRate * s) continue;
                    _juice.Emit(Juice.Kind.Leaf, view.position + new Vector3(0f, 1.3f * s, 0f), 1, Iso.SortOrder(view.position.y, 3, plant.Position.Z));
                }
            }

            // Akçe sayacı: değişince yuvarlanarak sayar, artışta yeşil, azalışta kırmızı yanar.
            float real = _household != null ? _household.Akce : 0f;
            if (float.IsNaN(_akceShown)) _akceShown = real;
            if (Mathf.Abs(real - _akceShown) >= 0.5f)
            {
                if (Time.unscaledTime > _akceFlashUntil - 0.6f) { _akceFlashUp = real > _akceShown; }
                _akceFlashUntil = Time.unscaledTime + 0.8f;
                float step = Mathf.Max(1f, Mathf.Abs(real - _akceShown) * 6f * Time.unscaledDeltaTime);
                _akceShown = Mathf.MoveTowards(_akceShown, real, step);
            }
            else _akceShown = real;
        }

        private readonly System.Random _fxRng = new System.Random(7);
        private float Random01() => (float)_fxRng.NextDouble();

        /// <summary>Editörde oyun testi için zamanı ileri sarar (küçük adımlarla gerçek simülasyon).</summary>
        private void FastForward(double days)
        {
            const double step = 0.02;
            for (double t = 0; t < days && !_dynastyEnded; t += step)
            {
                _clock.Restore(_clock.TotalDays + step, _clock.RequestedSpeed);
                Simulate(step);
            }
            _ambienceSnap = true;
            ShowMessage($"{days:0} gün ileri sarıldı");
        }

        /// <summary>Dış sıcaklık bunun altındayken su donar.</summary>
        private const float FrozenBelow = -1f;
        private readonly System.Random _pondRng = new System.Random(41);
        /// <summary>Yağmurda açık havada gün başına ıslaklık (yaklaşık iki saatte sırılsıklam).</summary>
        private const double RainSoakPerDay = 10.0;

        private void Simulate(double gameDays)
        {
            _heat.Tick(gameDays, _clock.TotalDays);
            _plants.Tick(gameDays, _clock.Season);
            _market.Tick(gameDays, _clock.TotalDays, _clock.Season);
            _animals.Tick(gameDays);
            _mines.Tick(gameDays);
            _roomMood.Tick(gameDays, _population.Members);
            UpdateAnimalViews();
            UpdateWildViews();
            foreach (var (plant, view) in _plantViews)
                if (view != null) { float s = PlantScale(plant); view.localScale = new Vector3(s, s, 1f); }
            foreach (var (heatItem, heatView) in _heatViews)
                if (heatView != null) { var lit = IsoArt.Item(heatItem, out _); if (lit != null && heatView.sprite != lit) heatView.sprite = lit; }
            foreach (var a in _actors)
                a.Character.AmbientTemperature = a.Character.IsVillager ? 15f : _heat.TemperatureAt(a.Character.Position);
            // Su donar: dondurucu soğukta gölet buz tutar (üstünden yürünür, buz kesilir, delikten balık tutulur).
            _world.Map.WaterFrozen = _heat.OutdoorTemperature <= FrozenBelow;
            // Yağmurda açık havadaki ıslanır (yalnızca hane; köylüler damda sayılır).
            if (_life.RainyToday)
                foreach (var a in _actors)
                {
                    var c = a.Character;
                    if (c.IsVillager || c.IsDead) continue;
                    var room = _world.Map.Rooms.RoomAt(c.Position);
                    if (room == null || !room.IsIndoor) c.Soak((float)(gameDays * RainSoakPerDay));
                }
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
                TickIdleShow(actor, gameDays);
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
            if (_animals != null && _animals.MountOf(actor.Character.Id) != null) pos += new Vector3(0f, 0.35f, 0f);   // at sırtında
            // Yürürken adım sallantısı: gövde iki piksel iner çıkar, gölge yerde kalır (gerçek yürüme animasyonu gelene kadar).
            bool moving = next.HasValue;
            float mdx = pos.x - actor.LastPos.x, mdy = pos.y - actor.LastPos.y;
            float moved = Mathf.Sqrt(mdx * mdx + mdy * mdy);
            if (moving && moved < 1f) actor.WalkPhase += moved / WalkStepUnits * Mathf.PI;
            else if (!moving) actor.WalkPhase = 0f;
            actor.LastPos = pos;
            // Gerçek yürüme kareleri varsa kodla sallantı gerekmez (kareler zaten iniş çıkışı taşır).
            bool framed = moving && IsoArt.CharacterWalk(actor.Character, actor.Direction, 0f) != null;
            float bob = moving && !framed ? Mathf.Round(Mathf.Abs(Mathf.Sin(actor.WalkPhase)) * WalkBobPixels) / ArtPixelsPerUnit : 0f;
            // Animasyon partisi 1: adım tozu, boştayken nefes, eşyada çalışırken vuruş (kare animasyonu gelene kadar).
            float squash = 0f;   // + ezilme (geniş, alçak), - esneme
            int seed = actor.Character.Id;
            int sortOrder = Iso.SortOrder(pos.y, 1, actor.Character.Position.Z);
            if (moving)
            {
                int stepIdx = Mathf.FloorToInt(actor.WalkPhase / Mathf.PI);
                if (stepIdx != actor.LastStep)
                {
                    actor.LastStep = stepIdx;
                    if (!framed) squash = 0.04f;   // ayak yere basınca kısa ezilme (kare animasyonunda gerekmez)
                    if (_juice != null && Random01() < 0.45f && _animals?.MountOf(actor.Character.Id) == null)
                        _juice.Emit(Juice.Kind.Dust, pos + new Vector3(0f, 0.02f, 0f), 1, sortOrder - 1);
                }
            }
            else if (actor.Runner.IsUsingItem && actor.Character.Stage != AgeStage.Baby)
            {
                var job = actor.Runner.Queue.Current;
                if (!ReferenceEquals(job, actor.WorkJob))
                {
                    actor.WorkJob = job;
                    actor.WorkIcon = job != null ? ActivityDescriber.DescribeJob(job, _world).Icon : null;
                }
                string icon = actor.WorkIcon;
                bool beats = icon == "chop" || icon == "farm" || icon == "craft" || icon == "cook" || icon == "work";
                // Gerçek etkileşim kareleri (018c-e) varsa onlar oynar; yoksa kodla vuruş.
                actor.ActionFrame = ActionFrameFor(actor, icon, seed);
                if (actor.ActionFrame != null && _juice != null)
                {
                    float cyc = ActionCycle(seed);
                    if (beats && cyc < actor.LastBeat)   // döngü başında işe göre parçacık
                    {
                        var at = pos + new Vector3(0f, 0.2f, 0f);
                        if (icon == "chop") _juice.Emit(Juice.Kind.Chip, at, 3, sortOrder + 1);
                        else if (icon == "farm") _juice.Emit(Juice.Kind.Soil, pos, 2, sortOrder + 1);
                        else if (icon == "cook") _juice.Emit(Juice.Kind.Steam, pos + new Vector3(0f, 0.6f, 0f), 1, sortOrder + 1);
                    }
                    actor.LastBeat = cyc;
                }
                else if (beats)
                {
                    float rate = icon == "chop" ? 1.3f : icon == "cook" ? 0.9f : 1.1f;
                    float beat = Juice.WorkBeat(seed, rate);
                    if (beat < 0.16f) { bob = -Juice.Px; squash = 0.06f; }        // vuruş anı: çöker
                    else if (beat > 0.75f) { bob = Juice.Px; squash = -0.03f; }   // hazırlık: kalkar
                    if (beat < actor.LastBeat && _juice != null)                  // yeni vuruş
                    {
                        var at = pos + new Vector3(0f, 0.2f, 0f);
                        switch (icon)
                        {
                            case "chop": _juice.Emit(Juice.Kind.Chip, at, 3, sortOrder + 1); break;
                            case "farm": _juice.Emit(Juice.Kind.Soil, pos, 2, sortOrder + 1); break;
                            case "cook": _juice.Emit(Juice.Kind.Steam, pos + new Vector3(0f, 0.6f, 0f), 1, sortOrder + 1); break;
                            case "craft": _juice.Emit(Juice.Kind.Chip, at, 1, sortOrder + 1); break;
                            default: _juice.Emit(Juice.Kind.Dust, pos, 1, sortOrder - 1); break;
                        }
                    }
                    actor.LastBeat = beat;
                }
            }
            else
            {
                // Nefes: göğüs 1 px kabarır (dikey esneme); herkes kendi evresinde.
                if (Juice.BreathOffset(seed) > 0f) squash = -0.018f;
            }
            actor.View.position = pos + new Vector3(0f, bob, 0f);
            // Merdivende yarıdan sonra hedef katın bandına geçer; iki kattan biri görünürse karakter görünür.
            int fromLevel = actor.Character.Position.Z;
            int toLevel = next.HasValue ? next.Value.Z : fromLevel;
            int level = toLevel != fromLevel && actor.Runner.StepProgress > 0.5f ? toLevel : fromLevel;
            actor.Renderer.enabled = _view.IsLevelVisible(fromLevel) || _view.IsLevelVisible(toLevel);
            // Gerçek görsel: yürüdüğü yöne bakar, yaş evresi değişince seti değişir.
            // Kare animasyonu (018a/018b) varsa yürürken onu oynat: tam döngü = iki adım (WalkPhase iki π).
            var walkFrame = moving ? IsoArt.CharacterWalk(actor.Character, actor.Direction, actor.WalkPhase / (2f * Mathf.PI)) : null;
            var real = walkFrame ?? (actor.Runner.IsUsingItem && !moving ? actor.ActionFrame : null) ?? IsoArt.Character(actor.Character, actor.Direction);
            var wanted = real != null ? real : PersonSprites.Get(actor.Character.Appearance, LookFor(actor));
            if (wanted != actor.LastSprite) { actor.Renderer.sprite = wanted; actor.LastSprite = wanted; }
            // Ekranda daha aşağıdaki (öndeki) nesne üste çizilir; aynı karoda karakter eşyanın önünde.
            actor.Renderer.sortingOrder = Iso.SortOrder(pos.y, 1, level);
            if (actor.Shadow != null)
            {
                actor.Shadow.enabled = actor.Renderer.enabled;
                actor.Shadow.sortingOrder = ShadowOrder(level);
            }
            float scale;
            if (real != null)   // gerçek setler zaten çocuk/yetişkin boyunda çizildi; bebek çocuk setinin küçüğü
                scale = actor.Character.Stage == AgeStage.Baby ? 0.6f : (actor.Character.Stage == AgeStage.Teen ? 0.94f : 1f);   // ergen yetişkin setinin biraz küçüğü
            else
            {
                scale = actor.Character.Stage == AgeStage.Baby ? 0.6f : (actor.Character.Stage == AgeStage.Child ? 0.8f : (actor.Character.Stage == AgeStage.Teen ? 0.92f : 1f));
            }
            actor.View.localScale = new Vector3(scale * (1f + squash * 0.5f), scale * (1f - squash), 1f);
            if (actor.Shadow != null) actor.Shadow.transform.localPosition = ShadowOffset - new Vector3(0f, bob / Mathf.Max(0.01f, scale), 0f);
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

        private static string SkillName(SkillType s) => SkillNames.Name(s);

        private static string SkillsText(Character c)
        {
            var parts = new System.Text.StringBuilder();
            foreach (SkillType t in System.Enum.GetValues(typeof(SkillType)))
            {
                int level = c.Skills.GetLevel(t);
                var apt = c.Skills.GetAptitude(t);
                if (level <= 1 && c.Skills.GetXp(t) <= 0 && apt == Aptitude.None) continue;
                if (parts.Length > 0) parts.Append(" · ");
                // Yatkınlık (★ yatkın, ★★ çok yatkın), doygunluk ("yoruldu"), yaşlıda mevsim kullanımı (karar 28).
                parts.Append($"{SkillName(t)} {level}{(apt == Aptitude.Major ? "★★" : apt == Aptitude.Minor ? "★" : "")}");
                if (c.Skills.IsSaturated(t)) parts.Append(" (bugün yoruldu)");
                else if (c.Stage == AgeStage.Elder && c.Skills.PacingEnabled && !c.Skills.UsedThisSeason(t)) parts.Append(" (bu mevsim çalışılmadı)");
            }
            return parts.Length == 0 ? "" : parts.ToString();
        }

        private static string StageName(AgeStage s) => AgeStages.Name(s);

        // Kurt riski, öksüz yavru, Doğa Defteri ve yarışma kaydı (belge 29, 31). Basit IMGUI; asıl arayüz PixelLab ekranlarıyla gelecek.
        private void DrawNatureAndContests(Character handler)
        {
            if (_life == null) return;
            if (_life.Wolves != null)
            {
                var (wolves, defense) = _life.Wolves.Risk(_clock.Season);
                string warn = _life.Wolves.RaidInDays >= 0 ? $" · kurtlar {(_life.Wolves.RaidInDays <= 0 ? "bu gece" : $"{_life.Wolves.RaidInDays} gün içinde")} gelebilir" : "";
                if (wolves > 0 || warn.Length > 0) GUILayout.Label($"   Kurt riski: {wolves} kurt / savunma {defense}{warn}");
            }
            var pending = _life.Adoptions.Pending;
            if (pending != null)
            {
                var sp = WildCatalog.Get(pending);
                GUILayout.BeginHorizontal();
                GUILayout.Label($"   Öksüz {sp?.Name.ToLowerInvariant()} yavrusu:");
                if (GUILayout.Button("Sahiplen")) { _life.Adoptions.Decide(true, _life.Journal); ShowMessage($"{sp?.Name} yavrusu bahçeye yerleşti."); }
                if (GUILayout.Button("Doğaya bırak")) { _life.Adoptions.Decide(false, _life.Journal); ShowMessage("Yavru ormana döndü."); }
                GUILayout.EndHorizontal();
            }
            GUILayout.Label($"   Doğa Defteri: {_life.Journal.SeenCount}/{NatureJournal.Total} tür");
            foreach (var a in _animals.Animals)
            {
                ContestKind kind;
                if (a.Species.Rideable) kind = ContestKind.HorseRace;
                else if (a.Species.Id == "animal.cow" && a.IsFemale) kind = ContestKind.BeautyContest;
                else if (a.Species.Draft && !a.IsFemale) kind = ContestKind.LogPull;
                else continue;
                if (_life.Contests.IsRegistered(a)) { GUILayout.Label($"   {a.Name}: yarışmaya kayıtlı"); continue; }
                string block = AnimalContests.Blocker(kind, a, handler);
                if (block != null) continue;
                double chance = AnimalContests.EstimateWinChance(kind, a, handler, (int)(_clock.DayIndex / _clock.Config.DaysPerYear), out _);
                if (GUILayout.Button($"   {a.Name}: yarışa yaz ({AnimalContests.EntryFee} akçe · Bahisçiler: %{chance * 100:0})"))
                {
                    var err = _life.Contests.Register(kind, a, handler, _household);
                    ShowMessage(err ?? $"{a.Name} yarışa yazıldı; şenlik günü {handler.Name} götürecek.");
                }
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

        // Boşta gösterisi (belge 30): ustalaşılmış kişisel eşyasıyla bekleyen hane üyesi arada bir gösteri yapar.
        private const double IdleShowEveryDays = 0.08;
        private readonly Dictionary<int, double> _idleShowTimer = new Dictionary<int, double>();

        private void TickIdleShow(Actor actor, double gameDays)
        {
            var c = actor.Character;
            if (c.IsVillager || c.IsDead || c.Stage == AgeStage.Baby || !actor.Runner.IsIdle) { _idleShowTimer.Remove(c.Id); return; }
            _idleShowTimer.TryGetValue(c.Id, out var t);
            t += gameDays;
            if (t < IdleShowEveryDays) { _idleShowTimer[c.Id] = t; return; }
            _idleShowTimer[c.Id] = 0;
            var show = _life.Upbringing.IdleShow(c);
            if (show != null && (_selected == actor || show.Watchers > 0)) ShowMessage(show.Text);
        }

        private static string ActivityText(JobRunner runner)
        {
            if (runner.IsIdle) return "Boşta";
            return runner.IsUsingItem ? "Kullanıyor" : "Yürüyor";
        }

        private void OnGUI()
        {
            if (_clock == null) return;
            if (Time.unscaledTime < _fadeUntil)
            {
                // Harita geçişi: kısa kararma (Stardew gibi).
                GUI.color = new Color(0f, 0f, 0f, Mathf.Clamp01((_fadeUntil - Time.unscaledTime) / RegionFadeSeconds));
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            if (_creator != null)
            {
                if (_namingSettlement) DrawSettlementNaming(); else _creator.OnGUI();
                if (Time.unscaledTime < _messageUntil)
                {
                    float ms = HudScale;
                    GUI.matrix = Matrix4x4.Scale(new Vector3(ms, ms, 1f));
                    GUI.Label(new Rect(8f, Screen.height / ms - 96f, Screen.width / ms - 260f, 40f), _message);
                    GUI.matrix = Matrix4x4.identity;
                }
                if (_hasSave)
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
            { var prevSkin = GUI.skin; GUI.skin = UiSkin.Skin; try { _buildMode.OnGUI(s); } finally { GUI.skin = prevSkin; } }   // 391 arayüz kiti
            var prevPanelSkin = GUI.skin; GUI.skin = UiSkin.Skin;   // 391 tema tüm panellere (belge 33)
            try { _marketPanel.OnGUI(s);
            _storagePanel.OnGUI(s);
            _minePanel.OnGUI(s);
            _keepsakePanel?.OnGUI(s);
            _familyPanel.OnGUI(s);
            _villagePanel.OnGUI(s); } finally { GUI.skin = prevPanelSkin; }
            DrawBalloons(s);
            _statusHud.LeftInset = HudRect.xMax;
            _statusHud.OnGUI(s);
            DrawInteractionMenu();
            if (_dynastyEnded) DrawDynastyEnded(s);

            // Sol panel (görsel analiz, 18.11.2026): varsayılan küçük; ihtiyaç listesi, beceriler ve ayarlar "Ayrıntı" (Tab) altında.
            GUILayout.BeginArea(HudRect, GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Yıl {_clock.YearIndex + 1} · {SeasonName(_clock.Season)} · Gün {_clock.DayOfSeason + 1}/{_clock.Config.DaysPerSeason}");
            GUILayout.FlexibleSpace();
            var prevColor = GUI.color;
            foreach (int speed in GameClock.AllowedSpeeds)
            {
                GUI.color = speed == _clock.RequestedSpeed ? new Color(1f, 0.9f, 0.55f) : prevColor;
                if (GUILayout.Button($"{speed}x", GUILayout.Width(34f))) _clock.SetSpeed(speed);
            }
            GUI.color = prevColor;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (HudToggle("Yapı (B)", _buildMode.Active)) ToggleBuild();
            if (HudToggle("Pazar (P)", _marketPanel.Active)) ToggleMarket();
            if (HudToggle("Kiler (K)", _storagePanel.Active)) _storagePanel.Toggle();
            if (HudToggle("Ocak (M)", _minePanel.Active)) _minePanel.Toggle();
            if (HudToggle("Aile (F)", _familyPanel.Active)) ToggleFamily();
            if (HudToggle("Köy (V)", _villagePanel.Active)) ToggleVillage();
            if (_keepsakePanel != null && HudToggle("Hatıra (J)", _keepsakePanel.Active)) _keepsakePanel.Toggle();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            var akceColor = GUI.contentColor;
            if (Time.unscaledTime < _akceFlashUntil) GUI.contentColor = _akceFlashUp ? new Color(0.6f, 1f, 0.55f) : new Color(1f, 0.55f, 0.5f);
            GUILayout.Label($"Akçe {(float.IsNaN(_akceShown) ? _household.Akce : Mathf.RoundToInt(_akceShown))}", GUILayout.Width(80f));
            GUI.contentColor = akceColor;
            GUILayout.Label(new GUIContent($"İtibar: {_life.Upbringing.Reputation.TierName}", $"{_life.Upbringing.Reputation.Points} puan · satış {_life.Upbringing.Reputation.SellBonus() * 100:+0.#;-0.#;0}%"), GUILayout.Width(110f));
            if (HudToggle("Etiketler (L)", _labelsAll)) _labelsAll = !_labelsAll;
            if (GUILayout.Button("Kaydet")) SaveNow(quiet: false);
            if (HudToggle(_hudExpanded ? "Ayrıntı ▴" : "Ayrıntı ▾", _hudExpanded)) _hudExpanded = !_hudExpanded;
            GUILayout.EndHorizontal();
            if (Time.unscaledTime < _messageUntil) GUILayout.Label(_message);

            if (_hudExpanded)
            {
                _hudScroll = GUILayout.BeginScrollView(_hudScroll);
                foreach (var actor in _actors)
                {
                    var c = actor.Character;
                    GUILayout.Label(
                        $"{c.Name} ({(c.IsDead ? "Öldü" : StageName(c.Stage))}{(c.IsPregnant ? ", hamile" : "")}, {c.AgeDays:0}g): Açlık {c.Needs.Get(NeedType.Hunger):0} · Enerji {c.Needs.Get(NeedType.Energy):0} · Isınma {c.Needs.Get(NeedType.Warmth):0}{(c.Wetness > 0.05f ? $" · Islak %{c.Wetness * 100f:0}" : "")}");
                    if (c.ComfortNeedsActive && !c.IsDead)
                        GUILayout.Label($"   Keyif {c.Needs.Get(NeedType.Fun):0} · Hijyen {c.Needs.Get(NeedType.Hygiene):0} · Tuvalet {c.Needs.Get(NeedType.Bladder):0} · Ruh hali: {MoodText(c)}");
                    if (!c.IsDead) GUILayout.Label($"   {ActivityText(actor.Runner)}");
                    var skills = SkillsText(c);
                    if (skills.Length > 0) GUILayout.Label($"   Beceri: {skills}");
                    if (c == Commander?.Character) DrawNatureAndContests(c);
                }

                if (Application.isEditor && _maud != null && _player != null && !_maud.Character.IsDead && GUILayout.Button("Test: Maud hamile (2 günde doğurur)"))
                {
                    _maud.Character.PregnancyDays = 2;
                    if (!_maud.Character.TryStartPregnancy(_player.Character.Id)) ShowMessage("Maud şu an hamile kalamaz");
                    else ShowMessage("Maud hamile");
                }
                if (GUILayout.Button(IsoArt.Enabled ? "Görsel: gerçek (G)" : "Görsel: yer tutucu (G)")) ToggleArt();
                // Çiftlik: çapa kipi, binek, hayvan çanı.
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(_hoeMode ? "Çapa kipi: açık (H) · çim çapala, yol onar" : "Çapa kipi (H)")) _hoeMode = !_hoeMode;
                var mount = Commander != null ? _animals.MountOf(Commander.Character.Id) : null;
                if (mount != null && GUILayout.Button($"{mount.Name} sırtından in")) { _animals.Dismount(Commander.Character.Id); ShowMessage($"{Commander.Character.Name} attan indi"); }
                if (mount != null)
                {
                    // Dörtnala (29-2): at 2 · Binicilik 2; dayanıklılık biter.
                    string gallopBlock = HorseAbilities.Blocker(HorseAbility.Gallop, mount, Commander.Character);
                    string gallopLabel = gallopBlock != null ? $"Dörtnala ({gallopBlock})" : mount.Galloping ? $"Dörtnala: açık · {mount.Stamina:0}" : $"Dörtnala · {mount.Stamina:0}";
                    GUI.enabled = gallopBlock == null;
                    if (GUILayout.Button(gallopLabel))
                    {
                        if (!_animals.SetGallop(Commander.Character.Id, !mount.Galloping)) ShowMessage(mount.Stamina <= 0 ? $"{mount.Name} yoruldu; dinlenmeli ya da ikram ver." : mount.ShyDays > 0 ? $"{mount.Name} ürkek; bugün dörtnala olmaz." : "Dörtnala olmaz.");
                    }
                    GUI.enabled = true;
                    // Engel atlama (29-2): yandaki çit ya da alçak eşya; başarıda at ve binici öteye iner.
                    foreach (var spot in JumpObstacles.Find(_world, Commander.Character.Position))
                    {
                        double jc = HorseAbilities.JumpChance(mount, Commander.Character, spot.Tiles, _life != null && _life.RainyToday);
                        GUI.enabled = jc > 0;
                        if (GUILayout.Button(jc > 0 ? $"Atla: {spot.Label} ({spot.Tiles} karo, %{jc * 100:0})" : $"Atla: {spot.Label} (at ya da Binicilik yetmez)"))
                        {
                            Commander.Runner.Cancel();
                            var o = _animals.TryJump(Commander.Character.Id, spot.Tiles, _life != null && _life.RainyToday);
                            if (o == JumpOutcome.Cleared) { Commander.Character.Position = spot.Landing; _animals.MoveMount(Commander.Character.Id, spot.Landing); ShowMessage($"{mount.Name} {spot.Label} üstünden aştı!"); }
                            else if (o == JumpOutcome.Refused) ShowMessage($"{mount.Name} engelin önünde durdu, ürktü.");
                            else if (o == JumpOutcome.Fell) ShowMessage($"{Commander.Character.Name} attan düştü!");
                        }
                        GUI.enabled = true;
                    }
                    if (_household.Count("item.apple") > 0 && GUILayout.Button("Elma ikram et") && _household.TryTakeItem("item.apple")) { _animals.GiveTreat(mount); ShowMessage($"{mount.Name} elmayı yedi (dayanıklılık {mount.Stamina:0})"); }
                }
                if (GUILayout.Button(_animals.Released ? "Hayvanlar: gündüz otlakta" : "Hayvanlar: barınakta")) _animals.Released = !_animals.Released;
                GUILayout.EndHorizontal();
                GUILayout.Label($"{_dynasty.SettlementName} · {_dynasty.Name} · {_dynasty.CurrentGeneration}. nesil · Baş: {_player?.Character.Name} · Komut alan: {Commander?.Character.Name}");
                GUILayout.BeginHorizontal();
                if (Application.isEditor && GUILayout.Button("Test: +5 gün")) FastForward(5);
                GUI.enabled = _hasSave;
                if (GUILayout.Button("Yükle")) { LoadSave(); return; }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
                // Kaza ayarı (karar 17.11.2026): yaratmada seçilir, burada değiştirilebilir. Eski kayıtta "Seyrek" sayılır.
                GUILayout.BeginHorizontal();
                GUILayout.Label("Kazalar", GUILayout.Width(60f));
                int acc = _life.Injuries.Setting == AccidentSetting.NotSet ? 1 : (int)_life.Injuries.Setting - 1;
                int newAcc = GUILayout.Toolbar(acc, new[] { "Kapalı", "Seyrek", "Normal" });
                if (newAcc != acc || _life.Injuries.Setting == AccidentSetting.NotSet) _life.Injuries.Setting = (AccidentSetting)(newAcc + 1);
                GUILayout.EndHorizontal();
                if (GUILayout.Button("Yeni oyun (yeni karakter yaratılınca eski kaydın üzerine yazılır)"))
                {
                    enabled = false;
                    new GameObject("GameRoot").AddComponent<GameRoot>();
                    Destroy(gameObject);
                    return;
                }
                int next = AccountProgress.XpForLevel(_account.Level + 1);
                GUILayout.Label($"Hesap Sv {_account.Level} · XP {_account.Xp}/{next} · bugünkü iş XP {_account.WorkXpToday(_clock.DayIndex)}/{AccountProgress.DailyWorkCap}");
                DrawHeatHud();
                // Hava bilgisi: kaç gün ilerisi görünür.
                var ahead = _life.WeatherAhead(_life.ForecastDays);
                var names = new List<string>();
                foreach (var w in ahead) names.Add(WeatherSystem.Name(w));
                GUILayout.Label($"Hava ({ahead.Count} gün): {string.Join(" · ", names)}");
                GUILayout.Label($"Eşya tanımı: {_content.Items.Count}");
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }

        /// <summary>Sol paneldeki aç-kapa düğmesi; açıkken vurgulu.</summary>
        private static bool HudToggle(string text, bool on)
        {
            var prev = GUI.color;
            if (on) GUI.color = new Color(1f, 0.9f, 0.55f);
            bool pressed = GUILayout.Button(text);
            GUI.color = prev;
            return pressed;
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
        /// <summary>
        /// Eşya etiketleri (görsel analiz, 18.11.2026): ad ve durum yalnızca fare üstündeki eşyada görünür.
        /// Alt basılıyken ya da etiketler açıkken (L) hepsi görünür. Dikkat isteyen eşyada (su yok, güç yok,
        /// yem bitti, susamış ekin) etiket kapalıyken küçük bir "!" kalır.
        /// </summary>
        private void DrawItemLabels(float guiScale)
        {
            var cam = Camera.main;
            if (cam == null) return;

            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 11,
                    wordWrap = false,
                    padding = new RectOffset(5, 5, 2, 2)
                };
                _alertStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 14,
                    fontStyle = FontStyle.Bold
                };
                _alertStyle.normal.textColor = new Color(1f, 0.62f, 0.25f);
            }

            var kb = UnityEngine.InputSystem.Keyboard.current;
            bool all = _labelsAll || (kb != null && (kb.leftAltKey.isPressed || kb.rightAltKey.isPressed));
            var hovered = all ? null : HoveredItem(cam);

            HashSet<int> net = null;     // sulama ağı bu karede bir kez hesaplanır
            HashSet<int> Net() => net ?? (net = _water.Report.ConnectedItems);
            List<ItemInstance> wheels = null;
            bool Powered(ItemInstance i) => _automation.IsPowered(i, wheels ?? (wheels = _automation.TurningWheels(Net())));
            string Comfort(ItemInstance home) { var r = _animals.ComfortOf(home); return $"konfor {r.Total:0}: {r.Hint()}"; }
            foreach (var item in _world.Items)
            {
                if (item.IsDecor || !_view.IsLevelVisible(item.Position.Z)) continue;
                if (item.IsWallMounted && !_world.IsOverloaded(item) && item.Condition == Thatchwick.Core.Content.ItemCondition.Sound && RottenDecorOn(item) == null && !(all || item == hovered)) continue;
                bool problem = ItemNeedsAttention(item, Net, Powered);
                bool show = all || item == hovered;
                if (!show && !problem) continue;
                Vector3 sp = cam.WorldToScreenPoint(_view.TileToWorld(item.Position) + new Vector3(0f, 1.5f, 0f));
                if (sp.z < 0f) continue;
                float x = sp.x / guiScale;
                float y = (Screen.height - sp.y) / guiScale;
                if (!show) { GUI.Label(new Rect(x - 10f, y - 12f, 20f, 24f), "!", _alertStyle); continue; }
                string label = item.Def.HeatOutput > 0f ? $"{item.Def.DisplayName} ({(item.IsLit ? "yanıyor" : "sönük")})"
                    : item.IsPlant ? PlantLabel(item)
                    : item.Def.IsHousing ? $"{item.Def.DisplayName} ({_animals.Residents(item.Id)}/{item.Def.HousingCapacity}, yem {item.FeedStock:0.#}{(item.StockTotal > 0 ? $", ürün {item.StockTotal}" : "")}, {Comfort(item)})"
                    : item.Def.IsTrough ? $"{item.Def.DisplayName} (yem {item.FeedStock:0.#}, günlük {_animals.DailyFeedNeed(item):0.#})"
                    : item.Def.WaterGate ? $"{item.Def.DisplayName} ({(item.Toggled ? "kapalı" : "açık")})"
                    : item.Def.NeedsPower ? $"{item.Def.DisplayName} ({(Powered(item) ? "dönüyor" : "güç yok: su çarkı ya da yel değirmeni gerekli")})"
                    : item.Def.PowerSource ? $"{item.Def.DisplayName} ({(Net().Contains(item.Id) ? "dönüyor" : "kuru: kaynağa bağlı kanala değmeli")})"
                    : item.Def.NeedsWaterSource ? $"{item.Def.DisplayName} ({(Net().Contains(item.Id) ? "su akıyor" : "kuru: kuyuya bağla")})"
                    : item.Def.IsCistern ? $"{item.Def.DisplayName} ({item.Fill}/{item.Def.CisternCapacity} su)"
                    : item.Def.WaterHead ? $"{item.Def.DisplayName} ({(Net().Contains(item.Id) ? "basınç var" : "kuru: kurşun boruya bağla")})"
                    : item.Def.IsFixture ? $"{item.Def.DisplayName} ({(_water.Report.IsSupplied(item) ? "su geliyor" : "su yok")})"
                    : DecorLabel(item);
                var size = _labelStyle.CalcSize(new GUIContent(label));
                GUI.Label(new Rect(x - size.x * 0.5f, y - size.y * 0.5f, size.x, size.y), label, _labelStyle);
            }
        }

        /// <summary>Etiket kapalıyken "!" ile işaretlenecek durum: oyuncunun bir şey yapması gerekiyor.</summary>
        private bool ItemNeedsAttention(ItemInstance item, System.Func<HashSet<int>> net, System.Func<ItemInstance, bool> powered)
        {
            if (item.IsPlant) return false;   // susamış ekin sayısı üst şeritte
            if (item.Def.IsHousing || item.Def.IsTrough)
                return _animals.Residents(item.Id) > 0 && item.FeedStock <= 0f
                    || item.Def.IsTrough && item.FeedStock < _animals.DailyFeedNeed(item);
            if (item.Def.NeedsPower) return !powered(item);
            if (item.Def.NeedsWaterSource || item.Def.WaterHead) return !net().Contains(item.Id);
            if (item.Def.IsFixture) return !_water.Report.IsSupplied(item);
            if (item.HasContents) return true;                                   // döküntü toplanmayı bekliyor
            if (_world.IsOverloaded(item)) return true;                          // raf çökebilir
            if (RottenDecorOn(item) != null) return true;                        // üstündeki sepette çürük var
            return false;
        }

        // Dekor etiketi (27): yük, aşırı yük uyarısı, durum, kararma, döküntü içeriği.
        private string DecorLabel(ItemInstance item)
        {
            string name = item.Def.DisplayName;
            var notes = new List<string>();
            if (item.HasContents) return $"{name}: {item.Contents.Count} eşya (Topla)";
            if (item.Condition != Thatchwick.Core.Content.ItemCondition.Sound) notes.Add(Thatchwick.Core.Content.Decor.ConditionName(item.Condition).ToLowerInvariant());
            if (item.Def.MaxLoad > 0 && item.Def.SurfaceSlots.Count > 0)
            {
                int load = _world.LoadOf(item.Id);
                notes.Add(load > item.Def.MaxLoad ? $"aşırı yük {load}/{item.Def.MaxLoad}: çökebilir" : $"yük {load}/{item.Def.MaxLoad}");
            }
            if (item.IsTarnished) notes.Add("kararmış: cilala");
            if (Thatchwick.Core.Build.DecorSystem.IsWilted(item, _clock.Season)) notes.Add("soldu: kaldır");
            string inscription = _life?.Keepsakes.LabelOf(item);
            if (inscription != null) return notes.Count > 0 ? $"{inscription} ({string.Join(", ", notes)})" : inscription;
            var rotten = RottenDecorOn(item);
            if (rotten != null) notes.Add($"{rotten.Def.DisplayName} çürüdü: boşalt");
            return notes.Count > 0 ? $"{name} ({string.Join(", ", notes)})" : name;
        }

        // Yüzeyin üstünde içi çürümüş doldurulan dekor (yoksa null).
        private ItemInstance RottenDecorOn(ItemInstance surface)
        {
            if (surface.Def.SurfaceSlots.Count == 0) return null;
            foreach (var d in _world.GetDecorOn(surface.Id))
                if (d.Def.IsFillDecor && d.HasContents && Thatchwick.Core.Build.DecorSystem.WorstFreshness(d, _content) == Thatchwick.Core.Content.Decor.Rotten)
                    return d;
            return null;
        }

        /// <summary>Fare (ya da son dokunuş) altındaki eşya; arayüzün üstündeyse yok.</summary>
        private ItemInstance HoveredItem(Camera cam)
        {
            var pointer = UnityEngine.InputSystem.Pointer.current;
            if (pointer == null) return null;
            Vector2 screen = pointer.position.ReadValue();
            if (IsOverHud(screen)) return null;
            var world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 0f));
            // Önce eşyanın gövdesi (karonun biraz üstü), sonra ayak izi.
            return _world.GetItemAt(_view.WorldToTile(world - new Vector3(0f, 24f / 32f, 0f))) ?? _world.GetItemAt(_view.WorldToTile(world));
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
