@echo off
rem Yerel yoklayici: origin'de onayli asset isi varsa art-worker.cmd'yi calistirir, yoksa hemen cikar.
rem Claude'u yalnizca is varken baslatir (tools/art/queue_ready.py PixelLab'e dokunmaz, generation harcamaz).
rem Gorev Zamanlayici'ya eklemek icin: tools\art-poll-install.cmd
cd /d "%~dp0.."
git diff --quiet -- art Assets/Art/Iso tools/art
if errorlevel 1 (
  echo Commit edilmemis asset degisikligi var; yoklama atlandi.
  exit /b 0
)
git pull --rebase -q
python tools\art\queue_ready.py --quiet
if errorlevel 1 exit /b 0
call "%~dp0art-worker.cmd"
