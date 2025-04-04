﻿using System.Text.Json.Serialization;
using VaccineScheduleTracking.API_Test.Models.Entities;

namespace VaccineScheduleTracking.API.Models.Entities
{
    public class Doctor
    {
        public int DoctorID { get; set; }
        public int AccountID { get; set; }

        [JsonIgnore]
        public Account Account { get; set; }
        public string? DoctorTimeSlots { get; set; }
    }
}
