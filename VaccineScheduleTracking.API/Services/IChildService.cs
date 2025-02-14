﻿using VaccineScheduleTracking.API.Models.Entities;

namespace VaccineScheduleTracking.API.Services
{
    public interface IChildService
    {
        Task<List<Child>> GetParentChildren(int parentID); 
        Task<Child> AddChild(Child child);
        Task<Child> UpdateChild(int id, Child child);
    }
}
