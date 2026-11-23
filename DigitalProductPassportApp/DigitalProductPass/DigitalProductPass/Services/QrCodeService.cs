using QRCoder;

namespace DigitalProductPass.Services;

public class QrCodeService
{
    public byte[] Png(string content)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data);
        return png.GetGraphic(10);
    }

    public string Svg(string content)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        var svg = new SvgQRCode(data);
        return svg.GetGraphic(10);
    }
}
