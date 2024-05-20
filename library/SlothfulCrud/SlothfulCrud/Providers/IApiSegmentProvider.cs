namespace SlothfulCrud.Providers
{
    public interface IApiSegmentProvider
    {
        string GetApiSegment(string entityName);
    }
}