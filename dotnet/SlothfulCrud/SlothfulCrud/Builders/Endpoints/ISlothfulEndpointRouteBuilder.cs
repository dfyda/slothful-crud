using Microsoft.AspNetCore.Builder;

namespace SlothfulCrud.Builders.Endpoints
{
    public interface ISlothfulEndpointRouteBuilder
    {
        void MapEndpoints(WebApplication webApplication, Type dbContextType, Type entityType);
        void MapGetEndpoint(WebApplication webApplication, Type dbContextType, Type entityType);
        void MapBrowseEndpoint(WebApplication app, Type dbContextType, Type entityType);
        void MapCreateEndpoint(WebApplication webApplication, Type dbContextType, Type entityType);
        void MapUpdateEndpoint(WebApplication webApplication, Type dbContextType, Type entityType);
        void MapDeleteEndpoint(WebApplication app, Type dbContextType, Type entityType);
    }
}