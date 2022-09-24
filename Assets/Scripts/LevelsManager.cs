using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Number of models for a single level
/// </summary>
[Serializable]
public class ModelCapacities
{
    public PrimitiveType ModelType; 

    public int PrimitiveCapacity; // maximum number of model on stage per frame
}

public class LevelsManager : GlobalBehaviour<LevelsManager>
{
    #region Global Variables

    private SoundsManager musicManager;

    #endregion

    #region Properties

    // The Current Active Scene Index in Build Settings
    public int ActiveIndex { get; private set; }

    // A flag to mark whether the levels manager is in the middle of switching the scenes.
    public bool LoadCompleted { get; private set; } = true;

    #endregion

    #region Unity Lifecycle

    IEnumerator Start()
    {
        yield return new WaitUntil(() =>
        {
            musicManager = SoundsManager.Instance;
            if (musicManager)
            {
                PlayLevelMusic();
                return true;
            }

            return false;
        });
    }

    #endregion

    #region Methods / Functions

    public void LoadNewLevel(string levelName)
    {
        var activeScene = SceneManager.GetActiveScene();
        var activeName = activeScene.name;

        if (activeName == levelName)
            return;

        LoadCompleteScene(levelName);
    }

    private void LoadCompleteScene(string levelName)
    {
        LoadCompleted = false;

        // use async load as unity engine needs time to figure out how to deal with those 'DontDestroyOnLoad' components.
        var asyncOper = SceneManager.LoadSceneAsync(levelName);

        asyncOper.completed += asyncResult =>
        {
            var levelScene = SceneManager.GetSceneByName(levelName);
            if (levelScene != null)
            {
                for (int sceneIndex = 0, sceneCount = SceneManager.sceneCountInBuildSettings; sceneIndex < sceneCount; sceneIndex++)
                {
                    var scene = SceneManager.GetSceneByBuildIndex(sceneIndex);
                    if (scene == levelScene)
                    {
                        ActiveIndex = sceneIndex;
                        LoadCompleted = true; 
                        PlayLevelMusic();
                        break;
                    }
                }
            }
        };
    }

    private void PlayLevelMusic()
    {
        var startMusic = (EnumMusics)Enum.ToObject(typeof(EnumMusics), ActiveIndex);
        StartCoroutine(musicManager.PlayMusicCoroutine(startMusic));
    }

    #endregion
}
