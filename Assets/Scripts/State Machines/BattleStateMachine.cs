using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleStateMachine : MonoBehaviour
{
    // Enumeration for battle states
    public enum PerformAction
    {
        WAIT,
        TAKEACTION,
        PERFORMACTION,
        ENEMYTURN, //Added a state for the enemy turn
        CHECKALIVE,
        WIN,
        LOSE
    }
    public PerformAction battleStates;

    // Hero GUI states
    public enum HEROGUI
    {
        ACTIVATE,
        WAITING,
        SWITCHPOSITIONS,
        INPUT1,
        INPUT2,
        DONE
    }
    public HEROGUI HeroInput;

    // Lists to manage heroes and enemies
    public List<HandleTurn> PerformList = new List<HandleTurn>();
    public List<GameObject> HeroesInGame = new List<GameObject>();
    public List<GameObject> EnemiesInBattle = new List<GameObject>();
    public List<GameObject> HeroesToManage = new List<GameObject>();

    // UI panels
    public GameObject AttackPanel;
    public GameObject EnemySelectPanel;
    public GameObject MagicPanel;
    public GameObject SwitchPositionsPanel;

    // UI button templates
    public GameObject actionButton;
    public GameObject enemyButton;

    // UI spacers
    public Transform actionSpacer;
    public Transform magicSpacer;
    public Transform switchPositionsSpacer;
    public Transform Spacer;

    // Other variables
    public HandleTurn HeroChoice;
    private List<GameObject> attackButtons = new List<GameObject>();
    private List<GameObject> enemyButtons = new List<GameObject>();
    private GameObject firstHeroToSwitch = null;
    private GameObject secondHeroToSwitch = null;

    //Spawn Points
    public List<Transform> EnemySpawnPoints = new List<Transform>();
    public List<Transform> HeroesSpawnPoints = new List<Transform>();

    //Keep count of whic enemys turn is
    public int enemyInPlayCount = 0;
    float holdManaCost = 0;
    public bool heroTurn = true;
    public GameObject winCanva;
    public GameObject defeatCanva;

    public bool waitOnceEnemy = true;

    void Awake()
    {
        // Initializeenemies
        for (int i = 0; i < GameManager.instance.enemyAmount; i++)
        {
            GameObject NewEnemy = Instantiate(GameManager.instance.enemiesToBattle[i], EnemySpawnPoints[i].position, Quaternion.identity) as GameObject;
            NewEnemy.name = NewEnemy.GetComponent<EnemyStateMachine>().enemy.characterName + "_" + (i+1);
            NewEnemy.GetComponent<EnemyStateMachine>().enemy.characterName = NewEnemy.name;
            EnemiesInBattle.Add(NewEnemy);
        }
        for (int i = 0; i < GameManager.instance.CurrentHeroes.Count; i++)
        {
            GameObject NewHero = Instantiate(GameManager.instance.CurrentHeroes[i], HeroesSpawnPoints[i].position, Quaternion.identity) as GameObject;
        }
    }

    // Initialization
    void Start()
    {
        battleStates = PerformAction.WAIT;

        // Initialize heroes
        HeroesInGame.AddRange(GameObject.FindGameObjectsWithTag("Hero"));

        // Disable UI panels
        AttackPanel.SetActive(false);
        EnemySelectPanel.SetActive(false);
        MagicPanel.SetActive(false);
        SwitchPositionsPanel.SetActive(false);
    }

    void Update()
    {
        // Handle battle state transitions
        switch (battleStates)
        {
            case PerformAction.WAIT:
                if (PerformList.Count > 0)
                    battleStates = PerformAction.TAKEACTION;
                break;

            case PerformAction.TAKEACTION:
                PerformActionStep();
                break;

            case PerformAction.ENEMYTURN:
                //StartCoroutine(wait(enemyTurn, 1f));
                enemyTurn();
                break;

            case PerformAction.PERFORMACTION:
                // Perform action in progress
                break;

            case PerformAction.CHECKALIVE:
                CheckAliveStatus();
                break;

            case PerformAction.WIN:
                StartCoroutine(winWait(true));
                break;

            case PerformAction.LOSE:
                StartCoroutine(winWait(false));
                break;
        }

        // Handle hero input states
        switch (HeroInput)
        {
            case HEROGUI.ACTIVATE:
                if(heroTurn)
                {
                    if (HeroesToManage.Count > 0)
                    {
                        HeroesToManage[0].transform.Find("Selector").gameObject.SetActive(true);
                        HeroChoice = new HandleTurn();
                        AttackPanel.SetActive(true);
                        PopulateActionPanel();
                        HeroInput = HEROGUI.WAITING;
                    }
                }
                break;

            case HEROGUI.WAITING:
                // Waiting for input
                break;

            case HEROGUI.SWITCHPOSITIONS:
                //PopulateSwitchPositionsPanel();
                break;

            case HEROGUI.DONE:
                //heroTurn = false;
                FinalizeHeroInput();
                break;
        }
    }

    public void ToCheck()
    {
        battleStates = PerformAction.CHECKALIVE;
    }
    public void ToEnemy()
    {
        battleStates = PerformAction.ENEMYTURN;
    }
    public void ToActive()
    {
        HeroInput = HEROGUI.ACTIVATE;
    }
    // Perform the next action
    void PerformActionStep()
    {
        GameObject performer = PerformList[0].AttackersGameObject;

        if (PerformList[0].Type == "Enemy")
        {
            EnemyStateMachine ESM = performer.GetComponent<EnemyStateMachine>();
            ESM.currentState = EnemyStateMachine.TurnState.ACTION;
        }
        else if (PerformList[0].Type == "Hero")
        {
            HeroStateMachine HSM = performer.GetComponent<HeroStateMachine>();
            HSM.EnemyToAttack = PerformList[0].AttackersTarget;
            HSM.currentState = HeroStateMachine.TurnState.ACTION;
            //Reduce mana of current Hero
            if(HeroChoice.chosenAttack.magic)
                HSM.hero.currentMP -= holdManaCost;
        }

        PerformList.RemoveAt(0);
        battleStates = PerformAction.PERFORMACTION;     
    }
    
    // Enemys Turn
    void enemyTurn()
    {
        EnemyStateMachine ESM = null;

        if (enemyInPlayCount >= EnemiesInBattle.Count)
            enemyInPlayCount = 0;

        GameObject enemyInPlay = EnemiesInBattle[enemyInPlayCount];
        enemyInPlayCount++;

        if (enemyInPlayCount >= EnemiesInBattle.Count)
            enemyInPlayCount = 0;

        ESM = enemyInPlay.GetComponent<EnemyStateMachine>();
        if (ESM.currentState == EnemyStateMachine.TurnState.DEAD)
        {
            enemyInPlayCount++;
            enemyTurn();
            Debug.Log($"{ESM.enemy.characterName} Dead Battle");
        }
        ESM.currentState = EnemyStateMachine.TurnState.CHOOSEACTION;

        battleStates = PerformAction.WAIT;    
    }

    //UI
    // Populate action buttons (Attack, Magic, Switch Positions)
    void PopulateActionPanel()
    {
        //Reset Panels
        AttackPanel.SetActive(true);
        EnemySelectPanel.SetActive(false);
        MagicPanel.SetActive(false);
        SwitchPositionsPanel.SetActive(false);

        ClearButtons(actionSpacer);
        attackButtons.Clear();

        // Attack button
        CreateButton(actionSpacer, "Attack", () => PopulateAttackPanel());

        // Magic button
        CreateButton(actionSpacer, "Magic", () => PopulateMagicPanel());

        // Switch Positions button
        CreateButton(actionSpacer, "Switch Positions", () => PopulateSwitchPositionsPanel());
    }

    //Populate Attack Panel with hero's attack
    void PopulateAttackPanel()
    {
        ClearButtons(actionSpacer);

        GameObject activeHero = HeroesToManage[0];
        HeroStateMachine heroState = activeHero.GetComponent<HeroStateMachine>();

        if (heroState != null && heroState.hero.attacks.Count > 0)
        {
            foreach (BaseAttack atk in heroState.hero.attacks)
            {
                CreateButton(actionSpacer, atk.attackName, () => Input1(atk));
            }
        }
        else
        {
            Debug.LogWarning("No attacks available for this hero.");
        }
        CreateButton(actionSpacer, "Back", () => PopulateActionPanel());
    }

    // Populate Magic Panel with hero's spells
    void PopulateMagicPanel()
    {
        AttackPanel.SetActive(false);
        MagicPanel.SetActive(true);
        ClearButtons(magicSpacer);

        GameObject activeHero = HeroesToManage[0];
        HeroStateMachine heroState = activeHero.GetComponent<HeroStateMachine>();

        if (heroState != null && heroState.hero.MagicAttacks.Count > 0)
        {
            foreach (BaseAttack spell in heroState.hero.MagicAttacks)
            {
                CreateButton(magicSpacer, spell.attackName, () => Input4(spell));
            }
        }
        else
        {
            Debug.LogWarning("No magic spells available for this hero.");
        }
        CreateButton(magicSpacer, "Back", () => PopulateActionPanel());
    }

    // Create enemy selection buttons
    void CreateEnemyButtons()
    {
        ClearButtons(Spacer);
        enemyButtons.Clear();

        foreach (GameObject enemy in EnemiesInBattle)
        {
            EnemyStateMachine curEnemy = enemy.GetComponent<EnemyStateMachine>();

            if (curEnemy.currentState != EnemyStateMachine.TurnState.DEAD)
            {
                GameObject newButton = Instantiate(enemyButton);
                EnemySelectButton button = newButton.GetComponent<EnemySelectButton>();

                // Set button text and target
                newButton.GetComponentInChildren<Text>().text = curEnemy.enemy.characterName;
                button.EnemyPrefab = enemy;

                // Add click event to select this enemy
                newButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Input2(enemy);
                });

                newButton.transform.SetParent(Spacer, false);
                enemyButtons.Add(newButton);
            }           
        }
        CreateButton(Spacer, "Back", () => PopulateActionPanel());
    }

    // Populate Switch Positions Panel
    void PopulateSwitchPositionsPanel()
    {
        AttackPanel.SetActive(false);
        HeroInput = HEROGUI.SWITCHPOSITIONS;
        ClearButtons(switchPositionsSpacer);

        firstHeroToSwitch = HeroesToManage[0];//Add hero in current turn to switch
        secondHeroToSwitch = null;

        foreach (GameObject hero in HeroesInGame)
        {
            if(hero != firstHeroToSwitch)
            {
                HeroStateMachine heroState = hero.GetComponent<HeroStateMachine>();

                CreateButton(switchPositionsSpacer, heroState.hero.characterName, () => SelectHeroForSwitch(hero));
            }   
        }

        SwitchPositionsPanel.SetActive(true);
        CreateButton(switchPositionsSpacer, "Back", () => PopulateActionPanel());
    }

    // Select heroes to switch positions
    public void SelectHeroForSwitch(GameObject selectedHero)
    {
        if (secondHeroToSwitch == null)
        {
            secondHeroToSwitch = selectedHero;
            Debug.Log($"Second hero selected: {secondHeroToSwitch.name}");
            SwapHeroPositions();
        }
        else
        {
            Debug.Log("Error Swapping Heros");
        }
    }

    // Generic method to create buttons
    void CreateButton(Transform parent, string buttonText, UnityEngine.Events.UnityAction onClick)
    {
        GameObject newButton = Instantiate(actionButton);
        newButton.GetComponentInChildren<Text>().text = buttonText;
        newButton.GetComponent<Button>().onClick.AddListener(onClick);
        newButton.transform.SetParent(parent, false);
    }
    // Clear buttons from a spacer
    void ClearButtons(Transform spacer)
    {
        foreach (Transform child in spacer)
        {
            Destroy(child.gameObject);
        }
    }

    //Acions
    // Swap the positions of the selected heroes
    void SwapHeroPositions()
    {
        if (firstHeroToSwitch != null && secondHeroToSwitch != null)
        {
            HeroStateMachine firstHeroState = firstHeroToSwitch.GetComponent<HeroStateMachine>();
            HeroStateMachine secondHeroState = secondHeroToSwitch.GetComponent<HeroStateMachine>();
            Vector3 tempPos = firstHeroState.startPosition;
            firstHeroState.startPosition =  secondHeroState.startPosition;
            secondHeroState.startPosition = tempPos;

            Vector3 tempPosition = firstHeroToSwitch.transform.position;
            firstHeroToSwitch.transform.position = secondHeroToSwitch.transform.position;
            secondHeroToSwitch.transform.position = tempPosition;

            Debug.Log($"Swapped positions: {firstHeroToSwitch.name} and {secondHeroToSwitch.name}");

            firstHeroToSwitch = null;
            secondHeroToSwitch = null;
            SwitchPositionsPanel.SetActive(false);

            HeroInput = HEROGUI.ACTIVATE;
            //Hero can attack after swaping
        }
    }

    // Physical Attack
    public void Input1(BaseAttack chosenAttack)
    {
        HeroChoice = new HandleTurn
        {
            Attacker = HeroesToManage[0].name,
            AttackersGameObject = HeroesToManage[0],
            Type = "Hero",
            chosenAttack = chosenAttack,
        };

        AttackPanel.SetActive(false);
        EnemySelectPanel.SetActive(true);
        CreateEnemyButtons();
    }
    
    //Magic Attack
    public void Input4(BaseAttack chosenMagic)
    {
        HeroStateMachine HSM = HeroesToManage[0].GetComponent<HeroStateMachine>();
        if(HSM.hero.currentMP > chosenMagic.attackManaCost)
        {
            holdManaCost = chosenMagic.attackManaCost; /////////////////////////////////
            HeroChoice = new HandleTurn
            {
                Attacker = HeroesToManage[0].name,
                AttackersGameObject = HeroesToManage[0],
                Type = "Hero",
                chosenAttack = chosenMagic
            };

            MagicPanel.SetActive(false);
            EnemySelectPanel.SetActive(true);
            CreateEnemyButtons();
        }
        else
        {
            Debug.Log("Not enogh mana");
        }
    }

    //Finish Attack
    public void Input2(GameObject chosenEnemy)
    {
        HeroChoice.AttackersTarget = chosenEnemy;
        HeroInput = HEROGUI.DONE;

        heroTurn = false;
    }

    void FinalizeHeroInput()
    {
        PerformList.Add(HeroChoice);
        EnemySelectPanel.SetActive(false);
        ClearButtons(actionSpacer);
        HeroesToManage[0].transform.Find("Selector").gameObject.SetActive(false);
        HeroesToManage.RemoveAt(0);
        heroTurn = false;
        HeroInput = HEROGUI.ACTIVATE;
    }

    // Check if heroes or enemies are still alive
    void CheckAliveStatus()
    {
        if (HeroesInGame.Count < 1)
        {
            battleStates = PerformAction.LOSE;
        }
        else if(EnemiesInBattle.Count < 1)
            battleStates = PerformAction.WIN;
        else
        {
            if (heroTurn)
            {
                HeroInput = HEROGUI.ACTIVATE;
            }
            else
            {
                if(waitOnceEnemy)
                {
                    StartCoroutine(wait(ToEnemy, 1f));
                    waitOnceEnemy = false;
                }
            }
            
        }
    }

    private IEnumerator winWait(bool a)
    {
        if (a)
        {
            yield return new WaitForSeconds(1f);

            winCanva.SetActive(true);

            yield return new WaitForSeconds(1f);

            GameManager.instance.gameState = GameManager.GameStates.WORLD_STATE;
            GameManager.instance.enemiesToBattle.Clear();
            GameManager.instance.LoadSceneAfterBattle();
        }
        else
        {
            yield return new WaitForSeconds(1f);

            defeatCanva.SetActive(true);

            yield return new WaitForSeconds(1f);

            GameManager.instance.gameState = GameManager.GameStates.WORLD_STATE;
            GameManager.instance.enemiesToBattle.Clear();
            GameManager.instance.LoadSceneAfterBattle();
        }     
    }

    private IEnumerator wait(Action callback, float time)
    {
        yield return new WaitForSeconds(time);
        callback?.Invoke();
    }
}