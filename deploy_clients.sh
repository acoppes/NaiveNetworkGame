#!/bin/sh

export SERVER=gemserk.com
export USER=acoppes

cd Client/Builds/Windows
rm ../NaiveNetworkGame-Windows.zip
zip -r ../NaiveNetworkGame-Windows.zip .
cd -
scp Client/Builds/NaiveNetworkGame-Windows.zip $USER@$SERVER:NaiveNetworkGame/
