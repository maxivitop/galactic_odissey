using UnityEngine;

public class PlanetRotator: MonoBehaviour
{
    public Vector3 up = Vector3.back;
    public float speed;
    private float rotation;

    private void Update()
    {
        rotation += speed * Time.deltaTime;
        transform.rotation = Quaternion.AngleAxis(rotation, up);
    }
}