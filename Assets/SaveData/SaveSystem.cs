using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

[System.Serializable]
public class HeroData
{
    public string heroName;
    public float currentHP;
    public float baseHP;
    public float currentMP;
    public float baseMP;
    public float currentSP;
    public float maxSP;
    public float currentATK;
    public float currentDEF;
}

[System.Serializable]
public class GameData
{
    public string sceneName; // Tracks the current scene
    public float playerPosX, playerPosY, playerPosZ; // Player position
    public List<HeroData> heroes = new List<HeroData>(); // Hero stats
}

public static class SaveSystem
{
    private static string savePath = Application.persistentDataPath + "/game_save.json";

    public static void SaveGame(HeroStateMachine[] heroes, Transform playerTransform)
    {
        GameData gameData = new GameData
        {
            sceneName = SceneManager.GetActiveScene().name, // Save the current scene
            playerPosX = playerTransform.position.x,
            playerPosY = playerTransform.position.y,
            playerPosZ = playerTransform.position.z
        };

        // Save each hero's stats
        foreach (var hero in heroes)
        {
            gameData.heroes.Add(new HeroData
            {
                heroName = hero.hero.characterName,
                currentHP = hero.hero.currentHP,
                baseHP = hero.hero.baseHP,
                currentMP = hero.hero.currentMP,
                baseMP = hero.hero.baseMP,
                currentSP = hero.hero.currentSP,
                maxSP = hero.hero.maxSP,
                currentATK = hero.hero.currentATK,
                currentDEF = hero.hero.currentDEF
            });
        }

        string json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Game saved to: " + savePath);
    }

    public static void LoadGame(HeroStateMachine[] heroes, Transform playerTransform)
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            GameData gameData = JsonUtility.FromJson<GameData>(json);

            // Load scene
            SceneManager.LoadScene(gameData.sceneName);

            // Load player position after the scene loads
            PlayerPrefs.SetFloat("PlayerX", gameData.playerPosX);
            PlayerPrefs.SetFloat("PlayerY", gameData.playerPosY);
            PlayerPrefs.SetFloat("PlayerZ", gameData.playerPosZ);

            // Load hero stats
            for (int i = 0; i < heroes.Length && i < gameData.heroes.Count; i++)
            {
                heroes[i].hero.characterName = gameData.heroes[i].heroName;
                heroes[i].hero.currentHP = gameData.heroes[i].currentHP;
                heroes[i].hero.baseHP = gameData.heroes[i].baseHP;
                heroes[i].hero.currentMP = gameData.heroes[i].currentMP;
                heroes[i].hero.baseMP = gameData.heroes[i].baseMP;
                heroes[i].hero.currentSP = gameData.heroes[i].currentSP;
                heroes[i].hero.maxSP = gameData.heroes[i].maxSP;
                heroes[i].hero.currentATK = gameData.heroes[i].currentATK;
                heroes[i].hero.currentDEF = gameData.heroes[i].currentDEF;
            }

            Debug.Log("Game loaded from: " + savePath);
        }
        else
        {
            Debug.LogWarning("Save file not found!");
        }
    }
}