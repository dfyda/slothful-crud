using FluentValidation;
using SlothfulCrud.Tests.Api.Domain;

namespace SlothfulCrud.Tests.Api.Validators
{
    public class SlothValidator : AbstractValidator<Sloth>
    {
        public SlothValidator()
        {
            RuleFor(sloth => sloth.Name)
                .NotEmpty()
                .MaximumLength(255);
        }
    }
}