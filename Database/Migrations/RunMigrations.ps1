# =============================================
# Run Search System SQL Migrations
# Execute in order: 001, 002, 003
# =============================================

$ErrorActionPreference = "Stop"

# Database connection string
$serverName = "localhost"
$databaseName = "TechMartNew"
$username = "sa"
$password = "111111"

# Migration files in order
$migrations = @(
    "001_CreateFullTextIndex.sql",
    "002_CreateSearchStoredProcedures.sql",
    "003_CreateSearchAnalytics.sql"
)

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "Search System SQL Migrations" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

foreach ($migration in $migrations) {
    $filePath = Join-Path $scriptPath $migration
    
    if (-not (Test-Path $filePath)) {
        Write-Host "ERROR: Migration file not found: $migration" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Running migration: $migration" -ForegroundColor Yellow
    Write-Host "---------------------------------------------" -ForegroundColor Gray
    
    try {
        # Run SQL script using sqlcmd
        sqlcmd -S $serverName -d $databaseName -U $username -P $password -i $filePath -b
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "SUCCESS: $migration completed" -ForegroundColor Green
        } else {
            Write-Host "ERROR: $migration failed with exit code $LASTEXITCODE" -ForegroundColor Red
            exit 1
        }
    }
    catch {
        Write-Host "ERROR: Failed to execute $migration" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        exit 1
    }
    
    Write-Host ""
}

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "All migrations completed successfully!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Verify FullText index is working" -ForegroundColor White
Write-Host "2. Test stored procedures with sample data" -ForegroundColor White
Write-Host "3. Build and run the application" -ForegroundColor White
