using FluentValidation;
using UserService.API.DTO;

namespace UserService.API.Utilities.Validation;

public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
{
    public UserUpdateDtoValidator()
    {
        RuleFor(updateDto => updateDto.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(1, 15).WithMessage("Name must be between 1 and 15 characters");
    }
}