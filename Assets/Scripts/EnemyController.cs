using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum GhostNodeState
    {
        respawning,
        leftNode,
        rightNode,
        centerNode,
        startNode,
        movingInNodes
    }

    public GhostNodeState ghostNodeState;

    public enum GhostType
    {
        red,
        blue,
        pink,
        orange
    }

    public GhostType ghostType;
    public GhostNodeState startGhostNodeState;
    public GhostNodeState respawnState;

    public GameObject ghostNodeLeft;
    public GameObject ghostNodeRight;
    public GameObject ghostNodeCenter;
    public GameObject ghostNodeStart;

    public MovementController movementController;

    public GameObject startingNode;

    public bool readyToLeaveHome = false;

    public GameManager gameManager;

    public bool testRespawn = false;

    public bool isFrightened = false;

    public GameObject[] scatterNodes;
    public int scatterNodeIndex;

    public bool leftHomeBefore = false;

    public bool isVisible = true;

    public SpriteRenderer ghostSprite;
    public SpriteRenderer eyesSprite;

    public Animator animator;

    public Color color;

    // Start is called before the first frame update
    void Awake()
    {
        animator = GetComponent<Animator>();
        ghostSprite = GetComponent<SpriteRenderer>();

        scatterNodeIndex = 0;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        movementController = GetComponent<MovementController>();
        
        switch (ghostType)
        {
            case GhostType.red:
                startGhostNodeState = GhostNodeState.startNode;
                startingNode = ghostNodeStart;
                respawnState = GhostNodeState.centerNode;
                break;
            case GhostType.blue:
                startGhostNodeState = GhostNodeState.leftNode;
                startingNode = ghostNodeLeft;
                respawnState = GhostNodeState.leftNode;
                break;
            case GhostType.pink:
                startGhostNodeState = GhostNodeState.centerNode;
                startingNode = ghostNodeCenter;
                respawnState = GhostNodeState.centerNode;
                break;
            case GhostType.orange:
                startGhostNodeState = GhostNodeState.rightNode;
                startingNode = ghostNodeRight;
                respawnState = GhostNodeState.rightNode;
                break;
        }
        movementController.currentNode = startingNode;
        transform.position = startingNode.transform.position;
    }

    public void Setup()
    {
        animator.SetBool("moving", false);
        ghostNodeState = startGhostNodeState;
        readyToLeaveHome = false;
        
        //Reset our ghosts back to their home position
        movementController.currentNode = startingNode;
        transform.position = startingNode.transform.position;

        movementController.direction = "";
        movementController.lastMovingDirection = "";

        scatterNodeIndex = 0;

        //Set isFrightened
        isFrightened = false;

        leftHomeBefore = false;

        //Set readyToLeaveHome to be false ig they are blue or pink
        if (ghostType == GhostType.red)
        {
            readyToLeaveHome = true;
            leftHomeBefore = true;
        }
        else if (ghostType == GhostType.pink)
        {
            readyToLeaveHome = true;
        }
        setVisible(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (ghostNodeState != GhostNodeState.movingInNodes || !gameManager.isPowerPelletRunning)
        {
            isFrightened = false;
        }
        //Show our sprites
        if (isVisible)
        {
            if (ghostNodeState != GhostNodeState.respawning)
            {
                ghostSprite.enabled = true;
            }
            else
            {
                ghostSprite.enabled = false;
            }
            eyesSprite.enabled = true;
        }//Hide our sprites
        else
        {
            ghostSprite.enabled = false;
            eyesSprite.enabled = false;
        }

        if (isFrightened)
        {
            animator.SetBool("frightened", true);
            eyesSprite.enabled = false;
            ghostSprite.color = new Color(255, 255, 255, 255);
        }
        else
        {
            animator.SetBool("frightened", false);
            animator.SetBool("frightenedBlinking", false);

            ghostSprite.color = color;
        }

        if (!gameManager.gameIsRunning)
        {
            return;
        }

        if (gameManager.powerPelletTimer - gameManager.currentPowerPelletTime <= 3)
        {
            animator.SetBool("frightenedBlinking", true);
        }
        else
        {
            animator.SetBool("frightenedBlinking", false);
        }

        animator.SetBool("moving", true);

        if (testRespawn)
        {
            readyToLeaveHome = false;
            ghostNodeState = GhostNodeState.respawning;
            testRespawn = false;
        }

        if (movementController.currentNode.GetComponent<NodeController>().isSideNode)
        {
            movementController.SetSpeed(1);
        }
        else
        {
            if (isFrightened)
            {
                movementController.SetSpeed(1);
            }
            else if (ghostNodeState == GhostNodeState.respawning)
            {
                movementController.SetSpeed(7);
            }
            else
            {
                movementController.SetSpeed(2);
            }
            
        }
    }

    public void SetFrightened(bool newIsFrightened)
    {
        isFrightened = newIsFrightened;
    }

    public void ReachedCenterOfNode(NodeController nodeController)
    {
        if(ghostNodeState == GhostNodeState.movingInNodes)
        {
            leftHomeBefore = true;
            //scatter mode
            if (gameManager.currentGhostMode == GameManager.GhostMode.scatter)
            {
                DetermineGhostScatterModeDirection();
            }
            //Frightened mode
            else if (isFrightened)
            {
                string direction = GetRandomDirection();
                movementController.setDirection(direction);
            }
            //chase mode
            else
            {
                //Determine next game node to go
                switch (ghostType)
                {
                    case GhostType.red:
                        DetermineRedGhostDirection();
                        break;
                    case GhostType.blue:
                        DetermineBlueGhostDirection();
                        break;
                    case GhostType.pink:
                        DeterminePinkGhostDirection();
                        break;
                    case GhostType.orange:
                        DetermineOrangeGhostDirection();
                        break;
                }
            }
        }
        else if(ghostNodeState == GhostNodeState.respawning)
        {
            string direction = "";
            
            //We have reached our start node, move to the center node
            if (transform.position.x == ghostNodeStart.transform.position.x && (transform.position.y == ghostNodeStart.transform.position.y))
            {
                direction = "down";
            } // We have reached our center node, either finish respawn or move to the left/right node
            else if (transform.position.x == ghostNodeCenter.transform.position.x && (transform.position.y == ghostNodeCenter.transform.position.y))
            {
                if (respawnState == GhostNodeState.centerNode)
                {
                    ghostNodeState = respawnState;
                }
                else if(respawnState == GhostNodeState.leftNode)
                {
                    direction = "left";
                }
                else if (respawnState == GhostNodeState.rightNode)
                {
                    direction = "right";
                }
            }
            //If our respawn state is either left or right node, and we go to that node, leave home again
            else if(
                (transform.position.x == ghostNodeLeft.transform.position.x && transform.position.y == ghostNodeLeft.transform.position.y)
                ||(transform.position.x == ghostNodeRight.transform.position.x && transform.position.y == ghostNodeRight.transform.position.y)
                )
            {
                ghostNodeState = respawnState;
            } 
            else
            //We are in the gameboard still, locate start node
            {
                //Determine quickest direction to home
                direction = GetClosestDirection(ghostNodeStart.transform.position);
            }

            movementController.setDirection(direction);
        }
        else
        {
            //if we are ready to leave our home
            if (readyToLeaveHome)
            {
                //if we are in the left home node, move to the center
                if (ghostNodeState == GhostNodeState.leftNode)
                {
                    ghostNodeState = GhostNodeState.centerNode;
                    movementController.setDirection("right");
                }//if we are in the right home node, move to the center
                else if (ghostNodeState == GhostNodeState.rightNode)
                {
                    ghostNodeState = GhostNodeState.centerNode;
                    movementController.setDirection("left");
                }
                //if we are in the center node, move to the start node
                else if (ghostNodeState == GhostNodeState.centerNode)
                {
                    ghostNodeState = GhostNodeState.startNode;
                    movementController.setDirection("up");
                }//if we are in the start node, start moving around in the game
                else if(ghostNodeState == GhostNodeState.startNode)
                {
                    ghostNodeState = GhostNodeState.movingInNodes;
                    movementController.setDirection("left");
                }
            }
        }
    }

    string GetRandomDirection()
    {
        List<string> possibleDirections = new List<string>();
        NodeController nodeController = movementController.currentNode.GetComponent<NodeController>();

        if (nodeController.canMoveDown && movementController.lastMovingDirection != "up")
        {
            possibleDirections.Add("down");
        }
        if (nodeController.canMoveUp && movementController.lastMovingDirection != "down")
        {
            possibleDirections.Add("up");
        }
        if (nodeController.canMoveRight && movementController.lastMovingDirection != "left")
        {
            possibleDirections.Add("right");
        }
        if (nodeController.canMoveLeft && movementController.lastMovingDirection != "right")
        {
            possibleDirections.Add("left");
        }

        string direction = "";
        int randomDirectionIndex = Random.Range(0, possibleDirections.Count-1);
        direction = possibleDirections[randomDirectionIndex];
        return direction;

    }

    void DetermineGhostScatterModeDirection()
    {
        //If we reached the scatter node, add one to our scatter node index
        if (transform.position.x == scatterNodes[scatterNodeIndex].transform.position.x && transform.position.y == scatterNodes[scatterNodeIndex].transform.position.y)
        {
            scatterNodeIndex++;

            if (scatterNodeIndex == scatterNodes.Length - 1)
            {
                scatterNodeIndex = 0;
            }
        }

        string direction = GetClosestDirection(scatterNodes[scatterNodeIndex].transform.position);

        movementController.setDirection(direction);
    }

    void DetermineRedGhostDirection()
    {
        string direction = GetClosestDirection(gameManager.pacman.transform.position);
        movementController.setDirection(direction);
    }

    void DeterminePinkGhostDirection()
    {
        string pacmansDirection = gameManager.pacman.GetComponent<MovementController>().lastMovingDirection;
        float distanceBetweenNodes = 0.35f;

        Vector2 target = gameManager.pacman.transform.position;
        if (pacmansDirection == "left")
        {
            target.x -= distanceBetweenNodes * 2;
        }
        else if (pacmansDirection == "left")
        {
            target.x += distanceBetweenNodes * 2;
        }
        else if (pacmansDirection == "up")
        {
            target.y += distanceBetweenNodes * 2;
        }
        else if (pacmansDirection == "down")
        {
            target.y -= distanceBetweenNodes * 2;
        }

        string direction = GetClosestDirection(target);
        movementController.setDirection(direction);
    }

    void DetermineBlueGhostDirection()
    {
        string pacmansDirection = gameManager.pacman.GetComponent<MovementController>().lastMovingDirection;
        float distanceBetweenNodes = 0.35f;

        Vector2 target = gameManager.pacman.transform.position;
        if (pacmansDirection == "left")
        {
            target.x -= distanceBetweenNodes * 2;
        }
        else if (pacmansDirection == "left")
        {
            target.x += distanceBetweenNodes * 2;
        }
        else if (pacmansDirection == "up")
        {
            target.y += distanceBetweenNodes * 2;
        }
        else if (pacmansDirection == "down")
        {
            target.y -= distanceBetweenNodes * 2;
        }

        GameObject Blinky = gameManager.blinky;
        float xDistance = target.x - Blinky.transform.position.x;
        float yDistance = target.y - Blinky.transform.position.y;

        Vector2 blueTarget = new Vector2(target.x + xDistance, target.y + yDistance);
        string direction = GetClosestDirection(blueTarget);
        movementController.setDirection(direction);
    }

    void DetermineOrangeGhostDirection()
    {
        float distance = Vector2.Distance(gameManager.pacman.transform.position, transform.position);
        float distanceBetweenNodes = 0.35f;

        if (distance < 0)
        {
            distance *= -1;
        }

        //If we are within 8 nodes of pacman, chase it using red's logic
        if (distance <= distanceBetweenNodes * 8)
        {
            DetermineRedGhostDirection();
        }
        //Otherwise scatter mode logic
        else
        {
            //Scatter Mode
            DetermineGhostScatterModeDirection();
        }
    }

    string GetClosestDirection(Vector2 target)
    {
        float shortestDistance = 0;
        string lastMovingDirection = movementController.lastMovingDirection;
        string newDirection = "";

        NodeController nodeController = movementController.currentNode.GetComponent<NodeController>();

        //if we can move up and we aren't reversing
        if(nodeController.canMoveUp && lastMovingDirection != "down")
        {
            //Get the node above us
            GameObject nodeUp = nodeController.nodeUp;
            //Get the distance between our top node and pacman
            float distance = Vector2.Distance(nodeUp.transform.position, target);
            //If this is the shortest distance so far, set our direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "up";
            }
        }

        if (nodeController.canMoveDown && lastMovingDirection != "up")
        {
            GameObject nodeDown = nodeController.nodeDown;

            float distance = Vector2.Distance(nodeDown.transform.position, target);

            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "down";
            }
        }

        if (nodeController.canMoveLeft && lastMovingDirection != "right")
        {
            GameObject nodeLeft = nodeController.nodeLeft;

            float distance = Vector2.Distance(nodeLeft.transform.position, target);

            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "left";
            }
        }

        if (nodeController.canMoveRight && lastMovingDirection != "left")
        {
            GameObject nodeRight = nodeController.nodeRight;

            float distance = Vector2.Distance(nodeRight.transform.position, target);

            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "right";
            }
        }

        return newDirection;
    }

    public void setVisible(bool newIsVisible)
    {
        isVisible = newIsVisible;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && ghostNodeState != GhostNodeState.respawning)
        {
            //Get eaten
            if (isFrightened)
            {
                gameManager.GhostEaten();
                ghostNodeState = GhostNodeState.respawning;
            }//Eat player
            else
            {
                StartCoroutine(gameManager.PlayerEaten());
            }
        }
    }
}
