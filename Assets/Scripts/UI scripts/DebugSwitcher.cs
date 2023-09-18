using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugSwitcher: MonoBehaviour
{
    public Singularity starSingularity;
    public Light pointLight;
    public GameObject projectileLauncher;
    public GameObject projectileLauncherPlanet;
    public float blackHoleSwitchSpeed = 1;
    public float lightIntensityMax = 1;
    

    private bool isBlackHoleEnabled;
    private bool isProjectileLauncherEnabled;
    private float initialLightIntensity;
    private Vector3 projectileLauncherRelativePosition;
    private float transitionFraction;

    private void Start()
    {
        projectileLauncherRelativePosition = 
            projectileLauncher.transform.position - projectileLauncherPlanet.transform.position;
        projectileLauncher.SetActive(false);
        initialLightIntensity = pointLight.intensity;
    }

    private static float SCurve(float x)
    {
        return 1 / (1 + Mathf.Pow(x / (1 - x), -3));
    }
    
    private static float BellCurve(float x)
    {
        return 32 * x * x * x * Mathf.Pow(1 - x, 3);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isBlackHoleEnabled = !isBlackHoleEnabled;
        }
        
        transitionFraction += Time.deltaTime * blackHoleSwitchSpeed * (isBlackHoleEnabled ? 1 : -1);
        transitionFraction = Mathf.Clamp01(transitionFraction);
        var t = SCurve(transitionFraction);
        starSingularity.t = t*t;
        
        pointLight.intensity = initialLightIntensity + BellCurve(transitionFraction) * lightIntensityMax;
        
        if (Input.GetKeyDown(KeyCode.M))
        {
            isProjectileLauncherEnabled = !isProjectileLauncherEnabled;
            projectileLauncher.SetActive(isProjectileLauncherEnabled);
            projectileLauncher.transform.position =
                projectileLauncherPlanet.transform.position + projectileLauncherRelativePosition;
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
