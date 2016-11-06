#!/usr/bin/env bash

cd "$(dirname "$0")"

if [ "$(uname)" == "Darwin" ]; then
	platform="macos"
	export PKG_CONFIG_PATH=$PKG_CONFIG_PATH:/usr/lib/pkgconfig:/Library/Frameworks/Mono.framework/Versions/Current/lib/pkgconfig
	export AS="as -arch i386"
	export CC="clang -arch i386 -framework CoreFoundation -lobjc -liconv -mmacosx-version-min=10.6"
else
	if [ "$(uname -m)" == 'x86_64' ]; then
		platform="linux_x64"
	else
		platform="linux_x86"
	fi
fi

echo Platform: $platform

binDir="datas/bin/"
mkdir $binDir$platform
mkbundle --static -z --deps -o ${binDir}${platform}/speedrun-timer-installer.exe \
	${binDir}windows/speedrun-timer-installer.exe ${binDir}windows/Mono.Cecil.dll

