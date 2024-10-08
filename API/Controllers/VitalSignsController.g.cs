using Microsoft.AspNetCore.Mvc;
using DadaBoudiBiriyani.Models;
using DadaBoudiBiriyani.Services;
using DadaBoudiBiriyani.Entities;
using DadaBoudiBiriyani.Filter;
using DadaBoudiBiriyani.Helpers;
using DadaBoudiBiriyani.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Task = System.Threading.Tasks.Task;
using DadaBoudiBiriyani.Authorization;

namespace DadaBoudiBiriyani.Controllers
{
    /// <summary>
    /// Controller responsible for managing vitalsigns related operations.
    /// </summary>
    /// <remarks>
    /// This Controller provides endpoints for adding, retrieving, updating, and deleting vitalsigns information.
    /// </remarks>
    [Route("api/vitalsigns")]
    [Authorize]
    public class VitalSignsController : BaseApiController
    {
        private readonly IVitalSignsService _vitalSignsService;

        /// <summary>
        /// Initializes a new instance of the VitalSignsController class with the specified context.
        /// </summary>
        /// <param name="ivitalsignsservice">The ivitalsignsservice to be used by the controller.</param>
        public VitalSignsController(IVitalSignsService ivitalsignsservice)
        {
            _vitalSignsService = ivitalsignsservice;
        }

        /// <summary>Adds a new vitalsigns</summary>
        /// <param name="model">The vitalsigns data to be added</param>
        /// <returns>The result of the operation</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NewRecord))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        [UserAuthorize("VitalSigns", Entitlements.Create)]
        public async Task<IActionResult> Post([FromBody] VitalSigns model)
        {
            model.TenantId = TenantId;
            model.CreatedOn = DateTime.UtcNow;
            model.CreatedBy = UserId;
            var id = await _vitalSignsService.Create(model);
            return Ok(new { id });
        }

        /// <summary>Retrieves a list of vitalsignss based on specified filters</summary>
        /// <param name="filters">The filter criteria in JSON format. Use the following format: [{"PropertyName": "PropertyName", "Operator": "Equal", "Value": "FilterValue"}] </param>
        /// <param name="searchTerm">To searching data.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortField">The entity's field name to sort.</param>
        /// <param name="sortOrder">The sort order asc or desc.</param>
        /// <returns>The filtered list of vitalsignss</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<VitalSigns>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        [UserAuthorize("VitalSigns", Entitlements.Read)]
        public async Task<IActionResult> Get([FromQuery] string filters, string searchTerm, int pageNumber = 1, int pageSize = 10, string sortField = null, string sortOrder = "asc")
        {
            List<FilterCriteria> filterCriteria = null;
            if (pageSize < 1)
            {
                return BadRequest(ExceptionFormatter.ErrorMessage("Page size invalid."));
            }

            if (pageNumber < 1)
            {
                return BadRequest(ExceptionFormatter.ErrorMessage("Page mumber invalid."));
            }

            if (!string.IsNullOrEmpty(filters))
            {
                filterCriteria = JsonHelper.Deserialize<List<FilterCriteria>>(filters);
            }

            var result = await _vitalSignsService.Get(filterCriteria, searchTerm, pageNumber, pageSize, sortField, sortOrder);
            return Ok(result);
        }

        /// <summary>Retrieves a specific vitalsigns by its primary key</summary>
        /// <param name="id">The primary key of the vitalsigns</param>
        /// <param name="fields">The fields is fetch data of selected fields</param>
        /// <returns>The vitalsigns data</returns>
        [HttpGet]
        [Route("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        [UserAuthorize("VitalSigns", Entitlements.Read)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, string fields = null)
        {
            var result = await _vitalSignsService.GetById( id, fields);
            return Ok(result);
        }

        /// <summary>Deletes a specific vitalsigns by its primary key</summary>
        /// <param name="id">The primary key of the vitalsigns</param>
        /// <returns>The result of the operation</returns>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionStatus))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        [Route("{id:Guid}")]
        [UserAuthorize("VitalSigns", Entitlements.Delete)]
        public async Task<IActionResult> DeleteById([FromRoute] Guid id)
        {
            var status = await _vitalSignsService.Delete(id);
            return Ok(new { status });
        }

        /// <summary>Updates a specific vitalsigns by its primary key</summary>
        /// <param name="id">The primary key of the vitalsigns</param>
        /// <param name="updatedEntity">The vitalsigns data to be updated</param>
        /// <returns>The result of the operation</returns>
        [HttpPut]
        [Route("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionStatus))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        [UserAuthorize("VitalSigns", Entitlements.Update)]
        public async Task<IActionResult> UpdateById(Guid id, [FromBody] VitalSigns updatedEntity)
        {
            if (id != updatedEntity.Id)
            {
                return BadRequest(ExceptionFormatter.ErrorMessage("Mismatched Id"));
            }

            updatedEntity.TenantId = TenantId;
            updatedEntity.UpdatedOn = DateTime.UtcNow;
            updatedEntity.UpdatedBy = UserId;
            var status = await _vitalSignsService.Update(id, updatedEntity);
            return Ok(new { status });
        }

        /// <summary>Updates a specific vitalsigns by its primary key</summary>
        /// <param name="id">The primary key of the vitalsigns</param>
        /// <param name="updatedEntity">The vitalsigns data to be updated</param>
        /// <returns>The result of the operation</returns>
        [HttpPatch]
        [Route("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionStatus))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        [UserAuthorize("VitalSigns", Entitlements.Update)]
        public async Task<IActionResult> PatchById(Guid id, [FromBody] JsonPatchDocument<VitalSigns> updatedEntity)
        {
            if (updatedEntity == null)
                return BadRequest(ExceptionFormatter.ErrorMessage("Patch document is missing."));
            var status = await _vitalSignsService.Patch(id, updatedEntity);
            return Ok(new { status });
        }
    }
}