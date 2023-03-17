using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using SimpleFileBrowser;

public class CellController : MonoBehaviour
{
    public const int GRID_SIZE = 500;
    public static CellController Instance;
    [SerializeField] GameObject pfCell;
    [SerializeField] TMP_Dropdown dropdownBlockSize;
    [SerializeField] Slider sliderStepsPerSecond;
    [SerializeField] TextMeshProUGUI textButtonPlay;
    [SerializeField] TextMeshProUGUI textCurrentStep;
    [SerializeField] TextMeshProUGUI textCellCount;
    [SerializeField] TextMeshProUGUI textStepsPerSecond;
    [SerializeField] new GameObject camera;
    [SerializeField] GameObject currentCell;
    [SerializeField] Material matPlaceCell;
    [SerializeField] Material matDeleteCell;
    [SerializeField] ConfigurationManager configurationManager;
    int blockSize = 2;
    int stepsPerSecond = 1;
    int currentStep;
    int cellCount;
    float timeBetweenUpdates = 1;
    bool running;
    float timeLastUpdate;
    List<Cell> cells = new List<Cell>();
    Configuration configuration = new Configuration();
    List<Cell> cellsToRemove = new List<Cell>();

    public float TimeBetweenUpdates { get => timeBetweenUpdates; set => timeBetweenUpdates = value; }
    public bool Running { get => running; set => running = value; }
    public bool[,,] Grid { get => grid; set => grid = value; }

    bool[,,] grid = new bool[GRID_SIZE, GRID_SIZE, GRID_SIZE];
    int[,,] gridNeighbourCount = new int[GRID_SIZE, GRID_SIZE, GRID_SIZE];
    private Vector3Int gridPosition;

    public void Awake()
    {
        Instance = this;
        gridPosition = new Vector3Int(GRID_SIZE / 2, GRID_SIZE / 2, GRID_SIZE / 2);
    }

    public void Start()
    {
        RemoveAllCells();
        OnLookAtCellClick();
    }

    private void OnPlaceCell(InputValue value)
    {
        ToggleGridValue(gridPosition);
        UpdateCellCountText();
        UpdateCurrentCellColor();
    }

    private void UpdateCellCountText()
    {
        textCellCount.text = cellCount.ToString();
    }

    private void UpdateCurrentStepText()
    {
        textCurrentStep.text = currentStep.ToString();
    }
    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: both, Allow multiple selection: true
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Load File", Submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, "default.golconfig", "Load Configuration", "Load");

        if (FileBrowser.Success)
        {
            configuration = configurationManager.Load(FileBrowser.Result[0]);
            RemoveAllCells();
            SetGridToCellConfiguration();
            UpdateCellCountText();
        }
    }

    IEnumerator ShowSaveDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: both, Allow multiple selection: true
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Load File", Submit button text: "Load"
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Files, false, null, "default.golconfig", "Save Configuration", "Save");

        if (FileBrowser.Success)
        {
            configurationManager.Save(configuration, FileBrowser.Result[0]);
        }
    }


    private void ToggleGridValue(Vector3Int position)
    {
        if (grid[position.x, position.y, position.z] == false)
        {
            CreateCell(position);
            configuration.Cells.Add(position);
        }
        else
        {
            Cell cell = cells.Find(o => o.X == position.x && o.Y == position.y && o.Z == position.z);
            RemoveCell(cell);
            configuration.Cells.Remove(position);
        }
    }

    private void UpdateCurrentCellColor()
    {
        if (grid[gridPosition.x, gridPosition.y, gridPosition.z] == false)
        {
            currentCell.GetComponent<Renderer>().material = matPlaceCell;
        }
        else
        {
            currentCell.GetComponent<Renderer>().material = matDeleteCell;
        }
    }

    private void OnPositionLeft(InputValue value)
    {
        gridPosition.x--;
        UpdateCurrentCellColor();
    }

    private void OnPositionRight(InputValue value)
    {
        gridPosition.x++;
        UpdateCurrentCellColor();
    }

    private void OnPositionUp(InputValue value)
    {
        gridPosition.y++;
        UpdateCurrentCellColor();
    }

    private void OnPositionDown(InputValue value)
    {
        gridPosition.y--;
        UpdateCurrentCellColor();
    }

    private void OnPositionForward(InputValue value)
    {
        gridPosition.z++;
        UpdateCurrentCellColor();
    }

    private void OnPositionBack(InputValue value)
    {
        gridPosition.z--;
        UpdateCurrentCellColor();
    }

    public void Update()
    {
        if (Running && Time.time - timeLastUpdate > TimeBetweenUpdates)
        {
            currentStep++;
            UpdateCurrentStepText();
            timeLastUpdate = Time.time;
            cellsToRemove.Clear();
            foreach (Cell cell in cells.ToArray())
            {
                // when there are less than 10 or more than 15 neighbour cells, the cell will die
                if (gridNeighbourCount[cell.X, cell.Y, cell.Z] < 10 || gridNeighbourCount[cell.X, cell.Y, cell.Z] > 15)
                {
                    cellsToRemove.Add(cell);
                }
                CheckNewLife(cell);
            }
            foreach (Cell cell in cellsToRemove.ToArray())
            {
                RemoveCell(cell);
            }

            UpdateCellCountText();
            if (cellCount == 0)
            {
                Running = false;
            }
        }
        currentCell.transform.position = gridPosition;
    }

    private void CheckNewLife(Cell cell)
    {
        for (int x = cell.X - 1; x <= cell.X + 1; x++)
        {
            for (int y = cell.Y - 1; y <= cell.Y + 1; y++)
            {
                for (int z = cell.Z - 1; z <= cell.Z + 1; z++)
                {
                    // when there are 14 or 15 neighbour cells to a empty place on the grid, a cell will grow there.
                    if (gridNeighbourCount[x, y, z] > 13 && gridNeighbourCount[x, y, z] < 16)
                    {
                        CreateCell(x, y, z);
                    }
                }
            }
        }
    }

    private void RemoveCell(Cell cell)
    {
        if (grid[cell.X, cell.Y, cell.Z] == false)
        {
            return;
        }
        cellCount--;
        Grid[cell.X, cell.Y, cell.Z] = false;
        gridNeighbourCount[cell.X, cell.Y, cell.Z]++;  // this is necessary because the cell itself is included in following loop
        for (int x = cell.X - 1; x <= cell.X + 1; x++)
        {
            for (int y = cell.Y - 1; y <= cell.Y + 1; y++)
            {
                for (int z = cell.Z - 1; z <= cell.Z + 1; z++)
                {
                    if (x > 0 && y > 0 && z > 0 && x < GRID_SIZE && y < GRID_SIZE && z < GRID_SIZE)
                    {
                        gridNeighbourCount[x, y, z]--;
                    }
                }
            }
        }
        Destroy(cell.GameObject);
        cells.Remove(cell);
    }

    public void OnSpawnCellClick()
    {
        for (int x = gridPosition.x - blockSize / 2; x <= gridPosition.x + blockSize / 2; x++)
        {
            for (int y = gridPosition.y - blockSize / 2; y <= gridPosition.y + blockSize / 2; y++)
            {
                for (int z = gridPosition.z - blockSize / 2; z <= gridPosition.z + blockSize / 2; z++)
                {
                    if (Random.value<0.5)
                    {
                        ToggleGridValue(new Vector3Int(x, y, z));
                    }
                }
            }
        }
        UpdateCellCountText();
    }

    private void CreateCell (Vector3Int position)
    {
        CreateCell(position.x, position.y, position.z);
    }

    private void CreateCell(int gridX, int gridY, int gridZ)
    {
        if (grid[gridX, gridY, gridZ] == true)
        {
            return;
        }
        cellCount++;
        grid[gridX, gridY, gridZ] = true;
        gridNeighbourCount[gridX, gridY, gridZ]--;  // this is necessary because the cell itself is included in following loop
        for (int x = gridX - 1; x <= gridX + 1; x++)
        {
            for (int y = gridY - 1; y <= gridY + 1; y++)
            {
                for (int z = gridZ - 1; z <= gridZ + 1; z++)
                {
                    if (x > 0 && y > 0 && z > 0 && x < GRID_SIZE && y < GRID_SIZE && z < GRID_SIZE)
                    {
                        gridNeighbourCount[x, y, z]++;
                    }
                }
            }
        }

        GameObject newCell = Instantiate(pfCell, new Vector3(gridX, gridY, gridZ), Quaternion.identity);
        newCell.name = "cell";
        cells.Add(new Cell(newCell, gridX, gridY, gridZ));
    }

    private void RemoveAllCells()
    {
        GameObject[] existingCells = GameObject.FindGameObjectsWithTag("Cell");
        foreach (GameObject cell in existingCells)
        {
            Destroy(cell);
        }
        cells.Clear();
        for (int x = 0; x < GRID_SIZE; x++)
        {
            for (int y = 0; y < GRID_SIZE; y++)
            {
                for (int z = 0; z < GRID_SIZE; z++)
                {
                    gridNeighbourCount[x, y, z] = 0;
                }
            }
        }
        cellCount = 0;
        currentStep = 0;
        UpdateCurrentStepText();
        UpdateCellCountText();
    }

    public void OnRemoveAllCellsClick()
    {
        RemoveAllCells();
        configuration = new Configuration();
    }

    public void OnSliderValueChanged()
    {
        stepsPerSecond = (int)sliderStepsPerSecond.value;
        textStepsPerSecond.text = stepsPerSecond.ToString();
        TimeBetweenUpdates = 1 / (float)stepsPerSecond;
    }

    public void OnSaveConfiguration()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Configuration Files", ".golconfig"));
        FileBrowser.SetDefaultFilter(".golconfig");
        StartCoroutine(ShowSaveDialogCoroutine());
    }

    public void OnLoadConfiguration()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Configuration Files", ".golconfig"));
        FileBrowser.SetDefaultFilter(".golconfig");
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    public void OnPlayClick()
    {
        running = !running;
        if (running)
        {
            textButtonPlay.text = "|| Stop";
        }
        else
        {
            textButtonPlay.text = ">> Play";
        }
    }

    public void OnLookAtCellClick()
    {
        camera.GetComponent<EditorController>().Yaw = 0;
        camera.GetComponent<EditorController>().Pitch = 0;
        camera.transform.position = new Vector3(gridPosition.x, gridPosition.y, gridPosition.z - 8);
    }

    public void OnDropdownBlockSizeChanged()
    {
        blockSize = dropdownBlockSize.value + 2;
    }

    public void OnRewindClick()
    {
        RemoveAllCells();
        SetGridToCellConfiguration();
    }

    private void SetGridToCellConfiguration()
    {
        foreach (Vector3Int cell in configuration.Cells)
        {
            CreateCell(cell);
        }
    }
}
