# Retro Diffusion 주인공 2장 생성 → 지표 → PixelLab과 비교 시트 (CharacterPixelRules 10절)
# 실행: PowerShell에서  & C:\Users\user\ProjectB\Tools\pixelfx\rd_hero_compare.ps1
$ErrorActionPreference = 'Stop'
$env:PYTHONIOENCODING = 'utf-8'
$uv = 'C:\Users\user\.local\bin\uv.exe'
$scratch = 'C:\Users\user\AppData\Local\Temp\claude\C--Users-user-ProjectB\8b62ae35-8290-4d53-8ae8-27ae6990900e\scratchpad'
Set-Location 'C:\Users\user\ProjectB'

Write-Host '== 1. Retro Diffusion 견적 후 2장 생성 =='
& $uv run -q --with mcp python "$scratch\rd_run.py" *>&1 | Tee-Object -FilePath "$scratch\rd_run.log"

Write-Host '== 2. 지표 =='
$d = 'Docs/ArtCompare/hero_proto'
python Tools/pixelfx/sprite_metrics.py "$d/rd_1.png" "$d/rd_2.png" "$d/styled1_auto.png" "$d/styled2_flat.png" *>&1 | Tee-Object -FilePath "$scratch\rd_metrics.log"

Write-Host '== 3. 비교 시트 =='
python Tools/pixelfx/compare_sheet.py "$d/rd_vs_pixellab.png" "REF=Docs/ArtCompare/hero_style_anchor_64.png" "RD Pro 1=$d/rd_1.png" "RD Pro 2=$d/rd_2.png" "PixelLab 정리=$d/styled1_auto.png" "PixelLab max16=$d/styled2_flat.png" --cols 5
Write-Host '완료 — Claude에게 "실행했어"라고 알려 주세요.'
