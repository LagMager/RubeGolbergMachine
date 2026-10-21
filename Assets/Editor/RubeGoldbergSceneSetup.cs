using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class RubeGoldbergSceneSetup
{
    static RubeGoldbergSceneSetup()
    {
        EditorApplication.delayCall += EnsureMachineInActiveScene;
    }

    private static void EnsureMachineInActiveScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        if (Object.FindFirstObjectByType<RubeGoldbergController>() == null)
        {
            BuildAndSaveScene();
        }
    }

    [MenuItem("Tools/Rube Goldberg/Rebuild Rube Goldberg Machine")]
    public static void BuildAndSaveScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("Exit Play Mode before rebuilding the Rube Goldberg machine in the scene.");
            return;
        }

        RubeGoldbergController controller = RubeGoldbergBuilder.BuildMachineInCurrentScene();
        if (controller != null)
        {
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            EditorSceneManager.SaveOpenScenes();
            FrameMachineInSceneView();
            Selection.activeGameObject = controller.gameObject;
            Debug.Log("Rube Goldberg Machine built in scene! Press Play (or Space in Play Mode) to watch the chain reaction.");
        }
    }

    [MenuItem("Tools/Rube Goldberg/Frame Machine in Scene View")]
    public static void FrameMachineInSceneView()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            sceneView.LookAt(new Vector3(1.2f, 3.0f, 0f), Quaternion.Euler(14f, -8f, 0f), 11.5f);
            sceneView.Repaint();
        }
    }
}
