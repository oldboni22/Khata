using AutoMapper;
using UserService.BLL.Models.User;

namespace UserService.API.Utilities.MessageGenerators.Exceptions;

public static class GenericValidationExceptionMessageGenerator
{
    public static string GenerateMessage<T>() => $"Validation for {typeof(T).Name} failed";
}