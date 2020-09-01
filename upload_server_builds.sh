#!/bin/sh

export SERVER=gemserk.com
export USER=acoppes

echo 'Creating linux zip'
cd Server/Builds/Linux
rm ../NaiveNetworkGame_Server-Linux.zip
zip -r ../NaiveNetworkGame_Server-Linux.zip .
cd -

echo 'Creating windows zip'
cd Server/Builds/Windows
rm ../NaiveNetworkGame_Server-Windows.zip
zip -r ../NaiveNetworkGame_Server-Windows.zip .
cd -

echo 'Uploading builds'
scp Server/Builds/NaiveNetworkGame_Server-Linux.zip $USER@$SERVER:NaiveNetworkGame/
scp Server/Builds/NaiveNetworkGame_Server-Windows.zip $USER@$SERVER:NaiveNetworkGame/
