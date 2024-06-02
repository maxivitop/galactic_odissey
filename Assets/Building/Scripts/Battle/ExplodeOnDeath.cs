using UnityEngine;

[RequireComponent(typeof(Damageable))]
public class ExplodeOnDeath: MonoBehaviour
{
    public GameObject explosionPrefab;
    private Damageable damageable;

    private void Awake()
    {
        damageable = GetComponent<Damageable>();
        damageable.onDeath.AddListener((() =>
        {
            Instantiate(explosionPrefab, transform.position, transform.rotation);
            Destroy(gameObject);
        }));
    }
}
