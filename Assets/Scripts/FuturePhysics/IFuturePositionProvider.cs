using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFuturePositionProvider
{
    Vector3 GetFuturePosition(int step, float dt=0);
    int GetPriority();

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