﻿using VaccineScheduleTracking.API.Models.Entities;
using VaccineScheduleTracking.API_Test.Repository.IRepository;

namespace VaccineScheduleTracking.API_Test.Services
{
    public class ChildService : IChildService
    {
        private readonly IChildRepository childRepository;

        public ChildService(IChildRepository childRepository)
        {
            this.childRepository = childRepository;
        }

        public async Task<List<Child>> GetParentChildren(int parentID)
        {
            return await childRepository.GetChildrenByParentID(parentID);
        }

        public async Task<Child> AddChild(Child child)
        {
            if (!child.Gender.ToLower().Equals("female") && !child.Gender.ToLower().Equals("male"))
            {
                throw new Exception("Please input valid gender!");
            } else
            {
                child.Gender = child.Gender.ToUpper()[0] + child.Gender.ToLower().Substring(1);
            }
            return await childRepository.AddChild(child);
        }

        public async Task<Child> UpdateChild(int id, Child child)
        {
            return await childRepository.UpdateChild(id, child);
        }

        public async Task<Child> DeleteChild(int id)
        {
            var child = await childRepository.GetChildById(id);
            if (child == null)
            {
                throw new Exception($"Không tìm thấy ID: {id}");
            }
            return await childRepository.DeleteChildAsync(child);

        }
    }
}
