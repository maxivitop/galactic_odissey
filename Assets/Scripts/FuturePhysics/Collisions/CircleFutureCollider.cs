using System;
using UnityEngine;

public class CircleFutureCollider : FutureCollider
{
   [NonSerialized]
   public float radius;

   private void Start()
   {
      TryGetComponent<SphereCollider>(out var sphere);
      if (sphere != null)
      {
         radius = sphere.radius;
      }
      TryGetComponent<CircleFutureCollider>(out var circle);
      if (circle != null)
      {
         radius = circle.radius;
      }

      var lossyScale = transform.lossyScale;
      radius *= Mathf.Max(lossyScale.x,
         lossyScale.y,
         lossyScale.z);
   }
}
