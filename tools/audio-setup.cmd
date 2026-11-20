@echo off
rem Ses hatti kurulumu (bir kez). ffmpeg ve Python paketleri.
cd /d "%~dp0.."
where ffmpeg >nul 2>&1 || winget install -e --id Gyan.FFmpeg
python -m pip install -r tools\audio\requirements.txt
if not exist .env (echo ELEVENLABS_API_KEY=buraya_anahtar> .env & echo .env olusturuldu: anahtari yaz.)
python tools\audio\worker.py --list
