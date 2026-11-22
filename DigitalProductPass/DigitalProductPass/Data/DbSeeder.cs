using DigitalProductPass.Models;
using DigitalProductPass.Services;

namespace DigitalProductPass.Data;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext db)
    {
        if (db.Brands.Any()) return;

        var brand = new Brand { Name = "Demo Textile", Country = "България", Website = "https://example.com" };

        var product = new Product
        {
            Brand = brand,
            Name = "Органична памучна тениска",
            Sku = "TSH-ORG-001",
            Category = "Тениски",
            Description = "Базова тениска от органичен памук, произведена с проследим произход.",
            CarbonFootprintKg = 4.8m,
            CareInstructions = "Пране на 30°C, без белина, гладене на ниска температура.",
            RecyclingInstructions = "Предай в контейнер за текстил или обратно в магазина на марката.",
            Materials =
            {
                new Material { Name = "Органичен памук", Percentage = 95, IsRecycled = false, Origin = "Турция" },
                new Material { Name = "Еластан", Percentage = 5, IsRecycled = true, Origin = "Италия" }
            },
            SupplyChainSteps =
            {
                new SupplyChainStep { Order = 1, Stage = SupplyChainStage.RawMaterial, FacilityName = "Памучна ферма Егейско", Country = "Турция", Date = new DateOnly(2026, 3, 10) },
                new SupplyChainStep { Order = 2, Stage = SupplyChainStage.Spinning, FacilityName = "Denizli Spinning Co.", Country = "Турция", Date = new DateOnly(2026, 4, 2) },
                new SupplyChainStep { Order = 3, Stage = SupplyChainStage.Dyeing, FacilityName = "EcoDye Textile", Country = "Турция", Date = new DateOnly(2026, 4, 20) },
                new SupplyChainStep { Order = 4, Stage = SupplyChainStage.Assembly, FacilityName = "Шивашка фабрика Пловдив", Country = "България", Date = new DateOnly(2026, 5, 12) },
                new SupplyChainStep { Order = 5, Stage = SupplyChainStage.Packaging, FacilityName = "Склад София", Country = "България", Date = new DateOnly(2026, 5, 20) }
            },
            Certificates =
            {
                new ProductCertificate { Name = "GOTS", Issuer = "Global Organic Textile Standard", ValidUntil = new DateOnly(2027, 12, 31) },
                new ProductCertificate { Name = "OEKO-TEX Standard 100", Issuer = "OEKO-TEX", ValidUntil = new DateOnly(2027, 6, 30) }
            }
        };

        var sizes = new[] { "S", "M", "L" };
        var taken = new HashSet<string>();
        foreach (var size in sizes)
        {
            string code;
            do { code = SerialCodeGenerator.New(); } while (!taken.Add(code));
            product.Items.Add(new ProductItem { SerialCode = code, Size = size, Color = "Бяло" });
        }

        db.Products.Add(product); // марката се добавя автоматично през навигацията
        db.SaveChanges();
    }
}
