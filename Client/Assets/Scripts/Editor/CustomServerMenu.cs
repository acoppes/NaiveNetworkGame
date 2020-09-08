
using UnityEditor;

namespace Editor
{
    public static class CustomServerMenu
    {
        const string kUseRemoteServerByDefault = "NaiveNetworkGame/UseRemoteServer";
        private const string kUseRemoteServerEditorPref = "Gemserk.NaiveNetworkGame.UseRemoteServerByDefault";
        
        [MenuItem(kUseRemoteServerByDefault)]
        public static void ToggleUseRemoteServerByDefault()
        {
            var useRemote = EditorPrefs.GetBool(kUseRemoteServerEditorPref, false);
            EditorPrefs.SetBool(kUseRemoteServerEditorPref, !useRemote);
        }
        
        [MenuItem(kUseRemoteServerByDefault, true)]
        public static bool ToggleUseRemoteServerByDefaultValidate()
        {
            var currentValue = EditorPrefs.GetBool(kUseRemoteServerEditorPref, false);
            Menu.SetChecked(kUseRemoteServerByDefault, currentValue);
            return true;
        }
    }
}
