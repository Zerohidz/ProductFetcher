# Product Fetcher - C# Edition

Modern C# ile yazılmış Trendyol ürün çekme ve Excel export aracı. Python versiyonundan refactor edilmiştir.

## ✨ Özellikler

- ✅ Pagination ve price range stratejisi ile tüm ürünleri çekme
- ✅ Ürün detayları ve açıklamaları otomatik toplama
- ✅ Kategorilere göre Excel export (şablonlu/şablonsuz)
- ✅ Async/await ile performanslı işlem
- ✅ Native AOT desteği (tek executable)
- ✅ Error handling ve failed products logging

## 🚀 Hızlı Başlangıç

### 1. Hazırlık: Excel Şablonları (Opsiyonel)

Uygulama, `excel_templates` klasörü altındaki şablonları kontrol eder.
- Şablonları bu klasöre koyun: `ProductFetcher/excel_templates/`
- Dosya adı kategori adıyla aynı olmalıdır (örn: `Telefon Tutucu.xlsx`)
- Şablon bulunamazsa standart format kullanılır.

### 2. Çalıştırma

```bash
cd ProductFetcher
dotnet run
```

### 3. Kullanım

Program çalıştığında sizden **Mağaza ID** isteyecektir.
- Mağaza ID'sini girip Enter'a basın (örn: `123456`)
- Program ürünleri çekecek, detaylandıracak ve Excel'e dönüştürecektir.

### 4. Çıktılar

Oluşturulan dosyalar `outputs` klasöründe mağaza adına göre gruplanır:
- Yol: `ProductFetcher/outputs/MağazaAdı_MağazaID/`
- Her kategori için ayrı bir `.xlsx` dosyası oluşturulur.

## 📦 Build

### Debug Build
```bash
dotnet build
```

### Release Build  
```bash
dotnet build -c Release
```

### Native AOT Publish (Tek Executable)
```bash
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishAot=true
```

Çıktı: `bin/Release/net9.0/linux-x64/publish/ProductFetcher.Console`

## 📁 Proje Yapısı

```
ProductFetcher/
├── Models/                 # Data models
│   ├── Product.cs
│   ├── Brand.cs
│   ├── PriceDetails.cs
│   └── ProductDetails.cs
├── Services/              # Business logic
│   ├── ProductFetcherService.cs
│   ├── ProductDetailsService.cs
│   └── ExcelExporterService.cs
├── Utils/                 # Utilities
│   ├── HttpClientHelper.cs
│   ├── HtmlParser.cs
│   ├── TextUtils.cs
│   └── FileHelper.cs
├── Program.cs            # Entry point
└── appsettings.json      # Configuration
```

## 🔧 Kullanılan Teknolojiler

| Amaç | Kütüphane | Neden |
|------|-----------|-------|
| HTTP | HttpClient | Built-in, AOT-friendly |
| Resilience | Polly | Retry, circuit breaker |
| HTML Parse | AngleSharp | Fast, modern |
| Excel | EPPlus | Feature-rich |
| JSON | System.Text.Json | AOT-friendly, fast |
| Config | Microsoft.Extensions.Configuration | Standard |

## 📊 Çıktılar

- `outputs/{MerchantName}_{MerchantId}/` - Excel dosyaları (kategori bazlı)
- `testing/products.json` - Ham ürün verileri 
- `testing/product_details.json` - Detaylı ürün verileri
- `testing/failed_products.json` - Başarısız ürünler (varsa)

## 🎯 Refactor Detayları

Python → C# refactor roadmap'i için `DOCS/CSHARP_REFACTOR_ROADMAP.md` dosyasına bakın.

### Temel Değişiklikler

- `merchant_product_fetcher.py` → `ProductFetcherService.cs`
- `product_details_fetcher.py` → `ProductDetailsService.cs` + `HtmlParser.cs`
- `excel_utils.py` → `ExcelExporterService.cs`
- `header_utils.py` → `HttpClientHelper.cs`
- `models.py` → `Models/*.cs` (record types ile)

## 📝 Commit Geçmişi

1. ✅ Models - Python models.py to C#
2. ✅ HTTP Helper + User Agents
3. ✅ Product Fetcher - Pagination logic
4. ✅ HTML Parser + JSON Extraction
5. ✅ Product Details Service
6-7. ✅ Excel Exporter (template support)
8. ✅ Main Program orchestration  
9. ✅ Configuration + Polly
10. ✅ Native AOT Setup

## ⚡ Performance

- Startup: ~0.5s (AOT)
- Memory: Efficient connection pooling
- Binary size: ~15-50MB (AOT)

## 📜 License

Bu proje sadece eğitim amaçlıdır.
