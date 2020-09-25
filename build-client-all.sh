#!/bin/sh

echo "Building client"
sh ./build-client-windows.sh
sh ./build-client-android.sh
echo "Deploying client builds"
sh ./upload_client_builds.sh
sh ./upload_server_builds.sh