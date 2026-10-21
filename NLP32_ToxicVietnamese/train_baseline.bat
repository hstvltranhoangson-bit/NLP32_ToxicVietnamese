@echo off
cd /d "%~dp0"
python -m src.main train --data real --model baseline
pause
