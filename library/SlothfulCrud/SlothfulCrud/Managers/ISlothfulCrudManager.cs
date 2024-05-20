using System.Reflection;
using Microsoft.AspNetCore.Builder;

namespace SlothfulCrud.Managers
{
    public interface ISlothfulCrudManager
    {
        WebApplication Register(WebApplication webApplication, Type dbContextType, Assembly executingAssembly);
    } 
}