namespace NPCLaunderers.LaundererSaveManager
{
    public enum LaundererTier
    {
        Low,
        Medium,
        High,
        Highest
    }
    public static class Launderers
    {
        public static Dictionary<string, LaundererTier> TierOf = new Dictionary<string, LaundererTier> {
            { "Donna", LaundererTier.Low },
            { "Dean", LaundererTier.Low },
            { "Jeff", LaundererTier.Medium },
            { "Marco", LaundererTier.Medium },
            { "Jeremy", LaundererTier.High },
            { "Fiona", LaundererTier.High },
            { "Ray", LaundererTier.Highest },
            { "Herbert", LaundererTier.Highest }
        };

        public static Dictionary<LaundererTier, float[]> LaunderAmountRanges = new Dictionary<LaundererTier, float[]> {
            { LaundererTier.Low, new float[] { 1000f, 3500f } },
            { LaundererTier.Medium, new float[] { 5500f, 7500f } },
            { LaundererTier.High, new float[] { 9500f, 11000f } },
            { LaundererTier.Highest, new float[] { 11500f, 13500f } }
        };
    }
}
