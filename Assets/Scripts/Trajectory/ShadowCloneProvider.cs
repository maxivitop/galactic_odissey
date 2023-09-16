using UnityEngine;

[RequireComponent(typeof(TrajectoryProvider))]
public class ShadowCloneProvider: MonoBehaviour
{
    public ShadowClone shadowClonePrefab;
    private TrajectoryProvider trajectoryProvider;
    private PlanetRotator planetRotator;
    private MeshFilter targetMesh;

    private void Awake()
    {
        trajectoryProvider = GetComponent<TrajectoryProvider>();
        targetMesh = GetComponentInChildren<MeshFilter>();
        planetRotator = GetComponentInChildren<PlanetRotator>();
    }

    public ShadowClone CreateShadowClone()
    {
        var shadowClone = Instantiate(shadowClonePrefab);
        shadowClone.Deactivate();
        shadowClone.targetMesh = targetMesh;
        shadowClone.targetGameObject = gameObject;
        shadowClone.trajectoryProvider = trajectoryProvider;
        planetRotator.CopyInto(shadowClone.myPlanetRotator);
        shadowClone.myPlanetRotator.rotateIndependently = false;
        return shadowClone;
    }
}