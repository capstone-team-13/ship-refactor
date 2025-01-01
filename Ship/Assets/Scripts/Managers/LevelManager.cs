using GenericEventBus;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : SingletonBehaviour<LevelManager>
{
    #region Editor API

    [SerializeField] private string m_levelName = "Level Name";
    [SerializeField] private LayerMask m_entityLayer;
    public LayerMask EntityLayer => m_entityLayer;

    public UnityEvent OnLevelStarted;
    public UnityEvent OnLevelEnded;

    #endregion

    #region API

    public static GenericEventBus<IEvent> EventBus { get; } = new();
    public static GenericEventBus<IEvent, GameObject> PlayerEventBus { get; } = new();
    public static GenericEventBus<IEvent, GameObject> ShipEventBus { get; } = new();

    #endregion

    #region Unity Callbacks

    [UsedImplicitly]
    private void Start()
    {
        StartLevel();
    }

    [UsedImplicitly]
    private void OnEnable()
    {
        EventBus.SubscribeTo<LevelStartedEvent>(OnLevelStartedEvent);
        EventBus.SubscribeTo<LevelEndedEvent>(OnLevelEndedEvent);
    }

    [UsedImplicitly]
    private void OnDisable()
    {
        EventBus.UnsubscribeFrom<LevelStartedEvent>(OnLevelStartedEvent);
        EventBus.UnsubscribeFrom<LevelEndedEvent>(OnLevelEndedEvent);
    }

    private void OnDestroy()
    {
        EndLevel();
    }

    #endregion

    public void StartLevel()
    {
        EventBus.Raise(new LevelStartedEvent(m_levelName));
    }

    public void EndLevel()
    {
        EventBus.Raise(new LevelEndedEvent(m_levelName));
    }

    #region Event Handlers

    private void OnLevelStartedEvent(ref LevelStartedEvent eventData)
    {
        Debug.Log($"{eventData.Name} started...");
        OnLevelStarted?.Invoke();
    }

    private void OnLevelEndedEvent(ref LevelEndedEvent eventData)
    {
        Debug.Log($"{eventData.Name} ended...");
        OnLevelEnded?.Invoke();
    }

    #endregion
}