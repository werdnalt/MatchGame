using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridOrganizer))]
public class ObjectOrganizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector options.
        DrawDefaultInspector();

        // Get a reference to the target script.
        GridOrganizer organizer = (GridOrganizer)target;

        // Add a button to the inspector.
        if (GUILayout.Button("Organize Objects"))
        {
            // Call the function from the target script when the button is pressed.
            organizer.OrganizeCells();
        }
    }
}