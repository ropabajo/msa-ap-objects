namespace Ropabajo.Church.Sanluis.Objects.Application.ViewModels
{
    public class UnprocessableVm
    {
        /// <summary>
        /// Código de estado HTTP
        /// </summary>
        /// <example>422</example>
        public string StatusCode { get; set; }

        /// <summary>
        /// Sello de tiempo
        /// </summary>
        /// <example>2023-06-22T16:35:10.9860611-05:00</example>
        public string Timestamp { get; set; }

        /// <summary>
        /// Ruta consultada
        /// </summary>
        /// <example>v1/payroll-concepts</example>
        public string Path { get; set; }

        /// <summary>
        /// Lista de errores
        /// </summary>
        public IEnumerable<UnprocessableErrorVm> Errors { get; set; }
    }

    public class UnprocessableErrorVm
    {
        /// <summary>
        /// Código del error
        /// </summary>
        /// <example>422.0001</example>
        public string Code { get; set; }

        /// <summary>
        /// Mensaje del error
        /// </summary>
        /// <example>El Código de Concepto ya existe</example>
        public string Message { get; set; }
    }
}
