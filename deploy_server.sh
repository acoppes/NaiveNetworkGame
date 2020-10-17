#!/bin/sh

export SERVER_IP=209.151.153.172

# ssh stop remote server
# ssh acoppes@$SERVER_IP 'nohup ./stop_server.sh > /dev/null 2>&1 &'

# copy everything to server
cp Server/start_server.sh Server/Builds/Linux
cp Server/stop_server.sh Server/Builds/Linux

rsync -avz --progress Server/Builds/Linux/ acoppes@$SERVER_IP:Linux

# ssh restart remote server
# ssh acoppes@$SERVER_IP 'nohup ./start_server.sh > /dev/null 2>&1 &'