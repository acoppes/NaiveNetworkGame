#!/bin/sh

# START_TIME="$(date +%s)"

# build main linux server, upload and restart
echo "Building server"
sh ./build-server-linux.sh
echo "Stop remote server"
sh ./stop_remote_server.sh
echo "Deploying new server"
sh ./deploy_server.sh
echo "starting remote server instance"
sh ./start_remote_server.sh

# build clients & upload them
sh ./build-client-all.sh

# build windows server & upload both servers
# sh ./build-server-windows.sh
sh ./upload_server_builds.sh

# END_TIME="$(($(date +%s)-$START_TIME))"
# echo "Completed in ${END_TIME} seconds"
