using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Audio;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Farmyard;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Game
{
    /// <summary>
    /// Avlu sisteminin ve ortam efektlerinin oyun katmanı (belge 57-57e): sinek ve öbür sürüler, koku dalgası, rüzgâr yönü
    /// ve hamle, yatmış tahıl, ıslak karakter, yağmur sıçraması ve saçak damlası, ıslak zemin, su parıltısı, arabaların
    /// sefere çıkışı, arabalar paneli ve efekt yoğunluğu ayarı. Kurallar Core.Farmyard ve LifeSystems.Farmyard'da.
    /// Buradaki her şey görseldir; hiçbiri kayda girmez.
    /// </summary>
    public sealed partial class GameRoot
    {
        private const string FxPref = "fx_intensity";
        private Ui.CartPanel _cartPanel;
        /// <summary>Efekt yoğunluğu: 0 kapalı, 1 az, 2 tam (sinek, damla gibi ortam efektleri).</summary>
        private int _fxLevel = 2;
        private float FxScale => _fxLevel <= 0 ? 0f : _fxLevel == 1 ? 0.5f : 1f;

        private float _wetGround, _wetGroundShown = -1f, _snowGround, _snowShown = -1f, _decalAt, _pondAt;
        private SpriteRenderer[] _clouds;
        private readonly Dictionary<int, float> _shiverAt = new Dictionary<int, float>();
        private readonly Dictionary<string, List<Vector3>> _cartRoutes = new Dictionary<string, List<Vector3>>(System.StringComparer.Ordinal);
        private readonly Dictionary<int, (Vector3 pos, int dir, bool hidden)> _hitched = new Dictionary<int, (Vector3, int, bool)>();
        private long _routesDay = long.MinValue;
        private float _flyBuzzAt, _splashDebt;
        private readonly Dictionary<int, float> _catDig = new Dictionary<int, float>();          // kedi → eşelemenin bittiği an
        private readonly Dictionary<int, Vector3> _cartHome = new Dictionary<int, Vector3>();    // araba eşyası → avludaki yeri
        private readonly HashSet<int> _wetTinted = new HashSet<int>();
        // Her karede bütün eşyaları taramamak için saniyede bir yenilenen listeler (belge 53: karede çöp yok).
        private readonly List<ItemInstance> _hotHeaps = new List<ItemInstance>();
        private readonly List<Cart> _placedCarts = new List<Cart>();
        private float _yardScanAt;
        private readonly Dictionary<int, float> _sneezeAt = new Dictionary<int, float>();
        private static readonly Color WetSkin = new Color(0.78f, 0.83f, 0.93f, 1f);
        private static readonly Color WetGround = new Color(0.8f, 0.84f, 0.92f, 1f);

        private void InitYard()
        {
            _fxLevel = Mathf.Clamp(PlayerPrefs.GetInt(FxPref, 2), 0, 2);
            _life.AdultPack = () => Thatchwick.Core.Content.ContentPacks.Adult;   // küfürlü azar yalnızca +18 paketinde
            _life.Compost.Powered = it => _automation != null && _automation.IsPowered(it);   // döner fıçı çark ya da değirmenle döner
            _life.Scolded += (witness, actor, key) =>
            {
                Say(witness, T(key, ("name", actor.Name)));
                // Gören yanına gelir: boşta ise yapanın yanındaki karoya yürür.
                var view = _actors.Find(a => a.Character == witness);
                if (view != null && view.Runner != null && view.Runner.IsIdle) view.Runner.Command(Job.MoveTo(NearestWalkable(actor.Position, 2)));
            };
            _life.SteppedInIt += (c, pile) =>
            {
                Say(c, T("yard.say.stepped"));
                var at = TileCenter(pile.Position);
                _juice?.Emit(Juice.Kind.Soil, at, 3, Iso.SortOrder(at.y, 2, pile.Position.Z));
                Sfx(SoundKeys.YardStepInIt, at, c.Id);
            };
            _life.CatBuried += cat =>
            {
                // Kedi birkaç saniye eşeler (kareler gelene kadar toprak savrulur; art/queue 704).
                _catDig[cat.Id] = Time.time + 2.4f;
                Sfx(SoundKeys.YardCatDig, TileCenter(cat.Position), cat.Id, 0.7f);
            };
            _life.CartSetOut += cart =>
            {
                var item = cart.ItemId != 0 ? _world.GetItem(cart.ItemId) : null;
                if (item != null) Sfx(SoundKeys.YardCartOut, TileCenter(item.Position), item.Id);
            };
            _effects.AppliedOn += (c, item, i) =>
            {
                if (c == null || item == null || i == null || string.IsNullOrEmpty(i.FarmAction)) return;
                string key = i.FarmAction == LifeSystems.ActPileCollect ? SoundKeys.YardShovel
                    : i.FarmAction == LifeSystems.ActCompostTurn ? SoundKeys.YardCompostTurn
                    : i.FarmAction == LifeSystems.ActBarnClean ? SoundKeys.YardMuckOut : null;
                if (key != null) Sfx(key, TileCenter(item.Position), c.Id);
            };
            _cartPanel = new Ui.CartPanel(_life, _animals, _content, () => EnsureUi())
            {
                Family = FamilyForCarts,
                Day = () => _clock.DayIndex
            };
            _cartPanel.Message += m => ShowMessage(m, 4f);
            _cartPanel.Opening += () => { _familyPanel?.Close(); _minePanel?.Close(); };   // IMGUI paneli pencerenin üstüne çizilmesin
            Shader.SetGlobalFloat("_TwLodgeSide", -1f);   // yağmur rüzgârı sola eser; tahıl o yöne yatar
        }

        private static Vector3 TileCenter(GridPos p) => Iso.GridToWorld(p.X + 0.5f, p.Y + 0.5f, p.Z);

        private IEnumerable<Character> FamilyForCarts()
        {
            foreach (var m in _life.Population.Members)
                if (!m.IsDead && !m.IsVillager && !m.Away) yield return m;
        }

        private void CycleFx()
        {
            _fxLevel = (_fxLevel + 1) % 3;
            PlayerPrefs.SetInt(FxPref, _fxLevel);
        }

        /// <summary>Ortam efektlerinin tek girişi; <see cref="UpdateJuice"/> her karede çağırır (yapı modunda çağrılmaz).</summary>
        private void UpdateYardFx(float dt)
        {
            if (_life == null || _juice == null || _clock == null) return;
            var weather = _life.Weather.Today(_clock.TotalDays);
            UpdateWind(weather);
            UpdateLodgedCrops();
            UpdateCartViews();
            float scale = FxScale;
            UpdateWetGround(dt, weather, scale);
            UpdateSnow(dt, weather);
            UpdateWetCharacters(dt, scale);
            UpdateDecals();
            UpdateClouds(dt, weather, scale);
            Shader.SetGlobalFloat("_TwWaterAmp", scale);
            if (scale <= 0f) return;
            UpdateWaterLife(dt, weather, scale);
            UpdateSwarms(dt, weather, scale);
            UpdateRainDetails(dt, weather, scale);
            UpdateCatDig();
        }

        // ---------- rüzgâr ----------

        /// <summary>
        /// Rüzgâr yönü ve hamle (belge 57 §8). Yağışta yön yağmurun yattığı yöndür (sola); açık havada gün gün değişir.
        /// Bitkiler yöne doğru hafif yatık durur, haritayı dolaşan hamle sırayla eğer; duman ve yaprak aynı rüzgârla savrulur.
        /// </summary>
        private void UpdateWind(WeatherType weather)
        {
            bool wetWeather = weather == WeatherType.Rain || weather == WeatherType.Storm;
            float dir = wetWeather || (_clock.DayIndex * 7919L) % 2 == 0 ? -1f : 1f;
            float lean = weather == WeatherType.Storm ? 2.5f : weather == WeatherType.Rain ? 1f : 0.3f;
            float gustAmp = weather == WeatherType.Storm ? 3f : weather == WeatherType.Rain ? 1.5f : weather == WeatherType.Snow ? 0.5f : 1f;
            float rainWind = _weatherFx != null ? _weatherFx.CurrentWind : 0f;
            _juice.Wind = wetWeather && rainWind != 0f ? rainWind * 0.35f : dir * (weather == WeatherType.Snow ? 0.15f : 0.3f);

            var cam = Camera.main;
            float centre = cam != null ? cam.transform.position.x + cam.transform.position.y * 0.5f : 0f;
            // Hamle 10 saniyede bir ekranı rüzgâr yönünde geçer (60 birimlik şerit, ekran ortada).
            float travel = (Time.time * 6f) % 60f - 30f;
            Shader.SetGlobalFloat("_TwLean", dir * lean);
            Shader.SetGlobalFloat("_TwGustPos", centre + dir * travel);
            Shader.SetGlobalFloat("_TwGustWidth", 5f);
            Shader.SetGlobalFloat("_TwGustAmp", dir * gustAmp);
        }

        /// <summary>
        /// Yatmış tahıl (belge 57d §2): fırtınanın yatırdığı ekin yatık shader'la çizilir (taban yerinde, üst satırlar
        /// rüzgâr yönüne kaymış, boy kısalmış). Shader yoksa ekin yana devrilir. Hasada kadar öyle kalır.
        /// </summary>
        private void UpdateLodgedCrops()
        {
            if (_plants == null || _items == null) return;
            var lodged = SpriteMaterials.Lodged;
            foreach (var kv in _plants.LodgedCrops)
            {
                if (!_items.Plants.TryGetValue(kv.Key, out var pv) || pv.View == null) continue;
                if (lodged != null && pv.Sr != null) { if (pv.Sr.sharedMaterial != lodged) pv.Sr.sharedMaterial = lodged; }
                else pv.View.localRotation = Quaternion.Euler(0f, 0f, -38f);
            }
        }

        // ---------- sürüler, koku, buhar ----------

        private bool RandomVisibleTile(out GridPos tile, out Vector3 world)
        {
            tile = default; world = default;
            var cam = Camera.main;
            if (cam == null || _world == null) return false;
            float halfH = cam.orthographicSize, halfW = halfH * cam.aspect;
            world = new Vector3(cam.transform.position.x + (Random01() * 2f - 1f) * halfW, cam.transform.position.y + (Random01() * 2f - 1f) * halfH, 0f);
            var g = Iso.WorldToGrid(new Vector2(world.x, world.y));
            tile = new GridPos(Mathf.FloorToInt(g.x), Mathf.FloorToInt(g.y), 0);
            if (!_world.Map.InBounds(tile)) return false;
            world = TileCenter(tile);
            return true;
        }

        private bool IsNightFx => HourOfDay < 5.5 || HourOfDay > 20.5;

        /// <summary>
        /// Sürüler (belge 57 §2): sinek (koku kaynağı), güve (gece yanan ışık), ateş böceği (yaz gecesi), kelebek ve arı
        /// (bahar ve yaz, çiçek açmış bitki). Ayrıca kokmuş kaynakta koku dalgası ve sıcak kompostta buhar.
        /// </summary>
        private void UpdateSwarms(float dt, WeatherType weather, float scale)
        {
            var filth = _life.Filth;
            bool raining = weather == WeatherType.Rain || weather == WeatherType.Storm;
            bool flying = !filth.FliesLanded;
            SmellSource? buzz = null;
            foreach (var s in filth.Sources)
            {
                var at = TileCenter(s.Position);
                int order = Iso.SortOrder(at.y, 4, s.Position.Z);
                int flies = flying ? filth.FlyCount(s) : 0;
                // Bir sinek ortalama 1,9 sn yaşar: sayıyı korumak için saniyede flies / 1,9 sinek doğar.
                if (flies > 0 && Random01() < dt * flies * scale / 1.9f) { _juice.Emit(Juice.Kind.Fly, at, 1, order); buzz = s; }
                if (s.Tier >= 2 && Random01() < dt * 1.2f * scale) _juice.Emit(Juice.Kind.Stink, at, 1, order);
            }
            if (buzz.HasValue && Time.time >= _flyBuzzAt)
            {
                _flyBuzzAt = Time.time + 5f + Random01() * 4f;
                Sfx(SoundKeys.YardFlies, TileCenter(buzz.Value.Position), buzz.Value.ItemId, 0.5f);
            }

            // Sıcak kompost tüter.
            foreach (var item in _hotHeaps)
            {
                if (Random01() >= dt * 0.8f * scale) continue;
                var at = TileCenter(item.Position) + new Vector3(0f, 0.45f, 0f);
                _juice.Emit(Juice.Kind.Steam, at, 1, Iso.SortOrder(at.y, 5, item.Position.Z));
            }

            bool night = IsNightFx;
            if (night && !raining && _lights.Count > 0 && _clock.Season != Season.Winter)
            {
                // Güve: yanan ışıklardan rastgele birinin çevresinde.
                var light = _lights[Mathf.Min(_lights.Count - 1, (int)(Random01() * _lights.Count))];
                if (Random01() < dt * 1.5f * scale) _juice.Emit(Juice.Kind.Moth, light.Position, 1, Iso.SortOrder(light.Position.y, 6));
            }
            if (night && !raining && _clock.Season == Season.Summer && Random01() < dt * 2.5f * scale
                && RandomVisibleTile(out var tile, out var world) && !_world.Map.IsWater(tile) && !IsShelteredAt(world))
                _juice.Emit(Juice.Kind.Firefly, world, 1, Iso.SortOrder(world.y, 6));

            if (!night && !raining && (_clock.Season == Season.Spring || _clock.Season == Season.Summer) && _items.Plants.Count > 0
                && Random01() < dt * 1.2f * scale)
            {
                // Kelebek ve arı: bu mevsimde çiçek açan, büyümüş rastgele bir bitkide.
                int pick = (int)(Random01() * _items.Plants.Count), n = 0;
                foreach (var pv in _items.Plants.Values)
                {
                    if (n++ != pick) continue;
                    if (pv.View == null || !pv.Item.IsGrown || !pv.Item.Def.BloomSeasons.Contains(_clock.Season)) break;
                    _juice.Emit(Random01() < 0.5f ? Juice.Kind.Butterfly : Juice.Kind.Bee, pv.View.position, 1, Iso.SortOrder(pv.View.position.y, 6, pv.Item.Position.Z));
                    break;
                }
            }
        }

        // ---------- yağmur ayrıntıları, su, ıslak zemin ----------

        /// <summary>
        /// Yağmur yere çarpar (sıçrama), suya düşer (halka yerine parıltı), saçaktan damlar; açık havada gündüz su parıldar.
        /// Karolar ekranda görünenlerden rastgele seçilir, böylece maliyet haritanın büyüklüğüne bağlı değildir.
        /// </summary>
        private void UpdateRainDetails(float dt, WeatherType weather, float scale)
        {
            bool raining = weather == WeatherType.Rain || weather == WeatherType.Storm;
            float level = raining && _weatherFx != null ? _weatherFx.Level : 0f;
            if (level > 0.05f)
            {
                // Kare hızından bağımsız: saniyede 28 (fırtınada 45) karo denenir; parçacık havuzu (220) duman ve sürüyle ortak.
                _splashDebt = Mathf.Min(6f, _splashDebt + dt * level * scale * (weather == WeatherType.Storm ? 45f : 28f));
                for (; _splashDebt >= 1f; _splashDebt -= 1f)
                {
                    if (!RandomVisibleTile(out var tile, out var world) || IsShelteredAt(world)) continue;
                    world += new Vector3((Random01() - 0.5f) * 0.6f, (Random01() - 0.5f) * 0.3f, 0f);
                    int order = Iso.SortOrder(world.y, 1);
                    if (_world.Map.IsWater(tile)) { if (_world.Map.IsFilled(tile) && !_world.Map.FrozenAt(tile)) _juice.Emit(Juice.Kind.Glint, world, 1, order); }
                    else _juice.Emit(Juice.Kind.Splash, world, 2, order);
                }
                // Saçak damlası: içerisi olan bir karonun hemen önündeki açık karoya çatıdan damla düşer.
                if (Random01() < dt * 14f * level * scale && RandomVisibleTile(out var eave, out var eaveWorld) && !IsShelteredAt(eaveWorld))
                {
                    var behind = _world.Map.Rooms.RoomAt(new GridPos(eave.X, eave.Y + 1, 0)) ?? _world.Map.Rooms.RoomAt(new GridPos(eave.X + 1, eave.Y, 0));
                    if (behind != null && behind.IsIndoor)
                        _juice.Emit(Juice.Kind.Drip, eaveWorld + new Vector3((Random01() - 0.5f) * 0.5f, 1.15f, 0f), 1, Iso.SortOrder(eaveWorld.y, 6));
                }
            }
            else if (weather == WeatherType.Clear && !IsNightFx && Random01() < dt * 6f * scale
                     && RandomVisibleTile(out var water, out var waterWorld) && _world.Map.IsWater(water) && _world.Map.IsFilled(water) && !_world.Map.FrozenAt(water))
                _juice.Emit(Juice.Kind.Glint, waterWorld, 1, Iso.SortOrder(waterWorld.y, 1));
        }

        /// <summary>Islak zemin: yağarken zemin koyulaşır, dinince yavaşça (yaklaşık bir dakikada) kurur.</summary>
        private void UpdateWetGround(float dt, WeatherType weather, float scale)
        {
            bool raining = weather == WeatherType.Rain || weather == WeatherType.Storm;
            float target = raining && scale > 0f && _weatherFx != null ? Mathf.Clamp01(_weatherFx.Level * 1.5f) : 0f;
            _wetGround = Mathf.MoveTowards(_wetGround, target, dt * (target > _wetGround ? 0.12f : 0.016f));
            if (Mathf.Abs(_wetGround - _wetGroundShown) < 0.02f || _view == null) return;
            _wetGroundShown = _wetGround;
            _view.SetGroundTint(Color.Lerp(Color.white, WetGround, _wetGround));
        }

        /// <summary>
        /// Islak karakter (belge 57 §7): ıslaklıkla soğuk bir tona döner ve üstünden su damlar; sıcak yerde kururken buharı
        /// tüter. Soğukta nefesi görünür; üşüttüyse arada ürperir ve hapşırır. Ton kapalı efektte de uygulanır (durum bilgisi).
        /// </summary>
        private void UpdateWetCharacters(float dt, float scale)
        {
            foreach (var actor in _actors)
            {
                var c = actor.Character;
                if (c == null || actor.Renderer == null || actor.View == null) continue;
                float wet = c.IsDead || c.Away ? 0f : c.Wetness;
                if (wet > 0.05f) { actor.Renderer.color = Color.Lerp(Color.white, WetSkin, wet); _wetTinted.Add(c.Id); }
                else if (_wetTinted.Remove(c.Id)) actor.Renderer.color = Color.white;
                if (scale <= 0f || c.IsDead || c.Away) continue;

                var head = actor.View.position + new Vector3(0f, 0.95f, 0f);
                int order = Iso.SortOrder(actor.View.position.y, 3, c.Position.Z);
                if (wet > 0.05f)
                {
                    if (Random01() < dt * 2.5f * wet * scale)
                        _juice.Emit(Juice.Kind.Drip, actor.View.position + new Vector3((Random01() - 0.5f) * 0.3f, 0.3f + Random01() * 0.5f, 0f), 1, order);
                    if (c.AmbientTemperature >= 18f && Random01() < dt * 1.2f * scale) _juice.Emit(Juice.Kind.Steam, head, 1, order);
                }
                bool outdoorCold = c.AmbientTemperature < 3f && !c.Asleep;
                if (outdoorCold && Random01() < dt * 0.4f * scale) _juice.Emit(Juice.Kind.Breath, head + new Vector3(0.1f, -0.12f, 0f), 1, order);
                // Titreme: üşüyen (üşütmüş ya da sıfırın altında ıslak) karakter sürekli, hafifçe titrer: bir piksel sağa sola.
                bool shivering = !c.Asleep && (c.Mood.Has(MoodCatalog.Chilled) || (c.AmbientTemperature < 0f && wet > 0.3f));
                if (shivering)
                {
                    if (!_shiverAt.TryGetValue(c.Id, out float at) || Time.time >= at)
                    {
                        _shiverAt[c.Id] = Time.time + 0.09f;
                        actor.View.position += new Vector3(((int)(Time.time * 11f) & 1) == 0 ? 1f / 32f : -1f / 32f, 0f, 0f);
                    }
                }
                else _shiverAt.Remove(c.Id);
                if (c.Mood.Has(MoodCatalog.Chilled) && !c.Asleep)
                {
                    if (!_sneezeAt.TryGetValue(c.Id, out float next)) _sneezeAt[c.Id] = next = Time.time + 6f + Random01() * 10f;
                    if (Time.time >= next)
                    {
                        _sneezeAt[c.Id] = Time.time + 18f + Random01() * 22f;
                        _juice.Pop(actor.View, 0.22f);   // ürperme
                        Say(c, T("yard.say.sneeze"));
                    }
                }
            }
        }

        // ---------- kedi ve arabalar ----------

        private void UpdateCatDig()
        {
            if (_catDig.Count == 0 || _animals == null) return;
            List<int> done = null;
            foreach (var kv in _catDig)
            {
                var cat = _animals.Get(kv.Key);
                if (cat == null || cat.IsDead || Time.time >= kv.Value) { (done ?? (done = new List<int>())).Add(kv.Key); continue; }
                if (Random01() < Time.deltaTime * 7f)
                {
                    var at = TileCenter(cat.Position) + new Vector3(-0.12f, 0.02f, 0f);
                    _juice.Emit(Juice.Kind.Soil, at, 1, Iso.SortOrder(at.y, 2, cat.Position.Z));
                }
            }
            if (done != null) foreach (var id in done) _catDig.Remove(id);
        }

        /// <summary>
        /// Arabaların seferi (belge 57e §2.2 "görünür"): sabah araba, önünde koşulu hayvanıyla avludan köy yoluna doğru
        /// yol bularak çıkar, gözden kaybolur, akşam aynı yoldan döner. Yol günde bir kez bulunur (Core yol bulucu);
        /// bulunamazsa araba yerinde kalır, hayvan yine gizlenir. Koşulu hayvanın görünümü yolculuk boyunca arabanın
        /// önüne taşınır; sürü içinde ikinci kez çizilmez.
        /// </summary>
        private void UpdateCartViews()
        {
            var fleet = _life.Fleet;
            if (fleet == null || _items == null) return;
            if (Time.time >= _yardScanAt)
            {
                _yardScanAt = Time.time + 1f;
                _placedCarts.Clear();
                foreach (var c in fleet.Carts) if (c.ItemId != 0) _placedCarts.Add(c);
                _hotHeaps.Clear();
                foreach (var it in _world.Items)
                    if (!it.IsDecor && _life.Compost.IsHeap(it) && _life.Compost.HeapOf(it).Hot) _hotHeaps.Add(it);
            }
            double frac = _clock.DayFraction;
            if (_routesDay != _clock.DayIndex) { _routesDay = _clock.DayIndex; _cartRoutes.Clear(); }
            _hitched.Clear();
            foreach (var cart in _placedCarts)
            {
                if (!_items.Views.TryGetValue(cart.ItemId, out var view) || view == null) continue;
                var (phase, t) = fleet.TripPhase(cart, frac);
                var worked = TodayCart(fleet, cart);
                if (phase == 0)
                {
                    if (!_cartHome.TryGetValue(cart.ItemId, out var home)) continue;
                    _cartHome.Remove(cart.ItemId);
                    view.position = home;
                    if (!view.gameObject.activeSelf) view.gameObject.SetActive(true);
                    continue;
                }
                if (!_cartHome.TryGetValue(cart.ItemId, out var start)) _cartHome[cart.ItemId] = start = view.position;
                int animalId = worked != null ? worked.AnimalId : 0;
                if (phase == 2)
                {
                    if (view.gameObject.activeSelf) view.gameObject.SetActive(false);
                    if (animalId != 0) _hitched[animalId] = (start, IsoArt.DefaultDirection, true);
                    continue;
                }
                if (!view.gameObject.activeSelf) view.gameObject.SetActive(true);
                var route = RouteFor(cart, start);
                float k = phase == 1 ? t : 1f - t;
                k = k * k * (3f - 2f * k);
                var pos = Along(route, k);
                var ahead = Along(route, Mathf.Min(1f, k + 1f / Mathf.Max(1, route.Count - 1)));
                if (phase == 3) ahead = Along(route, Mathf.Max(0f, k - 1f / Mathf.Max(1, route.Count - 1)));   // dönüşte avluya bakar
                view.position = pos;
                var heading = new Vector2(ahead.x - pos.x, ahead.y - pos.y);
                if (animalId != 0)
                {
                    // Hayvan arabanın bir karo önünde, gidiş yönüne bakar.
                    var lead = heading.sqrMagnitude > 0.0001f ? pos + (Vector3)(heading.normalized * 0.9f) : pos;
                    _hitched[animalId] = (lead, IsoArt.DirectionIndex(heading), false);
                }
                if (FxScale > 0f && Random01() < Time.deltaTime * 6f) _juice.Emit(Juice.Kind.Dust, pos + new Vector3(0f, 0.05f, 0f), 1, Iso.SortOrder(pos.y, -1));
            }
        }

        private static Cart TodayCart(CartFleet fleet, Cart cart)
        {
            foreach (var c in fleet.Today) if (c.Key == cart.Key) return c;
            return null;
        }

        private (Vector3 pos, int dir, bool hidden)? HitchedView(Animal a) =>
            a != null && _hitched.TryGetValue(a.Id, out var h) ? h : ((Vector3, int, bool)?)null;

        /// <summary>Avludaki arabadan köy yoluna giden yol (dünya noktaları). Yol bulunamazsa yalnızca başlangıç.</summary>
        private List<Vector3> RouteFor(Cart cart, Vector3 start)
        {
            if (_cartRoutes.TryGetValue(cart.Key, out var route)) return route;
            route = new List<Vector3> { start };
            var item = _world.GetItem(cart.ItemId);
            if (item != null)
            {
                var road = Regions.Road;
                var goal = new GridPos(road.X + 2, road.Y + road.H / 2, 0);
                // Araba kendi karosunu kapatır: yol bitişik açık karodan başlar.
                GridPos? from = null;
                foreach (var d in new[] { new GridPos(1, 0, 0), new GridPos(0, 1, 0), new GridPos(-1, 0, 0), new GridPos(0, -1, 0), new GridPos(2, 0, 0), new GridPos(0, 2, 0) })
                {
                    var p = new GridPos(item.Position.X + d.X, item.Position.Y + d.Y, 0);
                    if (_world.Map.InBounds(p) && _world.Map.IsWalkable(p)) { from = p; break; }
                }
                var path = from.HasValue && _world.Map.InBounds(goal) ? Pathfinder.FindPath(_world.Map, from.Value, goal) : null;
                if (path != null)
                {
                    // Ekranda yaklaşık 14 karo yürür, sonra köy yoluna sapmış sayılır (gözden kaybolur).
                    int n = Mathf.Min(path.Count, 14);
                    for (int i = 0; i < n; i++) route.Add(_view.TileToWorld(path[i]));
                }
            }
            _cartRoutes[cart.Key] = route;
            return route;
        }

        private static Vector3 Along(List<Vector3> route, float k)
        {
            if (route.Count == 1) return route[0];
            float f = Mathf.Clamp01(k) * (route.Count - 1);
            int i = Mathf.Min(route.Count - 2, Mathf.FloorToInt(f));
            return Vector3.Lerp(route[i], route[i + 1], f - i);
        }

        // ---------- kar, çıkartmalar, bulut gölgesi, su canlılığı ----------

        /// <summary>
        /// Kar birikimi (belge 57 §9): kar yağarken zemin yaklaşık bir dakikada örtülür; havada ılık (+1°C üstü) ya da
        /// yağmurda erir. Örtü shader'la çizilir (Thatchwick/SnowCover); efekt ayarından bağımsızdır (durum bilgisi).
        /// </summary>
        private void UpdateSnow(float dt, WeatherType weather)
        {
            float outdoor = _heat != null ? _heat.OutdoorTemperature : 5f;
            if (_snowShown < 0f && weather == WeatherType.Snow) _snowGround = 1f;   // yüklemede kar yağıyorsa örtü hazır
            if (weather == WeatherType.Snow && outdoor <= 1f) _snowGround = Mathf.MoveTowards(_snowGround, 1f, dt / 60f);
            else if (outdoor > 1f || weather == WeatherType.Rain || weather == WeatherType.Storm)
                _snowGround = Mathf.MoveTowards(_snowGround, 0f, dt / (weather == WeatherType.Rain ? 40f : 120f));
            if (Mathf.Abs(_snowGround - _snowShown) < 0.01f) return;
            _snowShown = _snowGround;
            Shader.SetGlobalFloat("_TwSnow", _snowGround);
        }

        private static int TileHash(int x, int y) => (x * 73856093) ^ (y * 19349663);

        /// <summary>Karo başına zemin çıkartması: yabani ot, kıyı köpüğü, yağmur birikintisi ve patikada çamur.</summary>
        private Sprite DecalAt(int x, int y)
        {
            var p = new GridPos(x, y, 0);
            var map = _world.Map;
            if (_plants != null && _plants.IsWeedy(p)) return IsoArt.Decal("iso_decal_weeds_a");   // b reddedildi (710); çeşitlilik çıkartma katmanının aynalamasından gelir
            var floor = map.GetFloor(p);
            if (floor == FloorType.Water)
            {
                if (!map.IsFilled(p) || map.FrozenAt(p) || _fxLevel <= 0) return null;
                bool shore = !map.IsWater(new GridPos(x + 1, y, 0)) || !map.IsWater(new GridPos(x, y + 1, 0))
                    || !map.IsWater(new GridPos(x - 1, y, 0)) || !map.IsWater(new GridPos(x, y - 1, 0));
                return shore ? IsoArt.Decal((TileHash(x, y) & 2) == 0 ? "iso_decal_shore_foam_a" : "iso_decal_shore_foam_b") : null;
            }
            if (_wetGround < 0.3f || _snowGround > 0.5f || _fxLevel <= 0) return null;
            var room = map.Rooms.RoomAt(p);
            if ((room != null && room.IsIndoor) || _world.GetItemAt(p) != null) return null;
            int h = TileHash(x, y) & 1023;
            if (map.GetRoad(p) != RoadType.None || floor == FloorType.Dirt)
                return h < _wetGround * 150f ? IsoArt.Decal("iso_decal_mud_a") : null;
            return h < _wetGround * 50f ? IsoArt.Decal((h & 1) == 0 ? "iso_decal_puddle_a" : "iso_decal_puddle_b") : null;
        }

        /// <summary>Çıkartmalar iki saniyede bir yenilenir (ot, birikinti ve çamur yavaş değişir).</summary>
        private void UpdateDecals()
        {
            if (Time.time < _decalAt || _view == null) return;
            _decalAt = Time.time + 2f;
            _view.RefreshDecals(DecalAt);
        }

        /// <summary>
        /// Bulut gölgesi (belge 57 §9): açık havada gündüz, birkaç yumuşak gölge rüzgâr yönünde ekranı yavaşça geçer.
        /// Yağışta gökyüzü kapalı sayılır, gölge çizilmez.
        /// </summary>
        private void UpdateClouds(float dt, WeatherType weather, float scale)
        {
            var cam = Camera.main;
            if (cam == null) return;
            bool show = scale > 0f && weather == WeatherType.Clear && !IsNightFx;
            if (_clouds == null)
            {
                if (!show) return;
                _clouds = new SpriteRenderer[4];
                for (int i = 0; i < _clouds.Length; i++)
                {
                    var go = new GameObject("Cloud shadow " + i);
                    go.transform.SetParent(transform, false);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sharedMaterial = SpriteMaterials.Unlit;
                    sr.sprite = SpriteMaterials.CloudShadow(1031 + i * 7);
                    sr.sortingOrder = WeatherFx.Order - 5;
                    go.transform.position = cam.transform.position + new Vector3((i - 1.5f) * 9f, (i % 2 == 0 ? 2.5f : -3f), 0f);
                    _clouds[i] = sr;
                }
            }
            float halfW = cam.orthographicSize * cam.aspect + 6f, halfH = cam.orthographicSize + 4f;
            float wind = _juice != null && _juice.Wind != 0f ? Mathf.Sign(_juice.Wind) : 1f;
            foreach (var sr in _clouds)
            {
                if (sr == null) continue;
                sr.enabled = show;
                if (!show) continue;
                sr.color = new Color(0f, 0f, 0f, 0.10f * scale);
                var tr = sr.transform;
                var pos = tr.position + new Vector3(wind * 0.35f * dt, 0.08f * dt, 0f);
                var c = cam.transform.position;
                // Ekrandan çıkan gölge öbür kenardan yeniden girer.
                if (pos.x > c.x + halfW) pos.x -= 2f * halfW; else if (pos.x < c.x - halfW) pos.x += 2f * halfW;
                if (pos.y > c.y + halfH) pos.y -= 2f * halfH; else if (pos.y < c.y - halfH) pos.y += 2f * halfH;
                tr.position = new Vector3(Mathf.Round(pos.x * 32f) / 32f, Mathf.Round(pos.y * 32f) / 32f, 0f);
            }
        }

        /// <summary>
        /// Suda yürüme ve gölet canlılığı (belge 57 §10): suya giren karakter ve hayvanın (ördek, kaz) çevresinde halka
        /// ve sıçrama; ekrandaki suda arada balık sıçrar, yazın gündüz kızböceği uçar.
        /// </summary>
        private void UpdateWaterLife(float dt, WeatherType weather, float scale)
        {
            var map = _world.Map;
            foreach (var actor in _actors)
            {
                var c = actor.Character;
                if (c == null || actor.View == null || c.IsDead || c.Away) continue;
                var p = c.Position;
                if (p.Z != 0 || !map.InBounds(p) || !map.IsWater(p) || !map.IsFilled(p) || map.FrozenAt(p)) continue;
                var step = actor.View.position - actor.LastPos;
                bool moving = step.x * step.x + step.y * step.y > 0.00001f;
                int order = Iso.SortOrder(actor.View.position.y, 2, 0);
                if (Random01() < dt * (moving ? 5f : 1.2f) * scale) _juice.Emit(Juice.Kind.Glint, actor.View.position, 1, order);
                if (moving && Random01() < dt * 3f * scale) _juice.Emit(Juice.Kind.Splash, actor.View.position, 2, order);
            }
            if (_animals != null)
                foreach (var a in _animals.Animals)
                {
                    var p = a.Position;
                    if (a.IsDead || p.Z != 0 || !map.InBounds(p) || !map.IsWater(p) || !map.IsFilled(p) || map.FrozenAt(p)) continue;
                    if (Random01() >= dt * 1.5f * scale) continue;
                    var at = _view.TileToWorld(p);
                    _juice.Emit(Juice.Kind.Glint, at, 1, Iso.SortOrder(at.y, 2, 0));
                }

            if (Time.time < _pondAt) return;
            _pondAt = Time.time + 0.5f;
            if (!RandomVisibleTile(out var tile, out var world) || !map.IsWater(tile) || !map.IsFilled(tile) || map.FrozenAt(tile)) return;
            bool day = !IsNightFx;
            if (Random01() < 0.12f * scale)
            {
                // Balık sıçrar: halka ve küçük sıçrama.
                int order = Iso.SortOrder(world.y, 1);
                _juice.Emit(Juice.Kind.Splash, world, 3, order);
                _juice.Emit(Juice.Kind.Glint, world, 2, order);
            }
            if (day && _clock.Season == Season.Summer && weather == WeatherType.Clear && Random01() < 0.2f * scale)
                _juice.Emit(Juice.Kind.Dragonfly, world, 1, Iso.SortOrder(world.y, 6));
        }
    }
}
