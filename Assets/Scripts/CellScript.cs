using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellScript : MonoBehaviour
{
    int gridX;
    int gridY;
    int gridZ;
    float timeLastUpdate;

    public void Initialize(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0 || x >= CellController.GRID_SIZE || y >= CellController.GRID_SIZE || z >= CellController.GRID_SIZE)
        {
            Destroy(gameObject);
        }
        gridX = x;
        gridY = y;
        gridZ = z;
        timeLastUpdate = Time.time;
        transform.position = new Vector3(x, y, z);
    }

    // Update is called once per frame
    void Update()
    {
        if (CellController.Instance.Running && Time.time - timeLastUpdate > CellController.Instance.TimeBetweenUpdates)
        {
            timeLastUpdate = Time.time;
            UpdateCell();
        }
    }

    private void UpdateCell()
    {
        int neighbourCount = -1;    // the cell itself will also be count, so deduct it.
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                for (int z = -1; z < 2; z++)
                {
                    if (CellController.Instance.Grid[gridX + x, gridY + y, gridZ + z] == true)
                    {
                        neighbourCount++;
                    }
                }
            }
        }

        // when there are less than 10 or more than 15 neighbour cells, the cell will die
        // when there are 14 or 15 neighbour cells to a dead cell, it will come to life.
        // 
        if (neighbourCount < 10 || neighbourCount > 15)
        {
            CellController.Instance.Grid[gridX, gridY, gridZ] = false;
            Destroy(gameObject);
        }
    }
}
