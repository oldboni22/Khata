namespace UserService.API.DTO;

public class UserCreateDto
{
    public required string Name { get; set; }
    
    public required string Auth0Id { get; set; }
}
