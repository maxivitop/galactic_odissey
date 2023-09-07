using UnityEngine;
using UnityEngine.Events;

public class TrajectoryMarker : MonoBehaviour
{
    public Material highlightedMaterial;
    public Material selectedMaterial;
    public Renderer[] myRenderers;
    public FutureTransform targetTransform;
    public TrajectoryProvider targetTrajectoryProvider;
    public int step;
    public bool isSpawned;
    private Quaternion? fixedRotation;

    public Quaternion? FixedRotation
    {
        get => fixedRotation;
        set
        {
            fixedRotation = value;
            if (value != null)
            {
                transform.rotation = value.Value;
            }
        }
    }

    private UnityAction onDeleteClicked;

    private Material usualMaterial;

    private bool isSelected;

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (isSelected == value) return;
            isSelected = value;
            var parent = TrajectoryUserEventCreator.Instance.markerUiParent;
            if (parent == null || parent.transform == null) // scene is destroying
                return;
            if (!isSelected)
            {
                TrajectoryUserEventCreator.Instance.markerUiDelete.onClick.RemoveListener(onDeleteClicked);
                while (parent.transform.childCount > 0)
                    DestroyImmediate(parent.transform.GetChild(0).gameObject);
            }
            else
            {
                var eventProviders = targetTransform.GetComponents<ITrajectoryUserEventProvider>();
                foreach (var provider in eventProviders)
                    if (provider.IsEnabled(step))
                        provider.CreateUI(step, this).transform.SetParent(parent.transform);
                TrajectoryUserEventCreator.Instance.markerUiDelete.onClick.AddListener(onDeleteClicked);
            }

            UpdateAppearance();
        }
    }

    private bool isHighlighted;

    public bool IsHighlighted
    {
        get => isHighlighted;
        set
        {
            if (isHighlighted == value) return;
            isHighlighted = value;
            UpdateAppearance();
        }
    }

    private void Awake()
    {
        myRenderers = GetComponentsInChildren<Renderer>();
        usualMaterial = myRenderers[0].material;
        onDeleteClicked = DestroySelf;
    }

    public void Spawn()
    {
        isSpawned = true;
    }

    private void UpdateAppearance()
    {
        foreach (var myRenderer in myRenderers)
        {
            myRenderer.material = IsHighlighted ? highlightedMaterial :
                IsSelected ? selectedMaterial : usualMaterial;
        }
    }

    private void Update()
    {
        if (isSpawned && step <= FuturePhysics.currentStep)
        {
            Destroy(gameObject);
            return;
        }
        var trajStep = TrajectoryProvider.PhysicsStepToTrajectoryStep(step);
        if (isSpawned)
        {
            if (targetTrajectoryProvider.trajectory.size <= trajStep)
            {
                transform.position = new Vector3(1e10f, 1e10f);
                return;
            }
            transform.position = targetTrajectoryProvider.trajectory[trajStep];
        }

        if (fixedRotation != null) return;
        if (trajStep + 1 >= targetTrajectoryProvider.trajectory.size) return;
        var direction = targetTrajectoryProvider.trajectory.array[trajStep + 1] -
                            targetTrajectoryProvider.trajectory.array[trajStep];
        transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
    }

    private void DestroySelf()
    {
        foreach (var provider in targetTransform.GetComponents<ITrajectoryUserEventProvider>())
        {
            provider.Destroy(this);
        }
        Destroy(gameObject);
    }

    protected void OnDestroy()
    {
        if (!isSpawned) return;
        IsHighlighted = false;
        IsSelected = false;
        TrajectoryUserEventCreator.Instance.UnregisterMarker(this);
    }
}