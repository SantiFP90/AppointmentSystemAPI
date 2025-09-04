using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Infrastructure.Persistence.Configurations
{
    public class TimeSlotConfiguration : IEntityTypeConfiguration<TimeSlot>
    {
        public void Configure(EntityTypeBuilder<TimeSlot> entity)
        {
            entity
                .HasOne(ts => ts.WorkingDay)
                .WithMany(wd => wd.TimeSlots)
                .HasForeignKey(ts => ts.WorkingDayId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
