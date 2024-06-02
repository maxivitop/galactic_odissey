 using System;
 using System.Collections.Generic;
 using UnityEngine;
 
 public class Enemy: MonoBehaviour
 {
     public static HashSet<Enemy> all = new();

     private void Awake()
     {
         all.Add(this);
         ModeSwitcher.modeChanged.AddListener((mode) =>
         {
             if (mode != ModeSwitcher.Mode.Battle)
             {
                Destroy(gameObject);
             }
         });
     }

     private void OnDestroy()
     {
         all.Remove(this);
     }
 }