using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class TrajectoryUserEventCreator : MonoBehaviour
{
    public static TrajectoryUserEventCreator Instance;
    public RectTransform markerUiParent;
    public Button markerUiDelete;
    public TrajectoryMarker markerPrefab;
    private TrajectoryMarker currentMarker;
    public float maxDistanceOfRenderingMarker = 1;

    private List<TrajectoryMarker> spawnedMarkers = new();
    private TrajectoryMarker highlightedForEditingMarker;
    private TrajectoryMarker selectedMarker;

    private float MaxSnappingDistance =>
        maxDistanceOfRenderingMarker * Camera.main!.transform.position.z;

    private IEnumerable<ShadowClone> shadowClones;

    private void Start()
    {
        Instance = this;
        shadowClones = FindObjectsOfType<ShadowCloneProvider>().Select(
            provider => provider.CreateShadowClone()).ToList();
    }

    private void Update()
    {
        if (!HighlightEditMarkers())
            MoveMarker();
        else
            RemoveCurrentMarker();
        UpdateShadowClones();
        HandleClick();
    }

    private void HandleClick()
    {
        var isSpawnedMarkerHighlighted = highlightedForEditingMarker != null &&
                                        highlightedForEditingMarker.isSpawned;
        if (!Input.GetButtonDown("Fire1") || !MouseHandler.IsMouseOverEmptySpace ||
            (currentMarker == null && !isSpawnedMarkerHighlighted)) return;

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

    private void UpdateShadowClones()
    {
        var position = -1;
        if (highlightedForEditingMarker != null)
        {
            position = highlightedForEditingMarker.step;
        }
        else if (selectedMarker != null)
        {
            position = selectedMarker.step;
        }
        else if (currentMarker != null)
        {
            position = currentMarker.step;
        }

        if (position == -1)
        {
            foreach (var shadowClone in shadowClones)
            {
                shadowClone.gameObject.SetActive(false);
            }
        }
        else
        {
            foreach (var shadowClone in shadowClones)
            {
                shadowClone.gameObject.SetActive(true);
                shadowClone.SetStep(position);
            }
        }
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
            currentMarker.step =
                closestReceiver.closestToMousePhysicsStep!.Value;
            currentMarker.targetTrajectoryProvider = closestReceiver.trajectoryProvider;
        }
        else
        {
            RemoveCurrentMarker();
        }
    }
}