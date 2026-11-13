@echo off
rem art-poll.cmd'yi Windows Gorev Zamanlayici'ya 15 dakikada bir calisacak sekilde ekler.
rem Kaldirmak icin: schtasks /Delete /TN "Thatchwick Art Worker" /F
schtasks /Create /SC MINUTE /MO 15 /TN "Thatchwick Art Worker" /TR "\"%~dp0art-poll.cmd\"" /F
if errorlevel 1 (
  echo Gorev eklenemedi.
  exit /b 1
)
echo Eklendi: her 15 dakikada bir onayli asset isi varsa uretilir.
