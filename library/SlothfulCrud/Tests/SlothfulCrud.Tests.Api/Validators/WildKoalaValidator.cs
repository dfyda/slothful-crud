using FluentValidation;
using SlothfulCrud.Tests.Api.Domain;

namespace SlothfulCrud.Tests.Api.Validators
{
    public class WildKoalaValidator : AbstractValidator<WildKoala>
    {
        public WildKoalaValidator()
        {
            RuleFor(sloth => sloth.Name)
                .NotEmpty()
                .MaximumLength(15);
        }
    }
}