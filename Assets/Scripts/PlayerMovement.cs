using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;

    public Rigidbody2D rb;
    public Animator animator;

    Vector2 movement;
    Vector3 curPos, lastPos;

    void Start()
    {
        if (GameManager.instance.isFirstGameStart)
        {
            // Center the player in the scene on first game start
            transform.position = Vector2.zero; // Adjust as needed for exact center
            GameManager.instance.isFirstGameStart = false; // Set flag to false
            Debug.Log("Player spawned at the center of the scene for the first time.");
        }
        else if (GameManager.instance.nextSpawnPoint != "")
        {
            GameObject spawnPoint = GameObject.Find(GameManager.instance.nextSpawnPoint);
            if (spawnPoint != null)
            {
                transform.position = spawnPoint.transform.position;
                Debug.Log($"Spawned at: {spawnPoint.name}");
            }
            else
            {
                Debug.LogWarning($"Spawn point '{GameManager.instance.nextSpawnPoint}' not found. Using default position.");
            }
            GameManager.instance.nextSpawnPoint = "";
        }
        else if (GameManager.instance.lastHeroPosition != Vector2.zero)
        {
            transform.position = GameManager.instance.lastHeroPosition;
            Debug.Log("Spawned at last known position.");
            GameManager.instance.lastHeroPosition = Vector2.zero;
        }
        else
        {
            Debug.LogWarning("No spawn point or last position set. Character may spawn at (0,0).");
        }

        if (PlayerPrefs.HasKey("PlayerX"))
        {
            float x = PlayerPrefs.GetFloat("PlayerX");
            float y = PlayerPrefs.GetFloat("PlayerY");
            float z = PlayerPrefs.GetFloat("PlayerZ");
            transform.position = new Vector3(x, y, z);
        }
    }


    // Update is called once per frame
    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);

        //This handles enemy encounters
        curPos = transform.position;
        if(curPos == lastPos)
        {
            GameManager.instance.isWalking = false;
        }
        else
        {
            GameManager.instance.isWalking = true;
        }
        lastPos = curPos;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Teleporter")
        {
            CollisionHandler col = other.gameObject.GetComponent<CollisionHandler>();
            GameManager.instance.nextSpawnPoint = col.spawnPointName;
            GameManager.instance.sceneToLoad = col.sceneToLoad;
            GameManager.instance.LoadNextScene();
        }

        // if(other.tag == "EnterArea")
        // {
        //     CollisionHandler col = other.gameObject.GetComponent<CollisionHandler>();
        //     GameManager.instance.nextHeroPosition = col.spawnPoint.transform.position;
        //     GameManager.instance.sceneToLoad = col.sceneToLoad;
        //     GameManager.instance.LoadNextScene();
        // }

        // if(other.tag == "LeaveArea")
        // {
        //     CollisionHandler col = other.gameObject.GetComponent<CollisionHandler>();
        //     GameManager.instance.nextHeroPosition = col.spawnPoint.transform.position;
        //     GameManager.instance.sceneToLoad = col.sceneToLoad;
        //     GameManager.instance.LoadNextScene();
        // }

        if (other.tag == "Region1")
        {
            GameManager.instance.currentRegions = 0;
            GameManager.instance.gotAttacked = true;
        }
        if (other.tag == "Region2")
        {
            GameManager.instance.currentRegions = 1;
            //GameManager.instance.loadCabin();
            GameManager.instance.gotAttacked = true;
        }

        if (other.tag == "Region3")
        {
            GameManager.instance.currentRegions = 2;
            GameManager.instance.gotAttacked = true;
        }

        if (other.tag == "Region4")
        {
            GameManager.instance.currentRegions = 3;
        }

        if (other.tag == "Region5")
        {
            GameManager.instance.currentRegions = 4;
        }

        if (other.tag == "Region6")
        {
            GameManager.instance.currentRegions = 5;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Region1" || other.tag == "Region2" || other.tag == "Region3" || other.tag == "Region4" || other.tag == "Region5" || other.tag == "Region6")
        {
            GameManager.instance.canGetEncounter = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Region1" || other.tag == "Region2" || other.tag == "Region3" || other.tag == "Region4" || other.tag == "Region5" || other.tag == "Region6")
        {
            GameManager.instance.canGetEncounter = false;
        }
    }
}
