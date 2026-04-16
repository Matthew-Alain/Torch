using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CombatCameraManager : MonoBehaviour
{
    [SerializeField] private Transform cam;
    public float dragSpeed = 0.7f;
    public float maxDragDistance = 5f;
    public Vector2 minBounds = new Vector2(0,0); // bottom-left (x, y)
    public Vector2 maxBounds = new Vector2(10,10); // top-right (x, y)
    private Vector3 cameraStartPos;
    private bool isDragging;
    private Vector3 dragOrigin;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Start dragging
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            dragOrigin = Mouse.current.position.ReadValue();
            cameraStartPos = cam.position;
            isDragging = true;
        }

        // Stop dragging
        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        // While dragging
        if (isDragging)
        {
            Vector3 currentMousePos = Mouse.current.position.ReadValue();
            Vector3 difference = currentMousePos - dragOrigin;

            // Convert screen movement to world movement
            Vector3 move = new Vector3(-difference.x, -difference.y, 0f) * dragSpeed * Time.deltaTime;

            // Calculate intended new position
            Vector3 targetPos = cam.position + move;

            targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
            targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);

            // Clamp distance from start position
            Vector3 offset = targetPos - cameraStartPos;
            if (offset.magnitude > maxDragDistance)
            {
                offset = offset.normalized * maxDragDistance;
                targetPos = cameraStartPos + offset;
            }

            cam.position = targetPos;

            // Update origin for smooth continuous drag
            dragOrigin = currentMousePos;
        }
    }
}
