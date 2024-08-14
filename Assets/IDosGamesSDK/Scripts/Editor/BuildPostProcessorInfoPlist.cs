#if UNITY_EDITOR && UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace IDosGames
{
    public class BuildPostProcessorInfoPlist
    {
        private const string TRACKING_DESCRIPTION = "Your data will be used to provide you a better and personalized ad experience.";

        [PostProcessBuild(0)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToXcode)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                AddPListValues(pathToXcode);
            }
        }

        private static void AddPListValues(string pathToXcode)
        {
            string plistPath = pathToXcode + "/Info.plist";

            PlistDocument plistObj = new();
            plistObj.ReadFromString(File.ReadAllText(plistPath));

            PlistElementDict plistRoot = plistObj.root;
            plistRoot.SetString("NSUserTrackingUsageDescription", TRACKING_DESCRIPTION);

            File.WriteAllText(plistPath, plistObj.WriteToString());
        }
    }
}
#endif