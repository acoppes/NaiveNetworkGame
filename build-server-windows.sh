#!/bin/sh

# UNITY_EXECUTABLE_PATH="/mnt/c/UnityHub/2020.2.0a19/Editor/Unity.exe"
UNITY_EXECUTABLE_PATH="/mnt/c/UnityHub/2020.2.0b2/Editor/Unity.exe"

# -logfile Server/Logs/build-server-linux.log
BUILD_COMMAND="${UNITY_EXECUTABLE_PATH} -buildTarget Win64 -forgetProjectPath -projectPath Server -quit -silent-crashes -batchmode -nographics -executeMethod BuildScript.BuildWindowsServer"

echo ${BUILD_COMMAND}
${BUILD_COMMAND}