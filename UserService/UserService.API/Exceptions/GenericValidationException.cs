using FluentValidation;
using FluentValidation.Results;
using UserService.API.Utilities.MessageGenerators.Exceptions;

namespace UserService.API.Exceptions;

public class GenericValidationException<T>(ValidationResult result) : 
    ValidationException(GenericValidationExceptionMessageGenerator.GenerateMessage<T>(result))
{
}
