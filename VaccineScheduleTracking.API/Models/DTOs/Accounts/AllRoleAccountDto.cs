﻿using VaccineScheduleTracking.API.Models.Entities;
using VaccineScheduleTracking.API_Test.Models.Entities;

namespace VaccineScheduleTracking.API_Test.Models.DTOs.Accounts
{
    public class AllRoleAccountDto
    {
        public int AccountID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? Avatar { get; set; }
        public string Status { get; set; }

        public Parent? Parent { get; set; }
        public Doctor? Doctor { get; set; }
        public Staff? Staff { get; set; }
        public Manager? Manager { get; set; }
    }
}
