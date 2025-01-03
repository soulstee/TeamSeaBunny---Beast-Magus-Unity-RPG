using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusicController : MonoBehaviour
{
    public AudioClip sceneMusic;

    void Start()
    {
        if (sceneMusic != null && MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic(sceneMusic);
        }
    }
}
