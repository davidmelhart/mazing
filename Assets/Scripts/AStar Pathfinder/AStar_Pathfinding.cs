using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class AStar_Pathfinding : MonoBehaviour {

    PathRequestManager pathRequestManager;
    Grid grid;

    int GetDistance(Node nodeA, Node nodeB) {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY) {
            return 14 * dstY + 10 * (dstX - dstY);
        } else {
            return 14 * dstX + 10 * (dstY - dstX);
        }
    }

    void Awake() {
        pathRequestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<Grid>();
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos) {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    IEnumerator FindPath(Vector3 startPosition, Vector3 targetPosition) {
        
        Vector3[] waypoints = new Vector3[0];
        Vector3[] riskyWaypoints = new Vector3[0];

        bool pathSuccess = false;

        Node startNode = grid.NodeFromWorldPoint(startPosition);
        Node targetNode = grid.NodeFromWorldPoint(targetPosition);

        if (startNode.walkable && targetNode.walkable) {
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.Count > 0) {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode) {
                    pathSuccess = true;
                    waypoints = RetracePath(startNode, targetNode);
                    break;
                }

                foreach (Node neighbourNode in grid.GetNeighbourNodes(currentNode)) {
                    if (!neighbourNode.walkable || closedSet.Contains(neighbourNode)) {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbourNode) + neighbourNode.weight;

                    if (newMovementCostToNeighbour < neighbourNode.gCost || !openSet.Contains(neighbourNode)) {
                        neighbourNode.gCost = newMovementCostToNeighbour;
                        neighbourNode.hCost = GetDistance(neighbourNode, targetNode);
                        neighbourNode.parent = currentNode;

                        if (!openSet.Contains(neighbourNode))
                            openSet.Add(neighbourNode);
                        else {
                            openSet.UpdateItem(neighbourNode);
                        }
                    }
                }
            }

            Heap<Node> riskyOpenSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> riskyClosedSet = new HashSet<Node>();

            riskyOpenSet.Add(startNode);

            while (riskyOpenSet.Count > 0) {
                Node currentNode = riskyOpenSet.RemoveFirst();
                riskyClosedSet.Add(currentNode);

                if (currentNode == targetNode) {
                    pathSuccess = true;
                    riskyWaypoints = RetracePath(startNode, targetNode);
                    break;
                }

                foreach (Node neighbourNode in grid.GetNeighbourNodes(currentNode)) {
                    if (!neighbourNode.walkable || riskyClosedSet.Contains(neighbourNode)) {
                        continue;
                    }
                    
                    if (neighbourNode.weight >= 1000) {
                        neighbourNode.weight -= 1000;
                    }

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbourNode) + neighbourNode.weight;

                    if (newMovementCostToNeighbour < neighbourNode.gCost || !riskyOpenSet.Contains(neighbourNode)) {
                        neighbourNode.gCost = newMovementCostToNeighbour;
                        neighbourNode.hCost = GetDistance(neighbourNode, targetNode);
                        neighbourNode.parent = currentNode;

                        if (!riskyOpenSet.Contains(neighbourNode))
                            riskyOpenSet.Add(neighbourNode);
                        else {
                            riskyOpenSet.UpdateItem(neighbourNode);
                        }
                    }
                }
            }

        }
        yield return null;
        if (pathSuccess) {
            //waypoints = RetracePath(startNode, targetNode);

            //Debug Code for displaying path per frame
            /*
            for (int i = 0; i < waypoints.Length; i++) {
                if (i == 0) {
                    Debug.Log("-----------------------------------------------");
                }
                Debug.Log(waypoints[i]);
            } */
        }
        pathRequestManager.FinishedProcessingPath(waypoints, riskyWaypoints, pathSuccess);
    }

    Vector3[] RetracePath (Node startNode, Node targetNode) {
        List<Node> path = new List<Node>();
        Node currentNode = targetNode;

        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    Vector3[] SimplifyPath(List<Node> path) {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 0; i < path.Count; i++) {
           //Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            //if (directionNew != directionOld) {
             //   waypoints.Add(path[i - 1].worldPosition);
                waypoints.Add(path[i].worldPosition);
           //}
           //directionOld = directionNew;
        }
        return waypoints.ToArray();
    }
}
