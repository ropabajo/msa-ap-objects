namespace Ropabajo.Church.Sanluis.Objects.Application.Constants;

/// <summary>
/// Centraliza códigos de las validaciones de trabajos.
/// </summary>
public static class FileJobValidationCodes
{
    /// <summary>
    /// El campo Código de Iglesia es requerido.
    /// </summary>
    public const string ChurchCodeRequired = "CHURCH_CODE_REQUIRED";

    /// <summary>
    /// El campo Identificador Único de la Solicitud es requerido.
    /// </summary>
    public const string RequestIdRequired = "REQUEST_ID_REQUIRED";

    /// <summary>
    /// El campo Tipo de Reporte es requerido.
    /// </summary>
    public const string ReportTypeRequired = "REPORT_TYPE_REQUIRED";

    /// <summary>
    /// El campo Versión del Contrato debe ser mayor a 0.
    /// </summary>
    public const string ContractVersionGtZero = "CONTRACT_VERSION_GT_ZERO";

    /// <summary>
    /// El campo Parámetros debe ser un objeto válido.
    /// </summary>
    public const string ParametersObject = "PARAMETERS_OBJECT";

    /// <summary>
    /// El campo Identificador Único del Trabajo es requerido.
    /// </summary>
    public const string JobIdRequired = "JOB_ID_REQUIRED";

    /// <summary>
    /// El campo Tamaño de Página debe ser mayor a 0.
    /// </summary>
    public const string PageSizeGtZero = "PAGE_SIZE_GT_ZERO";

    /// <summary>
    /// El campo Identificador Único del Archivo es requerido.
    /// </summary>
    public const string ArtifactIdRequired = "ARTIFACT_ID_REQUIRED";

    /// <summary>
    /// El campo Identificador Único del Intento es requerido.
    /// </summary>
    public const string AttemptIdRequired = "ATTEMPT_ID_REQUIRED";

    /// <summary>
    /// El campo Credencial del Intento es requerido.
    /// </summary>
    public const string LeaseTokenRequired = "LEASE_TOKEN_REQUIRED";

    /// <summary>
    /// El campo Nombre del Archivo es requerido.
    /// </summary>
    public const string NameRequired = "NAME_REQUIRED";

    /// <summary>
    /// El campo Tamaño del Archivo debe ser mayor a 0.
    /// </summary>
    public const string SizeBytesGtZero = "SIZE_BYTES_GT_ZERO";

    /// <summary>
    /// El campo Hash del Archivo es requerido.
    /// </summary>
    public const string Sha256Required = "SHA256_REQUIRED";

    /// <summary>
    /// El campo Archivos es requerido.
    /// </summary>
    public const string ArtifactIdsRequired = "ARTIFACT_IDS_REQUIRED";

    /// <summary>
    /// El campo Fecha de Datos es requerido.
    /// </summary>
    public const string DataAsOfRequired = "DATA_AS_OF_REQUIRED";

    /// <summary>
    /// El campo Código de Error es requerido.
    /// </summary>
    public const string CodeRequired = "CODE_REQUIRED";

    /// <summary>
    /// El campo ReportType debe tener como máximo 100 caracteres.
    /// </summary>
    public const string ReportTypeMaxLength = "REPORT_TYPE_MAX_LENGTH";

    /// <summary>
    /// El campo LeaseToken debe tener como máximo 64 caracteres.
    /// </summary>
    public const string LeaseTokenMaxLength = "LEASE_TOKEN_MAX_LENGTH";

    /// <summary>
    /// El campo Name debe tener como máximo 50 caracteres.
    /// </summary>
    public const string NameMaxLength = "NAME_MAX_LENGTH";

    /// <summary>
    /// El campo Sha256 debe tener como máximo 64 caracteres.
    /// </summary>
    public const string Sha256MaxLength = "SHA256_MAX_LENGTH";

    /// <summary>
    /// El campo Code debe tener como máximo 100 caracteres.
    /// </summary>
    public const string CodeMaxLength = "CODE_MAX_LENGTH";

    /// <summary>
    /// El campo Cursor debe tener como máximo 128 caracteres.
    /// </summary>
    public const string CursorMaxLength = "CURSOR_MAX_LENGTH";

    /// <summary>
    /// Límite superior de tamaño de página.
    /// </summary>
    public const string PageSizeMaximum = "PAGE_SIZE_MAXIMUM";

    /// <summary>
    /// Límite superior de versión del contrato.
    /// </summary>
    public const string ContractVersionMaximum = "CONTRACT_VERSION_MAXIMUM";

    /// <summary>
    /// Rango admitido de tamaño de página.
    /// </summary>
    public const string PageSizeRange = "PAGE_SIZE_RANGE";

    /// <summary>
    /// Rango admitido de versión del contrato.
    /// </summary>
    public const string ContractVersionRange = "CONTRACT_VERSION_RANGE";

}
