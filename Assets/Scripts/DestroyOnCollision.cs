using UnityEngine;

public class DestroyOnCollision : MonoBehaviour, ICollisionEnterHandler
{
    public GameObject explosion;
    private FutureBehaviour[] futureBehaviours;
    private void Awake()
    {
        futureBehaviours = GetComponents<FutureBehaviour>();
    }

    public void VirtualStepCollisionEnter(int step, FutureCollision collision)
    {
        foreach (var futureBehaviour in futureBehaviours)
        {
            futureBehaviour.Disable(disabledFromStep:step);
        }
    }

    public void StepCollisionEnter(int step, FutureCollision collision)
    {
        if (this == null) return;
        var instantiated = Instantiate(explosion, transform.position, Quaternion.identity);
        if (instantiated.TryGetComponent<Rigidbody2D>(out var rb2d))
        {
            rb2d.velocity = (collision.my.futureTransform.GetFuturePosition(step + 1) -
                             collision.my.futureTransform.GetFuturePosition(step)) / FuturePhysics.DeltaTime;
        }
        Destroy(gameObject);
    }
}
