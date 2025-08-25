using FluentValidation;
using UserService.API.DTO;

namespace UserService.API.Utilities.Validation;

public class UserCreateDtoValidator : AbstractValidator<UserCreateDto>
{
    public UserCreateDtoValidator()
    {
        RuleFor(createDto => createDto.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(1,15).WithMessage("Name must be between 1 and 15 characters");
    }
}