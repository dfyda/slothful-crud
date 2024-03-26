using Microsoft.AspNetCore.Builder;
using SlothfulCrud.Builders.Endpoints.Behaviors.Constructor;
using SlothfulCrud.Builders.Endpoints.Behaviors.ModifyMethod;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Builders.Endpoints.Parameters
{
    public class SlothfulBuilderParams
    {
        public WebApplication WebApplication { get; }
        public Type DbContextType { get; }
        public IApiSegmentProvider ApiSegmentProvider { get; }
        public ICreateConstructorBehavior CreateConstructorBehavior { get; }
        public IModifyMethodBehavior ModifyMethodBehavior { get; }

        public SlothfulBuilderParams(
            WebApplication webApplication,
            Type dbContextType,
            IApiSegmentProvider apiSegmentProvider,
            ICreateConstructorBehavior createConstructorBehavior,
            IModifyMethodBehavior modifyMethodBehavior)
        {
            WebApplication = webApplication;
            DbContextType = dbContextType;
            ApiSegmentProvider = apiSegmentProvider;
            CreateConstructorBehavior = createConstructorBehavior;
            ModifyMethodBehavior = modifyMethodBehavior;
        }
    }
}