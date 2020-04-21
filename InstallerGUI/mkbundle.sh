#!/usr/bin/env bash

cd "$(dirname "$0")"

extraArgs=""

if [ "$(uname)" == "Darwin" ]; then
	platform="macos"
	export PKG_CONFIG_PATH=$PKG_CONFIG_PATH:/usr/lib/pkgconfig:/Library/Frameworks/Mono.framework/Versions/Current/lib/pkgconfig
	export AS="as -arch amd64"
	export CC="clang -arch amd64 -framework CoreFoundation -lobjc -liconv -mmacosx-version-min=10.6"
else
	extraArgs="-L /usr/lib/mono/4.5"
	if [ "$(uname -m)" == 'x86_64' ]; then
		platform="linux_x64"
	else
		platform="linux_x86"
	fi
fi

echo Platform: $platform

binDir="datas/bin/"
mkdir $binDir$platform
mkbundle $extraArgs -L . --static -z --deps -o ${binDir}${platform}/speedrun-timer-installer.exe \
	${binDir}windows/speedrun-timer-installer.exe ${binDir}windows/Mono.Cecil.dll

