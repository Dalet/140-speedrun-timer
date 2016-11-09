#!/usr/bin/env python3
# -*- coding: utf-8 -*-

from enum import IntEnum
import os
import pkgutil
import platform
import subprocess
import sys
import tkinter
from tkinter import Tk, N, S, W, E, LEFT, RIGHT, BOTH, BOTTOM, TOP,\
	filedialog, messagebox, StringVar
from tkinter.ttk import Frame, Button, Label, Entry

def data_path(path):
	if hasattr(sys, '_MEIPASS'):
		return os.path.join(sys._MEIPASS, path)
	# below is for when the app isn't bundled into a single executable
	if path.startswith("bin/"):
		p = platform.system().lower()
		if p == "windows":
			folder = "windows"
		elif p == "darwin":
			folder = "macos"
		elif p == "linux":
			is_64bits = sys.maxsize > 2**32
			folder = "linux_x64" if is_64bits else "linux_x86"
		path = path.lstrip("bin/")
		path = os.path.join("bin", folder, path)
	path = os.path.join(os.path.abspath("."), "datas", path)
	return path

class ExitCode(IntEnum):
	Success = 0
	Error = 1
	NoArgs = 2
	InvalidPath = 3
	NotInstalled = 4
	ManualInstallationDetected = 5
	AlreadyDone = 6
	PermissionError = 7

class Form(Frame):

	def __init__(self, parent):
		Frame.__init__(self, parent)

		self.parent = parent
		self.centerWindow()
		self.pack(fill=BOTH, expand=1)
		self.initUI()

		gamePath = find_game()
		self.gamePath.set(gamePath)
		if len(gamePath) > 0:
			self.check_install()

	def centerWindow(self):
		parent = self.parent
		parent.update_idletasks()
		w = parent.winfo_screenwidth()
		h = parent.winfo_screenheight()
		size = tuple(int(_) for _ in parent.geometry().split('+')[0].split('x'))
		x = w/2 - size[0]/2
		y = h/2 - size[1]/2
		parent.geometry("%dx%d+%d+%d" % (size + (x, y)))

	def initUI(self):
		version = get_version()
		version = (" v" + version) if version != None else ""
		self.parent.title("Speedrun Timer Installer" + version)
		self.pack(anchor=N, fill=BOTH, expand=True, padx=5, pady=5)

		label = Label(self, text="Game folder:")
		label.grid(row=0, column=0)

		self.gamePath = StringVar()
		self.gamePath.trace('w', lambda *args: self.gamePath_changed())
		entryGamePath = Entry(self, textvariable=self.gamePath)
		entryGamePath.grid(row=0, column=1, sticky=E+W, padx=5)
		self.columnconfigure(1, weight=1)

		btnBrowse = Button(self, text="Browse...", command=self.btnBrowse_OnClick)
		self.btnBrowse = btnBrowse
		btnBrowse.grid(row=0, column=2)

		btnInstall = Button(self, text="Install", command=self.btnInstall_OnClick)
		self.btnInstall = btnInstall
		btnInstall.grid(row=1, columnspan=3)
		self.rowconfigure(1, weight=1)

	def gamePath_changed(self):
		if len(self.gamePath.get().strip()) == 0:
			self.btnInstall['state'] = tkinter.DISABLED
			return
		self.btnInstall['state'] = tkinter.NORMAL
		self.btnInstall['text'] = "Check folder"

	def btnInstall_OnClick(self):
		self.btnInstall['state'] = tkinter.DISABLED;

		if self.btnInstall['text'] == "Install":
			self.install()
		elif self.btnInstall['text'] == "Uninstall":
			self.uninstall()

		self.check_install()

	def install(self, uninstall=False):
		word = "Install" if not uninstall else "Uninstall"
		exitCode = install(self.gamePath.get(), uninstall)
		if exitCode == ExitCode.Success:
			messagebox.showinfo(word + "ation", word + "ation success.")
			return
		elif exitCode == ExitCode.PermissionError:
			message = "Permission denied.\nRestart the installer as administrator."
		elif exitCode == ExitCode.AlreadyDone:
			message = "Already " + word.lower() + "ed"
		elif exitCode == ExitCode.InvalidPath:
			message = "Invalid path"
		else:
			message = "Unexpected error " + str(exitCode)
		messagebox.showerror(word + "ation error", message)

	def uninstall(self):
		return self.install(True);

	def check_install(self):
		path = self.gamePath.get().strip()
		if len(path) == 0:
			self.btnInstall['state'] = tkinter.DISABLED
			return

		msgTitle = "Speedrun Timer Installer"
		btnText = "Check folder"
		btnState = tkinter.NORMAL

		e = check_install(path)
		if e == ExitCode.Success:
			btnText = "Uninstall"
		elif e == ExitCode.NotInstalled:
			btnText = "Install"
		elif e == ExitCode.InvalidPath:
			btnState = tkinter.DISABLED
			messagebox.showwarning(msgTitle, "Invalid game folder")
		elif e == ExitCode.ManualInstallationDetected:
			messagebox.showwarning(msgTitle, "A manual installation of the mod was detected.\n" \
			+ "This installer requires the game files to be clean. Verify the game files in Steam before trying again.")
		elif e == ExitCode.PermissionError:
			messagebox.showwarning(msgTitle, "Permission denied.\nRestart the installer as administrator.")
		else:
			messagebox.showerror(msgTitle, "Unexpected error " + str(e))

		self.btnInstall['text'] = btnText
		self.btnInstall['state'] = btnState

	def btnBrowse_OnClick(self):
		path = filedialog.askdirectory(mustexist=True, initialdir=self.gamePath.get().strip())
		if len(path.strip()) > 0:
			self.gamePath.set(path)
			self.check_install()

def find_game():
	output, e = commandLine(data_path("bin/speedrun-timer-installer.exe"), "--find-game")
	if e != ExitCode.Success:
		return ""
	return output.decode().strip()

def check_install(path):
	output, e = commandLine(data_path("bin/speedrun-timer-installer.exe"), "--check-install", path)
	return e

def install(path, uninstall=False):
	arg = "--install" if not uninstall else "--uninstall"
	output, e = commandLine(data_path("bin/speedrun-timer-installer.exe"), arg, path)
	return e

def commandLine(*args):
	creationflag = 0
	if platform.system().lower() == "windows":
		creationflag = 0x08000000
	p = subprocess.Popen(args, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, stdin=subprocess.PIPE, creationflags=creationflag)
	output, err = p.communicate()
	return (output, p.returncode)

def get_version():
	try:
		with open(data_path("version.txt"), "r") as f:
			content = f.read()
		return content.strip()
	except:
		return None

if __name__ == '__main__':
	root = Tk()
	root.resizable(0, 0)
	p = platform.system().lower();
	width = 800 if p == "darwin" else 500
	height = 75
	root.geometry(str(width) + "x" + str(height))
	if p == "windows":
		root.iconbitmap(default=data_path("Resources/icon.ico"))
	app = Form(root)
	root.mainloop()
