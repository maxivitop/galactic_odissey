
using UnityEngine;

public class TrajectoryMarker : FutureBehaviour
{
    public Material highlightedMaterial;
    public Material selectedMaterial;
    public Renderer myRenderer;
    public FutureTransform targetTransform;
    public TrajectoryRenderer targetTrajectoryRenderer;
    public int step;
    public bool isSpawned;
    private Material usualMaterial;

    private bool _isSelected;
    public bool IsSelected
    {
        get { return _isSelected; }
        set
        {
            if (_isSelected == value)
            {
                return;
            }
            _isSelected = value;
            RectTransform parent = TrajectoryUserEventCreator.Instance.markerUiParent;
            if (parent == null || parent.transform == null) // scene is destroying
            {
                return;
            }
            if (!_isSelected)
            {
                while (parent.transform.childCount > 0)
                {
                    DestroyImmediate(parent.transform.GetChild(0).gameObject);
                }
            } 
            else
            {
                ITrajectoryUserEventProvider[] eventProviders = targetTransform.GetComponents<ITrajectoryUserEventProvider>();
                foreach (var provider in eventProviders)
                {
                    if (provider.isEnabled(step))
                    {
                        provider.CreateUI(step, this).transform.SetParent(parent.transform);
                    }
                }
            }
            UpdateAppearance();
        }
    }

    private bool _isHighlighted;
    public bool IsHighlighted
    {
        get { return _isHighlighted; }
        set
        {
            if (_isHighlighted == value)
            {
                return;
            }
            _isHighlighted = value;
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
        targetTrajectoryRenderer = targetTransform.GetComponent<TrajectoryRenderer>();
    }

    public override void Step(int step)
    {
        if (isSpawned && this.step == step)
        {
            Destroy(gameObject);
        }
    }

    private void UpdateAppearance()
    {
        myRenderer.material = IsHighlighted ? highlightedMaterial : IsSelected ? selectedMaterial : usualMaterial;
    }

    private void Update()
    {
        if (isSpawned)
        {
            if(step - FuturePhysics.currentStep >= targetTrajectoryRenderer.trajectory.Length)
            {
                transform.position = new Vector3(1e10f, 1e10f);
                return;
            }
            transform.position = targetTrajectoryRenderer.trajectory[step - FuturePhysics.currentStep];
        }
    }

    private void OnDestroy()
    {
        if (isSpawned)
        {
            IsHighlighted = false;
            IsSelected = false;
            TrajectoryUserEventCreator.Instance.UnregisterMarker(this);
        }
    }
}
