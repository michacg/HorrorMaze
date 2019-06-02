using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Room 
{
    public Pair topLeft;
    public Pair topRight;
    public Pair bottomLeft;
    public Pair bottomRight;
    public List<Pair> connectors;

    public Room(Pair TL, Pair TR, Pair BL, Pair BR)
    {
        this.topLeft = TL;
        this.topRight = TR;
        this.bottomLeft = BL;
        this.bottomRight = BR;
        this.connectors = new List<Pair>();
    }

    public override string ToString()
    {
        return String.Format("Room TL: {0}; Room TR: {1}; Room BL: {2}; Room BR: {3}", topLeft, topRight, bottomLeft, bottomRight);
    }
}
