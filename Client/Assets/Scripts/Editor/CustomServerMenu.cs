using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;

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
        
        [MenuItem("NaiveNetworkGame/Toggle Local Server")]
        [Shortcut("NaiveNetworkGame/Toggle Local Server", KeyCode.T, 
            ShortcutModifiers.Shift | ShortcutModifiers.Alt)]
        public static void ToggleServerScene()
        {
            ToggleScene("ServerScene");
        }
        
        [MenuItem("NaiveNetworkGame/Toggle Second Player")]
        [Shortcut("NaiveNetworkGame/Toggle Second Player", KeyCode.Y, 
            ShortcutModifiers.Shift | ShortcutModifiers.Alt)]
        public static void ToggleSecondPlayer()
        {
            ToggleScene("ClientSceneTwoPlayers");
        }

        private static void ToggleScene(string serverSceneName)
        {
            for (var i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if (scene.name.Equals(serverSceneName))
                {
                    EditorSceneManager.UnloadSceneAsync(scene);
                    return;
                }
            }
                
            var guids = AssetDatabase.FindAssets($"t:scene {serverSceneName}");
            var serverScenePath = guids.Where(g =>
                AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(g)).name
                    .Equals(serverSceneName)).Select(g => AssetDatabase.GUIDToAssetPath(g)).First();
                
            var newServerScene = EditorSceneManager.OpenScene(serverScenePath, OpenSceneMode.Additive);
            EditorSceneManager.SetActiveScene(newServerScene);
        }
    }
}
