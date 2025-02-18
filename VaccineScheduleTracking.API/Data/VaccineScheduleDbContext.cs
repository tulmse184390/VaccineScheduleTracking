﻿using Microsoft.EntityFrameworkCore;
using VaccineScheduleTracking.API.Models.Entities;
using VaccineScheduleTracking.API_Test.Models.Entities;

namespace VaccineScheduleTracking.API.Data
{
    public class VaccineScheduleDbContext : DbContext
    {
        public VaccineScheduleDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {

        }

        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<DailySchedule> DailySchedule { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Child> Children { get; set; }
        public DbSet<VaccineType> VaccineTypes { get; set; }
        public DbSet<Vaccine> Vaccines { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
    }
}
