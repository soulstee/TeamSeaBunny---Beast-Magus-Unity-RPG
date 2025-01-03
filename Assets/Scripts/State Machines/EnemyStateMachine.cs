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
        switch (currentState)
        {
            case TurnState.PROCESSING:
                UpdateProgressBar();
                break;

            case TurnState.CHOOSEACTION:
                ChooseAction();
                currentState = TurnState.WAITING;
                break;

            case TurnState.WAITING:
                // Idle state
                break;

            case TurnState.ACTION:
                StartCoroutine(TimeForAction());
                break;

            case TurnState.DEAD:
                HandleDeath();
                break;
        }
    }

    private void UpdateProgressBar()
    {
        cur_coolddown += Time.deltaTime;
        if (cur_coolddown >= max_cooldown)
        {
            currentState = TurnState.CHOOSEACTION;
        }
    }

    private void ChooseAction()
    {
        if (BSM.HeroesInGame.Count == 0)
        {
            Debug.LogWarning("No heroes left to target.");
            return;
        }

        // Create a new action for this enemy
        HandleTurn enemyAttack = new HandleTurn
        {
            Attacker = enemy.characterName,
            Type = "Enemy",
            AttackersGameObject = this.gameObject,
            AttackersTarget = BSM.HeroesInGame[Random.Range(0, BSM.HeroesInGame.Count)], // Random hero target
            chosenAttack = enemy.attacks[Random.Range(0, enemy.attacks.Count)] // Random attack
        };

        Debug.Log($"{enemy.characterName} prepares to attack with {enemyAttack.chosenAttack.attackName}");
        BSM.PerformList.Add(enemyAttack); // Add to PerformList
    }

    private IEnumerator TimeForAction()
    {
        if (actionStarted)
        {
            yield break;
        }

        actionStarted = true;

        // Trigger attack animation
        animator.SetTrigger("Attack");

        // Move slightly forward
        Vector3 forwardPosition = new Vector3(startPosition.x + moveDistance, startPosition.y, startPosition.z);
        while (MoveTowardsTarget(forwardPosition))
        {
            yield return null;
        }

        // Wait briefly before attacking
        yield return new WaitForSeconds(0.5f);

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
            // Apply damage to all heroes or a specific hero
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
        BSM.EnemiesInBattle.Remove(this.gameObject);
        Selector.SetActive(false);

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

        this.gameObject.GetComponent<SpriteRenderer>().material.color = new Color32(105, 105, 105, 255);
        gameObject.GetComponent<Animator>().enabled = false;

        BSM.battleStates = BattleStateMachine.PerformAction.CHECKALIVE;
        alive = false;
    }

    public void TakeDamage(float getDamageAmount)
    {
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
        }
        else
        {
            currentState = TurnState.WAITING;
        }
    }
}
