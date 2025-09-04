using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Application.DTOS.Notification;
using AppointmentSystem.Domain.Entities;
using AutoMapper;

namespace AppointmentSystem.Application.Mapper
{
    public class NotificationLogProfile : Profile
    {
        public NotificationLogProfile()
        {
            CreateMap<NotificationLog, NotificationLogDto>().ReverseMap();
        }
    }
}
