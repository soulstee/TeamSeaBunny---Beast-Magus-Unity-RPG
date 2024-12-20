using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // HERO
    [Header("Hero Settings")]
    public GameObject overworldCharacter; // Prefab for the player's character
    private GameObject currentHero;       // Reference to the instantiated character

    // POSITIONS
    [Header("Position Settings")]
    public Vector2 nextHeroPosition;      // Position in the next scene
    public Vector2 lastHeroPosition;      // Last position before battle

    // SCENES
    [Header("Scene Settings")]
    public string sceneToLoad;            // The name of the next scene to load
    public string lastScene;              // The last scene name (e.g., for battle return)

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        // Singleton Pattern: Ensure only one instance of GameManager exists
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Make the GameManager persistent across scenes
        DontDestroyOnLoad(gameObject);

        // If the hero doesn't already exist in the scene, spawn them
        if (!GameObject.Find("OverworldPlayer"))
        {
            currentHero = Instantiate(overworldCharacter, nextHeroPosition, Quaternion.identity);
            currentHero.name = "OverworldPlayer";
        }
    }

    // This method loads the next scene and positions the hero
    public void LoadNextScene()
    {
        // Update the last scene name before switching
        lastScene = SceneManager.GetActiveScene().name;

        // Begin loading the next scene
        SceneManager.LoadScene(sceneToLoad);

        // Use SceneManager.sceneLoaded to reposition the hero after the scene loads
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // This method is triggered when the new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Unsubscribe to avoid duplicate calls
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Find or create the hero in the new scene
        if (!currentHero)
        {
            currentHero = Instantiate(overworldCharacter, nextHeroPosition, Quaternion.identity);
            currentHero.name = "OverworldPlayer";
        }
        else
        {
            // Move the existing hero to the next position
            currentHero.transform.position = nextHeroPosition;
        }
    }
}