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
    /// The billingcycleService responsible for managing billingcycle related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting billingcycle information.
    /// </remarks>
    public interface IBillingCycleService
    {
        /// <summary>Retrieves a specific billingcycle by its primary key</summary>
        /// <param name="id">The primary key of the billingcycle</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The billingcycle data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of billingcycles based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of billingcycles</returns>
        Task<List<BillingCycle>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new billingcycle</summary>
        /// <param name="model">The billingcycle data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(BillingCycle model);

        /// <summary>Updates a specific billingcycle by its primary key</summary>
        /// <param name="id">The primary key of the billingcycle</param>
        /// <param name="updatedEntity">The billingcycle data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, BillingCycle updatedEntity);

        /// <summary>Updates a specific billingcycle by its primary key</summary>
        /// <param name="id">The primary key of the billingcycle</param>
        /// <param name="updatedEntity">The billingcycle data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<BillingCycle> updatedEntity);

        /// <summary>Deletes a specific billingcycle by its primary key</summary>
        /// <param name="id">The primary key of the billingcycle</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The billingcycleService responsible for managing billingcycle related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting billingcycle information.
    /// </remarks>
    public class BillingCycleService : IBillingCycleService
    {
        private readonly DadaBoudiBiriyaniContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the BillingCycle class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public BillingCycleService(DadaBoudiBiriyaniContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific billingcycle by its primary key</summary>
        /// <param name="id">The primary key of the billingcycle</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The billingcycle data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.BillingCycle.AsQueryable();
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

        /// <summary>Retrieves a list of billingcycles based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of billingcycles</returns>/// <exception cref="Exception"></exception>
        public async Task<List<BillingCycle>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetBillingCycle(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new billingcycle</summary>
        /// <param name="model">The billingcycle data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(BillingCycle model)
        {
            model.Id = await CreateBillingCycle(model);
            return model.Id;
        }

        /// <summary>Updates a specific billingcycle by its primary key</summary>
        /// <param name="id">The primary key of the billingcycle</param>
        /// <param name="updatedEntity">The billingcycle data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, BillingCycle updatedEntity)
        {
            await UpdateBillingCycle(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific billingcycle by its primary key</summary>
        /// <param name="id">The primary key of the billingcycle</param>
        /// <param name="updatedEntity">The billingcycle data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<BillingCycle> updatedEntity)
        {
            await PatchBillingCycle(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific billingcycle by its primary key</summary>
        /// <param name="id">The primary key of the billingcycle</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteBillingCycle(id);
            return true;
        }
        #region
        private async Task<List<BillingCycle>> GetBillingCycle(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.BillingCycle.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<BillingCycle>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(BillingCycle), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<BillingCycle, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreateBillingCycle(BillingCycle model)
        {
            _dbContext.BillingCycle.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateBillingCycle(Guid id, BillingCycle updatedEntity)
        {
            _dbContext.BillingCycle.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteBillingCycle(Guid id)
        {
            var entityData = _dbContext.BillingCycle.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.BillingCycle.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchBillingCycle(Guid id, JsonPatchDocument<BillingCycle> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.BillingCycle.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.BillingCycle.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}