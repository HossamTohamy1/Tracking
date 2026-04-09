using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class GeneralRepository<T> : IGeneralRepository<T> where T : BaseEntity
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GeneralRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public IQueryable<T> GetAll()
        {
            return _dbSet.Where(x => !x.IsDeleted);
        }


        public async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdatePartialAsync(T entity, params string[] modifiedParams)
        {
            var existing = await _dbSet.FindAsync(entity.Id);
            if (existing == null)
                return false;

            var entry = _context.Entry(existing);

            foreach (var prop in modifiedParams)
            {
                var newValue = entity.GetType().GetProperty(prop)?.GetValue(entity);
                entry.Property(prop).CurrentValue = newValue;
                entry.Property(prop).IsModified = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}