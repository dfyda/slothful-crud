using Microsoft.AspNetCore.Routing;

namespace SlothfulCrud.Extensions
{
    public static class WebApplicationExtensions
    {
        public static IEndpointRouteBuilder RegisterSlothfulEndpoints(this IEndpointRouteBuilder webApplication)
        {
            return webApplication;
        }
    }
}