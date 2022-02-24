using UnityEditor;
using UnityEngine;

namespace UnityDeeplinkBridging
{
    [InitializeOnLoad]
    public class OpenPath
    {
        private const string HANDLER_ID = "OpenPath";
        static OpenPath()
        {
            UnityDeeplinkBridge.AddHandler(HANDLER_ID, HandleGUID);
        }

        private static void HandleGUID(string data)
        {
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(data);

            if(obj == null)
            {
                EditorUtility.DisplayDialog("Failed to find asset", "Unity could not find the asset linked. The file may have been moved, renamed, or does not exist.", "Okay");
            }
            else
            {
                Selection.activeObject = obj;
            }
        }

        [MenuItem("Assets/Deep Link/Copy Path Deeplink")]
        private static void CopyGUIDDeeplink()
        {
            if (Selection.activeObject == null)
            {
                return;
            }
            
            string path = AssetDatabase.GetAssetPath (Selection.activeObject);
            GUIUtility.systemCopyBuffer = UnityDeeplinkBridge.PROTOCOL_BASE + HANDLER_ID + "/" + path;
        }
    }
}