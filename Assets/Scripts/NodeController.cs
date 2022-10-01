using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeController : MonoBehaviour
{
    public bool canMoveLeft = false;
    public bool canMoveRight = false;
    public bool canMoveUp = false;
    public bool canMoveDown = false;

    public GameObject nodeLeft;
    public GameObject nodeRight;
    public GameObject nodeUp;
    public GameObject nodeDown;


    public bool isWarpRightNode = false;
    public bool isWarpLeftNode = false;
    public bool isGhostStartingNode = false;

    //if the node contains pellet when the game starts
    public bool isPelletNode = false;
    // if the node still has a pellet
    public bool hasPellet = false;

    public SpriteRenderer pelletStripe;

    public GameManager gameManager;

    public bool isSideNode = false;

    public bool isPowerPellet = false;

    public float powerPelletBlinkingTimer = 0;

    // Start is called before the first frame update
    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        float distance;

        if (transform.childCount > 0)
        {
            gameManager.GotPelletFromNodeController(this);
            hasPellet = true;
            isPelletNode = true;
            pelletStripe = GetComponentInChildren<SpriteRenderer>();
        }
        RaycastHit2D[] hitsDown;
        //Shoot raycast (line) going down
        hitsDown = Physics2D.RaycastAll(transform.position, -Vector2.up);

        distance = 20.0f; //Large distance value for start
        //loop through all of the gameObjects that the raycast hits
        for (int i = 0; i < hitsDown.Length; i++)
        {
            float currentDistance = Mathf.Abs(hitsDown[i].point.y - transform.position.y);
            if((currentDistance < distance) && currentDistance < 0.4f && hitsDown[i].collider.tag != "Player" && hitsDown[i].collider.tag != "Enemy")
            {
                canMoveDown = true;
                nodeDown = hitsDown[i].collider.gameObject;
                distance = currentDistance;
            }
        }


        RaycastHit2D[] hitsUp;
        //Shoot raycast (line) going down
        hitsUp = Physics2D.RaycastAll(transform.position, Vector2.up);

        distance = 20.0f; //Large distance value for start
        //loop through all of the gameObjects that the raycast hits
        for (int i = 0; i < hitsUp.Length; i++)
        {
            float currentDistance = Mathf.Abs(hitsUp[i].point.y - transform.position.y);
            if ((currentDistance < distance) && currentDistance < 0.4f && hitsUp[i].collider.tag != "Player" && hitsUp[i].collider.tag != "Enemy")
            {
                canMoveUp = true;
                nodeUp = hitsUp[i].collider.gameObject;
                distance = currentDistance;
            }
        }

        RaycastHit2D[] hitsLeft;
        //Shoot raycast (line) going down
        hitsLeft = Physics2D.RaycastAll(transform.position, Vector2.left);

        distance = 20.0f; //Large distance value for start
        //loop through all of the gameObjects that the raycast hits
        for (int i = 0; i < hitsLeft.Length; i++)
        {
            float currentDistance = Mathf.Abs(hitsLeft[i].point.x - transform.position.x);
            if ((currentDistance < distance) && currentDistance < 0.4f && hitsLeft[i].collider.tag != "Player" && hitsLeft[i].collider.tag != "Enemy")
            {
                canMoveLeft = true;
                nodeLeft = hitsLeft[i].collider.gameObject;
                distance = currentDistance;
            }
        }

        RaycastHit2D[] hitsRight;
        //Shoot raycast (line) going down
        hitsRight = Physics2D.RaycastAll(transform.position, -Vector2.left);

        distance = 20.0f; //Large distance value for start
        //loop through all of the gameObjects that the raycast hits
        for (int i = 0; i < hitsRight.Length; i++)
        {
            float currentDistance = Mathf.Abs(hitsRight[i].point.x - transform.position.x);
            if ((currentDistance < distance) && currentDistance < 0.4f && hitsRight[i].collider.tag != "Player" && hitsRight[i].collider.tag != "Enemy")
            {
                canMoveRight = true;
                nodeRight = hitsRight[i].collider.gameObject;
                distance = currentDistance;
            }
        }

        if (isGhostStartingNode)
        {
            canMoveDown = true;
            nodeDown = gameManager.ghostNodeCenter;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.gameIsRunning)
        {
            return;
        }

        if (isPowerPellet && hasPellet)
        {
            powerPelletBlinkingTimer += Time.deltaTime;
            if (powerPelletBlinkingTimer >= 0.1f)
            {
                powerPelletBlinkingTimer = 0;
                pelletStripe.enabled = !pelletStripe.enabled;
            }
        }
    }

    public GameObject GetNodeFromDirection(string direction)
    {
        if (direction == "left" && canMoveLeft)
        {
            return nodeLeft;
        }
        else if (direction == "right" && canMoveRight)
        {
            return nodeRight;
        }
        else if (direction == "up" && canMoveUp)
        {
            return nodeUp;
        }
        else if (direction == "down" && canMoveDown)
        {
            return nodeDown;
        }
        else
        {
            return null;
        }
    }

    public void RespawnPellet()
    {
        if (isPelletNode)
        {
            hasPellet = true;
            pelletStripe.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player" && hasPellet)
        {
            hasPellet = false;
            pelletStripe.enabled = false;
            StartCoroutine(gameManager.CollectedPellet(this));
        }
    }
}
