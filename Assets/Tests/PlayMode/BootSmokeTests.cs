using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Thatchwick.Game;

namespace Thatchwick.PlayTests
{
    /// <summary>
    /// Oyun katmanının duman testi: çekirdek testleri (core/) Unity'siz koşar ve Game katmanını yalnızca derler;
    /// bu test sahneyi gerçekten açar. Açılışta ya da ilk karelerde hata günlüğü (Debug.LogError, istisna) düşerse
    /// Unity Test Framework testi kendiliğinden kırar: bozuk içerik, eksik Resources dosyası, çöken OnGUI burada yakalanır.
    /// Kayıt dosyasına yazmaz (karakter yaratılmadan otomatik kayıt çalışmaz).
    /// </summary>
    public class BootSmokeTests
    {
        /// <summary>
        /// GameRoot oynatma başlarken kendiliğinden kurulur (GameBootstrap, AfterSceneLoad). Test sahnesinde kamera
        /// yoksa bir tane eklenir; GameRoot bir nedenle yoksa burada kurulur.
        /// </summary>
        private static IEnumerator EnsureRoot()
        {
            if (Camera.main == null)
            {
                var cam = new GameObject("Main Camera") { tag = "MainCamera" }.AddComponent<Camera>();
                cam.orthographic = true;
            }
            yield return null;
            if (Object.FindFirstObjectByType<GameRoot>() == null) new GameObject("GameRoot").AddComponent<GameRoot>();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Sahne_Acilir_GameRootKurulur_HataDusmez()
        {
            yield return EnsureRoot();
            for (int i = 0; i < 30; i++) yield return null;

            var root = Object.FindFirstObjectByType<GameRoot>();
            Assert.IsNotNull(root, "Sahnede GameRoot yok: oyun açılmadı.");
            Assert.IsTrue(root.enabled, "GameRoot açılışta kendini kapattı.");
            Assert.That(GameRoot.SaveSlot, Is.InRange(1, 3));
        }

        [UnityTest]
        public IEnumerator YeniGameRoot_EskisininYerineGecer_HataDusmez()
        {
            yield return EnsureRoot();
            for (int i = 0; i < 10; i++) yield return null;
            var old = Object.FindFirstObjectByType<GameRoot>();
            Assert.IsNotNull(old);

            // "Ana menü" düğmesinin yaptığı: eskisi kapanır, yenisi kurulur.
            old.enabled = false;
            new GameObject("GameRoot").AddComponent<GameRoot>();
            Object.Destroy(old.gameObject);
            for (int i = 0; i < 20; i++) yield return null;

            Assert.AreEqual(1, Object.FindObjectsByType<GameRoot>(FindObjectsSortMode.None).Length);
        }
    }
}
