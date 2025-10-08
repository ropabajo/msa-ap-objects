namespace Ropabajo.Church.Sanluis.Objects.Application.Shared.Enums
{
    public static class BulkLoadResultState
    {
        public const string Observed = "OBSERVADO";
        public const string Processed = "PROCESADO"; //SI ENCUENTRA ERRROR MANDA AL ESTADO OBSERVADO
        public const string Completed = "COMPLETADO";

        public static readonly HashSet<string> ValidCodes =
            new (StringComparer.OrdinalIgnoreCase)
            {
                Observed,
                Processed,
                Completed
            };

    }
}
