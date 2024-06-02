using UnityEngine;
using UnityEngine.Events;

public class ModeSwitcher: MonoBehaviour
{
    public static readonly UnityEvent<Mode> modeChanged = new();

    private static Mode currentMode;
    
    public enum Mode
    {
        Building,
        Battle,
    }

    public void SwitchToBattle()
    {
        if (CurrentMode == Mode.Battle)
        {
            CurrentMode = Mode.Building;
        }
        else
        {
            CurrentMode = Mode.Battle;
        }
    }

    public static Mode CurrentMode
    {
        get => currentMode;
        set
        {
            currentMode = value;
            modeChanged.Invoke(currentMode);
        }
    }
}
