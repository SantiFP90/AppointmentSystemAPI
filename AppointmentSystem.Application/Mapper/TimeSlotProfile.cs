using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Application.DTOS.TimeSlot;
using AppointmentSystem.Domain.Entities;
using AutoMapper;

namespace AppointmentSystem.Application.Mapper
{
    public class TimeSlotProfile : Profile
    {
        public TimeSlotProfile()
        {
            CreateMap<TimeSlot, TimeSlotDto>().ReverseMap();
        }
    }
}
