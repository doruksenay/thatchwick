using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Thatchwick.Core.Build;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Heat;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Time;
using Thatchwick.Core.World;

namespace Thatchwick.Game
{
    /// <summary>
    /// Denge simülasyonu (belge 41): oynatma kipinde yeni bir oyun kurar, oyuncu hiçbir şey yapmadan (yalnızca
    /// otonomi ve verilen görevlerle) N gün hızlı ilerletir, örnekleri BalanceReport'a yazar; sonunda
    /// Logs/denge/&lt;senaryo&gt;.csv ve .txt dosyalarını bırakır ve oyunu karakter yaratma ekranına döndürür.
    /// Kayıt dosyasına dokunmaz: koşu boyunca GameRoot kapalıdır (günlük otomatik kayıt çalışmaz), sonunda yok edilir.
    /// GameRoot'a dokunmamak için iç alanlara yansımayla erişir (oyun kodu değişirse burada adlar güncellenir).
    /// Editörde: Thatchwick &gt; Denge menüsü. Koşu sırasında oynatma kipinden çıkmayın (kapanışta kayıt alınabilir).
    /// </summary>
    public sealed class BalanceRunner : MonoBehaviour
    {
        public enum Scenario
        {
            /// <summary>Görev yok: hane yalnızca ihtiyaç otonomisiyle yaşar (oyuncu hiç oynamıyor).</summary>
            Idle,
            /// <summary>Oyuncu dışındaki yetişkinler "Tarla" görevinde (ekin, sulama, odun).</summary>
            FarmDuty,
            /// <summary>Yaşlı hane (belge 51 §3): bütün hane üyeleri yaşlı başlar; sıkıntılar normal ayarda.</summary>
            ElderHousehold
        }

        public const double Step = 0.02;
        public const double SamplesPerDay = 4;
        /// <summary>Kare başına ilerletilen oyun günü (editör donmasın diye parça parça).</summary>
        public const double DaysPerFrame = 0.5;

        public static bool Running { get; private set; }
        public static string LastSummary { get; private set; }
        public static string LastCsvPath { get; private set; }

        // Enter Play Mode Options açık: statikler oynatmalar arasında kalır; yarıda kalan koşu "çalışıyor" görünmesin.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() { Running = false; LastSummary = null; LastCsvPath = null; }

        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        private GameRoot _root;
        private double _days;
        private double _startDay;
        private Scenario _scenario;
        private BalanceReport _report;

        /// <summary>Koşuyu başlatır (oynatma kipinde). Zaten koşuyorsa ya da GameRoot yoksa false.</summary>
        public static bool Begin(int days, Scenario scenario, double startDay = 0)
        {
            if (Running || !Application.isPlaying) return false;
            var old = FindFirstObjectByType<GameRoot>();
            if (old != null) { old.enabled = false; Destroy(old.gameObject); }
            var go = new GameObject("BalanceRunner");
            var runner = go.AddComponent<BalanceRunner>();
            runner._days = days;
            runner._scenario = scenario;
            runner._startDay = startDay;
            Running = true;
            runner.StartCoroutine(runner.RunAll());
            return true;
        }

        private static T Get<T>(object o, string field) where T : class
        {
            var f = typeof(GameRoot).GetField(field, Flags) ?? throw new MissingFieldException(nameof(GameRoot), field);
            return f.GetValue(o) as T;
        }

        private IEnumerator RunAll()
        {
            Application.runInBackground = true;
            yield return null;   // eski GameRoot'un yok edilmesi bitsin
            var go = new GameObject("GameRoot");
            _root = go.AddComponent<GameRoot>();
            yield return null;   // Awake/Start
            _root.enabled = false;
            Exception error = null;
            try { Setup(); } catch (Exception e) { error = e; }
            if (error == null)
            {
                var clock = Get<GameClock>(_root, "_clock");
                var simulate = typeof(GameRoot).GetMethod("Simulate", Flags);
                double end = clock.TotalDays + _days;
                double nextSample = clock.TotalDays;
                while (clock.TotalDays < end - 1e-9 && error == null)
                {
                    double frameEnd = Math.Min(end, clock.TotalDays + DaysPerFrame);
                    try
                    {
                        while (clock.TotalDays < frameEnd - 1e-9)
                        {
                            if (clock.TotalDays >= nextSample - 1e-9) { TakeSample(); nextSample += 1.0 / SamplesPerDay; }
                            clock.Restore(clock.TotalDays + Step, clock.RequestedSpeed);
                            simulate.Invoke(_root, new object[] { Step });
                        }
                    }
                    catch (Exception e) { error = e is TargetInvocationException t && t.InnerException != null ? t.InnerException : e; }
                    yield return null;
                }
                if (error == null) TakeSample();
            }
            Finish(error);
        }

        private void Setup()
        {
            if (Get<object>(_root, "_creator") != null)
            {
                typeof(GameRoot).GetField("_namingSettlement", Flags)?.SetValue(_root, false);
                typeof(GameRoot).GetMethod("FinishCreation", Flags).Invoke(_root, null);
            }
            var clock = Get<GameClock>(_root, "_clock");
            if (_startDay > 0) clock.Restore(_startDay, clock.RequestedSpeed);
            var pop = Get<Population>(_root, "_population");
            if (_scenario == Scenario.FarmDuty)
                foreach (var c in pop.Members)
                    if (!c.IsVillager && !c.IsDead && Thatchwick.Core.Work.DutyPlanner.CanTakeDuty(c)) c.Duty = DutyKind.Farm;
            var life0 = Get<LifeSystems>(_root, "_life");
            if (_scenario == Scenario.ElderHousehold)
                foreach (var c in pop.Members)
                    if (!c.IsVillager && !c.IsDead)
                    {
                        c.SetAge(Thatchwick.Core.Characters.AgeConfig.Default.ElderStartDay + 0.5);
                        life0?.OnBecameElder(c);
                    }
            _report = new BalanceReport { Scenario = $"{ScenarioName(_scenario)}, {_days:0} gün, başlangıç günü {clock.TotalDays:0}" };
            if (life0 != null)
            {
                _report.AgingConditions = c => { int n = 0; foreach (var _ in life0.Ailments.Conditions(c)) n++; return n; };
                _report.TailorSpend = () => life0.TailorSpent;
                _report.ClothUsed = () => life0.ClothUsed;
            }
        }

        public static string ScenarioName(Scenario s) => s == Scenario.FarmDuty ? "tarla görevli" : s == Scenario.ElderHousehold ? "yaşlı hane" : "görevsiz";

        private void TakeSample()
        {
            var clock = Get<GameClock>(_root, "_clock");
            var house = Get<Household>(_root, "_household");
            var life = Get<LifeSystems>(_root, "_life");
            var world = Get<WorldState>(_root, "_world");
            var pop = Get<Population>(_root, "_population");
            var runners = new Dictionary<int, JobRunner>();
            if (Get<IEnumerable>(_root, "_actors") is IEnumerable actors)
                foreach (var a in actors)
                {
                    var t = a.GetType();
                    if (t.GetField("Character")?.GetValue(a) is Character c && t.GetField("Runner")?.GetValue(a) is JobRunner r) runners[c.Id] = r;
                }
            string Activity(Character c)
            {
                if (!runners.TryGetValue(c.Id, out var r)) return "";
                var job = r.Queue.Current;
                if (job != null) return ActivityDescriber.DescribeJob(job, world).Text;
                // Boştaysa nedeni: bunalım, uyku ya da son başarısız işin gerekçesi.
                if (c.InBreakdown) return "bunalım: " + c.Breakdown;
                return string.IsNullOrEmpty(r.LastFailure) ? "boşta" : "boşta (" + r.LastFailure + ")";
            }
            _report.Add(clock.TotalDays, clock.Season.ToString(), Climate.OutdoorTemperature(clock.Config, clock.TotalDays), house,
                life.Forecast.FoodStock(), house.Count("item.firewood"), pop.Members, Activity);
        }

        private void Finish(Exception error)
        {
            try
            {
                if (error != null)
                {
                    LastSummary = "Denge koşusu hata verdi: " + error;
                    Debug.LogError(LastSummary);
                }
                else
                {
                    string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs", "denge"));
                    Directory.CreateDirectory(dir);
                    string name = $"{(_scenario == Scenario.FarmDuty ? "tarla" : "gorevsiz")}_{_days:0}g_{DateTime.Now:yyyyMMdd_HHmm}";
                    LastCsvPath = Path.Combine(dir, name + ".csv");
                    File.WriteAllText(LastCsvPath, _report.ToCsv());
                    LastSummary = _report.Summary();
                    File.WriteAllText(Path.Combine(dir, name + ".txt"), LastSummary);
                    Debug.Log("Denge raporu: " + LastCsvPath + "\n" + LastSummary);
                }
            }
            finally
            {
                if (_root != null) Destroy(_root.gameObject);   // simüle edilen oyun kaydedilmeden kalkar
                new GameObject("GameRoot").AddComponent<GameRoot>();   // karakter yaratma ekranı
                Running = false;
                Destroy(gameObject);
            }
        }
    }
}
