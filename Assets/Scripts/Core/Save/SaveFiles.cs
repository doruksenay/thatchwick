using System;
using System.IO;

namespace Thatchwick.Core.Save
{
    /// <summary>
    /// Kayıt dosyasını güvenli yazar ve okur. Yazma önce geçici dosyaya yapılır, sonra eski kayıt ".bak" olarak
    /// saklanıp yenisi yerine taşınır: yazma sırasında uygulama kapanırsa (mobilde sık olur) ya eski ya yeni kayıt
    /// sağlam kalır. Okuma, ana dosya yoksa ya da bozuksa geçici dosyaya, sonra yedeğe düşer.
    /// </summary>
    public static class SaveFiles
    {
        public const string TempSuffix = ".tmp";
        public const string BackupSuffix = ".bak";
        public const string CorruptSuffix = ".corrupt";

        public static void WriteAtomic(string path, string text)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Kayıt yolu boş.", nameof(path));
            string tmp = path + TempSuffix, bak = path + BackupSuffix;
            File.WriteAllText(tmp, text ?? "");
            if (File.Exists(path))
            {
                if (File.Exists(bak)) File.Delete(bak);
                File.Move(path, bak);
            }
            File.Move(tmp, path);
        }

        /// <summary>Okunabilecek bir kayıt (ana, yarım kalmış taşıma ya da yedek) var mı.</summary>
        public static bool Exists(string path) =>
            File.Exists(path) || File.Exists(path + BackupSuffix) || File.Exists(path + TempSuffix);

        /// <summary>
        /// Geçerli ilk kaydı okur: ana dosya, sonra (ana dosya yoksa yarım kalmış taşımadan kalan) geçici dosya, sonra yedek.
        /// isValid metnin çözülebildiğini denetler (null ise boş olmayan her metin geçerlidir). usedFallback: ana dosya dışından okundu.
        /// </summary>
        public static bool TryRead(string path, Func<string, bool> isValid, out string text, out bool usedFallback)
        {
            usedFallback = false;
            string[] candidates = File.Exists(path)
                ? new[] { path, path + BackupSuffix }
                : new[] { path + TempSuffix, path + BackupSuffix };
            for (int i = 0; i < candidates.Length; i++)
            {
                if (!TryReadOne(candidates[i], isValid, out text)) continue;
                usedFallback = candidates[i] != path;
                return true;
            }
            text = null;
            return false;
        }

        /// <summary>
        /// Açılamayan kaydı (ana dosya, yoksa geçici dosya ya da yedek) "&lt;yol&gt;.corrupt" adıyla kopyalar: sonraki kayıtlar
        /// .bak döngüsüyle onu silmez, oyuncu ya da destek sonradan bakabilir. Önceki .corrupt kopyasının üzerine yazar.
        /// Dönen: kopyanın yolu; kopyalanacak dosya yoksa ya da kopyalanamazsa null.
        /// </summary>
        public static string Quarantine(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            string target = path + CorruptSuffix;
            foreach (var source in new[] { path, path + TempSuffix, path + BackupSuffix })
            {
                try
                {
                    if (!File.Exists(source)) continue;
                    File.Copy(source, target, overwrite: true);
                    return target;
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
            return null;
        }

        private static bool TryReadOne(string file, Func<string, bool> isValid, out string text)
        {
            text = null;
            try
            {
                if (!File.Exists(file)) return false;
                string t = File.ReadAllText(file);
                if (string.IsNullOrWhiteSpace(t)) return false;
                if (isValid != null && !isValid(t)) return false;
                text = t;
                return true;
            }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
        }
    }
}
