namespace SlothfulCrud.Types
{
    internal static class BrowseFields
    {
        public static readonly IDictionary<string, Type> Fields = new Dictionary<string, Type>
        {
            { "Rows", typeof(ushort) },
            { "SortDirection", typeof(string) },
            { "SortBy", typeof(string) }
        };
    }
}