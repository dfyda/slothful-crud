using SlothfulCrud.Providers;

namespace SlothfulCrud.Api.Slothful.Providers
{
    public class CustomApiSegmentProvider : IApiSegmentProvider
    {
        public string GetApiSegment(string entityName)
        {
            return $"my-custom-{entityName}";
        }
    }
}