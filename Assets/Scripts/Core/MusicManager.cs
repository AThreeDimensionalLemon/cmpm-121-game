// this script is attached to the MusicPlayer component in Unity
// which has various music tracks as components

using UnityEngine;

public class MusicManager : MonoBehaviour
{
    // set these by dragging the desired audio sources (which are also components) in
    [SerializeField] AudioSource StartMenuMusic;
    [SerializeField] AudioSource WaveMusic;

    void Start()
    {
        // TODO: consider giving gamemanager a method for setting game state, with a listener for when it changes,
        // so that we don't have to check the state in update()
    }

    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME && !StartMenuMusic.isPlaying)
        {
            StartMenuMusic.Play();
            WaveMusic.Stop();   // we could make a state machine so we don't have to do this but that is perhaps overkill for 2-3 audio tracks
        } 
        else if (GameManager.Instance.state == GameManager.GameState.COUNTDOWN && !WaveMusic.isPlaying)
        {
            WaveMusic.Play();
            StartMenuMusic.Stop();
        }
    }
}