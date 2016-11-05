#!/usr/bin/env bash

cd "$(dirname "$0")"
pyinstaller -y pyinstaller.spec
