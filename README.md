# 🚀 PgSafeExport

**Lightning-fast PostgreSQL export with built-in anonymization**

→ Safely copy production data into dev/test environments in seconds
→ No data leaks. No complex setup. Just one command.

---

## ⚡ Why PgSafeExport?

Working with real data in development is painful:

* ❌ You can’t use production data (PII, GDPR, security risks)
* ❌ `pg_dump` is slow and dumps everything
* ❌ No built-in anonymization
* ❌ Manual scripts = fragile and time-consuming

**PgSafeExport solves this.**

---

## 🔥 Key Features

* ⚡ **Ultra-fast export** via PostgreSQL `COPY`
* 🧵 **Parallel processing** (multi-table export)
* 🔐 **Deterministic anonymization**
* 🎯 **Selective export** (tables, filters, sampling)
* 🧩 **Simple YAML config for masking**
* 🛠 **CLI-first, CI/CD friendly**

---

## 🆚 Why not pg_dump?

| Feature            | pg_dump    | PgSafeExport |
| ------------------ | ---------- | ------------ |
| Speed              | ❌          | ✅            |
| Parallel export    | ⚠️ limited | ✅            |
| Anonymization      | ❌          | ✅            |
| Partial export     | ❌          | ✅            |
| Dev-ready datasets | ❌          | ✅            |

---

## 🚀 Quick Start

```bash
pgsafe export \
  --conn "Host=localhost;Database=mydb;Username=postgres;Password=postgres" \
  --out ./dump \
  --mask mask.yaml
```

---

## 🧠 Example: Masking config

```yaml
tables:
  users:
    email: fake_email
    full_name: fake_name
    phone: fake_phone
```

---

## 🔐 Example: Before / After

**Before:**

```json
{
  "email": "john.doe@gmail.com",
  "name": "John Doe"
}
```

**After:**

```json
{
  "email": "user_a83f91@example.test",
  "name": "Alex Brown"
}
```

👉 Same input → same output (deterministic masking)

---

## 🎯 Partial Export

Export only recent data:

```yaml
tables:
  orders:
    where: "created_at > now() - interval '30 days'"
```

---

## 🧪 Dry Run

Preview what will be masked:

```bash
pgsafe export --dry-run
```

---

## 📦 Output

```text
dump/
  public.users.csv
  public.orders.csv
  pgsafe-report.json
```

---

## 📊 Example Report

```json
{
  "tables": [
    {
      "name": "users",
      "rows": 12000,
      "duration_ms": 850
    }
  ]
}
```

---

## ⚙️ CLI Options

```bash
--conn            PostgreSQL connection string
--out             Output directory
--mask            Path to mask.yaml
--tables          Include only specific tables
--exclude-tables  Exclude tables
--parallel        Number of workers
--dry-run         Preview without export
--zip             Output as zip archive
```

---

## 🧵 Performance

PgSafeExport uses:

* PostgreSQL `COPY TO STDOUT`
* Streaming (no memory overhead)
* Parallel table export

👉 Designed for **millions of rows**

---

## 🎯 Use Cases

* Copy production → development safely
* Generate test datasets
* QA environments
* GDPR-compliant data pipelines
* Local debugging with real-like data

---

## 💡 Roadmap

* [ ] Auto-detect PII columns
* [ ] JSON/JSONB smart masking
* [ ] Direct import (restore)
* [ ] GUI version
* [ ] Cloud sync

---

## 💰 Pro Version (planned)

* GUI interface
* Scheduled exports
* Advanced masking rules
* Team access & audit logs

---

## 🤝 Contributing

PRs are welcome.
If you find this useful — ⭐ the repo.

---

## 📄 License

MIT License
