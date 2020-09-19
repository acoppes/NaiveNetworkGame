#/bin/sh

echo "killing previous server instance"
pkill -9 server.x86_64

# wait $!
./Linux/server.x86_64 -targetFrameRate 60 -logStatistics > server_output.txt &
echo "new server instance created"

