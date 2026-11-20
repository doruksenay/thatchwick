using System;
using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.World;

namespace Thatchwick.Game
{
    /// <summary>
    /// Şehir dükkânı paneli (geçici IMGUI, belge 41). Dükkânın içine dokununca açılır. Alışveriş için oyuncu dükkânın
    /// içinde, dükkân açık ve sahibi dükkânda olmalı (Stardew gibi). Mallar Market kurallarıyla (ticaret becerisi,
    /// mevsim, talep), eşyalar BuildService ile (beceri kilidi, fiyat) satılır; dükkân yalnızca kendi mallarını alır.
    /// </summary>
    public sealed class ShopPanel
    {
        private readonly Market _market;
        private readonly BuildService _build;
        private readonly Household _house;
        private readonly ContentDatabase _content;
        private readonly WorldState _world;
        private readonly Func<Character> _player;
        private readonly Func<JobRunner> _runner;
        private readonly Func<double> _hour;
        private readonly Func<long> _day;
        private readonly Func<ShopDef, Character> _owner;
        private readonly Dictionary<string, RectI> _areas;
        private ShopDef _shop;
        private Vector2 _scroll;
        private string _status = "";
        private int _tab;   // 0 mallar, 1 eşyalar
        private List<ItemDef> _goods, _wares;

        public bool Active { get; private set; }
        /// <summary>Hayvan pazarında hayvan satışı için (barınak ve kilit kuralları AnimalSystem.Buy'da).</summary>
        public Thatchwick.Core.Animals.AnimalSystem Animals { get; set; }
        public Func<int> AccountLevel { get; set; }
        public ShopDef Shop => _shop;
        /// <summary>Meyhaneden bir şey alındı (alan, eşya): "her zamankinden mi?" easter egg'i (belge 55 §2b).</summary>
        public System.Action<Character, string> TavernOrdered { get; set; }
        /// <summary>Köy hayatı (belge 66f): Pazar, ustanın bunalımı/hastalığı/yası, çağrı, stok ve keyifli ustanın ikramı.</summary>
        public Thatchwick.Core.Life.LivingVillage Village { get; set; }

        public ShopPanel(Market market, BuildService build, Household house, ContentDatabase content, WorldState world,
            Func<Character> player, Func<JobRunner> runner, Func<double> hour, Func<long> day, Func<ShopDef, Character> owner,
            Dictionary<string, RectI> areas)
        {
            _market = market; _build = build; _house = house; _content = content; _world = world;
            _player = player; _runner = runner; _hour = hour; _day = day; _owner = owner; _areas = areas;
        }

        /// <summary>Bu karo hangi dükkânın içinde (yoksa null).</summary>
        public ShopDef ShopAt(GridPos tile)
        {
            if (_areas == null) return null;
            foreach (var kv in _areas) if (kv.Value.Contains(tile)) return Shops.Get(kv.Key);
            return null;
        }

        public void Open(ShopDef shop)
        {
            if (shop == null) return;
            if (_shop != shop) { _goods = null; _wares = null; _scroll = Vector2.zero; _tab = shop.Wares == null ? 0 : _tab; }
            _shop = shop; Active = true; _status = "";
        }

        public void Close() { Active = false; }

        public Rect PanelRect(float guiScale) =>
            new Rect(Screen.width / guiScale - 338f, 8f, 330f, Screen.height / guiScale - 16f);

        private bool PlayerInside()
        {
            var p = _player();
            return p != null && _shop != null && _areas.TryGetValue(_shop.Id, out var r) && r.Contains(p.Position);
        }

        public void WalkIn()
        {
            var p = _player(); var runner = _runner();
            if (p == null || runner == null || !_areas.TryGetValue(_shop.Id, out var r)) return;
            // Dükkânın içinde oyuncuya en yakın açık karo.
            GridPos best = default; int bestD = int.MaxValue;
            for (int y = r.Y; y < r.Y + r.H; y++)
                for (int x = r.X; x < r.X + r.W; x++)
                {
                    var t = new GridPos(x, y);
                    if (!_world.Map.IsWalkable(t)) continue;
                    int d = Math.Abs(x - p.Position.X) + Math.Abs(y - p.Position.Y);
                    if (d < bestD) { bestD = d; best = t; }
                }
            if (bestD == int.MaxValue) { _status = "Dükkânın içi kapalı."; return; }
            runner.Command(Job.MoveTo(best));
            _status = $"{_shop.Name} yoluna çıktı...";
        }

        private List<ItemDef> Goods()
        {
            if (_goods != null) return _goods;
            _goods = new List<ItemDef>();
            foreach (var g in _market.Goods()) if (_shop.Goods != null && _shop.Goods(g)) _goods.Add(g);
            return _goods;
        }

        private List<ItemDef> Wares()
        {
            if (_wares != null) return _wares;
            _wares = new List<ItemDef>();
            if (_shop.Wares == null) return _wares;
            foreach (var d in _content.Items.All)
            {
                if (d.Placement == ItemPlacement.Resource || d.Price <= 0) continue;
                if (d.HasTag("craftonly") || d.IsRetired) continue;
                if (_shop.Wares(d)) _wares.Add(d);
            }
            _wares.Sort((a, b) => a.Price.CompareTo(b.Price));
            return _wares;
        }

        /// <summary>Keyifli usta düşük olasılıkla ikram eder (belge 66f §B.8): eşya ya da indirim; balonu köy hayatı çıkarır.</summary>
        private void Treat(Character owner, int paid)
        {
            var treat = Village?.OnPurchase(owner);
            if (treat == null) return;
            if (treat.ItemId != null)
            {
                _house.AddItem(treat.ItemId, 1);
                _status += " " + GameLocalization.T("ui.village.treat_item", ("item", _content.Items.TryGet(treat.ItemId, out var d) ? d.DisplayName : treat.ItemId));
            }
            else if (treat.Discount > 0 && paid > 0)
            {
                int back = System.Math.Max(1, (int)System.Math.Round(paid * treat.Discount));
                _house.AddAkce(back);
                _status += " " + GameLocalization.T("ui.village.treat_discount", ("percent", (int)System.Math.Round(treat.Discount * 100)), ("akce", back));
            }
        }

        public void OnGUI(float guiScale)
        {
            if (!Active || _shop == null) return;
            GUILayout.BeginArea(PanelRect(guiScale), GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{_shop.Name.ToUpperInvariant()} · Akçe {_house.Akce}");
            if (GUILayout.Button("Kapat", GUILayout.Width(56f))) Active = false;
            GUILayout.EndHorizontal();

            var owner = _owner(_shop);
            bool open = Village != null ? Village.IsShopOpen(_shop, owner) : _shop.IsOpenAt(_hour(), _day());
            var closure = Village != null ? Village.ClosureOf(_shop, owner) : Thatchwick.Core.Life.ShopClosure.None;
            bool ownerHere = owner != null && _areas.TryGetValue(_shop.Id, out var area) && area.Contains(owner.Position);
            bool inside = PlayerInside();
            GUILayout.Label(_shop.Blurb);
            GUILayout.Label($"Saatler: {_shop.HoursText} · {(open ? "açık" : "kapalı")}");
            GUILayout.Label(owner == null ? "Dükkânın sahibi yok." : ownerHere ? $"{owner.Name} tezgâhta." : $"{owner.Name} şu an dükkânda değil.");
            if (closure != Thatchwick.Core.Life.ShopClosure.None)
                GUILayout.Label(GameLocalization.T("ui.village.closed_reason", ("reason", GameLocalization.R(Thatchwick.Core.Life.LivingVillage.ClosureText(closure)))));
            if (_status.Length > 0) GUILayout.Label(_status);

            bool canTrade = inside && open && ownerHere;
            if (!inside && GUILayout.Button("Dükkâna yürü")) WalkIn();
            else if (!canTrade) GUILayout.Label(!open ? "Kapalı saatte gelmişsin." : "Sahibi dönünce alışveriş yapabilirsin.");

            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(_tab == 0, "Mallar", GUI.skin.button)) _tab = 0;
            if (Wares().Count > 0 && GUILayout.Toggle(_tab == 1, $"Eşyalar ({Wares().Count})", GUI.skin.button)) _tab = 1;
            bool animalShop = _shop.Id == "animals" && Animals != null;
            if (animalShop && GUILayout.Toggle(_tab == 2, "Hayvanlar", GUI.skin.button)) _tab = 2;
            GUILayout.EndHorizontal();

            _scroll = GUILayout.BeginScrollView(_scroll);
            GUI.enabled = canTrade;
            if (_tab == 2 && animalShop)
            {
                GUILayout.Label("Hayvan, uygun barınağına (kümes, ahır, ağıl) yerleşir; barınak yoksa önce onu kur.");
                foreach (var sp in _content.Animals.All)
                {
                    if (sp.Price <= 0) continue;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(sp.DisplayName, GUILayout.Width(150f));
                    if (GUILayout.Button($"Al {sp.Price}", GUILayout.Width(64f)))
                    {
                        var r = Animals.Buy(sp.Id, _house, out var a, AccountLevel != null ? AccountLevel() : int.MaxValue);
                        _status = r.Ok ? $"{sp.DisplayName} alındı: {a?.Name}." : r.Reason;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            else if (_tab == 0 || Wares().Count == 0)
            {
                GUILayout.Label($"Pazarlık (ticaret): satış x{_market.TradeSellFactor:0.00} · alış x{_market.TradeBuyFactor:0.00}");
                if (Goods().Count == 0) GUILayout.Label("Bu dükkân mal satmıyor.");
                foreach (var g in Goods())
                {
                    GUILayout.BeginHorizontal();
                    IsoArt.IconCell(g.Id);
                    GUILayout.Label($"{g.DisplayName} ({_house.Count(g.Id)})", GUILayout.Width(130f));
                    if (_market.Stock != null) GUILayout.Label(GameLocalization.T("ui.village.stock", ("n", _market.Stock.Available(g.Id))), GUILayout.Width(52f));
                    if (_house.Count(g.Id) > 0 && GUILayout.Button($"Sat {_market.SellPrice(g.Id)}", GUILayout.Width(56f)))
                    {
                        int earned = _market.Sell(g.Id, 1);
                        _status = earned > 0 ? $"{g.DisplayName} satıldı: +{earned} akçe." : $"{g.DisplayName} yok.";
                    }
                    if (!_market.IsBuyable(g.Id)) GUILayout.Label("yalnızca yapılır", GUILayout.Width(90f));
                    else if (GUILayout.Button($"Al {_market.BuyPrice(g.Id)}", GUILayout.Width(52f)))
                    {
                        int paid = _market.BuyPrice(g.Id);
                        var r = _market.Buy(g.Id);
                        _status = r.Ok ? $"{g.DisplayName} alındı." : r.Reason;
                        if (r.Ok) Treat(owner, paid);
                        if (r.Ok && _shop != null && _shop.Id == "tavern") TavernOrdered?.Invoke(_player(), g.Id);   // "her zamankinden mi?" (belge 55)
                    }
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Label("Alınan eşya hane envanterine girer; yapı modunda yerleştirilir.");
                foreach (var d in Wares())
                {
                    GUILayout.BeginHorizontal();
                    IsoArt.IconCell(d.Id);
                    GUILayout.Label(d.DisplayName, GUILayout.Width(150f));
                    if (!_build.IsUnlocked(d)) GUILayout.Label(_build.LockText(d), GUILayout.Width(110f));
                    else if (GUILayout.Button($"Al {_build.PriceFor(d)}", GUILayout.Width(64f)))
                    {
                        int paid = _build.PriceFor(d);
                        var r = _build.Buy(d.Id);
                        _status = r.Ok ? $"{d.DisplayName} alındı (envanterde)." : r.Reason;
                        if (r.Ok) Treat(owner, paid);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUI.enabled = true;
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
