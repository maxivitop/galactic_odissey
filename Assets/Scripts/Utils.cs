using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static Vector3 WorldMousePosition =>
        Camera.main!.ScreenToWorldPoint(new Vector3(
            Input.mousePosition.x,
            Input.mousePosition.y,
            -Camera.main.transform.position.z
        ));

    public static IFuturePositionProvider SelectFuturePositionProvider(GameObject gameObject)
    {
        var maxPriority = int.MinValue;
        IFuturePositionProvider maxPriorityFuturePositionProvider = null;
        foreach (var futurePositionProvider in gameObject.GetComponents<IFuturePositionProvider>())
            if (maxPriority < futurePositionProvider.GetPriority())
            {
                maxPriority = futurePositionProvider.GetPriority();
                maxPriorityFuturePositionProvider = futurePositionProvider;
            }

        return maxPriorityFuturePositionProvider;
    }
}