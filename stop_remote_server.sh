#!/bin/sh

export SERVER_IP=209.151.153.172

# ssh stop remote server
ssh acoppes@$SERVER_IP 'cd Linux; nohup ./stop_server.sh > /dev/null 2>&1 &'