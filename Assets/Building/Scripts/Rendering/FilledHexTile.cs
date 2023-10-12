using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FilledHexTile : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
}
