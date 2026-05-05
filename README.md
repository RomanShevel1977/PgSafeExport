# 🚀 PgSafeExport
**One command to safely copy production data**

Fast PostgreSQL export with built-in anonymization

**Lightning-fast PostgreSQL export with built-in anonymization**

![License](https://img.shields.io/github/license/RomanShevel1977/PgSafeExport)
![Release](https://img.shields.io/github/v/release/RomanShevel1977/PgSafeExport)
![Language](https://img.shields.io/github/languages/top/RomanShevel1977/PgSafeExport)

→ Safely copy production data into dev/test environments in seconds  
→ No data leaks. No complex setup. Just one command.

---

## ⚡ Why PgSafeExport?

Working with real data in development is painful:

- ❌ You can’t use production data (PII, GDPR, security risks)
- ❌ `pg_dump` creates full dumps without anonymization
- ❌ Manual masking scripts are fragile and time-consuming
- ❌ Hard to create realistic dev/test datasets safely

**PgSafeExport solves this.**

---

## 🔥 Key Features

- ⚡ High-performance export via PostgreSQL COPY
- 🧵 Parallel processing (multi-table export)
- 🔐 Deterministic anonymization
- 🎯 Selective export (tables, filters)
- 🧩 Simple YAML masking configuration
- 🧪 Dry-run mode
- 📦 Optional ZIP output
- 📊 Export report with metrics

---

## 🆚 Why not pg_dump?

| Feature              | pg_dump | PgSafeExport |
|---------------------|--------|-------------|
| Speed               | ⚠️ Good | ✅ Faster for selective export |
| Parallel export     | ⚠️ Limited modes | ✅ Built-in |
| Anonymization       | ❌ | ✅ |
| Partial export      | ❌ | ✅ |
| Dev-ready datasets  | ❌ | ✅ |

> pg_dump is great for backups, but PgSafeExport is designed for safe data cloning into dev/test environments

---

## 🚀 Quick Start

pgsafe export \
  --conn "Host=localhost;Database=mydb;Username=postgres;Password=postgres" \
  --out ./dump \
  --mask mask.yaml

---

## 📦 Installation

Download the latest release:

https://github.com/RomanShevel1977/PgSafeExport/releases

---

## 🧠 Masking Configuration

tables:
  users:
    columns:
      email: fake_email
      full_name: fake_name
      phone: fake_phone

---

## 🔐 Example

Before:
john.doe@gmail.com

After:
user_a83f91@example.test

---

## 🎯 Use Cases

- Safe production → development data cloning
- QA environments
- GDPR workflows

---

## 📄 License

MIT License
