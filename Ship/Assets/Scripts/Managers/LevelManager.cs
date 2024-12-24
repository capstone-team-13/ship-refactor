using GenericEventBus;
using JetBrains.Annotations;
using UnityEngine;

public class LevelManager : SingletonBehaviour<LevelManager>
{
    #region Editor API

    [SerializeField] private string m_levelName = "Level Name";

    #endregion

    #region API

    public static GenericEventBus<IEvent> EventBus { get; } = new();
    public static GenericEventBus<IEvent, PlayerModel> PlayerEventBus { get; } = new();

    #endregion

    #region Unity Callbacks

    [UsedImplicitly]
    private void Start()
    {
        EventBus.Raise(new LevelStartedEvent(m_levelName));
    }

    [UsedImplicitly]
    private void OnEnable()
    {
        EventBus.SubscribeTo<LevelStartedEvent>(OnLevelStartedEvent);
    }

    [UsedImplicitly]
    private void OnDisable()
    {
        EventBus.UnsubscribeFrom<LevelStartedEvent>(OnLevelStartedEvent);
    }

    #endregion

    #region Event Handlers

    private static void OnLevelStartedEvent(ref LevelStartedEvent eventData)
    {
        Debug.Log($"{eventData.Name} started...");
    }

    #endregion
}