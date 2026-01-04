# C# Refactor Roadmap - Product Fetcher (SIMPLE VERSION)

**Tarih:** 2026-01-04  
**Hedef:** Python Product Fetcher'ı basit tek-proje C# ile yeniden yazma + Native AOT  
**Prensip:** Keep It Simple, Stupid (KISS)

---

## 🎯 Proje Yapısı (Single Project)

```
ProductFetcher/
├── ProductFetcher.csproj
├── Program.cs                    # Entry point
├── Models/
│   ├── Product.cs
│   ├── Brand.cs
│   ├── PriceDetails.cs
│   └── ProductDetails.cs
├── Services/
│   ├── ProductFetcher.cs         # merchant_product_fetcher.py
│   ├── ProductDetailsService.cs  # product_details_fetcher.py
│   └── ExcelExporter.cs          # excel_utils.py
├── Utils/
│   ├── HttpClientHelper.cs
│   ├── HtmlParser.cs
│   └── FileHelper.cs
└── appsettings.json
```

---

## 📅 Implementation Plan (10 Commits)

### Commit 1: Models (Python models.py → C#)
**Dosyalar:**
- `Models/Product.cs`
- `Models/Brand.cs`
- `Models/PriceDetails.cs`
- `Models/ProductDetails.cs`

**Özellikler:**
- `record` types (immutable)
- Nullable reference types
- JSON serialization attributes

---

### Commit 2: HTTP Helper + User Agents
**Dosyalar:**
- `Utils/HttpClientHelper.cs`

**Python karşılık:**
- `utils/header_utils.py` → HttpClientHelper

**Özellikler:**
- Random User-Agent
- HttpClient singleton
- Base HTTP methods

---

### Commit 3: Product Fetcher (merchant_product_fetcher.py)
**Dosyalar:**
- `Services/ProductFetcher.cs`

**Özellikler:**
- Pagination logic
- Price range strategy
- Async/await
- Progress reporting (Console.WriteLine)

---

### Commit 4: HTML Parser + JSON Extraction
**NuGet:** AngleSharp

**Dosyalar:**
- `Utils/HtmlParser.cs`

**Python karşılık:**
- `product_details_fetcher.py` → HtmlParser
- Regex JSON extraction

---

### Commit 5: Product Details Service
**Dosyalar:**
- `Services/ProductDetailsService.cs`

**Özellikler:**
- `GetProductDetailsAsync()`
- `GetProductDescriptionAsync()`
- Error handling

---

### Commit 6: Excel Exporter (Part 1)
**NuGet:** EPPlus

**Dosyalar:**
- `Services/ExcelExporter.cs`

**Özellikler:**
- Basic Excel generation
- Header mapping
- Product data rows

---

### Commit 7: Excel Exporter (Part 2 - Templates)
**Özellikler:**
- Template support
- Category grouping
- Attribute columns

---

### Commit 8: File Helper + Main Orchestration
**Dosyalar:**
- `Utils/FileHelper.cs`
- `Program.cs` (update)

**Özellikler:**
- Directory management
- JSON save/load
- Main workflow

---

### Commit 9: Configuration + Polly Resilience
**NuGet:**
- Microsoft.Extensions.Configuration.Json
- Microsoft.Extensions.Http.Polly

**Dosyalar:**
- `appsettings.json`
- Update services with retry policies

---

### Commit 10: Native AOT Setup
**Project File Updates:**
```xml
<PublishAot>true</PublishAot>
<InvariantGlobalization>false</InvariantGlobalization>
```

**Build & Test:**
```bash
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishAot=true
```

---

## 🔧 Technology Stack

| Concern | Library | Why |
|---------|---------|-----|
| HTTP | HttpClient | Built-in, AOT-friendly |
| Resilience | Polly | Retry, circuit breaker |
| HTML Parse | AngleSharp | Fast, modern |
| Excel | EPPlus | Feature-rich |
| JSON | System.Text.Json | AOT-friendly, fast |
| Config | Microsoft.Extensions.Configuration | Standard |

---

## 📊 Estimated Time: ~8 hours

| Commit | Time | Cumulative |
|--------|------|------------|
| 1 | 1h | 1h |
| 2 | 30m | 1.5h |
| 3 | 1.5h | 3h |
| 4 | 1h | 4h |
| 5 | 1h | 5h |
| 6 | 1h | 6h |
| 7 | 1h | 7h |
| 8 | 30m | 7.5h |
| 9 | 30m | 8h |
| 10 | 30m | 8.5h |

---

## ✅ Success Criteria

- [ ] Tüm Python features C#'ta implement edildi
- [ ] Native AOT binary < 50MB
- [ ] Startup < 1 second
- [ ] No runtime dependencies (.NET runtime gerektirmiyor)
- [ ] Build warnings = 0

---

**Basit, temiz, production-ready!** 🚀
