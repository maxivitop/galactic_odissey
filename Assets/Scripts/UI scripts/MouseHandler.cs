using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseHandler : MonoBehaviour
{
    public static Vector3 WorldMousePosition;
    public static bool IsMouseOverEmptySpace;

    
    private readonly List<RaycastResult> raycastResults = new();
    private bool checkedMouseThisFrame;
    private bool isMouseOverObject;
    private readonly PointerEventData pointerEventData = new(EventSystem.current);
    private PhysicsRaycaster physicsRaycaster;
    public static void UpdateWorldMousePosition()
    {
        WorldMousePosition = Camera.main!.ScreenToWorldPoint(new Vector3(
            Input.mousePosition.x,
            Input.mousePosition.y,
            -Camera.main.transform.position.z
        ));
    }
    private void Start()
    {
        physicsRaycaster = Camera.main!.GetComponent<PhysicsRaycaster>();
    }

    private void Update()
    {

        UpdateWorldMousePosition();
        IsMouseOverEmptySpace = !IsMouseOverUiOrGameObject();
    }

    private bool IsMouseOverUiOrGameObject()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        raycastResults.Clear();
        physicsRaycaster.Raycast(pointerEventData, raycastResults);
        return raycastResults.Count > 0;
    }
}
