using System.Linq;
using UnityEditor;
using UnityEngine;

public class BuildScript : MonoBehaviour
{
    public static void BuildLinuxServer()
    {
        var playerOptions = new BuildPlayerOptions
        {
            target = BuildTarget.StandaloneLinux64,
            targetGroup = BuildTargetGroup.Standalone,
            options = BuildOptions.EnableHeadlessMode | 
                      BuildOptions.UncompressedAssetBundle | 
                      BuildOptions.DetailedBuildReport,
            locationPathName = "Builds/Linux/server.x86_64",
            scenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray()
        };
        
        BuildPipeline.BuildPlayer(playerOptions);
    }
    
    public static void BuildWindowsServer()
    {
        var playerOptions = new BuildPlayerOptions
        {
            target = BuildTarget.StandaloneWindows64,
            targetGroup = BuildTargetGroup.Standalone,
            options = BuildOptions.EnableHeadlessMode | 
                      BuildOptions.UncompressedAssetBundle | 
                      BuildOptions.DetailedBuildReport,
            locationPathName = "Builds/Windows/server-x86_64.exe",
            scenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray()
        };
        
        BuildPipeline.BuildPlayer(playerOptions);
    }
}