using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Builders.Endpoints.Behaviors.ModifyMethod;
using SlothfulCrud.Providers;
using SlothfulCrud.Types;

namespace SlothfulCrud.Builders.Endpoints.Methods
{
    public class SlothfulUpdateEndpointBuilder
    {
        private readonly WebApplication _webApplication;
        private readonly Type _dbContextType;
        private readonly IApiSegmentProvider _apiSegmentProvider;
        private readonly IModifyMethodBehavior _modifyMethodBehavior;

        public SlothfulUpdateEndpointBuilder(
            WebApplication webApplication,
            Type dbContextType,
            IApiSegmentProvider apiSegmentProvider,
            IModifyMethodBehavior modifyMethodBehavior)
        {
            _webApplication = webApplication;
            _dbContextType = dbContextType;
            _apiSegmentProvider = apiSegmentProvider;
            _modifyMethodBehavior = modifyMethodBehavior;
        }
        
        public IEndpointConventionBuilder Map(Type entityType)
        {
            if (!BuildModifyMethodType(entityType, out var inputType)) _webApplication;

            var mapMethod = GetGenericMapTypedMethod(nameof(MapTypedPut));
            return (IEndpointConventionBuilder)mapMethod.MakeGenericMethod(inputType).Invoke(this, [
                _webApplication,
                entityType,
                _dbContextType
            ]);
        }

        private bool BuildModifyMethodType(Type entityType, out Type inputType)
        {
            var modifyMethod = _modifyMethodBehavior.GetModifyMethod(entityType);
            if (modifyMethod is null)
            {
                inputType = null;
                return false;
            }
            
            ParameterInfo[] parameters = modifyMethod.GetParameters();
            inputType = DynamicType.NewDynamicType(parameters, entityType, "Update");
            return true;
        }
        
        private MethodInfo GetGenericMapTypedMethod(string methodName)
        {
            return typeof(SlothfulEndpointRouteBuilder).GetMethod(methodName);
        }
        
        public IEndpointConventionBuilder MapTypedPut<T>(
            WebApplication app,
            Type entityType,
            Type dbContextType)
        {
            return app.MapPut(_apiSegmentProvider.GetApiSegment(entityType.Name) + "/{id}", ([FromRoute] Guid id, [FromBody] T command) =>
                {
                    using var serviceScope = app.Services.CreateScope();
                    var service = SlothfulTypesProvider.GetConcreteOperationService(dbContextType, entityType, serviceScope);
                    service.Update(id, command);
                    return Results.NoContent();
                })
                .WithName($"Update{entityType.Name}")
                .Produces(204)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);
        }
    }
}