using UnityEngine;

public class TrajectoryMarker : FutureBehaviour
{
    public Material highlightedMaterial;
    public Material selectedMaterial;
    public Renderer myRenderer;
    public FutureTransform targetTransform;
    public TrajectoryProvider targetTrajectoryProvider;
    public int step;
    public bool isSpawned;
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
                while (parent.transform.childCount > 0)
                    DestroyImmediate(parent.transform.GetChild(0).gameObject);
            }
            else
            {
                var eventProviders = targetTransform.GetComponents<ITrajectoryUserEventProvider>();
                foreach (var provider in eventProviders)
                    if (provider.IsEnabled(step))
                        provider.CreateUI(step, this).transform.SetParent(parent.transform);
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

    private void OnEnable()
    {
        myRenderer = GetComponent<Renderer>();
        usualMaterial = myRenderer.material;
    }

    public void Spawn()
    {
        isSpawned = true;
        targetTrajectoryProvider = targetTransform.GetComponent<TrajectoryProvider>();
    }

    // ReSharper disable once ParameterHidesMember
    public override void Step(int step)
    {
        if (isSpawned && this.step == step) Destroy(gameObject);
    }

    private void UpdateAppearance()
    {
        myRenderer.material = IsHighlighted ? highlightedMaterial :
            IsSelected ? selectedMaterial : usualMaterial;
    }

    private void Update()
    {
        if (!isSpawned) return;
        if (step - FuturePhysics.currentStep >= targetTrajectoryProvider.trajectory.size)
        {
            transform.position = new Vector3(1e10f, 1e10f);
            return;
        }

        transform.position =
            targetTrajectoryProvider.trajectory.array[
                targetTrajectoryProvider.PhysicsStepToTrajectoryStep(step)
            ];
    }

    private void OnDestroy()
    {
        if (!isSpawned) return;
        IsHighlighted = false;
        IsSelected = false;
        TrajectoryUserEventCreator.Instance.UnregisterMarker(this);
    }
}