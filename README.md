# Product Fetcher (C# Edition)

Basit, production-ready C# implementation. Tek proje, Native AOT support.

## 🚀 Features

- **Native AOT**: Single .exe, no .NET runtime needed
- **Simple**: Tek proje, clean code
- **Fast**: Async/await, connection pooling
- **Resilient**: Polly retry policies

## 📋 Requirements

- .NET 9.0 SDK

## 🔨 Build

```bash
cd ProductFetcher
dotnet build
```

### Native AOT
```bash
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishAot=true -o ./dist
```

## 🎯 Usage

```bash
dotnet run
# Mağaza ID: [enter merchant id]
```

## 📖 Docs

- [Python Kod Analizi](DOCS/PYTHON_CODE_ANALYSIS_REPORT.md)
- [C# Roadmap](DOCS/CSHARP_REFACTOR_ROADMAP.md)
