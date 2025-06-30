using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Postica.BindingSystem
{

    public class BindingSystemBuild : UnityEditor.Build.IPreprocessBuildWithReport, UnityEditor.Build.IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private string _FilePath => "Assets/AOT_Accessors.cs";

        public void OnPreprocessBuild(BuildReport report)
        {
            var buildGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            if (PlayerSettings.GetScriptingBackend(NamedBuildTarget.FromBuildTargetGroup(buildGroup)) == ScriptingImplementation.IL2CPP)
            {
                Optimizer.GenerateLinkFile(null, (s, v) => EditorUtility.DisplayProgressBar("Binding System", s, v));
            }
        }
        
        public void OnPostprocessBuild(BuildReport report)
        {
            if (System.IO.File.Exists(_FilePath))
            {
                System.IO.File.Delete(_FilePath);
            }
        }
    }
}