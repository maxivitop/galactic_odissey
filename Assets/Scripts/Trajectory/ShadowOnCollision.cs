using UnityEngine;

[RequireComponent(typeof(ShadowCloneProvider))]
public class ShadowOnCollision: MonoBehaviour, ICollisionEnterHandler
{
    private ShadowCloneProvider shadowCloneProvider;
    private ShadowClone shadowClone;
    private bool hasShadowClone;
    private int collisionStep;
    public Material collisionMaterial;

    private void Start()
    {
        shadowCloneProvider = GetComponent<ShadowCloneProvider>();
        FuturePhysics.beforeReset.AddListener(resetParams =>
        {
            if (!hasShadowClone || resetParams.step > collisionStep) return;
            DestroyShadowClone();
        });
    }

    public void VirtualStepCollisionEnter(int step, FutureCollision collision)
    {
        FuturePhysicsRunner.ExecuteOnUpdate(() =>
        {
            hasShadowClone = true;
            collisionStep = step;
            shadowClone = shadowCloneProvider.CreateShadowClone();
            shadowClone.SetStep(step);
            shadowClone.meshRenderer.material = collisionMaterial;
            shadowClone.Activate();
        });
    }

    public void StepCollisionEnter(int step, FutureCollision collision)
    {
        DestroyShadowClone();
    }

    private void OnDestroy()
    {
        DestroyShadowClone();
    }

    private void DestroyShadowClone()
    {
        hasShadowClone = false;
        if (shadowClone != null) Destroy(shadowClone.gameObject);
        shadowClone = null;
    }
}