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
    private HandleTurn HeroChoice;
    private List<GameObject> attackButtons = new List<GameObject>();
    private List<GameObject> enemyButtons = new List<GameObject>();
    private GameObject firstHeroToSwitch;
    private GameObject secondHeroToSwitch;

    //Spawn Points
    public List<Transform> spawnPoints = new List<Transform>();

    //Keep count of whic enemys turn is
    bool magicUse = false;
    int enemyInPlayCount = 0;
    float holdManaCost = 0;

    void Awake()
    {
        for(int i = 0; i < GameManager.instance.enemyAmount; i++)
        {
            GameObject NewEnemy = Instantiate(GameManager.instance.enemiesToBattle[i], spawnPoints[i].position, Quaternion.identity) as GameObject;
            NewEnemy.name = NewEnemy.GetComponent<EnemyStateMachine>().enemy.characterName + "_" + (i+1);
            NewEnemy.GetComponent<EnemyStateMachine>().enemy.characterName = NewEnemy.name;
            EnemiesInBattle.Add(NewEnemy);
        }
    }

    // Initialization
    void Start()
    {
        battleStates = PerformAction.WAIT;

        // Initialize heroes and enemies
        //EnemiesInBattle.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        HeroesInGame.AddRange(GameObject.FindGameObjectsWithTag("Hero"));

        // Disable UI panels
        AttackPanel.SetActive(false);
        EnemySelectPanel.SetActive(false);
        MagicPanel.SetActive(false);
        SwitchPositionsPanel.SetActive(false);

        // Create enemy buttons
        CreateEnemyButtons();
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
                enemyTurn();
                break;

            case PerformAction.PERFORMACTION:
                // Perform action in progress
                break;

            case PerformAction.CHECKALIVE:
                CheckAliveStatus();
                break;

            case PerformAction.WIN:
                Debug.Log("You won the battle!");

                GameManager.instance.LoadSceneAfterBattle();
                GameManager.instance.gameState = GameManager.GameStates.WORLD_STATE;
                GameManager.instance.enemiesToBattle.Clear();
                break;

            case PerformAction.LOSE:
                Debug.Log("You lost the battle.");
                break;
        }

        // Handle hero input states
        switch (HeroInput)
        {
            case HEROGUI.ACTIVATE:
                if (HeroesToManage.Count > 0)
                {
                    HeroesToManage[0].transform.Find("Selector").gameObject.SetActive(true);
                    HeroChoice = new HandleTurn();
                    AttackPanel.SetActive(true);
                    PopulateActionPanel();
                    HeroInput = HEROGUI.WAITING;
                }
                break;

            case HEROGUI.WAITING:
                // Waiting for input
                break;

            case HEROGUI.SWITCHPOSITIONS:
                PopulateSwitchPositionsPanel();
                break;

            case HEROGUI.DONE:
                FinalizeHeroInput();
                break;
        }
    }

    // Enemys Turn
    void enemyTurn()
    {
        GameObject enemyInPlay = EnemiesInBattle[enemyInPlayCount];
        enemyInPlayCount++;

        if(enemyInPlayCount >= EnemiesInBattle.Count)
            enemyInPlayCount = 0;

        EnemyStateMachine ESM = enemyInPlay.GetComponent<EnemyStateMachine>();
        ESM.currentState = EnemyStateMachine.TurnState.CHOOSEACTION;

        battleStates = PerformAction.WAIT;
    }

    // Populate action buttons (Attack, Magic, Switch Positions)
    void PopulateActionPanel()
    {
        /////////////////////////////////////////////////
        AttackPanel.SetActive(true);
        EnemySelectPanel.SetActive(false);
        MagicPanel.SetActive(false);
        SwitchPositionsPanel.SetActive(false);
        //////////////////////////////////////////////////

        ClearButtons(actionSpacer);
        attackButtons.Clear();

        // Attack button
        CreateButton(actionSpacer, "Attack", () => PopulateAttackPanel());

        // Magic button
        CreateButton(actionSpacer, "Magic", () => Input3());

        // Switch Positions button
        CreateButton(actionSpacer, "Switch Positions", () => InputSwitchPositions());
    }

    // Populate Magic Panel with hero's spells
    void PopulateMagicPanel()
    {
        ClearButtons(magicSpacer);

        magicUse = true;

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
            GameObject newButton = Instantiate(enemyButton);
            EnemySelectButton button = newButton.GetComponent<EnemySelectButton>();
            EnemyStateMachine curEnemy = enemy.GetComponent<EnemyStateMachine>();

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

    // Populate Switch Positions Panel
    void PopulateSwitchPositionsPanel()
    {
        ClearButtons(switchPositionsSpacer);

        foreach (GameObject hero in HeroesInGame)
        {
            GameObject newButton = Instantiate(actionButton);
            newButton.GetComponentInChildren<Text>().text = hero.name;

            // Add click event to select the hero
            newButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                SelectHeroForSwitch(hero);
            });

            newButton.transform.SetParent(switchPositionsSpacer, false);
        }

        SwitchPositionsPanel.SetActive(true);
    }

    // Select heroes to switch positions
    public void SelectHeroForSwitch(GameObject selectedHero)
    {
        if (firstHeroToSwitch == null)
        {
            firstHeroToSwitch = selectedHero;
            Debug.Log($"First hero selected: {firstHeroToSwitch.name}");
        }
        else if (secondHeroToSwitch == null)
        {
            secondHeroToSwitch = selectedHero;
            Debug.Log($"Second hero selected: {secondHeroToSwitch.name}");
            SwapHeroPositions();
        }
    }

    // Swap the positions of the selected heroes
    void SwapHeroPositions()
    {
        if (firstHeroToSwitch != null && secondHeroToSwitch != null)
        {
            Vector3 tempPosition = firstHeroToSwitch.transform.position;
            firstHeroToSwitch.transform.position = secondHeroToSwitch.transform.position;
            secondHeroToSwitch.transform.position = tempPosition;

            Debug.Log($"Swapped positions: {firstHeroToSwitch.name} and {secondHeroToSwitch.name}");

            firstHeroToSwitch = null;
            secondHeroToSwitch = null;
            SwitchPositionsPanel.SetActive(false);

            HeroInput = HEROGUI.ACTIVATE;
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

    // Hero Input Handlers
    public void Input1(BaseAttack chosenAttack)
    {
        magicUse = false; ////////////////////////////////////
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
    
    /////////////////////////////////////////////////////
    void PopulateAttackPanel()
    {
        ClearButtons(actionSpacer);

        magicUse = false;

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
    /////////////////////////////////////////////////////////////////
    public void Input3()
    {
        AttackPanel.SetActive(false);
        MagicPanel.SetActive(true);
        PopulateMagicPanel();
    }

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

    public void Input2(GameObject chosenEnemy)
    {
        HeroChoice.AttackersTarget = chosenEnemy;
        HeroInput = HEROGUI.DONE;

        battleStates = PerformAction.ENEMYTURN; ////////////////////////////////
    }

    public void InputSwitchPositions()
    {
        AttackPanel.SetActive(false);
        HeroInput = HEROGUI.SWITCHPOSITIONS;
        PopulateSwitchPositionsPanel();
    }

    void FinalizeHeroInput()
    {
        PerformList.Add(HeroChoice);
        EnemySelectPanel.SetActive(false);
        ClearButtons(actionSpacer);
        HeroesToManage[0].transform.Find("Selector").gameObject.SetActive(false);
        HeroesToManage.RemoveAt(0);
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
            ////////////////////////////////////////////////////
            if(magicUse)
                HSM.hero.currentMP -= holdManaCost;
        }

        PerformList.RemoveAt(0);
        battleStates = PerformAction.PERFORMACTION;
    }


    // Check if heroes or enemies are still alive
    void CheckAliveStatus()
    {
        if (HeroesInGame.Count < 1)
        {
            battleStates = PerformAction.LOSE;
        }
        else if (EnemiesInBattle.Count < 1)
        {
            battleStates = PerformAction.WIN;
        }
        else
        {
            HeroInput = HEROGUI.ACTIVATE;
        }
    }
}