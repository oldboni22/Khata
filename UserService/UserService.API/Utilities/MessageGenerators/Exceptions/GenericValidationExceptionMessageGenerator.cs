using FluentValidation.Results;

namespace UserService.API.Utilities.MessageGenerators.Exceptions;

public static class GenericValidationExceptionMessageGenerator
{
    public static string GenerateMessage<T>(ValidationResult result) => 
        $"Validation for {typeof(T).Name} failed. Message : {result.Errors}";
}