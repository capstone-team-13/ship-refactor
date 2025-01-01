using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShooterStatusUI : MonoBehaviour
{
    [SerializeField] private GameObject m_player;
    [SerializeField] private Image m_shooterIcon;
    [SerializeField] private Image m_projectileIcon;
    [SerializeField] private TMP_Text m_ammoNumText;

    private void OnEnable()
    {
        LevelManager.PlayerEventBus.SubscribeToTarget<PlayerAmmoChangedEvent>(m_player, OnPlayerAmmoChanged);
    }

    private void OnDisable()
    {
        LevelManager.PlayerEventBus.UnsubscribeFromTarget<PlayerAmmoChangedEvent>(m_player, OnPlayerAmmoChanged);
    }

    private void OnPlayerAmmoChanged(ref PlayerAmmoChangedEvent eventData, GameObject target, GameObject source)
    {
        m_ammoNumText.text = $"{eventData.RemainingAmmo} | {eventData.MaxAmmo}";
    }
}