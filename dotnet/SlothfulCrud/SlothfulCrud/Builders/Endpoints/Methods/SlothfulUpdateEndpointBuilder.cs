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
    public class SlothfulUpdateEndpointBuilder : SlothfulMethodEndpointRouteBuilder
    {
        public SlothfulUpdateEndpointBuilder(SlothfulBuilderParams builderParams) : base(builderParams)
        {
            BuilderParams = builderParams;
        }
        
        public SlothfulUpdateEndpointBuilder Map()
        {
            if (!BuildModifyMethodType(BuilderParams.EntityType, out var inputType)) return this;

            var mapMethod = GetGenericMapTypedMethod(nameof(MapTypedPut));
            ConventionBuilder = (RouteHandlerBuilder)mapMethod.MakeGenericMethod(inputType).Invoke(this, [BuilderParams.EntityType]);

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
            
            var parameters = modifyMethod.GetParameters();
            inputType = DynamicType.NewDynamicType(parameters, entityType, "Update");
            GeneratedDynamicTypes.Add(inputType.Name, inputType);
            return true;
        }
        
        private MethodInfo GetGenericMapTypedMethod(string methodName)
        {
            return typeof(SlothfulUpdateEndpointBuilder).GetMethod(methodName);
        }
        
        public IEndpointConventionBuilder MapTypedPut<T>(Type entityType)
        {
            return BuilderParams.WebApplication.MapPut(BuilderParams.ApiSegmentProvider.GetApiSegment(entityType.Name) + "/{id}", 
                    ([FromRoute] Guid id, [FromBody] T command) =>
                {
                    using var serviceScope = BuilderParams.WebApplication.Services.CreateScope();
                    var service = SlothfulTypesProvider.GetConcreteOperationService(entityType, BuilderParams.DbContextType, serviceScope);
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