namespace Arkanis.Overlay.Infrastructure.UnitTests;

public static class TestConstants
{
    public static class Collections
    {
        public const string DbContext = "DbContext Tests";

        public const string RepositorySyncCachedApi = "Cached Sync Repository Tests";
        public const string RepositorySyncLiveApi = "Live Sync Repository Tests";
    }

    public static class Traits
    {
        public static class DataSource
        {
            public const string ExternalApi = "External API";
        }

        public static class DataState
        {
            public const string Cached = "Cached";
            public const string Live = "Live";
        }
    }
}
