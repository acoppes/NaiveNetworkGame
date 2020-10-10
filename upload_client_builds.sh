#!/bin/sh

export SERVER=gemserk.com
export USER=acoppes

export WINDOWS_BUILD=NaiveNetworkGameClient-Windows.zip
# export ANDROID_BUILD=Android/NaiveNetworkGame.apk

echo 'Creating zip for build'
cd Client/Builds/Windows
rm ../${WINDOWS_BUILD}
zip -r ../${WINDOWS_BUILD} .
cd -

echo 'Uploading Windows client build to server'
scp Client/Builds/${WINDOWS_BUILD} $USER@$SERVER:NaiveNetworkGame/

# echo 'Uploading Android client build to server'
# scp Client/Builds/${ANDROID_BUILD} $USER@$SERVER:NaiveNetworkGame/

echo 'Uploading Android client to Firebase App Distribution'
bundle exec fastlane android distribute
