using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BoardPosition
{
    /// <summary>
    /// The position of a space on the board in the form of (column, row)
    /// </summary>
    public BoardManager.Coordinates boardSpaceCoordinates;

    /// <summary>
    /// The position value that should be assigned to a unit's transform to occupy this position
    /// </summary>
    public Vector3 worldSpacePosition;

    /// <summary>
    /// Optionally, a unit that is occupying this position
    /// </summary>
    public UnitBehaviour unit;

    public BoardPosition(BoardManager.Coordinates boardSpaceCoordinates, Vector3 worldSpacePosition)
    {
        this.boardSpaceCoordinates = boardSpaceCoordinates;
        this.worldSpacePosition = worldSpacePosition;
        unit = null;
    }
}
