using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    public Vector2 heroGridOrigin = new Vector2(-3, 0); // Starting position of the Hero grid
    public Vector2 enemyGridOrigin = new Vector2(3, 0); // Starting position of the Enemy grid
    public Vector2 gridSpacing = new Vector2(2, 2);     // Spacing between grid cells
    public int columns = 3;                             // Number of columns in the grid
    public int rows = 2;                                // Number of rows in the grid

    private void Start()
    {
        // Create the hero grid with a blue background
        CreateGrid("Hero", heroGridOrigin, new Color(0, 0, 1, 0.5f));

        // Create the enemy grid with a red background
        CreateGrid("Enemy", enemyGridOrigin, new Color(1, 0, 0, 0.5f));
    }

    void CreateGrid(string tag, Vector2 gridOrigin, Color gridColor)
    {
        // Find all game objects with the specified tag
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);

        // Create a grid background using a plain color
        GameObject gridBackground = new GameObject($"{tag}GridBackground");
        SpriteRenderer gridRenderer = gridBackground.AddComponent<SpriteRenderer>();

        // Create a simple white square sprite for the background
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        Sprite gridSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        gridRenderer.sprite = gridSprite;

        // Set the background color
        gridRenderer.color = gridColor;

        // Position and scale the background
        gridBackground.transform.position = new Vector3(gridOrigin.x + (columns * gridSpacing.x / 2) - (gridSpacing.x / 2), 
                                                        gridOrigin.y - (rows * gridSpacing.y / 2) + (gridSpacing.y / 2), 
                                                        0); // Ensure it's at z=0 for 2D rendering
        gridBackground.transform.localScale = new Vector3(columns * gridSpacing.x, rows * gridSpacing.y, 1);

        // Ensure the grid is rendered behind other objects
        gridRenderer.sortingOrder = -1;

        int count = 0;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // Check if there's still an object to place
                if (count >= objects.Length)
                    return;

                // Calculate the position of the current cell in the grid
                Vector3 cellPosition = new Vector3(gridOrigin.x + (col * gridSpacing.x), 
                                                   gridOrigin.y - (row * gridSpacing.y), 
                                                   0);

                // Move the object to the calculated position
                objects[count].transform.position = cellPosition;

                // Increment the count to move to the next object
                count++;
            }
        }
    }
}
