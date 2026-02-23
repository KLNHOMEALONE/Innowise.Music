using Microsoft.AspNetCore.Identity;

namespace Innowise.MusicIdentityServer.Data;

public class ApiUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

}