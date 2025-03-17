using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class LoadButton : MonoBehaviour
{
    public Button loadButton; // Assign the UI button in the Inspector
    private string savePath;

    void Start()
    {
        savePath = Application.persistentDataPath + "/game_save.json";

        // Check if a save file exists. If not, disable the button
        if (!File.Exists(savePath))
        {
            loadButton.gameObject.SetActive(false);
        }
    }

    public void LoadGame()
    {
        if (File.Exists(savePath))
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

            // Hide the button after clicking
            loadButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No saved game found!");
        }
    }
}
