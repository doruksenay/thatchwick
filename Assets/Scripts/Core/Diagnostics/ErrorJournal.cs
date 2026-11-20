using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Thatchwick.Core.Diagnostics
{
    /// <summary>
    /// Hata günlüğü (yayın altyapısı, 20.11.2026). Oyundaki her hata (simülasyon, arayüz, kayıt, Unity'nin yakaladığı
    /// istisnalar) buraya düşer. Aynı hata tekilleştirilir: ilk birkaç kez tam yazılır, sonra yalnızca sayılır; böylece
    /// her karede yinelenen bir hata dosyayı ve diski doldurmaz. Dosya büyüyünce bir önceki sürümü <c>.1</c> olarak
    /// saklanır. İş parçacığı güvenlidir (Unity günlük geri çağrısı başka iş parçacığından gelebilir).
    /// Dosyaya yazılamazsa oyun etkilenmez; sayaçlar bellekte tutulmaya devam eder.
    /// </summary>
    public sealed class ErrorJournal
    {
        /// <summary>Aynı hatanın ilk bu kadar tekrarı tam yazılır.</summary>
        public const int FullReports = 3;
        /// <summary>Sonrasında her bu kadar tekrarda bir tek satırlık sayaç yazılır.</summary>
        public const int CountEvery = 200;
        /// <summary>Bir oturumda tam yazılan farklı hata sayısı tavanı (hata fırtınasında dosya şişmesin).</summary>
        public const int MaxDistinct = 200;

        private readonly object _gate = new object();
        private readonly string _path;
        private readonly long _maxBytes;
        private readonly Func<string> _header;
        private readonly Func<DateTime> _now;
        private readonly Dictionary<string, int> _counts = new Dictionary<string, int>(StringComparer.Ordinal);
        private bool _headerWritten;
        private int _unseen;

        public ErrorJournal(string path, Func<string> header = null, long maxBytes = 512 * 1024, Func<DateTime> now = null)
        {
            _path = path;
            _header = header;
            _maxBytes = Math.Max(4096, maxBytes);
            _now = now ?? (() => DateTime.UtcNow);
        }

        public string Path => _path;
        /// <summary>Bu oturumdaki toplam hata (tekrarlar dahil).</summary>
        public int Total { get; private set; }
        /// <summary>Bu oturumdaki farklı hata sayısı.</summary>
        public int Distinct { get { lock (_gate) return _counts.Count; } }
        /// <summary>Son hatanın tek satırlık özeti (oyuncuya ve rapora).</summary>
        public string LastSummary { get; private set; } = "";

        /// <summary>
        /// Oyuncuya henüz bildirilmemiş yeni (ilk kez görülen) hata sayısını verir ve sıfırlar.
        /// Arayüz bunu ana iş parçacığında yoklar; günlük geri çağrısından doğrudan arayüze dokunulmaz.
        /// </summary>
        public int TakeUnseen()
        {
            lock (_gate) { int n = _unseen; _unseen = 0; return n; }
        }

        /// <summary>Hata kaydeder. Bu hata oturumda ilk kez görülüyorsa true.</summary>
        public bool Report(string source, string message, string stack, string context = null)
        {
            string sig = Signature(source, message, stack);
            lock (_gate)
            {
                Total++;
                _counts.TryGetValue(sig, out int n);
                bool known = n > 0;
                if (!known && _counts.Count >= MaxDistinct) return false;   // fırtına: yenilerini sayma, yazma
                _counts[sig] = ++n;
                LastSummary = Summary(source, message);
                if (!known) _unseen++;

                if (n <= FullReports)
                {
                    var sb = new StringBuilder();
                    sb.Append(_now().ToString("o")).Append(" [").Append(source ?? "?").Append("] #").Append(n);
                    if (!string.IsNullOrEmpty(context)) sb.Append(" {").Append(context).Append('}');
                    sb.Append('\n').Append(message ?? "");
                    if (!string.IsNullOrEmpty(stack)) sb.Append('\n').Append(stack.TrimEnd());
                    sb.Append("\n\n");
                    Append(sb.ToString());
                }
                else if (n % CountEvery == 0)
                    Append($"{_now():o} [{source ?? "?"}] yineleniyor: {n} kez · {Summary(source, message)}\n\n");
                return !known;
            }
        }

        public bool Report(string source, Exception ex, string context = null) =>
            ex == null ? false : Report(source, ex.GetType().Name + ": " + ex.Message, ex.StackTrace, context);

        /// <summary>
        /// Hatanın imzası: kaynak, iletinin ilk satırı (sayılar yıldızlanır: "karo 12,7" ile "karo 3,9" aynı hata sayılır)
        /// ve yığının ilk üç satırı.
        /// </summary>
        public static string Signature(string source, string message, string stack)
        {
            var sb = new StringBuilder(160);
            sb.Append(source ?? "").Append('|');
            AppendNormalized(sb, FirstLine(message), 120);
            if (!string.IsNullOrEmpty(stack))
            {
                int lines = 0, start = 0;
                while (lines < 3 && start < stack.Length)
                {
                    int end = stack.IndexOf('\n', start);
                    if (end < 0) end = stack.Length;
                    string line = stack.Substring(start, end - start).Trim();
                    if (line.Length > 0) { sb.Append('|'); AppendNormalized(sb, line, 100); lines++; }
                    start = end + 1;
                }
            }
            return sb.ToString();
        }

        /// <summary>Oyuncunun geliştiriciye gönderebileceği rapor: başlık ve günlüğün son kısmı.</summary>
        public string BuildReport(int maxChars = 12000)
        {
            var sb = new StringBuilder();
            sb.Append(SafeHeader());
            sb.Append("Bu oturum: ").Append(Total).Append(" hata, ").Append(Distinct).Append(" tekil\n\n");
            string tail = "";
            lock (_gate)
            {
                try
                {
                    if (!string.IsNullOrEmpty(_path) && File.Exists(_path))
                    {
                        string all = File.ReadAllText(_path);
                        tail = all.Length > maxChars ? "…\n" + all.Substring(all.Length - maxChars) : all;
                    }
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
            return sb.Append(tail).ToString();
        }

        private static string Summary(string source, string message)
        {
            string line = FirstLine(message);
            if (line.Length > 160) line = line.Substring(0, 160) + "…";
            return "[" + (source ?? "?") + "] " + line;
        }

        private static string FirstLine(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            int nl = text.IndexOf('\n');
            return (nl < 0 ? text : text.Substring(0, nl)).Trim();
        }

        private static void AppendNormalized(StringBuilder sb, string text, int max)
        {
            bool inDigits = false;
            int written = 0;
            foreach (char c in text)
            {
                if (written >= max) break;
                if (c >= '0' && c <= '9')
                {
                    if (!inDigits) { sb.Append('*'); written++; }
                    inDigits = true;
                }
                else { sb.Append(c); written++; inDigits = false; }
            }
        }

        private string SafeHeader()
        {
            if (_header == null) return "";
            try { return (_header() ?? "").TrimEnd() + "\n"; }
            catch (Exception) { return ""; }
        }

        // _gate altında çağrılır.
        private void Append(string text)
        {
            if (string.IsNullOrEmpty(_path)) return;
            try
            {
                var info = new FileInfo(_path);
                if (info.Exists && info.Length > _maxBytes)
                {
                    string old = _path + ".1";
                    if (File.Exists(old)) File.Delete(old);
                    File.Move(_path, old);
                    _headerWritten = false;
                }
                if (!_headerWritten)
                {
                    _headerWritten = true;
                    text = "==== " + _now().ToString("o") + " ====\n" + SafeHeader() + "\n" + text;
                }
                File.AppendAllText(_path, text);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    /// <summary>
    /// Yarım kalan oturum tespiti. Oyun açılırken bir işaret dosyası yazılır, düzgün kapanışta (ve mobilde arka plana
    /// alınırken) silinir. Açılışta dosya duruyorsa önceki oturum çökmüş ya da zorla kapatılmış demektir.
    /// </summary>
    public sealed class SessionMarker
    {
        private readonly string _path;

        public SessionMarker(string path) { _path = path; }

        /// <summary>Önceki oturum düzgün kapanmadıysa true. İşareti bu oturum için yeniden yazar.</summary>
        public bool Begin(string note = null)
        {
            bool unclean = false;
            try
            {
                unclean = File.Exists(_path);
                if (unclean) PreviousNote = File.ReadAllText(_path).Trim();
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            Mark(note);
            return unclean;
        }

        /// <summary>Önceki oturumun işaretine yazılmış not (sürüm, zaman); yoksa null.</summary>
        public string PreviousNote { get; private set; }

        /// <summary>Oturum sürüyor (açılış, ön plana dönüş).</summary>
        public void Mark(string note = null)
        {
            try { File.WriteAllText(_path, note ?? DateTime.UtcNow.ToString("o")); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        /// <summary>Düzgün kapanış ya da arka plana alınma.</summary>
        public void End()
        {
            try { if (File.Exists(_path)) File.Delete(_path); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
