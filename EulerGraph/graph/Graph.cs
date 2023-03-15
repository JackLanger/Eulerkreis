using System;
using System.Collections.Generic;
using System.Linq;

namespace EulerGraph.graph;

public class Graph<T>
{
    private readonly List<List<T>> paths;
    private readonly Node<T> root;

    public Graph(Node<T> root)
    {
        this.root = root;
        paths = new List<List<T>>();
    }

    public List<T> Traverse()
    {
        Stack<Node<T>.Kante> stack = new();
        List<T> path = new();
        path.Add(root.Data);
        root.Kanten.ForEach(k => stack.Push(k));
        visit(root, stack, path);
        var max = 0;
        List<T> longestPath = null;
        foreach (var p in paths)
            if (p.Count > max)
            {
                max = p.Count;
                longestPath = p;
            }

        return longestPath;
    }

    private void visit(Node<T> parent, Stack<Node<T>.Kante> kanten, List<T> path)
    {
        while (kanten.Count > 0)
        {
            var kante = kanten.Pop();
            var target = kante.getEnd();

            // check if end has any more than 1 edges.
            while (target.Kanten.Count() <= 1 && kanten.Count > 0)
            {
                kante = kanten.Pop();
                target = kante.getEnd();
            }
            
            // check if target is root
            if (target == root)
            {
                if (kanten.Count > 0)
                {
                    // if there are more edges, pick another one
                    kante = kanten.Pop();
                    target = kante.getEnd();
                }
                else
                {
                    if (target.Kanten.Count > 0)
                    {
                        RemoveVisitedKante(parent, kante, target);
                        visit(target,GetKantenStack(target),new(path));
                    }
                    
                    // return the path as an array
                    path.Add(target.Data);
                    Console.WriteLine(path);
                    paths.Add(path);
                    continue;
                }
            }
            // remove edges from both target and parent to prevent back propagation
            RemoveVisitedKante(parent, kante, target);
            var stack = GetKantenStack(target);
            List<T> tmp = new(path);
            tmp.Add(target.Data);
            visit(target, stack, tmp);
        }

        Console.WriteLine(path);
    }

    private static void RemoveVisitedKante(Node<T> parent, Node<T>.Kante kante, Node<T> target)
    {
        parent.Kanten.Remove(kante);
        target.Kanten.RemoveAll(k => k.getEnd() == parent);
    }
    private static Stack<Node<T>.Kante> GetKantenStack(Node<T> target)
    {
        Stack<Node<T>.Kante> stack = new();
        target.Kanten.ForEach(k => stack.Push(k));
        return stack;
    }


    public List<Node<T>> TraverseHierholzer(Node<T> startNode)
    {
        List<Node<T>> nodeList = new List<Node<T>>(); // Initialisiert die Liste der durchlaufenen Knoten
        Stack<Node<T>> currentPath = new Stack<Node<T>>(); // Initialisiert einen Stapelspeicher für die hinzugefügten Knoten
        currentPath.Push(startNode); // Fügt den Startknoten dem Stapelspeicher hinzu
        Node<T> currentNode = startNode; // Setzt den Startknoten
        while (currentPath.Count != 0) // So lange der Stapelspeicher für die hinzugefügten Knoten nicht leer ist
        {
                // Wenn noch nicht alle anliegenden Kanten des aktuellen Knotens durchlaufen wurden
            if (currentNode.Kanten.Count != 0) 
            {
                // Fügt den aktuellen Knoten dem Stapelspeicher hinzu
                currentPath.Push(currentNode); 
                List<Node<T>.Kante>.Enumerator enumerator = currentNode.Kanten.GetEnumerator();
                enumerator.MoveNext();
                Node<T>.Kante nextKante = enumerator.Current;
                // Setzt den nächsten Knoten auf einen Nachbarknoten des aktuellen Knotens
                Node<T> nextNode = nextKante.getEnd(); 
                // Löscht die Kante zwischen aktuellem Knoten und Nachbarknoten
                currentNode.Disconnect(nextKante); 
                currentNode = nextNode; // Setzt den aktuellen Knoten auf den Nachbarknoten
            }
            else 
            {
                // Sonst Backtracking verwenden, um einen weiteren Kreis zu finden
                // Fügt den aktuellen Knoten am Anfang der Liste der durchlaufenen Knoten ein
                nodeList.Insert(0, currentNode); 
                // Löscht das oberste Element, also den letzten Knoten vom Stapelspeicher
                currentNode = currentPath.Pop(); 
            }
        }
        return nodeList;
    }
}