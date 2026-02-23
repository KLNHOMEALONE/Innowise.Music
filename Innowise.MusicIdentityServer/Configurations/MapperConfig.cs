using AutoMapper;
using Innowise.MusicIdentityServer.Data;
using Innowise.MusicIdentityServer.Models.User;

namespace Innowise.MusicIdentityServer.Configurations;

public class MapperConfig : Profile
{
    public MapperConfig()
    {
        CreateMap<ApiUser, UserDto>().ReverseMap();
    }
    
}