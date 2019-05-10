using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pair : IEquatable<Pair>
{
    public int first;
    public int second;

    public Pair(int f, int s)
    {
        this.first = f;
        this.second = s;
    }

    public override string ToString()
    {
        return String.Format("({0}, {1})", first, second);
    }

    public bool Equals(Pair other)
    {
        return (first == other.first && second == other.second);
    }

    public static Pair operator - (Pair a, Pair b)
    {
        return new Pair(a.first - b.first, a.second - b.second);
    }

    public static float Distance(Pair a, Pair b)
    {
        return Mathf.Abs(a.first - b.first) + Mathf.Abs(a.second - b.second);
        // return Mathf.Sqrt(Mathf.Pow(a.first - b.first, 2) + Mathf.Pow(a.second - b.second, 2));
    }
}
