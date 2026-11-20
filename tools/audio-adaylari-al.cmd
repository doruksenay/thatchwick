@echo off
rem Buluttaki son ses uretiminin adaylarini indirir (.audio_work) ve inceleme sayfasini acar.
rem GitHub CLI (gh) gerekir: winget install GitHub.cli  ve bir kez: gh auth login
rem gh yoksa: GitHub > Actions > "Ses uretimi" > son calisma > Artifacts > ses-adaylari-N indir,
rem           icerigini repo kokundeki .audio_work klasorune ac.
cd /d "%~dp0.."
git pull --rebase
for /f %%i in ('gh run list --workflow "ses-uretimi.yml" --status success --limit 1 --json databaseId --jq ".[0].databaseId"') do set RUN=%%i
if "%RUN%"=="" (echo Basarili calisma bulunamadi. & exit /b 1)
gh run download %RUN% --pattern "ses-adaylari-*" --dir .audio_work_indir
for /d %%d in (.audio_work_indir\ses-adaylari-*) do xcopy /e /y /i "%%d" .audio_work >nul
rmdir /s /q .audio_work_indir
python tools\audio\review.py
