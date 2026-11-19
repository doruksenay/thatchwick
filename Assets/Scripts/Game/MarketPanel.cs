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
    /// Köy pazarı paneli (geçici IMGUI). İşlem için oyuncunun karakteri pazar tezgâhının yanında
    /// olmalı (en çok 2 karo); değilse "Pazara yürü" ile oraya gider. Kurallar Core.Economy.Market'te.
    /// </summary>
    public sealed class MarketPanel
    {
        private const int NearDistance = 2;

        private readonly Market _market;
        private readonly Household _house;
        private readonly StorageService _storage;
        private readonly ContentDatabase _content;
        private readonly WorldState _world;
        private readonly Func<Character> _player;
        private readonly Func<JobRunner> _runner;
        private readonly int _stallId;
        private Vector2 _scroll;
        private string _status = "";

        public bool Active { get; private set; }

        /// <summary>Gezgin tüccar (belge 34-D5); null ise bölüm görünmez.</summary>
        public TravelingMerchant Merchant { get; set; }
        public Func<long> DayIndex { get; set; }

        public MarketPanel(Market market, Household house, StorageService storage, ContentDatabase content, WorldState world,
            Func<Character> player, Func<JobRunner> runner, int stallId)
        {
            _market = market; _house = house; _storage = storage; _content = content; _world = world;
            _player = player; _runner = runner; _stallId = stallId;
        }

        public void Toggle() { Active = !Active; _status = ""; }
        public void Close() { Active = false; }

        public Rect PanelRect(float guiScale) =>
            new Rect(Screen.width / guiScale - 368f, 8f, 360f, Screen.height / guiScale - 16f);

        private ItemInstance Stall => _world.GetItem(_stallId);

        public bool PlayerIsNear()
        {
            var p = _player();
            var stall = Stall;
            if (p == null || stall == null) return false;
            foreach (var t in stall.OccupiedTiles())
                if (Math.Max(Math.Abs(t.X - p.Position.X), Math.Abs(t.Y - p.Position.Y)) <= NearDistance) return true;
            return false;
        }

        private void WalkToStall()
        {
            var stall = Stall;
            var runner = _runner();
            if (stall == null || runner == null) return;
            var dirs = new[] { new GridPos(0, -1), new GridPos(0, 1), new GridPos(-1, 0), new GridPos(1, 0) };
            foreach (var t in stall.OccupiedTiles())
                foreach (var d in dirs)
                {
                    var n = t + d;
                    if (!_world.Map.IsWalkable(n)) continue;
                    runner.Command(Job.MoveTo(n));
                    _status = "Pazara yürüyor...";
                    return;
                }
            _status = "Tezgâhın yanı kapalı.";
        }

        public void OnGUI(float guiScale)
        {
            if (!Active) return;
            GUILayout.BeginArea(PanelRect(guiScale), GUI.skin.box);
            GUILayout.BeginHorizontal();
            UiSkin.Wrap($"PAZAR · Akçe {_house.Akce} · Depo {_storage.Used}/{_storage.Capacity}", 344f);
            if (GUILayout.Button("Kapat", GUILayout.Width(56f))) Active = false;
            GUILayout.EndHorizontal();
            if (_status.Length > 0) UiSkin.Wrap(_status, 344f);

            bool near = PlayerIsNear();
            if (!near)
            {
                UiSkin.Wrap("Alışveriş için tezgâhın yanında olmalısın (köyün doğusu, çit geçidinden çık).", 344f);
                if (GUILayout.Button("Pazara yürü")) WalkToStall();
            }

            _scroll = GUILayout.BeginScrollView(_scroll);
            GUI.enabled = near;

            UiSkin.Wrap("— Bugünün teklifleri —", UiSkin.MarketTextW);
            foreach (var o in _market.Offers)
            {
                GUILayout.BeginHorizontal();
                UiSkin.Wrap(_market.Describe(o), 205f);   // sabit genişlik: Kabul düğmesi panelden taşmasın
                if (o.Taken) GUILayout.Label("alındı", GUILayout.Width(48f));
                else if (GUILayout.Button("Kabul", GUILayout.Width(52f)))
                {
                    var r = _market.Accept(o.Id);
                    _status = r.Ok ? "Anlaşma tamam." : r.Reason;
                }
                GUILayout.EndHorizontal();
            }

            DrawMerchant();
            UiSkin.Wrap(_market.Season == Thatchwick.Core.Time.Season.Winter ? "— Mallar (kış: yakıt ve yiyecek pahalı) —" : "— Mallar —", UiSkin.MarketTextW);
            UiSkin.Wrap($"Pazarlık (ticaret becerisi): satış x{_market.TradeSellFactor:0.00} · alış x{_market.TradeBuyFactor:0.00}", UiSkin.MarketTextW);
            foreach (var g in _market.Goods())
            {
                GUILayout.BeginHorizontal();
                IsoArt.IconCell(g.Id);
                GUILayout.Label($"{g.DisplayName} ({_house.Count(g.Id)})", GUILayout.Width(120f));
                if (GUILayout.Button($"Sat {_market.SellPrice(g.Id)}", GUILayout.Width(52f))) Report(_market.Sell(g.Id, 1), g);
                if (GUILayout.Button("Sat 5", GUILayout.Width(44f))) Report(_market.Sell(g.Id, 5), g);
                if (!_market.IsBuyable(g.Id)) GUILayout.Label("yalnızca yapılır", GUILayout.Width(90f));
                else if (GUILayout.Button($"Al {_market.BuyPrice(g.Id)}", GUILayout.Width(48f)))
                {
                    var r = _market.Buy(g.Id);
                    _status = r.Ok ? $"{g.DisplayName} alındı." : r.Reason;
                }
                GUILayout.EndHorizontal();
            }

            GUI.enabled = true;
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        // Gezgin tüccar ve panayır: uzak diyar malları, damızlık, sipariş.
        private void DrawMerchant()
        {
            var m = Merchant;
            if (m == null || (!m.Here && m.Order == null)) return;
            UiSkin.Wrap(m.Here ? (m.IsFair ? "— Panayır: gezgin tüccar —" : "— Gezgin tüccar —") : "— Tüccarın siparişi (pazar günü gelir) —", UiSkin.MarketTextW);
            if (m.Here)
            {
                if (m.Offers.Count == 0) UiSkin.Wrap("Tüccarın malı tükendi.", UiSkin.MarketTextW);
                foreach (var o in new List<MerchantOffer>(m.Offers))
                {
                    GUILayout.BeginHorizontal();
                    string name;
                    if (o.Kind == MerchantOfferKind.Breed)
                    {
                        GUILayout.Space(28f);
                        name = (_content.Animals.TryGet(o.Id, out var sp) ? sp.DisplayName : o.Id) + " (damızlık, soy hayvanı)";
                    }
                    else
                    {
                        IsoArt.IconCell(o.Id);
                        name = (_content.Items.TryGet(o.Id, out var d) ? d.DisplayName : o.Id) + $" x{o.Count}";
                    }
                    GUILayout.Label(name, GUILayout.Width(150f));
                    if (GUILayout.Button($"Al {m.UnitPrice(o)}", GUILayout.Width(60f)))
                    {
                        var r = m.Buy(o);
                        _status = r.Ok ? (o.Kind == MerchantOfferKind.Breed ? "Damızlık barınağına yerleşti." : "Alındı.") : r.Reason;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            if (m.Order != null)
            {
                var od = m.Order;
                string item = _content.Items.TryGet(od.ItemId, out var idf) ? idf.DisplayName : od.ItemId;
                string bonus = od.BonusItemId != null && _content.Items.TryGet(od.BonusItemId, out var bd) ? $" + {bd.DisplayName}" : "";
                GUILayout.BeginHorizontal();
                IsoArt.IconCell(od.ItemId);
                UiSkin.Wrap($"Sipariş: {item} {_house.Count(od.ItemId)}/{od.Count} → {od.RewardAkce} akçe, itibar +{od.RewardReputation * (m.IsFair ? 2 : 1)}{bonus}", 200f);
                string why = m.DeliverBlocker();
                GUI.enabled = why == null;
                if (GUILayout.Button(new GUIContent("Teslim et", why ?? ""), GUILayout.Width(70f)))
                    _status = m.Deliver(DayIndex != null ? DayIndex() : 0) ?? "Sipariş teslim edildi; tüccar yeni sipariş bıraktı.";
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
        }

        private void Report(int earned, ItemDef g) =>
            _status = earned > 0 ? $"{g.DisplayName} satıldı: +{earned} akçe (çok satınca fiyat düşer, ertesi gün toparlanır)." : $"{g.DisplayName} yok.";
    }
}
