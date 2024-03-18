using SlothfulCrud.Extensions;

namespace SlothfulCrud.Providers
{
    public static class ApiSegmentProvider
    {
        public static string GetApiSegment(string entityName)
        {
            return entityName.ToPlural().CamelToHyphen();
        }
    }
}