using System;
using System.Collections.Generic;
using UnityEngine;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Life;
using Thatchwick.Core.Life.Requests;

namespace Thatchwick.Game
{
    /// <summary>
    /// İstekler defteri (I, geçici IMGUI; belge 46 §12). Sekmeler: Ricalar · Siparişler · Mevsim · Hikâyeler · Adaklar; açık
    /// çağrılar her sekmenin üstünde. Kurallar RequestService'te; burası yalnızca çağırır ve nedeni gösterir.
    /// Görseller (defter çerçevesi, tür ikonları, pano kâğıtları) PixelLab kuyruğunda; gelene kadar düz kutu.
    /// </summary>
    public sealed class RequestPanel
    {
        private static readonly string[] Tabs = { "Ricalar", "Siparişler", "Mevsim", "Hikâyeler", "Adaklar" };

        private readonly LifeSystems _life;
        private readonly Func<Character> _commander;
        private int _tab;
        private Vector2 _scroll;
        private string _status = "";
        private int _assignFor;

        public bool Active { get; private set; }
        /// <summary>Haritada göster: verenin konumuna kamerayı götürür (oyun katmanı).</summary>
        public Action<Character> ShowOnMap { get; set; }

        public RequestPanel(LifeSystems life, Func<Character> commander)
        {
            _life = life;
            _commander = commander;
        }

        private RequestService Req => _life.Requests;

        public void Toggle() { Active = !Active; _status = ""; _assignFor = 0; }
        public void Close() { Active = false; }

        public Rect PanelRect(float guiScale) =>
            new Rect(Screen.width / guiScale - 388f, 8f, 380f, Screen.height / guiScale - 16f);

        public void OnGUI(float guiScale)
        {
            if (!Active) return;
            GUILayout.BeginArea(PanelRect(guiScale), GUI.skin.box);
            GUILayout.BeginHorizontal();
            int unread = Req.UnreadCount;
            GUILayout.Label($"İSTEKLER · itibar {_life.Upbringing.Reputation.Points}{(unread > 0 ? $" · panoda {unread} okunmamış" : "")}");
            if (GUILayout.Button("Kapat", GUILayout.Width(56f))) Active = false;
            GUILayout.EndHorizontal();
            if (unread > 0 && !Req.Literate)
                GUILayout.Label("Yeni ilanları okumak için meydandaki ilan tahtasına git (\"İlanları oku\"). Hanede biri okumayı öğrenirse pano buradan okunur.");
            int tab = GUILayout.Toolbar(_tab, Tabs);
            if (tab != _tab) { _tab = tab; _status = ""; _assignFor = 0; }
            if (_status.Length > 0) GUILayout.Label(_status);
            _scroll = GUILayout.BeginScrollView(_scroll);
            DrawCalls();
            switch (_tab)
            {
                case 0: DrawList(RequestKind.Favor, "Askıda rica yok. Köylülerle konuş ya da panoya bak."); break;
                case 1: DrawOrders(); break;
                case 2: DrawSeason(); break;
                case 3: DrawStories(); break;
                default: DrawVows(); break;
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        // ---- çağrılar ----

        private void DrawCalls()
        {
            bool any = false;
            foreach (var r in Req.All)
            {
                if (r.Kind != RequestKind.Call || !r.IsOpen) continue;
                if (!any) { GUILayout.Label("— ÇAĞRI —"); any = true; }
                if (!r.Read) { GUILayout.Label("! Panoda acil bir ilan var (okumak için panoya git)."); continue; }
                GUILayout.Label($"! {r.Title} · {r.GiverName} · {Req.TimeLeft(r)}");
                GUILayout.Label("   " + r.Text);
                if (r.State == RequestState.Accepted)
                {
                    var solver = _life.Population.Find(r.SolverId);
                    GUILayout.Label($"   {solver?.Name ?? "Biri"} yolda: {r.Paths[r.ChosenPath].Label}.");
                    continue;
                }
                var c = _commander();
                for (int i = 0; i < r.Paths.Count; i++)
                {
                    var p = r.Paths[i];
                    string why = Req.PathBlocker(r, i, c);
                    string cost = (p.Skill.HasValue ? $"{SkillNames.Name(p.Skill.Value)} {p.Level}" : "") +
                                  (p.ItemId != null ? $" · {p.ItemCount} {Req.ItemName(p.ItemId)}" : "") +
                                  (p.Akce > 0 ? $" · {p.Akce} akçe" : "") + (p.Hours > 0 ? $" · {p.Hours:0} saat" : "");
                    GUI.enabled = why == null;
                    if (GUILayout.Button(new GUIContent($"{p.Label} ({cost.Trim(' ', '·')})", why ?? p.Outcome)))
                    {
                        Req.ChoosePath(r.Id, i, c, out var msg);
                        _status = msg;
                    }
                    GUI.enabled = true;
                    if (why != null) GUILayout.Label("      " + why);
                }
            }
            if (any) GUILayout.Space(6f);
        }

        // ---- ortak satır ----

        private void DrawList(RequestKind kind, string empty)
        {
            int n = 0;
            foreach (var r in Req.All)
            {
                if (r.Kind != kind || !r.IsOpen) continue;
                DrawRequest(r);
                n++;
            }
            if (n == 0) GUILayout.Label(empty);
            DrawRecent(kind);
        }

        private void DrawRecent(RequestKind kind)
        {
            bool header = false;
            for (int i = Req.All.Count - 1, shown = 0; i >= 0 && shown < 5; i--)
            {
                var r = Req.All[i];
                if (r.Kind != kind || r.IsOpen) continue;
                if (!header) { GUILayout.Label("— Geçmiş —"); header = true; }
                GUILayout.Label($"   {StateText(r.State)} · {r.Title} ({r.GiverName})");
                shown++;
            }
        }

        private static string StateText(RequestState s)
        {
            switch (s)
            {
                case RequestState.Done: return "✓ bitti";
                case RequestState.Failed: return "✗ yetişmedi";
                case RequestState.Expired: return "süresi doldu";
                case RequestState.Rival: return "komşu hane aldı";
                case RequestState.Dropped: return "bırakıldı";
                default: return "";
            }
        }

        private void DrawRequest(Request r)
        {
            if (!r.Read)
            {
                GUILayout.Label($"• Panoda okunmamış bir {Request.KindName(r.Kind).ToLowerInvariant()} ilanı.");
                return;
            }
            string tags = (r.Master ? " · USTA" : "") + (r.Stretch ? " · zorlayıcı" : "") +
                          (r.Skill.HasValue ? $" · {SkillNames.Name(r.Skill.Value)} {r.Level}" : "");
            GUILayout.Label(UiSkin.C($" {r.Title} · {r.GiverName}{tags}", IconKey(r.Kind)));
            if (!string.IsNullOrEmpty(r.Text)) GUILayout.Label("   " + r.Text);
            string state = r.State == RequestState.Posted ? "panoda" : "üstlenildi";
            GUILayout.Label($"   {state} · kalan {Req.TimeLeft(r)}{(r.Kind == RequestKind.Seasonal ? $" · günler {r.DayFrom}-{r.DayTo}" : "")}");
            foreach (var o in r.CurrentObjectives) GUILayout.Label($"   ○ {Req.ObjectiveText(o)}");
            GUILayout.Label($"   ödül: {Req.RewardText(r.Kind == RequestKind.Vow && r.Stage < r.Stages.Count ? r.Stages[r.Stage].Reward : r.Reward)}");
            if (r.AssignedTo != 0)
            {
                var who = _life.Population.Find(r.AssignedTo);
                if (who != null) GUILayout.Label($"   atanan: {who.Name}{(who.Away ? " (görevde)" : "")}");
            }
            DrawButtons(r);
            if (_assignFor == r.Id) DrawAssign(r);
            GUILayout.Space(4f);
        }

        /// <summary>Tür ikonu (Resources/UI/Icons/ui_icon_request_*.png, PixelLab işi 480); yoksa yalnız yazı.</summary>
        public static string IconKey(RequestKind k) => "request_" + k.ToString().ToLowerInvariant();

        private bool HasDeliverable(Request r)
        {
            foreach (var o in r.CurrentObjectives)
                if (!o.Done && (o.Kind == ObjectiveKind.Deliver || o.Kind == ObjectiveKind.Pay)) return true;
            return false;
        }

        private void DrawButtons(Request r)
        {
            var c = _commander();
            GUILayout.BeginHorizontal();
            GUILayout.Space(12f);
            if (r.State == RequestState.Posted)
            {
                string why = Req.CanAccept(r);
                GUI.enabled = why == null;
                if (GUILayout.Button(new GUIContent("Kabul et", why ?? ""), GUILayout.Width(80f))) { Req.Accept(r.Id, out var m); _status = m; }
                GUI.enabled = true;
            }
            if (HasDeliverable(r))
            {
                if (GUILayout.Button("Teslim et", GUILayout.Width(80f))) { Req.Deliver(r.Id, c, false, out var m); _status = m; }
                var giver = _life.Population.Find(r.GiverId);
                bool courier = giver != null && !giver.IsDead && r.Kind != RequestKind.Vow && r.Kind != RequestKind.Bequest && r.Kind != RequestKind.Seasonal;
                if (courier && GUILayout.Button($"Ulakla ({RequestService.CourierFee} akçe)", GUILayout.Width(110f))) { Req.Deliver(r.Id, c, true, out var m); _status = m; }
            }
            if (r.Kind != RequestKind.Vow && r.Kind != RequestKind.Call && GUILayout.Button(_assignFor == r.Id ? "Ata…" : "Ata", GUILayout.Width(50f)))
                _assignFor = _assignFor == r.Id ? 0 : r.Id;
            if (ShowOnMap != null && r.GiverId != 0)
            {
                var giver = _life.Population.Find(r.GiverId);
                if (giver != null && !giver.IsDead && GUILayout.Button("Haritada", GUILayout.Width(70f))) ShowOnMap(giver);
            }
            if (GUILayout.Button(r.Kind == RequestKind.Vow ? "Bırak (yarım)" : "Bırak", GUILayout.Width(r.Kind == RequestKind.Vow ? 96f : 50f)))
            {
                Req.Drop(r.Id, out var m);
                _status = m;
            }
            GUILayout.EndHorizontal();
        }

        private void DrawAssign(Request r)
        {
            GUILayout.Label("   Kim gitsin? Eşya isteği uygun yere süreli göreve çevrilir (azığını alır, dönünce teslim eder).");
            foreach (var m in Req.Household())
            {
                if (m.Stage == AgeStage.Baby) continue;
                string duty = null;
                foreach (var o in r.CurrentObjectives)
                {
                    if (o.Done) continue;
                    var order = Req.DutyFor(r, o, m);
                    if (order != null) { duty = Thatchwick.Core.Work.TimedDutyService.Place(order.PlaceId)?.Name; break; }
                }
                GUILayout.BeginHorizontal();
                GUILayout.Space(18f);
                GUI.enabled = !m.Away;
                if (GUILayout.Button($"{m.Name} ({AgeStages.Name(m.Stage)}){(duty != null ? $" → {duty}" : " · elle")}", GUILayout.Width(300f)))
                {
                    Req.Assign(r.Id, m, out var msg);
                    _status = msg;
                    _assignFor = 0;
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
        }

        // ---- siparişler ----

        private void DrawOrders()
        {
            int accepted = 0;
            foreach (var r in Req.All) if (r.Kind == RequestKind.Order && r.State == RequestState.Accepted) accepted++;
            GUILayout.Label($"Siparişler pazar günü panoya asılır. Üstlenilen {accepted}/{Req.MaxOrders}. Bir gün alınmayan sipariş komşu haneye gider; kabul edip yetiştirmezsen itibar −2.");
            DrawList(RequestKind.Order, "Panoda sipariş yok; pazar gününü bekle.");
            GUILayout.Label("— İlan sıralaması —");
            int i = 1;
            foreach (var row in Req.Ranking("Senin hanen"))
            {
                GUILayout.Label($"   {i}. {row.Name}{(row.IsPlayer ? " ★" : "")} · {row.Points}");
                if (++i > 8) break;
            }
        }

        // ---- mevsim ----

        private void DrawSeason()
        {
            int done = 0, total = 0;
            double seasonStart = Req.SeasonIndex * (double)Req.DaysPerSeason;
            foreach (var r in Req.All)
            {
                if (r.Kind != RequestKind.Seasonal || r.PostedDay < seasonStart - 1e-6) continue;
                total++;
                if (r.State == RequestState.Done) done++;
            }
            GUILayout.Label($"Mevsim Defteri · {Req.DayOfSeason}. gün · hanenin payı {done}/{total} (en az {RequestService.SeasonSetJobs} iş: mevsim takımından bir parça)");
            foreach (var r in Req.All)
            {
                if (r.Kind != RequestKind.Seasonal || !r.IsOpen) continue;
                int pct = (int)Math.Round(Req.VillageProgress(r) * 100);
                string when = Req.InWindow(r) ? "şimdi" : Req.DayOfSeason < r.DayFrom ? $"{r.DayFrom}. günden" : "geçti";
                GUILayout.Label($"• {r.Title} · köy %{pct} · {when}");
                GUILayout.Label("   " + r.Text);
                foreach (var o in r.Objectives) GUILayout.Label($"   ○ {Req.ObjectiveText(o)}");
                GUILayout.BeginHorizontal();
                GUILayout.Space(12f);
                GUI.enabled = Req.InWindow(r);
                if (HasDeliverable(r) && GUILayout.Button("Teslim et", GUILayout.Width(80f))) { Req.Deliver(r.Id, _commander(), false, out var m); _status = m; }
                GUI.enabled = true;
                if (GUILayout.Button(_assignFor == r.Id ? "Ata…" : "Ata", GUILayout.Width(50f))) _assignFor = _assignFor == r.Id ? 0 : r.Id;
                GUILayout.EndHorizontal();
                if (_assignFor == r.Id) DrawAssign(r);
            }
            DrawRecent(RequestKind.Seasonal);
        }

        // ---- hikâyeler ve vasiyetler ----

        private void DrawStories()
        {
            GUILayout.Label("Köylülerle yakınlık 30, 60 ve 100'e ulaşınca hikâyeleri açılır. Vasiyetler süresizdir.");
            DrawList(RequestKind.Bequest, "Vasiyet yok.");
            DrawList(RequestKind.Story, "Açık hikâye yok; köylülerle konuş, hediye ver.");
        }

        // ---- adaklar ----

        private void DrawVows()
        {
            GUILayout.Label("Adak kuşaklar boyu sürer: aşamalar farklı beceriler ve kuşaklar ister. Bitince Soy Mührü.");
            foreach (var r in Req.All)
            {
                if (r.Kind != RequestKind.Vow || !r.IsOpen) continue;
                if (r.State == RequestState.Posted)
                {
                    GUILayout.Label($"• {r.Title} (son dilek: {r.VowedByName})");
                    GUILayout.Label("   " + r.Text);
                    DrawVowButton(r.TemplateId);
                    continue;
                }
                GUILayout.Label($"• {r.Title} · adayan {r.VowedByName} · aşama {r.Stage + 1}/{r.Stages.Count}");
                for (int i = 0; i < r.Stages.Count; i++)
                    GUILayout.Label($"   {(i < r.Stage ? "✓" : i == r.Stage ? "→" : "·")} {r.Stages[i].Title}");
                foreach (var o in r.CurrentObjectives) GUILayout.Label($"      ○ {Req.ObjectiveText(o)}");
                DrawButtons(r);
            }
            GUILayout.Label("— Adanabilir —");
            foreach (var t in Req.AvailableVows())
            {
                GUILayout.Label($"{t.Title}: {t.Text} ({t.Stages.Count} aşama, Soy Mührü +{t.FinalSeals})");
                DrawVowButton(t.Id);
            }
        }

        private void DrawVowButton(string templateId)
        {
            var c = _commander();
            string why = Req.CanVow(templateId, c);
            GUILayout.BeginHorizontal();
            GUILayout.Space(12f);
            GUI.enabled = why == null;
            if (GUILayout.Button(new GUIContent(c != null ? $"{c.Name} adasın" : "Ada", why ?? ""), GUILayout.Width(140f)))
            {
                Req.Vow(templateId, c, out var m);
                _status = m;
            }
            GUI.enabled = true;
            if (why != null) GUILayout.Label(why);
            GUILayout.EndHorizontal();
        }
    }
}
