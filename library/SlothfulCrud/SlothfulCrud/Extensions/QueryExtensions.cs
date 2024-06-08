using System.Collections;
using SlothfulCrud.Exceptions;

namespace SlothfulCrud.Extensions
{
    internal static class QueryExtensions
    {
        private const string DefaultCode = "not_found";
        private const string DefaultMessage = "Object not found";
        
        public static async Task<T> OrFail<T>(
            this Task<T> task,
            string code = DefaultCode,
            string message = DefaultMessage)
        {
            var instance = await task;       

            return instance.OrFail(code, message);
        }

        public static T OrFail<T>(this T instance,
            string code = DefaultCode,
            string message = DefaultMessage)
        {           
            if (instance == null)
            {
                throw new NotFoundException(code, message);
            }

            if (instance is IEnumerable c)
            {
                var enumerator = c.GetEnumerator();
                using var cleaner = enumerator as IDisposable;
                if (!enumerator.MoveNext())
                {
                    throw new NotFoundException(code, message);
                }
            }

            return instance;
        }
    }
}