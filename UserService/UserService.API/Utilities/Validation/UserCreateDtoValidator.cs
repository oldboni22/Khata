using FluentValidation;
using FluentValidation.Results;
using UserService.API.DTO;
using UserService.API.Exceptions;

namespace UserService.API.Utilities.Validation;

public class UserCreateDtoValidator : AbstractValidator<UserCreateDto>
{
    public UserCreateDtoValidator()
    {
        RuleFor(createDto => createDto.Name)
            .NotNull().WithMessage("Name is required")
            .NotEmpty().WithMessage("Name is required")
            .Length(1,15).WithMessage("Name must be between 1 and 15 characters");
    }

    protected override void RaiseValidationException(ValidationContext<UserCreateDto> context, ValidationResult result)
    {
        throw new GenericValidationException<UserCreateDto>(result);
    }
}
