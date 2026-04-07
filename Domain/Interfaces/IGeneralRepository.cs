using Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface IGeneralRepository<T> where T : BaseEntity
    {
        IQueryable<T> GetAll();
        Task<T> GetByIdAsync(Guid id);

        Task AddAsync(T entity);
        Task<bool> UpdatePartialAsync(T entity, params string[] modifiedProperties);
        Task DeleteAsync(Guid id);
        Task SaveAsync();
    }
}
