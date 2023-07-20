using UnityEngine;

public class CircleFutureCollider : FutureCollider
{
   public float radius;

   private void Start()
   {
      var spheres = gameObject.GetComponentsInChildren<SphereCollider>();
      if (spheres.Length > 0)
      {
         radius = spheres[0].radius * spheres[0].transform.lossyScale.x;
      }
      else
      {
         var circles = gameObject.GetComponentsInChildren<CircleCollider2D>();
         if (circles.Length > 0)
         {
            radius = circles[0].radius * circles[0].transform.lossyScale.x;
         }
      }
   }
}
