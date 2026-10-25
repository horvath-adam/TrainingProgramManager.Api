namespace TrainingProgramManager.Api.Caching
{
    public static class CacheKeys
    {
        public const string PublicWorkshops = "workshops:public-list";

        // EN: Keys must be built only from real entity ids and server-derived user ids, never from arbitrary client-controlled strings.
        // HU: A kulcsokat csak valós entitásazonosítókból és a szerver által meghatározott felhasználóazonosítókból szabad építeni, soha nem tetszőleges, kliens által vezérelt szövegből.
        public static string WorkshopDetails(int id) => $"workshops:details:{id}";

        public static string MyWorkshops(string userId) => $"workshops:mine:{userId}";
    }
}
