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
    /// The clinicService responsible for managing clinic related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting clinic information.
    /// </remarks>
    public interface IClinicService
    {
        /// <summary>Retrieves a specific clinic by its primary key</summary>
        /// <param name="id">The primary key of the clinic</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The clinic data</returns>
        Task<dynamic> GetById(Guid id, string fields);

        /// <summary>Retrieves a list of clinics based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of clinics</returns>
        Task<List<Clinic>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc");

        /// <summary>Adds a new clinic</summary>
        /// <param name="model">The clinic data to be added</param>
        /// <returns>The result of the operation</returns>
        Task<Guid> Create(Clinic model);

        /// <summary>Updates a specific clinic by its primary key</summary>
        /// <param name="id">The primary key of the clinic</param>
        /// <param name="updatedEntity">The clinic data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Update(Guid id, Clinic updatedEntity);

        /// <summary>Updates a specific clinic by its primary key</summary>
        /// <param name="id">The primary key of the clinic</param>
        /// <param name="updatedEntity">The clinic data to be updated</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Patch(Guid id, JsonPatchDocument<Clinic> updatedEntity);

        /// <summary>Deletes a specific clinic by its primary key</summary>
        /// <param name="id">The primary key of the clinic</param>
        /// <returns>The result of the operation</returns>
        Task<bool> Delete(Guid id);
    }

    /// <summary>
    /// The clinicService responsible for managing clinic related operations.
    /// </summary>
    /// <remarks>
    /// This service for adding, retrieving, updating, and deleting clinic information.
    /// </remarks>
    public class ClinicService : IClinicService
    {
        private readonly DadaBoudiBiriyaniContext _dbContext;
        private readonly IFieldMapperService _mapper;

        /// <summary>
        /// Initializes a new instance of the Clinic class.
        /// </summary>
        /// <param name="dbContext">dbContext value to set.</param>
        /// <param name="mapper">mapper value to set.</param>
        public ClinicService(DadaBoudiBiriyaniContext dbContext, IFieldMapperService mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>Retrieves a specific clinic by its primary key</summary>
        /// <param name="id">The primary key of the clinic</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The clinic data</returns>
        public async Task<dynamic> GetById(Guid id, string fields)
        {
            var query = _dbContext.Clinic.AsQueryable();
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

        /// <summary>Retrieves a list of clinics based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of clinics</returns>/// <exception cref="Exception"></exception>
        public async Task<List<Clinic>> Get(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            var result = await GetClinic(filters, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return result;
        }

        /// <summary>Adds a new clinic</summary>
        /// <param name="model">The clinic data to be added</param>
        /// <returns>The result of the operation</returns>
        public async Task<Guid> Create(Clinic model)
        {
            model.Id = await CreateClinic(model);
            return model.Id;
        }

        /// <summary>Updates a specific clinic by its primary key</summary>
        /// <param name="id">The primary key of the clinic</param>
        /// <param name="updatedEntity">The clinic data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Update(Guid id, Clinic updatedEntity)
        {
            await UpdateClinic(id, updatedEntity);
            return true;
        }

        /// <summary>Updates a specific clinic by its primary key</summary>
        /// <param name="id">The primary key of the clinic</param>
        /// <param name="updatedEntity">The clinic data to be updated</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Patch(Guid id, JsonPatchDocument<Clinic> updatedEntity)
        {
            await PatchClinic(id, updatedEntity);
            return true;
        }

        /// <summary>Deletes a specific clinic by its primary key</summary>
        /// <param name="id">The primary key of the clinic</param>
        /// <returns>The result of the operation</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Delete(Guid id)
        {
            await DeleteClinic(id);
            return true;
        }
        #region
        private async Task<List<Clinic>> GetClinic(List<FilterCriteria> filters = null, string searchTerm = "", int pageNumber = 1, int pageSize = 1, string sortField = null, string sortOrder = "asc")
        {
            if (pageSize < 1)
            {
                throw new ApplicationException("Page size invalid!");
            }

            if (pageNumber < 1)
            {
                throw new ApplicationException("Page mumber invalid!");
            }

            var query = _dbContext.Clinic.IncludeRelated().AsQueryable();
            int skip = (pageNumber - 1) * pageSize;
            var result = FilterService<Clinic>.ApplyFilter(query, filters, searchTerm);
            if (!string.IsNullOrEmpty(sortField))
            {
                var parameter = Expression.Parameter(typeof(Clinic), "b");
                var property = Expression.Property(parameter, sortField);
                var lambda = Expression.Lambda<Func<Clinic, object>>(Expression.Convert(property, typeof(object)), parameter);
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

        private async Task<Guid> CreateClinic(Clinic model)
        {
            _dbContext.Clinic.Add(model);
            await _dbContext.SaveChangesAsync();
            return model.Id;
        }

        private async Task UpdateClinic(Guid id, Clinic updatedEntity)
        {
            _dbContext.Clinic.Update(updatedEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> DeleteClinic(Guid id)
        {
            var entityData = _dbContext.Clinic.FirstOrDefault(entity => entity.Id == id);
            if (entityData == null)
            {
                throw new ApplicationException("No data found!");
            }

            _dbContext.Clinic.Remove(entityData);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task PatchClinic(Guid id, JsonPatchDocument<Clinic> updatedEntity)
        {
            if (updatedEntity == null)
            {
                throw new ApplicationException("Patch document is missing!");
            }

            var existingEntity = _dbContext.Clinic.FirstOrDefault(t => t.Id == id);
            if (existingEntity == null)
            {
                throw new ApplicationException("No data found!");
            }

            updatedEntity.ApplyTo(existingEntity);
            _dbContext.Clinic.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}