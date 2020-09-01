#!/bin/sh

export SERVER=gemserk.com
export USER=acoppes

export LINUX_BUILD=NaiveNetworkGameServer-Linux.zip
export WINDOWS_BUILD=NaiveNetworkGameServer-Windows.zip

echo 'Creating linux zip'
cd Server/Builds/Linux
rm ../${LINUX_BUILD}
zip -r ../${LINUX_BUILD} .
cd -

echo 'Creating windows zip'
cd Server/Builds/Windows
rm ../${WINDOWS_BUILD}
zip -r ../${WINDOWS_BUILD} .
cd -

echo 'Uploading builds'
scp Server/Builds/${LINUX_BUILD} $USER@$SERVER:NaiveNetworkGame/
scp Server/Builds/${WINDOWS_BUILD} $USER@$SERVER:NaiveNetworkGame/
