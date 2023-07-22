using UnityEngine;

[RequireComponent(typeof(TrajectoryProvider))]
public class ShadowCloneProvider: MonoBehaviour
{
    public ShadowClone shadowClonePrefab;
    private TrajectoryProvider trajectoryProvider;
    private MeshFilter targetMesh;

    private void Awake()
    {
        trajectoryProvider = GetComponent<TrajectoryProvider>();
        targetMesh = GetComponentInChildren<MeshFilter>();
    }

    public ShadowClone CreateShadowClone()
    {
        var shadowClone = Instantiate(shadowClonePrefab);
        shadowClone.Deactivate();
        shadowClone.targetMesh = targetMesh;
        shadowClone.targetGameObject = gameObject;
        shadowClone.trajectoryProvider = trajectoryProvider;
        return shadowClone;
    }
}