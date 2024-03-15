using Microsoft.AspNetCore.Http;

namespace SlothfulCrud.Providers
{
    public static class QueryObjectProvider
    {
        public static T PrepareQueryObject<T>(T query, HttpContext context) where T : new()
        {
            return new T();
        }
    }
}
