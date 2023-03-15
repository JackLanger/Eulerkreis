using System.Collections.Generic;

namespace EulerGraph.graph;

public class Node<T>
{
    public Node(T data, params Node<T>[] targets)
    {
        Data = data;
        Kanten = new List<Kante>();
        foreach (var target in targets) addBiDirectionalKante(target, 1);
    }

    public void Disconnect(Kante target)
    {
        Kanten.Remove(target);
        target.getEnd().Kanten.RemoveAll(k => k.getEnd() == this);
    }

    public T Data { get; }

    public List<Kante> Kanten { get; }

    public void addBiDirectionalKante(Node<T> target, int cost)
    {
        addBiDirectionalKante(target, cost, cost);
    }

    public void addBiDirectionalKante(Node<T> target, int cost, int costTarget)
    {
        addKante(target, cost);
        target.addKante(this, costTarget);
    }

    public void addKante(Node<T> target, int cost)
    {
        Kanten.Add(new Kante(this, target, cost));
    }


    public class Kante
    {
        private readonly Node<T> end;
        private readonly Node<T> start;
        private int cost;


        public Kante(Node<T> start, Node<T> end) : this(start, end, 1)
        {
        }

        public Kante(Node<T> start, Node<T> end, int cost)
        {
            this.start = start;
            this.end = end;
            this.cost = cost;
        }

        public Kante setCost(int cost)
        {
            this.cost = cost;
            return this;
        }


        public Node<T> getStart()
        {
            return start;
        }

        public Node<T> getEnd()
        {
            return end;
        }

        public int getCost()
        {
            return cost;
        }
    }
}