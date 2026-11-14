using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayableDirector))]
public sealed class OpeningCutsceneSceneTransition : MonoBehaviour
{
    private PlayableDirector director;

    private void OnEnable()
    {
        director = GetComponent<PlayableDirector>();
        director.stopped += LoadNextScene;
    }

    private void OnDisable()
    {
        director.stopped -= LoadNextScene;
    }

    private void LoadNextScene(PlayableDirector stoppedDirector)
    {
        SceneManager.LoadSceneAsync("TheLand_Scene", LoadSceneMode.Single);
    }
}
