﻿using AutoMapper;
using static VaccineScheduleTracking.API_Test.Helpers.ValidationHelper;
using VaccineScheduleTracking.API.Models.Entities;
using VaccineScheduleTracking.API_Test.Models.DTOs.Appointments;
using VaccineScheduleTracking.API_Test.Repository.Appointments;
using VaccineScheduleTracking.API_Test.Repository.DailyTimeSlots;
using VaccineScheduleTracking.API_Test.Repository.Vaccines;
using VaccineScheduleTracking.API_Test.Services.Children;
using VaccineScheduleTracking.API_Test.Services.DailyTimeSlots;
using VaccineScheduleTracking.API_Test.Services.Vaccines;
using VaccineScheduleTracking.API_Test.Services.Doctors;
using Microsoft.IdentityModel.Tokens;
using VaccineScheduleTracking.API_Test.Services.Record;
using VaccineScheduleTracking.API_Test.Models.DTOs;
using VaccineScheduleTracking.API_Test.Helpers;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;
using VaccineScheduleTracking.API_Test.Models.Entities;
using Microsoft.VisualBasic;
using System.Globalization;
using System.Collections.Generic;

namespace VaccineScheduleTracking.API_Test.Services.Appointments
{
    public class AppointmentService : IAppointmentService
    {
        private readonly TimeSlotHelper _timeSlotHelper;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IVaccineRepository _vaccineRepository;
        private readonly IDailyScheduleRepository _dailyScheduleRepository;

        private readonly IVaccineRecordService _vaccineRecordService;
        private readonly IChildService _childServices;
        private readonly IDoctorServices _doctorServices;
        private readonly ITimeSlotServices _timeSlotServices;
        private readonly IVaccineService _vaccineServices;

        private readonly IMapper _mapper;

        public AppointmentService(
            TimeSlotHelper timeSlotHelper,
            IAppointmentRepository appointmentRepository,
            IVaccineRepository vaccineRepository,
            IDailyScheduleRepository dailyScheduleRepository,

            IVaccineRecordService vaccineRecordService,
            IChildService childServices,
            IDoctorServices doctorServices,
            ITimeSlotServices timeSlotServices,
            IVaccineService vaccineServices,

            IMapper mapper)
        {
            _timeSlotHelper = timeSlotHelper;
            _appointmentRepository = appointmentRepository;
            _dailyScheduleRepository = dailyScheduleRepository;
            _vaccineRepository = vaccineRepository;

            _vaccineRecordService = vaccineRecordService;
            _childServices = childServices;
            _doctorServices = doctorServices;
            _timeSlotServices = timeSlotServices;
            _vaccineServices = vaccineServices;

            _mapper = mapper;
        }

        public async Task<Appointment?> GetAppointmentByIDAsync(int appointmentID)
        {
            ValidateInput(appointmentID, "chưa nhập ID");
            return await _appointmentRepository.GetAppointmentByIDAsync(appointmentID);
        }


        public async Task<List<Appointment>> GetAppointmentByDateAsync(int childId, DateOnly date)
        {
            ValidateInput(childId, "chưa nhập child ID");
            ValidateInput(date, "chưa nhập ngày");

            return await _appointmentRepository.GetAppointmentByDateAsync(childId, date);
        }

        /// <summary>
        /// giới hạn số lượng appointment cho 1 ngày
        /// </summary>
        /// <param name="childId"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> LimitAmount(int childId, DateOnly date)
        {
            var appointments = await GetAppointmentByDateAsync(childId, date);
            var validAppointments = appointments
                .Where(a => a.Status == "FINISHED" || a.Status == "PENDING")
                .ToList();
            return (validAppointments.Count >= 5);

        }


        public async Task<List<Appointment>> GetPendingDoctorAppointmentAsync(int doctorId)
        {
            return await _appointmentRepository.GetPendingDoctorAppointmentAsync(doctorId);
        }


        /// <summary>
        /// tìm ngày tiêm vaccine x gần nhất 
        /// </summary>  
        /// <param name="childId"></param>
        /// <param name="vaccineId"></param>
        /// <param name="appointmentDate"></param>
        /// <returns></returns>
        public async Task<DateOnly?> GetLatestVaccineDate(int childId, int vaccineId)
        {
            var appointments = await GetChildAppointmentsAsync(childId);
            var vacHistory = await _vaccineRecordService.GetRecordsAsync(childId);

            var latestDates = new Dictionary<int, DateOnly>();

            foreach (var app in appointments)
            {
                if (app.Status == "FINISHED" || app.Status == "PENDING" || app.Status == "CONFIRMED")
                {
                    if (app.VaccineID != null && app.VaccineID == vaccineId)
                    {
                        var appointmentDate = app.TimeSlots.DailySchedule.AppointmentDate;
                        if (!latestDates.TryGetValue(app.VaccineID, out var latest) || appointmentDate > latest)
                        {
                            latestDates[app.VaccineID] = appointmentDate;
                        }
                    }
                }
            }
            foreach (var record in vacHistory)
            {
                if (record.VaccineID != null && record.VaccineID == vaccineId)
                {
                    var recordDate = record.Date;
                    if (!latestDates.TryGetValue(record.VaccineID.Value, out var latest) || recordDate > latest)
                    {
                        latestDates[record.VaccineID.Value] = recordDate;
                    }
                }
            }
            return latestDates.TryGetValue(vaccineId, out var latestDate) ? latestDate : null;
        }






        /// <summary>
        /// giới hạn thời gian cùng loại vaccine
        /// </summary>
        /// <param name="vaccineId"></param>
        /// <param name="childId"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public async Task<string> LimitVaccinePeriod(int vaccineId, int childId, DateOnly date)
        {
            string message = null ;
            var vaccine = await _vaccineServices.GetVaccineByIDAsync(vaccineId);
            DateOnly? latestDate = await GetLatestVaccineDate(childId, vaccineId);

            if (latestDate == null) return null;

            DateOnly limitDate = _timeSlotHelper.GetPeriodDate(vaccine.Period, latestDate.Value);

            if (date <= limitDate)
            {
                message = ($"Vaccine này đã được dùng ngày {latestDate:dd/MM/yyyy}, để đảm bảo an toàn vaccine loại này sẽ khả dụng sau ngày {limitDate:dd/MM/yyyy}");
            }
            return message;
        }



        public async Task<List<string>> ValidateVaccineConditions(int vaccineId, int childId, DateOnly date)
        {
            var error = new List<string>() { };
            if (await LimitAmount(childId, date))
            {
                error.Add("Không thể đặt quá 5 lịch hẹn 1 ngày");
            }
            var message = await LimitVaccinePeriod(vaccineId, childId, date);
            if (!string.IsNullOrEmpty(message))
            {
                error.Add(message);
            }
            return error;
        }



        /// <summary>
        /// Chuyển trạng thái Appointment - đã xong
        /// </summary>
        /// <param name="appointmentId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Appointment?> SetAppointmentStatusAsync(int? appointmentId, string status, string? note)
        {
            if (appointmentId == null)
                return null;
            //ValidateInput(appointmentId, "Id buổi hẹn không thể để trống");
            var appointment = await _appointmentRepository.GetAppointmentByIDAsync((int)appointmentId);
            if (appointment == null)
                throw new Exception("không tìm thấy buổi hẹn");
            else if (appointment.Status == "OVERDUE")
                throw new Exception(" buổi hẹn đã quá hạn");
            else if (appointment.Status == "CANCELED")
                throw new Exception(" buổi hẹn đã bị hủy");
            var Time = $"{appointment.TimeSlots.DailySchedule.AppointmentDate} - {appointment.TimeSlots.StartTime}";
            string childName = $"{appointment.Child.Lastname} {appointment.Child.Firstname}";

            if (appointment.Status == "CONFIRMED" && status == "CONFIRMED")
                throw new Exception($"buổi hẹn {Time} cho bé {childName} đã được thanh toán trước đó");

            if (appointment.Status == "PENDING" && status == "FINISHED")
                throw new Exception($"buổi hẹn {Time} cho bé {childName} chưa được thanh toán!!");

            appointment.Status = status;
            if (status == "FINISHED")
            {
                await AddAppointmentToRecord(appointment, note);    
            }
            return await _appointmentRepository.UpdateAppointmentAsync(appointment);
        }


        public async Task AddAppointmentToRecord(Appointment appointment, string? note)
        {
            ValidateInput(appointment, "appointment đưa vào chưa đầy đủ thông tin");
            var record = new CreateVaccineRecordDto
            {
                ChildID = appointment.ChildID,
                AppointmentID = appointment.AppointmentID,
                VaccineTypeID = appointment.Vaccine.VaccineTypeID,
                VaccineID = appointment.VaccineID,
                Date = appointment.TimeSlots.DailySchedule.AppointmentDate,
                Note = note ?? "không phản ứng phụ được ghi nhận"
            };

            var rec = await _vaccineRecordService.AddVaccineRecordAsync(record).ConfigureAwait(false);
            if (rec is null)
            {
                throw new InvalidOperationException($"Không thể tạo vaccineRecord cho Appointment {appointment.AppointmentID}");
            }

        }



        /// <summary>
        /// hủy Appointment - đã xong
        /// </summary>
        /// <param name="appointmentId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Appointment?> CancelAppointmentAsync(int appointmentId, string reason)
        {
            ValidateInput(appointmentId, "Id buổi hẹn không thể để trống");
            var appointment = await _appointmentRepository.GetAppointmentByIDAsync(appointmentId);

            DateOnly date = appointment.TimeSlots.DailySchedule.AppointmentDate;
            int slotNumber = appointment.TimeSlots.SlotNumber;
            int doctorID = appointment.Account.Doctor.DoctorID;
            int childID = appointment.ChildID;
            string doctorName = $"{appointment.Account.Lastname} {appointment.Account.Firstname}";

            if (appointment == null)
            {
                throw new Exception($"không tìm thấy buổi hẹn có ID: {appointmentId}");
            }
            if (appointment.Status.ToUpper() == "OVERDUE")
            {
                throw new Exception($"buổi hẹn ngày {date} slot {slotNumber} đã quá hạn");
            }
            if (appointment.Status == "CONFIRMED")
                throw new Exception("không thể hủy lịch hẹn đã được thanh toán");

            if (date == DateOnly.FromDateTime(DateTime.Now))
            {
                throw new Exception("Lịch hẹn chỉ có thể hủy trước ngày tiêm 1 ngày");
            }

            var childTimeSlot = await _childServices.GetChildTimeSlotBySlotNumberAsync(childID, slotNumber, date);
            var doctorTimeSlot = await _doctorServices.FindDoctorTimeSlotAsync(doctorID, date, slotNumber);

            if (appointment.Status == "PENDING")
            {
                doctorTimeSlot.Available = true;
                childTimeSlot.Available = false;
                appointment.Vaccine.Stock += 1;
                appointment.Status = "CANCELED";
            }

            var cancelReason = new CancelAppointment()
            {
                AppointmentID = appointment.AppointmentID,
                CancelDate = DateTime.Now,
                Reason = reason
            };
            await _doctorServices.UpdateDoctorTimeSlotAsync(doctorTimeSlot);
            await _childServices.UpdateChildTimeSlotAsync(childTimeSlot);

            await _appointmentRepository.createCancelReasonAsync(cancelReason);

            return await _appointmentRepository.UpdateAppointmentAsync(appointment);
        }


        public async Task<List<string>> ValidateAppointmentConditions(int childID, int vaccineID, int slotNumber, DateOnly date)
        {
            var errors = new List<string>();

            _timeSlotHelper.LimitDate(date, "Hiện tại chỉ có thể đặt lịch trước ngày");

            var timeSlot = await _timeSlotServices.GetTimeSlotAsync(slotNumber, date);
            if (timeSlot == null || slotNumber < 1 || slotNumber > 20)
                errors.Add("Slot nhập vào không hợp lệ (1 - 20)");

            if (timeSlot != null && !timeSlot.Available)
                errors.Add("Slot này đã quá hạn.");

            // child
            var child = await _childServices.GetChildByIDAsync(childID);
            if (child == null)
                errors.Add($"Không tìm thấy trẻ có ID {childID}");
            else if (!child.Available)
                errors.Add($"Tài khoản của trẻ {child.Lastname} {child.Firstname} đã bị vô hiệu hóa");

            var childTimeSlot = await _childServices.GetChildTimeSlotBySlotNumberAsync(childID, slotNumber, date);
            if (childTimeSlot != null && childTimeSlot.Available)
                errors.Add("Slot này đã được đăng ký");

            // vaccine
            var vaccine = await _vaccineServices.GetVaccineByIDAsync(vaccineID);
            if (vaccine == null)
                errors.Add($"Không tìm thấy vaccine có ID: {vaccineID}");
            else if (vaccine.Stock <= 0)
                errors.Add($"Vaccine {vaccine.Name} đã hết hàng!");

            bool c = await LimitAmount(childID, date);
            if (c)
            {
                errors.Add("Không thể đăng ký quá 5 buổi hẹn trong 1 ngày.");
            }

            // doctor
            var doctorSlot = await _doctorServices.GetSuitableDoctorTimeSlotAsync(slotNumber, date);
            if (doctorSlot == null)
                errors.Add($"Đã hết bác sĩ ở slot {slotNumber}");

            return errors;
        }


        public async Task ModifyAppointmentResources(int childID, int slotNumber, DateOnly date, Vaccine vaccine, DoctorTimeSlot doctorSlot)
        {
            // childTimeSlot
            var childTimeSlot = await _childServices.GetChildTimeSlotBySlotNumberAsync(childID, slotNumber, date);
            if (childTimeSlot != null && childTimeSlot.Available)
            {
                childTimeSlot.Available = false;
                await _childServices.UpdateChildTimeSlotAsync(childTimeSlot);
            }
            else
            {
                await _childServices.CreateChildTimeSlot(slotNumber, date, childID);
            }

            // DoctorTimeSlot
            await _doctorServices.SetDoctorTimeSlotAsync(doctorSlot, false);

            // vaccine
            vaccine.Stock -= 1;
            await _vaccineRepository.UpdateVaccineAsync(vaccine);
        }



        /// <summary>
        /// hàm tạo lịch hẹn - đang sửa 
        /// </summary>
        /// <param name="createAppointment"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Result<Appointment>> CreateAppointmentAsync(CreateAppointmentDto createAppointment)
        {
            var result = new Result<Appointment>();

            DateOnly date = createAppointment.Date;
            int slotNumber = createAppointment.SlotNumber;
            int childID = createAppointment.ChildID;
            int vaccineID = createAppointment.VaccineID;

            var validateErr = await ValidateVaccineConditions(vaccineID, childID, date);

            var errors = await ValidateAppointmentConditions(childID, vaccineID, slotNumber, date);
            if (errors.Any() || validateErr.Any())
            {
                result.Errors.AddRange(errors);
                result.Errors.AddRange(validateErr);
                return result;
            }

            var timeSlot = await _timeSlotServices.GetTimeSlotAsync(slotNumber, date);
            var vaccine = await _vaccineServices.GetVaccineByIDAsync(vaccineID);
            var doctorSlot = await _doctorServices.GetSuitableDoctorTimeSlotAsync(slotNumber, date);
            var docAccount = await _doctorServices.GetDoctorByIDAsync(doctorSlot.DoctorID);

            var appointment = new Appointment
            {
                ChildID = childID,
                AccountID = docAccount.AccountID,
                VaccineID = vaccineID,
                TimeSlotID = timeSlot.TimeSlotID,
                Status = "PENDING"
            };

            await ModifyAppointmentResources(childID, slotNumber, date, vaccine, doctorSlot);

            result.Data = await _appointmentRepository.CreateAppointmentAsync(appointment);
            return result;
        }



        public class Result<T>
        {
            public T? Data { get; set; }
            public List<string> Errors { get; set; } = new();
            public bool IsSuccess => !Errors.Any();

            public string Message => Errors.Any() ? Errors.First() : string.Empty;
        }




        /// <summary>
        /// cập nhật lịch hẹn (Appointment)
        /// </summary>
        /// <param name="appointmentID"></param>
        /// <param name="modAppointment"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Appointment?> UpdateAppointmentAsync(int appointmentID, UpdateAppointmentDto modAppointment)// đã xong - chưa tối ưu
        {
            var appointment = await _appointmentRepository.GetAppointmentByIDAsync(appointmentID);
            int oldChildId = appointment.ChildID;
            int oldDoctorId = appointment.Account.Doctor.DoctorID;
            int oldVaccineId = appointment.VaccineID;
            int oldSlotNumber = appointment.TimeSlots.SlotNumber;
            DateOnly oldDate = appointment.TimeSlots.DailySchedule.AppointmentDate;

            var child = await _childServices.GetChildByIDAsync(oldChildId);
            if (child == null) throw new Exception($"Không tìm thấy trẻ có ID {oldChildId}");
            if (child.Available == false) throw new Exception($"tài khoản của trẻ {child.Lastname} {child.Firstname} đã bị vô hiệu hóa");

            ValidateInput(appointment, "Lịch hẹn không tồn tại!");
            _timeSlotHelper.LimitDate(modAppointment.Date, "TimeSlot chỉ được tạo đến trước ngày");

            if (appointment.TimeSlots.Available == false)
            {
                throw new Exception("lịch hẹn này đã quá hạn, vui lòng đăng kí lịch hẹn mới");
            }

            var tempAppoint = new UpdateAppointmentDto();
            int upChildId = tempAppoint.ChildID;
            int upDoctorId = tempAppoint.DoctorID;
            int upVaccineId = tempAppoint.VaccineID;
            int upSlotNumber = tempAppoint.SlotNumber;
            DateOnly upDate = tempAppoint.Date;

            upChildId = NullValidator(modAppointment.ChildID)
                ? modAppointment.ChildID
                : oldChildId;
            upDoctorId = NullValidator(modAppointment.DoctorID)
                ? modAppointment.DoctorID
                : oldDoctorId;
            upVaccineId = NullValidator(modAppointment.VaccineID)
                ? modAppointment.VaccineID
                : oldVaccineId;
            upSlotNumber = NullValidator(modAppointment.SlotNumber)
              ? modAppointment.SlotNumber
              : oldSlotNumber;
            upDate = NullValidator(modAppointment.Date)
                ? modAppointment.Date
                : oldDate;

            ///kiểm tra ngày hẹn
            if (_timeSlotHelper.CompareNowTime(oldDate) == -1)
            {
                throw new Exception("không thể đặt ngày đã qua hạn!");
            }
            ///giới hạn tiêm 
            var error = await ValidateVaccineConditions(upVaccineId, upChildId, upDate);
            if (error.Any())
                throw new Exception(error.First());
            ///kiểm tra slot
            var timeSlot = await _timeSlotServices.GetTimeSlotAsync(upSlotNumber, upDate);
            if (timeSlot.Available == false)
            {
                throw new Exception("không thể đặt slot đã quá hạn!");
            }

            ///kiểm DoctorSlot
            var doctorTimeSlot = await _doctorServices.FindDoctorTimeSlotAsync(upDoctorId, upDate, upSlotNumber);
            if (doctorTimeSlot == null || !doctorTimeSlot.Available)
            {
                var docAccount = await _doctorServices.GetDoctorByIDAsync(upDoctorId);
                throw new Exception($"Bác sĩ {docAccount.Firstname} không còn lịch trống vào ngày {upDate} slot {upSlotNumber} Chọn bác sĩ khác hoặc đổi thời gian hẹn lịch nhé <3");
            }

            ///kiểm tra slot này đã được đăng kí chưa
            var childTimeSlot = await _childServices.GetChildTimeSlotBySlotNumberAsync(upChildId, upSlotNumber, upDate);
            if (childTimeSlot != null && childTimeSlot.Available)
            {
                throw new Exception("Bạn đã đăng kí Slot này");
            }

            ///kiểm tra vaccine
            var vaccine = await _vaccineServices.GetVaccineByIDAsync(upVaccineId);
            if (vaccine.Stock == 0)
            {
                throw new Exception($"Loại vaccine {vaccine.Name} này đã hết");
            }

            //------------------các hàm cập nhật lại-------------------- 
            ///cập nhật đổi doctor
            var oldDoctorSlot = await _doctorServices.FindDoctorTimeSlotAsync(oldDoctorId, oldDate, oldSlotNumber);
            if (modAppointment.DoctorID != 0 && modAppointment.DoctorID != oldDoctorId)
            {
                if (oldDoctorSlot != null)
                {
                    oldDoctorSlot.Available = true;
                }
                doctorTimeSlot.Available = false;

                oldDoctorId = doctorTimeSlot.DoctorID;
            }
            ///cập nhật lại stock vaccine
            if (modAppointment.VaccineID != 0 && modAppointment.VaccineID != oldVaccineId)
            {
                var oldVaccine = await _vaccineServices.GetVaccineByIDAsync(oldVaccineId);
                if (oldVaccine != null)
                {
                    oldVaccine.Stock += 1;
                }
                vaccine.Stock -= 1;

                oldVaccineId = modAppointment.VaccineID;
            }

            ///cập nhật lại slot cho child + doctor - đang lỗi 
            if (modAppointment.SlotNumber != 0 && modAppointment.SlotNumber != oldSlotNumber
                || modAppointment.Date != default && modAppointment.Date != oldDate)
            {
                var oldChildSlot = await _childServices.GetChildTimeSlotBySlotNumberAsync(oldChildId, oldSlotNumber, oldDate);
                if (oldChildSlot != null)
                {
                    oldChildSlot.Available = false;
                }
                if (childTimeSlot != null && childTimeSlot.Available == false)
                {
                    childTimeSlot.Available = true;
                }
                else
                {
                    await _childServices.CreateChildTimeSlot(upSlotNumber, upDate, upChildId);
                }

                if (oldDoctorSlot != null)
                {
                    oldDoctorSlot.Available = true;
                }
                doctorTimeSlot.Available = false;

                appointment.VaccineID = upVaccineId;
                appointment.Account.Doctor.DoctorID = doctorTimeSlot.DoctorID;
                appointment.TimeSlotID = timeSlot.TimeSlotID;
            }

            return await _appointmentRepository.UpdateAppointmentAsync(appointment);
        }




        /// <summary>
        /// hàm lấy danh sách lịch hẹn theo của Child
        /// </summary>
        /// <param name="id"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<Appointment>> GetChildAppointmentsAsync(int childId)
        {
            var child = await _childServices.GetChildByIDAsync(childId);
            var appointmentList = await _appointmentRepository.GetAppointmentsByChildIDAsync(childId);
            //if (appointmentList.IsNullOrEmpty())
            //{
            //    throw new Exception($"không tìm thấy buổi hẹn nào cho bé {child.Lastname} {child.Firstname}");
            //}

            return appointmentList;
        }


        /// <summary>
        /// hàm lấy danh sách lịch hẹn theo doctor
        /// </summary>
        /// <param name="doctorId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<Appointment>> GetDoctorAppointmentsAsync(int doctorId)
        {
            var docAccount = await _doctorServices.GetDoctorByIDAsync(doctorId);
            var doctorAppointments = await _appointmentRepository.GetAppointmentsByDoctorIDAsync(doctorId);
            if (doctorAppointments.IsNullOrEmpty())
            {
                return null;
            }
            return doctorAppointments;
        }

        //public async Task DeleteOverDueAppointmentAsync(List<Appointment> appointments)
        //{
        //    if (appointments.Any())
        //    {
        //        await _appointmentRepository.DeleteOverDueAppointmentAsync(appointments);
        //    }
        //}

        /// <summary>
        /// tự động chuyển các apointment thành OVERDUE khi bị quá hạn
        /// </summary>
        /// <returns></returns>
        public async Task SetOverdueAppointmentAsync(int threshold)// đã xong 
        {
            var dailySchedules = await _dailyScheduleRepository.GetAllDailyScheduleAsync();
            var allAppointments = await _appointmentRepository.GetAllAppointmentsAsync();
            var today = DateOnly.FromDateTime(DateTime.Today);
            var thresholdDate = today.AddDays(-threshold);
            var overdueAppointments = new List<Appointment>();

            foreach (var date in dailySchedules)
            {
                int dateStatus = _timeSlotHelper.CompareNowTime(date.AppointmentDate);
                var filteredAppointments = allAppointments.Where(a => a.TimeSlots.DailySchedule.AppointmentDate == date.AppointmentDate).ToList();

                //if (date.AppointmentDate < thresholdDate)
                //{
                //    overdueAppointments.AddRange(filteredAppointments);
                //}
                if (dateStatus == -1)
                {
                    foreach (var appointment in filteredAppointments)
                    {
                        if (appointment.Status == "PENDING")
                        {
                            appointment.Status = "OVERDUE";
                            await _appointmentRepository.UpdateAppointmentAsync(appointment);
                        }
                    }
                }
                else if (dateStatus == 0)
                {
                    foreach (var appointment in filteredAppointments)
                    {
                        if (_timeSlotHelper.CompareNowTime(appointment.TimeSlots.StartTime) == -1 && appointment.Status == "PENDING")
                        {
                            appointment.Status = "OVERDUE";
                            await _appointmentRepository.UpdateAppointmentAsync(appointment);
                        }
                    }
                }
            }
            //await DeleteOverDueAppointmentAsync(overdueAppointments);
        }


        public async Task<CancelAppointment> GetCancelAppointmentReasonAsync(int appointmentId)
        {
            return await _appointmentRepository.GetCancelAppointmentReasonAsync(appointmentId);
        }

        public async Task<List<Appointment>> GetPendingAppointments(int beforeDueDate)
        {
            var dueDate = _timeSlotHelper.CalculateDate(beforeDueDate);

            return await _appointmentRepository.GetPendingAppointments(dueDate);
        }


        public async Task<Appointment> FindAppointment(CreateAppointmentDto appointmentDto)
        {
            var appointment = await _appointmentRepository.FindAppointment(appointmentDto);
            if (appointment == null)
            {
                return null;
            }
            return appointment;
        }

    }
}
