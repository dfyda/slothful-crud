using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Builders.Configurations;
using SlothfulCrud.Builders.Endpoints.Parameters;
using SlothfulCrud.Domain;
using SlothfulCrud.Providers;
using SlothfulCrud.Providers.Types;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Builders.Endpoints.Methods
{
    public class SlothfulUpdateEndpointBuilder<TEntity> : SlothfulMethodEndpointRouteBuilder<TEntity>
        where TEntity : class, ISlothfulEntity
    {
        public SlothfulUpdateEndpointBuilder(
            SlothfulBuilderParams builderParams,
            EndpointsConfiguration endpointsConfiguration,
            IDictionary<string, Type> generatedDynamicTypes,
            SlothEntityBuilder<TEntity> configurationBuilder
        ) : base(builderParams, endpointsConfiguration, generatedDynamicTypes, configurationBuilder)
        {
            BuilderParams = builderParams;
        }
        
        public SlothfulUpdateEndpointBuilder<TEntity> Map()
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
            
            inputType = CommandProvider.PrepareUpdateCommand(modifyMethod, entityType);
            GeneratedDynamicTypes.Add(inputType.Name, inputType);
            return true;
        }
        
        private MethodInfo GetGenericMapTypedMethod(string methodName)
        {
            return typeof(SlothfulUpdateEndpointBuilder<TEntity>).GetMethod(methodName);
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