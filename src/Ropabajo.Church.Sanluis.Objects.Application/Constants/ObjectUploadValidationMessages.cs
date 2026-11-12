namespace Ropabajo.Church.Sanluis.Objects.Application.Constants;
/// <summary>
/// Mensajes de validación para reservar y confirmar archivos.
/// </summary>
public static class ObjectUploadValidationMessages
{
    /// <summary>
    /// Carpeta requerida.
    /// </summary>
    public const string PathRequired = "El campo Carpeta del Archivo es requerido.";
    /// <summary>
    /// Longitud máxima de carpeta.
    /// </summary>
    public const string PathMaxLength = "El campo Carpeta del Archivo debe tener como máximo 400 caracteres.";
    /// <summary>
    /// Formatos requeridos.
    /// </summary>
    public const string ExtensionsRequired = "El campo Formatos Permitidos es requerido.";
    /// <summary>
    /// Longitud máxima de formatos.
    /// </summary>
    public const string ExtensionsMaxLength = "El campo Formatos Permitidos debe tener como máximo 40 caracteres.";
    /// <summary>
    /// Tamaño requerido.
    /// </summary>
    public const string MaxLengthRequired = "El campo Tamaño Máximo es requerido.";
    /// <summary>
    /// Tamaño positivo.
    /// </summary>
    public const string MaxLengthPositive = "El campo Tamaño Máximo debe ser mayor a 0.";
    /// <summary>
    /// Vigencia requerida.
    /// </summary>
    public const string ExpirationRequired = "El campo Vigencia de la Carga es requerido.";
    /// <summary>
    /// Vigencia positiva.
    /// </summary>
    public const string ExpirationPositive = "El campo Vigencia de la Carga debe ser mayor a 0.";
    /// <summary>
    /// Nombre requerido.
    /// </summary>
    public const string FileNameRequired = "El campo Nombre del Archivo es requerido.";
    /// <summary>
    /// Longitud máxima de nombre.
    /// </summary>
    public const string FileNameMaxLength = "El campo Nombre del Archivo debe tener como máximo 80 caracteres. Renombra el archivo e inténtalo nuevamente.";
    /// <summary>
    /// Identificador requerido.
    /// </summary>
    public const string ObjectCodeRequired = "El campo Identificador Único del Archivo es requerido.";
    /// <summary>
    /// Identificador inválido.
    /// </summary>
    public const string ObjectCodeInvalid = "El campo Identificador Único del Archivo no es válido. Vuelve a seleccionar el archivo.";
}
