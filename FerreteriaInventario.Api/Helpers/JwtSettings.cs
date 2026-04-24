namespace FerreteriaInventario.Api.Helpers;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Key { get; set; } = "CAMBIAR_EN_PRODUCCION";
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 480;
}
