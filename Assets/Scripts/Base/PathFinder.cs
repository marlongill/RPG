using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

using UnityEngine;
using UnityEngine.Tilemaps;

public class PathNode : IComparable
{
    public bool IsObstacle = false;
    public bool Visited = false;
    public float GlobalGoal = float.MaxValue;
    public float LocalGoal = float.MaxValue;
    public int X, Y;
    public List<PathNode> Neighbours = new List<PathNode>();
    public PathNode Parent;

    public int CompareTo(object obj)
    {
        if (obj is PathNode)
            return GlobalGoal.CompareTo(((PathNode)obj).GlobalGoal);
        else
            return 1;
    }

    public float Distance(PathNode target)
    {
        Vector2 v = new Vector2(target.X - X, target.Y - Y);
        return v.magnitude;
    }

    public float Heuristic(PathNode target)
    {
        return Distance(target);
    }
}

public class PathFinder : MonoBehaviour
{
    // Private Members
    private List<List<bool>> _obstacles;
    private int _width, _height;
    private List<List<PathNode>> _nodes = new List<List<PathNode>>();
    private PathNode _start, _end;

    private PathNode _currentNode;
    private List<PathNode> _untestedNodes;

    // Private Methods
    private bool FindPath(int maxIterations)
    {
        int iterations = 0;
        while (_untestedNodes.Count > 0)
        {
            // Sort list in order of global goal
            _untestedNodes.Sort();

            // Get rid of visited items from the list
            while (_untestedNodes.Count > 0 && _untestedNodes[0].Visited)
                _untestedNodes.RemoveAt(0);
            if (_untestedNodes.Count == 0)
                break;

            // Set current node to the first item in remaining list 
            _currentNode = _untestedNodes[0];
            _currentNode.Visited = true;

            // Debug - Show Tested Nodes
            //Grid grid2 = FindObjectOfType<Grid>();
            //GameObject go2 = grid2.gameObject;
            //Transform trans2 = go2.transform.GetChild(3);
            //Tilemap map2 = trans2.GetComponent<Tilemap>();
            //MapLoader mapLoader = go2.GetComponent<MapLoader>();
            //map2.SetTile(new Vector3Int(_currentNode.X, _currentNode.Y, 0), mapLoader.CurrentMap.Tiles[208]);
            // End Debug

            // Check the neighbors of this cell
            foreach (PathNode neighbour in _currentNode.Neighbours)
            {
                if (!neighbour.Visited && !neighbour.IsObstacle)
                { 
                    _untestedNodes.Add(neighbour);

                    // Calculate the neighbours potential lowest parent distance
                    float newNeighbourGoal = _currentNode.LocalGoal + _currentNode.Distance(neighbour);
                    if (newNeighbourGoal < neighbour.LocalGoal)
                    {
                        neighbour.Parent = _currentNode;
                        neighbour.LocalGoal = newNeighbourGoal;
                        neighbour.GlobalGoal = neighbour.LocalGoal + neighbour.Heuristic(_end);
                    }
                }
            }

            iterations++;
            if (iterations > maxIterations)
                break;

            if (_currentNode == _end)
            {
                _untestedNodes.Clear();
                break;
            }
        }

        return _untestedNodes.Count == 0;
    }

    // Public Members

    // Public Methods
    public void SetObstacles(List<List<bool>> o)
    {
        _obstacles = new List<List<bool>>();
        for (int y = 0; y < o.Count; y++)
        {
            _obstacles.Add(new List<bool>());
            for (int x = 0; x < o[y].Count; x++)
            {
                _obstacles[y].Add(o[y][x]);
            }
        }
    }

    public void SetObstacles(GameObject objectToAvoid)
    {
        MapController mapController = GameObject.FindGameObjectWithTag("Grid").GetComponent<MapController>();
        List<Vector2Int> blocakges = mapController.GetBlockedCells(objectToAvoid);
        foreach (Vector2Int blocked in blocakges)
            _obstacles[blocked.y][blocked.x] = true;
    }

    public void Initialise(Vector2Int gridSize, Vector2Int start, Vector2Int end)
    {
        _width = gridSize.x; _height = gridSize.y;

        // Initialise Check Grid
        _nodes.Clear();
        for (int y=0; y < _height; y++)
        {
            _nodes.Add(new List<PathNode>());
            for (int x=0; x < _width; x++)
            {
                _nodes[y].Add(new PathNode()
                {
                    X = x,
                    Y = y,
                    IsObstacle = _obstacles[y][x],
                    Neighbours = new List<PathNode>(),
                    Parent = null
                }); 
            }
        }

        // Create Neighbors in grid
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                if (x > 0) _nodes[y][x].Neighbours.Add(_nodes[y][x - 1]);
                if (x < _width - 1) _nodes[y][x].Neighbours.Add(_nodes[y][x + 1]);
                if (y > 0) _nodes[y][x].Neighbours.Add(_nodes[y - 1][x]);
                if (y < _height - 1) _nodes[y][x].Neighbours.Add(_nodes[y + 1][x]);
            }
        }

        // Start and End Positions
        _start = _nodes[start.y][start.x];
        _end = _nodes[end.y][end.x];
    }

    public bool StartFindPath(int maxIterations = int.MaxValue)
    {
        // Current Check Node
        _currentNode = _start;
        _currentNode.LocalGoal = 0.0f;
        _currentNode.GlobalGoal = _currentNode.Heuristic(_end);

        // Nodes to check
        _untestedNodes = new List<PathNode>();
        _untestedNodes.Add(_currentNode);

        return FindPath(maxIterations);
    }

    public bool ContinueFindPath(int maxIterations = int.MaxValue)
    {
        return FindPath(maxIterations);
    }

    public List<Vector2> GetFoundPath()
    {
        List<Vector2> path = new List<Vector2>();
        if (_end.Parent != null)
        {
            _currentNode = _end;
            while (_currentNode.Parent != null)
            {
                path.Insert(0, new Vector2(_currentNode.X + 0.5f, _currentNode.Y + 0.5f));
                _currentNode = _currentNode.Parent;
            }
        }
        return path;
    }
}
