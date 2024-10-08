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
    /// The explanationofbenefitsService responsible for managing explanationofbenefits related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting explanationofbenefits information.
    /// </remarks>
    public interface IExplanationOfBenefitsService
    {
        /// <summary>Retrieves a specific explanationofbenefits by its primary key</summary>
        /// <param name="id">The primary key of the explanationofbenefits</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The explanationofbenefits data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of explanationofbenefitss based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of explanationofbenefitss</returns>
        Task<List<ExplanationOfBenefits>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new explanationofbenefits</summary>
        /// <param name="model">The explanationofbenefits data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(ExplanationOfBenefits model);

        /// <summary>Updates a specific explanationofbenefits by its primary key</summary>
        /// <param name="id">The primary key of the explanationofbenefits</param>
        /// <param name="updatedEntity">The explanationofbenefits data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, ExplanationOfBenefits updatedEntity);

        /// <summary>Updates a specific explanationofbenefits by its primary key</summary>
        /// <param name="id">The primary key of the explanationofbenefits</param>
        /// <param name="updatedEntity">The explanationofbenefits data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<ExplanationOfBenefits> updatedEntity);

        /// <summary>Deletes a specific explanationofbenefits by its primary key</summary>
        /// <param name="id">The primary key of the explanationofbenefits</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The explanationofbenefitsService responsible for managing explanationofbenefits related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting explanationofbenefits information.
    /// </remarks>
    public class ExplanationOfBenefitsService : IExplanationOfBenefitsService
    {
        private readonly DadaBoudiBiriyaniContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the ExplanationOfBenefits class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public ExplanationOfBenefitsService(DadaBoudiBiriyaniContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific explanationofbenefits by its primary key</summary>
        /// <param name="id">The primary key of the explanationofbenefits</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The explanationofbenefits data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.ExplanationOfBenefits.AsQueryable();
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

            string[] navigationProperties = ["ClaimId_Claim"];
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

        /// <summary>Retrieves a list of explanationofbenefitss based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of explanationofbenefitss</returns>/// <exception cref="Exception"></exception>
        public async Task<List<ExplanationOfBenefits>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetExplanationOfBenefits(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new explanationofbenefits</summary>
        /// <param name="model">The explanationofbenefits data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(ExplanationOfBenefits model)
        {
            model.Id = await CreateExplanationOfBenefits(model);
            return model.Id;
        }

        /// <summary>Updates a specific explanationofbenefits by its primary key</summary>
        /// <param name="id">The primary key of the explanationofbenefits</param>
        /// <param name="updatedEntity">The explanationofbenefits data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, ExplanationOfBenefits updatedEntity)
        {
            await UpdateExplanationOfBenefits(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific explanationofbenefits by its primary key</summary>
        /// <param name="id">The primary key of the explanationofbenefits</param>
        /// <param name="updatedEntity">The explanationofbenefits data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<ExplanationOfBenefits> updatedEntity)
        {
            await PatchExplanationOfBenefits(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific explanationofbenefits by its primary key</summary>
        /// <param name="id">The primary key of the explanationofbenefits</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteExplanationOfBenefits(id);
            return true;
        }
        #region
        private async Task<List<ExplanationOfBenefits>> GetExplanationOfBenefits(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.ExplanationOfBenefits.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<ExplanationOfBenefits>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(ExplanationOfBenefits), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<ExplanationOfBenefits, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreateExplanationOfBenefits(ExplanationOfBenefits model)
        {
            _dbContext.ExplanationOfBenefits.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateExplanationOfBenefits(Guid id, ExplanationOfBenefits updatedEntity)
        {
            _dbContext.ExplanationOfBenefits.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteExplanationOfBenefits(Guid id)
        {
            var entityData = _dbContext.ExplanationOfBenefits.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.ExplanationOfBenefits.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchExplanationOfBenefits(Guid id, JsonPatchDocument<ExplanationOfBenefits> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.ExplanationOfBenefits.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.ExplanationOfBenefits.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}