# Quick check that the API container is reachable on host port 44397.
# Run from repo root after: docker-compose up -d

Write-Host "Containers:" -ForegroundColor Cyan
docker-compose ps

Write-Host "`nAPI listening on 8080 inside container?" -ForegroundColor Cyan
docker-compose exec api sh -c "wget -q -O- http://127.0.0.1:8080/health-status 2>&1 || echo 'wget failed'"

Write-Host "`nFrom host (port 44397):" -ForegroundColor Cyan
try {
    $r = Invoke-WebRequest -Uri "http://127.0.0.1:44397/health-status" -UseBasicParsing -TimeoutSec 5
    Write-Host "OK Status: $($r.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "FAIL: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Try: docker-compose logs api" -ForegroundColor Yellow
}
