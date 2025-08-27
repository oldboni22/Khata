namespace UserService.API.Utilities.MessageGenerators;

public static class UnauthorizedWebhookMessageGenerator
{
    public static string GenerateMessage(string webhook) => $"The webhook '{webhook}' is not authorized.";
}
