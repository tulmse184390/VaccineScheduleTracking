﻿using AutoMapper;
using System.Numerics;
using static VaccineScheduleTracking.API_Test.Helpers.TimeSlotHelper;
using static VaccineScheduleTracking.API_Test.Helpers.ValidationHelper;
using VaccineScheduleTracking.API.Models.Entities;
using VaccineScheduleTracking.API_Test.Models.Entities;
using VaccineScheduleTracking.API_Test.Repository;
using VaccineScheduleTracking.API_Test.Repository.DailyTimeSlots;

namespace VaccineScheduleTracking.API.Services
{
    public class DoctorServices : IDoctorServices
    {
        private readonly IDailyScheduleRepository _dailyScheduleRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IMapper _mapper;

        public DoctorServices(IDailyScheduleRepository dailyScheduleRepository, IDoctorRepository doctorRepository, IMapper mapper)
        {
            _dailyScheduleRepository = dailyScheduleRepository;
            _doctorRepository = doctorRepository;
            _mapper = mapper;
        }

        public async Task<List<Doctor>> GetAllDoctorAsync()
        {
            return await _doctorRepository.GetAllDoctorAsync();
        }

        public async Task<DoctorTimeSlot> FindDoctorTimeSlotAsync(int DoctorID, DateOnly Date, int SlotNumber)
        {
            ValidateInput(DoctorID, "ID của bác sĩ không thể để trống");
            ValidateInput(Date, "Ngày không thể để trống");
            ValidateInput(SlotNumber, "Slot không thể để trống");
            return await _doctorRepository.GetSpecificDoctorTimeSlotAsync(DoctorID, Date, SlotNumber);
        }

        public async Task<Doctor> GetDoctorByIDAsync(int doctorID)
        {
            ValidateInput(doctorID, "ID của bác sĩ không thể để trống");
            return await _doctorRepository.GetDoctorByIDAsync(doctorID);
        }

        public async Task<List<Doctor>> GetDoctorByTimeSlotAsync(int slotNumber, DateOnly date)
        {
            ValidateInput(slotNumber, "Slot không thể để trống");
            ValidateInput(date, "Ngày không thể để trống");
            var doctorList = await _doctorRepository.GetAllDoctorAsync();
            List<Doctor> result = new List<Doctor>();
            foreach (var doctor in doctorList)
            {
                var doctorSlots = await _doctorRepository.GetDoctorTimeSlotsForDayAsync(doctor.DoctorID, date);
                if (doctorSlots.Any(ts => ts.SlotNumber == slotNumber && ts.Available))
                {
                    result.Add(doctor);
                }
            }
            return result;
        }

        public async Task<DoctorTimeSlot> SetDoctorTimeSlotAsync(DoctorTimeSlot docTimeSlot, bool status)
        {
            
            if (docTimeSlot.Available == false && status == false)
            {
                throw new Exception($"không còn slot cho bác sĩ {docTimeSlot.DoctorID} vào ngày {docTimeSlot.DailySchedule.AppointmentDate} slot {docTimeSlot.SlotNumber}");
            }
            else if (docTimeSlot.Available != status)
            {
                docTimeSlot.Available = status;
            }
            else
            {
                throw new Exception($"Slot {docTimeSlot.SlotNumber} của bác sĩ {docTimeSlot.DoctorID} vào ngày {docTimeSlot.DailySchedule.AppointmentDate} đã được cập nhật trước đó");
            }
            return await UpdateDoctorScheduleAsync(docTimeSlot);

        }




        /// <summary>
        /// kiểm tra xem các slot đã được tạo chưa
        /// </summary>
        /// <param name="doctor"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public async Task<bool> ExistingDoctorScheduleAsync(Doctor doctor, DailySchedule day)
        {
            var existingSlots = await _doctorRepository.GetDoctorTimeSlotsForDayAsync(doctor.DoctorID, day.AppointmentDate);
            return existingSlots.Any();
        }



        /// <summary>
        /// tìm bác sĩ phù hợp dựa trên slot và ngày là việc 
        /// </summary>
        /// <param name="slotNumber"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public async Task<Doctor?> GetSutableDoctorAsync(int slotNumber, DateOnly date)
        {
            var doctorList = await _doctorRepository.GetAllDoctorAsync();
            Doctor? suitableDoctor = null;
            int minSlotsOccupied = int.MaxValue;

            foreach (var doctor in doctorList)
            {
                var doctorSlots = await _doctorRepository.GetDoctorTimeSlotsForDayAsync(doctor.DoctorID, date);
                var availableSlot = doctorSlots.FirstOrDefault(ts => ts.SlotNumber == slotNumber && ts.Available);

                if (availableSlot != null)
                {
                    int slotsOccupied = doctorSlots.Count(ts => !ts.Available);
                    if (slotsOccupied < minSlotsOccupied)
                    {
                        minSlotsOccupied = slotsOccupied;
                        suitableDoctor = doctor;
                    }
                }
            }
            return suitableDoctor;
        }


        public async Task<DoctorTimeSlot> UpdateDoctorScheduleAsync(DoctorTimeSlot doctorSlot)
        {
            var slot = await _doctorRepository.GetDoctorTimeSlotByIDAsync(doctorSlot.DoctorTimeSlotID);
            if (slot == null)
            {
                throw new Exception($"không tìm thấy timeSlot có ID {doctorSlot.DoctorTimeSlotID}");
            }

            slot.DoctorID = NullValidator(doctorSlot.DoctorID)
               ? doctorSlot.DoctorID
               : slot.DoctorID;
            slot.SlotNumber = NullValidator(doctorSlot.SlotNumber)
                ? doctorSlot.SlotNumber
                : slot.SlotNumber;
            slot.Available = NullValidator(doctorSlot.Available)
                ? doctorSlot.Available
                : slot.Available;
            slot.DailyScheduleID = NullValidator(doctorSlot.DailyScheduleID)
                ? doctorSlot.DailyScheduleID
                : slot.DailyScheduleID;

           
            return await _doctorRepository.UpdateDoctorTimeSlotAsync(slot);
        }


        //----------------------------- Các hàm tự tạo ------------------------------

        /// <summary>
        /// loop qua các ngày
        /// </summary>
        /// <param name="doctor"></param>
        /// <param name="numberOfDays"></param>
        /// <returns></returns>
        public async Task GenerateDoctorCalanderAsync(List<Doctor> doctorList, int numberOfDays)
        {
            var dailyScheduleList = await _dailyScheduleRepository.GetAllDailyScheduleAsync();
            foreach (var doctor in doctorList)
            {
                foreach (var day in dailyScheduleList)
                {
                    await GenerateDoctorScheduleAsync(doctor, day);
                }
            }
        }


        /// <summary>
        /// tạo TimeSlot cho bác sĩ
        /// </summary>
        /// <param name="doctor"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public async Task GenerateDoctorScheduleAsync(Doctor doctor, DailySchedule day)
        {
            if (await ExistingDoctorScheduleAsync(doctor, day))
            {
                return;
            }
            int[] Slots = doctor.DoctorTimeSlots.Split(",").Select(int.Parse).ToArray();
            foreach (int item in Slots)
            {
                var slot = new DoctorTimeSlot
                {
                    DoctorID = doctor.DoctorID,
                    SlotNumber = item,
                    Available = true,
                    DailyScheduleID = day.DailyScheduleID
                };
                await _doctorRepository.AddTimeSlotForDoctorAsync(slot);
            }
        }


        /// <summary>
        /// lhàm này sẽ kiểm tra xem các slot đã quá hạn chưa
        /// </summary>
        /// <returns></returns>
        public async Task SetOverdueDoctorScheduleAsync()
        {
            var doctorList = await _doctorRepository.GetAllDoctorAsync();
            var dailySchedules = await _dailyScheduleRepository.GetAllDailyScheduleAsync();
            var today = DateOnly.FromDateTime(DateTime.Now);
            var now = DateTime.Now;

            foreach (var doctor in doctorList)
            {
                foreach (var date in dailySchedules)
                {
                    if (date.AppointmentDate < today)
                    {
                        var timeSlots = await _doctorRepository.GetDoctorTimeSlotsForDayAsync(doctor.DoctorID, date.AppointmentDate);
                        foreach (var slot in timeSlots)
                        {
                            if (slot.Available)
                            {
                                slot.Available = false;
                                await _doctorRepository.UpdateDoctorTimeSlotAsync(slot);
                            }
                        }
                    }
                    else if (date.AppointmentDate == today)
                    {
                        var timeSlots = await _doctorRepository.GetDoctorTimeSlotsForDayAsync(doctor.DoctorID, date.AppointmentDate);
                        foreach (var slot in timeSlots)
                        {
                            var startTime = CalculateStartTime(slot.SlotNumber);
                            var slotDateTime = date.AppointmentDate.ToDateTime(startTime);
                            if (slotDateTime < now && slot.Available)
                            {
                                slot.Available = false;
                                await _doctorRepository.UpdateDoctorTimeSlotAsync(slot);
                            }
                        }
                    }
                }
            }
        }

       
    }
}
