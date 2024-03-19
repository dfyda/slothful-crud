using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Managers;

namespace SlothfulCrud.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseSlothfulCrud(this WebApplication webApplication)
        {
            return webApplication;
        }

        public static WebApplication UseSlothfulCrud<T>(this WebApplication webApplication) where T : DbContext
        {
            using var scope = webApplication.Services.CreateScope();
            var manager = scope.ServiceProvider.GetRequiredService<ISlothfulCrudManager>();
            return manager.Register(webApplication, typeof(T), Assembly.GetEntryAssembly());
        }
    }
}