using System;

namespace Thatchwick.Core.Work
{
    public enum FishingPhase { Waiting = 0, Bite = 1, Reeling = 2, Caught = 3, Escaped = 4 }

    /// <summary>Balığın kaçma biçimi: smooth (ağır ve düzenli), darting (ani sıçrar), sinker (dibe kaçar), floater (yüzeye kaçar), mixed.</summary>
    public enum FishBehavior { Smooth = 0, Darting = 1, Sinker = 2, Floater = 3, Mixed = 4 }

    /// <summary>
    /// Balık tutma mini oyunu (Project belgesi 50; kullanıcı 20.11.2026: "balıkçılık oto yapılmıyorsa mini oyun").
    /// Oyuncu kendisi oltayla ya da kavanozla tutarken açılır; aile görevlisi, hizmetkâr ve balık sepeti eskisi gibi
    /// kendiliğinden tutar.
    ///
    /// Akış: <b>Bekle</b> (şamandıra) → <b>Vurdu!</b> (kısa pencerede bas, yoksa kaçar) → <b>Çek</b>: dikey sütunda balık
    /// türüne göre kaçar; oyuncu basılı tutarak yeşil çubuğu yukarı iter, bırakınca çubuk düşer. Balık çubuğun içindeyken
    /// tutma çizgisi dolar, dışındayken boşalır; dolunca tutulur, boşalınca kaçar. Hiç kaçırmadan tutmak "kusursuz"dur.
    ///
    /// Çubuk boyu Balıkçılık seviyesiyle (ve olta kademesiyle) büyür; balığın hızı ve kaçışı zorluğuyla artar. Kum saati
    /// gerçek saniyedir (oyun saatinden bağımsız). Rastgelelik tohumludur (test edilir). Koordinatlar 0 (dip) - 1 (yüzey).
    /// </summary>
    public sealed class FishingGame
    {
        public const float BiteWindow = 1.0f, MinWait = 1.2f, MaxWait = 3.5f;
        public const float BaseBar = 0.2f, BarPerLevel = 0.012f, MaxBar = 0.42f;
        public const float Lift = 2.2f, Gravity = 1.8f, MaxSpeed = 0.9f;
        public const float FillRate = 0.30f, DrainBase = 0.16f, DrainPerDifficulty = 0.25f, StartProgress = 0.3f;
        /// <summary>Balık çubuğun içinde bu kadar (zorluğa göre kısalır) rahat kalırsa ürküp uzağa kaçar.</summary>
        public const float BoltSeconds = 0.8f;

        private readonly Random _rng;
        private float _target, _retarget, _fishVel, _inside;

        public string FishId { get; }
        public int Difficulty { get; }
        public FishBehavior Behavior { get; }
        public FishingPhase Phase { get; private set; } = FishingPhase.Waiting;
        public float FishPos { get; private set; } = 0.25f;
        public float BarPos { get; private set; }
        public float BarSize { get; }
        public float BarVelocity { get; private set; }
        public float Progress { get; private set; } = StartProgress;
        public float WaitLeft { get; private set; }
        public float BiteLeft { get; private set; }
        public bool Perfect { get; private set; } = true;
        /// <summary>Kaçış nedeni: true ise vurunca geç basıldı.</summary>
        public bool MissedBite { get; private set; }
        public bool Done => Phase == FishingPhase.Caught || Phase == FishingPhase.Escaped;
        public bool Overlapping => FishPos >= BarPos && FishPos <= BarPos + BarSize;

        /// <param name="skillLevel">Balıkçılık seviyesi (1-10).</param>
        /// <param name="barBonus">Olta kademesi ya da kavanoz kolaylığı (0-0.1).</param>
        /// <param name="skipWait">Kavanozla yakalarken bekleme yok: doğrudan çekmeye geçer.</param>
        public FishingGame(string fishId, int difficulty, FishBehavior behavior, int skillLevel, float barBonus = 0f, int seed = 1, bool skipWait = false)
        {
            FishId = fishId;
            Difficulty = Math.Max(0, Math.Min(100, difficulty));
            Behavior = behavior;
            _rng = new Random(seed);
            BarSize = Math.Min(MaxBar, BaseBar + BarPerLevel * Math.Max(0, skillLevel - 1) + Math.Max(0f, barBonus));
            BarPos = 0f;
            WaitLeft = MinWait + (float)_rng.NextDouble() * (MaxWait - MinWait);
            _target = 0.25f;
            if (skipWait) Phase = FishingPhase.Reeling;
        }

        public static FishBehavior ParseBehavior(string s)
        {
            switch (s)
            {
                case "darting": return FishBehavior.Darting;
                case "sinker": return FishBehavior.Sinker;
                case "floater": return FishBehavior.Floater;
                case "mixed": return FishBehavior.Mixed;
                default: return FishBehavior.Smooth;
            }
        }

        /// <param name="dt">Gerçek saniye.</param>
        /// <param name="hold">Basılı tutuluyor (çubuğu yukarı iter).</param>
        /// <param name="pressed">Bu karede basıldı (vurunca çekmek için).</param>
        public void Step(float dt, bool hold, bool pressed)
        {
            if (Done || dt <= 0f) return;
            switch (Phase)
            {
                case FishingPhase.Waiting:
                    WaitLeft -= dt;
                    if (WaitLeft <= 0f) { Phase = FishingPhase.Bite; BiteLeft = BiteWindow; }
                    return;
                case FishingPhase.Bite:
                    if (pressed) { Phase = FishingPhase.Reeling; return; }
                    BiteLeft -= dt;
                    if (BiteLeft <= 0f) { Phase = FishingPhase.Escaped; MissedBite = true; Perfect = false; }
                    return;
            }

            // Çubuk: basılıyken yukarı ivme, bırakınca yerçekimi; dipte ve tepede durur.
            BarVelocity += (hold ? Lift : -Gravity) * dt;
            BarVelocity = Math.Max(-MaxSpeed, Math.Min(MaxSpeed, BarVelocity));
            BarPos += BarVelocity * dt;
            if (BarPos < 0f) { BarPos = 0f; BarVelocity = 0f; }
            if (BarPos > 1f - BarSize) { BarPos = 1f - BarSize; BarVelocity = Math.Min(0f, BarVelocity); }

            MoveFish(dt);

            if (Overlapping) Progress += FillRate * dt;
            else { Progress -= (DrainBase + DrainPerDifficulty * Difficulty / 100f) * dt; Perfect = false; }
            if (Progress >= 1f) { Progress = 1f; Phase = FishingPhase.Caught; }
            else if (Progress <= 0f) { Progress = 0f; Phase = FishingPhase.Escaped; }
        }

        private float R => (float)_rng.NextDouble();

        private void MoveFish(float dt)
        {
            float d = Difficulty / 100f;
            _retarget -= dt;
            _inside = Overlapping ? _inside + dt : 0f;
            bool bolt = _inside > BoltSeconds * (1.5f - d);
            if (_retarget <= 0f || bolt)
            {
                var b = Behavior == FishBehavior.Mixed ? (FishBehavior)_rng.Next(4) : Behavior;
                float reach = 0.15f + 0.6f * d;   // zor balık uzağa kaçar
                if (bolt) { reach += 0.25f; _inside = 0f; }
                switch (b)
                {
                    case FishBehavior.Darting: _target = FishPos + (R - 0.5f) * 2f * (reach + 0.15f); _retarget = 0.3f + R * (1.2f - d); break;
                    case FishBehavior.Sinker: _target = FishPos + (R - 0.65f) * 2f * reach; _retarget = 0.6f + R * (1.4f - d); break;
                    case FishBehavior.Floater: _target = FishPos + (R - 0.35f) * 2f * reach; _retarget = 0.6f + R * (1.4f - d); break;
                    default: _target = FishPos + (R - 0.5f) * 2f * reach; _retarget = 0.8f + R * (1.6f - d); break;
                }
                // Dipte duran (bırakılmış) çubuğun içinde dinlenmez: hiç basmadan balık tutulmaz.
                _target = Math.Max(BarSize + 0.02f, Math.Min(0.98f, _target));
            }
            // Hedefe yay gibi yaklaşır; hız zorlukla artar. Sıçrayan balık daha sert ivmelenir.
            float stiffness = (Behavior == FishBehavior.Darting ? 14f : 6f) * (0.5f + d);
            float damping = 4.5f;
            _fishVel += ((_target - FishPos) * stiffness - _fishVel * damping) * dt;
            float maxV = 0.3f + 1.4f * d;
            _fishVel = Math.Max(-maxV, Math.Min(maxV, _fishVel));
            FishPos = Math.Max(0f, Math.Min(1f, FishPos + _fishVel * dt));
        }
    }
}
