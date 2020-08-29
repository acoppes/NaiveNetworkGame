#!/bin/sh

UNITY_EXECUTABLE_PATH="/c/UnityHub/2020.2.0a19/Editor/Unity.exe"
PROJECT_PATH="$(pwd)/Server"

# -logfile "${PROJECT_PATH}/Logs/build-linux.txt"
${UNITY_EXECUTABLE_PATH} -buildLinux64Player "${PROJECT_PATH}/Builds/Linux/server.x86_64" -buildTarget Linux64 -forgetProjectPath -projectPath "${PROJECT_PATH}" -quit -silent-crashes -batchmode -nographics
