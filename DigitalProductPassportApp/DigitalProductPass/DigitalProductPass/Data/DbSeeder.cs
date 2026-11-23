using DigitalProductPass.Models;
using DigitalProductPass.Services;

namespace DigitalProductPass.Data;

public static class DbSeeder
{
    private const string DemoSku = "TSH-ORG-001";

    public static void Seed(ApplicationDbContext db)
    {
        if (db.Brands.Any())
        {
            BackfillDemo(db);
            return;
        }

        var brand = new Brand { Name = "Demo Textile", Country = "Türkiye", Website = "https://example.com" };

        var product = new Product
        {
            Brand = brand,
            Name = "Organic Cotton T-Shirt",
            Sku = DemoSku,
            Category = "T-Shirts",
            Description = "A basic tee made from organic cotton with fully traceable origin. (Demo data with illustrative values.)",
            Gtin = "05901234123457",
            DifferentiationCode = "TSHORG",
            WeightKg = 0.2m,
            CarbonFootprintKg = 4.8m,
            WaterUsageLiters = 2700,
            RecyclabilityScore = 62,
            RepairabilityIndex = 6.5m,
            WashInstructions = "Wash at 30°C with similar colours, no bleach, tumble dry low, iron on low heat.",
            EndOfLifeGuidance = "Repair small holes first. Then return to the brand's take-back scheme or a textile collection bin.",
            Materials =
            {
                new Material { Name = "Organic cotton", Percentage = 95, IsRecycled = false, Origin = "Türkiye", CarbonIndexKgPerKg = 3.8m, CertificateCode = "GOTS-DEMO-10432" },
                new Material { Name = "Elastane", Percentage = 5, IsRecycled = true, Origin = "Italy", CarbonIndexKgPerKg = 2.9m, CertificateCode = "GRS-DEMO-77120" }
            },
            SupplyChainSteps =
            {
                new SupplyChainStep { Order = 1, Stage = SupplyChainStage.RawMaterial, FacilityName = "Aegean Cotton Farm", Country = "Türkiye", Date = new DateOnly(2026, 3, 10) },
                new SupplyChainStep { Order = 2, Stage = SupplyChainStage.Spinning, FacilityName = "Denizli Spinning Co.", Country = "Türkiye", Date = new DateOnly(2026, 4, 2) },
                new SupplyChainStep { Order = 3, Stage = SupplyChainStage.Dyeing, FacilityName = "EcoDye Textile", Country = "Türkiye", Date = new DateOnly(2026, 4, 20) },
                new SupplyChainStep { Order = 4, Stage = SupplyChainStage.Assembly, FacilityName = "Plovdiv Sewing Factory", Country = "Bulgaria", Date = new DateOnly(2026, 5, 12) },
                new SupplyChainStep { Order = 5, Stage = SupplyChainStage.Packaging, FacilityName = "Sofia Warehouse", Country = "Bulgaria", Date = new DateOnly(2026, 5, 20) }
            },
            Certificates =
            {
                new ProductCertificate { Name = "GOTS", Issuer = "Global Organic Textile Standard", ValidUntil = new DateOnly(2027, 12, 31) },
                new ProductCertificate { Name = "OEKO-TEX Standard 100", Issuer = "OEKO-TEX", ValidUntil = new DateOnly(2027, 6, 30) }
            }
        };

        var taken = new HashSet<string>();
        foreach (var size in new[] { "S", "M", "L" })
        {
            string code;
            do { code = SerialCodeGenerator.New(product.DifferentiationCode, "White", size); } while (!taken.Add(code));
            product.Items.Add(new ProductItem
            {
                SerialCode = code,
                DifferentiationCode = product.DifferentiationCode,
                Size = size,
                Color = "White",
                BatchNumber = "B2026-05"
            });
        }

        db.Products.Add(product); // the brand is added through the navigation property
        db.SaveChanges();
    }

    /// <summary>
    /// Databases created before the carbon-index upgrade already contain the demo product.
    /// Fill its new columns once, so the public passport has something to show. Never touches other products.
    /// </summary>
    private static void BackfillDemo(ApplicationDbContext db)
    {
        var demo = db.Products.FirstOrDefault(p => p.Sku == DemoSku);
        if (demo is null || demo.WaterUsageLiters is not null || demo.DifferentiationCode is not null) return;

        demo.Gtin = "05901234123457";
        demo.DifferentiationCode = "TSHORG";
        demo.WeightKg = 0.2m;
        demo.WaterUsageLiters = 2700;
        demo.RecyclabilityScore = 62;
        demo.RepairabilityIndex = 6.5m;
        demo.WashInstructions = demo.CareInstructions;
        demo.EndOfLifeGuidance = demo.RecyclingInstructions;

        foreach (var m in db.Materials.Where(m => m.ProductId == demo.Id && m.CarbonIndexKgPerKg == null))
        {
            if (m.Percentage == 95) { m.CarbonIndexKgPerKg = 3.8m; m.CertificateCode = "GOTS-DEMO-10432"; }
            else if (m.Percentage == 5) { m.CarbonIndexKgPerKg = 2.9m; m.CertificateCode = "GRS-DEMO-77120"; }
        }

        foreach (var item in db.ProductItems.Where(i => i.ProductId == demo.Id && i.DifferentiationCode == null))
        {
            item.DifferentiationCode = "TSHORG";
            item.BatchNumber ??= "B2026-05";
        }
        db.SaveChanges();
    }
}
