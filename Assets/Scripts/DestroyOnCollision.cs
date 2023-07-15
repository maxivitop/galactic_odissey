using UnityEngine;

public class DestroyOnCollision : MonoBehaviour, ICollisionEnterHandler
{
    public GameObject explosion;
    private FutureBehaviour[] futureBehaviours;
    private void Start()
    {
        futureBehaviours = GetComponents<FutureBehaviour>();
    }

    public void VirtualStepCollisionEnter(int step, FutureCollision collision)
    {
        foreach (var futureBehaviour in futureBehaviours)
        {
            futureBehaviour.Disable(disablingSourceStep:step, disabledFromStep:step);
        }
    }

    public void StepCollisionEnter(int step, FutureCollision collision)
    {
        if (this == null) return;
        Instantiate(explosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
