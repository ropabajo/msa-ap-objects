using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ropabajo.Church.Sanluis.Objects.Application.ViewModels
{
    public class BadRequestVm
    {
        /// <summary>
        /// Código de estado HTTP
        /// </summary>
        /// <example>400</example>
        public string StatusCode { get; set; }

        /// <summary>
        /// Sello de tiempo
        /// </summary>
        /// <example>2023-06-22T16:33:49.2772032-05:00</example>
        public string Timestamp { get; set; }

        /// <summary>
        /// Ruta consultada
        /// </summary>
        /// <example>v1/payroll-concepts</example>
        public string Path { get; set; }

        /// <summary>
        /// Lista de errores
        /// </summary>
        public IEnumerable<BadRequestErrorVm> Errors { get; set; }
    }

    public class BadRequestErrorVm
    {
        /// <summary>
        /// Código del error
        /// </summary>
        /// <example>400.0101</example>
        public string Code { get; set; }

        /// <summary>
        /// Mensaje del error
        /// </summary>
        /// <example>{LaborRegimeCode} is required.</example>
        public string Message { get; set; }
    }
}
