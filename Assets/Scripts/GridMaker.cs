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
    [SerializeField, Min(3)] private int gridSize = 5;
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

    /// <summary>
    /// Generate butonu tıklandığında çağrılır. Yeni grid boyutunu input'tan alır ve grid'i yeniden oluşturur.
    /// </summary>
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

    /// <summary>
    /// Mevcut grid'i temizler ve tüm hücreleri yok eder. Sayacı sıfırlar.
    /// </summary>
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

    /// <summary>
    /// Belirtilen boyutta grid oluşturur. Her hücreyi instantiate eder ve konumlandırır.
    /// </summary>
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

                /// Cell'lerin sıralanırken gerçekleştirdiği animasyonu burda Dotween ile yapıyorum.
                cell.transform.localScale = Vector3.zero;
                cell.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetDelay((x + y) * 0.03f);
            }
        }

        ScaleToFitScreen();
    }

    /// <summary>
    /// Kamera boyutunu ve konumunu grid'e göre ayarlar. Grid'in ekrana sığmasını sağlar.
    /// </summary>
    private void ScaleToFitScreen()
    {
        Camera cam = Camera.main;

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float screenRatio = screenWidth / screenHeight;

        float gridWidth = gridSize * (1 + spacing);
        float gridHeight = gridSize * (1 + spacing);

        /// Ekranın üst %60ını oyun alanı olarak kullanmak istedim bu sebeple alan hesabını 
        /// yaparken hesabı tam ekrandan değil de yükseklikten %60 alarak hesaplattım.
        float usableHeightPortion = 0.6f;
        float screenHeightInUnits = gridHeight / usableHeightPortion;

        float camSizeBasedOnHeight = screenHeightInUnits / 2f;
        float camSizeBasedOnWidth = gridWidth / (2f * screenRatio);

        cam.orthographicSize = Mathf.Max(camSizeBasedOnHeight, camSizeBasedOnWidth);

        float gridHeightInWorldUnits = gridHeight;
        float targetMidpointY = (cam.orthographicSize * usableHeightPortion) - (gridHeightInWorldUnits / 2f);
        cam.transform.position = new Vector3(0, -targetMidpointY, -10);
    }

    /// <summary>
    /// Belirtilen konumdaki hücreyi kontrol eder ve yatay/dikey eşleşmeleri bulur. 3 veya daha fazla eşleşme varsa bunları temizler.
    /// </summary>
    public void CheckMatches(int x, int y)
    {
        // Debug.Log($"Checking matches for cell at ({x}, {y})");

        //yatay eşleşmelere bakar varsa işareti kaldırıp sayacı artırır
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

        //dikey eşleşmelere bakar varsa işareti kaldırıp sayacı artırır
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

    /// <summary>
    /// Sayac değerini günceller ve animasyonlu olarak gösterir.
    /// </summary>
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

    /// <summary>
    /// Belirtilen yönde grid sınırları içerisindeki sıralı işaretlileri bir liste olarak tutup verir.
    /// İstenen değerlerde bu yüzden x ve y de başlangıç konumu ve yönü.
    /// </summary>
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

    /// <summary>
    /// Grid sınırlarını kontrol eder. Verilen koordinatların grid içinde olup olmadığını verir. Grid bounds hesabı yani.
    /// </summary>
    private bool BoundsChecker(int x, int y)
    {
        return x >= 0 && y >= 0 && x < gridSize && y < gridSize;
    }
}
