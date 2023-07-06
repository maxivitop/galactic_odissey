using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static Vector3 worldMousePosition
    {
        get
        {
            return Camera.main.ScreenToWorldPoint(new Vector3(
                    Input.mousePosition.x,
                    Input.mousePosition.y,
                    -Camera.main.transform.position.z
           ));
        }
    }
    public static IFuturePositionProvider SelectFuturePositionProvider(GameObject gameObject)
    {
        int maxPriority = int.MinValue;
        IFuturePositionProvider maxPriorityFuturePositionProvider = null;
        foreach (IFuturePositionProvider futurePositionProvider in gameObject.GetComponents<IFuturePositionProvider>())
        {
            if (maxPriority < futurePositionProvider.GetPriority()) {
                maxPriority = futurePositionProvider.GetPriority();
                maxPriorityFuturePositionProvider = futurePositionProvider;
            }
        }
        return maxPriorityFuturePositionProvider;
    }
}
