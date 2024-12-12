using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleStateMachine : MonoBehaviour
{

    public enum PerformAction
    {
        WAIT,
        TAKEACTION,
        PERFORMACTION,
        CHECKALIVE,
        WIN,
        LOSE
    }

    public PerformAction battleStates;

    public List<HandleTurn> PerformList = new List<HandleTurn> ();
    public List<GameObject> HeroesInGame = new List<GameObject>();
    public List<GameObject> EnemiesInBattle = new List<GameObject> ();

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

    public List<GameObject> HeroesToManage = new List<GameObject> ();
    private HandleTurn HeroChoice;

    public GameObject enemyButton;
    public Transform Spacer;
    
    public GameObject AttackPanel;
    public GameObject EnemySelectPanel;
    public GameObject MagicPanel;
    public GameObject SwitchPositionsPanel;

    //attack of heroes
    public Transform actionSpacer;
    public Transform magicSpacer;
    public Transform switchPositionsSpacer;
    public GameObject actionButton;
    public GameObject magicButton;
    public GameObject switchPositionsButton;
    private List<GameObject> attackButtons = new List<GameObject>();

    //enemy buttons
    private List<GameObject> enemyButtons = new List<GameObject>();

    private GameObject firstHeroToSwitch;
    private GameObject secondHeroToSwitch;

    // Start is called before the first frame update
    void Start()
    {
        battleStates = PerformAction.WAIT;

        // Add heroes and enemies to their respective lists
        EnemiesInBattle.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        HeroesInGame.AddRange(GameObject.FindGameObjectsWithTag("Hero"));

        // Sort heroes by name
        HeroesInGame.Sort((x, y) => string.Compare(x.name, y.name));

        AttackPanel.SetActive(false);
        EnemySelectPanel.SetActive(false);
        MagicPanel.SetActive(false);
        SwitchPositionsPanel.SetActive(false);

        EnemyButtons();
    }

    public void OnSwitchPositionsButton()
    {
        // Activate hero selection for switching positions
        HeroInput = HEROGUI.SWITCHPOSITIONS;
        Debug.Log("Switch positions mode activated. Select two heroes.");
    }

    public void SelectHeroForSwitch(GameObject selectedHero)
    {
        if (HeroInput == HEROGUI.SWITCHPOSITIONS)
        {
            if (firstHeroToSwitch == null)
            {
                firstHeroToSwitch = selectedHero;
                firstHeroToSwitch.GetComponent<HeroStateMachine>().Selector.SetActive(true); // Highlight the first hero
                Debug.Log($"First hero selected: {firstHeroToSwitch.name}");
            }
            else if (secondHeroToSwitch == null)
            {
                secondHeroToSwitch = selectedHero;
                secondHeroToSwitch.GetComponent<HeroStateMachine>().Selector.SetActive(true); // Highlight the second hero
                Debug.Log($"Second hero selected: {secondHeroToSwitch.name}");

                // Perform the swap
                SwapHeroPositions();
            }
        }
    }

    private void SwapHeroPositions()
    {
        if (firstHeroToSwitch != null && secondHeroToSwitch != null)
        {
            // Swap positions
            Vector3 tempPosition = firstHeroToSwitch.transform.position;
            firstHeroToSwitch.transform.position = secondHeroToSwitch.transform.position;
            secondHeroToSwitch.transform.position = tempPosition;

            Debug.Log($"Swapped positions: {firstHeroToSwitch.name} and {secondHeroToSwitch.name}");

            // Deactivate selectors
            firstHeroToSwitch.GetComponent<HeroStateMachine>().Selector.SetActive(false);
            secondHeroToSwitch.GetComponent<HeroStateMachine>().Selector.SetActive(false);

            // Clear selections
            firstHeroToSwitch = null;
            secondHeroToSwitch = null;

            // Hide the switch positions panel
            SwitchPositionsPanel.SetActive(false);

            // Reset AttackPanel buttons
            AttackPanel.SetActive(false);
            CreateAttackButtons(); // Refresh buttons to avoid duplicates

            // Return to normal state
            HeroInput = HEROGUI.ACTIVATE;
            AttackPanel.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch(battleStates)
        {
            case(PerformAction.WAIT):
                if(PerformList.Count > 0)
                {
                    battleStates = PerformAction.TAKEACTION;
                }
            break;

            case(PerformAction.TAKEACTION):
                GameObject performer = PerformList[0].AttackersGameObject;
                if(PerformList[0].Type == "Enemy")
                {
                    EnemyStateMachine ESM = performer.GetComponent<EnemyStateMachine> ();
                        for(int i = 0; i < HeroesInGame.Count; i++)
                        {
                            if(PerformList[0].AttackersTarget == HeroesInGame[i])
                            {
                                ESM.HeroToAttack = PerformList[0].AttackersTarget;
                                ESM.currentState = EnemyStateMachine.TurnState.ACTION;
                                break;
                            }
                            else
                            {
                                PerformList[0].AttackersTarget = HeroesInGame[Random.Range(0,HeroesInGame.Count)];
                                ESM.HeroToAttack = PerformList[0].AttackersTarget;
                                ESM.currentState = EnemyStateMachine.TurnState.ACTION;
                            }
                        }
                }

                if(PerformList[0].Type == "Hero")
                {
                    HeroStateMachine HSM = performer.GetComponent<HeroStateMachine> ();
                    HSM.EnemyToAttack = PerformList[0].AttackersTarget;
                    HSM.currentState = HeroStateMachine.TurnState.ACTION;
                }
                battleStates = PerformAction.PERFORMACTION;
            break;

            case(PerformAction.PERFORMACTION):
                //idle
            break;

            case(PerformAction.CHECKALIVE):
                if(HeroesInGame.Count < 1)
                {
                    battleStates = PerformAction.LOSE;
                }
                else if (EnemiesInBattle.Count < 1)
                {
                    battleStates = PerformAction.WIN;
                }
                else
                {
                    //call function
                    clearAttackPanel();
                    HeroInput = HEROGUI.ACTIVATE;
                }
            break;

            case(PerformAction.LOSE):
                {
                    Debug.Log("You lost the battle. :(");
                }
            break;

            case(PerformAction.WIN):
                {
                    Debug.Log("You won the battle. :D");
                    for(int i = 0; i < HeroesInGame.Count; i++)
                    {
                        HeroesInGame[i].GetComponent<HeroStateMachine>().currentState = HeroStateMachine.TurnState.WAITING;
                    }
                }
            break;
        }

        switch (HeroInput)
        {
            case(HEROGUI.ACTIVATE):
                if(HeroesToManage.Count > 0)
                {
                    HeroesToManage[0].transform.Find("Selector").gameObject.SetActive(true);
                    //create new handleturn instance
                    HeroChoice = new HandleTurn();

                    AttackPanel.SetActive(true);
                    //populate action buttons
                    CreateAttackButtons();
                    HeroInput = HEROGUI.WAITING;
                }
            break;

            case(HEROGUI.WAITING):
                //idle state
            break;

            case(HEROGUI.SWITCHPOSITIONS):
                // Waiting for two heroes to be selected
            break;

            case(HEROGUI.DONE):
                HeroInputDone();
            break;
        }
    }

    public void CollectActions(HandleTurn input)
    {
        PerformList.Add (input);
    }

    public void EnemyButtons()
    {
        //cleanup
        foreach(GameObject enemyButton in enemyButtons)
        {
            Destroy(enemyButton);
        }
        enemyButtons.Clear();

        //create buttons
        foreach(GameObject enemy in EnemiesInBattle)
        {
            GameObject newButton = Instantiate(enemyButton) as GameObject;
            EnemySelectButton button = newButton.GetComponent<EnemySelectButton>();

            EnemyStateMachine cur_enemy = enemy.GetComponent<EnemyStateMachine>();

            Text buttonText = newButton.GetComponentInChildren<Text>();
            buttonText.text = cur_enemy.enemy.characterName;

            button.EnemyPrefab = enemy;

            newButton.transform.SetParent(Spacer, false);
            enemyButtons.Add(newButton);
        }
    }

    public void Input1() //attack button
    {
        HeroChoice.Attacker = HeroesToManage[0].name;
        HeroChoice.AttackersGameObject = HeroesToManage[0];
        HeroChoice.Type = "Hero";
        HeroChoice.chosenAttack = HeroesToManage[0].GetComponent<HeroStateMachine>().hero.attacks[0];
        AttackPanel.SetActive(false);
        EnemySelectPanel.SetActive(true);
    }

    public void Input2(GameObject chosenEnemy) //enemy selection
    {
        HeroChoice.AttackersTarget = chosenEnemy;
        HeroInput = HEROGUI.DONE;
    }

    void HeroInputDone()
    {
        PerformList.Add(HeroChoice);
        EnemySelectPanel.SetActive(false);

        //clean the attackpanel
        foreach(GameObject attackButton in attackButtons)
        {
            Destroy(attackButton);
        }

        attackButtons.Clear();

        HeroesToManage[0].transform.Find("Selector").gameObject.SetActive(false);
        HeroesToManage.RemoveAt(0);
        HeroInput = HEROGUI.ACTIVATE;
    }

    void clearAttackPanel()
    {
        EnemySelectPanel.SetActive(false);
        AttackPanel.SetActive(false);
        MagicPanel.SetActive(false);

        foreach (GameObject attackButton in attackButtons)
        {
            Destroy(attackButton);
        }
        attackButtons.Clear();
    }

    //create action buttons
    void CreateAttackButtons()
    {
        // Clear existing buttons
        foreach (Transform child in actionSpacer)
        {
            Destroy(child.gameObject);
        }

        attackButtons.Clear(); // Clear the internal list to prevent redundant references

        // Create Attack Button
        GameObject AttackButton = Instantiate(actionButton) as GameObject;
        Text AttackButtonText = AttackButton.transform.Find("Text").gameObject.GetComponent<Text>();
        AttackButtonText.text = "Attack";
        AttackButton.GetComponent<Button>().onClick.AddListener(() => Input1());
        AttackButton.transform.SetParent(actionSpacer, false);
        attackButtons.Add(AttackButton);

        // Create Magic Button
        GameObject MagicAttackButton = Instantiate(actionButton) as GameObject;
        Text MagicAttackButtonText = MagicAttackButton.transform.Find("Text").gameObject.GetComponent<Text>();
        MagicAttackButtonText.text = "Magic";
        MagicAttackButton.GetComponent<Button>().onClick.AddListener(() => Input3());
        MagicAttackButton.transform.SetParent(actionSpacer, false);
        attackButtons.Add(MagicAttackButton);

        // Create Switch Positions Button
        GameObject SwitchPositionsButton = Instantiate(actionButton) as GameObject;
        Text SwitchButtonText = SwitchPositionsButton.transform.Find("Text").gameObject.GetComponent<Text>();
        SwitchButtonText.text = "Switch Positions";
        SwitchPositionsButton.GetComponent<Button>().onClick.AddListener(() => InputSwitchPositions());
        SwitchPositionsButton.transform.SetParent(actionSpacer, false);
        attackButtons.Add(SwitchPositionsButton);
    }

    public void Input4(BaseAttack chosenMagic) //choosng a magic attack
    {
        HeroChoice.Attacker = HeroesToManage[0].name;
        HeroChoice.AttackersGameObject = HeroesToManage[0];
        HeroChoice.Type = "Hero";

        HeroChoice.chosenAttack = chosenMagic;
        MagicPanel.SetActive(false);
        EnemySelectPanel.SetActive(true);
    }

    public void Input3() // switching to magic attacks
    {
        AttackPanel.SetActive(false);
        MagicPanel.SetActive(true);
    }

    public void InputSwitchPositions()
    {
        Debug.Log("Switch Positions mode activated!");
        HeroInput = HEROGUI.SWITCHPOSITIONS;

        // Hide the attack panel and show the switch positions panel
        AttackPanel.SetActive(false);
        SwitchPositionsPanel.SetActive(true);

        // Populate the panel with hero buttons
        PopulateSwitchPositionsPanel();

        Debug.Log("Select two heroes to swap their positions.");
    }

    void PopulateSwitchPositionsPanel()
    {
        // Clear existing buttons in the SwitchPositionsPanel
        foreach (Transform child in switchPositionsSpacer)
        {
            Destroy(child.gameObject);
        }

        // Create a button for each hero
        foreach (GameObject hero in HeroesInGame)
        {
            GameObject heroButton = Instantiate(actionButton); // Use your prefab for buttons
            Text buttonText = heroButton.transform.Find("Text").GetComponent<Text>();

            // Retrieve the hero's characterName from its HeroStateMachine
            HeroStateMachine heroState = hero.GetComponent<HeroStateMachine>();
            if (heroState != null && heroState.hero != null)
            {
                buttonText.text = heroState.hero.characterName; // Set button text to the hero's assigned name
            }
            else
            {
                buttonText.text = "Unnamed Hero"; // Fallback in case something is missing
            }

            // Add an OnClick listener for hero selection
            heroButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                SelectHeroForSwitch(hero);
            });

            // Set the button as a child of the SwitchPositionsSpacer
            heroButton.transform.SetParent(switchPositionsSpacer, false);
        }
    }
}
