param(
    [Parameter(Mandatory)][Alias('P')][string]$Package,
    [Parameter(Mandatory)][Alias('V')][string]$Version
)

$tag = "$Package-v$Version"

Write-Host "Tworze tag $tag..." -ForegroundColor Cyan
git tag $tag

Write-Host "Wypycham tag na GitHub..." -ForegroundColor Cyan
git push origin $tag

Write-Host "Gotowe! GitHub Actions opublikuje paczkę." -ForegroundColor Green
Write-Host "Postep: https://github.com/WiktorWijata/IntegratorAI/actions" -ForegroundColor Yellow
