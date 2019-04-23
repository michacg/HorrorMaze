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
}
