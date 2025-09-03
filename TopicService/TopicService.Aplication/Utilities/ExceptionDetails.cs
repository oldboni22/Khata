using System.Net;

namespace UserService.API.Utilities;

public record ExceptionDetails(string Message, int StatusCode);
