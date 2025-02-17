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
                throw new Exception($"Can't find child with ID: {id}");
            }
            return await childRepository.DeleteChildAsync(child);

        }
    }
}
