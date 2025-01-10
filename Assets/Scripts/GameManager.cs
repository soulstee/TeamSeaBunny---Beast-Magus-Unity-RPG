using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // CLASS RANDOM MONSTERS
    [System.Serializable]
    public class RegionData
    {
        public string regionName;
        public int maxAmountEnemies = 6;
        public string BattleScene;
        public List<GameObject> possibleEnemies = new List<GameObject>();
    }

    public int currentRegions;
    public List<RegionData> Regions = new List<RegionData>();

    // SPAWNPOINTS
    [Header("Spawn Point Settings")]
    public string nextSpawnPoint;

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
    public string sceneCabin;
    public string sceneToLoad;            // The name of the next scene to load
    public string lastScene;              // The last scene name (e.g., for battle return)

    // BOOLS
    [Header("Bool Settings")]
    public bool isFirstGameStart = true;
    public bool isWalking = false;
    public bool canGetEncounter = false;
    public bool gotAttacked = false;

    // ENUMS
    public enum GameStates
    {
        WORLD_STATE,
        TOWN_STATE,
        BATTLE_STATE,
        IDLE
    }

    // ENCOUNTER VARIABLES
    public float encounterCooldown = 0f; // Time until next possible encounter
    private float encounterInterval = 1.0f; // Minimum seconds between checks
    private Vector2 lastPosition;         // Hero's last position
    public float distanceWalked = 0f;    // Total distance walked
    public float encounterDistance = 5f; // Distance required to trigger check

    public int enemyAmount;
    public List<GameObject> enemiesToBattle = new List<GameObject>();

    public GameStates gameState;

    void Awake()
    {
        // Singleton Pattern
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        Vector3 spawnPosition = (GameObject.FindWithTag("Teleporter") != null)
            ? GameObject.FindWithTag("Teleporter").transform.position
            : (Vector2)nextHeroPosition;

        if (!GameObject.Find("OverworldPlayer"))
        {
            currentHero = Instantiate(overworldCharacter, spawnPosition, Quaternion.identity);
            currentHero.name = "OverworldPlayer";
            DontDestroyOnLoad(currentHero);
        }

        lastPosition = currentHero != null ? currentHero.transform.position : Vector2.zero;
    }

    void Update()
    {
        if (encounterCooldown > 0f)
        {
            encounterCooldown -= Time.deltaTime;
        }

        lastHeroPosition = Vector2.zero;

        switch (gameState)
        {
            case GameStates.WORLD_STATE:
                if (isWalking)
                {
                    TrackWalkingDistance();
                }
                if (gotAttacked)
                {
                    gameState = GameStates.BATTLE_STATE;
                }
                break;

            case GameStates.TOWN_STATE:
                // Town logic
                break;

            case GameStates.BATTLE_STATE:
                StartBattle();
                gameState = GameStates.IDLE;
                break;

            case GameStates.IDLE:
                // Idle logic
                break;
        }
    }

    public void LoadNextScene()
    {
        lastScene = SceneManager.GetActiveScene().name;

        // Begin loading the next scene
        SceneManager.LoadScene(sceneToLoad);

        // Only subscribe to sceneLoaded for overworld scenes
        if (sceneToLoad != Regions[currentRegions].BattleScene)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Unsubscribe to avoid duplicate calls
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Find the spawn point GameObject by name
        if (!string.IsNullOrEmpty(nextSpawnPoint))
        {
            GameObject spawnPoint = GameObject.Find(nextSpawnPoint);
            if (spawnPoint != null)
            {
                nextHeroPosition = spawnPoint.transform.position; // Update hero position
            }
            else
            {
                Debug.LogWarning($"Spawn point '{nextSpawnPoint}' not found in scene '{scene.name}'.");
            }
        }

    // Spawn or move the hero to the correct position
    if (currentHero == null)
    {
        currentHero = Instantiate(overworldCharacter, nextHeroPosition, Quaternion.identity);
        currentHero.name = "OverworldPlayer";
        DontDestroyOnLoad(currentHero); // Make persistent for future scenes
    }
    else
    {
        currentHero.transform.position = nextHeroPosition;
    }

    // Clear the spawn point after using it
    nextSpawnPoint = "";
}


    public void LoadSceneAfterBattle()
    {
        SceneManager.LoadScene(lastScene);
        sceneToLoad = lastScene; // Ensure we're loading the correct scene
        nextHeroPosition = lastHeroPosition; // Reset to the position before the battle
        LoadNextScene(); // Use the same logic to load and handle positioning
    }


    void TrackWalkingDistance()
    {
        distanceWalked += Vector2.Distance((Vector2)currentHero.transform.position, lastPosition);
        lastPosition = currentHero.transform.position;

        if (distanceWalked >= encounterDistance)
        {
            //gotAttacked = true;
            RandomEncounter();
            distanceWalked = 0f; // Reset distance counter after check
        }
    }

    void RandomEncounter()
    {
        if (isWalking && canGetEncounter && encounterCooldown <= 0f)
        {
            if (Random.Range(0, 10000) < 700) // 7% chance
            {
                Debug.Log("I got attacked!");
                gotAttacked = true;
            }

            encounterCooldown = encounterInterval; // Reset cooldown
        }
    }

    void StartBattle()
    {
        lastHeroPosition = GameObject.Find("OverworldPlayer").transform.position;
        nextHeroPosition = lastHeroPosition;
        lastScene = SceneManager.GetActiveScene().name;

        Destroy(currentHero);

        enemiesToBattle.Clear();
        enemyAmount = Random.Range(1, Regions[currentRegions].maxAmountEnemies + 1);
        for (int i = 0; i < enemyAmount; i++)
        {
            enemiesToBattle.Add(Regions[currentRegions].possibleEnemies[Random.Range(0, Regions[currentRegions].possibleEnemies.Count)]);
        }

        SceneManager.LoadScene(Regions[currentRegions].BattleScene);

        isWalking = false;
        gotAttacked = false;
        canGetEncounter = false;
    }

    /*public void loadCabin()
    {
        SceneManager.LoadScene(sceneCabin);
    }*/
}
