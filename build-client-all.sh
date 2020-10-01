#!/bin/sh

echo "Building client"
time sh ./build-client-windows.sh
time sh ./build-client-android.sh
echo "Deploying client builds"
sh ./upload_client_builds.sh