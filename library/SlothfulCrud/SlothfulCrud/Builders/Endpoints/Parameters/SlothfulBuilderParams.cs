using Microsoft.AspNetCore.Builder;
using SlothfulCrud.Builders.Endpoints.Behaviors.Constructor;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Builders.Endpoints.Parameters
{
    public class SlothfulBuilderParams
    {
        public WebApplication WebApplication { get; }
        public Type DbContextType { get; }
        public Type EntityType { get; }
        public IApiSegmentProvider ApiSegmentProvider { get; }
        public ICreateConstructorBehavior CreateConstructorBehavior { get; }

        public SlothfulBuilderParams(
            WebApplication webApplication,
            Type dbContextType,
            Type entityType,
            IApiSegmentProvider apiSegmentProvider,
            ICreateConstructorBehavior createConstructorBehavior)
        {
            WebApplication = webApplication;
            DbContextType = dbContextType;
            EntityType = entityType;
            ApiSegmentProvider = apiSegmentProvider;
            CreateConstructorBehavior = createConstructorBehavior;
        }
    }
}