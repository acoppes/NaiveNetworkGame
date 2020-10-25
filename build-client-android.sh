#!/bin/sh

UNITY_EXECUTABLE_PATH="/mnt/c/UnityHub/2020.2.0b8/Editor/Unity.exe"

# -logfile Client/Logs/build-client-windows.log
BUILD_COMMAND="${UNITY_EXECUTABLE_PATH} -buildTarget Android -forgetProjectPath -projectPath Client -logfile Client/Logs/android-build.log -quit -silent-crashes -batchmode -nographics -executeMethod BuildScript.BuildAndroidPlayer"

echo ${BUILD_COMMAND}
${BUILD_COMMAND}