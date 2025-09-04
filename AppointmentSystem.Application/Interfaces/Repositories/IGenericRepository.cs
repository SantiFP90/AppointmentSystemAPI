using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AppointmentSystem.Application.DTOS.Response;

namespace AppointmentSystem.Application.Interfaces.Repositories
{
    public interface IGenericRepository<TModel> where TModel : class
    {
        Task<TModel?> GetByItem(
          Expression<Func<TModel, bool>> predicate,
          Func<IQueryable<TModel>, IQueryable<TModel>>? include = null);
        IQueryable<TModel> GetAll(
            Expression<Func<TModel, bool>>? filter = null,
            Func<IQueryable<TModel>, IQueryable<TModel>>? include = null);
        Task<PaginatedResponse<TModel>> GetPagedAsync(
            int page,
            int pageSize,
            Expression<Func<TModel, bool>>? filter = null,
            Func<IQueryable<TModel>, IQueryable<TModel>>? include = null,
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>>? orderBy = null
        );
        Task<TModel> Create(TModel model);
        Task<bool> Edit(TModel model);
        Task<bool> Delete(TModel model);
    };
}
