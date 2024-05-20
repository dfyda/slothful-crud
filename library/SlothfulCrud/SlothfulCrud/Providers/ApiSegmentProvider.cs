using SlothfulCrud.Extensions;

namespace SlothfulCrud.Providers
{
    public class ApiSegmentProvider : IApiSegmentProvider
    {
        public string GetApiSegment(string entityName)
        {
            return entityName.ToPlural().CamelToHyphen();
        }
    }
}