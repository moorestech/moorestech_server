using System.IO;
using System.Linq;
using GameConst;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;


public class BuildWithServer
{
    private const string OutputPathKey = "WindowsBuildOutputPath";
    [MenuItem("moorestech/WindowsBuild")]
    public static void WindowsBuild()
    {
        Pipeline(BuildTarget.StandaloneWindows64,false);
    }
    [MenuItem("moorestech/LinuxBuild")]
    public static void LinuxBuild()
    {
        Pipeline(BuildTarget.StandaloneLinux64,false);
    }
    
    #region from Github Action
    public static void WindowsBuildFromGithubAction()
    {
        Pipeline(BuildTarget.StandaloneWindows64,true);
    }
    public static void LinuxBuildFromGithubAction()
    {
        Pipeline(BuildTarget.StandaloneLinux64,true);
    }
    #endregion

    private static void Pipeline(BuildTarget buildTarget,bool isErrorExit)
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
            
        var report = BuildPipeline.BuildPlayer(buildOptions);
        
        EditorUtility.RevealInFinder( path );
        
        
        if (isErrorExit)
        {
            //ビルドレポートを表示
            foreach (var buildStep in report.steps)
            {
                var stepName = buildStep.name;
                foreach (var message in buildStep.messages)
                {
                    Debug.LogError(stepName + " : " + message.type + " : " + message.content);
                }
            }
            if (report.summary.result == BuildResult.Failed)
            {
                EditorApplication.Exit(1);
            }
        }
    }



}