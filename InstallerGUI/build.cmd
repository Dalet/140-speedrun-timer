@echo off
setlocal enableextensions enabledelayedexpansion
cd /d "%~dp0"
set "ENV_SCRIPT=env\Scripts\activate.bat"
IF EXIST "%ENV_SCRIPT%" call "%ENV_SCRIPT%"
pyinstaller -y pyinstaller.spec
