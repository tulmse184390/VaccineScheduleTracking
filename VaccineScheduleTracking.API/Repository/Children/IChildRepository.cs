﻿using VaccineScheduleTracking.API.Models.Entities;
using VaccineScheduleTracking.API_Test.Models.Entities;

namespace VaccineScheduleTracking.API_Test.Repository.Children
{
    public interface IChildRepository
    {
        Task<List<Child>> GetChildrenByParentID(int parentID);
        Task<List<Child>> GetAllChildrenAsync();
        Task<Child?> GetChildByID(int id);
        Task<Child> AddChild(Child child);
        Task<Child> UpdateChild(int id, Child updateChild);
        Task<Child> DeleteChildAsync(Child child);

        //-----------------ChildTimeSlot-----------------
        Task<ChildTimeSlot?> GetChildTimeSlotBySlotNumberAsync(int slotNumber, DateOnly date);
        Task<ChildTimeSlot> AddChildTimeSlotAsync(ChildTimeSlot childTimeSlot);
        Task<List<ChildTimeSlot>> GetChildTimeSlotsForDayAsync(int childID, DateOnly appointmentDate);
        Task UpdateChildTimeSlotsAsync(ChildTimeSlot slot);
    }
}
