using System;
using System.Collections.Generic;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Farmyard;
using Thatchwick.Core.Life;

namespace Thatchwick.Game.Ui
{
    /// <summary>
    /// Arabalar paneli (UI Toolkit, <see cref="ListWindow"/>; belge 57e §2.4). Her araba bir satır: görev, çeki hayvanı ve
    /// arabacı düğmeye her basışta sıradakine geçer, "Ata" seçimi uygular. Yorgun, hasta ya da başka arabadaki hayvan
    /// seçilince uyarı hemen alt satırda görünür ("Öküz Karabaş bugün çok yoruldu, yapamaz"). Kurallar Core.Farmyard.CartFleet'te.
    /// </summary>
    public sealed class CartPanel
    {
        private readonly LifeSystems _life;
        private readonly AnimalSystem _animals;
        private readonly ContentDatabase _content;
        private readonly Func<UiHost> _host;
        private readonly Dictionary<string, (CartJob job, int animal, int driver)> _pick = new Dictionary<string, (CartJob, int, int)>(StringComparer.Ordinal);
        private ListWindow _window;
        private string _status;

        /// <summary>Arabacı olabilecek hane üyeleri.</summary>
        public Func<IEnumerable<Character>> Family { get; set; }
        public Func<long> Day { get; set; }
        /// <summary>Uyarı (oyuncuya kısa bildirim olarak da gösterilir).</summary>
        public event Action<string> Message;
        /// <summary>Pencere açılmadan önce (altta kalacak IMGUI panellerini kapatmak için).</summary>
        public event Action Opening;

        public CartPanel(LifeSystems life, AnimalSystem animals, ContentDatabase content, Func<UiHost> host)
        {
            _life = life; _animals = animals; _content = content; _host = host;
        }

        public bool Active => _window != null && _window.Visible;

        public void Toggle()
        {
            if (Active) { Close(); return; }
            var host = _host?.Invoke();
            if (host == null) return;
            if (_window == null) _window = new ListWindow(host, Build, () => T("ui.cart.close"), 620f, 5);
            Opening?.Invoke();
            _status = null;
            _pick.Clear();
            _window.Visible = true;
        }

        public void Close() { if (_window != null) _window.Visible = false; }
        public void Refresh() => _window?.Refresh();

        private static string T(string key) => GameLocalization.T(key);
        private string Name(string id) => id != null && _content.Items.TryGet(id, out var d) ? d.DisplayName : id;
        private static string Label(Animal a) => string.IsNullOrEmpty(a.Name) ? a.Species.DisplayName : a.Species.DisplayName + " " + a.Name;

        private List<Animal> Pullers(Cart cart)
        {
            var list = new List<Animal>();
            foreach (var a in _animals.Animals) if (!a.IsDead && CartFleet.CanPull(cart, a)) list.Add(a);
            return list;
        }

        private static CartJob NextJob(Cart cart, CartJob job)
        {
            for (int i = 0; i < 7; i++)
            {
                job = (CartJob)(((int)job + 1) % 7);
                if (CartFleet.JobAllowed(cart, job)) return job;
            }
            return CartJob.Idle;
        }

        private void Warn(string text)
        {
            _status = text;
            if (!string.IsNullOrEmpty(text)) Message?.Invoke(text);
        }

        private ListWindow.Model Build()
        {
            var fleet = _life.Fleet;
            var m = new ListWindow.Model { Title = T("ui.cart.title"), Status = _status };
            if (fleet == null || fleet.Carts.Count == 0) { m.Info = T("ui.cart.none"); return m; }
            long day = Day != null ? Day() : 0;
            var family = new List<Character>();
            if (Family != null) foreach (var c in Family()) if (!c.IsDead && c.Stage >= AgeStage.Teen) family.Add(c);

            foreach (var cart in new List<Cart>(fleet.Carts))
            {
                if (!_pick.TryGetValue(cart.Key, out var pick)) pick = (cart.Job, cart.AnimalId, cart.DriverId);
                string key = cart.Key;
                var row = new ListWindow.Row(
                    Name(cart.DefId) + " · " + GameLocalization.T("ui.cart.load", ("n", fleet.Capacity(cart))) + " · "
                    + GameLocalization.T("ui.cart.wear", ("pct", (int)Math.Round(cart.Wear * 100))) + " · "
                    + T(cart.ItemId == 0 ? "ui.cart.stored" : fleet.Sheltered(cart) ? "ui.cart.sheltered" : "ui.cart.exposed"));
                if (cart.Job != CartJob.Idle)
                {
                    var why = fleet.Blocker(cart, day);
                    row.Detail = why.IsEmpty ? T("ui.cart.ok") : _life.Text(why);
                }
                row.Marked = cart.Job != CartJob.Idle;

                row.Actions.Add(new ListWindow.Act(T("ui.cart.job." + (int)pick.job), () =>
                {
                    var p = _pick.TryGetValue(key, out var v) ? v : pick;
                    p.job = NextJob(cart, p.job); _pick[key] = p;
                }));
                if (CartFleet.NeedsAnimal(cart))
                {
                    var chosen = pick.animal != 0 ? _animals.Get(pick.animal) : null;
                    row.Actions.Add(new ListWindow.Act(chosen != null ? Label(chosen) : T("ui.cart.animal_auto"), () =>
                    {
                        var p = _pick.TryGetValue(key, out var v) ? v : pick;
                        var pullers = Pullers(cart);
                        var now = p.animal != 0 ? _animals.Get(p.animal) : null;
                        int at = now != null ? pullers.IndexOf(now) : -1;
                        var next = at + 1 < pullers.Count ? pullers[at + 1] : null;
                        p.animal = next != null ? next.Id : 0; _pick[key] = p;
                        // Seçer seçmez uyarı: yorgun, hasta, gebe, aç, binek ya da başka arabada.
                        var warn = next != null ? fleet.AnimalBlocker(cart, next, day) : default;
                        Warn(warn.IsEmpty ? null : _life.Text(warn));
                    }));
                }
                var driver = pick.driver != 0 ? family.Find(c => c.Id == pick.driver) : null;
                row.Actions.Add(new ListWindow.Act(driver != null ? driver.Name : T("ui.cart.driver_none"), () =>
                {
                    var p = _pick.TryGetValue(key, out var v) ? v : pick;
                    var now = p.driver != 0 ? family.Find(c => c.Id == p.driver) : null;
                    int at = now != null ? family.IndexOf(now) : -1;
                    var next = at + 1 < family.Count ? family[at + 1] : null;
                    p.driver = next != null ? next.Id : 0; _pick[key] = p;
                }));
                bool changed = pick.job != cart.Job || pick.animal != cart.AnimalId || pick.driver != cart.DriverId;
                row.Actions.Add(new ListWindow.Act(T("ui.cart.assign"), () =>
                {
                    var p = _pick.TryGetValue(key, out var v) ? v : pick;
                    string why = _life.AssignCart(key, p.job, p.animal, p.driver);
                    if (why != null) Warn(why);
                    else { _status = T("ui.cart.assigned"); _pick.Remove(key); }
                }, disabled: !changed));
                var repair = fleet.RepairBlocker(cart);
                if (cart.Wear >= 0.5)
                    row.Actions.Add(new ListWindow.Act(T("ui.cart.repair"), () => { if (fleet.Repair(cart)) _status = T("ui.cart.repaired"); }, disabled: !repair.IsEmpty));
                if (cart.Wear >= 0.5 && !repair.IsEmpty)
                    row.Detail = string.IsNullOrEmpty(row.Detail) ? _life.Text(repair) : row.Detail + " · " + _life.Text(repair);
                m.Rows.Add(row);
            }

            m.Footer.Add(new ListWindow.Act((fleet.FertilizeRule ? "[x] " : "[ ] ") + T("ui.cart.rule"), () => fleet.FertilizeRule = !fleet.FertilizeRule, on: fleet.FertilizeRule));
            m.Footer.Add(new ListWindow.Act((fleet.StewardCarts ? "[x] " : "[ ] ") + T("ui.cart.steward"), () => fleet.StewardCarts = !fleet.StewardCarts, on: fleet.StewardCarts));
            return m;
        }
    }
}
