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
    private TrajectoryMarker highlightedForEditingMarker = null;
    private TrajectoryMarker selectedMarker = null;

    private List<RaycastResult> raycastResults = new();
    private bool checkedMouseThisFrame = false;
    private bool isMouseOverObject = false;
    private PointerEventData pointerEventData = new PointerEventData(EventSystem.current);

    private float maxSnappingDistance
    {
        get
        {
            return maxDistanceOfRenderingMarker * Camera.main.transform.position.z;
        }
    }

    private void Start()
    {
        Instance = this;
    }

    void Update()
    {
        checkedMouseThisFrame = false;
        if (!HighlightEditMarkers())
        {
            MoveMarker();
        }
        else
        {
            RemoveCurrentMarker();
        }
        bool isSpawnedMarkerHighligted = highlightedForEditingMarker != null && highlightedForEditingMarker.isSpawned;
        if (Input.GetButtonDown("Fire1") && CanHandleMouse() && (currentMarker != null || isSpawnedMarkerHighligted))
        {
            if (highlightedForEditingMarker != null && highlightedForEditingMarker.isSpawned)
            {
                SelectMarker(highlightedForEditingMarker);
            }
            else
            {
                SpawnMarker();
            }
        }
    }

    void RemoveCurrentMarker()
    {
        if (currentMarker != null)
        {
            Destroy(currentMarker.gameObject);
            currentMarker = null;
        }
    }

    public void UnregisterMarker(TrajectoryMarker marker)
    {
        spawnedMarkers.Remove(marker);
    }

    private void SelectMarker(TrajectoryMarker marker)
    {
        if (selectedMarker != null)
        {
            selectedMarker.IsSelected = false;
        }
        marker.IsSelected = true;
        selectedMarker = marker;
    }

    bool HighlightEditMarkers()
    {
        HighlightMarker(currentMarker);
        if (!CanHandleMouse())
        {
            return false;
        }
        if (spawnedMarkers.Count == 0)
        {
            return false;
        }
        TrajectoryMarker closestMarker = null;
        float minDistance = float.MaxValue;
        foreach (var marker in spawnedMarkers)
        {
            var distance = (marker.transform.position - Utils.worldMousePosition).sqrMagnitude;
            if (minDistance > distance)
            {
                closestMarker = marker;
                minDistance = distance;
            }
        }
        if (minDistance > maxSnappingDistance * maxSnappingDistance)
        {
            return false;
        }
        HighlightMarker(closestMarker);
        return true;
    }

    void HighlightMarker(TrajectoryMarker marker)
    {
        if (highlightedForEditingMarker != null)
        {
            highlightedForEditingMarker.IsHighlighted = false;
            highlightedForEditingMarker = null;
        }
        if (marker != null)
        {
            marker.IsHighlighted = true;
        }
        highlightedForEditingMarker = marker;
    }

    void SpawnMarker()
    {
        if (currentMarker != null)
        {
            currentMarker.Spawn();
            spawnedMarkers.Add(currentMarker);
            SelectMarker(currentMarker);
        }
        currentMarker = Instantiate(markerPrefab.gameObject).GetComponent<TrajectoryMarker>();
    }
    void MoveMarker()
    {
        if (!CanHandleMouse())
        {
            RemoveCurrentMarker();
            return;
        }
        Vector3 worldMousePosition = Utils.worldMousePosition;
        float minDistance = float.MaxValue;
        Vector3? closest = null;
        FutureTransform closestTransform = null;
        int closestStep = FuturePhysics.currentStep;
        foreach (TrajectoryUserEventReceiver reciever in TrajectoryUserEventReceiver.all)
        {
            int i = 0;
            foreach (var position in reciever.trajectoryRenderer.trajectory)
            {
                if (!closest.HasValue)
                {
                    closest = position;
                    minDistance = (closest.Value - worldMousePosition).sqrMagnitude;
                    closestTransform = reciever.futureTransform;
                    closestStep = FuturePhysics.currentStep + i;
                    continue;
                }
                var distance = (position - worldMousePosition).sqrMagnitude;
                if (distance < minDistance)
                {
                    closest = position;
                    minDistance = distance;
                    closestTransform = reciever.futureTransform;
                    closestStep = FuturePhysics.currentStep + i;
                }
                i++;
            }
        }
        if (closest.HasValue)
        {
            if (minDistance < maxSnappingDistance * maxSnappingDistance)
            {
                if (currentMarker == null)
                {
                    SpawnMarker();
                }
                currentMarker.transform.position = closest.Value;
                currentMarker.targetTransform = closestTransform;
                currentMarker.step = closestStep;
            }
            else RemoveCurrentMarker();
        }
    }

    private bool CanHandleMouse()
    {
        if (checkedMouseThisFrame)
        {
            return !isMouseOverObject;
        }
        isMouseOverObject = IsMouseOverObject();
        checkedMouseThisFrame = true;
        return !isMouseOverObject;
    }

    private bool IsMouseOverObject()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }
        raycastResults.Clear();
        Camera.main.GetComponent<PhysicsRaycaster>().Raycast(pointerEventData, raycastResults);
        return raycastResults.Count > 0;
    }
}
