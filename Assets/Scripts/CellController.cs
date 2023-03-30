using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.Linq;
using System;

public class CellController : MonoBehaviour
{
    public const int GRID_SIZE = 500;
    public const int MAX_RANDOM_BLOCK_SIZE = 10;

    public static CellController Instance;
    [SerializeField] GameObject pfCell;
    [SerializeField] TMP_Dropdown dropdownBlockSize;
    [SerializeField] TMP_Dropdown dropdownLowerLimitBirth;
    [SerializeField] TMP_Dropdown dropdownLowerLimitDeath;
    [SerializeField] TMP_Dropdown dropdownUpperLimitBirth;
    [SerializeField] TMP_Dropdown dropdownUpperLimitDeath;
    [SerializeField] Slider sliderStepsPerSecond;
    [SerializeField] TextMeshProUGUI textButtonPlay;
    [SerializeField] TextMeshProUGUI textCurrentStep;
    [SerializeField] TextMeshProUGUI textCellCount;
    [SerializeField] TextMeshProUGUI textCreateCount;
    [SerializeField] TextMeshProUGUI textDeleteCount;
    [SerializeField] TextMeshProUGUI textStepsPerSecond;
    [SerializeField] TextMeshProUGUI textSpawnRandomCells;
    [SerializeField] TextMeshProUGUI textTries;
    [SerializeField] TextMeshProUGUI labelTries;
    [SerializeField] GameObject panelHelp;
    [SerializeField] GameObject panelLifeConditions;
    [SerializeField] new GameObject camera;
    [SerializeField] GameObject currentCell;
    [SerializeField] Material matPlaceCell;
    [SerializeField] Material matRemoveCell;
    [SerializeField] ConfigurationManager configurationManager;
    int blockSize = 2;
    int stepsPerSecond = 1;
    int currentStep;
    long cellCount;
    long createCount;
    long deleteCount;
    float timeBetweenUpdates = 1;
    bool running;
    bool simulationMode;
    float timeLastUpdate;
    List<Cell> cells = new List<Cell>();
    Configuration configuration = new Configuration();
    List<Cell> cellsToRemove = new List<Cell>();
    long previousCreated, previousDeleted;
    LifeConditions lifeConditions = new LifeConditions();
    bool spawnRandomCells;
    int bestNumberOfSteps;
    float timeLastRandomSpawn;

    bool motionDetected;
    Vector3Int minValues;
    Vector3Int maxValues;
    Vector3Int timesMinChanged;
    Vector3Int timesMaxChanged;

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
        ResetAllCells();
        OnLookAtCellClick();
        labelTries.enabled = false;
        panelHelp.SetActive(false);
        panelLifeConditions.SetActive(false);
    }

    private void OnPlaceCell(InputValue value)
    {
        if (!running)
        {
            ToggleGridValue(gridPosition);
            UpdateCellCountText();
            UpdateCurrentCellColor();
        }
    }

    private void UpdateCellCountText()
    {
        textCellCount.text = cellCount.ToString();
    }

    private void UpdateCreateCountText()
    {
        textCreateCount.text = createCount.ToString();
    }
    private void UpdateDeleteCountText()
    {
        textDeleteCount.text = deleteCount.ToString();
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
            ResetAllCells();
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
//            CheckDoubles();
        }
        else
        {
            Cell cell = cells.Find(o => o.X == position.x && o.Y == position.y && o.Z == position.z);
            RemoveCell(cell);
            configuration.Cells.Remove(position);
        }
    }
    private void CheckCellsMarkedAsDoubles()
    {
        foreach(Vector3Int cell in configuration.Cells)
        {
            if (grid[cell.x, cell.y, cell.z] == false)
            {
                Debug.LogError("Cell should be marked in-use!");
            }
        }
    }

    private void CheckDoubles()
    {
        var doubles = configuration.Cells.GroupBy(x => x)
              .Where(g => g.Count() > 1)
              .Select(y => y.Key)
              .ToList();
        if (doubles.Count() > 0)
        {
            Debug.LogError("Cell already used!");
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
            currentCell.GetComponent<Renderer>().material = matRemoveCell;
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

    public void OnPlayOneStepClick()
    {
        PlayOneStep();
    }

    public void OnButtonSpawnRandomCellsClick()
    {
        spawnRandomCells = !spawnRandomCells;
        if(spawnRandomCells)
        {
            textSpawnRandomCells.text = "on";
        }
        else
        {
            textSpawnRandomCells.text = "off";
        }
    }

    private void UpdateCells()
    {
        currentStep++;
        cellsToRemove.Clear();
        foreach (Cell cell in cells.ToArray())
        {
            // when there are less than 5 or more than 20 neighbour cells, the cell will die
            if (gridNeighbourCount[cell.X, cell.Y, cell.Z] <= lifeConditions.LowerLimitDeath || gridNeighbourCount[cell.X, cell.Y, cell.Z] >= lifeConditions.UpperLimitDeath)
            {
                deleteCount++;
                cellsToRemove.Add(cell);
            }
            CheckNewLife(cell);
        }
        foreach (Cell cell in cellsToRemove.ToArray())
        {
            RemoveCell(cell);
        }

        if (cellCount == 0)
        {
            Running = false;
        }
    }

    private void SpawnRandomCells()
    {
        CreateRandomBlock(UnityEngine.Random.Range(2, MAX_RANDOM_BLOCK_SIZE), 
            new Vector3Int(UnityEngine.Random.Range(gridPosition.x - 50, gridPosition.x + 50), 
            UnityEngine.Random.Range(gridPosition.y - 50, gridPosition.y + 50), 
            UnityEngine.Random.Range(gridPosition.z - 50, gridPosition.z + 50)));
    }

    public void Update()
    {
        if (spawnRandomCells)
        {
            if (Time.time-timeLastRandomSpawn>1.0f)
            {
                timeLastRandomSpawn = Time.time;
                SpawnRandomCells();
            }
        }

        if (simulationMode)
        {
            return;
        }
        if (Running)
        {
            while (Time.time - timeLastUpdate > TimeBetweenUpdates)
            {
                if (!PlayOneStep())
                {
                    break;
                }
            }
            if (!running)
            {
                currentCell.SetActive(true);
                textButtonPlay.text = ">> Play";
            }
        }
        currentCell.transform.position = gridPosition;
    }

    private bool PlayOneStep()
    {
        UpdateCells();
        UpdateCurrentStepText();
        UpdateCellCountText();
        UpdateCreateCountText();
        UpdateDeleteCountText();
        if (previousCreated == createCount && previousDeleted == deleteCount && !spawnRandomCells)
        {
            // there is no activity anymore, so stop
            running = false;
            return false;
        }
        previousCreated = createCount;
        previousDeleted = deleteCount;

        timeLastUpdate += TimeBetweenUpdates;

        return true;
    }

    private void CheckNewLife(Cell cell)
    {
        for (int x = cell.X - 1; x <= cell.X + 1; x++)
        {
            for (int y = cell.Y - 1; y <= cell.Y + 1; y++)
            {
                for (int z = cell.Z - 1; z <= cell.Z + 1; z++)
                {
                    // when there are 14-15 neighbour cells to a empty place on the grid, a cell will grow there.
                    if (grid[x, y, z]==false && gridNeighbourCount[x, y, z] >= lifeConditions.LowerLimitBirth && gridNeighbourCount[x, y, z] <= lifeConditions.UpperLimitBirth)
                    {
                        createCount++;
                        CreateCell(x, y, z);
                    }
                }
            }
        }
    }

    public void OnSpawnCellClick()
    {
        CreateRandomBlock(blockSize, gridPosition);
        UpdateCellCountText();
    }

    private void CreateRandomBlock(int blockSize, Vector3Int position)
    {
        for (int x = 0; x < blockSize; x++)
        {
            for (int y = 0; y < blockSize; y++)
            {
                for (int z = 0; z < blockSize; z++)
                {
                    if (UnityEngine.Random.value < 0.5)
                    {
                        ToggleGridValue(new Vector3Int(position.x + x, position.y + y, position.z + z));
                    }
                }
            }
        }
    }

    private void CreateCell(Vector3Int position)
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

        GameObject newCell = null;
        if (!simulationMode)
        {
            newCell = Instantiate(pfCell, new Vector3(gridX, gridY, gridZ), Quaternion.identity);
            newCell.name = "cell";
        }

        SetMinMaxValues(gridX, gridY, gridZ);

        cells.Add(new Cell(newCell, gridX, gridY, gridZ));
    }

    private void SetMinMaxValues(int gridX, int gridY, int gridZ)
    {
        if (gridX < minValues.x)
        {
            minValues.x = gridX;
            timesMinChanged.x++;
            if (timesMinChanged.x > 2)
            {
                motionDetected = true;
            }
        }
        if (gridX > maxValues.x)
        {
            maxValues.x = gridX;
            timesMaxChanged.x++;
            if (timesMaxChanged.x > 2)
            {
                motionDetected = true;
            }
        }
        if (gridY < minValues.y)
        {
            minValues.y = gridY;
            timesMinChanged.y++;
            if (timesMinChanged.y > 2)
            {
                motionDetected = true;
            }
        }
        if (gridY > maxValues.y)
        {
            maxValues.y = gridY;
            timesMaxChanged.y++;
            if (timesMaxChanged.y > 2)
            {
                motionDetected = true;
            }
        }
        if (gridZ < minValues.z)
        {
            minValues.z = gridZ;
            timesMinChanged.z++;
            if (timesMinChanged.z > 2)
            {
                motionDetected = true;
            }
        }
        if (gridZ > maxValues.z)
        {
            maxValues.z = gridZ;
            timesMaxChanged.z++;
            if (timesMaxChanged.z > 2)
            {
                motionDetected = true;
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
        if (!simulationMode)
        {
            cell.GameObject.GetComponent<CellScript>().Terminate();
//            Destroy(cell.GameObject);
        }
        cells.Remove(cell);
    }

    private void ResetAllCells()
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
                    grid[x, y, z] = false;
                    gridNeighbourCount[x, y, z] = 0;
                }
            }
        }
        cellCount = 0;
        createCount = 0;
        deleteCount = 0;
        currentStep = 0;
        previousCreated = -1;
        previousDeleted = -1;

        if (!simulationMode)
        {
            UpdateCurrentStepText();
            UpdateCellCountText();
            UpdateCreateCountText();
            UpdateDeleteCountText();
        }

        minValues = new Vector3Int(GRID_SIZE, GRID_SIZE, GRID_SIZE);
        maxValues = new Vector3Int(-1, -1, -1);
        timesMinChanged = new Vector3Int(0, 0, 0);
        timesMaxChanged = new Vector3Int(0, 0, 0);
        motionDetected = false;
    }

    public void OnRemoveAllCellsClick()
    {
        ResetAllCells();
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
            currentCell.SetActive(false);
        }
        else
        {
            textButtonPlay.text = ">> Play";
            currentCell.SetActive(true);
        }
        timeLastUpdate = Time.time;
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

    public void OnDropdownUpperDeathChanged()
    {
        lifeConditions.UpperLimitDeath = Convert.ToInt32(dropdownUpperLimitDeath.options[dropdownUpperLimitDeath.value].text);
    }

    public void OnDropdownLowerDeathChanged()
    {
        lifeConditions.LowerLimitDeath = Convert.ToInt32(dropdownLowerLimitDeath.options[dropdownLowerLimitDeath.value].text);
    }

    public void OnDropdownUpperLifeChanged()
    {
        lifeConditions.UpperLimitBirth = Convert.ToInt32(dropdownUpperLimitBirth.options[dropdownUpperLimitBirth.value].text);
    }

    public void OnDropdownLowerLifeChanged()
    {
        lifeConditions.LowerLimitBirth = Convert.ToInt32(dropdownLowerLimitBirth.options[dropdownLowerLimitBirth.value].text);
    }

    public void OnRewindClick()
    {
        ResetAllCells();
        SetGridToCellConfiguration();
    }

    public void OnButtonHelpClick()
    {
        panelHelp.SetActive(!panelHelp.activeInHierarchy);
    }

    public void OnButtonLifeConditionsClick()
    {
        panelLifeConditions.SetActive(!panelLifeConditions.activeInHierarchy);
        if (panelLifeConditions.activeInHierarchy)
        {
            dropdownLowerLimitDeath.value = dropdownLowerLimitDeath.options.FindIndex(option => option.text.Equals(lifeConditions.LowerLimitDeath.ToString()));
            dropdownLowerLimitBirth.value = dropdownLowerLimitBirth.options.FindIndex(option => option.text.Equals(lifeConditions.LowerLimitBirth.ToString()));
            dropdownUpperLimitBirth.value = dropdownUpperLimitBirth.options.FindIndex(option => option.text.Equals(lifeConditions.UpperLimitBirth.ToString()));
            dropdownUpperLimitDeath.value = dropdownUpperLimitDeath.options.FindIndex(option => option.text.Equals(lifeConditions.UpperLimitDeath.ToString()));
        }
    }

    private void SetGridToCellConfiguration()
    {
        foreach (Vector3Int cell in configuration.Cells)
        {
            CreateCell(cell);
        }
        UpdateCellCountText();
    }

    public void OnRunTestClick()
    {
        StartCoroutine(FindBestConfiguration());
    }

    IEnumerator FindBestConfiguration()
    {
        int maxTries = 10000;
        bestNumberOfSteps = 0;
        simulationMode = true;
        labelTries.enabled = true;

        for (int i = 100; i > 0; i--)
        {
            ResetAllCells();
            int sizeTestObject = (int)(6 + UnityEngine.Random.value * 12);
            configuration = new Configuration(); 
            bool makeClone = UnityEngine.Random.value < 0.5;

            // create random config
            for (int gridX = 0; gridX < sizeTestObject; gridX++)
            {
                float density = (float)(0.2 + UnityEngine.Random.value * 0.4);
                for (int gridY = 0; gridY < sizeTestObject; gridY++)
                {
                    for (int gridZ = 0; gridZ < sizeTestObject; gridZ++)
                    {
                        if (UnityEngine.Random.value < density)
                        {
//                            CheckCellsMarkedAsDoubles();
                            ToggleGridValue(new Vector3Int(gridX + GRID_SIZE / 2, gridY + GRID_SIZE / 2, gridZ + GRID_SIZE / 2));
                            if (makeClone)
                            {
                                // copy the structure with one cell in between
                                ToggleGridValue(new Vector3Int(gridX + sizeTestObject + 1 + GRID_SIZE / 2, gridY + GRID_SIZE / 2, gridZ + GRID_SIZE / 2));
                            }
                        }
                    }
                }
            }

            ResetAllCells();
            SetGridToCellConfiguration();

            // run it
            running = true;
            int tries = 0;
            while (Running)
            {
                UpdateCells();
                tries++;
                if (tries >= maxTries)
                {
                    break;
                }
                if (previousCreated == createCount && previousDeleted == deleteCount)
                {
                    break;
                }
                if (createCount - previousCreated > 2000)
                {
                    break;
                }

                previousCreated = createCount;
                previousDeleted = deleteCount;

                yield return null;
            }

            // check amount and save if best
            if (currentStep > bestNumberOfSteps)
            {
                if (currentStep != maxTries)
                {
                    bestNumberOfSteps = currentStep;
                }
                    // WriteAnalysis(lifeConditions.LowerLimitDeath + "_" + lifeConditions.LowerLimitBirth + "_" + lifeConditions.UpperLimitBirth + "_" + lifeConditions.UpperLimitDeath + "-" + sizeTestObject + "-" + createCount + deleteCount + "-" + cells.Count + "-" + currentStep, 0);
                    configurationManager.Save(configuration, "F:\\tmp\\config_" + lifeConditions.LowerLimitDeath + "_" + lifeConditions.LowerLimitBirth + "_" + lifeConditions.UpperLimitBirth + "_" + lifeConditions.UpperLimitDeath + "-" + sizeTestObject + "-" + createCount + deleteCount + "-" + cells.Count + "-" + currentStep + ".golconfig");
            }
            textTries.text = i.ToString();

            yield return null;
        }

        textTries.text = "";
        running = false;
        ResetAllCells();
        configuration = new Configuration();
        simulationMode = false;
        labelTries.enabled = true;
    }

    private void WriteAnalysis(string value, int analysisNumber)
    {
        // write the serialized data to the file
        using (FileStream stream = new FileStream("F:\\tmp\\analysis_" + analysisNumber + ".gol", FileMode.Append))
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.WriteLine(value);
            }
        }
    }
}
