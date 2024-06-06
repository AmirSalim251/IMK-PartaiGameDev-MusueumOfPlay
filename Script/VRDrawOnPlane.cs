using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class VRDrawOnPlane : MonoBehaviour
{
    public Transform drawingPlane; // Reference to the plane
    public LineRenderer linePrefab; // Reference to the LineRenderer prefab
    public LineRenderer laserPointer; // Reference to the laser pointer LineRenderer
    public InputActionProperty triggerAction; // Input action for trigger

    private LineRenderer currentLine;
    private List<Vector3> points = new List<Vector3>();

    void Update()
    {
        bool isPressed = triggerAction.action.ReadValue<float>() > 0.1f;

        if (isPressed)
        {
            if (currentLine == null)
            {
                StartDrawing();
            }
            Draw();
        }
        else
        {
            if (currentLine != null)
            {
                EndDrawing();
            }
        }
    }

    void StartDrawing()
    {
        currentLine = Instantiate(linePrefab);
        points.Clear();
    }

    void Draw()
    {
        Vector3 point = GetDrawingPoint();
        if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], point) > 0.1f)
        {
            points.Add(point);
            currentLine.positionCount = points.Count;
            currentLine.SetPositions(points.ToArray());
        }
    }

    void EndDrawing()
    {
        currentLine = null;
    }

    Vector3 GetDrawingPoint()
    {
        if (laserPointer.positionCount > 1)
        {
            Vector3 laserEndPoint = laserPointer.GetPosition(laserPointer.positionCount - 1);
            Ray ray = new Ray(laserPointer.GetPosition(0), laserEndPoint - laserPointer.GetPosition(0));
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == drawingPlane)
                {
                    return hit.point;
                }
            }
        }
        return Vector3.zero;
    }
}
