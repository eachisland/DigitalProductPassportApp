namespace DigitalProductPass.Services;

/// <summary>
/// Строи публичния URL, който се кодира в QR кода.
/// Ако "PublicBaseUrl" е зададен в конфигурацията (напр. https://dpp.moyadomain.com
/// или ngrok адрес за тест с телефон), се ползва той, иначе адресът на текущата заявка.
/// </summary>
public class PassportLinkService
{
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _accessor;

    public PassportLinkService(IConfiguration config, IHttpContextAccessor accessor)
    {
        _config = config;
        _accessor = accessor;
    }

    public string For(Guid publicId)
    {
        var configured = _config["PublicBaseUrl"];
        string baseUrl;
        if (!string.IsNullOrWhiteSpace(configured))
        {
            baseUrl = configured.TrimEnd('/');
        }
        else
        {
            var req = _accessor.HttpContext?.Request;
            baseUrl = req is null ? "" : $"{req.Scheme}://{req.Host}";
        }
        return $"{baseUrl}/p/{publicId}";
    }
}
