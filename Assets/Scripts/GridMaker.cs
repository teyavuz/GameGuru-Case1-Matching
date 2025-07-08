using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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


    private void Start()
    {
        CreateGrid();
    }

    private void CreateGrid()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 position = new Vector3(x * (1 + spacing), y * (1 + spacing), 0);
                Instantiate(cellPrefab, position, Quaternion.identity);
            }
        }
    }
}
