using AppointmentSystem.Domain.Entities;
using AutoMapper;
using AppointmentSystem.Application.DTOS.User;

namespace AppointmentSystem.Application.Mapper
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<RegisterUserDto, User>();
            CreateMap<User, UserDto>()
             .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name))
             .ReverseMap()
             .ForMember(dest => dest.Role, opt => opt.Ignore());
        }
    }
}
