using SlothfulCrud.Extensions;

namespace SlothfulCrud.Providers
{
    internal class ApiSegmentProvider : IApiSegmentProvider
    {
        public string GetApiSegment(string entityName)
        {
            return entityName.ToPlural().CamelToHyphen();
        }
    }
}