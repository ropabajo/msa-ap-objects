namespace Ropabajo.Church.Sanluis.Objects.Application.Imports;

/// <summary>
/// Constantes funcionales de importación.
/// </summary>
public static class ImportValidationCodes
{
    /// <summary>
    /// Regla funcional NotFound.
    /// </summary>
    public const string NotFound = "IMPORT_NOT_FOUND";

    /// <summary>
    /// Regla funcional ObjectUnavailable.
    /// </summary>
    public const string ObjectUnavailable = "IMPORT_OBJECT_UNAVAILABLE";

    /// <summary>
    /// Regla funcional FormatInvalid.
    /// </summary>
    public const string FormatInvalid = "IMPORT_FORMAT_INVALID";

    /// <summary>
    /// Regla funcional IntentConflict.
    /// </summary>
    public const string IntentConflict = "IMPORT_INTENT_CONFLICT";

    /// <summary>
    /// Regla funcional ObjectUsed.
    /// </summary>
    public const string ObjectUsed = "IMPORT_OBJECT_USED";

    /// <summary>
    /// Regla funcional RetryUnavailable.
    /// </summary>
    public const string RetryUnavailable = "IMPORT_RETRY_UNAVAILABLE";

    /// <summary>
    /// Regla funcional ChurchCodeRequired.
    /// </summary>
    public const string ChurchCodeRequired = "IMPORT_CHURCH_REQUIRED";

    /// <summary>
    /// Regla funcional RequestIdRequired.
    /// </summary>
    public const string RequestIdRequired = "IMPORT_REQUEST_REQUIRED";

    /// <summary>
    /// Regla funcional ObjectCodeRequired.
    /// </summary>
    public const string ObjectCodeRequired = "IMPORT_OBJECT_REQUIRED";

    /// <summary>
    /// Regla funcional FormatCodeRequired.
    /// </summary>
    public const string FormatCodeRequired = "IMPORT_FORMAT_REQUIRED";

    /// <summary>
    /// Regla funcional LoadCodeRequired.
    /// </summary>
    public const string LoadCodeRequired = "IMPORT_LOAD_REQUIRED";

    /// <summary>
    /// Regla funcional RowCodeRequired.
    /// </summary>
    public const string RowCodeRequired = "IMPORT_ROW_REQUIRED";

    /// <summary>
    /// Regla funcional DescriptionMaxLength.
    /// </summary>
    public const string DescriptionMaxLength = "IMPORT_DESCRIPTION_MAX_LENGTH";

    /// <summary>
    /// Regla funcional PageNumberRange.
    /// </summary>
    public const string PageNumberRange = "IMPORT_PAGE_NUMBER_RANGE";

    /// <summary>
    /// Regla funcional PageSizeRange.
    /// </summary>
    public const string PageSizeRange = "IMPORT_PAGE_SIZE_RANGE";

    /// <summary>
    /// Regla funcional StateInvalid.
    /// </summary>
    public const string StateInvalid = "IMPORT_STATE_INVALID";

    /// <summary>
    /// Contexto autenticado requerido para consultar el avance.
    /// </summary>
    public const string RequesterRequired = "IMPORT_REQUESTER_REQUIRED";

    /// <summary>
    /// Identificadores válidos, distintos y acotados de la suscripción.
    /// </summary>
    public const string LoadCodesValid = "IMPORT_PROGRESS_CODES_INVALID";

    /// <summary>
    /// No se pudo preparar una conexión autorizada de avance.
    /// </summary>
    public const string ProgressUnavailable = "IMPORT_PROGRESS_UNAVAILABLE";
}
