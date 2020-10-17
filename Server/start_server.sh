#/bin/sh

echo "killing previous server instance"
pkill -9 server.x86_64

# wait $!
./Linux/server.x86_64 -port 9000 -logStatistics > output_server1.txt &
./Linux/server.x86_64 -port 9001 -logStatistics > output_server2.txt &
./Linux/server.x86_64 -port 9002 -logStatistics > output_server3.txt &
./Linux/server.x86_64 -port 9003 -logStatistics > output_server4.txt &
./Linux/server.x86_64 -port 9004 -logStatistics > output_server5.txt &

echo "new server instance created"

