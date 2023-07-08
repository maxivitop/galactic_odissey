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

    private float MaxSnappingDistance =>
        maxDistanceOfRenderingMarker * Camera.main!.transform.position.z;

    private void Start()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!HighlightEditMarkers())
            MoveMarker();
        else
            RemoveCurrentMarker();

        HandleClick();
    }

    private void HandleClick()
    {
        var isSpawnedMarkerHighligted = highlightedForEditingMarker != null &&
                                        highlightedForEditingMarker.isSpawned;
        if (!Input.GetButtonDown("Fire1") || !MouseHandler.IsMouseOverEmptySpace ||
            (currentMarker == null && !isSpawnedMarkerHighligted)) return;

        if (highlightedForEditingMarker != null && highlightedForEditingMarker.isSpawned)
            SelectMarker(highlightedForEditingMarker);
        else
            SpawnMarker();
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
        if (!MouseHandler.IsMouseOverEmptySpace) return false;
        if (spawnedMarkers.Count == 0) return false;
        TrajectoryMarker closestMarker = null;
        var minDistance = float.MaxValue;
        foreach (var marker in spawnedMarkers)
        {
            var distance = (marker.transform.position - MouseHandler.WorldMousePosition)
                .sqrMagnitude;
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
        if (!MouseHandler.IsMouseOverEmptySpace)
        {
            RemoveCurrentMarker();
            return;
        }

        var minDistance = float.MaxValue;
        TrajectoryUserEventReceiver closestReceiver = null;
        foreach (var receiver in TrajectoryUserEventReceiver.all)
        {
            if (receiver.closestToMouseTrajectoryPosition == null) continue;
            var distance = (
                receiver.closestToMouseTrajectoryPosition.Value -
                MouseHandler.WorldMousePosition
            ).sqrMagnitude;
            if (distance > minDistance) continue;
            closestReceiver = receiver;
            minDistance = distance;
        }

        if (closestReceiver == null) return;
        if (minDistance < MaxSnappingDistance * MaxSnappingDistance)
        {
            if (currentMarker == null) SpawnMarker();
            currentMarker.transform.position =
                closestReceiver.closestToMouseTrajectoryPosition!.Value;
            currentMarker.targetTransform = closestReceiver.futureTransform;
            currentMarker.step = TrajectoryProvider.TrajectoryStepToPhysicsStep(
                closestReceiver.closestToMouseTrajectoryStep!.Value
            );
        }
        else
        {
            RemoveCurrentMarker();
        }
    }
}