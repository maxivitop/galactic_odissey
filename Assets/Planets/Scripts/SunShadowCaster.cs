using UnityEngine;

public class SunShadowCaster : MonoBehaviour {
	Transform track;

	void Start () {
		track = Camera.main!.transform;
	}

	void LateUpdate ()
	{
		var targetPos = track.position;
		targetPos.z = 0;
		transform.LookAt (targetPos);
	}
}