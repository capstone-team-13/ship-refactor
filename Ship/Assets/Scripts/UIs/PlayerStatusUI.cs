using JetBrains.Annotations;
using UnityEngine;

public class PlayerStatusUI : MonoBehaviour
{
    #region Editor API

    [SerializeField] private GameObject m_player;
    [SerializeField] private SlicedFilledImage m_healthStatus;
    [SerializeField] private SlicedFilledImage m_manaStatus;

    [SerializeField] private float m_changeSpeed = 5.0f;

    #endregion

    #region Unity Callbacks

    [UsedImplicitly]
    private void OnEnable()
    {
        LevelManager.PlayerEventBus.SubscribeToTarget<PlayerHealthChanged>(m_player, OnPlayerHealthChanged);
        LevelManager.PlayerEventBus.SubscribeToTarget<PlayerManaChanged>(m_player, OnPlayerManaChanged);
    }

    [UsedImplicitly]
    private void OnDisable()
    {
        LevelManager.PlayerEventBus.UnsubscribeFromTarget<PlayerHealthChanged>(m_player.gameObject,
            OnPlayerHealthChanged);
        LevelManager.PlayerEventBus.UnsubscribeFromTarget<PlayerManaChanged>(m_player.gameObject, OnPlayerManaChanged);
    }

    [UsedImplicitly]
    private void Update()
    {
        var speed = m_changeSpeed * Time.deltaTime;
        m_healthStatus.fillAmount = Mathf.Lerp(m_healthStatus.fillAmount, m_targetHealthFillAmount, speed);
        m_manaStatus.fillAmount = Mathf.Lerp(m_manaStatus.fillAmount, m_targetManaFillAmount, speed);
    }

    #endregion

    #region Event Handlers

    private void OnPlayerHealthChanged(ref PlayerHealthChanged eventData, GameObject target, GameObject source)
    {
        var fillAmount = eventData.Current / eventData.Max;
        m_targetHealthFillAmount = fillAmount;
    }

    private void OnPlayerManaChanged(ref PlayerManaChanged eventData, GameObject target, GameObject source)
    {
        var fillAmount = eventData.Current / eventData.Max;
        m_targetManaFillAmount = fillAmount;
    }

    #endregion

    #region Internals

    private float m_targetHealthFillAmount;
    private float m_targetManaFillAmount;

    #endregion
}