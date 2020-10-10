using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BuildScript : MonoBehaviour
{
    public static void BuildAndroidPlayer()
    {
        var keystorePath = Environment.GetEnvironmentVariable("BUILD_KEY_STORE_PATH");
        if (!string.IsNullOrWhiteSpace(keystorePath))
        {
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystorePath;
            PlayerSettings.Android.keyaliasName = Environment.GetEnvironmentVariable("BUILD_KEY_STORE_ALIAS");
            PlayerSettings.Android.keystorePass = Environment.GetEnvironmentVariable("BUILD_KEY_STORE_PASS");
            PlayerSettings.Android.keyaliasPass = Environment.GetEnvironmentVariable("BUILD_KEY_STORE_ALIAS_PASS");
        }

        var playerOptions = new BuildPlayerOptions
        {
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.CompressWithLz4,
            locationPathName = "Builds/Android/NaiveNetworkGame.apk",
            scenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray()
        };
        BuildPipeline.BuildPlayer(playerOptions);
    }
    
}