namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoadStates.Queries.GetBulkLoadStates
{
    public class BulkLoadStatesVm
    {
        /// <summary>
        /// Fecha y hora del cambio de estado
        /// </summary>
        /// <example>2025-02-25 01:58:36</example>
        public DateTime Date { get; set; }

        /// <summary>
        /// Estado actual de la carga masiva
        /// 
        /// **Elementos:**
        /// 
        /// | Código        |     Descripción  |
        /// |---------------|------------------|
        /// |   Pending     |     PENDIENTE    |
        /// |   Processing  |     PROCESANDO   |
        /// |   Processed   |     PROCESADO    |
        /// </summary>
        public string StateCode { get; set; }

        /// <summary>
        /// Nombre de usuario que realizó el cambio de estado
        /// </summary>
        /// <example>RBARRERA</example>
        public string User { get; set; }
    }
}
