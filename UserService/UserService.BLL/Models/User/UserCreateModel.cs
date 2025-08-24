using System.ComponentModel.DataAnnotations;

namespace UserService.BLL.Models.User;

public class UserCreateModel
{
    public required string Name { get; set; }
}
