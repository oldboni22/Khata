namespace UserService.BLL.Models.User;

public class UserUpdateModel
{
    public required string Name { get; init; }

    public string? Auth0Id { get; set; }
}
