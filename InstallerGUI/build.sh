#!/usr/bin/env bash

cd "$(dirname "$0")"
rm -rf build/
pyinstaller -y pyinstaller.spec
