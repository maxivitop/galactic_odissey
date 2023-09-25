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
    private static Plane worldPlane = new Plane(Vector3.forward, 0);
    private static Camera myCamera;

    public static void UpdateWorldMousePosition()
    {
        var ray = myCamera.ScreenPointToRay(Input.mousePosition);
        if (worldPlane.Raycast(ray, out var distance)){
            WorldMousePosition = ray.GetPoint(distance);
        }
    }
    private void Start()
    {
        myCamera = Camera.main!;
        physicsRaycaster = Camera.main!.GetComponent<PhysicsRaycaster>();
    }

    private void Update()
    {

        UpdateWorldMousePosition();
        pointerEventData.position = Input.mousePosition;
        IsMouseOverEmptySpace = !IsMouseOverUiOrGameObject();
    }

    private bool IsMouseOverUiOrGameObject()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            physicsRaycaster.Raycast(pointerEventData, raycastResults);
            return true;
        }
        raycastResults.Clear();
        physicsRaycaster.Raycast(pointerEventData, raycastResults);
        return raycastResults.Count > 0;
    }
}
