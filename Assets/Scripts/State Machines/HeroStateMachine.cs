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

    // Progress Bar
    public float cur_coolddown = 0f;
    public float max_cooldown = 5f;
    public Image ProgressBar;
    public GameObject Selector;
    public GameObject EnemyToAttack;
    private bool actionStarted = false;
    private Vector3 startPosition;
    public float animSpeed;
    int count = 0; //Count the amount of times it runs

    // Animation
    private Animator animator;

    // Dead
    private bool alive = true;

    // Hero Panel
    private HeroPanelStats stats;
    public GameObject HeroPanel;
    private Transform HeroPanelSpacer;

    void Start()
    {
        HeroPanelSpacer = GameObject.Find("BattleCanvas").transform.Find("HeroPanel").transform.Find("HeroPanelSpacer");
        CreateHeroPanel();

        startPosition = transform.position;
        //cur_coolddown = Random.Range(0, 2.5f);
        Selector.SetActive(false);
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        currentState = TurnState.PROCESSING;

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        UpgradeProgressBar();

        switch (currentState)
        {
            case TurnState.PROCESSING:
                //UpgradeProgressBar();
                currentState = TurnState.ADDTOLIST;
                break;

            case TurnState.ADDTOLIST:
                BSM.HeroesToManage.Add(this.gameObject);
                currentState = TurnState.WAITING;
                break;

            case TurnState.WAITING:
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
        //////////////////////////////////////////
        stats.HeroHP.text = "HP: " + hero.currentHP;
        stats.HeroMP.text = "MP: " + hero.currentMP;
        ////////////////////////////////////////////
        
        cur_coolddown += Time.deltaTime;
        float calc_cooldown = cur_coolddown / max_cooldown;
        ProgressBar.transform.localScale = new Vector3(Mathf.Clamp(calc_cooldown, hero.baseHP / hero.baseHP, hero.currentHP / hero.baseHP), ProgressBar.transform.localScale.y, ProgressBar.transform.localScale.z);

        /*if (cur_coolddown >= max_cooldown)
        {
            currentState = TurnState.ADDTOLIST;
        }*/
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
        animator.SetTrigger("Magic");

        // Move slightly forward
        Vector3 forwardPosition = new Vector3(startPosition.x + 0.5f, startPosition.y, startPosition.z);
        while (MoveTowardsTarget(forwardPosition))
        {
            yield return null;
        }

        // Wait for animation to play
        yield return new WaitForSeconds(0.5f);

        // Perform attack
        DoDamage();

        // Move back to the starting position
        while (MoveTowardsTarget(startPosition))
        {
            yield return null;
        }

        // Reset state
        ResetAfterAction();
        actionStarted = false;
    }

    private bool MoveTowardsTarget(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }

    private void DoDamage()
    {
        if (EnemyToAttack != null)
        {
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

        this.gameObject.tag = "DeadHero";
        BSM.HeroesInGame.Remove(this.gameObject);
        BSM.HeroesToManage.Remove(this.gameObject);
        Selector.SetActive(false);

        if (BSM.HeroesInGame.Count > 0)
        {
            for (int i = 0; i < BSM.PerformList.Count; i++)
            {
                if (i != 0)
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
        }

        this.gameObject.GetComponent<SpriteRenderer>().material.color = new Color32(105, 105, 105, 255);
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

        //UpdateHeroPanel();
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

    /*private void UpdateHeroPanel()
    {
        stats.HeroHP.text = "HP: " + hero.currentHP;
        stats.HeroMP.text = "MP: " + hero.currentMP;
    }*/
}
