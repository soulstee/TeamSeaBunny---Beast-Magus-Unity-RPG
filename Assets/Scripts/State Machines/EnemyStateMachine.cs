using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    private BattleStateMachine BSM;
    public BaseEnemy enemy;

    public GameObject Selector;

    public enum TurnState
    {
        PROCESSING,
        CHOOSEACTION,
        WAITING,
        ACTION,
        DEAD
    }

    public TurnState currentState;

    //for the ProgressBar
    private float cur_coolddown = 0f;
    private float max_cooldown = 10f;

    //thisgameobject
    private Vector3 startposition;
    //timeforaction stuff
    private bool actionStarted = false;
    public GameObject HeroToAttack;
    public float animSpeed;

    //alive
    private bool alive = true;

    // Start is called before the first frame update
    void Start()
    {
        Selector.SetActive(false);
        currentState = TurnState.PROCESSING;
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        startposition = transform.position;
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

            case (TurnState.CHOOSEACTION):
                ChooseAction ();
                currentState = TurnState.WAITING;
            break;

            case (TurnState.WAITING):
                //idle state
            break;

            case(TurnState.ACTION):
            StartCoroutine (TimeForAction ());
            break;

            case(TurnState.DEAD):
                if(!alive)
                {
                    return;
                }
                else
                {
                    //chnage tag of enemies
                    this.gameObject.tag = "DeadEnemy";
                    //not atackable by heroes anymore
                    BSM.EnemiesInBattle.Remove(this.gameObject);
                    //disable Selector
                    Selector.SetActive(false);
                    //remove all inputs heroattacks
                    if(BSM.EnemiesInBattle.Count > 0)
                    {
                        for (int i = 0; i < BSM.PerformList.Count; i++)
                        {
                            if (BSM.PerformList[i].AttackersGameObject == this.gameObject)
                            {
                                BSM.PerformList.Remove(BSM.PerformList[i]);
                            }
                            else if (BSM.PerformList[i].AttackersTarget == this.gameObject)
                            {
                                BSM.PerformList[i].AttackersTarget = BSM.EnemiesInBattle[Random.Range(0, BSM.EnemiesInBattle.Count)]; 
                            }
                        }
                    } 
                    //change the color to gray / play dead animation
                    this.gameObject.GetComponent<SpriteRenderer>().material.color = new Color32(105,105,105,255);
                    //set alive false
                    alive = false;
                    //reset enemybuttons
                    BSM.EnemyButtons();
                    //check alive
                    BSM.battleStates = BattleStateMachine.PerformAction.CHECKALIVE;
                }
            break;
        }
    }

    void UpgradeProgressBar()
    {
        cur_coolddown = cur_coolddown + Time.deltaTime;
        if(cur_coolddown >= max_cooldown)
        {
            currentState = TurnState.CHOOSEACTION;
        }
    }

    void ChooseAction()
    {
        HandleTurn myAttack = new HandleTurn ();
        myAttack.Attacker = enemy.characterName;
        myAttack.Type = "Enemy";
        myAttack.AttackersGameObject = this.gameObject;
        myAttack.AttackersTarget = BSM.HeroesInGame[Random.Range(0, BSM.HeroesInGame.Count)];

        int num = Random.Range(0, enemy.attacks.Count);
        myAttack.chosenAttack = enemy.attacks[num];
        Debug.Log (this.gameObject.name + " does " + myAttack.chosenAttack.attackName + ", dealing " + myAttack.chosenAttack.attackDamage + " damage!");
        BSM.CollectActions (myAttack);
    }

    private IEnumerator TimeForAction()
    {
        if(actionStarted)
        {
            yield break;
        }

        actionStarted = true;

        //animate the enemy near the hero to attack
        Vector3 heroPosition = new Vector3(HeroToAttack.transform.position.x+1.5f, HeroToAttack.transform.position.y, HeroToAttack.transform.position.z);
        while(MoveTowardsEnemy(heroPosition))
        {
            yield return null;
        }

        //wait a bit
        yield return new WaitForSeconds(1.0f);
        //do damage
        DoDamage();

        //animate back to start position
        Vector3 firstPosition = startposition;
        while(MoveTowardsEnemy(firstPosition))
        {
            yield return null;
        }

        //remove this performer from the list in BSM
        BSM.PerformList.RemoveAt(0);

        //reset battle state machine -> wait
        BSM.battleStates =  BattleStateMachine.PerformAction.WAIT;
        //end coroutine

        actionStarted =  false;
        //reset this enemy state
        cur_coolddown = 0f;
        currentState = TurnState.PROCESSING;
    }

    private bool MoveTowardsEnemy(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards (transform.position, target, animSpeed * Time.deltaTime));
    }

    private bool MoveTowardsStart(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards (transform.position, target, animSpeed * Time.deltaTime));
    }

    void DoDamage()
    {
        float calc_damage = enemy.currentATK +  BSM.PerformList[0].chosenAttack.attackDamage;
        HeroToAttack.GetComponent<HeroStateMachine> ().TakeDamage(calc_damage);
    }

    public void TakeDamage(float getDamageAmount)
    {
        enemy.currentHP -= getDamageAmount;
        if(enemy.currentHP <= 0)
        {
            enemy.currentHP = 0;
            currentState = TurnState.DEAD;
        }
    }
}
