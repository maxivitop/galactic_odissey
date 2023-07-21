using System;
using UnityEngine;

public class LODController : MonoBehaviour
{
    private CelestialBodyGenerator[] celestialBodyGenerators;
    private Transform cameraTransform;
    public float lod1Threshold;
    public float lod2Threshold;

    private void Start()
    {
        celestialBodyGenerators = FindObjectsOfType<CelestialBodyGenerator>();
        cameraTransform = Camera.main!.transform;
    }

    private void Update()
    {
        foreach (var celestialBodyGenerator in celestialBodyGenerators)
        {
            var sqrMagnitude =
                (celestialBodyGenerator.transform.position - cameraTransform.position).sqrMagnitude;
            if (sqrMagnitude > lod2Threshold * lod2Threshold)
            {
                celestialBodyGenerator.SetLOD(2);
            }
            else if (sqrMagnitude > lod1Threshold * lod1Threshold)
            {
                celestialBodyGenerator.SetLOD(1);
            }
            else
            {
                celestialBodyGenerator.SetLOD(0);
            }
        }
    }
}