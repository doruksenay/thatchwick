using System;
using System.Collections.Generic;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Economy;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.World;

namespace Thatchwick.Game.Ui
{
    /// <summary>
    /// Köy dükkânı penceresi (UI Toolkit, <see cref="ListWindow"/>; belge 41, 66f; eski IMGUI ShopPanel'in yerine, belge 66g).
    /// Dükkânın içine dokununca açılır. Alışveriş için oyuncu dükkânın içinde, dükkân açık ve sahibi tezgâhta olmalı (Stardew gibi).
    /// Mallar Market kurallarıyla (ticaret becerisi, mevsim, talep, günlük stok), eşyalar BuildService ile satılır; dükkân
    /// yalnızca kendi mallarını alır. Pazar, ustanın bunalımı, hastalığı ya da yası kapıda yazar; keyifli usta ikram eder.
    /// </summary>
    public sealed class ShopWindow
    {
        private readonly Market _market;
        private readonly BuildService _build;
        private readonly Household _house;
        private readonly ContentDatabase _content;
        private readonly WorldState _world;
        private readonly Func<Character> _player;
        private readonly Func<JobRunner> _runner;
        private readonly Func<ShopDef, Character> _owner;
        private readonly Dictionary<string, RectI> _areas;
        private readonly Func<UiHost> _host;
        private ListWindow _window;
        private ShopDef _shop;
        private string _status = "";
        private int _tab;   // 0 mallar, 1 eşyalar, 2 hayvanlar
        private List<ItemDef> _goods, _wares;

        /// <summary>Hayvan pazarında hayvan satışı için (barınak ve kilit kuralları AnimalSystem.Buy'da).</summary>
        public Thatchwick.Core.Animals.AnimalSystem Animals { get; set; }
        public Func<int> AccountLevel { get; set; }
        /// <summary>Meyhaneden bir şey alındı (alan, eşya): "her zamankinden mi?" easter egg'i (belge 55 §2b).</summary>
        public Action<Character, string> TavernOrdered { get; set; }
        /// <summary>Köy hayatı (belge 66f): Pazar, ustanın bunalımı/hastalığı/yası, çağrı, stok ve keyifli ustanın ikramı.</summary>
        public LivingVillage Village { get; set; }
        /// <summary>Saat ve gün (Village bağlı değilse dükkân saatleri için).</summary>
        public Func<double> Hour { get; set; }
        public Func<long> Day { get; set; }
        /// <summary>Pencere açılmadan önce (altta kalacak IMGUI panellerini kapatmak için).</summary>
        public event Action Opening;

        public ShopWindow(Market market, BuildService build, Household house, ContentDatabase content, WorldState world,
            Func<Character> player, Func<JobRunner> runner, Func<ShopDef, Character> owner, Dictionary<string, RectI> areas, Func<UiHost> host)
        {
            _market = market; _build = build; _house = house; _content = content; _world = world;
            _player = player; _runner = runner; _owner = owner; _areas = areas; _host = host;
        }

        public bool Active => _window != null && _window.Visible;
        public ShopDef Shop => _shop;

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
            var host = _host?.Invoke();
            if (host == null) return;
            if (_shop != shop) { _goods = null; _wares = null; _tab = 0; }
            _shop = shop;
            _status = "";
            if (_window == null) _window = new ListWindow(host, Build, () => T("ui.common.close"), 600f, 8);
            Opening?.Invoke();
            _window.Visible = true;
            _window.Refresh();
        }

        public void Close() { if (_window != null) _window.Visible = false; }

        /// <summary>Açıkken içerik dışarıdan değişir (sahibi geldi, saat döndü): oyun katmanı ara ara çağırır.</summary>
        public void Refresh() { if (Active) _window.Refresh(); }

        private static string T(string key) => GameLocalization.T(key);
        private static string T(string key, params (string, object)[] args) => GameLocalization.T(key, args);

        private bool PlayerInside()
        {
            var p = _player();
            return p != null && _shop != null && _areas != null && _areas.TryGetValue(_shop.Id, out var r) && r.Contains(p.Position);
        }

        /// <summary>Oyuncu dükkânın içinde kendisine en yakın açık karoya yürür.</summary>
        public void WalkIn()
        {
            var p = _player(); var runner = _runner();
            if (p == null || runner == null || _shop == null || _areas == null || !_areas.TryGetValue(_shop.Id, out var r)) return;
            GridPos best = default; int bestD = int.MaxValue;
            for (int y = r.Y; y < r.Y + r.H; y++)
                for (int x = r.X; x < r.X + r.W; x++)
                {
                    var t = new GridPos(x, y);
                    if (!_world.Map.IsWalkable(t)) continue;
                    int d = Math.Abs(x - p.Position.X) + Math.Abs(y - p.Position.Y);
                    if (d < bestD) { bestD = d; best = t; }
                }
            if (bestD == int.MaxValue) { _status = T("ui.shopw.closed_inside"); return; }
            runner.Command(Job.MoveTo(best));
            _status = T("ui.shopw.walking", ("shop", _shop.Name));
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
                _status += " " + T("ui.village.treat_item", ("item", _content.Items.TryGet(treat.ItemId, out var d) ? d.DisplayName : treat.ItemId));
            }
            else if (treat.Discount > 0 && paid > 0)
            {
                int back = Math.Max(1, (int)Math.Round(paid * treat.Discount));
                _house.AddAkce(back);
                _status += " " + T("ui.village.treat_discount", ("percent", (int)Math.Round(treat.Discount * 100)), ("akce", back));
            }
        }

        private ListWindow.Model Build()
        {
            var m = new ListWindow.Model { Title = T("ui.shopw.title", ("shop", _shop?.Name ?? ""), ("akce", _house.Akce)), Status = _status };
            if (_shop == null) return m;
            var owner = _owner(_shop);
            bool open = Village != null ? Village.IsShopOpen(_shop, owner) : _shop.IsOpenAt(Hour != null ? Hour() : 12, Day != null ? Day() : 0);
            var closure = Village != null ? Village.ClosureOf(_shop, owner) : ShopClosure.None;
            bool ownerHere = owner != null && _areas != null && _areas.TryGetValue(_shop.Id, out var area) && area.Contains(owner.Position);
            bool inside = PlayerInside();
            bool canTrade = inside && open && ownerHere;

            var info = new List<string> { _shop.Blurb, T("ui.shopw.hours", ("hours", _shop.HoursText), ("state", T(open ? "ui.shopw.open" : "ui.shopw.closed"))) };
            info.Add(owner == null ? T("ui.shopw.no_owner") : ownerHere ? T("ui.shopw.owner_here", ("name", owner.Name)) : T("ui.shopw.owner_away", ("name", owner.Name)));
            if (closure != ShopClosure.None) info.Add(T("ui.village.closed_reason", ("reason", GameLocalization.R(LivingVillage.ClosureText(closure)))));
            if (!canTrade && inside) info.Add(T(!open ? "ui.shopw.come_open" : "ui.shopw.wait_owner"));
            m.Info = string.Join("\n", info);

            var tabs = m.AddTabRow();
            tabs.Add(new ListWindow.Act(T("ui.shopw.tab.goods"), () => _tab = 0, on: _tab == 0));
            if (Wares().Count > 0) tabs.Add(new ListWindow.Act(T("ui.shopw.tab.wares", ("n", Wares().Count)), () => _tab = 1, on: _tab == 1));
            bool animalShop = _shop.Id == "animals" && Animals != null;
            if (animalShop) tabs.Add(new ListWindow.Act(T("ui.shopw.tab.animals"), () => _tab = 2, on: _tab == 2));
            if (_tab == 1 && Wares().Count == 0) _tab = 0;
            if (_tab == 2 && !animalShop) _tab = 0;

            if (!inside) m.Footer.Add(new ListWindow.Act(T("ui.shopw.walk_in"), () => { WalkIn(); Close(); }));

            if (_tab == 2) AnimalRows(m, canTrade);
            else if (_tab == 1) WareRows(m, canTrade, owner);
            else GoodRows(m, canTrade, owner);
            return m;
        }

        private void GoodRows(ListWindow.Model m, bool canTrade, Character owner)
        {
            m.FooterLabel = T("ui.shopw.haggle_factors", ("sell", _market.TradeSellFactor.ToString("0.00")), ("buy", _market.TradeBuyFactor.ToString("0.00")));
            if (Goods().Count == 0) { m.Rows.Add(new ListWindow.Row(T("ui.shopw.no_goods"))); return; }
            foreach (var g in Goods())
            {
                var item = g;
                string detail = T("ui.shopw.have", ("n", _house.Count(item.Id)));
                if (_market.Stock != null) detail += " · " + T("ui.village.stock", ("n", _market.Stock.Available(item.Id)));
                var row = new ListWindow.Row(item.DisplayName, detail);
                if (_house.Count(item.Id) > 0)
                    row.Actions.Add(new ListWindow.Act(T("ui.shopw.sell", ("price", _market.SellPrice(item.Id))), () =>
                    {
                        int earned = _market.Sell(item.Id, 1);
                        _status = earned > 0 ? T("ui.shopw.sold", ("item", item.DisplayName), ("akce", earned)) : T("ui.common.not_held");
                    }, disabled: !canTrade));
                if (!_market.IsBuyable(item.Id)) row.Detail += " · " + T("ui.shopw.craft_only");
                else
                    row.Actions.Add(new ListWindow.Act(T("ui.shopw.buy", ("price", _market.BuyPrice(item.Id))), () =>
                    {
                        int paid = _market.BuyPrice(item.Id);
                        var r = _market.Buy(item.Id);
                        _status = r.Ok ? T("ui.shopw.bought", ("item", item.DisplayName)) : r.Reason;
                        if (!r.Ok) return;
                        Treat(owner, paid);
                        if (_shop.Id == "tavern") TavernOrdered?.Invoke(_player(), item.Id);   // "her zamankinden mi?" (belge 55)
                    }, disabled: !canTrade));
                m.Rows.Add(row);
            }
        }

        private void WareRows(ListWindow.Model m, bool canTrade, Character owner)
        {
            m.FooterLabel = T("ui.shopw.wares_help");
            foreach (var d in Wares())
            {
                var def = d;
                var row = new ListWindow.Row(def.DisplayName);
                if (!_build.IsUnlocked(def)) row.Detail = _build.LockText(def);
                else
                    row.Actions.Add(new ListWindow.Act(T("ui.shopw.buy", ("price", _build.PriceFor(def))), () =>
                    {
                        int paid = _build.PriceFor(def);
                        var r = _build.Buy(def.Id);
                        _status = r.Ok ? T("ui.shopw.bought_ware", ("item", def.DisplayName)) : r.Reason;
                        if (r.Ok) Treat(owner, paid);
                    }, disabled: !canTrade));
                m.Rows.Add(row);
            }
        }

        private void AnimalRows(ListWindow.Model m, bool canTrade)
        {
            m.FooterLabel = T("ui.shopw.animals_help");
            foreach (var sp in _content.Animals.All)
            {
                if (sp.Price <= 0) continue;
                var species = sp;
                var row = new ListWindow.Row(species.DisplayName);
                row.Actions.Add(new ListWindow.Act(T("ui.shopw.buy", ("price", species.Price)), () =>
                {
                    var r = Animals.Buy(species.Id, _house, out var a, AccountLevel != null ? AccountLevel() : int.MaxValue);
                    _status = r.Ok ? T("ui.shopw.bought_animal", ("item", species.DisplayName), ("name", a?.Name ?? "")) : r.Reason;
                }, disabled: !canTrade));
                m.Rows.Add(row);
            }
        }
    }
}
