@echo off
rem Asset işçisini bir tur çalıştırır: art/queue icindeki uygun isleri isler, sonra cikar.
rem Ayrinti: art/README.md. Ilk calistirmada izin istemleri cikarsa onayla ya da --allowedTools listesine ekle.
rem git yalnizca isin gerektirdigi alt komutlarla acik (zorla push, config, hook yok).
cd /d "%~dp0.."
git pull --rebase
claude -p "art/queue icindeki uygun isleri art/budget.md kurallariyla isle ve ozet rapor ver." --agent pixellab-artist --max-turns 80 --allowedTools "mcp__pixellab" "Read" "Write" "Edit" "Glob" "Grep" "Bash(git status:*)" "Bash(git diff:*)" "Bash(git log:*)" "Bash(git add:*)" "Bash(git commit:*)" "Bash(git pull --rebase:*)" "Bash(git push)" "Bash(git restore:*)" "Bash(git rebase --abort)" "Bash(python:*)" "Bash(cp:*)" "Bash(ls:*)" "Bash(rm art/.worker.lock)"
