# -*- mode: python -*-

import os
import platform
import re
import shutil
import subprocess
import sys

block_cipher = None

executableName = "speedrun-timer-installer"
iconFileName = "icon.ico"

p = platform.system().lower()
if p == "windows":
	platform_name = "windows"
elif p == "darwin":
	platform_name = "macos"
	iconFileName = "icon.icns"
elif p == "linux":
	if sys.maxsize > 2**32:
		platform_name = "linux_x64"
		executableName += "_linux.x64"
	else:
		platform_name = "linux_x86"
		executableName += "_linux.x86"


print("copy windows binaries")
mono_installer_path = "../Installer/bin/Release/"
os.makedirs("datas/bin/windows", exist_ok=True)
shutil.copy(os.path.join(mono_installer_path, "speedrun-timer-installer.exe"), "datas/bin/windows/")
shutil.copy(os.path.join(mono_installer_path, "Mono.Cecil.dll"), "datas/bin/windows/")


print("build native executable bundled with mono for the C# installer")
if p == "darwin" or p == "linux":
	subprocess.check_call("./mkbundle.sh")


print("extract version from SharedAssembly.cs and write it to datas/version.txt")
with open("../SharedAssemblyInfo.cs", "r") as f:
	sharedAssemblyInfo = f.read()
match = re.search(r"\[assembly: AssemblyVersion\(\"(.+)\"\)\]", sharedAssemblyInfo)
if match != None:
	with open("datas/version.txt", "w") as f:
		f.write(match.group(1))


data_files = [
	('datas/Resources', os.path.join('Resources/', iconFileName)),
	('datas/bin/' + platform_name, 'bin'),
	('datas/version.txt', '.')
]

print("analysis")
a = Analysis(['InstallerGUI.py'],
             pathex=['.'],
             binaries=None,
             datas=data_files,
             hiddenimports=[],
             hookspath=[],
             runtime_hooks=[],
             excludes=[],
             win_no_prefer_redirects=False,
             win_private_assemblies=False,
             cipher=block_cipher)

print("pyz")
pyz = PYZ(a.pure, a.zipped_data,
             cipher=block_cipher)

print("exe")
exe = EXE(pyz,
          a.scripts,
          a.binaries,
          a.zipfiles,
          a.datas,
          name=executableName,
          icon='datas/Resources/icon.ico',
          debug=False,
          strip=False,
          upx=True,
          console=False,
		  uac_admin=True)

print("macos")
if platform_name == "macos":
	app = BUNDLE(exe,
            a.datas,
            name='speedrun-timer-installer.app',
            icon='datas/Resources/icon.icns',
            bundle_identifier=None)
