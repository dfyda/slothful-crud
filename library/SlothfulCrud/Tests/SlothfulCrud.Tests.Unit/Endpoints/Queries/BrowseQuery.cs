namespace SlothfulCrud.Tests.Unit.Endpoints.Queries
{
    public class BrowseQuery
    {
        public int Rows { get; set; }
        public string SortBy { get; set; }
        public string SortDirection { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAtFrom { get; set; }
        public int? Age { get; set; }
    }
}