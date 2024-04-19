using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Departments.Commands.CreateDepartment;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Departments.Queries.GetDepartments;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Districts.Queries.GetDistricts;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Provinces.Queries.GetProvinces;
using Ropabajo.Church.Sanluis.Objects.Application.ViewModels;
using System.Net.Mime;

namespace Ropabajo.Church.Sanluis.Objects.Api.Controllers
{
    /// <summary>
    /// Gestión de departamentos
    /// </summary> 
    [ApiController]
    [Route("v1")]
    public class UbigeoController : ApiController
    {
        private readonly IMediatorBus _mediator;

        public UbigeoController(
            IMediatorBus mediator,
            INotificationHandler<Header> headers,
            INotificationHandler<Notification> notifications,
            IActionContextAccessor actionContextAccessor
        ) : base(headers, notifications, actionContextAccessor)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtener departamentos
        /// </summary>
        /// <remarks>
        /// Obtiene un listado de departamentos
        /// </remarks>
        /// <response code="200">Solicitud exitosa</response>
        /// <response code="204">Sin contenido</response>
        /// <response code="400">Solicitud incorrecta</response>
        /// <response code="401">No autorizado</response>
        /// <response code="404">No encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("departament", Name = "GetDepartmentsAsync")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(IEnumerable<DepartmentsVm>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DepartmentsVm>>> GetDepartmentsAsync([FromQuery] GetDepartmentsQuery query)
        {
            var periods = await _mediator.SendAsync(query);
            return Response(periods);
        }

        /// <summary>
        /// Carga inicial de departamentos
        /// </summary>
        /// <remarks>
        /// Carga inicial de departamentos
        /// </remarks>
        /// <response code="201">Recurso creado</response>
        /// <response code="400">Solicitud incorrecta</response>
        /// <response code="401">No autorizado</response>
        /// <response code="422">Entidad no procesable</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost(Name = "CreateDepartmentAsync")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(BadRequestVm), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UnprocessableVm), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> CreateDepartmentAsync([FromBody] CreateDepartmentCommand command, CancellationToken cancellationToken = default)
        {
            await _mediator.SendAsync(command);

            return Response();
        }

        /// <summary>
        /// Obtener provincias
        /// </summary>
        /// <remarks>
        /// Obtiene un listado de provincias
        /// </remarks>
        /// <response code="200">Solicitud exitosa</response>
        /// <response code="204">Sin contenido</response>
        /// <response code="400">Solicitud incorrecta</response>
        /// <response code="401">No autorizado</response>
        /// <response code="404">No encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("province", Name = "GetProvincesAsync")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(IEnumerable<ProvincesVm>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProvincesVm>>> GetProvincesAsync([FromQuery] GetProvincesQuery query)
        {
            var periods = await _mediator.SendAsync(query);
            return Response(periods);
        }

        /// <summary>
        /// Obtener distrito
        /// </summary>
        /// <remarks>
        /// Obtiene un listado de distrito
        /// </remarks>
        /// <response code="200">Solicitud exitosa</response>
        /// <response code="204">Sin contenido</response>
        /// <response code="400">Solicitud incorrecta</response>
        /// <response code="401">No autorizado</response>
        /// <response code="404">No encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("district", Name = "GetDistrictsAsync")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(IEnumerable<DistrictsVm>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DistrictsVm>>> GetDistrictsAsync([FromQuery] GetDistrictsQuery query)
        {
            var periods = await _mediator.SendAsync(query);
            return Response(periods);
        }
    }
}
