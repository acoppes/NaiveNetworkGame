#!/bin/sh

# UNITY_EXECUTABLE_PATH="/mnt/c/UnityHub/2020.2.0a19/Editor/Unity.exe"
UNITY_EXECUTABLE_PATH="/mnt/c/UnityHub/2020.2.0b2/Editor/Unity.exe"

BUILD_COMMAND="${UNITY_EXECUTABLE_PATH} -buildTarget Linux64 -forgetProjectPath -projectPath Server -logfile Server/Logs/linux-build.log -quit -silent-crashes -batchmode -nographics -executeMethod BuildScript.BuildLinuxServer"

echo ${BUILD_COMMAND}
${BUILD_COMMAND}