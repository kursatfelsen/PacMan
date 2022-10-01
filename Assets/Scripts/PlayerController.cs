using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    MovementController movementController;

    public SpriteRenderer sprite;
    public Animator animator;

    public GameObject startNode;

    public Vector2 startPos;

    public GameManager gameManager;

    public bool isDead = false;

    string pressedRecently = "";

    public float currentpressTime = 0;
    public float pressTimer = 0.2f;

    // Start is called before the first frame update
    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        startPos = new Vector2(0.04f, -0.93f  );
        animator = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        movementController = GetComponent<MovementController>();
        startNode = movementController.currentNode;
    }

    public void Setup()
    {
        isDead = false;
        animator.SetBool("dead", false);
        animator.SetBool("moving", false);

        movementController.currentNode = startNode;
        movementController.direction = "left";
        movementController.lastMovingDirection = "left";
        sprite.flipX = false;
        transform.position = startPos;
        animator.speed = 1;
        animator.SetBool("moving", false);
    }

    public void Stop()
    {
        animator.speed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.gameIsRunning)
        {
            if (!isDead)
            {
                animator.speed = 0;
            }
            return;
        }
        animator.speed = 1;

        animator.SetBool("moving", true);

        currentpressTime += Time.deltaTime;
        if (currentpressTime >= pressTimer)
        {
            currentpressTime = 0;
            pressedRecently = "";
        }

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || pressedRecently == "left")
        {
            movementController.setDirection("left");
            if (pressedRecently != "left")
            {
                pressedRecently = "left";
                currentpressTime = 0;
            }
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || pressedRecently == "right")
        {
            movementController.setDirection("right");
            if (pressedRecently != "right")
            {
                pressedRecently = "right";
                currentpressTime = 0;
            }
        }
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || pressedRecently == "up")
        {
            movementController.setDirection("up");
            if (pressedRecently != "up")
            {
                pressedRecently = "up";
                currentpressTime = 0;
            }
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) || pressedRecently == "down")
        {
            movementController.setDirection("down");
            if (pressedRecently != "down")
            {
                pressedRecently = "down";
                currentpressTime = 0;
            }
        }

        bool flipX = false;
        bool flipY = false;
        if (movementController.lastMovingDirection == "left")
        {
            animator.SetInteger("direction", 0);
        }
        else if (movementController.lastMovingDirection == "right")
        {
            animator.SetInteger("direction", 0);
            flipX = true;
        }
        else if (movementController.lastMovingDirection == "up")
        {
            animator.SetInteger("direction", 1);
        }
        else if (movementController.lastMovingDirection == "down")
        {
            animator.SetInteger("direction", 1);
            flipY = true;
        }

        sprite.flipX = flipX;
        sprite.flipY = flipY;
    }

    public void Death()
    {
        isDead = true;
        animator.SetBool("moving", false);
        animator.speed = 1;
        animator.SetBool("dead", true);
    }
}
