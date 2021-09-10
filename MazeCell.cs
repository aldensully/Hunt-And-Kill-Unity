using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCell
{
    public bool visited = false;
    public bool north = false;
    public bool south = false;
    public bool east = false;
    public bool west = false;
    public int id = Random.Range(0, 10000);
    public int x;
    public int y;
    public MazeCell(int xCoord,int yCoord)
    {
        x = xCoord;
        y = yCoord;
    }
}