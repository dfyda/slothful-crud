namespace SlothfulCrud.Types
{
    public static class BrowseFields
    {
        public static readonly IDictionary<string, Type> Fields = new Dictionary<string, Type>
        {
            { "Skip", typeof(ushort) },
            { "Rows", typeof(ushort) },
            { "SortDirection", typeof(string) },
            { "SortBy", typeof(string) }
        };
    }
}