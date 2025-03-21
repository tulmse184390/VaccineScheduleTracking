﻿using VaccineScheduleTracking.API.Helpers;
using VaccineScheduleTracking.API_Test.Helpers;

namespace VaccineScheduleTracking.API_Test.Models.DTOs.Appointments
{
    public class UpdateAppointmentDto
    {
        //public int AppointmentID { get; set; }
        public int ChildID { get; set; }
        public int DoctorID { get; set; }
        public int VaccineID { get; set; }
        public int SlotNumber { get; set; }
        [ValidDate]
        public DateOnly Date { get; set; }
        
    }
}
