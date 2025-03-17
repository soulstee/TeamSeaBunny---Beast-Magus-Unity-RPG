using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveButton : MonoBehaviour
{
    private static SaveButton instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep the entire PersistentCanvas in all scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate UI Canvases if they exist
        }
    }

    public void SaveGame()
    {
        HeroStateMachine[] heroes = FindObjectsOfType<HeroStateMachine>();
        Transform player = GameObject.FindGameObjectWithTag("OverworldPlayer")?.transform;

        if (player != null)
        {
            SaveSystem.SaveGame(heroes, player);
        }
        else
        {
            Debug.LogWarning("Player not found when saving the game.");
        }
    }

    public void LoadGame()
    {
        HeroStateMachine[] heroes = FindObjectsOfType<HeroStateMachine>();
        Transform player = GameObject.FindGameObjectWithTag("OverworldPlayer")?.transform;

        if (player != null)
        {
            SaveSystem.LoadGame(heroes, player);
        }
        else
        {
            Debug.LogWarning("Player not found when loading the game.");
        }
    }
}
