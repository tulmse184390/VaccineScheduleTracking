﻿using VaccineScheduleTracking.API.Models.Entities;
using VaccineScheduleTracking.API_Test.Models.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VaccineScheduleTracking.API_Test.Repository.Doctors
{
    public interface IDoctorRepository
    {
        //Doctor
        //===================================================
        Task<Account> GetAccountByAccountIDAsync(int accountId);
        Task<Doctor?> GetDoctorByAccountIDAsync(int accountId);
        //====================================================
        Task<List<Account>> GetAllDoctorAsync();
        //Task<Doctor?> GetSuitableDoctor(int slot, DateTime time);
        Task<Account> AddDoctorByAccountIdAsync(Account account, Doctor doctorInfo);
        Task<Account?> GetDoctorByIDAsync(int doctorId);
        Task<Account?> UpdateDoctorAsync(Doctor doctor);


        //----------------------DoctorTimeSlot---------------------------
        Task<List<DoctorTimeSlot>> GetDoctorTimeSlotsForDayAsync(int doctorId, DateOnly date);
        Task<DoctorTimeSlot> GetDoctorTimeSlotByIDAsync(int doctorTimeSlotId);
        Task<DoctorTimeSlot> GetSpecificDoctorTimeSlotAsync(int doctorId, DateOnly date, int slotNumber);
        Task AddTimeSlotForDoctorAsync(DoctorTimeSlot doctorSlot);
        Task<DoctorTimeSlot> UpdateDoctorTimeSlotAsync(DoctorTimeSlot doctorSlot);
        Task DeleteDoctorTimeSlotsAsync(List<DoctorTimeSlot> doctorSchedule);
        Task DeleteDoctorTimeSlotByDoctorIDAsync(int doctorId);
        
    }
}
