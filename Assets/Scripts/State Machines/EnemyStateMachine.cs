using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Random = UnityEngine.Random;

public class EnemyStateMachine : MonoBehaviour
{
    private BattleStateMachine BSM;
    public BaseEnemy enemy;

    //public GameObject Selector;
    public GameObject target; //Hold the target of the next attack

    public enum TurnState
    {
        PROCESSING,
        CHOOSEACTION,
        WAITING,
        ACTION,
        DEAD
    }

    public TurnState currentState;

    // For the ProgressBar
    private float cur_coolddown = 0f;
    private float max_cooldown = 10f;

    // Position and movement
    private Vector3 startPosition;
    private bool actionStarted = false;
    public float moveDistance = 0.5f; // Distance to move forward
    public float animSpeed;

    // Animation
    private Animator animator;

    // Status
    private bool alive = true;

    //Special Attack
    public bool special = false;

    public GameObject damageText;
    public Transform textSpawn;

    //Hero UI
    [Header("Enemy UI")]
    public GameObject faceFrame;
    public GameObject Selector;
    public bool active = false;
    public Transform healthBar;
    public Transform manaBar;
    public Transform specialBar;
    private GameObject NewFace;

    void Start()
    {
        Selector.SetActive(false);
        currentState = TurnState.PROCESSING;
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        startPosition = transform.position;

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        //Health Bar
        UpgradeProgressBar();

        switch (currentState)
        {
            case TurnState.PROCESSING:
                Selector.SetActive(false); 
                //UpdateProgressBar();
                break;

            case TurnState.CHOOSEACTION:
                //StartCoroutine(wait(ChooseAction, 1f));
                //Selector.SetActive(true);
                if (enemy.currentHP > 0)
                {
                    ChooseAction();
                    currentState = TurnState.WAITING;
                }
                break;

            case TurnState.WAITING:
                Selector.SetActive(false);
                // Idle state
                break;

            case TurnState.ACTION:
                //Selector.SetActive(true);
                StartCoroutine(TimeForAction());
                break;

            case TurnState.DEAD:
                Selector.SetActive(false);
                HandleDeath();
                break;
        }
    }

    void UpgradeProgressBar()
    {
        healthBar.transform.localScale = new Vector3(Mathf.Clamp(1f, enemy.baseHP / enemy.baseHP, enemy.currentHP / enemy.baseHP), 1, 1);
        manaBar.transform.localScale = new Vector3(Mathf.Clamp(1f, enemy.baseMP / enemy.baseMP, enemy.currentMP / enemy.baseMP), 1, 1);
        specialBar.transform.localScale = new Vector3(Mathf.Clamp(1f, enemy.maxSP / enemy.maxSP, enemy.currentSP / enemy.maxSP), 1, 1);
    }
    public void setEnemyUI(Vector3 facePosition, GameObject Select, Transform health, Transform mana, Transform special)
    {
        NewFace = Instantiate(faceFrame, facePosition, Quaternion.identity) as GameObject;
        Selector = Select;
        healthBar = health;
        manaBar = mana;
        specialBar = special;
    }

    private void ChooseAction()
    {
        Selector.SetActive(true);
        if (BSM.HeroesInGame.Count == 0)
        {
            Debug.LogWarning("No heroes left to target.");
            return;
        }

        target = BSM.HeroesInGame[Random.Range(0, BSM.HeroesInGame.Count)]; // Hold Random hero target
        // Create a new action for this enemy
        HandleTurn enemyAttack = new HandleTurn
        {
            Attacker = enemy.characterName,
            Type = "Enemy",
            AttackersGameObject = this.gameObject,
            AttackersTarget = target, // Set hero target 
            chosenAttack = enemy.attacks[Random.Range(0, enemy.attacks.Count)] // Random attack
        };

        Debug.Log($"{enemy.characterName} prepares to attack with {enemyAttack.chosenAttack.attackName} at {enemyAttack.AttackersTarget}");
        BSM.PerformList.Add(enemyAttack); // Add to PerformList
        BSM.attackDescrption.text = $"{enemy.characterName} attack with {enemyAttack.chosenAttack.attackName} at {enemyAttack.AttackersTarget.name}";
        Selector.SetActive(false);
    }

    private IEnumerator TimeForAction()
    {
        Selector.SetActive(true);
        if (actionStarted)
        {
            yield break;
        }

        actionStarted = true;

        // Trigger attack animation
        animator.SetTrigger("Attack");

        // Move slightly forward
        Vector3 forwardPosition = new Vector3(startPosition.x + moveDistance, startPosition.y, startPosition.z);
        //Move to the traget position
        //Vector3 forwardPosition = new Vector3(target.transform.position.x - moveDistance, startPosition.y, startPosition.z);
        while (MoveTowardsTarget(forwardPosition))
        {
            yield return null;
        }

        // Wait briefly before attacking
        yield return new WaitForSeconds(1f);

        // Perform attack
        DoDamage();

        // Move back to the starting position
        while (MoveTowardsTarget(startPosition))
        {
            yield return null;
        }

        // Mark action as complete
        actionStarted = false;

        // Remove this performer from the PerformList
        if (BSM.PerformList.Count > 0)
        {
            BSM.PerformList.RemoveAt(0);
        }

        Selector.SetActive(false);
        BSM.waitOnceEnemy = true;
        // Reset state
        ResetAfterAction();
    }

    private bool MoveTowardsTarget(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }

    private void DoDamage() 
    {
        if (BSM.HeroesInGame.Count > 0)
        {
            if(special)
            {
                // Apply damage to all heroes
                foreach (GameObject hero in BSM.HeroesInGame)
                {
                    HeroStateMachine heroState = hero.GetComponent<HeroStateMachine>();
                    if (heroState != null)
                    {
                        float calc_damage = enemy.currentATK + Random.Range(0, 5); // Add randomness to the damage
                        heroState.TakeDamage(calc_damage);
                    }
                }
            }
            else
            {
                int choosenAttack = Random.Range(0, enemy.attacks.Count);
                //Attack an especific hero
                HeroStateMachine heroState = target.GetComponent<HeroStateMachine>();
                if (heroState != null)
                {
                    float calc_damage = 0f;
                    if (enemy.attacks[choosenAttack].magic)
                        calc_damage = enemy.currentATK + Random.Range(0, 5) + enemy.attacks[choosenAttack].attackDamage - heroState.hero.magicDEF;// Add randomness to the damage
                    else
                        calc_damage = enemy.currentATK + Random.Range(0, 5) + enemy.attacks[choosenAttack].attackDamage - heroState.hero.currentDEF;

                    if (calc_damage < 0f)
                        calc_damage = 0f;
                    heroState.TakeDamage(calc_damage);
                }
            }
        }
        else
        {
            Debug.LogWarning("No heroes available to attack.");
        }
    }

    private void HandleDeath()
    {
        if (!alive)
        {
            return;
        }

        this.gameObject.tag = "DeadEnemy";
        Destroy(NewFace);
        BSM.EnemiesInBattle.Remove(this.gameObject);
        Selector.SetActive(false);
        animator.SetTrigger("Death");

        if (BSM.EnemiesInBattle.Count > 0)
        {
            for (int i = 0; i < BSM.PerformList.Count; i++)
            {
                if (i != 0)
                {
                    if (BSM.PerformList[i].AttackersGameObject == this.gameObject)
                    {
                        BSM.PerformList.Remove(BSM.PerformList[i]);
                    }
                }    
            }
        }
        BSM.battleStates = BattleStateMachine.PerformAction.CHECKALIVE;
        alive = false;
    }
    
    public void TakeDamage(float getDamageAmount)
    {
        bool once = true;
        if (once)
        {
            GameObject text = Instantiate(damageText, textSpawn.position, Quaternion.identity) as GameObject;
            text.GetComponent<TMP_Text>().text = $"{getDamageAmount}";
            once = false;
        }

        animator.SetTrigger("Hurt");
        enemy.currentHP -= getDamageAmount;
        if (enemy.currentHP <= 0)
        {
            enemy.currentHP = 0;
            currentState = TurnState.DEAD;
        }
    }
    
    private void ResetAfterAction()
    {
        if (BSM.battleStates != BattleStateMachine.PerformAction.WIN && BSM.battleStates != BattleStateMachine.PerformAction.LOSE)
        {
            BSM.battleStates = BattleStateMachine.PerformAction.WAIT;
            cur_coolddown = 0f;
            currentState = TurnState.PROCESSING;
            BSM.heroTurn = true;
        }
        else
        {
            currentState = TurnState.WAITING;
        }
    }
}
