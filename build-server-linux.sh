#!/bin/sh

UNITY_EXECUTABLE_PATH="/mnt/c/UnityHub/2020.2.0a19/Editor/Unity.exe"
PROJECT_PATH="$(pwd)/Server/"

echo $UNITY_EXECUTABLE_PATH
echo $PROJECT_PATH

# -logfile "${PROJECT_PATH}/Logs/build-linux.txt"
${UNITY_EXECUTABLE_PATH} -buildLinux64Player "Server/Builds/Linux/server.x86_64" -buildTarget Linux64 -forgetProjectPath -projectPath "Server/" -quit -silent-crashes -batchmode -nographics
