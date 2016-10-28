#!/usr/bin/python
# -*- coding: utf-8 -*-

import pkgutil
from tkinter import *
from tkinter import filedialog, messagebox
from tkinter.ttk import Frame, Button, Style
from PIL import ImageTk

version = "0.1"

def resource_path(path):
    if hasattr(sys, '_MEIPASS'):
        return os.path.join(sys._MEIPASS, path)
    return os.path.join(os.path.abspath("."), path)

class Form(Frame):

	def __init__(self, parent):
		Frame.__init__(self, parent)

		self.parent = parent
		self.centerWindow()
		self.pack(fill=BOTH, expand=1)
		self.initUI()

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
		global version
		self.parent.title("Speedrun Timer Installer v" + version)
		self.pack(anchor=N, fill=BOTH, expand=True, padx=5, pady=5)

		label = Label(self, text="Game folder:")
		label.grid(row=0, column=0)

		self.gamePath = StringVar()
		entryGamePath = Entry(self, textvariable=self.gamePath)
		entryGamePath.grid(row=0, column=1, sticky=E+W, padx=5)
		self.columnconfigure(1, weight=1)

		btnBrowse = Button(self, text="Browse...", command=self.btnBrowse_OnClick)
		self.btnBrowse = btnBrowse
		btnBrowse.grid(row=0, column=2)

		btnInstall = Button(self, text="OK", command=self.btnInstall_OnClick)
		self.btnInstall = btnInstall
		btnInstall.grid(row=1, columnspan=3)
		self.rowconfigure(1, weight=1)

	def btnInstall_OnClick(self):
		messagebox.showerror("error", "oshit")
		self.btnInstall["text"] = ":o"

	def btnBrowse_OnClick(self):
		self.gamePath.set(filedialog.askdirectory(mustexist=True, initialdir=self.gamePath.get()))


if __name__ == '__main__':
	root = Tk()
	root.resizable(0, 0)
	root.geometry("400x75")
	root.iconbitmap(default=resource_path("speedruntimer.ico"))
	app = Form(root)
	root.mainloop()
