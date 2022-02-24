#if UNITY_EDITOR_WIN

using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace UnityDeeplinkBridging
{
    [InitializeOnLoad]
    internal class WindowsInstallPrompt
    {
        private static bool Prompted
        {
            get => PlayerPrefs.GetInt("UnityDeeplinkBridgePrompted", 0) > 0;
            set => PlayerPrefs.SetInt("UnityDeeplinkBridgePrompted", value ? 1 : 0);
        }
        private static string _message = @"It looks like this is the first time UnityDeeplinkBridge has loaded. 
UnityDeeplinkBridge requires a small program to be installed that sends the url information to Unity.
Would you like to install it now? You can manually install it later by running the .exe in the UnityDeeplinkBridge folder.";
        static WindowsInstallPrompt()
        {
            if (!Prompted)
            {
                Prompted = true;
                if (EditorUtility.DisplayDialog("Install UnityDeeplinkBridge?", _message, "Install", "Ignore"))
                {
                    string command = Application.dataPath + "/UnityDeeplinkBridge/UnityDeeplinkBridgeSetup.exe";
                    ProcessStartInfo ps = new ProcessStartInfo(command);
                    using (Process p = new Process())
                    {
                        p.StartInfo = ps;
                        p.Start();
                        p.WaitForExit();
                    }
                }
            }
        }
    }
}

#endif