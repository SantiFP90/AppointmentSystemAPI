using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Application.DTOS.WorkingDay;
using AppointmentSystem.Domain.Entities;
using AutoMapper;

namespace AppointmentSystem.Application.Mapper
{
    public class WorkingDayProfile : Profile
    {
        public WorkingDayProfile()
        {
            CreateMap<WorkingDay, WorkingDayDto>().ReverseMap();
            CreateMap<WorkingDayCreateDto, WorkingDay>();
        }
    }
}
