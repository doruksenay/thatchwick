@echo off
rem Asset işçisini bir tur çalıştırır: art/queue icindeki uygun isleri isler, sonra cikar.
rem Ayrinti: art/README.md. Ilk calistirmada izin istemleri cikarsa onayla ya da --allowedTools listesine ekle.
cd /d "%~dp0.."
git pull --rebase
claude -p "art/queue icindeki uygun isleri art/budget.md kurallariyla isle ve ozet rapor ver." --agent pixellab-artist --max-turns 80 --allowedTools "mcp__pixellab" "Read" "Write" "Edit" "Glob" "Grep" "Bash(git:*)" "Bash(python:*)" "Bash(cp:*)" "Bash(ls:*)" "Bash(rm art/.worker.lock)"
