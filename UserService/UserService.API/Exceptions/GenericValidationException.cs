using System.ComponentModel.DataAnnotations;
using UserService.API.Utilities.MessageGenerators.Exceptions;

namespace UserService.API.Exceptions;

public class GenericValidationException<T>() : ValidationException(GenericValidationExceptionMessageGenerator.GenerateMessage<T>())
{
}
