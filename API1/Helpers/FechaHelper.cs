namespace API1.Helpers;

/// <summary>
/// Helper para la gestión centralizada de la zona horaria oficial de Bolivia (UTC-4).
/// Utiliza TimeZoneInfo para convertir con precisión la hora UTC a la hora boliviana,
/// funcionando de forma transparente en servidores Windows y Linux.
/// </summary>
public static class FechaHelper
{
    private static readonly TimeZoneInfo ZonaHorariaBolivia = InicializarZonaBolivia();

    private static TimeZoneInfo InicializarZonaBolivia()
    {
        try
        {
            // Windows ID oficial para Bolivia (La Paz, Georgetown, San Juan)
            return TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                // Linux / IANA ID oficial
                return TimeZoneInfo.FindSystemTimeZoneById("America/La_Paz");
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback seguro universal UTC-4
                return TimeZoneInfo.CreateCustomTimeZone("Bolivia Standard Time", TimeSpan.FromHours(-4), "(UTC-04:00) Bolivia", "Bolivia Time");
            }
        }
    }

    /// <summary>
    /// Retorna la fecha y hora actual en la zona horaria de Bolivia (UTC-4).
    /// </summary>
    public static DateTime AhoraBolivia => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaHorariaBolivia);

    /// <summary>
    /// Convierte una fecha UTC a la hora oficial de Bolivia.
    /// </summary>
    public static DateTime ConvertirABolivia(DateTime fechaUtc)
    {
        var utcDate = fechaUtc.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(fechaUtc, DateTimeKind.Utc)
            : fechaUtc.ToUniversalTime();

        return TimeZoneInfo.ConvertTimeFromUtc(utcDate, ZonaHorariaBolivia);
    }
}
