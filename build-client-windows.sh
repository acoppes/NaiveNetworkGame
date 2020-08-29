#!/bin/sh

UNITY_EXECUTABLE_PATH="/c/UnityHub/2020.2.0a19/Editor/Unity.exe"

BUILD_COMMAND="${UNITY_EXECUTABLE_PATH} -buildWindows64Player Client/Builds/Windows/ -buildTarget Win64 -forgetProjectPath -projectPath Client/ -quit -silent-crashes -batchmode -nographics -logfile Client/Logs/build-client-windows.log"

echo ${BUILD_COMMAND}
${BUILD_COMMAND}