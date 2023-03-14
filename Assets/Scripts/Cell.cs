using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    int x, y, z;
    GameObject gameObject;

    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }
    public int Z { get => z; set => z = value; }
    public GameObject GameObject { get => gameObject; set => gameObject = value; }

    public Cell(GameObject gameObject, int x, int y, int z)
    {
        this.gameObject = gameObject;
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Cell(Vector3Int position)
    {
        this.x = position.x;
        this.y = position.y;
        this.z = position.z;
    }
}
