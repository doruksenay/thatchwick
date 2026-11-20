using System;
using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Animals;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Content;
using Thatchwick.Core.Farmyard;
using Thatchwick.Core.Life;

namespace Thatchwick.Game
{
    /// <summary>
    /// Arabalar paneli (geçici IMGUI; belge 57e §2.4): her arabanın görevi, çeki hayvanı ve arabacısı buradan seçilir.
    /// Yorgun, hasta ya da başka arabaya koşulu hayvan seçilince uyarı hemen görünür ("Öküz Karabaş bugün çok yoruldu,
    /// yapamaz"). Kurallar Core.Farmyard.CartFleet'te.
    /// </summary>
    public sealed class CartPanel
    {
        private readonly LifeSystems _life;
        private readonly AnimalSystem _animals;
        private readonly ContentDatabase _content;
        private readonly Dictionary<string, (CartJob job, int animal, int driver)> _pick = new Dictionary<string, (CartJob, int, int)>(StringComparer.Ordinal);
        private Vector2 _scroll;
        private string _status;

        /// <summary>Arabacı olabilecek hane üyeleri.</summary>
        public Func<IEnumerable<Character>> Family { get; set; }
        public Func<long> Day { get; set; }
        public event Action<string> Message;

        public bool Active { get; private set; }

        public CartPanel(LifeSystems life, AnimalSystem animals, ContentDatabase content)
        {
            _life = life; _animals = animals; _content = content;
        }

        public void Toggle() => Active = !Active;
        public void Close() => Active = false;

        public Rect PanelRect(float guiScale) =>
            new Rect(Screen.width / guiScale - 438f, 8f, 430f, Screen.height / guiScale - 16f);

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

        public void OnGUI(float guiScale)
        {
            if (!Active) return;
            var fleet = _life.Fleet;
            GUILayout.BeginArea(PanelRect(guiScale), GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label(T("ui.cart.title"));
            if (GUILayout.Button(T("ui.cart.close"), GUILayout.Width(56f))) Active = false;
            GUILayout.EndHorizontal();
            if (fleet == null || fleet.Carts.Count == 0)
            {
                GUILayout.Label(T("ui.cart.none"));
                GUILayout.EndArea();
                return;
            }
            long day = Day != null ? Day() : 0;
            bool rule = GUILayout.Toggle(fleet.FertilizeRule, " " + T("ui.cart.rule"));
            if (rule != fleet.FertilizeRule) fleet.FertilizeRule = rule;
            if (!string.IsNullOrEmpty(_status)) GUILayout.Label(_status);
            _scroll = GUILayout.BeginScrollView(_scroll);
            var family = new List<Character>();
            if (Family != null) foreach (var m in Family()) if (!m.IsDead && m.Stage >= AgeStage.Teen) family.Add(m);

            foreach (var cart in new List<Cart>(fleet.Carts))
            {
                if (!_pick.TryGetValue(cart.Key, out var pick)) pick = (cart.Job, cart.AnimalId, cart.DriverId);
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label(Name(cart.DefId) + " · " + GameLocalization.T("ui.cart.load", ("n", fleet.Capacity(cart))) + " · "
                    + GameLocalization.T("ui.cart.wear", ("pct", Mathf.RoundToInt((float)cart.Wear * 100f))));

                GUILayout.BeginHorizontal();
                if (GUILayout.Button(T("ui.cart.job." + (int)pick.job), GUILayout.Width(120f))) pick.job = NextJob(cart, pick.job);
                if (CartFleet.NeedsAnimal(cart))
                {
                    var pullers = Pullers(cart);
                    var chosen = pick.animal != 0 ? _animals.Get(pick.animal) : null;
                    if (GUILayout.Button(chosen != null ? Label(chosen) : T("ui.cart.animal_auto"), GUILayout.Width(140f)))
                    {
                        int at = chosen != null ? pullers.IndexOf(chosen) : -1;
                        chosen = at + 1 < pullers.Count ? pullers[at + 1] : null;
                        pick.animal = chosen != null ? chosen.Id : 0;
                        // Seçer seçmez uyarı: yorgun, hasta, gebe, aç, binek ya da başka arabada.
                        var warn = chosen != null ? fleet.AnimalBlocker(cart, chosen, day) : default;
                        _status = warn.IsEmpty ? null : _life.Text(warn);
                        if (!warn.IsEmpty) Message?.Invoke(_status);
                    }
                }
                var driver = pick.driver != 0 ? family.Find(m => m.Id == pick.driver) : null;
                if (GUILayout.Button(driver != null ? driver.Name : T("ui.cart.driver_none"), GUILayout.Width(120f)))
                {
                    int at = driver != null ? family.IndexOf(driver) : -1;
                    driver = at + 1 < family.Count ? family[at + 1] : null;
                    pick.driver = driver != null ? driver.Id : 0;
                }
                GUILayout.EndHorizontal();
                _pick[cart.Key] = pick;

                GUILayout.BeginHorizontal();
                bool changed = pick.job != cart.Job || pick.animal != cart.AnimalId || pick.driver != cart.DriverId;
                GUI.enabled = changed;
                if (GUILayout.Button(T("ui.cart.assign"), GUILayout.Width(120f)))
                {
                    string why = _life.AssignCart(cart.Key, pick.job, pick.animal, pick.driver);
                    _status = why ?? T("ui.cart.assigned");
                    if (why != null) Message?.Invoke(why);
                    else _pick.Remove(cart.Key);
                }
                GUI.enabled = fleet.CanRepair(cart);
                if (GUILayout.Button(T("ui.cart.repair"), GUILayout.Width(140f)) && fleet.Repair(cart)) _status = T("ui.cart.repaired");
                GUI.enabled = true;
                GUILayout.EndHorizontal();

                if (cart.Job != CartJob.Idle)
                {
                    var why = fleet.Blocker(cart, day);
                    GUILayout.Label(why.IsEmpty ? T("ui.cart.ok") : _life.Text(why));
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
