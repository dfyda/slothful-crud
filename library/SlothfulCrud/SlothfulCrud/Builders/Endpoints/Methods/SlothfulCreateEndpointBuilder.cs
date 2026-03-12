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
    internal class SlothfulCreateEndpointBuilder<TEntity> : SlothfulMethodEndpointRouteBuilder<TEntity>
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
            ConventionBuilder = (RouteHandlerBuilder)mapMethod
                .MakeGenericMethod(EndpointsConfiguration.Entity.KeyPropertyType, inputType)
                .Invoke(this, [BuilderParams.EntityType]);

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

        public IEndpointConventionBuilder MapTypedPost<TKeyType, TCommandType>(Type entityType)
        {
            var apiSegment = BuilderParams.ApiSegmentProvider.GetApiSegment(entityType.Name);
            var endpoint = BuilderParams.WebApplication.MapPost(apiSegment, 
                    ([FromBody] TCommandType command) =>
                {
                    using var serviceScope = BuilderParams.WebApplication.Services.CreateScope();
                    var entityPropertyKeyValueProvider = SlothfulTypesProvider.GetEntityPropertyKeyValueProvider(entityType, serviceScope);
                    var keyProperty = (object)entityPropertyKeyValueProvider.GetNextValue(EndpointsConfiguration.Entity);
                    var service = SlothfulTypesProvider.GetConcreteOperationService(entityType, BuilderParams.DbContextType, serviceScope);
                    service.Create(keyProperty, command, serviceScope);
                    return Results.Created(BuildCreatedLocation(apiSegment, keyProperty), keyProperty);
                })
                .WithName($"Create{entityType.Name}")
                .Produces<TKeyType>(201)
                .Produces<BadRequestResult>(400);
            
            if (EndpointsConfiguration.Create.IsAuthorizationEnable)
            {
                endpoint.RequireAuthorization(EndpointsConfiguration.Create.PolicyNames);
            }

            return endpoint;
        }

        internal static string BuildCreatedLocation(string apiSegment, object keyProperty)
        {
            if (string.IsNullOrWhiteSpace(apiSegment))
            {
                throw new ArgumentException("API segment cannot be null or whitespace.", nameof(apiSegment));
            }

            if (keyProperty is null)
            {
                throw new ArgumentNullException(nameof(keyProperty));
            }

            var normalizedSegment = apiSegment.StartsWith("/") ? apiSegment : $"/{apiSegment}";
            return $"{normalizedSegment}/{keyProperty}";
        }
    }
}