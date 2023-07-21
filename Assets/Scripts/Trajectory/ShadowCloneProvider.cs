using UnityEngine;

[RequireComponent(typeof(TrajectoryProvider))]
public class ShadowCloneProvider: MonoBehaviour
{
    public ShadowClone shadowClonePrefab;
    private TrajectoryProvider trajectoryProvider;
    private MeshFilter targetMesh;

    private void Start()
    {
        trajectoryProvider = GetComponent<TrajectoryProvider>();
        targetMesh = GetComponentInChildren<MeshFilter>();
    }

    public ShadowClone CreateShadowClone()
    {
        var shadowClone = Instantiate(shadowClonePrefab).GetComponent<ShadowClone>();
        shadowClone.gameObject.SetActive(false);
        shadowClone.target = targetMesh;
        shadowClone.trajectoryProvider = trajectoryProvider;
        return shadowClone;
    }
}