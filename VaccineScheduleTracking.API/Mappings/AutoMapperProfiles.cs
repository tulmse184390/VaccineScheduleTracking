﻿using AutoMapper;
using VaccineScheduleTracking.API.Models.Entities;
using VaccineScheduleTracking.API_Test.Models.DTOs;
using VaccineScheduleTracking.API_Test.Models.DTOs.Accounts;
using VaccineScheduleTracking.API_Test.Models.DTOs.Appointments;
using VaccineScheduleTracking.API_Test.Models.DTOs.Children;
using VaccineScheduleTracking.API_Test.Models.DTOs.Doctors;
using VaccineScheduleTracking.API_Test.Models.DTOs.Vaccines;
using VaccineScheduleTracking.API_Test.Models.Entities;

namespace VaccineScheduleTracking.API.Mappings
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Account, BlankAccountDto>();
            CreateMap<Appointment, AppointmentDto>()
                .ForMember(dest => dest.AppointmentID, opt => opt.MapFrom(src => src.AppointmentID))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.TimeSlots.DailySchedule.AppointmentDate))
                .ForMember(dest => dest.SlotNumber, opt => opt.MapFrom(src => src.TimeSlots.SlotNumber))
                .ForMember(dest => dest.Doctor, opt => opt.MapFrom(src => src.Account));

            CreateMap<Account, AppDoctorDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => $"{src.Firstname} {src.Lastname}"))
                .ForMember(dest => dest.DoctorID, opt => opt.MapFrom(src => src.Doctor.DoctorID));
            CreateMap<Child, AppChildDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => $"{src.Firstname} {src.Lastname}"));
            CreateMap<Vaccine, AppVaccineDto>()
                .ForMember(dest => dest.VaccineType, opt => opt.MapFrom(src => src.VaccineType));

            //--------------
            CreateMap<ModifyAppointmentDto, Appointment>().ReverseMap()
                .ForMember(dest => dest.SlotNumber, opt => opt.MapFrom(src => src.TimeSlots.SlotNumber))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.TimeSlots.DailySchedule.AppointmentDate));
            CreateMap<UpdateAppointmentDto, Appointment>().ReverseMap()
                .ForMember(dest => dest.SlotNumber, opt => opt.MapFrom(src => src.TimeSlots.SlotNumber));

            CreateMap<Appointment, CreateAppointmentDto>().ReverseMap();

            CreateMap<Account, DoctorAccountDto>().ReverseMap();
            CreateMap<Account, ManagerAccountDto>().ReverseMap();
            CreateMap<Account, StaffAccountDto>().ReverseMap();
            CreateMap<Doctor, DoctorDto>()
                .ForMember(dest => dest.DoctorId, opt => opt.MapFrom(src => src.DoctorID))
                .ForMember(dest => dest.DoctorTimeSlot, opt => opt.MapFrom(src => src.DoctorTimeSlots))
                .ReverseMap();
                
            CreateMap<ChildTimeSlot, ChildTimeSlotDto>();

            CreateMap<Account, AccountDto>().ReverseMap();
            CreateMap<Account, RegisterBlankAccountDto>().ReverseMap();
            CreateMap<Account, RegisterAccountDto>().ReverseMap();
            CreateMap<Account, UpdateAccountDto>().ReverseMap();
            CreateMap<Account, DeleteAccountDto>().ReverseMap();
            CreateMap<Vaccine, VaccineDto>().ReverseMap();
            CreateMap<VaccineType, FilterVaccineTypeDto>().ReverseMap();
            CreateMap<VaccineType, AddVaccineTypeDto>().ReverseMap();
            CreateMap<Child, ChildDto>().ReverseMap();
            CreateMap<Child, AddChildDto>().ReverseMap();
            CreateMap<Child, UpdateChildDto>().ReverseMap();
            CreateMap<VaccineRecord, VaccineRecordDto>().ReverseMap();
            CreateMap<VaccineCombo, VaccineComboDto>().ReverseMap();
            CreateMap<VaccineContainer, VaccineContainerDto>().ReverseMap();
        }
    }
}
