using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CombatCameraManager : MonoBehaviour
{
    [SerializeField] private Transform cam;
    public float dragSpeed = 0.7f;
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

            cam.transform.Translate(move, Space.World);

            // Update origin for smooth continuous drag
            dragOrigin = currentMousePos;
        }
    }
}
