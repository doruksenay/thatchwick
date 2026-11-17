namespace Thatchwick.Core.Jobs
{
    /// <summary>
    /// Bir eşyanın ya da aletin işe kattığı çarpanlar (ör. yadigâr balta, atadan kalma yatak). 1 = etkisiz.
    /// Hız iş süresini böler; XP beceri deneyimini, Restore ihtiyaç doldurmayı çarpar.
    /// </summary>
    public readonly struct ItemBonus
    {
        public readonly double Speed;
        public readonly double Xp;
        public readonly double Restore;

        public ItemBonus(double speed, double xp, double restore)
        {
            Speed = speed;
            Xp = xp;
            Restore = restore;
        }

        public static ItemBonus None => new ItemBonus(1.0, 1.0, 1.0);
        public bool IsNone => Speed == 1.0 && Xp == 1.0 && Restore == 1.0;
    }
}
