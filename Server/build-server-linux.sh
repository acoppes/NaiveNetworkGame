#!/bin/sh

UNITY_EXECUTABLE_PATH="/mnt/c/UnityHub/2020.2.0a19/Editor/Unity.exe"
# PROJECT_PATH=$(pwd)
# LOG_FILE=Logs/build-linux.txt 

# -logfile \"${LOG_FILE}\"
# -projectPath \"${PROJECT_PATH}\"

${UNITY_EXECUTABLE_PATH} -buildLinux64Player Builds/Linux -buildTarget Linux64 -forgetProjectPath -quit -silent-crashes -batchmode -nographics 
