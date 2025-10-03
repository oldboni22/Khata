using Domain.Exceptions.Messages;

namespace Domain.Exceptions;

public class InvalidStringInputException(int minLenght, int maxLength) 
    : Exception(InvalidStringInputExceptionMessageGenerator.Generate(minLenght, maxLength))
{
    
}
