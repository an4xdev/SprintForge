param (
    [string]$service
)

if (-not $service) {
    Write-Host "Usage: .\build_and_start.ps1 <service-name>"
    exit 1
}

Write-Host "Building service: $service"
docker-compose build $service

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed for service: $service"
    exit $LASTEXITCODE
}

Write-Host "Starting service: $service"
docker-compose up -d --no-deps $service

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to start service: $service"
    exit $LASTEXITCODE
}

Write-Host "Service $service has been built and started successfully."