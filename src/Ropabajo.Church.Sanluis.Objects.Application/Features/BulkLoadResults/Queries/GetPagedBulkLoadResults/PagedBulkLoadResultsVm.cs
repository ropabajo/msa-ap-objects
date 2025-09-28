namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoadResults.Queries.GetPagedBulkLoadResults
{
    public class PagedBulkLoadResultsVm
    {
        /// <summary>
        /// Código único del resultado de la carga masiva
        /// </summary>
        /// <example>86b25787-0c35-4fe7-a07e-cd116a03d093</example>
        public Guid BulkLoadResultCode { get; set; }

        /// <summary>
        /// Código único de la carga masiva
        /// </summary>
        /// <example>25e6dde1-f6a7-4486-96ac-7672641db173</example>
        public Guid BulkLoadCode { get; set; }

        /// <summary>
        /// Código único de formato
        /// </summary>
        /// <example>82d1d1db-0727-421e-a0fe-78750146f433</example>
        public Guid FormatCode { get; set; }

        /// <summary>
        /// Número de fila en el archivo de carga
        /// </summary>
        /// <example>1</example>
        public int RowNumber { get; set; }

        /// <summary>
        /// Información de la fila en formato JSON
        /// </summary>
        /// <example>{"TIPO DOCUMENTO IDENTIDAD":"2","NUMERO DOCUMENTO IDENTIDAD":"4545345345","APELLIDOS":"cruz del aguilar","NOMBRES":"tabita","GENERO":"2","FECHA NACIMIENTO":"2000-12-05","NUMERO MOVIL":"963852711"}</example>
        public string Row { get; set; }

        /// <summary>
        /// Estado actual del resultado de la carga masiva
        /// 
        /// **Elementos:**
        /// 
        /// | Código        |     Descripción  |
        /// |---------------|------------------|
        /// |   Observed    |     OBSERVADO    |
        /// |   Processed   |     PROCESADO    |
        /// |   Completed   |     COMPLETADO   |
        /// </summary>
        public string StateCode { get; set; }

        /// <summary>
        /// Observación del resultado de la carga masiva
        /// </summary>
        /// <example>Fecha de nacimiento no puede ser futura. | 'Fecha de nacimiento' debe ser menor o igual que '24/09/2025 00:00:00'.</example>
        public string? Observation { get; set; }
    }
}
