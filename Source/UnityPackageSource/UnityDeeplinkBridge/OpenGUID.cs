using UnityEditor;
using UnityEngine;

namespace UnityDeeplinkBridging
{
    [InitializeOnLoad]
    public class OpenGUID
    {
        private const string HANDLER_ID = "OpenGUID";
        static OpenGUID()
        {
            UnityDeeplinkBridge.AddHandler(HANDLER_ID, HandleGUID);
        }

        private static void HandleGUID(string data)
        {
            string path = AssetDatabase.GUIDToAssetPath(data);
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            
            if(obj == null)
            {
                EditorUtility.DisplayDialog("Failed to find asset", "Unity could not find the asset linked. The original file may have been replaced without preserving the GUID, or the asset may not exist.", "Okay");
            }
            else
            {
                Selection.activeObject = obj;
            }
        }

        [MenuItem("Assets/Deep Link/Copy GUID Deeplink")]
        private static void CopyGUIDDeeplink()
        {
            if (Selection.activeObject == null)
            {
                return;
            }
            string path = AssetDatabase.GetAssetPath (Selection.activeObject);
            string guid = AssetDatabase.AssetPathToGUID(path);
            GUIUtility.systemCopyBuffer = UnityDeeplinkBridge.PROTOCOL_BASE + HANDLER_ID + "/" + guid;
        }
    }
}