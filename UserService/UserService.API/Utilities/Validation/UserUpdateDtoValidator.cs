using FluentValidation;
using FluentValidation.Results;
using UserService.API.DTO;
using UserService.API.Exceptions;


namespace UserService.API.Utilities.Validation;

public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
{
    public UserUpdateDtoValidator()
    {
        RuleFor(updateDto => updateDto.Name)
            .NotNull().WithMessage("Name is required")
            .NotEmpty().WithMessage("Name is required")
            .Length(1, 15).WithMessage("Name must be between 1 and 15 characters");
    }

    protected override void RaiseValidationException(ValidationContext<UserUpdateDto> context, ValidationResult result)
    {
        throw new GenericValidationException<UserUpdateDto>(result);
    }
}
