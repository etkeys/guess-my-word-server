using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;

namespace GmwServer;

[Index(nameof(Email), IsUnique = true)]
public class User
{
    [Key]
    public UserId Id {get; init;} = new UserId(Guid.NewGuid());

    [EmailAddress]
    public MailAddress Email {get; set;} = null!;

    [MaxLength(50)]
    public string DisplayName {get; set;} = null!;
}