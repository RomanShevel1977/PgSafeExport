# PgSafeExport

Fast PostgreSQL exporter with deterministic anonymization for safe dev/test datasets.

## Features

- Fast PostgreSQL export using `COPY TO STDOUT`
- Parallel table export
- Streaming transformation, no `DataTable`, no EF
- Deterministic anonymization
- YAML masking config
- Partial export with per-table `where`
- Include/exclude table filters
- Dry-run export plan
- ZIP output
- Validation report with row count, duration, file size and masked columns
- Basic JSON/JSONB redaction rule

## Install / restore

```bash
dotnet restore
```

## Basic usage

```bash
dotnet run -- export \
  --conn "Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=postgres" \
  --out ./dump \
  --mask ./sample-mask.yaml \
  --parallel 4
```

## Dry run

```bash
dotnet run -- export \
  --conn "Host=localhost;Database=mydb;Username=postgres;Password=postgres" \
  --out ./dump \
  --mask ./sample-mask.yaml \
  --dry-run
```

## ZIP output

```bash
dotnet run -- export \
  --conn "Host=localhost;Database=mydb;Username=postgres;Password=postgres" \
  --out ./dump.zip \
  --mask ./sample-mask.yaml \
  --zip
```

## Select tables

```bash
--tables users,orders,public.customers
--exclude-tables audit_logs,events
```

## Mask rules

Supported MVP rules:

- `fake_email`
- `fake_name`
- `fake_phone`
- `mask_last4`
- `redact`
- `null`
- `json_redact`

## Config example

See `sample-mask.yaml`.

## Notes

This is an MVP skeleton. Before production use, add automated tests, benchmark scripts, and stricter validation for user-provided SQL `where` clauses.
