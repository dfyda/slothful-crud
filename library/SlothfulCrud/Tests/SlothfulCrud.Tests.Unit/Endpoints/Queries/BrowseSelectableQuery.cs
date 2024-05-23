namespace SlothfulCrud.Tests.Unit.Endpoints.Queries
{
    public class BrowseSelectableQuery
    {
        public int Rows { get; set; }
        public string SortBy { get; set; }
        public string SortDirection { get; set; }
        public string Search { get; set; }
        public DateTime? CreatedAtFrom { get; set; }
    }
}