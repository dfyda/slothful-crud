using System.Reflection;
using Microsoft.AspNetCore.Builder;
using SlothfulCrud.Builders.Configurations;
using SlothfulCrud.Builders.Endpoints;
using SlothfulCrud.Builders.Endpoints.Behaviors.Constructor;
using SlothfulCrud.Builders.Endpoints.Parameters;
using SlothfulCrud.Domain;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Managers
{
    public class SlothfulCrudManager : ISlothfulCrudManager
    {
        private readonly IApiSegmentProvider _apiSegmentProvider;
        private readonly ICreateConstructorBehavior _createConstructorBehavior;
        private readonly SlothConfigurationBuilder _slothConfigurationBuilder = new();

        public SlothfulCrudManager(
            IApiSegmentProvider apiSegmentProvider,
            ICreateConstructorBehavior createConstructorBehavior)
        {
            _apiSegmentProvider = apiSegmentProvider;
            _createConstructorBehavior = createConstructorBehavior;
        }
        
        public WebApplication Register(WebApplication webApplication, Type dbContextType, Assembly executingAssembly)
        {
            _slothConfigurationBuilder.ApplyConfigurationsFromAssembly(executingAssembly);
            
            var getConfigurationBuilderMethod =
                typeof(SlothConfigurationBuilder).GetMethod(nameof(SlothConfigurationBuilder.GetBuilder));
            var buildEndpointMethod = this.GetType()
                .GetMethod(nameof(BuildEndpoints), BindingFlags.NonPublic | BindingFlags.Instance)
                .GetGenericMethodDefinition();
            
            var entityTypes = SlothfulTypesProvider.GetSlothfulEntityTypes(executingAssembly);
            foreach (var entityType in entityTypes)
            {
                RegisterEntity(webApplication, dbContextType, getConfigurationBuilderMethod, entityType, buildEndpointMethod);
            }

            return webApplication;
        }

        private void RegisterEntity(WebApplication webApplication, Type dbContextType, MethodInfo getConfigurationBuilderMethod,
            Type entityType, MethodInfo buildEndpointMethod)
        {
            var configurationBuilder = getConfigurationBuilderMethod.MakeGenericMethod(entityType).Invoke(_slothConfigurationBuilder, null);
                
            var parameters = new SlothfulBuilderParams(
                webApplication,
                dbContextType,
                entityType,
                _apiSegmentProvider,
                _createConstructorBehavior);
                
            var genericBuilderType = typeof(SlothfulEndpointRouteBuilder<>).MakeGenericType(entityType);
            var builder = Activator.CreateInstance(genericBuilderType, parameters, configurationBuilder);
                
            buildEndpointMethod.MakeGenericMethod(entityType).Invoke(this, [builder]);
        }

        private void BuildEndpoints<TEntity>(SlothfulEndpointRouteBuilder<TEntity> builder)
            where TEntity : class, ISlothfulEntity
        {
            builder
                .GetEndpoint.Map()
                .BrowseEndpoint.Map()
                .BrowseSelectableEndpoint.Map()
                .CreateEndpoint.Map()
                .UpdateEndpoint.Map()
                .DeleteEndpoint.Map();
        }
    }
}