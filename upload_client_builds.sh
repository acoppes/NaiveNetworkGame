#!/bin/sh

export SERVER=gemserk.com
export USER=acoppes

export WINDOWS_BUILD=NaiveNetworkGameClient-Windows.zip

echo 'Creating zip for build'
cd Client/Builds/Windows
rm ../${WINDOWS_BUILD}
zip -r ../${WINDOWS_BUILD} .
cd -

echo 'Uploading build to server'
scp Client/Builds/${WINDOWS_BUILD} $USER@$SERVER:NaiveNetworkGame/
