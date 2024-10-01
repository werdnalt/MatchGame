using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridOrganizer : MonoBehaviour
{
    public int numColumns;            // Number of columns in the grid
    public int numRows;               // Number of rows in the grid
    public float spaceBetweenColumns; // Horizontal space between grid cells
    public float spaceBetweenRows;    // Vertical space between grid cells
    public float spaceAboveHeroRow;   // Extra space above the hero row
    public List<GameObject> gridCells; // List of grid cells (must be populated in the editor or via code)

    public void OrganizeCells()
    {
        // Ensure there are enough cells to populate the grid
        if (gridCells.Count != numRows * numColumns)
        {
            Debug.LogError("Grid cell count does not match the number of cells needed for the specified rows and columns.");
            return;
        }

        // Get the size of the first cell (assuming all cells have the same size)
        var cellSize = gridCells[0].GetComponent<SpriteRenderer>().bounds.size;

        // Loop through columns and rows and position the cells
        for (var j = 0; j < numColumns; j++) // Outer loop now goes through columns
        {
            for (var i = 0; i < numRows; i++) // Inner loop now goes through rows
            {
                // Calculate the index of the current cell in the gridCells list (column-first indexing)
                int index = j * numRows + i;

                // Calculate the position for the current cell
                float xPos = j * (cellSize.x + spaceBetweenColumns);
                float yPos = i * (cellSize.y + spaceBetweenRows);

                // Apply the extra spaceAboveHeroRow for rows above the hero row
                if (i == 0) // Assuming the hero row is the first row (index 0)
                {
                    yPos += spaceAboveHeroRow;
                }

                // Set the position of the cell
                gridCells[index].transform.position = new Vector3(xPos, yPos, 0);
                gridCells[index].name = $"{i}, {j}";
            }
        }
    }

}