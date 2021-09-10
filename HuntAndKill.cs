using System.Collections.Generic;
using UnityEngine;

public class MazeGen1 : MonoBehaviour
{
    private int width;
    private int length;
    private MazeCell[,] maze;
    private MazeCell current;
    public int scaleFactor = 10;
    public int wallHeight = 5;
    private int offset;
    private int startX;
    private int startY;
    private bool justHunted;
    public GameObject floor;
    public GameObject wall;
    public GameObject startMarker;
    public GameObject endMarker;
    Transform t;

    void Start()
    {
        floor.transform.localScale = new Vector3(10, 0.5f, 10);
        wall.transform.localScale = new Vector3(10, wallHeight, 0.5f);
        t = GetComponent<Transform>(); //initialize grid layout to the size of the starting object(not necessary)
        width = (int)(t.localScale.x * 0.1f);
        length = (int)(t.localScale.z * 0.1f);
        offset = scaleFactor / 2;

        maze = new MazeCell[width, length];

        //initialize grid layout
        for (int i = 0; i < maze.GetLength(0); i++)
        {
            for (int j = 0; j < maze.GetLength(1); j++)
            {
                maze[i, j] = new MazeCell(i, j);
                Instantiate(floor, new Vector3(i * scaleFactor + offset, 0, j * scaleFactor + offset), Quaternion.identity);

                if (i == maze.GetLength(0) - 1)
                {
                    Instantiate(wall, new Vector3(i * scaleFactor + scaleFactor, wallHeight / 2 + 0.25f, j * scaleFactor + offset), Quaternion.Euler(0, 90, 0));
                }
                if (j == maze.GetLength(1) - 1)
                {
                    Instantiate(wall, new Vector3(i * scaleFactor + offset, wallHeight / 2 + 0.25f, j * scaleFactor + scaleFactor), Quaternion.identity);
                }
                if (i == 0)
                {
                    Instantiate(wall, new Vector3(i * scaleFactor, wallHeight / 2 + 0.25f, j * scaleFactor + offset), Quaternion.Euler(0, 90, 0));
                }
                if (j == 0)
                {
                    Instantiate(wall, new Vector3(i * scaleFactor + offset, wallHeight / 2 + 0.25f, j * scaleFactor), Quaternion.identity);
                }
            }
        }

        CreateMaze();
    }

    void CreateMaze()
    {
        //create random starting point
        startX = Random.Range(0, width);
        startY = Random.Range(0, length);
        current = maze[startX, startY];
        Debug.Log("starting at: " + current.x + ", " + current.y);

        Instantiate(startMarker, new Vector3(current.x * scaleFactor + offset, 0, current.y * scaleFactor + offset), Quaternion.identity);

        List<char> neighbors = new List<char>(); //unvisited neighbors (for each move)
        List<char> walls = new List<char>();  //possible walls to remove (after a hunt)

        char picked;
        bool terminate = false;
        justHunted = false;
        while (!terminate)
        {
            maze[current.x, current.y].visited = true;  //flag the current cell as visited

            if (current.x + 1 < width)
            {
                if (maze[current.x + 1, current.y].visited == false) neighbors.Add('E');  //not visited yet, add to possible moves
                else if (justHunted) walls.Add('E');  //visited but a possible wall to clear(we just hunted
            }
            if (current.x - 1 >= 0)
            {
                if (maze[current.x - 1, current.y].visited == false) neighbors.Add('W');
                else if (justHunted) walls.Add('W');
            }
            if (current.y + 1 < length)
            {
                if (maze[current.x, current.y + 1].visited == false) neighbors.Add('N');
                else if (justHunted) walls.Add('N');
            }
            if (current.y - 1 >= 0)
            {
                if (maze[current.x, current.y - 1].visited == false) neighbors.Add('S');
                else if (justHunted) walls.Add('S');
            }

            if (walls.Count > 0) //remove a wall at random by setting its flag to false
            {
                int w = Random.Range(0, walls.Count);
                char pickedWall = walls[w];
                Debug.Log("Deleting wall : " + pickedWall);
                if (pickedWall == 'N') maze[current.x, current.y + 1].south = false;
                else if (pickedWall == 'S') maze[current.x, current.y - 1].north = false;
                else if (pickedWall == 'E') maze[current.x + 1, current.y].west = false;
                else if (pickedWall == 'W') maze[current.x - 1, current.y].east = false;
            }
            //list of unvisited neighbors to move to, choose one at random then flag the other directions to get a wall
            if (neighbors.Count > 0)
            {
                int rand = Random.Range(0, neighbors.Count);
                picked = neighbors[rand];
                neighbors.RemoveAt(rand);
                //flag walls that arent the randomly picked one
                for (int n = 0; n < neighbors.Count; n++)
                {
                    Debug.Log("Creating wall: " + neighbors[n]);
                    if (neighbors[n] == 'N') maze[current.x, current.y].north = true;
                    else if (neighbors[n] == 'S') maze[current.x, current.y].south = true;
                    else if (neighbors[n] == 'E') maze[current.x, current.y].east = true;
                    else if (neighbors[n] == 'W') maze[current.x, current.y].west = true;
                }
                Debug.Log("moving to: " + picked);
                if (picked == 'N') current = maze[current.x, current.y + 1];
                else if (picked == 'S') current = maze[current.x, current.y - 1];
                else if (picked == 'E') current = maze[current.x + 1, current.y];
                else if (picked == 'W') current = maze[current.x - 1, current.y];

                justHunted = false;
            }

            //there werent any unvisited neighbors, TIME TO HUNT!
            else if (neighbors.Count == 0)
            {
                //place dead end marker
                Instantiate(endMarker, new Vector3(current.x * scaleFactor + offset, 0, current.y * scaleFactor + offset), Quaternion.identity);
                (int, int) hunted;
                hunted = Hunt();
                if (hunted != (-1, -1))  //(-1,-1) is a flag that there wasnt a found cell
                {
                    current = maze[hunted.Item1, hunted.Item2]; //the hunted cell
                    justHunted = true;
                }
                else
                {
                    Debug.Log("Hunt did not return a value, breaking from loop");
                    terminate = true;
                }
            }
            neighbors.Clear();
            walls.Clear();
        }

        //algorithm over, now draw the walls based on the cell wall booleans(north,south,east,west)
        for (int i = 0; i < maze.GetLength(0); i++)
        {
            for (int j = 0; j < maze.GetLength(1); j++)
            {
                if (maze[i, j].north == true)
                {
                    Instantiate(wall, new Vector3(i * scaleFactor + offset, wallHeight / 2 + 0.25f, j * scaleFactor + scaleFactor), Quaternion.identity);  //north and south walls
                }
                if (maze[i, j].south == true)
                {
                    Instantiate(wall, new Vector3(i * scaleFactor + offset, wallHeight / 2 + 0.25f, j * scaleFactor), Quaternion.identity);  //north and south walls
                }
                if (maze[i, j].east == true)
                {
                    Instantiate(wall, new Vector3(i * scaleFactor + scaleFactor, wallHeight / 2 + 0.25f, j * scaleFactor + offset), Quaternion.Euler(0, 90, 0)); //east and west walls
                }
                if (maze[i, j].west == true)
                {
                    Instantiate(wall, new Vector3(i * scaleFactor, wallHeight / 2 + 0.25f, j * scaleFactor + offset), Quaternion.Euler(0, 90, 0)); //east and west walls
                }
            }
        }
    }
    (int, int) Hunt()
    {
        Debug.Log("Hunting!!");
        for (int i = maze.GetLength(0) - 1; i >= 0; i--)
        {
            for (int j = maze.GetLength(1) - 1; j >= 0; j--)
            {
                if (maze[i, j].visited == false)
                {
                    Debug.Log("Killed: " + i + ", " + j);
                    return (i, j);
                }
            }
        }
        return (-1, -1);
    }
}