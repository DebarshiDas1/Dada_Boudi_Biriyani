using DadaBoudiBiriyani.Models;
using DadaBoudiBiriyani.Data;
using DadaBoudiBiriyani.Filter;
using DadaBoudiBiriyani.Entities;
using DadaBoudiBiriyani.Logger;
using Microsoft.AspNetCore.JsonPatch;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using Task = System.Threading.Tasks.Task;

namespace DadaBoudiBiriyani.Services
{
    /// <summary>
    /// The remittanceadviceService responsible for managing remittanceadvice related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting remittanceadvice information.
    /// </remarks>
    public interface IRemittanceAdviceService
    {
        /// <summary>Retrieves a specific remittanceadvice by its primary key</summary>
        /// <param name="id">The primary key of the remittanceadvice</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The remittanceadvice data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of remittanceadvices based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of remittanceadvices</returns>
        Task<List<RemittanceAdvice>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new remittanceadvice</summary>
        /// <param name="model">The remittanceadvice data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(RemittanceAdvice model);

        /// <summary>Updates a specific remittanceadvice by its primary key</summary>
        /// <param name="id">The primary key of the remittanceadvice</param>
        /// <param name="updatedEntity">The remittanceadvice data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, RemittanceAdvice updatedEntity);

        /// <summary>Updates a specific remittanceadvice by its primary key</summary>
        /// <param name="id">The primary key of the remittanceadvice</param>
        /// <param name="updatedEntity">The remittanceadvice data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<RemittanceAdvice> updatedEntity);

        /// <summary>Deletes a specific remittanceadvice by its primary key</summary>
        /// <param name="id">The primary key of the remittanceadvice</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The remittanceadviceService responsible for managing remittanceadvice related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting remittanceadvice information.
    /// </remarks>
    public class RemittanceAdviceService : IRemittanceAdviceService
    {
        private readonly DadaBoudiBiriyaniContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the RemittanceAdvice class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public RemittanceAdviceService(DadaBoudiBiriyaniContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific remittanceadvice by its primary key</summary>
        /// <param name="id">The primary key of the remittanceadvice</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The remittanceadvice data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.RemittanceAdvice.AsQueryable();
            List<string> allfields = new List<string>();
            if (!string.IsNullOrEmpty(fields))
            {
                allfields.AddRange(fields.Split(","));
                fields = $"Id,{fields}";
            }
            else
            {
                fields = "Id";
            }

            string[] navigationProperties = [];
            foreach (var navigationProperty in navigationProperties)
            {
                if (allfields.Any(field => field.StartsWith(navigationProperty + ".", StringComparison.OrdinalIgnoreCase)))
                {
                    query = query.Include(navigationProperty);
                }
            }

            query = query.Where(entity => entity.Id == id);
            return _mapper.MapToFields(await query.FirstOrDefaultAsync(),fields);
        }

        /// <summary>Retrieves a list of remittanceadvices based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of remittanceadvices</returns>/// <exception cref="Exception"></exception>
        public async Task<List<RemittanceAdvice>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetRemittanceAdvice(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new remittanceadvice</summary>
        /// <param name="model">The remittanceadvice data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(RemittanceAdvice model)
        {
            model.Id = await CreateRemittanceAdvice(model);
            return model.Id;
        }

        /// <summary>Updates a specific remittanceadvice by its primary key</summary>
        /// <param name="id">The primary key of the remittanceadvice</param>
        /// <param name="updatedEntity">The remittanceadvice data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, RemittanceAdvice updatedEntity)
        {
            await UpdateRemittanceAdvice(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific remittanceadvice by its primary key</summary>
        /// <param name="id">The primary key of the remittanceadvice</param>
        /// <param name="updatedEntity">The remittanceadvice data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<RemittanceAdvice> updatedEntity)
        {
            await PatchRemittanceAdvice(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific remittanceadvice by its primary key</summary>
        /// <param name="id">The primary key of the remittanceadvice</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteRemittanceAdvice(id);
            return true;
        }
        #region
        private async Task<List<RemittanceAdvice>> GetRemittanceAdvice(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.RemittanceAdvice.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<RemittanceAdvice>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(RemittanceAdvice), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<RemittanceAdvice, object>>(Expression.Convert(property, typeof(object)), parameter);
                if (sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase))
                {
                    result = result.OrderBy(lambda);
                }
                else if (sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase))
                {
                    result = result.OrderByDescending(lambda);
                }
                else
                {
                    throw new ApplicationException("Invalid sort order. Use 'asc' or 'desc'");
                }
            }

            var paginatedResult = await result.Skip(skip).Take(pageSize).ToListAsync();
            return paginatedResult;
        }

        private async Task<Guid> CreateRemittanceAdvice(RemittanceAdvice model)
        {
            _dbContext.RemittanceAdvice.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateRemittanceAdvice(Guid id, RemittanceAdvice updatedEntity)
        {
            _dbContext.RemittanceAdvice.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteRemittanceAdvice(Guid id)
        {
            var entityData = _dbContext.RemittanceAdvice.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.RemittanceAdvice.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchRemittanceAdvice(Guid id, JsonPatchDocument<RemittanceAdvice> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.RemittanceAdvice.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.RemittanceAdvice.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}