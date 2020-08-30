#!/bin/sh

export SERVER=gemserk.com
export USER=acoppes

echo 'Creating zip for build'
cd Client/Builds/Windows
rm ../NaiveNetworkGame-Windows.zip
zip -r ../NaiveNetworkGame-Windows.zip .
cd -

echo 'Uploading build to server'
scp Client/Builds/NaiveNetworkGame-Windows.zip $USER@$SERVER:NaiveNetworkGame/
