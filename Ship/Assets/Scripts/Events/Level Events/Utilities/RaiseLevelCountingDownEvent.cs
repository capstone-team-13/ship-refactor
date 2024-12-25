using JetBrains.Annotations;
using UnityEngine;

public class RaiseLevelCountingDownEvent : MonoBehaviour
{
    [UsedImplicitly]
    public static void Raise(float remainingTime)
    {
        LevelManager.EventBus.Raise(new LevelCountingDownEvent(remainingTime));
    }
}