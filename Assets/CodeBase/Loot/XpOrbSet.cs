namespace CodeBase.Loot
{
    public struct XpOrbSet
    {
        public const int LargeValue = 10;
        public const int MediumValue = 5;
        public const int SmallValue = 1;

        public readonly XpOrb Large;
        public readonly XpOrb Medium;
        public readonly XpOrb Small;

        public XpOrbSet(XpOrb large, XpOrb medium, XpOrb small)
        {
            Large = large;
            Medium = medium;
            Small = small;
        }
    }
}
