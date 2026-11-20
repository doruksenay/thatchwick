using UnityEditor;
using UnityEngine;
using Thatchwick.Game;

namespace Thatchwick.Editor
{
    /// <summary>
    /// Denge simülasyonu menüsü (belge 41). Oynatma kipinde çalışır: yeni oyun kurup oyuncu dokunmadan N gün ilerletir,
    /// raporu proje kökündeki Logs/denge klasörüne yazar (CSV + özet), sonra karakter yaratma ekranına döner.
    /// </summary>
    public static class BalanceMenu
    {
        private static void Run(int days, BalanceRunner.Scenario scenario, double startDay = 0)
        {
            if (!Application.isPlaying) { EditorUtility.DisplayDialog("Denge", "Önce oynatma kipine geçin (Play).", "Tamam"); return; }
            if (!BalanceRunner.Begin(days, scenario, startDay)) Debug.LogWarning("Denge koşusu başlatılamadı (zaten çalışıyor olabilir).");
            else Debug.Log($"Denge koşusu başladı: {BalanceRunner.ScenarioName(scenario)}, {days} gün. Bitince Logs/denge.");
        }

        [MenuItem("Thatchwick/Denge/60 gün - görevsiz (oyuncu oynamıyor)")]
        private static void Idle60() => Run(60, BalanceRunner.Scenario.Idle);

        [MenuItem("Thatchwick/Denge/60 gün - tarla görevli")]
        private static void Farm60() => Run(60, BalanceRunner.Scenario.FarmDuty);

        [MenuItem("Thatchwick/Denge/Kış: 15 gün - tarla görevli (gün 28'den)")]
        private static void Winter15() => Run(15, BalanceRunner.Scenario.FarmDuty, 28);

        [MenuItem("Thatchwick/Denge/Yaşlı hane: 30 gün (belge 51)")]
        private static void Elder30() => Run(30, BalanceRunner.Scenario.ElderHousehold);
    }
}
