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
    public class SlothfulCreateEndpointBuilder : SlothfulMethodEndpointRouteBuilder
    {
        public SlothfulCreateEndpointBuilder(SlothfulBuilderParams builderParams) : base(builderParams)
        {
            BuilderParams = builderParams;
        }
        
        public SlothfulCreateEndpointBuilder Map(Type entityType)
        {
            if (!BuildCreateCommandType(entityType, out var inputType)) return this;

            var mapMethod = GetGenericMapTypedMethod(nameof(MapTypedPost));
            ConventionBuilder = (RouteHandlerBuilder)mapMethod.MakeGenericMethod(inputType).Invoke(this, [entityType]);

            return this;
        }
        
        private MethodInfo GetGenericMapTypedMethod(string methodName)
        {
            return typeof(SlothfulCreateEndpointBuilder).GetMethod(methodName);
        }

        private bool BuildCreateCommandType(Type entityType, out Type inputType)
        {
            var constructor = BuilderParams.CreateConstructorBehavior.GetConstructorInfo(entityType);
            if (constructor is null)
            {
                inputType = null;
                return false;
            }

            var parameters = constructor.GetParameters();
            inputType = DynamicType.NewDynamicType(parameters, entityType, "Create");
            return true;
        }
        
        public IEndpointConventionBuilder MapTypedPost<T>(Type entityType)
        {
            return BuilderParams.WebApplication.MapPost(BuilderParams.ApiSegmentProvider.GetApiSegment(entityType.Name), ([FromBody] T command) =>
                {
                    var id = Guid.NewGuid();
                    using var serviceScope = BuilderParams.WebApplication.Services.CreateScope();
                    var service = SlothfulTypesProvider.GetConcreteOperationService(entityType, BuilderParams.DbContextType, serviceScope);
                    service.Create(id, command);
                    return Results.Created($"/{entityType.Name}s/", id);
                })
                .WithName($"Create{entityType.Name}")
                .Produces<Guid>(201)
                .Produces<BadRequestResult>(400);
        }
    }
}