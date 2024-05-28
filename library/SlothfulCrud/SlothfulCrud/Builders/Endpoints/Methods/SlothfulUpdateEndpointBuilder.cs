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
            if (!EndpointsConfiguration.Update.IsEnable) 
                return this;
            
            if (!BuildModifyMethodType(BuilderParams.EntityType, out var inputType))
                return this;

            var mapMethod = GetGenericMapTypedMethod(nameof(MapTypedPut));
            ConventionBuilder = (RouteHandlerBuilder)mapMethod
                .MakeGenericMethod(EndpointsConfiguration.Entity.KeyPropertyType, inputType)
                .Invoke(this, [BuilderParams.EntityType]);

            return this;
        }

        private bool BuildModifyMethodType(Type entityType, out Type inputType)
        {
            var modifyMethod = entityType.GetMethod(EndpointsConfiguration.Entity.UpdateMethod);
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
        
        public IEndpointConventionBuilder MapTypedPut<TKeyType, TCommand>(Type entityType)
        {
            var endpoint = BuilderParams.WebApplication.MapPut(BuilderParams.ApiSegmentProvider.GetApiSegment(entityType.Name) + "/{id}", 
                    ([FromRoute] TKeyType id, [FromBody] TCommand command) =>
                {
                    using var serviceScope = BuilderParams.WebApplication.Services.CreateScope();
                    var service = SlothfulTypesProvider.GetConcreteOperationService(entityType, BuilderParams.DbContextType, serviceScope);
                    service.Update(id, command, serviceScope);
                    return Results.NoContent();
                })
                .WithName($"Update{entityType.Name}")
                .Produces(204)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);
            
            if (EndpointsConfiguration.Update.IsAuthorizationEnable)
            {
                endpoint.RequireAuthorization(EndpointsConfiguration.Update.PolicyNames);
            }

            return endpoint;
        }
    }
}