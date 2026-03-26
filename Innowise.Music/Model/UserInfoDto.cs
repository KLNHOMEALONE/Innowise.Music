namespace Innowise.Music.Model;

public class UserInfoDto
{
    public string Token { get; set; }
    public Google.Apis.Oauth2.v2.Data.Userinfo UserInfo { get; set; }
}