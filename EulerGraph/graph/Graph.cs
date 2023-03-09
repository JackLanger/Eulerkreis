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
                    // return the path as an array
                    path.Add(target.Data);
                    Console.WriteLine(path);
                    paths.Add(path);
                    continue;
                }
            }

            // remove edges from both target and parent to prevent back propagation

            parent.Kanten.Remove(kante);
            target.Kanten.RemoveAll(k => k.getEnd() == parent);

            Stack<Node<T>.Kante> stack = new();
            target.Kanten.ForEach(k => stack.Push(k));
            List<T> tmp = new(path);
            tmp.Add(target.Data);
            visit(target, stack, tmp);
        }

        Console.WriteLine(path);
    }
}