#!/bin/sh

export SERVER_IP=209.151.153.172

# ssh restart remote server
ssh acoppes@$SERVER_IP 'nohup Linux/start_server.sh > /dev/null 2>&1 &'