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

    // For the ProgressBar
    private float cur_coolddown = 0f;
    private float max_cooldown = 5f;
    public Image ProgressBar;
    public GameObject Selector;
    public GameObject EnemyToAttack;
    private bool actionStarted = false;
    private Vector3 startPosition;
    public float animSpeed;

    // Dead
    private bool alive = true;

    // Hero Panel
    private HeroPanelStats stats;
    public GameObject HeroPanel;
    private Transform HeroPanelSpacer;

    void Start()
    {
        // Find spacer
        HeroPanelSpacer = GameObject.Find("BattleCanvas").transform.Find("HeroPanel").transform.Find("HeroPanelSpacer");

        // Create panel, fill in info
        CreateHeroPanel();

        startPosition = transform.position;
        cur_coolddown = Random.Range(0, 2.5f); // Random initial progress
        Selector.SetActive(false);
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        currentState = TurnState.PROCESSING;
    }

    void Update()
    {
        switch (currentState)
        {
            case TurnState.PROCESSING:
                UpgradeProgressBar();
                break;

            case TurnState.ADDTOLIST:
                BSM.HeroesToManage.Add(this.gameObject);
                currentState = TurnState.WAITING;
                break;

            case TurnState.WAITING:
                // Idle
                break;

            case TurnState.ACTION:
                StartCoroutine(TimeForAction());
                break;

            case TurnState.DEAD:
                HandleDeath();
                break;
        }
    }

    void UpgradeProgressBar()
    {
        cur_coolddown += Time.deltaTime;
        float calc_cooldown = cur_coolddown / max_cooldown;
        ProgressBar.transform.localScale = new Vector3(Mathf.Clamp(calc_cooldown, 0, 1), ProgressBar.transform.localScale.y, ProgressBar.transform.localScale.z);

        if (cur_coolddown >= max_cooldown)
        {
            currentState = TurnState.ADDTOLIST;
        }
    }

    private IEnumerator TimeForAction()
    {
        if (actionStarted)
        {
            yield break;
        }

        actionStarted = true;

        // Animate the hero moving towards the enemy
        Vector3 enemyPosition = new Vector3(EnemyToAttack.transform.position.x - 1.5f, EnemyToAttack.transform.position.y, EnemyToAttack.transform.position.z);
        while (MoveTowardsTarget(enemyPosition))
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
        if (EnemyToAttack != null)
        {
            // Safely perform damage
            float calc_damage = hero.currentATK + (BSM.PerformList.Count > 0 ? BSM.PerformList[0].chosenAttack.attackDamage : 0);
            EnemyToAttack.GetComponent<EnemyStateMachine>().TakeDamage(calc_damage);
        }
        else
        {
            Debug.LogWarning("EnemyToAttack is null. Skipping damage.");
        }
    }

    private void HandleDeath()
    {
        if (!alive)
        {
            return;
        }

        // Update state
        this.gameObject.tag = "DeadHero";
        BSM.HeroesInGame.Remove(this.gameObject);
        BSM.HeroesToManage.Remove(this.gameObject);
        Selector.SetActive(false);
        BSM.AttackPanel.SetActive(false);
        BSM.EnemySelectPanel.SetActive(false);

        // Remove from PerformList
        if (BSM.HeroesInGame.Count > 0)
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
        hero.currentHP -= getDamageAmount;
        if (hero.currentHP <= 0)
        {
            hero.currentHP = 0;
            currentState = TurnState.DEAD;
        }

        UpdateHeroPanel();
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

    private void CreateHeroPanel()
    {
        HeroPanel = Instantiate(HeroPanel) as GameObject;
        stats = HeroPanel.GetComponent<HeroPanelStats>();
        stats.HeroName.text = hero.characterName;
        stats.HeroHP.text = "HP: " + hero.currentHP;
        stats.HeroMP.text = "MP: " + hero.currentMP;

        ProgressBar = stats.ProgressBar;
        HeroPanel.transform.SetParent(HeroPanelSpacer, false);
    }

    private void UpdateHeroPanel()
    {
        stats.HeroHP.text = "HP: " + hero.currentHP;
        stats.HeroMP.text = "MP: " + hero.currentMP;
    }
}
