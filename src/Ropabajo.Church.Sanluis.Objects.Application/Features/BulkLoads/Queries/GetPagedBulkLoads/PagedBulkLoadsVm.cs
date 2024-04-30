namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Queries.GetPagedBulkLoads
{
    public class PagedBulkLoadsVm
    {
        /// <summary>
        /// Código único de la carga masiva
        /// </summary>
        /// <example>9b40c616-b1e3-4ee3-81b3-e59d962aba7c</example>
        public string BulkLoadCode { get; set; }

        /// <summary>
        /// Código único de objeto de planilla
        /// </summary>
        /// <example>c2d5ca14-dea4-4a4a-9728-5c2e8148490b</example>
        public string PayrollObjectCode { get; set; }

        /// <summary>
        /// Descripción de la carga
        /// </summary>
        /// <example>Carga inicial de persona</example>
        public string Description { get; set; }

        /// <summary>
        /// Estado actual de la carga masiva
        /// 
        /// **Elementos:**
        /// 
        /// | Código |     Descripción     |
        /// |--------|--------------------|
        /// |   processing    |     PROCESANDO     |
        /// |   processed     |     PROCESADO     |
        /// </summary>
        public string StateCode { get; set; }

        /// <summary>
        /// Usuario que realizó la carga masiva
        /// </summary>
        /// <example>Juan Perez</example>
        public string User { get; set; }

        /// <summary>
        /// Fecha de cambio de estado
        /// </summary>
        /// <example>2022-01-01</example>
        public DateTime Date { get; set; }
    }
}
