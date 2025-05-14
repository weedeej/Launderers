namespace Clean_Cash.SaveManager
{
    public enum LaundererTiers
    {
        Low,
        Medium,
        High,
        Highest
    }
    public static class Launderers
    {
        public static Dictionary<string, LaundererTiers> TierOf = new Dictionary<string, LaundererTiers> {
            { "Donna", LaundererTiers.Low },
            { "Dean", LaundererTiers.Low },
            { "Jeff", LaundererTiers.Medium },
            { "Marco", LaundererTiers.Medium },
            { "Jeremy", LaundererTiers.High },
            { "Fiona", LaundererTiers.High },
            { "Ray", LaundererTiers.Highest },
            { "Herbert", LaundererTiers.Highest }
        };

        public static Dictionary<LaundererTiers, float[]> LaunderAmountRanges = new Dictionary<LaundererTiers, float[]> {
            { LaundererTiers.Low, new float[] { 1000f, 3500f } },
            { LaundererTiers.Medium, new float[] { 5500f, 7500f } },
            { LaundererTiers.High, new float[] { 9500f, 11000f } },
            { LaundererTiers.Highest, new float[] { 11500f, 13500f } }
        };
    }
}
