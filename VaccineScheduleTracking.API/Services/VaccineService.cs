﻿using AutoMapper;
using VaccineScheduleTracking.API.Models.DTOs;
using VaccineScheduleTracking.API.Models.Entities;
using VaccineScheduleTracking.API.Repository;

namespace VaccineScheduleTracking.API.Services
{
    public class VaccineService : IVaccineService
    {
        private readonly IVaccineRepository vaccineRepository;
        private readonly IMapper mapper;

        public VaccineService(IVaccineRepository vaccineRepository, IMapper mapper)
        {
            this.vaccineRepository = vaccineRepository;
            this.mapper = mapper;
        }

        // Vaccine funtion
        public async Task<List<Vaccine>> GetVaccinesAsync(FilterVaccineDto filterVaccineDto)
        {
            return await vaccineRepository.GetVaccinesAsync(filterVaccineDto);
        }

        public async Task<Vaccine?> CreateVaccineAsync(AddVaccineDto addVaccineDto)
        {
            var vaccine = await vaccineRepository.GetVaccineByNameAsync(addVaccineDto.Name);
            if (vaccine != null)
            {
                throw new Exception($"{addVaccineDto.Name} is exist!");
            }
            var vaccineType = await vaccineRepository.GetVaccineTypeByNameAsync(addVaccineDto.VaccineType);
            if (vaccineType == null)
            {
                throw new Exception($"{addVaccineDto.VaccineType} is invalid!");
            }
            if (vaccine.FromAge >= vaccine.ToAge)
            {
                throw new Exception("Invalid age limit!");
            }
            if (vaccine.Period == 0)
            {
                throw new Exception("The period is invalid!");
            }

            vaccine = new Vaccine
            {
                Name = addVaccineDto.Name,
                VaccineTypeID = vaccineType.VaccineTypeID,
                Manufacturer = addVaccineDto.Manufacturer,
                Stock = addVaccineDto.Stock,
                Price = addVaccineDto.Price,
                Description = addVaccineDto.Description,
                FromAge = addVaccineDto.FromAge,
                ToAge = addVaccineDto.ToAge,
                Period = addVaccineDto.Period,
                VaccineType = vaccineType
            };
            await vaccineRepository.AddVaccineAsync(vaccine);
            return vaccine;
        }

        // VaccineType function
        public async Task<VaccineType?> CreateVaccineTypeAsync(AddVaccineTypeDto addVaccineTypeDto)
        {
            var vaccineType = await vaccineRepository.GetVaccineTypeByNameAsync(addVaccineTypeDto.Name);

            if (vaccineType != null)
            {
                return null;
            }

            vaccineType = await vaccineRepository.AddVaccineTypeAsync(mapper.Map<VaccineType>(addVaccineTypeDto));

            return vaccineType;
        }
    }
}
