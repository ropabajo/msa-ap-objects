namespace Ropabajo.Church.Sanluis.Objects.Application.Constants;

/// <summary>
/// Centraliza mensajes de las validaciones de trabajos.
/// </summary>
public static class FileJobValidationMessages
{
    /// <summary>
    /// El campo Código de Iglesia es requerido.
    /// </summary>
    public const string ChurchCodeRequired = "El campo Código de Iglesia es requerido.";

    /// <summary>
    /// El campo Identificador Único de la Solicitud es requerido.
    /// </summary>
    public const string RequestIdRequired = "El campo Identificador Único de la Solicitud es requerido.";

    /// <summary>
    /// El campo Tipo de Reporte es requerido.
    /// </summary>
    public const string ReportTypeRequired = "El campo Tipo de Reporte es requerido.";

    /// <summary>
    /// El campo Versión del Contrato debe ser mayor a 0.
    /// </summary>
    public const string ContractVersionGtZero = "El campo Versión del Contrato debe ser mayor a 0.";

    /// <summary>
    /// El campo Parámetros debe ser un objeto válido.
    /// </summary>
    public const string ParametersObject = "El campo Parámetros debe ser un objeto válido.";

    /// <summary>
    /// El campo Identificador Único del Trabajo es requerido.
    /// </summary>
    public const string JobIdRequired = "El campo Identificador Único del Trabajo es requerido.";

    /// <summary>
    /// El campo Tamaño de Página debe ser mayor a 0.
    /// </summary>
    public const string PageSizeGtZero = "El campo Tamaño de Página debe ser mayor a 0.";

    /// <summary>
    /// El campo Identificador Único del Archivo es requerido.
    /// </summary>
    public const string ArtifactIdRequired = "El campo Identificador Único del Archivo es requerido.";

    /// <summary>
    /// El campo Identificador Único del Intento es requerido.
    /// </summary>
    public const string AttemptIdRequired = "El campo Identificador Único del Intento es requerido.";

    /// <summary>
    /// El campo Credencial del Intento es requerido.
    /// </summary>
    public const string LeaseTokenRequired = "El campo Credencial del Intento es requerido.";

    /// <summary>
    /// El campo Nombre del Archivo es requerido.
    /// </summary>
    public const string NameRequired = "El campo Nombre del Archivo es requerido.";

    /// <summary>
    /// El campo Tamaño del Archivo debe ser mayor a 0.
    /// </summary>
    public const string SizeBytesGtZero = "El campo Tamaño del Archivo debe ser mayor a 0.";

    /// <summary>
    /// El campo Hash del Archivo es requerido.
    /// </summary>
    public const string Sha256Required = "El campo Hash del Archivo es requerido.";

    /// <summary>
    /// El campo Archivos es requerido.
    /// </summary>
    public const string ArtifactIdsRequired = "El campo Archivos es requerido.";

    /// <summary>
    /// El campo Fecha de Datos es requerido.
    /// </summary>
    public const string DataAsOfRequired = "El campo Fecha de Datos es requerido.";

    /// <summary>
    /// El campo Código de Error es requerido.
    /// </summary>
    public const string CodeRequired = "El campo Código de Error es requerido.";

    /// <summary>
    /// El campo Tipo de Reporte debe tener como máximo 100 caracteres.
    /// </summary>
    public const string ReportTypeMaxLength = "El campo Tipo de Reporte debe tener como máximo 100 caracteres.";

    /// <summary>
    /// El campo Credencial del Intento debe tener como máximo 64 caracteres.
    /// </summary>
    public const string LeaseTokenMaxLength = "El campo Credencial del Intento debe tener como máximo 64 caracteres.";

    /// <summary>
    /// El campo Nombre del Archivo debe tener como máximo 50 caracteres.
    /// </summary>
    public const string NameMaxLength = "El campo Nombre del Archivo debe tener como máximo 50 caracteres.";

    /// <summary>
    /// El campo Huella SHA-256 debe tener como máximo 64 caracteres.
    /// </summary>
    public const string Sha256MaxLength = "El campo Huella SHA-256 debe tener como máximo 64 caracteres.";

    /// <summary>
    /// El campo Código del Fallo debe tener como máximo 100 caracteres.
    /// </summary>
    public const string CodeMaxLength = "El campo Código del Fallo debe tener como máximo 100 caracteres.";

    /// <summary>
    /// El campo Cursor debe tener como máximo 128 caracteres.
    /// </summary>
    public const string CursorMaxLength = "El campo Cursor debe tener como máximo 128 caracteres.";

    /// <summary>
    /// Límite superior de tamaño de página.
    /// </summary>
    public const string PageSizeMaximum = "El campo Tamaño de Página debe ser menor o igual a 50.";

    /// <summary>
    /// Límite superior de versión del contrato.
    /// </summary>
    public const string ContractVersionMaximum = "El campo Versión del Contrato debe ser menor o igual a 999.";

    /// <summary>
    /// Rango admitido de tamaño de página.
    /// </summary>
    public const string PageSizeRange = "El campo Tamaño de Página debe estar entre 1 y 50.";

    /// <summary>
    /// Rango admitido de versión del contrato.
    /// </summary>
    public const string ContractVersionRange = "El campo Versión del Contrato debe estar entre 1 y 999.";

}
