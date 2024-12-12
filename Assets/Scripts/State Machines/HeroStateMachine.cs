using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroStateMachine : MonoBehaviour
{
    private BattleStateMachine BSM;
    public BaseHero hero;

    public enum TurnState
    {
        PROCESSING,
        ADDTOLIST,
        WAITING,
        SELECTING,
        ACTION,
        DEAD
    }

    public TurnState currentState;

    //for the ProgressBar
    private float cur_coolddown = 0f;
    private float max_cooldown = 5f;
    public Image ProgressBar;
    public GameObject Selector;
    public GameObject EnemyToAttack;
    private bool actionStarted = false;
    private Vector3 startPosition;
    public float animSpeed;

    //dead
    private bool alive = true;

    //heroPanel
    private HeroPanelStats stats;
    public GameObject HeroPanel;
    private Transform HeroPanelSpacer;

    // Start is called before the first frame update
    void Start()
    {
        //find spacer
        HeroPanelSpacer = GameObject.Find("BattleCanvas").transform.Find("HeroPanel").transform.Find("HeroPanelSpacer");
        // create panel , fill in info
        CreateHeroPanel();
        
        startPosition = transform.position;
        cur_coolddown = Random.Range(0, 2.5f); //Determines the filling of the hero bars, assign speed stat to it
        Selector.SetActive(false);
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        currentState = TurnState.PROCESSING;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log (currentState);
        switch(currentState)
        {
            case (TurnState.PROCESSING):
                UpgradeProgressBar();
            break;

            case (TurnState.ADDTOLIST):
            BSM.HeroesToManage.Add(this.gameObject);
            currentState = TurnState.WAITING;
            break;

            case (TurnState.WAITING):
            //idle
            break;

            case(TurnState.ACTION):
            StartCoroutine (TimeForAction ());
            break;

            case(TurnState.DEAD):
                if (!alive)
                {
                    return;
                }
                else
                {
                    //change tag
                    this.gameObject.tag = "DeadHero";
                    //not attackable by enemies
                    BSM.HeroesInGame.Remove(this.gameObject);
                    //not managable
                    BSM.HeroesToManage.Remove(this.gameObject);
                    //deactivate selector
                    Selector.SetActive(false);
                    //reset gui
                    BSM.AttackPanel.SetActive(false);
                    BSM.EnemySelectPanel.SetActive(false);
                    //remove item from perform list
                    if(BSM.HeroesInGame.Count > 0)
                    {
                        for(int i = 0; i < BSM.PerformList.Count; i++)
                        {
                            if(BSM.PerformList[i].AttackersGameObject == this.gameObject)
                            {
                                BSM.PerformList.Remove(BSM.PerformList[i]);
                            }

                            else if (BSM.PerformList[i].AttackersTarget == this.gameObject)
                            {
                                BSM.PerformList[i].AttackersTarget = BSM.HeroesInGame[Random.Range(0, BSM.HeroesInGame.Count)];
                            }
                        }
                    }
                    //change color or play animation
                    this.gameObject.GetComponent<SpriteRenderer>().material.color = new Color32(105,105,105,255);
                    //rest hero input
                    BSM.battleStates = BattleStateMachine.PerformAction.CHECKALIVE;
                    alive = false;
                }
            break;
        }
    }


    void UpgradeProgressBar()
    {
        cur_coolddown = cur_coolddown + Time.deltaTime;
        float calc_cooldown = cur_coolddown / max_cooldown;
        ProgressBar.transform.localScale = new Vector3(Mathf.Clamp(calc_cooldown, 0, 1), ProgressBar.transform.localScale.y, ProgressBar.transform.localScale.z);
        if(cur_coolddown >= max_cooldown)
        {
            currentState = TurnState.ADDTOLIST;
        }
    }

    private IEnumerator TimeForAction()
    {
        if(actionStarted)
        {
            yield break;
        }

        actionStarted = true;

        //animate the enemy near the hero to attack
        Vector3 enemyPosition = new Vector3(EnemyToAttack.transform.position.x-1.5f, EnemyToAttack.transform.position.y, EnemyToAttack.transform.position.z);
        while(MoveTowardsEnemy(enemyPosition))
        {
            yield return null;
        }

        //wait a bit
        yield return new WaitForSeconds(1.0f);

        //do damage
        DoDamage();
        
        //animate back to start position
        Vector3 firstPosition = startPosition;
        while(MoveTowardsEnemy(firstPosition))
        {
            yield return null;
        }

        //remove this performer from the list in BSM
        BSM.PerformList.RemoveAt(0);

        //reset battle state machine -> wait
        if(BSM.battleStates != BattleStateMachine.PerformAction.WIN && BSM.battleStates != BattleStateMachine.PerformAction.LOSE)
        {
            BSM.battleStates =  BattleStateMachine.PerformAction.WAIT;
            //reset this enemy state
            cur_coolddown = 0f;
            currentState = TurnState.PROCESSING;
        }
        else
        {
            currentState = TurnState.WAITING;
        }
        //end couroutine
        actionStarted = false;
    }

    private bool MoveTowardsEnemy(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards (transform.position, target, animSpeed * Time.deltaTime));
    }

    private bool MoveTowardsStart(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards (transform.position, target, animSpeed * Time.deltaTime));
    }

    public void TakeDamage(float getDamageAmount)
    {
        hero.currentHP -= getDamageAmount;
        if(hero.currentHP <= 0)
        {
            hero.currentHP = 0;
            currentState = TurnState.DEAD;
        }
    }

    //do damage
    void DoDamage()
    {
        float calc_damage = hero.currentATK + BSM.PerformList[0].chosenAttack.attackDamage;
        EnemyToAttack.GetComponent<EnemyStateMachine>().TakeDamage(calc_damage);
    }
    //Create a Hero Panel
    void CreateHeroPanel()
    {
        HeroPanel = Instantiate(HeroPanel) as GameObject;
        stats = HeroPanel.GetComponent<HeroPanelStats>();
        stats.HeroName.text = hero.characterName;
        stats.HeroHP.text = "HP: " + hero.currentHP;
        stats.HeroMP.text = "MP " + hero.currentMP;

        ProgressBar = stats.ProgressBar;
        HeroPanel.transform.SetParent(HeroPanelSpacer, false);
    }

    //update stats on damage/heal
    void UpdateHeroPanel()
    {
        stats.HeroHP.text = "HP: " + hero.currentHP;
        stats.HeroMP.text = "MP: " + hero.currentMP;
    }
}
