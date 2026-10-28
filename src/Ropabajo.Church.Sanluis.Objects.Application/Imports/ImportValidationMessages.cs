namespace Ropabajo.Church.Sanluis.Objects.Application.Imports;

/// <summary>
/// Constantes funcionales de importación.
/// </summary>
public static class ImportValidationMessages
{
    /// <summary>
    /// Regla funcional NotFound.
    /// </summary>
    public const string NotFound = "No se encontró la carga en tu iglesia y usuario.";

    /// <summary>
    /// Regla funcional ObjectUnavailable.
    /// </summary>
    public const string ObjectUnavailable =
        "El archivo no está disponible para esta cuenta o aún no terminó de subirse.";

    /// <summary>
    /// Regla funcional FormatInvalid.
    /// </summary>
    public const string FormatInvalid =
        "El archivo no corresponde a la plantilla de personas y miembros.";

    /// <summary>
    /// Regla funcional IntentConflict.
    /// </summary>
    public const string IntentConflict =
        "La solicitud ya fue utilizada para otro archivo o iglesia.";

    /// <summary>
    /// Regla funcional ObjectUsed.
    /// </summary>
    public const string ObjectUsed = "El archivo ya está asociado a una carga.";

    /// <summary>
    /// Regla funcional RetryUnavailable.
    /// </summary>
    public const string RetryUnavailable =
        "La fila no tiene un paso pendiente que se pueda reintentar.";

    /// <summary>
    /// Regla funcional ChurchCodeRequired.
    /// </summary>
    public const string ChurchCodeRequired =
        "El campo Identificador Único de Iglesia es requerido.";

    /// <summary>
    /// Regla funcional RequestIdRequired.
    /// </summary>
    public const string RequestIdRequired =
        "El campo Identificador Único de Solicitud es requerido.";

    /// <summary>
    /// Regla funcional ObjectCodeRequired.
    /// </summary>
    public const string ObjectCodeRequired =
        "El campo Identificador Único de Archivo es requerido.";

    /// <summary>
    /// Regla funcional FormatCodeRequired.
    /// </summary>
    public const string FormatCodeRequired =
        "El campo Identificador Único de Formato es requerido.";

    /// <summary>
    /// Regla funcional LoadCodeRequired.
    /// </summary>
    public const string LoadCodeRequired = "El campo Identificador Único de Carga es requerido.";

    /// <summary>
    /// Regla funcional RowCodeRequired.
    /// </summary>
    public const string RowCodeRequired = "El campo Identificador Único de Fila es requerido.";

    /// <summary>
    /// Regla funcional DescriptionMaxLength.
    /// </summary>
    public const string DescriptionMaxLength =
        "El campo Descripción no debe exceder 400 caracteres.";

    /// <summary>
    /// Regla funcional PageNumberRange.
    /// </summary>
    public const string PageNumberRange = "El campo Página debe estar entre 1 y 100000.";

    /// <summary>
    /// Regla funcional PageSizeRange.
    /// </summary>
    public const string PageSizeRange = "El campo Tamaño de Página debe estar entre 1 y 100.";

    /// <summary>
    /// Regla funcional StateInvalid.
    /// </summary>
    public const string StateInvalid = "El campo Estado no tiene un valor válido.";

    /// <summary>
    /// Contexto autenticado requerido para consultar el avance.
    /// </summary>
    public const string RequesterRequired = "El campo Contexto del solicitante es requerido.";

    /// <summary>
    /// Identificadores válidos, distintos y acotados de la suscripción.
    /// </summary>
    public const string LoadCodesValid =
        "El campo Cargas debe contener entre una y cincuenta cargas distintas y válidas.";

    /// <summary>
    /// No se pudo preparar una conexión autorizada de avance.
    /// </summary>
    public const string ProgressUnavailable =
        "No se pudo iniciar el seguimiento. Vuelve a intentarlo.";
}
