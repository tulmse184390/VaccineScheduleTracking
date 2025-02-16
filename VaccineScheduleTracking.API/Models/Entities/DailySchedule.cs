﻿using VaccineScheduleTracking.API.Models.DTOs;

namespace VaccineScheduleTracking.API.Models.Entities
{
    public class DailySchedule
    {
        public int DailyScheduleID { get; set; }
        public int Slot { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime => StartTime.AddMinutes(45);

    }
}
