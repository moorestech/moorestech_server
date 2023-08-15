using System.IO;
using System.Linq;
using GameConst;
using UnityEditor;
using UnityEngine;


public class BuildWithServer
{
    private const string OutputPathKey = "WindowsBuildOutputPath";
    [MenuItem("moorestech/WindowsBuild")]
    public static void WindowsBuild()
    {
        Pipeline(BuildTarget.StandaloneWindows64);
    }
    [MenuItem("moorestech/LinuxBuild")]
    public static void LinuxBuild()
    {
        Pipeline(BuildTarget.StandaloneLinux64);
    }
    



    private static void Pipeline(BuildTarget buildTarget)
    {
        var playerPrefsKey = OutputPathKey + buildTarget;
        var path = EditorUtility.OpenFolderPanel("Build", PlayerPrefs.GetString(playerPrefsKey,""), 
            "");

        if (path == string.Empty)
        {
            return;
        }
        
        PlayerPrefs.SetString(playerPrefsKey, path);
        PlayerPrefs.Save();


        DirectoryProcessor.CopyAndReplace(ServerConst.ServerDirectory, Path.Combine(path,ServerConst.ServerDirName));
            
        var buildOptions = new BuildPlayerOptions();
        buildOptions.target = buildTarget;
        buildOptions.locationPathName = path + "/moorestech.exe";
        buildOptions.scenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
            
        BuildPipeline.BuildPlayer(buildOptions);
        
        EditorUtility.RevealInFinder( path );
    }
}