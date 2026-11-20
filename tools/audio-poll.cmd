@echo off
rem Ses kuyrugu yoklayicisi (Project 45). Gorev Zamanlayici 15 dakikada bir calistirir.
rem Hazir isleri uretir, kabul edilenleri oyuna yukseltir. Gunluk: tools\audio-poll.log
setlocal
cd /d "%~dp0.."
echo ==== %date% %time% >> tools\audio-poll.log
git pull --rebase >> tools\audio-poll.log 2>&1
python tools\audio\worker.py --once --git >> tools\audio-poll.log 2>&1
python tools\audio\promote.py --git >> tools\audio-poll.log 2>&1
endlocal
