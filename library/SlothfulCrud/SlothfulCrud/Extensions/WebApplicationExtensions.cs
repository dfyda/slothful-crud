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

        [Obsolete("Use UseSlothfulCrud<TDbContext, TAssemblyMarker>() instead. This overload relies on Assembly.GetEntryAssembly() which may not work correctly in test or hosted scenarios.")]
        public static WebApplication UseSlothfulCrud<T>(this WebApplication webApplication, Action<SlothfulOptions> configureOptions = null) where T : DbContext
        {
            return webApplication.UseSlothfulCrudCore<T>(Assembly.GetEntryAssembly(), configureOptions);
        }

        public static WebApplication UseSlothfulCrud<TDbContext, TAssemblyMarker>(
            this WebApplication webApplication,
            Action<SlothfulOptions> configureOptions = null)
            where TDbContext : DbContext
        {
            return webApplication.UseSlothfulCrudCore<TDbContext>(typeof(TAssemblyMarker).Assembly, configureOptions);
        }

        private static WebApplication UseSlothfulCrudCore<TDbContext>(
            this WebApplication webApplication,
            Assembly assembly,
            Action<SlothfulOptions> configureOptions = null)
            where TDbContext : DbContext
        {
            var options = webApplication.Services.GetService<SlothfulOptions>() ?? new SlothfulOptions();
            configureOptions?.Invoke(options);

            if (options.UseSlothfulProblemHandling)
            {
                RegisterSlothfullProblemHandling(webApplication);
            }
            
            using var scope = webApplication.Services.CreateScope();
            var manager = scope.ServiceProvider.GetRequiredService<ISlothfulCrudManager>();
            return manager.Register(webApplication, typeof(TDbContext), assembly);
        }
        
        private static void RegisterSlothfullProblemHandling(this WebApplication webApplication)
        {
            webApplication.UseMiddleware<ExceptionMiddleware>();
        }
    }
}