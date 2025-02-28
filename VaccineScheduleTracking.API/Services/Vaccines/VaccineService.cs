﻿using AutoMapper;
using VaccineScheduleTracking.API.Models.Entities;
using static VaccineScheduleTracking.API_Test.Helpers.ValidationHelper;
using System.Reflection.PortableExecutable;
using VaccineScheduleTracking.API_Test.Models.DTOs.Vaccines;
using VaccineScheduleTracking.API_Test.Repository.Vaccines;


namespace VaccineScheduleTracking.API_Test.Services.Vaccines
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
        public async Task<Vaccine?> GetVaccineByIDAsync(int id)
        {
            ValidateInput(id, "ID vaccine không được để trống");
            return await vaccineRepository.GetVaccineByIDAsync(id);
        }
        public async Task<List<Vaccine>> GetVaccinesAsync(FilterVaccineDto filterVaccineDto)
        {
            return await vaccineRepository.GetVaccinesAsync(filterVaccineDto);
        }

        public async Task<Vaccine?> CreateVaccineAsync(AddVaccineDto addVaccineDto)
        {
            var vaccine = await vaccineRepository.GetVaccineByNameAsync(addVaccineDto.Name);
            if (vaccine != null)
            {
                throw new Exception($"{addVaccineDto.Name} đã tồn tại ");
            }
            var vaccineType = await vaccineRepository.GetVaccineTypeByNameAsync(addVaccineDto.VaccineType);
            if (vaccineType == null)
            {
                throw new Exception($"{addVaccineDto.VaccineType} không khả dụng");
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

        /// <summary>
        /// đưa vào id và vaccine để cập nhật lại
        /// LƯU Í: với các field để trống sẽ lấy lại giá trị cũ
        /// </summary>
        /// <param name="id"> ID vaccine muốn sửa </param>
        /// <param name="updateVaccine"> Object chứa chi tiết những field sửa </param>
        /// <returns></returns>
        public async Task<Vaccine?> UpdateVaccineAsync(int id, UpdateVaccineDto updateVaccine)
        {
            var vaccine = await vaccineRepository.GetVaccineByIDAsync(id);
            if (vaccine == null)
            {
                throw new Exception($"Can't find vaccine with ID {id}");
            }
            vaccine.Name = NullValidator(updateVaccine.Name)
                ? updateVaccine.Name
                : vaccine.Name;
            //vaccine.VaccineTypeID = updateVaccine.VaccineTypeID ?? vaccine.VaccineTypeID;
            vaccine.Manufacturer = NullValidator(updateVaccine.Manufacturer)
                ? updateVaccine.Manufacturer
                : vaccine.Manufacturer;
            vaccine.Stock = NullValidator(updateVaccine.Stock)
                ? updateVaccine.Stock
                : vaccine.Stock;
            vaccine.Price = NullValidator(updateVaccine.Price)
                ? updateVaccine.Price
                : vaccine.Price;
            vaccine.Description = NullValidator(updateVaccine.Description)
                ? updateVaccine.Description
                : vaccine.Description;
            vaccine.FromAge = NullValidator(updateVaccine.FromAge)
                ? updateVaccine.FromAge
                : vaccine.FromAge;
            vaccine.ToAge = NullValidator(updateVaccine.ToAge)
                ? updateVaccine.ToAge
                : vaccine.ToAge;
            vaccine.Period = NullValidator(updateVaccine.Period)
                ? updateVaccine.Period
                : vaccine.Period;
            vaccine.DosesRequired = NullValidator(updateVaccine.DosesRequired)
                ? updateVaccine.DosesRequired
                : vaccine.DosesRequired;
            vaccine.Priority = NullValidator(updateVaccine.Priority)
                ? updateVaccine.Priority
                : vaccine.Priority;

            return await vaccineRepository.UpdateVaccineAsync(vaccine);
        }


        public async Task<Vaccine?> DeleteVaccineAsync(int id)
        {
            if (id == null)
            {
                throw new Exception("id can't be empty");
            }
            var vaccine = await vaccineRepository.GetVaccineByIDAsync(id);
            if (vaccine == null)
            {
                throw new Exception($"VaccineID {id} not found!");
            }
            return await vaccineRepository.DeleteVaccineAsync(vaccine);
        }


        public async Task<List<Vaccine>> GetSutableVaccineAsync(int age, int typeID)
        {
            ValidateInput(age, "Tuổi của trẻ đang trống!!! (có thể là lỗi BE)");
            ValidateInput(typeID, "Chưa điền ID loại vaccine cần tìm");

            var vaccineList = await vaccineRepository.GetVaccineByTypeIDAsync(typeID);
            List<Vaccine> result = new List<Vaccine>();

            foreach (var vaccine in vaccineList)
            {
                if (age >= vaccine.FromAge && age <= vaccine.ToAge)
                {
                    result.Add(vaccine);
                }
            }
            ValidateInput(result, $"không tìm thấy vaccine loại {typeID} phù hợp với trẻ {age} tuổi");

            return result;
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

        public async Task<VaccineType?> UpdateVaccineTypeAsync(int id, UpdateVaccineTypeDto updateVaccineType)
        {
            if (id == null)
            {
                throw new Exception("id can't be empty");
            }
            var vaccineType = await vaccineRepository.GetVaccineTypeByIDAsync(id);
            if (vaccineType == null)
            {
                throw new Exception($"vaccineType with ID {id} not found!");
            }
            vaccineType.Name = NullValidator(updateVaccineType.Name)
                ? updateVaccineType.Name
                : vaccineType.Name;
            vaccineType.Description = NullValidator(updateVaccineType.Description)
                ? updateVaccineType.Description
                : vaccineType.Description;

            return await vaccineRepository.UpdateVaccineTypeAsync(vaccineType);
        }

        public async Task<VaccineType?> DeleteVaccineTypeAsync(int id)
        {
            if (id == null)
            {
                throw new Exception("id can't be empty");
            }
            var vaccineType = await vaccineRepository.GetVaccineTypeByIDAsync(id);
            if (vaccineType == null)
            {
                throw new Exception($"vaccineType with ID {id} not found!");
            }
            return await vaccineRepository.DeleteVaccineTypeAsync(vaccineType);
        }

        public async Task<List<VaccineType>> GetAllVaccineTypeAsync()
        {
            return await vaccineRepository.GetVaccinesTypeAsync();
        }

    }
}
