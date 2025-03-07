﻿using GenesisFEPortalWeb.Database.Data;
using GenesisFEPortalWeb.Models.Entities.Tenant;
using Microsoft.EntityFrameworkCore;

namespace GenesisFEPortalWeb.BL.Repositories.Core
{
    public interface ITenantRepository
    {
        Task<TenantModel?> GetByIdAsync(long id);
        Task<TenantModel?> GetByIdentificationAsync(string identification);
        Task<bool> IdentificationExistsAsync(string identification);
        Task<TenantModel> CreateTenantAsync(TenantModel tenant);
        Task SaveChangesAsync();
    }

    public class TenantRepository : ITenantRepository
    {
        private readonly AppDbContext _context;

        public TenantRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TenantModel?> GetByIdAsync(long id)
        {
            return await _context.Tenants.FindAsync(id);
        }

        public async Task<TenantModel?> GetByIdentificationAsync(string identification)
        {
            return await _context.Tenants
                .FirstOrDefaultAsync(t => t.Identification == identification);
        }

        public async Task<bool> IdentificationExistsAsync(string identification)
        {
            return await _context.Tenants
                .AnyAsync(t => t.Identification == identification);
        }

        public async Task<TenantModel> CreateTenantAsync(TenantModel tenant)
        {
            var result = await _context.Tenants.AddAsync(tenant);
            return result.Entity;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
