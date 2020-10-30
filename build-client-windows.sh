#!/bin/sh

UNITY_EXECUTABLE_PATH="/mnt/c/UnityHub/2020.2.0b9/Editor/Unity.exe"

# -logfile Client/Logs/build-client-windows.log
BUILD_COMMAND="${UNITY_EXECUTABLE_PATH} -buildWindows64Player Builds/Windows/NaiveNetworkGame.exe -buildTarget Win64 -forgetProjectPath -projectPath Client -quit -silent-crashes -batchmode -nographics"

echo ${BUILD_COMMAND}
${BUILD_COMMAND}