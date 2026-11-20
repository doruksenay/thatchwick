@echo off
rem Ses inceleme sayfasi (http://localhost:8765). Kararlardan sonra kapat (Ctrl+C);
rem kabul edilenler bir sonraki yoklamada oyuna yukseltilir ya da hemen: python tools\audio\promote.py --git
cd /d "%~dp0.."
python tools\audio\review.py
