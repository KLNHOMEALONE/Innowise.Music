# LightRAG Backup & Reuse Guide

## Backup Strategy

### 1. Source Documents Backup
All source documents live in `Docs/` — already in git. This is the canonical backup.

**Key files:**
- `solution-architecture.md` — Complete system architecture
- `patterns-library.md` — Reusable coding patterns (portable)
- `tasktracker.md` — All completed/in-progress tasks
- `changelog.md` — Full change history
- `project.md` — Project architecture overview
- `music-architecture.md` — Music streaming architecture design
- `admin-dashboard.md` — Admin dashboard architecture
- `admin-dashboard-plan.md` — Implementation plan
- `uploading-tracks.md` — Track upload implementation plan
- `docker-setup.md` — Docker development setup
- `admin-endpoint-verification.md` — API endpoint verification
- `Dashboard_Code_Review.md` — Initial code review
- `Dashboard_Code_Review_Verification.md` — Review verification
- `DashboardAdminCodeReviewV2.md` — V2 code review
- `InnowiseMusicReview.md` — MAUI client code review
- `validation.md` — Validation framework documentation
- `Enterprise-Application-Patterns-Using-.NET-MAUI.pdf` — .NET MAUI patterns book

### 2. LightRAG Data Backup

LightRAG stores processed chunks and vector embeddings in its data directory. To back up:

```bash
# Find LightRAG data directory (check docker-compose or .env)
# Typically: ./lightrag_data or /path/to/lightrag/data

# Backup command
docker cp lightrag:/app/data ./lightrag-backup-$(date +%Y%m%d)

# Or if running natively:
# cp -r /path/to/lightrag/data ./lightrag-backup-$(date +%Y%m%d)
```

### 3. Portable Knowledge Export

The `patterns-library.md` file contains all reusable patterns extracted from the project. This is the portable artifact for use in new projects.

```bash
# Quick export of all LightRAG documents via API
curl -s "http://localhost:9621/documents" | jq -r '.statuses.processed[].file_path' | while read f; do
    echo "## $f" >> lightrag-export-$(date +%Y%m%d).md
    echo "" >> lightrag-export-$(date +%Y%m%d).md
    echo "(Full content available in Docs/$f)" >> lightrag-export-$(date +%Y%m%d).md
    echo "" >> lightrag-export-$(date +%Y%m%d).md
    echo "---" >> lightrag-export-$(date +%Y%m%d).md
    echo "" >> lightrag-export-$(date +%Y%m%d).md
done
```

---

## Reusing in New Projects

### Option A: Copy Patterns Library

1. Copy `Docs/patterns-library.md` to your new project
2. Use as reference for implementing:
   - Validation framework (`ValidatableObject<T>`, `IValidationRule<T>`)
   - MVVM with CommunityToolkit
   - JWT authentication patterns
   - Service layer patterns
   - API design patterns
   - Docker compose setup

### Option B: Migrate LightRAG Knowledge Base

1. Set up LightRAG in the new project:
   ```bash
   git clone https://github.com/HKUDS/LightRAG.git
   cd LightRAG
   docker-compose up -d
   ```

2. Upload the patterns library:
   ```bash
   curl -X POST "http://localhost:9621/documents/upload" \
     -F "file=@path/to/patterns-library.md"
   ```

3. Upload project-specific docs as they're created

### Option C: Shared Knowledge Base

Keep one LightRAG instance with shared knowledge across all projects:

1. Upload `patterns-library.md` to a central LightRAG instance
2. Upload new patterns discovered in each project
3. Query from any project: `POST http://central-lightrag:9621/api/chat`

---

## Automated Backup Script

Save as `scripts/backup-lightrag.sh`:

```bash
#!/bin/bash
# LightRAG Knowledge Base Backup Script

BACKUP_DIR="./backups/lightrag"
DATE=$(date +%Y%m%d_%H%M%S)
mkdir -p "$BACKUP_DIR"

echo "=== LightRAG Backup ==="
echo "Date: $DATE"

# 1. Export document list
curl -s "http://localhost:9621/documents" > "$BACKUP_DIR/document-list-$DATE.json"
echo "Document list saved"

# 2. Export patterns library (if exists)
if [ -f "Docs/patterns-library.md" ]; then
    cp "Docs/patterns-library.md" "$BACKUP_DIR/patterns-library-$DATE.md"
    echo "Patterns library saved"
fi

# 3. Export all Docs
cp -r "Docs/" "$BACKUP_DIR/docs-$DATE/"
echo "All docs saved"

# 4. Backup LightRAG data directory (if accessible)
if [ -d "/path/to/lightrag/data" ]; then
    cp -r "/path/to/lightrag/data" "$BACKUP_DIR/lightrag-data-$DATE/"
    echo "LightRAG data saved"
fi

# 5. Clean old backups (keep last 5)
ls -t "$BACKUP_DIR" | tail -n +6 | xargs -I {} rm -rf "$BACKUP_DIR/{}"
echo "Old backups cleaned (keeping last 5)"

echo "=== Backup Complete ==="
echo "Location: $BACKUP_DIR"
```

Run weekly via cron:
```bash
# crontab -e
0 2 * * 0 /path/to/scripts/backup-lightrag.sh
```

---

## Quick Start for New Project

1. Create `Docs/patterns-library.md` from this project's version
2. Set up LightRAG: `docker run -d -p 9621:9621 hkuds/lightrag`
3. Upload patterns: `curl -X POST http://localhost:9621/documents/upload -F "file=@Docs/patterns-library.md"`
4. Add project-specific docs as the project evolves
5. Query via: `curl -X POST http://localhost:9621/api/chat -H "Content-Type: application/json" -d '{"model":"lightrag","messages":[{"role":"user","content":"What validation patterns should I use?"}],"stream":false}'`
