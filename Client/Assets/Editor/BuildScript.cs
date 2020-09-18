using System.Linq;
using UnityEditor;
using UnityEngine;

public class BuildScript : MonoBehaviour
{
    public static void BuildAndroidPlayer()
    {
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