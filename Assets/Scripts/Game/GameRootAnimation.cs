using UnityEngine;
using Thatchwick.Core.Characters;
using Thatchwick.Core.Elders;
using Thatchwick.Core.Jobs;
using Thatchwick.Core.Life;
using Thatchwick.Core.Mood;
using Thatchwick.Core.Relations;

namespace Thatchwick.Game
{
    /// <summary>
    /// Animasyon çeşitliliği, kod katmanı (Project belgesi 69 §2-§4, 0 gen): her fiile hâle, beceriye, yorgunluğa, yaşa ve
    /// havaya göre tempo ve ritim; hâl durmaları ve yürüyüş biçimleri; ara klipler; ter, nota ve hırçın püfü. Kurallar
    /// Core'da (<see cref="AnimStyleRules"/>, <see cref="InterjectionPicker"/>); burada yalnızca görünüm. Kare gelmeden de çalışır:
    /// karesi olmayan biçim normal kareyi kodla değiştirilmiş hızda ve salınımda oynar. Hiçbiri kayda girmez.
    /// </summary>
    public sealed partial class GameRoot
    {
        /// <summary>Kişi başına animasyon durumu (görsel; kayda girmez).</summary>
        private sealed class AnimState
        {
            public AnimStyle Style = new AnimStyle { Tempo = 1f, Bob = 1f, Cadence = 1f, Irregularity = 0.15f };
            public AnimInputs Inputs;
            public float StyleAt = -1f;
            public float Phase = -1f;
            public float HoldUntil;
            public int Cycles;
            public Interjection Ix;
            public float IxStart, IxUntil, IxNextCheck;
            public bool WasWorking;
        }

        private readonly InterjectionPicker _interjections = new InterjectionPicker();
        private const float StyleRefresh = 0.5f, BaseCycleSeconds = 0.9f;

        /// <summary>Biçimi yarım saniyede bir yeniler (moodlet ve ihtiyaçlar o kadar hızlı değişmez).</summary>
        private AnimStyle StyleOf(Actor actor, bool moving, bool working)
        {
            var a = actor.Anim;
            if (a.StyleAt >= 0f && Time.time - a.StyleAt < StyleRefresh) return a.Style;
            a.StyleAt = Time.time;
            var c = actor.Character;
            var i = AnimInputs.From(c);
            i.Outdoors = actor.View != null && !IsShelteredAt(actor.View.position);
            i.Running = moving && actor.Runner.IsRunning;
            i.Work = working ? WorkContextOf(actor.WorkIcon) : WorkContext.None;
            i.SkillLevel = working ? CurrentSkillLevel(actor) : 0;
            if (_life != null && c.Stage == AgeStage.Elder)
            {
                i.KneeTrouble = _life.Ailments.Has(c, AgingCondition.Knees);
                i.HasCane = _life.Ailments.Cane(c);
            }
            i.Tipsy = Thatchwick.Core.Content.ContentPacks.Adult && c.Mood != null && c.Mood.Has(LifeSystems.DrinkMood);
            a.Inputs = i;
            a.Style = AnimStyleRules.Compute(i);
            return a.Style;
        }

        private static WorkContext WorkContextOf(string icon)
        {
            switch (icon)
            {
                case "chop": case "mine": case "mine_support": case "mine_bucket": case "farm": case "scythe": return WorkContext.Heavy;
                case "sow": case "reap": case "bend": case "pick": case "water": case "pickup": return WorkContext.Bend;
                case "craft": case "work": return WorkContext.Craft;
                case "cook": return WorkContext.Cook;
                case "social": case "love": case "care": return WorkContext.Social;
                case "fun": case "eat": case "fish": return WorkContext.Calm;
                default: return WorkContext.None;
            }
        }

        /// <summary>Şu anki eşya işinin beceri seviyesi (beceri işi değilse 0).</summary>
        private int CurrentSkillLevel(Actor actor)
        {
            var job = actor.Runner.Queue.Current;
            if (job == null || job.Kind != JobKind.UseItem || _world == null) return 0;
            var def = _world.GetItem(job.ItemId)?.Def;
            if (def == null) return 0;
            foreach (var it in def.Interactions)
                if (it.Id == job.InteractionId) return it.Skill.HasValue ? actor.Character.Skills.GetLevel(it.Skill.Value) : 0;
            return 0;
        }

        /// <summary>
        /// İş ve durma döngüsünün evresi (K1-K3): kişinin kendi temposuyla ilerler; döngü başında düzensizlik olasılığıyla kısa
        /// bekler, yorgun her birkaç döngüde mola verir. Başlangıç evresi kişiye göre (yan yana iki kişi aynı anda vurmaz).
        /// </summary>
        private void AdvanceCycle(Actor actor, AnimStyle style, int seed)
        {
            var a = actor.Anim;
            if (a.Phase < 0f) a.Phase = Mathf.Repeat(seed * 0.17f, 1f);
            float now = Time.time;
            if (now < a.HoldUntil) return;
            a.Phase += Time.deltaTime / BaseCycleSeconds * style.Tempo;
            if (a.Phase < 1f) return;
            a.Phase -= Mathf.Floor(a.Phase);
            a.Cycles++;
            if (style.RestEvery > 0 && a.Cycles % style.RestEvery == 0) a.HoldUntil = now + 0.6f;
            else if (Random01() < style.Irregularity) a.HoldUntil = now + 0.1f + Random01() * 0.15f;
        }

        /// <summary>Hâl durması (69 §3.1): karesi varsa; yaşlıda diz sıkıntısı varsa baston durması.</summary>
        private Sprite HalIdleFrame(Actor actor)
        {
            var c = actor.Character;
            var i = actor.Anim.Inputs;
            string key = c.Stage == AgeStage.Elder && (i.KneeTrouble || i.HasCane) ? "idle_cane" : AnimStyleRules.IdleKey(i.Hal, c.Stage);
            return key != null ? IsoArt.CharacterAction(c, key, actor.Direction, actor.Anim.Phase) : null;
        }

        /// <summary>
        /// Ara klip (69 §4): birkaç saniyede bir uygun klip aranır; iş bitince hemen bakılır. Klip oynarken karesi varsa o, yoksa
        /// kodla küçük hareket ve parçacık. Dönen kare null değilse diğer karelerin önüne geçer.
        /// </summary>
        private Sprite UpdateInterjection(Actor actor, bool moving, bool working, Vector3 pos, int sortOrder, ref float bob, ref float squash, ref float sway)
        {
            var a = actor.Anim;
            var c = actor.Character;
            float now = Time.time;
            bool visible = actor.Renderer != null && actor.Renderer.enabled && c.Stage != AgeStage.Baby && !c.Asleep;
            bool workDone = a.WasWorking && !working && !moving;
            a.WasWorking = working;
            if (a.Ix == Interjection.None && visible && (workDone || now >= a.IxNextCheck))
            {
                a.IxNextCheck = now + 6f + Random01() * 5f;
                var moment = workDone ? InterjectionMoment.WorkDone : moving ? InterjectionMoment.Walking : working ? InterjectionMoment.Working : InterjectionMoment.Idle;
                // Her fırsatta değil: kişi başına yaklaşık her 2-3 bakışta bir (iş bitişi her zaman).
                if (workDone || Random01() < 0.4f)
                {
                    bool morning = _clock != null && _clock.DayFraction < 0.33;
                    var ix = _interjections.Pick(c.Id, a.Inputs, moment, moment == InterjectionMoment.Walking && FriendNear(actor), morning, now);
                    if (ix != Interjection.None) { a.Ix = ix; a.IxStart = now; a.IxUntil = now + (float)InterjectionPicker.ClipSeconds; IxParticles(actor, ix, pos, sortOrder); }
                }
            }
            if (a.Ix == Interjection.None) return null;
            if (now >= a.IxUntil || !visible) { a.Ix = Interjection.None; return null; }
            float k = (now - a.IxStart) / (float)InterjectionPicker.ClipSeconds;
            // Yürürken klip karesi yürümeyi bozmasın: yalnızca el sallama (kare varsa) ve kodla hareket.
            var f = moving && a.Ix != Interjection.Wave ? null : IsoArt.CharacterAction(c, InterjectionPicker.Key(a.Ix), actor.Direction, k);
            if (f != null) return f;
            // Kare yok: kodla küçük hareket (K12).
            switch (a.Ix)
            {
                case Interjection.Wave:
                case Interjection.SmallCheer: if (k < 0.3f) bob += Juice.Px; break;                    // kısa sıçrama
                case Interjection.Sigh: bob -= Juice.Px; squash = 0.03f; break;                         // omuz düşer
                case Interjection.Yawn: case Interjection.BackStretch: if (k > 0.2f && k < 0.7f) squash = -0.04f; break;   // gerinme
                case Interjection.HoldBelly: case Interjection.Cough: if (((int)(k * 6f) & 1) == 0) squash = 0.03f; break;
                case Interjection.ShakeOff: sway += ((int)(k * 14f) & 1) == 0 ? Juice.Px : -Juice.Px; break;
            }
            return null;
        }

        private void IxParticles(Actor actor, Interjection ix, Vector3 pos, int sortOrder)
        {
            if (_juice == null || FxScale <= 0f) return;
            var head = pos + new Vector3(0f, 0.95f, 0f);
            switch (ix)
            {
                case Interjection.WipeBrow: _juice.Emit(Juice.Kind.Sweat, head, 2, sortOrder + 1); break;
                case Interjection.ShakeOff:
                    _juice.Emit(actor.Character.Wetness > 0.3f ? Juice.Kind.Drip : Juice.Kind.Dust, pos + new Vector3(0f, 0.4f, 0f), 3, sortOrder + 1); break;
                case Interjection.Cough: _juice.Emit(Juice.Kind.Dust, head + new Vector3(0.1f, -0.1f, 0f), 1, sortOrder + 1); break;
                case Interjection.SmallCheer: _juice.Emit(Juice.Kind.Pop, head, 3, sortOrder + 1); break;
                case Interjection.BlowHands: _juice.Emit(Juice.Kind.Breath, head + new Vector3(0.1f, -0.25f, 0f), 2, sortOrder + 1); break;
            }
        }

        /// <summary>Görüşte (5 karo) Arkadaş ya da daha yakın biri var mı (el sallama).</summary>
        private bool FriendNear(Actor actor)
        {
            var rel = _life?.Relations;
            if (rel == null) return false;
            var p = actor.Character.Position;
            foreach (var o in _actors)
            {
                if (o == actor || o.Character.IsDead || o.Character.Position.Z != p.Z) continue;
                var q = o.Character.Position;
                if (Mathf.Abs(q.X - p.X) + Mathf.Abs(q.Y - p.Y) > 5) continue;
                if (rel.Stage(actor.Character.Id, o.Character.Id) >= FriendStage.Friend) return true;
            }
            return false;
        }

        /// <summary>Biçimden gelen sürekli parçacıklar (K6): terleyen, ıslık çalan, hırçın.</summary>
        private void StyleParticles(Actor actor, AnimStyle style, bool working, Vector3 pos, int sortOrder)
        {
            float scale = FxScale;
            if (_juice == null || scale <= 0f || actor.Renderer == null || !actor.Renderer.enabled) return;
            float dt = Time.deltaTime;
            var head = pos + new Vector3(0f, 0.95f, 0f);
            var h = actor.Anim.Inputs.Hal;
            if (style.Sweat && Random01() < dt * 0.5f * scale) _juice.Emit(Juice.Kind.Sweat, head, 1, sortOrder + 1);
            if (working && (h == Hal.Cheerful || h == Hal.Eager) && actor.Anim.Inputs.Work != WorkContext.Social
                && Random01() < dt * 0.22f * scale) _juice.Emit(Juice.Kind.Note, head, 1, sortOrder + 1);   // ıslık (60 §2.2)
            if (h == Hal.Cross && Random01() < dt * 0.15f * scale) _juice.Emit(Juice.Kind.Huff, head + new Vector3(0.12f, -0.05f, 0f), 1, sortOrder + 1);
        }
    }
}
