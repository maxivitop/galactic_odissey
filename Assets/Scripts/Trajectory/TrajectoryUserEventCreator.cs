using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class TrajectoryUserEventCreator : MonoBehaviour
{
    public static TrajectoryUserEventCreator Instance;

    public RectTransform markerUiParent;
    public TrajectoryMarker markerPrefab;
    private TrajectoryMarker currentMarker;
    public float maxDistanceOfRenderingMarker = 1;
    private List<TrajectoryMarker> spawnedMarkers = new();
    private TrajectoryMarker highlightedForEditingMarker;
    private TrajectoryMarker selectedMarker;

    private List<RaycastResult> raycastResults = new();
    private bool checkedMouseThisFrame;
    private bool isMouseOverObject;
    private PointerEventData pointerEventData = new(EventSystem.current);
    private PhysicsRaycaster physicsRaycaster;

    private float MaxSnappingDistance =>
        maxDistanceOfRenderingMarker * Camera.main!.transform.position.z;

    private void Start()
    {
        physicsRaycaster = Camera.main!.GetComponent<PhysicsRaycaster>();
        Instance = this;
    }

    private void Update()
    {
        checkedMouseThisFrame = false;
        if (!HighlightEditMarkers())
            MoveMarker();
        else
            RemoveCurrentMarker();
        var isSpawnedMarkerHighligted = highlightedForEditingMarker != null &&
                                        highlightedForEditingMarker.isSpawned;
        if (Input.GetButtonDown("Fire1") && CanHandleMouse() &&
            (currentMarker != null || isSpawnedMarkerHighligted))
        {
            if (highlightedForEditingMarker != null && highlightedForEditingMarker.isSpawned)
                SelectMarker(highlightedForEditingMarker);
            else
                SpawnMarker();
        }
    }

    private void RemoveCurrentMarker()
    {
        if (currentMarker == null) return;
        Destroy(currentMarker.gameObject);
        currentMarker = null;
    }

    public void UnregisterMarker(TrajectoryMarker marker)
    {
        spawnedMarkers.Remove(marker);
    }

    private void SelectMarker(TrajectoryMarker marker)
    {
        if (selectedMarker != null) selectedMarker.IsSelected = false;
        marker.IsSelected = true;
        selectedMarker = marker;
    }

    private bool HighlightEditMarkers()
    {
        HighlightMarker(currentMarker);
        if (!CanHandleMouse()) return false;
        if (spawnedMarkers.Count == 0) return false;
        TrajectoryMarker closestMarker = null;
        var minDistance = float.MaxValue;
        foreach (var marker in spawnedMarkers)
        {
            var distance = (marker.transform.position - Utils.WorldMousePosition).sqrMagnitude;
            if (distance > minDistance) continue;
            closestMarker = marker;
            minDistance = distance;
        }

        if (minDistance > MaxSnappingDistance * MaxSnappingDistance) return false;
        HighlightMarker(closestMarker);
        return true;
    }

    private void HighlightMarker(TrajectoryMarker marker)
    {
        if (highlightedForEditingMarker != null)
        {
            highlightedForEditingMarker.IsHighlighted = false;
            highlightedForEditingMarker = null;
        }

        if (marker != null) marker.IsHighlighted = true;
        highlightedForEditingMarker = marker;
    }

    private void SpawnMarker()
    {
        if (currentMarker != null)
        {
            currentMarker.Spawn();
            spawnedMarkers.Add(currentMarker);
            SelectMarker(currentMarker);
        }

        currentMarker = Instantiate(markerPrefab.gameObject).GetComponent<TrajectoryMarker>();
    }

    private void MoveMarker()
    {
        if (!CanHandleMouse())
        {
            RemoveCurrentMarker();
            return;
        }

        var worldMousePosition = Utils.WorldMousePosition;
        var minDistance = float.MaxValue;
        Vector3? closest = null;
        FutureTransform closestTransform = null;
        var closestStep = FuturePhysics.currentStep;
        foreach (var receiver in TrajectoryUserEventReceiver.all)
        {
            for (var i = 0; i < receiver.trajectoryProvider.trajectory.size; i++)
            {
                var position = receiver.trajectoryProvider.trajectory.array[i];
                if (!closest.HasValue)
                {
                    closest = position;
                    minDistance = (closest.Value - worldMousePosition).sqrMagnitude;
                    closestTransform = receiver.futureTransform;
                    closestStep = TrajectoryProvider.TrajectoryStepToPhysicsStep(i);
                    continue;
                }

                var distance = (position - worldMousePosition).sqrMagnitude;
                if (distance < minDistance)
                {
                    closest = position;
                    minDistance = distance;
                    closestTransform = receiver.futureTransform;
                    closestStep = FuturePhysics.currentStep + i;
                }

                i++;
            }
        }

        if (!closest.HasValue) return;
        if (minDistance < MaxSnappingDistance * MaxSnappingDistance)
        {
            if (currentMarker == null) SpawnMarker();
            currentMarker.transform.position = closest.Value;
            currentMarker.targetTransform = closestTransform;
            currentMarker.step = closestStep;
        }
        else
        {
            RemoveCurrentMarker();
        }
    }

    private bool CanHandleMouse()
    {
        if (checkedMouseThisFrame) return !isMouseOverObject;
        isMouseOverObject = IsMouseOverObject();
        checkedMouseThisFrame = true;
        return !isMouseOverObject;
    }

    private bool IsMouseOverObject()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        raycastResults.Clear();
        physicsRaycaster.Raycast(pointerEventData, raycastResults);
        return raycastResults.Count > 0;
    }
}