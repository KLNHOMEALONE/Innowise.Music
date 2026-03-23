using AutoMapper;
using Innowise.MusicIdentityServer.Data;
using Innowise.MusicIdentityServer.Models.User;

namespace Innowise.MusicIdentityServer.Configurations;

public class MapperConfig : Profile
{
    public MapperConfig()
    {
        CreateMap<UserDto, ApiUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));
        CreateMap<ApiUser, UserDto>();
    }
    
}