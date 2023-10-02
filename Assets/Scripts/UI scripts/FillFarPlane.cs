using UnityEngine;

public class FillFarPlane: MonoBehaviour
{
    void Update()
    {
        var cam = Camera.main!;

        var pos = (cam.farClipPlane - 10f); // due precision issues

        transform.position = cam.transform.position + cam.transform.forward * pos;

        var h = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f)*pos*2f;

        transform.localScale = new Vector3(h * cam.aspect,h,0f);
    }
}
