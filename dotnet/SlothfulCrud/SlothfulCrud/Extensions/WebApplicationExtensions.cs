using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Exceptions.Middlewares;
using SlothfulCrud.Managers;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseSlothfulCrud(this WebApplication webApplication)
        {
            return webApplication;
        }

        public static WebApplication UseSlothfulCrud<T>(this WebApplication webApplication, Action<SlothfulOptions> configureOptions = null) where T : DbContext
        {
            var options = PrepareOptions(configureOptions);

            if (options.UseSlothfullProblemHandling)
            {
                RegisterSlothfullProblemHandling(webApplication);
            }
            
            using var scope = webApplication.Services.CreateScope();
            var manager = scope.ServiceProvider.GetRequiredService<ISlothfulCrudManager>();
            // pass configuration to the manager
            // with for example additional produces to the endpoints
            return manager.Register(webApplication, typeof(T), Assembly.GetEntryAssembly());
        }
        
        private static void RegisterSlothfullProblemHandling(this WebApplication webApplication)
        {
            webApplication.UseMiddleware<ExceptionMiddleware>();
        }

        private static SlothfulOptions PrepareOptions(Action<SlothfulOptions> configureOptions)
        {
            var options = new SlothfulOptions();
            configureOptions?.Invoke(options);
            return options;
        }
    }
}