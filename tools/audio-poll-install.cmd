@echo off
rem Ses yoklayicisini Gorev Zamanlayici'ya 15 dakikada bir calisacak sekilde kurar.
rem Kaldirmak icin: schtasks /delete /tn "Thatchwick Ses Kuyrugu" /f
schtasks /create /f /sc minute /mo 15 /tn "Thatchwick Ses Kuyrugu" /tr "\"%~dp0audio-poll.cmd\""
echo Kuruldu. Elle denemek icin: tools\audio-poll.cmd
