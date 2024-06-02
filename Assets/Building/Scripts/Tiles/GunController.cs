using System;
using System.Linq;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public float lastTimeFired;
    public float fireRate;
    public Bullet bulletPrefab;
    
    
    private void Update()
    {
        if (ModeSwitcher.CurrentMode != ModeSwitcher.Mode.Battle)
        {
            return;
        }

        if (Enemy.all.Count == 0)
        {
            return;
        }
        var closest = Enemy.all.First();
        var closestDist = Vector3.Distance(closest.transform.position, transform.position);
        foreach (var enemy in Enemy.all)
        {
            var distance = Vector3.Distance(enemy.transform.position, transform.position);
            if (distance < closestDist)
            {
                closestDist = distance;
                closest = enemy;
            }
        }
        var direction = closest.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        if (Time.time - lastTimeFired > 1f / fireRate)
        {
            var origin = transform.position +
                         direction.normalized * RenderingSettings.Instance.side;
            
            var hit = Physics2D.Raycast(origin, direction, float.PositiveInfinity, 
                LayerMask.GetMask("Player"));
            if (!hit)
            {
                lastTimeFired = Time.time;
                var bullet = Instantiate(bulletPrefab, origin, transform.rotation);
                bullet.Launch(direction);
            }
        }
    }
}