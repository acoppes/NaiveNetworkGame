#!/bin/sh

echo "Building server"
sh ./build-server-linux.sh
sh ./build-server-windows.sh
echo "Stop remote server"
sh ./stop_remote_server.sh
echo "Deploying new server"
sh ./deploy_server.sh
echo "starting remote server instance"
sh ./start_remote_server.sh
echo "Building client"
sh ./build-client-windows.sh
echo "Deploying client builds"
sh ./upload_client_builds.sh
sh ./upload_server_builds.sh