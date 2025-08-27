using UserService.API.Utilities.MessageGenerators;
using UserService.API.Utilities.MessageGenerators.Exceptions;

namespace UserService.API.Exceptions.Unauthorized;

public class UnauthorizedWebhookException(string webhook) : 
    UnauthorizedException(UnauthorizedWebhookMessageGenerator.GenerateMessage(webhook))
{
}
