﻿using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Builders.Endpoints.Parameters;
using SlothfulCrud.Providers;
using SlothfulCrud.Types;

namespace SlothfulCrud.Builders.Endpoints.Methods
{
    public class SlothfulGetEndpointBuilder : SlothfulMethodEndpointRouteBuilder
    {
        public SlothfulGetEndpointBuilder(SlothfulBuilderParams builderParams) : base(builderParams)
        {
            BuilderParams = builderParams;
        }
        
        public SlothfulGetEndpointBuilder Map(Type entityType)
        {
            var mapMethod = GetGenericMapTypedMethod(nameof(MapTypedGet));
            var resultType = BuildGetDtoType(entityType);
            ConventionBuilder = (RouteHandlerBuilder)mapMethod.MakeGenericMethod(resultType).Invoke(this, [entityType]);

            return this;
        }
        
        private Type BuildGetDtoType(Type entityType)
        {
            return DynamicType.NewDynamicTypeDto(entityType, $"{entityType}DetailsDto");
        }
        
        private MethodInfo GetGenericMapTypedMethod(string methodName)
        {
            return typeof(SlothfulGetEndpointBuilder).GetMethod(methodName);
        }
        
        public IEndpointConventionBuilder MapTypedGet<T>(Type entityType)
        {
            return BuilderParams.WebApplication.MapGet(BuilderParams.ApiSegmentProvider.GetApiSegment(entityType.Name) + "/{id}", (Guid id) =>
                {
                    using var serviceScope = BuilderParams.WebApplication.Services.CreateScope();
                    var service =
                        SlothfulTypesProvider.GetConcreteOperationService(entityType, BuilderParams.DbContextType, serviceScope);
                    var item = service.Get(id);
                    var resultDto = DynamicType.MapToDto(item, entityType, typeof(T));
                    return resultDto;
                })
                .WithName($"Get{entityType.Name}Details")
                .Produces(200, typeof(T))
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);
        }
    }
}