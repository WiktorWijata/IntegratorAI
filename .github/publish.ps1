param(
    [Parameter(Mandatory)][Alias('P')][string]$Package,
    [Parameter(Mandatory)][Alias('V')][string]$Version
)

$tag = "$Package-v$Version"

Write-Host "Zamierzasz opublikowac: $tag" -ForegroundColor Yellow
$confirm = Read-Host "Kontynuowac? (t/n)"
if ($confirm -ne 't') {
    Write-Host "Anulowano." -ForegroundColor Red
    exit
}

Write-Host "Tworze tag $tag..." -ForegroundColor Cyan
git tag $tag

Write-Host "Wypycham tag na GitHub..." -ForegroundColor Cyan
git push origin $tag

Write-Host "Gotowe! GitHub Actions opublikuje paczk." -ForegroundColor Green
Write-Host "Postep: https://github.com/WiktorWijata/IntegratorAI/actions" -ForegroundColor Yellow
