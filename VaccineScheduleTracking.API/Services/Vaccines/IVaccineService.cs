﻿using VaccineScheduleTracking.API.Models.Entities;
using VaccineScheduleTracking.API_Test.Models.DTOs.Vaccines;

namespace VaccineScheduleTracking.API_Test.Services.Vaccines
{
    public interface IVaccineService
    {
        // Vaccine funtion
        Task<Vaccine?> GetVaccineByIDAsync(int id);
        Task<List<Vaccine>> GetSutableVaccineAsync(int Age, int TypeID);
        Task<List<Vaccine>> GetVaccinesAsync(FilterVaccineDto filterVaccineDto);
        Task<Vaccine?> CreateVaccineAsync(AddVaccineDto addVaccineDto);
        Task<Vaccine?> UpdateVaccineAsync(int id, UpdateVaccineDto updateVaccineDto);
        Task<Vaccine?> DeleteVaccineAsync(int id);

        // VaccineType function
        Task<List<VaccineType>> GetAllVaccineTypeAsync();
        Task<VaccineType?> CreateVaccineTypeAsync(AddVaccineTypeDto addVaccineTypeDto);
        Task<VaccineType?> UpdateVaccineTypeAsync(int id, UpdateVaccineTypeDto updateVaccineTypeDto);
        Task<VaccineType?> DeleteVaccineTypeAsync(int id);

    }
}

