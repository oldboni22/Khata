using System.ComponentModel.DataAnnotations;

namespace UserService.BLL.Models.User;

public class UserCreateModel
{
    [MaxLength(15)]
    public required string Name { get; set; }
}