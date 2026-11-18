using System.Collections;
using System.Reflection;

namespace Xunit
{
    [AttributeUsage(AttributeTargets.Method)] public class FactAttribute : Attribute { public string Skip { get; set; } public string DisplayName { get; set; } }
    [AttributeUsage(AttributeTargets.Method)] public class TheoryAttribute : FactAttribute { }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)] public class InlineDataAttribute : Attribute { public object[] Data; public InlineDataAttribute(params object[] data) { Data = data; } }

    public class XunitException : Exception { public XunitException(string m) : base(m) { } }

    public static class Assert
    {
        static XunitException F(string m) => new XunitException(m);
        public static void True(bool c, string msg = null) { if (!c) throw F("True failed " + msg); }
        public static void True(bool? c, string msg = null) { if (c != true) throw F("True failed " + msg); }
        public static void False(bool c, string msg = null) { if (c) throw F("False failed " + msg); }
        public static void False(bool? c, string msg = null) { if (c != false) throw F("False failed " + msg); }
        public static void Null(object o) { if (o != null) throw F($"Null failed: {o}"); }
        public static void NotNull(object o) { if (o == null) throw F("NotNull failed"); }
        public static void Same(object a, object b) { if (!ReferenceEquals(a, b)) throw F("Same failed"); }
        public static void NotSame(object a, object b) { if (ReferenceEquals(a, b)) throw F("NotSame failed"); }
        public static void Equal<T>(T e, T a) { if (!Eq(e, a)) throw F($"Equal failed. Expected: {Show(e)} Actual: {Show(a)}"); }
        public static void Equal(double e, double a, int precision) { if (Math.Round(e - a, precision) != 0 && Math.Round(e, precision) != Math.Round(a, precision)) throw F($"Equal failed {e} vs {a}"); }
        public static void Equal(float e, float a, int precision) => Equal((double)e, (double)a, precision);
        public static void Equal(double e, double a, double tolerance) { if (Math.Abs(e - a) > tolerance) throw F($"Equal failed {e} vs {a}"); }
        public static void Equal<T>(IEnumerable<T> e, IEnumerable<T> a) { if (!e.SequenceEqual(a)) throw F($"Equal(seq) failed. Expected: {Show(e)} Actual: {Show(a)}"); }
        public static void NotEqual<T>(T e, T a) { if (Eq(e, a)) throw F($"NotEqual failed: {Show(a)}"); }
        public static void InRange<T>(T v, T lo, T hi) where T : IComparable<T> { if (v.CompareTo(lo) < 0 || v.CompareTo(hi) > 0) throw F($"InRange failed {v} not in [{lo},{hi}]"); }
        public static void Contains(string sub, string s) { if (s == null || !s.Contains(sub)) throw F($"Contains failed: '{sub}' in '{s}'"); }
        public static void DoesNotContain(string sub, string s) { if (s != null && s.Contains(sub)) throw F($"DoesNotContain failed: '{sub}' in '{s}'"); }
        public static void StartsWith(string p, string s) { if (s == null || !s.StartsWith(p)) throw F($"StartsWith failed: '{p}' '{s}'"); }
        public static void Contains<T>(T item, IEnumerable<T> c) { foreach (var x in c) if (Eq(x, item)) return; throw F($"Contains failed: {Show(item)}"); }
        public static void DoesNotContain<T>(T item, IEnumerable<T> c) { foreach (var x in c) if (Eq(x, item)) throw F($"DoesNotContain failed: {Show(item)}"); }
        public static T Contains<T>(IEnumerable<T> c, Predicate<T> p) { foreach (var x in c) if (p(x)) return x; throw F("Contains(predicate) failed"); }
        public static void DoesNotContain<T>(IEnumerable<T> c, Predicate<T> p) { foreach (var x in c) if (p(x)) throw F("DoesNotContain(predicate) failed"); }
        public static void Contains<K, V>(K key, IReadOnlyDictionary<K, V> d) { if (!d.ContainsKey(key)) throw F($"Contains key failed {key}"); }
        public static void Contains<K, V>(K key, IDictionary<K, V> d) { if (!d.ContainsKey(key)) throw F($"Contains key failed {key}"); }
        public static void Contains<K, V>(K key, Dictionary<K, V> d) { if (!d.ContainsKey(key)) throw F($"Contains key failed {key}"); }
        public static void DoesNotContain<K, V>(K key, IReadOnlyDictionary<K, V> d) { if (d.ContainsKey(key)) throw F($"DoesNotContain key failed {key}"); }
        public static void DoesNotContain<K, V>(K key, Dictionary<K, V> d) { if (d.ContainsKey(key)) throw F($"DoesNotContain key failed {key}"); }
        public static void Empty(IEnumerable c) { foreach (var _ in c) throw F("Empty failed"); }
        public static void NotEmpty(IEnumerable c) { foreach (var _ in c) return; throw F("NotEmpty failed"); }
        public static T Single<T>(IEnumerable<T> c) { var l = c.ToList(); if (l.Count != 1) throw F($"Single failed: {l.Count}"); return l[0]; }
        public static T Single<T>(IEnumerable<T> c, Predicate<T> p) { var l = c.Where(x => p(x)).ToList(); if (l.Count != 1) throw F($"Single(pred) failed: {l.Count}"); return l[0]; }
        public static object Single(IEnumerable c) { var l = c.Cast<object>().ToList(); if (l.Count != 1) throw F($"Single failed: {l.Count}"); return l[0]; }
        public static void All<T>(IEnumerable<T> c, Action<T> a) { foreach (var x in c) a(x); }
        public static T Throws<T>(Action a) where T : Exception { try { a(); } catch (T e) { return e; } catch (Exception e) { throw F($"Throws: wrong {e.GetType().Name}"); } throw F("Throws: none"); }
        public static T Throws<T>(Func<object> a) where T : Exception => Throws<T>(() => { a(); });
        static bool Eq<T>(T e, T a)
        {
            if (e is IEnumerable ee && a is IEnumerable ae && !(e is string))
                return ee.Cast<object>().SequenceEqual(ae.Cast<object>());
            return EqualityComparer<T>.Default.Equals(e, a);
        }
        static string Show(object o) => o == null ? "null" : o is IEnumerable e && !(o is string) ? "[" + string.Join(",", e.Cast<object>()) + "]" : o.ToString();
    }

    public static class Runner
    {
        public static int Main(string[] args)
        {
            string filter = args.Length > 0 ? args[0] : null;
            int pass = 0, fail = 0, skip = 0;
            foreach (var t in typeof(Runner).Assembly.GetTypes().OrderBy(t => t.FullName))
            {
                if (filter != null && !t.Name.Contains(filter)) continue;
                foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    var fact = m.GetCustomAttribute<FactAttribute>();
                    if (fact == null) continue;
                    if (fact.Skip != null) { skip++; continue; }
                    var rows = m.GetCustomAttributes<InlineDataAttribute>().Select(r => r.Data).ToList();
                    if (rows.Count == 0) rows.Add(Array.Empty<object>());
                    foreach (var row in rows)
                    {
                        try
                        {
                            object inst = t.IsAbstract && t.IsSealed ? null : Activator.CreateInstance(t);
                            var ps = m.GetParameters();
                            var conv = row.Select((v, i) => v == null ? null : ps[i].ParameterType.IsEnum ? Enum.ToObject(ps[i].ParameterType, v) : Convert.ChangeType(v, Nullable.GetUnderlyingType(ps[i].ParameterType) ?? ps[i].ParameterType)).ToArray();
                            var r = m.Invoke(inst, conv);
                            if (r is Task task) task.GetAwaiter().GetResult();
                            (inst as IDisposable)?.Dispose();
                            pass++;
                        }
                        catch (Exception e)
                        {
                            var inner = e is TargetInvocationException ti ? ti.InnerException : e;
                            fail++;
                            Console.WriteLine($"FAIL {t.Name}.{m.Name}({string.Join(",", row)}): {inner.GetType().Name}: {inner.Message}");
                        }
                    }
                }
            }
            Console.WriteLine($"Geçti {pass}, kaldı {fail}, atlandı {skip}");
            return fail == 0 ? 0 : 1;
        }
    }
}
