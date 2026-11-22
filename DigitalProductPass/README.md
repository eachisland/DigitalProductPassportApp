# Digital Product Passport

ASP.NET Core MVC (.NET 10) + EF Core (SQL Server) + QRCoder.

## Пускане
1. Сложи своя connection string в `DigitalProductPass/appsettings.json` (ключ `ConnectionStrings:DefaultConnection`).
2. Стартирай от Visual Studio (F5) или: `cd DigitalProductPass && dotnet run`

При първо стартиране в Development базата се създава сама и се добавят демо данни.

## Тест с телефон
QR кодовете съдържат пълен URL. Задай `"PublicBaseUrl": "https://<ngrok-или-LAN-адрес>"`
в `appsettings.Development.json`. Камерата в браузъра изисква HTTPS (localhost е изключение).

## Маршрути
- `/p/{guid}` публичен паспорт (целта на QR кода)
- `/scan` скенер с камера и ръчно въвеждане на код
- `/qr/{guid}.png|svg` генериране на QR
- `/dashboard`, `/products`, `/brands/create`
