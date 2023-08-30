using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Vector2 hotSpot = Vector2.zero; // You can adjust this if your cursor's "active point" isn't in the top-left corner.
    public CursorMode cursorMode = CursorMode.Auto;

    private void Start()
    {
        SetCustomCursor();
    }

    void SetCustomCursor()
    {
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
    }
}