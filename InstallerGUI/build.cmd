@echo off
setlocal enableextensions enabledelayedexpansion
cd /d "%~dp0"
call env\Scripts\activate.bat
pyinstaller -y pyinstaller.spec
