using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GridMaker : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("Grid size must be greater than or equal to 1.")]
    [SerializeField, Min(1)] private int gridSize = 5;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private float spacing = 0.1f;

    [Header("UI Elements")]
    [SerializeField] private TMP_InputField gridSizeInput;
    [SerializeField] private Button generateButton;
    [SerializeField] private TMP_Text counterText;

    private GameObject[,] grid;
    private int Counter = 0;

    private void Start()
    {
        CreateGrid();

        if (generateButton != null && gridSizeInput != null)
        {
            generateButton.onClick.AddListener(OnGenerateGridClicked);
            gridSizeInput.text = gridSize.ToString();
        }
        else
        {
            Debug.LogWarning("Generate button or grid size input field is not assigned!");
        }
    }

    private void OnGenerateGridClicked()
    {
        string input = gridSizeInput.text;
        int newSize = Convert.ToInt32(input);

        if (newSize >= 2)
        {
            gridSize = newSize;
            ClearExistingGrid();
            CreateGrid();
        }
        else
        {
            Debug.LogWarning("Grid size must be greater than or equal to 2.");
        }
    }

    private void ClearExistingGrid()
    {
        if (grid == null) return;

        foreach (GameObject obj in grid)
        {
            if (obj != null)
                Destroy(obj);
        }

        grid = null;

        Counter = 0;
        counterText.text = Counter.ToString();
    }

    public void CreateGrid()
    {
        grid = new GameObject[gridSize, gridSize];
        float offset = (gridSize - 1) / 2f;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector2 position = new Vector2((x - offset), (y - offset)) * (1 + spacing);
                GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                cell.name = $"Cell_{x}_{y}";

                grid[x, y] = cell;

                cell.GetComponent<Cell>().Initialize(x, y, this);

                //Anims
                cell.transform.localScale = Vector3.zero;
                cell.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetDelay((x + y) * 0.03f);
            }
        }

        ScaleToFitScreen();
    }

    private void ScaleToFitScreen()
    {
        Camera cam = Camera.main;

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float screenRatio = screenWidth / screenHeight;

        float gridWidth = gridSize * (1 + spacing);
        float gridHeight = gridSize * (1 + spacing);

        float usableHeightPortion = 0.6f;
        float screenHeightInUnits = gridHeight / usableHeightPortion;

        float camSizeBasedOnHeight = screenHeightInUnits / 2f;
        float camSizeBasedOnWidth = gridWidth / (2f * screenRatio);

        cam.orthographicSize = Mathf.Max(camSizeBasedOnHeight, camSizeBasedOnWidth);

        float gridHeightInWorldUnits = gridHeight;
        float targetMidpointY = (cam.orthographicSize * usableHeightPortion) - (gridHeightInWorldUnits / 2f);
        cam.transform.position = new Vector3(0, -targetMidpointY, -10);
    }

    public void CheckMatches(int x, int y)
    {
        Debug.Log($"Checking matches for cell at ({x}, {y})");

        //TO-DO: x ve y 'yi doğru yakalıyo dikey ve yatay eşlemele kontrolünü ekle yarın.

        //yatay
        List<Cell> horizontalMatches = CheckDirection(x, y, Vector2Int.left).Concat(CheckDirection(x, y, Vector2Int.right)).ToList();
        horizontalMatches.Add(grid[x, y].GetComponent<Cell>());

        if (horizontalMatches.Count >= 3)
        {
            foreach (Cell c in horizontalMatches)
            {
                c.Clear();
            }

            Counter++;
            AnimateCounter();
        }

        //dikey
        List<Cell> verticalMatches = CheckDirection(x, y, Vector2Int.up).Concat(CheckDirection(x, y, Vector2Int.down)).ToList();
        verticalMatches.Add(grid[x, y].GetComponent<Cell>());

        if (verticalMatches.Count >= 3)
        {
            foreach (Cell c in verticalMatches)
            {
                c.Clear();
            }

            Counter++;
            AnimateCounter();
        }
    }

    private void AnimateCounter()
    {
        counterText.text = Counter.ToString();
        counterText.transform.DOKill(); // Aynı anda birden fazla animasyon olmaması içinmiş dökümantasyondan kontrol et
        counterText.transform.localScale = Vector3.one;
        counterText.transform.DOScale(1.4f, 0.2f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                counterText.transform.DOScale(1f, 0.2f).SetEase(Ease.InBack);
            });
    }

    private List<Cell> CheckDirection(int startX, int startY, Vector2Int direction)
    {
        List<Cell> matches = new List<Cell>();

        int x = startX + direction.x;
        int y = startY + direction.y;

        while (BoundsChecker(x, y))
        {
            Cell cell = grid[x, y].GetComponent<Cell>();
            if (cell.IsMarked())
            {
                matches.Add(cell);
                x += direction.x;
                y += direction.y;
            }
            else
            {
                break;
            }
        }

        return matches;
    }

    private bool BoundsChecker(int x, int y)
    {
        return x >= 0 && y >= 0 && x < gridSize && y < gridSize;
    }
}
