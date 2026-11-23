namespace DigitalProductPass.Services;

/// <summary>
/// Tiny EN / TR / DE localiser. Language comes from the "dpp.lang" cookie, then Accept-Language, then English.
/// Usage in views: @T["nav.scan"]; with arguments: @T.F("msg.issued", 5).
/// </summary>
public class UiText
{
    public const string CookieName = "dpp.lang";
    public static readonly string[] Supported = { "en", "tr", "de" };

    private readonly IHttpContextAccessor _accessor;
    private string? _lang;

    public UiText(IHttpContextAccessor accessor) => _accessor = accessor;

    public string Lang => _lang ??= Resolve();

    public string this[string key]
    {
        get
        {
            if (!Map.TryGetValue(key, out var v)) return key;
            var i = Lang switch { "tr" => 1, "de" => 2, _ => 0 };
            return string.IsNullOrEmpty(v[i]) ? v[0] : v[i];
        }
    }

    public string F(string key, params object[] args) => string.Format(this[key], args);

    private string Resolve()
    {
        var ctx = _accessor.HttpContext;
        if (ctx is null) return "en";

        if (ctx.Request.Cookies.TryGetValue(CookieName, out var c) && Supported.Contains(c)) return c!;

        var header = ctx.Request.Headers.AcceptLanguage.ToString();
        foreach (var part in header.Split(','))
        {
            var tag = part.Split(';')[0].Trim().ToLowerInvariant();
            if (tag.Length >= 2 && Supported.Contains(tag[..2])) return tag[..2];
        }
        return "en";
    }

    // ---- Translations: key → [EN, TR, DE] ----
    private static readonly Dictionary<string, string[]> Map = Build();

    private static Dictionary<string, string[]> Build()
    {
        var m = new Dictionary<string, string[]>();
        void A(string key, string en, string tr, string de) => m[key] = new[] { en, tr, de };

        // Layout / common
        A("brand.title", "Digital Product Passport", "Dijital Ürün Pasaportu", "Digitaler Produktpass");
        A("nav.scan", "Scan", "Tara", "Scannen");
        A("nav.products", "Products", "Ürünler", "Produkte");
        A("nav.brands", "New brand", "Yeni marka", "Neue Marke");
        A("nav.dashboard", "Dashboard", "Panel", "Dashboard");
        A("nav.login", "Store owner login", "Mağaza girişi", "Shop-Login");
        A("nav.logout", "Sign out", "Çıkış yap", "Abmelden");
        A("theme.toggle", "Toggle light / dark mode", "Açık / koyu modu değiştir", "Hell/Dunkel umschalten");
        A("lang.label", "Language", "Dil", "Sprache");
        A("footer.text", "Transparent garments, one scan away.", "Şeffaf giysiler, tek tarama uzağınızda.", "Transparente Kleidung, nur einen Scan entfernt.");
        A("common.yes", "Yes", "Evet", "Ja");
        A("common.cancel", "Cancel", "Vazgeç", "Abbrechen");
        A("common.close", "Close", "Kapat", "Schließen");
        A("common.na", "Not declared", "Beyan edilmedi", "Nicht angegeben");

        // Home
        A("home.eyebrow", "Digital Product Passport", "Dijital Ürün Pasaportu", "Digitaler Produktpass");
        A("home.title", "Know exactly what you are wearing.", "Ne giydiğinizi tam olarak bilin.", "Wissen, was Sie wirklich tragen.");
        A("home.lead", "Scan the QR code on a garment to see its materials, carbon and water footprint, where it was made and how to care for it, repair it and recycle it.",
            "Bir giysinin üzerindeki QR kodu tarayın; malzemelerini, karbon ve su ayak izini, nerede üretildiğini ve nasıl bakım, onarım ve geri dönüşüm yapılacağını görün.",
            "Scannen Sie den QR-Code auf dem Kleidungsstück und sehen Sie Materialien, CO₂- und Wasserfußabdruck, Herkunft sowie Pflege-, Reparatur- und Recyclinghinweise.");
        A("home.scanCta", "Scan a QR code", "QR kod tara", "QR-Code scannen");
        A("home.storeCta", "Manage products", "Ürünleri yönet", "Produkte verwalten");
        A("home.loginCta", "Store owner login", "Mağaza girişi", "Shop-Login");
        A("home.f1.t", "Material carbon index", "Malzeme karbon indeksi", "Material-CO₂-Index");
        A("home.f1.d", "Every fibre carries its own kg CO₂e per kg, weighted into one honest footprint.", "Her lif kendi kg CO₂e/kg değerini taşır; tek ve dürüst bir ayak izinde ağırlıklandırılır.", "Jede Faser hat ihren eigenen Wert in kg CO₂e pro kg, gewichtet zu einem ehrlichen Fußabdruck.");
        A("home.f2.t", "Unique variant SKUs", "Varyanta özel benzersiz SKU", "Eindeutige Varianten-SKUs");
        A("home.f2.d", "Colour, size and batch are baked into a serial that exists only once.", "Renk, beden ve parti, yalnızca bir kez bulunan bir seri koduna işlenir.", "Farbe, Größe und Charge stecken in einer Seriennummer, die es nur einmal gibt.");
        A("home.f3.t", "GS1 Digital Link QR", "GS1 Digital Link QR", "GS1-Digital-Link-QR");
        A("home.f3.d", "Standards-based links that work with any phone camera and any retailer.", "Her telefon kamerasıyla ve her perakendeciyle çalışan standart tabanlı bağlantılar.", "Standardbasierte Links, die mit jeder Handykamera und bei jedem Händler funktionieren.");

        // Login
        A("login.title", "Store owner login", "Mağaza sahibi girişi", "Shop-Login");
        A("login.lead", "Sign in to create products, issue variant SKUs and revoke items.", "Ürün oluşturmak, varyant SKU üretmek ve ürünleri iptal etmek için giriş yapın.", "Melden Sie sich an, um Produkte anzulegen, Varianten-SKUs auszugeben und Artikel zu widerrufen.");
        A("login.username", "Username", "Kullanıcı adı", "Benutzername");
        A("login.password", "Password", "Şifre", "Passwort");
        A("login.remember", "Keep me signed in", "Oturumum açık kalsın", "Angemeldet bleiben");
        A("login.submit", "Sign in", "Giriş yap", "Anmelden");
        A("login.invalid", "Wrong username or password.", "Kullanıcı adı veya şifre hatalı.", "Benutzername oder Passwort ist falsch.");
        A("denied.title", "Store owners only", "Yalnızca mağaza sahipleri", "Nur für Shop-Inhaber");
        A("denied.text", "Your account does not have access to this page.", "Hesabınızın bu sayfaya erişimi yok.", "Ihr Konto hat keinen Zugriff auf diese Seite.");

        // Scan
        A("scan.title", "Scan a product", "Ürün tara", "Produkt scannen");
        A("scan.camera", "Camera", "Kamera", "Kamera");
        A("scan.hint", "Point the camera at the QR code on the label.", "Kamerayı etiketteki QR koda doğrultun.", "Richten Sie die Kamera auf den QR-Code auf dem Etikett.");
        A("scan.start", "Start camera", "Kamerayı aç", "Kamera starten");
        A("scan.active", "Camera is active…", "Kamera etkin…", "Kamera ist aktiv …");
        A("scan.noCamera", "No camera access. Allow it (HTTPS required) or type the code instead.", "Kameraya erişilemiyor. İzin verin (HTTPS gerekir) veya kodu elle girin.", "Kein Kamerazugriff. Erlauben Sie ihn (HTTPS nötig) oder geben Sie den Code ein.");
        A("scan.manual", "Enter code manually", "Kodu elle gir", "Code manuell eingeben");
        A("scan.codeLabel", "Serial number or passport link", "Seri numarası veya pasaport bağlantısı", "Seriennummer oder Pass-Link");
        A("scan.submit", "Show passport", "Pasaportu göster", "Pass anzeigen");

        // Messages
        A("msg.enterCode", "Enter or scan a code.", "Bir kod girin veya tarayın.", "Code eingeben oder scannen.");
        A("msg.codeNotFound", "We couldn't find a product with that code.", "Bu kodla bir ürün bulamadık.", "Zu diesem Code wurde kein Produkt gefunden.");
        A("msg.matInvalid", "A material needs a name and a share between 0 and 100.", "Malzemenin adı ve 0 ile 100 arasında bir oranı olmalı.", "Ein Material braucht einen Namen und einen Anteil zwischen 0 und 100.");
        A("msg.matOver", "Material shares can't exceed 100% (currently {0}%).", "Malzeme oranları %100'ü aşamaz (şu an %{0}).", "Materialanteile dürfen 100 % nicht überschreiten (aktuell {0} %).");
        A("msg.matAdded", "Material added.", "Malzeme eklendi.", "Material hinzugefügt.");
        A("msg.matRemoved", "Material removed.", "Malzeme silindi.", "Material entfernt.");
        A("msg.stepInvalid", "Enter a facility and a country.", "Tesis ve ülke girin.", "Betrieb und Land angeben.");
        A("msg.countRange", "Quantity must be between 1 and 500.", "Adet 1 ile 500 arasında olmalı.", "Die Anzahl muss zwischen 1 und 500 liegen.");
        A("msg.diffInvalid", "Differentiation code must be 2-6 letters or digits.", "Ayırt edici kod 2-6 harf veya rakam olmalı.", "Der Differenzierungscode muss aus 2-6 Buchstaben oder Ziffern bestehen.");
        A("msg.batchInvalid", "Batch number: up to 20 letters, digits or hyphens.", "Parti no: en fazla 20 harf, rakam veya tire.", "Chargennummer: bis zu 20 Buchstaben, Ziffern oder Bindestriche.");
        A("msg.issued", "Issued {0} variants with unique serial SKUs.", "{0} varyant benzersiz seri SKU ile üretildi.", "{0} Varianten mit eindeutigen Serien-SKUs ausgegeben.");
        A("msg.revoked", "Item {0} has been revoked.", "{0} kodlu ürün iptal edildi.", "Artikel {0} wurde widerrufen.");
        A("msg.gtinInvalid", "Invalid GTIN (check digit does not match).", "Geçersiz GTIN (kontrol hanesi uyuşmuyor).", "Ungültige GTIN (Prüfziffer stimmt nicht).");
        A("msg.skuDup", "This brand already has a product with that SKU.", "Bu markada aynı SKU'ya sahip bir ürün var.", "Diese Marke hat bereits ein Produkt mit dieser SKU.");

        // Products
        A("prod.title", "Products", "Ürünler", "Produkte");
        A("prod.new", "+ New product", "+ Yeni ürün", "+ Neues Produkt");
        A("prod.name", "Name", "Ad", "Name");
        A("prod.brand", "Brand", "Marka", "Marke");
        A("prod.sku", "SKU", "SKU", "SKU");
        A("prod.variants", "Variants", "Varyantlar", "Varianten");
        A("prod.manage", "Manage", "Yönet", "Verwalten");
        A("prod.none", "No products yet.", "Henüz ürün yok.", "Noch keine Produkte.");
        A("prod.back", "← All products", "← Tüm ürünler", "← Alle Produkte");
        A("prod.createTitle", "New product", "Yeni ürün", "Neues Produkt");
        A("prod.create", "Create product", "Ürünü oluştur", "Produkt anlegen");
        A("prod.pick", "— choose —", "— seçin —", "— wählen —");
        A("prod.newBrand", "+ New brand", "+ Yeni marka", "+ Neue Marke");

        A("f.brand", "Brand", "Marka", "Marke");
        A("f.name", "Name", "Ad", "Name");
        A("f.sku", "Product SKU", "Ürün SKU", "Produkt-SKU");
        A("f.diff", "Differentiation code", "Ayırt edici kod", "Differenzierungscode");
        A("f.diff.hint", "2-6 characters, first block of every variant serial.", "2-6 karakter; her varyant seri kodunun ilk bloğu.", "2-6 Zeichen, erster Block jeder Varianten-Seriennummer.");
        A("f.gtin", "GTIN (EAN/UPC)", "GTIN (EAN/UPC)", "GTIN (EAN/UPC)");
        A("f.gtin.hint", "Optional. Enables a real GS1 Digital Link in the QR code.", "İsteğe bağlı. QR kodda gerçek bir GS1 Digital Link sağlar.", "Optional. Aktiviert einen echten GS1 Digital Link im QR-Code.");
        A("f.category", "Category", "Kategori", "Kategorie");
        A("f.desc", "Description", "Açıklama", "Beschreibung");
        A("f.image", "Image URL", "Görsel URL", "Bild-URL");
        A("f.weight", "Garment weight (kg)", "Giysi ağırlığı (kg)", "Gewicht des Kleidungsstücks (kg)");
        A("f.weight.hint", "Used to turn the material carbon index into kg CO₂e.", "Malzeme karbon indeksini kg CO₂e'ye çevirmek için kullanılır.", "Wandelt den Material-CO₂-Index in kg CO₂e um.");
        A("f.carbon", "Declared lifecycle footprint (kg CO₂e)", "Beyan edilen yaşam döngüsü ayak izi (kg CO₂e)", "Angegebener Lebenszyklus-Fußabdruck (kg CO₂e)");
        A("f.water", "Water use (litres)", "Su kullanımı (litre)", "Wasserverbrauch (Liter)");
        A("f.recycl", "Recyclability score (0-100)", "Geri dönüştürülebilirlik puanı (0-100)", "Recyclingfähigkeit (0-100)");
        A("f.repair", "Repairability index (0-10)", "Onarılabilirlik indeksi (0-10)", "Reparierbarkeit (0-10)");
        A("f.wash", "Wash instructions", "Yıkama talimatları", "Waschanleitung");
        A("f.eol", "End-of-life guidance", "Ömür sonu yönlendirmesi", "Hinweise zum Lebensende");

        A("mat.title", "Materials", "Malzemeler", "Materialien");
        A("mat.total", "total {0}%", "toplam %{0}", "gesamt {0} %");
        A("mat.mustBe100", "Shares must add up to exactly 100% to calculate the carbon footprint.", "Karbon ayak izini hesaplamak için oranlar tam olarak %100 etmelidir.", "Die Anteile müssen genau 100 % ergeben, damit der CO₂-Fußabdruck berechnet wird.");
        A("mat.name", "Material", "Malzeme", "Material");
        A("mat.pct", "Share %", "Oran %", "Anteil %");
        A("mat.origin", "Origin", "Menşei", "Herkunft");
        A("mat.carbon", "kg CO₂e / kg", "kg CO₂e / kg", "kg CO₂e / kg");
        A("mat.cert", "Certificate code", "Sertifika kodu", "Zertifikatscode");
        A("mat.recycled", "Recycled", "Geri dönüştürülmüş", "Recycelt");
        A("mat.add", "Add material", "Malzeme ekle", "Material hinzufügen");
        A("mat.remove", "Remove", "Sil", "Entfernen");
        A("mat.none", "None added.", "Eklenmedi.", "Keine hinzugefügt.");
        A("mat.weighted", "Weighted carbon index", "Ağırlıklı karbon indeksi", "Gewichteter CO₂-Index");
        A("mat.footprint", "Calculated footprint", "Hesaplanan ayak izi", "Berechneter Fußabdruck");
        A("mat.needWeight", "Add a garment weight to the product to get kg CO₂e.", "kg CO₂e için ürüne giysi ağırlığı ekleyin.", "Gewicht des Kleidungsstücks ergänzen, um kg CO₂e zu erhalten.");
        A("mat.needIndices", "Every material needs a carbon index.", "Her malzemenin bir karbon indeksi olmalı.", "Jedes Material braucht einen CO₂-Index.");

        A("chain.title", "Supply chain", "Tedarik zinciri", "Lieferkette");
        A("chain.facility", "Facility / factory", "Tesis / fabrika", "Betrieb / Fabrik");
        A("chain.country", "Country", "Ülke", "Land");
        A("chain.add", "Add step", "Adım ekle", "Schritt hinzufügen");
        A("chain.none", "No steps yet.", "Henüz adım yok.", "Noch keine Schritte.");

        A("var.title", "Variants & QR codes", "Varyantlar ve QR kodları", "Varianten & QR-Codes");
        A("var.issue", "Issue variant SKUs", "Varyant SKU üret", "Varianten-SKUs ausgeben");
        A("var.count", "Quantity", "Adet", "Anzahl");
        A("var.size", "Size", "Beden", "Größe");
        A("var.color", "Colour", "Renk", "Farbe");
        A("var.diff", "Differentiation code", "Ayırt edici kod", "Differenzierungscode");
        A("var.batch", "Batch no.", "Parti no.", "Chargen-Nr.");
        A("var.serial", "Serial SKU", "Seri SKU", "Serien-SKU");
        A("var.preview", "Serial format: CODE-COL-SIZE-RANDOM", "Seri biçimi: KOD-REN-BEDEN-RASTGELE", "Seriennummer-Format: CODE-FAR-GRÖSSE-ZUFALL");
        A("var.passport", "Open passport", "Pasaportu aç", "Pass öffnen");
        A("var.revoke", "Revoke", "İptal et", "Widerrufen");
        A("var.revoked", "Revoked", "İptal edildi", "Widerrufen");
        A("var.active", "Active", "Aktif", "Aktiv");
        A("var.confirm", "Revoke this item? Its public passport will show it as withdrawn.", "Bu ürün iptal edilsin mi? Herkese açık pasaportta geri çekilmiş olarak görünür.", "Diesen Artikel widerrufen? Der öffentliche Pass zeigt ihn dann als zurückgezogen an.");
        A("var.none", "No variants yet. Issue the first SKUs above.", "Henüz varyant yok. İlk SKU'ları yukarıdan üretin.", "Noch keine Varianten. Geben Sie oben die ersten SKUs aus.");

        A("stage.RawMaterial", "Raw material", "Hammadde", "Rohstoff");
        A("stage.Spinning", "Spinning", "İplik eğirme", "Spinnen");
        A("stage.Weaving", "Weaving / knitting", "Dokuma / örme", "Weben / Stricken");
        A("stage.Dyeing", "Dyeing", "Boyama", "Färben");
        A("stage.Assembly", "Sewing / assembly", "Dikim / montaj", "Nähen / Montage");
        A("stage.Packaging", "Packaging", "Paketleme", "Verpackung");
        A("stage.Distribution", "Distribution", "Dağıtım", "Vertrieb");

        // Brands
        A("brand.createTitle", "New brand", "Yeni marka", "Neue Marke");
        A("brand.name", "Name", "Ad", "Name");
        A("brand.country", "Country", "Ülke", "Land");
        A("brand.website", "Website", "Web sitesi", "Webseite");
        A("brand.save", "Save", "Kaydet", "Speichern");

        // Dashboard
        A("dash.title", "Dashboard", "Panel", "Dashboard");
        A("dash.products", "Products", "Ürünler", "Produkte");
        A("dash.items", "Variants with QR", "QR'lı varyantlar", "Varianten mit QR");
        A("dash.total", "Total scans", "Toplam tarama", "Scans gesamt");
        A("dash.today", "Today", "Bugün", "Heute");
        A("dash.chart", "Scans in the last 7 days", "Son 7 günde taramalar", "Scans der letzten 7 Tage");
        A("dash.series", "Scans", "Tarama", "Scans");
        A("dash.top", "Most scanned products", "En çok taranan ürünler", "Meistgescannte Produkte");
        A("dash.none", "No scans yet.", "Henüz tarama yok.", "Noch keine Scans.");
        A("dash.recent", "Recent scans", "Son taramalar", "Letzte Scans");
        A("dash.time", "Time (UTC)", "Zaman (UTC)", "Zeit (UTC)");
        A("dash.product", "Product", "Ürün", "Produkt");
        A("dash.code", "Code", "Kod", "Code");

        // Public passport
        A("pp.serial", "Serial", "Seri", "Serie");
        A("pp.size", "Size", "Beden", "Größe");
        A("pp.color", "Colour", "Renk", "Farbe");
        A("pp.batch", "Batch", "Parti", "Charge");
        A("pp.produced", "Produced", "Üretim", "Produziert");
        A("pp.revoked.title", "This item has been revoked", "Bu ürün iptal edildi", "Dieser Artikel wurde widerrufen");
        A("pp.revoked.text", "The store owner withdrew this passport. Do not rely on it for purchase or resale.", "Mağaza sahibi bu pasaportu geri çekti. Satın alma veya yeniden satışta buna güvenmeyin.", "Der Shop-Inhaber hat diesen Pass zurückgezogen. Verlassen Sie sich beim Kauf oder Wiederverkauf nicht darauf.");
        A("pp.showQr", "Show QR code", "QR kodu göster", "QR-Code anzeigen");
        A("pp.impact", "Environmental impact", "Çevresel etki", "Umweltwirkung");
        A("pp.carbon", "Carbon footprint", "Karbon ayak izi", "CO₂-Fußabdruck");
        A("pp.carbon.unit", "kg CO₂e", "kg CO₂e", "kg CO₂e");
        A("pp.carbon.calc", "Weighted from the material mix", "Malzeme karışımından ağırlıklandırıldı", "Aus der Materialmischung gewichtet");
        A("pp.carbon.declared", "Declared by the brand", "Marka tarafından beyan edildi", "Vom Hersteller angegeben");
        A("pp.carbon.life", "Declared lifecycle: {0} kg CO₂e", "Beyan edilen yaşam döngüsü: {0} kg CO₂e", "Angegebener Lebenszyklus: {0} kg CO₂e");
        A("pp.water", "Water use", "Su kullanımı", "Wasserverbrauch");
        A("pp.water.unit", "litres", "litre", "Liter");
        A("pp.recycl", "Recyclability", "Geri dönüştürülebilirlik", "Recyclingfähigkeit");
        A("pp.repair", "Repairability", "Onarılabilirlik", "Reparierbarkeit");
        A("pp.of100", "out of 100", "100 üzerinden", "von 100");
        A("pp.of10", "out of 10", "10 üzerinden", "von 10");
        A("pp.recycled", "Recycled content", "Geri dönüştürülmüş içerik", "Recycelter Anteil");
        A("pp.materials", "Material breakdown", "Malzeme dağılımı", "Materialzusammensetzung");
        A("pp.recycledTag", "recycled", "geri dönüştürülmüş", "recycelt");
        A("pp.origin", "Origin", "Menşei", "Herkunft");
        A("pp.index", "{0} kg CO₂e per kg", "kg başına {0} kg CO₂e", "{0} kg CO₂e pro kg");
        A("pp.certCode", "Certificate", "Sertifika", "Zertifikat");
        A("pp.weighted", "Weighted material carbon index", "Ağırlıklı malzeme karbon indeksi", "Gewichteter Material-CO₂-Index");
        A("pp.weightedNote", "Sum of (share × carbon index) across materials that total 100%.", "Toplamı %100 olan malzemelerde (oran × karbon indeksi) toplamı.", "Summe aus (Anteil × CO₂-Index) über Materialien, die 100 % ergeben.");
        A("pp.chain", "Supply chain", "Tedarik zinciri", "Lieferkette");
        A("pp.care", "Care & washing", "Bakım ve yıkama", "Pflege & Waschen");
        A("pp.eol", "End of life", "Ömür sonu", "Lebensende");
        A("pp.certs", "Certificates", "Sertifikalar", "Zertifikate");
        A("pp.until", "valid until {0}", "{0} tarihine kadar geçerli", "gültig bis {0}");
        A("pp.qr.title", "Product QR code", "Ürün QR kodu", "Produkt-QR-Code");
        A("pp.qr.hint", "Scan with any phone camera to open this passport.", "Bu pasaportu açmak için herhangi bir telefon kamerasıyla tarayın.", "Mit jeder Handykamera scannen, um diesen Pass zu öffnen.");
        A("pp.qr.gs1", "GS1 Digital Link", "GS1 Digital Link", "GS1 Digital Link");
        A("pp.qr.plain", "Passport link", "Pasaport bağlantısı", "Pass-Link");
        A("pp.qr.noGtin", "No GTIN on this product, so the QR uses the standard passport link.", "Bu üründe GTIN yok; QR standart pasaport bağlantısını kullanıyor.", "Für dieses Produkt gibt es keine GTIN, daher nutzt der QR-Code den Standard-Passlink.");
        A("pp.qr.copy", "Copy link", "Bağlantıyı kopyala", "Link kopieren");
        A("pp.qr.copied", "Copied!", "Kopyalandı!", "Kopiert!");
        A("pp.qr.download", "Download", "İndir", "Herunterladen");

        A("nf.title", "Product not found", "Ürün bulunamadı", "Produkt nicht gefunden");
        A("nf.text", "This QR code isn't linked to a product passport.", "Bu QR kod bir ürün pasaportuna bağlı değil.", "Dieser QR-Code gehört zu keinem Produktpass.");
        A("nf.retry", "Try again", "Tekrar dene", "Erneut versuchen");

        return m;
    }
}
