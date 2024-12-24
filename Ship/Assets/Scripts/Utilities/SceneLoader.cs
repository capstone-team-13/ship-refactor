using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneLoader : MonoBehaviour
{
    private enum SceneName
    {
        Start = 0,
        Level0,
        Undefined
    }

    private static readonly string[] m_sceneNames =
    {
        "Start",
        "Level #0",
    };

    #region Editor API

    [SerializeField] private SceneName m_sceneName = SceneName.Undefined;
    [Space(4)] public UnityEvent OnBeforeLoad;

    #endregion

    #region Unity Callbacks

    private void OnValidate()
    {
#if UNITY_EDITOR
        var buildScenes = EditorBuildSettings.scenes;
        var buildSceneNames = new HashSet<string>();

        foreach (EditorBuildSettingsScene scene in buildScenes)
        {
            if (scene.enabled) buildSceneNames.Add(System.IO.Path.GetFileNameWithoutExtension(scene.path));
        }

        foreach (var sceneName in m_sceneNames)
        {
            if (!buildSceneNames.Contains(sceneName))
            {
                Debug.LogError(
                    $"Scene '{sceneName}' is defined in SceneLoader but missing in Build Settings. " +
                    $"This may cause errors when attempting to load the scene.");
            }
        }

        foreach (string sceneName in buildSceneNames.Where(sceneName =>
                     System.Array.IndexOf(m_sceneNames, sceneName) == -1))
        {
            Debug.LogWarning(
                $"Scene '{sceneName}' is in Build Settings but not in SceneLoader. " +
                $"Consider updating the SceneLoader class to standardize scene loading.");
        }
#endif
    }

    #endregion

    #region API

    public void Load()
    {
#if UNITY_EDITOR
        if (m_sceneName == SceneName.Undefined)
        {
            Debug.LogWarning($"You need to define which scene you want to load. ({gameObject.name})");
        }
#endif
        var sceneIndex = (int)m_sceneName;
        bool canLoadScene = __M_ExecuteValidations();
        if (canLoadScene) SceneManager.LoadScene(m_sceneNames[sceneIndex]);
    }

    #endregion

    private bool __M_ExecuteValidations()
    {
        try
        {
            OnBeforeLoad.Invoke();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Validation failed: {e.Message}");
            return false;
        }

        return true;
    }
}