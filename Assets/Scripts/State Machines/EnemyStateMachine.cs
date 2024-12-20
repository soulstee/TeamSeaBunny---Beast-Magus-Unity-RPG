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
    public GameObject HeroToAttack;
    public float animSpeed;

    // Status
    private bool alive = true;

    void Start()
    {
        Selector.SetActive(false);
        currentState = TurnState.PROCESSING;
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        startPosition = transform.position;
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

        // Create a new action
        HandleTurn enemyAttack = new HandleTurn
        {
            Attacker = enemy.characterName,
            Type = "Enemy",
            AttackersGameObject = this.gameObject,
            AttackersTarget = BSM.HeroesInGame[Random.Range(0, BSM.HeroesInGame.Count)], // Random hero target
            chosenAttack = enemy.attacks[Random.Range(0, enemy.attacks.Count)] // Random attack
        };

        Debug.Log($"{enemy.characterName} attacks {enemyAttack.AttackersTarget.name} with {enemyAttack.chosenAttack.attackName}");
        BSM.PerformList.Add(enemyAttack); // Add to the PerformList
    }

    private IEnumerator TimeForAction()
    {
        if (actionStarted)
        {
            yield break;
        }

        actionStarted = true;

        // Animate moving towards the hero
        Vector3 heroPosition = new Vector3(HeroToAttack.transform.position.x + 1.5f, HeroToAttack.transform.position.y, HeroToAttack.transform.position.z);
        while (MoveTowardsTarget(heroPosition))
        {
            yield return null;
        }

        // Wait briefly before attacking
        yield return new WaitForSeconds(1.0f);

        // Perform attack
        DoDamage();

        // Animate back to starting position
        while (MoveTowardsTarget(startPosition))
        {
            yield return null;
        }

        // Remove this performer from the PerformList
        if (BSM.PerformList.Count > 0)
        {
            BSM.PerformList.RemoveAt(0);
        }

        // Reset state
        ResetAfterAction();

        actionStarted = false; // Mark action as complete
    }

    private bool MoveTowardsTarget(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }

    private void DoDamage()
    {
        if (HeroToAttack != null)
        {
            // Safely perform damage
            float calc_damage = enemy.currentATK + (BSM.PerformList.Count > 0 ? BSM.PerformList[0].chosenAttack.attackDamage : 0);
            HeroToAttack.GetComponent<HeroStateMachine>().TakeDamage(calc_damage);
        }
        else
        {
            Debug.LogWarning("HeroToAttack is null. Skipping damage.");
        }
    }

    private void HandleDeath()
    {
        if (!alive)
        {
            return;
        }

        // Update state
        this.gameObject.tag = "DeadEnemy";
        BSM.EnemiesInBattle.Remove(this.gameObject);
        Selector.SetActive(false);

        // Remove from PerformList
        if (BSM.EnemiesInBattle.Count > 0)
        {
            for (int i = 0; i < BSM.PerformList.Count; i++)
            {
                if (BSM.PerformList[i].AttackersGameObject == this.gameObject)
                {
                    BSM.PerformList.Remove(BSM.PerformList[i]);
                }
                else if (BSM.PerformList[i].AttackersTarget == this.gameObject)
                {
                    BSM.PerformList[i].AttackersTarget = BSM.HeroesInGame[Random.Range(0, BSM.HeroesInGame.Count)];
                }
            }
        }

        // Change appearance to indicate death
        this.gameObject.GetComponent<SpriteRenderer>().material.color = new Color32(105, 105, 105, 255);

        // Update battle state
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
