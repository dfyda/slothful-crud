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
    public class SlothfulCreateEndpointBuilder<TEntity> : SlothfulMethodEndpointRouteBuilder<TEntity>
        where TEntity : class, ISlothfulEntity
    {
        public SlothfulCreateEndpointBuilder(
            SlothfulBuilderParams builderParams,
            EndpointsConfiguration endpointsConfiguration,
            IDictionary<string, Type> generatedDynamicTypes,
            SlothEntityBuilder<TEntity> configurationBuilder
        ) : base(builderParams, endpointsConfiguration, generatedDynamicTypes, configurationBuilder)
        {
            BuilderParams = builderParams;
        }
        
        public SlothfulCreateEndpointBuilder<TEntity> Map()
        {
            if (!EndpointsConfiguration.Create.IsEnable) 
                return this;
            
            if (!BuildCreateCommandType(BuilderParams.EntityType, out var inputType))
                return this;

            var mapMethod = GetGenericMapTypedMethod(nameof(MapTypedPost));
            ConventionBuilder = (RouteHandlerBuilder)mapMethod.MakeGenericMethod(inputType).Invoke(this, [BuilderParams.EntityType]);

            return this;
        }
        
        private MethodInfo GetGenericMapTypedMethod(string methodName)
        {
            return typeof(SlothfulCreateEndpointBuilder<TEntity>).GetMethod(methodName);
        }

        private bool BuildCreateCommandType(Type entityType, out Type inputType)
        {
            var constructor = BuilderParams.CreateConstructorBehavior.GetConstructorInfo(entityType);
            if (constructor is null)
            {
                inputType = null;
                return false;
            }

            inputType = CommandProvider.PrepareCreateCommand(constructor, entityType);
            GeneratedDynamicTypes.Add(inputType.Name, inputType);
            return true;
        }

        public IEndpointConventionBuilder MapTypedPost<TCommandType>(Type entityType)
        {
            var endpoint = BuilderParams.WebApplication.MapPost(BuilderParams.ApiSegmentProvider.GetApiSegment(entityType.Name), 
                    ([FromBody] TCommandType command) =>
                {
                    using var serviceScope = BuilderParams.WebApplication.Services.CreateScope();
                    var entityPropertyKeyValueProvider = SlothfulTypesProvider.GetEntityPropertyKeyValueProvider(entityType, serviceScope);
                    var keyProperty = (object)entityPropertyKeyValueProvider.GetNextValue(EndpointsConfiguration.Entity);
                    var service = SlothfulTypesProvider.GetConcreteOperationService(entityType, BuilderParams.DbContextType, serviceScope);
                    service.Create(keyProperty, command, serviceScope);
                    return Results.Created($"/{entityType.Name}s/", keyProperty);
                })
                .WithName($"Create{entityType.Name}")
                .Produces<Guid>(201)
                .Produces<BadRequestResult>(400);
            
            if (EndpointsConfiguration.Create.IsAuthorizationEnable)
            {
                endpoint.RequireAuthorization(EndpointsConfiguration.Create.PolicyNames);
            }

            return endpoint;
        }
    }
}