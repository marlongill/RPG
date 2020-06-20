using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public enum NPCState { Idle, FindingPath, FollowingPath, Travelling }

[System.Serializable]
public class NPC
{
    // Private Members
    private Grid grid;
    private PathFinder pathFinder;
    private List<Vector2> currentPath = new List<Vector2>();
    private MapObject currentMap;
    private Vector2 destination;
    private GlobalObjects globalObjects;

    // Public Members
    public NPCState state = NPCState.Idle;
    public string name;
    public GameObject gameObject;
    public SpriteAnimator animator;
    public bool CanMoveToPOI = false;
    public bool CanWander = false;
    public float maxSpeed = 1.0f;
    public string VisibilityFunction = "";
    public string VisibilityParams = "";
    public string InteractFunction = "";
    public string InteractParams = "";

    public bool CanMove { get { return CanMoveToPOI || CanWander; } }

    // Private Methods 
    private void SetAnimation()
    {
        if (Mathf.Abs(destination.y - gameObject.transform.position.y) > Mathf.Abs(destination.x - gameObject.transform.position.x))
        {
            if (destination.y > gameObject.transform.position.y)
                animator.PlayAnimation("Walk_North");
            else
                animator.PlayAnimation("Walk_South");
        }
        else
        {
            if (destination.x > gameObject.transform.position.x)
                animator.PlayAnimation("Walk_East");
            else
                animator.PlayAnimation("Walk_West");
        }
    }

    private void Move()
    {
        Vector3 velocity = new Vector3(
            destination.x.CompareTo(gameObject.transform.position.x) * maxSpeed * Time.deltaTime,
            destination.y.CompareTo(gameObject.transform.position.y) * maxSpeed * Time.deltaTime,
            0
        );
        gameObject.transform.Translate(velocity);

        float dx = gameObject.transform.position.x - destination.x;
        float dy = gameObject.transform.position.y - destination.y;
        if (Mathf.Abs(dx) < 0.01 && Mathf.Abs(dy) < 0.01)
        {
            if (state == NPCState.FollowingPath)
            {
                currentPath.RemoveAt(0);
                gameObject.transform.position = new Vector3(destination.x, destination.y, -0.1f);
                if (currentPath.Count == 0)
                {
                    state = NPCState.Idle;
                    animator.PlayAnimation(animator.CurrentAnimation.Replace("Walk", "Idle"));
                }
                else
                {
                    destination = currentPath[0];
                    SetAnimation();
                }
            }
            else
            {
                state = NPCState.Idle;
                animator.PlayAnimation(animator.CurrentAnimation.Replace("Walk", "Idle"));
            }
        }

        gameObject.GetComponent<SpriteRenderer>().sortingOrder = (int)((currentMap.MapHeight - gameObject.transform.position.y) * 100) + 100;
    }


    // Public Methods
    public NPC(string n, GameObject go, MapObject map, GlobalObjects globals)
    {
        name = n;
        gameObject = go;
        animator = go.GetComponent<SpriteAnimator>();
        animator.SetSpriteRenderer(go.GetComponent<SpriteRenderer>());
        pathFinder = go.GetComponent<PathFinder>();
        grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid>();
        globalObjects = globals;
        currentMap = map;
    }

    public void RecalculatePath(GameObject objectToAvoid)
    {
        StopMoving();
        if (currentPath.Count > 0)
        {
            Vector3 currentPos = grid.WorldToCell(gameObject.transform.position);
            Vector2Int source = new Vector2Int((int)currentPos.x, (int)currentPos.y);
            Vector2Int dest = new Vector2Int((int)(currentPath[currentPath.Count - 1].x - 0.5f), (int)(currentPath[currentPath.Count - 1].y - 0.5f));
            currentPath.Clear();

            pathFinder.SetObstacles(currentMap.PathFindingGrid);
            pathFinder.SetObstacles(objectToAvoid);

            pathFinder.Initialise(new Vector2Int(currentMap.MapWidth, currentMap.MapHeight), source, dest);
            if (pathFinder.StartFindPath(10))
            {
                currentPath = pathFinder.GetFoundPath();
                if (currentPath.Count > 0)
                {
                    destination = currentPath[0];
                    state = NPCState.FollowingPath;
                    SetAnimation();
                }
                else
                    state = NPCState.Idle;
            }
            else
                state = NPCState.FindingPath;
        }
    }

    public void FindPath(Vector2Int target)
    {
        Vector3 currentPos = grid.WorldToCell(gameObject.transform.position);
        Vector2Int source = new Vector2Int((int)currentPos.x, (int)currentPos.y);
        pathFinder.SetObstacles(currentMap.PathFindingGrid);
        pathFinder.Initialise(new Vector2Int(currentMap.MapWidth, currentMap.MapHeight), source, target);
        if (pathFinder.StartFindPath(10))
        {
            currentPath = pathFinder.GetFoundPath();
            if (currentPath.Count > 0)
            {
                destination = currentPath[0];
                state = NPCState.FollowingPath;
                SetAnimation();
            }
            else
                state = NPCState.Idle;
        }
        else
            state = NPCState.FindingPath;
    }

    public void StopMoving()
    {
        state = NPCState.Idle;
        animator.Stop();
    }

    public void Update()
    {
        if (globalObjects.Paused)
            return;

        switch (state)
        {
            case NPCState.FindingPath:
                if (pathFinder.ContinueFindPath(10))
                {
                    currentPath = pathFinder.GetFoundPath();
                    if (currentPath.Count > 0)
                    {
                        destination = currentPath[0];
                        state = NPCState.FollowingPath;
                        SetAnimation();
                    }
                    else
                        state = NPCState.Idle;
                }
                break;
            case NPCState.FollowingPath:
            case NPCState.Travelling:
                Move();
                break;
            case NPCState.Idle:
                if (CanMove)
                {
                    int random = UnityEngine.Random.Range(0, 10000);
                    if (random > 9900)
                    {
                        int poi = UnityEngine.Random.Range(0, currentMap.PointsOfInterest.Count);
                        state = NPCState.FindingPath;
                        FindPath(currentMap.GetPointOfInterestByIndex(poi).Location);
                    }
                    else if (random > 9500)
                    {

                    }
                }
                break;
        }
    }
}
