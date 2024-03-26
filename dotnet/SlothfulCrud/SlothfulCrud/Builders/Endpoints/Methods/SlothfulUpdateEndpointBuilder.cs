using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Builders.Endpoints.Parameters;
using SlothfulCrud.Providers;
using SlothfulCrud.Types;

namespace SlothfulCrud.Builders.Endpoints.Methods
{
    public class SlothfulUpdateEndpointBuilder : SlothfulEndpointRouteBuilder
    {
        private RouteHandlerBuilder ConventionBuilder { get; set; }

        public SlothfulUpdateEndpointBuilder(SlothfulBuilderParams builderParams) : base(builderParams)
        {
            BuilderParams = builderParams;
        }
        
        public SlothfulUpdateEndpointBuilder Map(Type entityType)
        {
            if (!BuildModifyMethodType(entityType, out var inputType)) return this;

            var mapMethod = GetGenericMapTypedMethod(nameof(MapTypedPut));
            ConventionBuilder = (RouteHandlerBuilder)mapMethod.MakeGenericMethod(inputType).Invoke(this, [
                BuilderParams.WebApplication,
                entityType,
                BuilderParams.DbContextType
            ]);

            return this;
        }

        private bool BuildModifyMethodType(Type entityType, out Type inputType)
        {
            var modifyMethod = BuilderParams.ModifyMethodBehavior.GetModifyMethod(entityType);
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
            return typeof(SlothfulUpdateEndpointBuilder).GetMethod(methodName);
        }
        
        public IEndpointConventionBuilder MapTypedPut<T>(
            WebApplication app,
            Type entityType,
            Type dbContextType)
        {
            return app.MapPut(BuilderParams.ApiSegmentProvider.GetApiSegment(entityType.Name) + "/{id}", ([FromRoute] Guid id, [FromBody] T command) =>
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