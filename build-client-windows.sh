#!/bin/sh

UNITY_EXECUTABLE_PATH="/c/UnityHub/2020.2.0a19/Editor/Unity.exe"
PROJECT_PATH="$(pwd)/Client"

${UNITY_EXECUTABLE_PATH} -buildWindows64Player "${PROJECT_PATH}/Builds/Windows/" -buildTarget Win64 -forgetProjectPath -projectPath "${PROJECT_PATH}" -quit -silent-crashes -batchmode -nographics -logfile "${PROJECT_PATH}/Logs/build-windows.txt"
