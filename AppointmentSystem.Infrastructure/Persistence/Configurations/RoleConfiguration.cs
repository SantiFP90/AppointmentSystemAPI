using System;
using System.Collections.Generic;
using AppointmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Infrastructure.Persistence.Configurations
{
    internal class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> entity)
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.Name).IsUnique();

            entity.Property(r => r.Id)
                  .ValueGeneratedOnAdd();

            entity.HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Client" },
                new Role { Id = 3, Name = "User" }
            );
        }
    }
}

