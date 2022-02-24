using UnityEditor;

namespace UnityDeeplinkBridging
{
    [InitializeOnLoad]
    internal class UnityDeeplinkBridgeBootstrap
    {
        private static UnityDeeplinkBridge _bridge;
        static UnityDeeplinkBridgeBootstrap()
        {
            //Only windows is supported for now
            #if UNITY_EDITOR_WIN
                IFocusController focusController = new WindowsFocusController();
            #else
                IFocusController focusController = new NullFocusController();
            #endif
            _bridge = new UnityDeeplinkBridge(focusController);
        }
    }
}