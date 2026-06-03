using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class PlayModeStartSceneSetter
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

    static PlayModeStartSceneSetter()
    {
        SceneAsset mainMenu = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuScenePath);
        if (mainMenu != null)
        {
            EditorSceneManager.playModeStartScene = mainMenu;
        }
    }
}
