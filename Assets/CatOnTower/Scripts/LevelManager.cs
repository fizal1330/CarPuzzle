using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelData
{
    public int width;
    public int height;
    public int totalBlocks;
    public Vector2Int garageIndex;
    public List<string> cellBehavior = new List<string>();
}

public class Tower
{
    public int towerIndex;
    public List<LevelData> towerLevels;
}

namespace CatOnTower
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager instance;

        [SerializeField] private float _cellSize;
        [SerializeField] private List<Cell> prefabs;
        [SerializeField] private int levelIndex;
        public GameObject gridParent;
        public GameObject sheetsParent;
        public GameObject bottomSheet;
        public int currentCompletedBlocks = 0;
        public Cell wall;
        public Cell opening;
        public Cell[,] cells;
        public Grid grid;
        public Cell currentCell;
        bool canMove = false;
        public TextAsset levelDataCSV;
        public List<LevelData> levelData = new List<LevelData>();
        public List<Color> wallColor;
        public Transform cameraHolder;
        private SwipeControl swipeControl;

        private Vector2 swipeStartPosition;
        private Vector2 swipeEndPosition;

        public float movementSmoothness;

        [Header("PREFABS")]
        public Cell carPrefab;
        public Cell wallPrefab;
        public Cell garagePrefab;
        public Cell paintShopPrefab;
        public Cell emptyPrefab;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        void Start()
        {
            canMove = false;
            swipeControl = new SwipeControl();
            swipeControl.SetLevelManager(this);
            swipeControl.canDetectCube = true;
            ReadLevelDataFromCSV();

            //cameraHolder.transform.position -= new Vector3(0, levelData.Count, 0);
            //cameraHolder.DOMove(new Vector3(cameraHolder.position.x, 9f, cameraHolder.position.z), 1f);
        }


        void ReadLevelDataFromCSV()
        {

            string[] lines = levelDataCSV.text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                //creates new empty gameobject parent for each level
                GameObject levelParent = new GameObject();
                levelParent.name = "Level - " + i;
                levelParent.transform.SetParent(gridParent.transform);
                levelParent.transform.localPosition = Vector3.zero;
                levelParent.transform.localRotation = Quaternion.Euler(0, 0, 0);

                LevelData levelDat = new LevelData();
                //splitting the lines
                string[] data;
                data = lines[i].Split('|');
                //gets the height and width  of the grid from csv
                levelDat.height = int.Parse(data[2].Split(',')[0]);
                levelDat.width = int.Parse(data[2].Split(',')[1]);
                //gets the cell's direction on the grid from csv
                levelDat.cellBehavior = data[3].Split(',').ToList<string>();
                //gets the outindex of the level from csv

                int intValueX = int.Parse(data[4].Split(',')[0]);
                int intValueY = int.Parse(data[4].Split(',')[1]);
                Vector2Int outInd = new Vector2Int();
                outInd.x = intValueX;
                outInd.y = intValueY;

                levelDat.garageIndex = outInd;
                //adding the level data class to the level data list
                levelData.Add(levelDat);
            }
            //creates the grids based on the tower size
            for (int i = 0; i < levelData.Count; i++)
            {
                //only initializes the current level grid
                if (i == levelIndex)
                {
                    CreateGrid(Vector3.zero, i);
                }
            }

            //foreach (GameObject sheets in sheetsParent.transform)
            //{
            //    sheets.SetActive(false);
            //}

            //sheetsParent.transform.GetChild(levelIndex).gameObject.SetActive(true);

        }
        //checks if the current level is completed
        public IEnumerator CheckForLevelCompletion()
        {
            currentCompletedBlocks += 1;

            //gets the next level initiated when completed the previous level
            if (currentCompletedBlocks == levelData[levelIndex].totalBlocks)
            {

                //sets the completed level's blocks and bottom sheet setactive false
                yield return new WaitForSeconds(1);
                sheetsParent.transform.GetChild(levelIndex).gameObject.SetActive(false);
                gridParent.transform.GetChild(levelIndex).gameObject.SetActive(false);
                //moves the camera down
                //cameraHolder.DOMove(new Vector3(cameraHolder.position.x, cameraHolder.position.y - 1.2f, cameraHolder.position.z), 1f);
                //resets the completed blocks count
                currentCompletedBlocks = 0;
                //increses the level index
                levelIndex += 1;
                //pans the camera according to level size
                if (levelData[levelIndex].width > 4)
                    Camera.main.DOOrthoSize(6.9f, 1f);
                else
                    Camera.main.DOOrthoSize(6.5f, 1f);
                //initiates the next level
                CreateGrid(Vector3.zero, levelIndex);
            }
        }


        //moves the cell
        public void MoveCube(Swipe dir, Vector2Int index)
        {
            if (currentCell == null)
                return;

            switch (dir)
            {
                case Swipe.Up:
                    if (currentCell.currentType == Cell.Type.CAR)
                    {
                        MoveUp(index);
                    }

                    break;

                case Swipe.Down:
                    if (currentCell.currentType == Cell.Type.CAR)
                    {
                        MoveDown(index);
                    }
                    break;

                case Swipe.Left:
                    if (currentCell.currentType == Cell.Type.CAR)
                    {
                        MoveLeft(index);
                    }
                    break;

                case Swipe.Right:
                    if (currentCell.currentType == Cell.Type.CAR)
                    {
                        MoveRight(index);
                    }
                    break;
            }
        }

        void MoveUp(Vector2Int index)
        {
            int steps = 0;
            for (int i = 1; i < levelData[levelIndex].width; i++)
            {
                if (index.y + i <= levelData[levelIndex].width)
                {
                    Debug.Log(index.y - i);
                    if (cells[index.x, index.y + i] == null)
                    {

                        Debug.Log("slop available");
                        steps++;
                    }
                    else if (cells[index.x, index.y + i].currentType == Cell.Type.EMPTY)
                    {
                        steps++;
                    }
                }
            }
            Debug.Log(steps);
            currentCell.transform.DOLocalMoveZ(currentCell.transform.position.y + steps, 0.5f).SetEase(Ease.InOutSine);
            currentCell = null;
        }

        void MoveDown(Vector2Int index)
        {
            int steps = 0;
            for (int i = 1; i < levelData[levelIndex].width; i++)
            {
                if(index.y - i >= 0)
                {
                    Debug.Log(index.y - i);
                    if (cells[index.x, index.y - i] == null)
                    {

                        Debug.Log("slop available");
                        steps++;
                    }
                    else if (cells[index.x, index.y - i].currentType == Cell.Type.EMPTY)
                    {
                        steps++;
                    }
                }             
            }
            Debug.Log(steps);
            currentCell.transform.DOLocalMoveZ(currentCell.transform.position.z - steps, 0.5f).SetEase(Ease.InOutSine);
            currentCell = null;
        }

        void MoveLeft(Vector2Int index)
        {
            int steps = 0;
            for (int i = 1; i < levelData[levelIndex].width; i++)
            {
                if (index.x - i >= 0)
                {
                    Debug.Log(index.x - i);
                    if (cells[index.x - i, index.y] == null)
                    {

                        Debug.Log("slop available");
                        steps++;
                    }
                    else if (cells[index.x - i, index.y].currentType == Cell.Type.EMPTY)
                    {
                        steps++;
                    }
                }
            }
            Debug.Log(steps);
            currentCell.transform.DOLocalMoveX(currentCell.transform.position.x - steps, 0.5f).SetEase(Ease.InOutSine);
            currentCell = null;
        }

        void MoveRight(Vector2Int index)
        {
            int steps = 0;
            for (int i = 1; i < levelData[levelIndex].width; i++)
            {
                if (index.x + i <= levelData[levelIndex].width)
                {
                    Debug.Log(index.x - i);
                    if (cells[index.x + i, index.y] == null)
                    {

                        Debug.Log("slop available");
                        steps++;
                    }
                    else if (cells[index.x + i, index.y].currentType == Cell.Type.EMPTY)
                    {
                        steps++;
                    }
                }
            }
            Debug.Log(steps);
            currentCell.transform.DOLocalMoveX(currentCell.transform.position.x + steps, 0.5f).SetEase(Ease.InOutSine);
            currentCell = null;
        }

        private void ShakeCube(Cell currentcell)
        {
            // Specify the shake duration, strength, and vibrato (number of shakes)
            float shakeDuration = 0.3f;
            float shakeStrength = 0.1f;
            int shakeVibrato = 50;

            // Specify the move duration
            float moveDuration = 0.3f;

            // Sequence the movements using DoTween
            Sequence sequence = DOTween.Sequence();

            // Move up from the original Y-axis position
            sequence.Append(currentcell.gameObject.transform.DOMoveY(currentCell.transform.localPosition.y + 0.2f, moveDuration));

            // Shake the cube
            sequence.Append(currentcell.gameObject.transform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato));

            // Move down to the original Y-axis position
            sequence.Append(currentcell.gameObject.transform.DOMove(currentCell.transform.localPosition, moveDuration));
        }

        private void Update()
        {
            if (swipeControl != null)
            {
                swipeControl.OnUpdate();
            }
        }

        //creates the cells
        private Cell CreateCells(int x, int y, Vector3 originPos, int direction, int level)
        {
            switch (direction)
            {
                case 0:
                    Cell car = Instantiate(carPrefab, gridParent.transform);
                    car.transform.localScale = new Vector3(_cellSize, 0.25f, _cellSize);
                    car.transform.localPosition = originPos + grid.GetCellWorldPosition(x, y) + new Vector3(0, -level, 0);
                    cells[x, y] = car;
                    return car;
                case 1:
                    Cell wall = Instantiate(wallPrefab, gridParent.transform);
                    wall.transform.localScale = new Vector3(_cellSize, 0.25f, _cellSize);
                    wall.transform.localPosition = originPos + grid.GetCellWorldPosition(x, y) + new Vector3(0, -level, 0);
                    cells[x, y] = wall;
                    return wall;
                case 2:
                    Cell empty = Instantiate(emptyPrefab, gridParent.transform);
                    empty.transform.localScale = new Vector3(_cellSize, 0.25f, _cellSize);
                    empty.transform.localPosition = originPos + grid.GetCellWorldPosition(x, y) + new Vector3(0, -level, 0);
                    cells[x, y] = null;
                    return empty;
                case 3:
                    Cell paintShop = Instantiate(paintShopPrefab, gridParent.transform);
                    paintShop.transform.localScale = new Vector3(_cellSize, 0.25f, _cellSize);
                    paintShop.transform.localPosition = originPos + grid.GetCellWorldPosition(x, y) + new Vector3(0, -level, 0);
                    cells[x, y] = paintShop;
                    return paintShop;              
                case 4:
                    Cell garage = Instantiate(garagePrefab, gridParent.transform);
                    garage.transform.localScale = new Vector3(_cellSize, 0.25f, _cellSize);
                    garage.transform.localPosition = originPos + grid.GetCellWorldPosition(x, y) + new Vector3(0, -level, 0);
                    cells[x, y] = garage;
                    return garage;

                default: return null;
                   
            }


        }

        public Vector2Int WorldToGridIndex(Vector3 worldPosition, float cellSize, Vector3 gridOrigin)
        {
            // Calculate the displacement from the grid origin
            Vector3 displacement = worldPosition - gridOrigin;

            // Calculate the grid indices by dividing the displacement by the cell size
            int xIndex = Mathf.FloorToInt(displacement.x / cellSize);
            int zIndex = Mathf.FloorToInt(displacement.z / cellSize);

            //Debug.Log("POS - " + xIndex + " , " + zIndex);

            canMove = true;

            // Return the grid indices as a Vector2Int
            return new Vector2Int(xIndex, zIndex);

        }

        private void CreateSheets(int index)
        {
            GameObject sheet = Instantiate(bottomSheet, sheetsParent.transform);
            sheet.transform.localRotation = Quaternion.Euler(0, 90, 0);
            sheet.transform.localPosition = new Vector3(-0.5f, -index, -0.5f);
            float scaleX = (((float)levelData[index].width / 10) - 0.2f);
            float scaleZ = (((float)levelData[index].width / 10) - 0.2f);
            sheet.transform.localScale = new Vector3(scaleX, 1, scaleZ);
        }

        private void CreateGrid(Vector3 originPos, int levelInd)
        {
            //initializing the grid respective to level data
            grid = new Grid();
            grid.Initialize(levelData[levelInd].width, levelData[levelInd].height, _cellSize);
            cells = new Cell[levelData[levelInd].width, levelData[levelInd].height];

            int counter = 0;

            for (int x = 0; x < grid.GridArray.GetLength(1); x++)
            {
                for (int y = 0; y < grid.GridArray.GetLength(0); y++)
                {
                    string dir = levelData[levelInd].cellBehavior[counter];
                    counter++;

                    //creates all cells and walls if its the current level

                    switch (dir)
                    {
                        case "C":
                            cells[x, y] = CreateCells(x, y, originPos, 0, levelInd);
                            break;
                        case "W":
                            cells[x, y] = CreateCells(x, y, originPos, 1, levelInd);
                            break;
                        case "E":
                            cells[x, y] = CreateCells(x, y, originPos, 2, levelInd);
                            break;
                        case "P":
                            cells[x, y] = CreateCells(x, y, originPos, 3, levelInd);
                            break;
                        case "G":
                            cells[x, y] = CreateCells(x, y, originPos, 4, levelInd);
                            break;

                    }




                }
            }
        }



    }
}