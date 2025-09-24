using System.Linq.Expressions;
using AppointmentSystem.Application.DTOS.Response;
using AppointmentSystem.Application.Interfaces.Repositories;
using AppointmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Infrastructure.Repositories
{
    public class GenericRepository<TModel> : IGenericRepository<TModel> where TModel : class
    {
        protected readonly AppDbContext _dbContext;

        public GenericRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<TModel> Create(TModel model)
        {
            _dbContext.Set<TModel>().Add(model);
            return Task.FromResult(model); 
        }

        public Task<bool> Edit(TModel model)
        {
            _dbContext.Set<TModel>().Update(model);
            return Task.FromResult(true);
        }

        public Task<bool> Delete(TModel model)
        {
            _dbContext.Set<TModel>().Remove(model);
            return Task.FromResult(true); 
        }

        public IQueryable<TModel> GetAll(
          Expression<Func<TModel, bool>>? filter = null,
          Func<IQueryable<TModel>, IQueryable<TModel>>? include = null)
        {
            IQueryable<TModel> query = _dbContext.Set<TModel>();

            if (include != null)
                query = include(query);

            if (filter != null)
                query = query.Where(filter);

            return query;
        }


        public async Task<TModel?> GetByItem(
            Expression<Func<TModel, bool>> predicate,
            Func<IQueryable<TModel>, IQueryable<TModel>>? include = null
        )
        {
            IQueryable<TModel> query = _dbContext.Set<TModel>();

            if (include != null)
                query = include(query);

            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<PaginatedResponse<TModel>> GetPagedAsync(
            int page,
            int pageSize,
            Expression<Func<TModel, bool>>? filter = null,
            Func<IQueryable<TModel>, IQueryable<TModel>>? include = null,
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>>? orderBy = null
        )
        {
            IQueryable<TModel> query = _dbContext.Set<TModel>();

            if (include != null)
                query = include(query);

            if (filter != null)
                query = query.Where(filter);

            if (orderBy != null)
                query = orderBy(query);

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponse<TModel>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }
    }
}