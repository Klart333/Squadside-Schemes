using DataStructures.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PathFinding
{
    public static List<Tile> FindPath(Tile startTile, Tile targetTile, Tile[,] board)
    {
        Vector2Int start = startTile.Index;
        Vector2Int target = targetTile.Index;

        Dictionary<Vector2Int, Vector2Int> WalkedNodes = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, int> Distance = new Dictionary<Vector2Int, int>();
        PriorityQueue<int, Vector2Int> NodeQueue = new PriorityQueue<int, Vector2Int>();

        WalkedNodes.Add(start, start);
        Distance.Add(start, 0);
        NodeQueue.Enqueue(1, start);

        while (NodeQueue.Count > 0)
        {
            Vector2Int current = NodeQueue.Dequeue();
            if (current == target)
            {
                break;
            }

            List<Vector2Int> neighbours = GetNeighbours(current, targetTile, board);

            for (int i = 0; i < neighbours.Count; i++)
            {
                if (!WalkedNodes.ContainsKey(neighbours[i]))
                {
                    int heuristic = Mathf.RoundToInt(Mathf.Abs(board[neighbours[i].x, neighbours[i].y].WorldPosition.x - targetTile.WorldPosition.x) + Mathf.Abs(board[neighbours[i].x, neighbours[i].y].WorldPosition.y - targetTile.WorldPosition.y));
                    int dist = Distance[current] + 1;

                    Distance.Add(neighbours[i], dist);
                    WalkedNodes.Add(neighbours[i], current);
                    NodeQueue.Enqueue(dist + heuristic, neighbours[i]);
                }
            }
        }

        return GetPath(WalkedNodes, target, board);
    }

    private static List<Tile> GetPath(Dictionary<Vector2Int, Vector2Int> walkedNodes, Vector2Int target, Tile[,] board)
    {
        List<Tile> path = new List<Tile>();
        Vector2Int current = target;

        if (!walkedNodes.ContainsKey(current))
        {
            Debug.Log("Could not find path");
            return null;
        }

        while (walkedNodes[current] != current)
        {
            path.Add(board[current.x, current.y]);

            current = walkedNodes[current];
        }

        return path;
    }

    private static List<Vector2Int> GetNeighbours(Vector2Int index, Tile targetTile, Tile[,] map)
    {
        List<Vector2Int> neighbours = new List<Vector2Int>();

        // Left
        if (index.x > 0)
        {
            if (IsWalkable(index.x - 1, index.y))
            {
                neighbours.Add(new Vector2Int(index.x - 1, index.y));
            }
        }

        // Right
        if (index.x + 1 < map.GetLength(0))
        {
            if (IsWalkable(index.x + 1, index.y))
            {
                neighbours.Add(new Vector2Int(index.x + 1, index.y));
            }
        }

        // Back
        if (index.y > 0)
        {
            if (IsWalkable(index.x, index.y - 1))
            {
                neighbours.Add(new Vector2Int(index.x, index.y - 1));
            }
        }

        // Up
        if (index.y + 1 < map.GetLength(1))
        {
            if (IsWalkable(index.x, index.y + 1))
            {
                neighbours.Add(new Vector2Int(index.x, index.y + 1));
            }
        }

        if (index.y % 2 == 0)
        {
            // Up Left
            if (index.x > 0 && index.y + 1 < map.GetLength(1))
            {
                if (IsWalkable(index.x - 1, index.y + 1))
                {
                    neighbours.Add(new Vector2Int(index.x - 1, index.y + 1));
                }
            }

            // Down Left
            if (index.x > 0 && index.y > 0)
            {
                if (IsWalkable(index.x - 1, index.y - 1))
                {
                    neighbours.Add(new Vector2Int(index.x - 1, index.y - 1));
                }
            }
        }
        else
        {
            // Up Right
            if (index.x + 1 < map.GetLength(0) && index.y + 1 < map.GetLength(1))
            {
                if (IsWalkable(index.x + 1, index.y + 1))
                {
                    neighbours.Add(new Vector2Int(index.x + 1, index.y + 1));
                }
            }

            // Down Right
            if (index.x + 1 < map.GetLength(0) && index.y > 0)
            {
                if (IsWalkable(index.x + 1, index.y - 1))
                {
                    neighbours.Add(new Vector2Int(index.x + 1, index.y - 1));
                }
            }
        }

        return neighbours;

        bool IsWalkable(int x, int y)
        {
            return map[x, y] == targetTile || map[x, y].Walkable;
        }
    }

    public static List<Vector2Int> GetNeighbours(Vector2Int index, Tile[,] map)
    {
        List<Vector2Int> neighbours = new List<Vector2Int>();

        // Left
        if (index.x > 0)
        {
            neighbours.Add(new Vector2Int(index.x - 1, index.y));
        }

        // Right
        if (index.x + 1 < map.GetLength(0))
        {
            neighbours.Add(new Vector2Int(index.x + 1, index.y));
        }

        // Back
        if (index.y > 0)
        {
            neighbours.Add(new Vector2Int(index.x, index.y - 1));
        }

        // Up
        if (index.y + 1 < map.GetLength(1))
        {
            neighbours.Add(new Vector2Int(index.x, index.y + 1));
        }

        if (index.y % 2 == 0)
        {
            // Up Left
            if (index.x > 0 && index.y + 1 < map.GetLength(1))
            {
                neighbours.Add(new Vector2Int(index.x - 1, index.y + 1));
            }

            // Down Left
            if (index.x > 0 && index.y > 0)
            {
                neighbours.Add(new Vector2Int(index.x - 1, index.y - 1));
            }
        }
        else
        {
            // Up Right
            if (index.x + 1 < map.GetLength(0) && index.y + 1 < map.GetLength(1))
            {
                neighbours.Add(new Vector2Int(index.x + 1, index.y + 1));
            }

            // Down Right
            if (index.x + 1 < map.GetLength(0) && index.y > 0)
            {
                neighbours.Add(new Vector2Int(index.x + 1, index.y - 1));
            }
        }

        return neighbours;
    }
}

namespace DataStructures.Queue
{
    public class PriorityQueueEntry<TPrio, TItem>
    {
        public TPrio p { get; }
        public TItem data { get; }
        public PriorityQueueEntry(TPrio p, TItem data)
        {
            this.p = p;
            this.data = data;
        }
    }

    public class PriorityQueue<TPrio, TItem> where TPrio : IComparable
    {
        private LinkedList<PriorityQueueEntry<TPrio, TItem>> q;

        public PriorityQueue()
        {
            q = new LinkedList<PriorityQueueEntry<TPrio, TItem>>();
        }

        public int Count { get { return q.Count(); } }

        public void Enqueue(TPrio p, TItem data)
        {
            if (q.Count == 0)
            {
                q.AddFirst(new PriorityQueueEntry<TPrio, TItem>(p, data));
                return;
            }
            // This is a bit classical C but whatever
            LinkedListNode<PriorityQueueEntry<TPrio, TItem>> current = q.First;
            while (current != null)
            {
                if (current.Value.p.CompareTo(p) >= 0)
                {
                    q.AddBefore(current, new PriorityQueueEntry<TPrio, TItem>(p, data));
                    return;
                }
                current = current.Next;
            }
            q.AddLast(new PriorityQueueEntry<TPrio, TItem>(p, data));
        }

        public TItem Dequeue()
        {
            // LinkedList -> LinkedListNode -> PriorityQueueEntry -> data
            var ret = q.First.Value.data;
            q.RemoveFirst();
            return ret;
        }
    }
}
