using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Application.DTOS.Appoiment;
using AppointmentSystem.Domain.Entities;
using AutoMapper;

namespace AppointmentSystem.Application.Mapper
{
    public class AppointmentProfile : Profile
    {
        public AppointmentProfile()
        {
            CreateMap<Appointment, AppointmentDto>().ReverseMap();
            CreateMap<AppointmentCreateDto, Appointment>();
            CreateMap<AppointmentUpdateDto, Appointment>();
            CreateMap<Appointment, AppoimentMonthDto>()
            .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.Client.Id))
            .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client.FullName))
            .ForMember(dest => dest.ClientEmail, opt => opt.MapFrom(src => src.Client.Email))
            .ForMember(dest => dest.ClientPhoneNumber, opt => opt.MapFrom(src => src.Client.PhoneNumber))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.TimeSlot.WorkingDay.Date))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.TimeSlot.StartTime))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.TimeSlot.EndTime));
        }
    }
}
