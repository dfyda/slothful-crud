using Microsoft.AspNetCore.Mvc;

namespace SlothfulCrud.Exceptions
{
    public class SlothProblemDetails : ProblemDetails
    {
        public Guid ProblemId { get; set; }
        public string Code { get; set; }
    }
}