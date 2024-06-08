using FluentValidation;
using SlothfulCrud.Api.Domain;

namespace SlothfulCrud.Api.Validators
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