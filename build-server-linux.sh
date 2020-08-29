#!/bin/sh

# UNITY_EXECUTABLE_PATH="/mnt/c/UnityHub/2020.2.0a19/Editor/Unity.exe"
UNITY_EXECUTABLE_PATH="/c/UnityHub/2020.2.0a19/Editor/Unity.exe"

BUILD_COMMAND="${UNITY_EXECUTABLE_PATH} -buildLinux64Player Builds/Linux/server.x86_64 -buildTarget Linux64 -forgetProjectPath -projectPath Server/ -quit -silent-crashes -batchmode -nographics -logfile Server/Logs/build-server-linux.log"

echo ${BUILD_COMMAND}
${BUILD_COMMAND}
# tail -f Server/Logs/build-server-linux.log

