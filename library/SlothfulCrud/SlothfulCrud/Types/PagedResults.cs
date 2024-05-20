namespace SlothfulCrud.Types
{
    public class PagedResults<T> where T : new()
    {
        public int First { get; }
        public int Rows { get; }
        public int Total { get; }
        public List<T> Data { get; }

        public PagedResults(int first, int total, int rows, List<T> data)
        {
            First = first;
            Total = total;
            Rows = rows;
            Data = data;
        }
    }
}