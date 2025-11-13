using UnityEngine;
using System.Collections.Generic;

public class JigsawMatrixShuffler : MonoBehaviour
{
    [Header("Grid Settings")]
    public int rows = 3;
    public int cols = 3;
    public bool randomRotation = false;

    private List<Transform> pieces = new List<Transform>();
    private Vector3[,] originalPositions;

    void Start()
    {
        RecordOriginalMatrix();
        ShuffleMatrix();
    }

    void RecordOriginalMatrix()
    {
        pieces.Clear();
        foreach (Transform child in transform)
            pieces.Add(child);

        if (pieces.Count != rows * cols)
        {
            Debug.LogError("Piece count does not match rows × cols!");
            return;
        }

        // Store positions in a matrix
        originalPositions = new Vector3[rows, cols];

        int index = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                originalPositions[r, c] = pieces[index].position;
                index++;
            }
        }
    }

    public void ShuffleMatrix()
    {
        if (originalPositions == null)
        {
            Debug.LogWarning("Original matrix not recorded yet.");
            return;
        }

        // Create a flat list of matrix coordinates
        List<Vector2Int> coords = new List<Vector2Int>();
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                coords.Add(new Vector2Int(r, c));

        // Shuffle coordinates using Fisher–Yates
        for (int i = coords.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (coords[i], coords[j]) = (coords[j], coords[i]);
        }

        // Assign shuffled positions to each piece
        for (int i = 0; i < pieces.Count; i++)
        {
            int r = coords[i].x;
            int c = coords[i].y;
            pieces[i].position = originalPositions[r, c];

            if (randomRotation)
                pieces[i].rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        }
    }

    public void ResetMatrix()
    {
        int index = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                pieces[index].position = originalPositions[r, c];
                pieces[index].rotation = Quaternion.identity;
                index++;
            }
        }
    }
}
