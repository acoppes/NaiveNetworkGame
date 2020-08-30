#!/bin/sh

# UNITY_EXECUTABLE_PATH="/mnt/c/UnityHub/2020.2.0a19/Editor/Unity.exe"
UNITY_EXECUTABLE_PATH="/mnt/c/UnityHub/2020.2.0a21/Editor/Unity.exe"

# -logfile Server/Logs/build-server-linux.log
BUILD_COMMAND="${UNITY_EXECUTABLE_PATH} -buildTarget Linux64 -forgetProjectPath -projectPath Server -quit -silent-crashes -batchmode -nographics -executeMethod BuildScript.BuildLinuxServer"

echo ${BUILD_COMMAND}
${BUILD_COMMAND}