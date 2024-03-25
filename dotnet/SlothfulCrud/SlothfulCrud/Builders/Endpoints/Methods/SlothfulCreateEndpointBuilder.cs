using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Builders.Endpoints.Behaviors.Constructor;
using SlothfulCrud.Providers;
using SlothfulCrud.Types;

namespace SlothfulCrud.Builders.Endpoints.Methods
{
    public class SlothfulCreateEndpointBuilder
    {
        private readonly WebApplication _webApplication;
        private readonly Type _dbContextType;
        private readonly IApiSegmentProvider _apiSegmentProvider;
        private readonly ICreateConstructorBehavior _createConstructorBehavior;

        public SlothfulCreateEndpointBuilder(
            WebApplication webApplication,
            Type dbContextType,
            IApiSegmentProvider apiSegmentProvider,
            ICreateConstructorBehavior createConstructorBehavior)
        {
            _webApplication = webApplication;
            _dbContextType = dbContextType;
            _apiSegmentProvider = apiSegmentProvider;
            _createConstructorBehavior = createConstructorBehavior;
        }
        
        public IEndpointConventionBuilder Map(Type entityType)
        {
            if (!BuildCreateCommandType(entityType, out var inputType)) return;

            var mapMethod = GetGenericMapTypedMethod(nameof(MapTypedPost));
            return (IEndpointConventionBuilder)mapMethod.MakeGenericMethod(inputType).Invoke(this, [
                _webApplication,
                entityType,
                _dbContextType
            ]);
        }
        
        private MethodInfo GetGenericMapTypedMethod(string methodName)
        {
            return typeof(SlothfulEndpointRouteBuilder).GetMethod(methodName);
        }

        private bool BuildCreateCommandType(Type entityType, out Type inputType)
        {
            var constructor = _createConstructorBehavior.GetConstructorInfo(entityType);
            if (constructor is null)
            {
                inputType = null;
                return false;
            }

            var parameters = constructor.GetParameters();
            inputType = DynamicType.NewDynamicType(parameters, entityType, "Create");
            return true;
        }
        
        public IEndpointConventionBuilder MapTypedPost<T>(
            WebApplication app,
            Type entityType,
            Type dbContextType)
        {
            return app.MapPost(_apiSegmentProvider.GetApiSegment(entityType.Name), ([FromBody] T command) =>
                {
                    var id = Guid.NewGuid();
                    using var serviceScope = app.Services.CreateScope();
                    var service = SlothfulTypesProvider.GetConcreteOperationService(dbContextType, entityType, serviceScope);
                    service.Create(id, command);
                    return Results.Created($"/{entityType.Name}s/", id);
                })
                .WithName($"Create{entityType.Name}")
                .Produces<Guid>(201)
                .Produces<BadRequestResult>(400);
        }
    }
}