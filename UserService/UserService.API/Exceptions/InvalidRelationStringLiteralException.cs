using UserService.API.Utilities.MessageGenerators.Exceptions;

namespace UserService.API.Exceptions;

public class InvalidRelationStringLiteralException(string literal) : 
    Exception(InvalidRelationStringLiteralExceptionMessageGenerator.GenerateMessage(literal))
{
}
