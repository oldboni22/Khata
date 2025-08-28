using System.ComponentModel;
using FluentValidation;
using FluentValidation.Results;
using Shared.Enums;
using UserService.API.Utilities.MessageGenerators.Exceptions;

namespace UserService.API.Utilities.Validation;

public class UserTopicStatusValidator : AbstractValidator<UserTopicRelationStatus>
{
    public UserTopicStatusValidator()
    {
        RuleFor(status => status)
            .NotNull()
            .NotEmpty()
            .IsInEnum();
    }

    protected override void RaiseValidationException(ValidationContext<UserTopicRelationStatus> context, ValidationResult result)
    {
        throw new BadHttpRequestException(InvalidEnumExceptionMessageGenerator<UserTopicRelationStatus>.GenerateMessage());
    }
}
