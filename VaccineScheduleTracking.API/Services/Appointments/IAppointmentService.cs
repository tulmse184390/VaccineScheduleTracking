﻿using VaccineScheduleTracking.API.Models.Entities;
using VaccineScheduleTracking.API_Test.Models.DTOs.Appointments;
using VaccineScheduleTracking.API_Test.Models.Entities;

namespace VaccineScheduleTracking.API_Test.Services.Appointments
{
    public interface IAppointmentService
    {
        Task<Appointment?> GetAppointmentByIDAsync(int appointmentID);
        Task<Appointment?> CreateAppointmentAsync(CreateAppointmentDto appointment);
        Task<List<Appointment>> GetDoctorAppointmentsAsync(int doctorId);
        Task<List<Appointment>> GetChildAppointmentsAsync(int childId);
        Task<List<Appointment>> GetPendingDoctorAppointmentAsync(int doctorId);
        Task<List<Appointment>> GetPendingAppointments(int beforeDueDate);
        Task<Appointment?> UpdateAppointmentAsync(int appointmenID, UpdateAppointmentDto appointment);
        Task<Appointment?> SetAppointmentStatusAsync(int appointmentId, string status, string? note);
        Task AddAppointmentToRecord(Appointment appointment, string? note);
        Task<Appointment?> CancelAppointmentAsync(int appointmentId, string reason);
        Task<CancelAppointment> GetCancelAppointmentReasonAsync(int appointmentId);
        Task SetOverdueAppointmentAsync(int threshold);
        
    }
}
