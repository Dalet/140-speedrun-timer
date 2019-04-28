#!/usr/bin/env bash

echo "mkbundle start"
echo "$(uname)"

cd "$(dirname "$0")"

extraArgs=""

echo "$(mono --version)"

if [ "$(uname)" == "Darwin" ]; then
	echo "mac"
	platform="macos"
	export PKG_CONFIG_PATH=$PKG_CONFIG_PATH:/usr/lib/pkgconfig:/Library/Frameworks/Mono.framework/Versions/Current/lib/pkgconfig
	export AS="as -arch i386"
	export CC="clang -arch i386 -framework CoreFoundation -lobjc -liconv -mmacosx-version-min=10.6"
	extraArgs="--sdk /Library/Frameworks/Mono.framework/Versions/Current"
else
	echo "linux"
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
mkbundle $extraArgs -L. -v --static -z --deps -o ${binDir}${platform}/speedrun-timer-installer.exe \
	${binDir}windows/speedrun-timer-installer.exe ${binDir}windows/Mono.Cecil.dll

