using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseHandler : MonoBehaviour
{
    public static Vector3 WorldMousePosition;
    public static bool IsMouseOverEmptySpace;

    
    private readonly List<RaycastResult> raycastResults = new();
    private bool checkedMouseThisFrame;
    private bool isMouseOverObject;
    private readonly PointerEventData pointerEventData = new(EventSystem.current);
    private PhysicsRaycaster physicsRaycaster;
    private GraphicRaycaster graphicsRaycaster;
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
        graphicsRaycaster = FindObjectOfType<GraphicRaycaster>();
    }

    private void Update()
    {

        UpdateWorldMousePosition();
        IsMouseOverEmptySpace = !IsMouseOverUiOrGameObject();
    }

    private bool IsMouseOverUiOrGameObject()
    {
        pointerEventData.position = Input.mousePosition;
        raycastResults.Clear();
        graphicsRaycaster.Raycast(pointerEventData, raycastResults);
        if (raycastResults.Count > 0)
        {
            return true;
        }
        raycastResults.Clear();
        physicsRaycaster.Raycast(pointerEventData, raycastResults);
        foreach (var raycastResult in raycastResults)
        {
            if (raycastResult.worldPosition.z < 0)
            {
                return true;
            }
        }
        return false;
    }
}
