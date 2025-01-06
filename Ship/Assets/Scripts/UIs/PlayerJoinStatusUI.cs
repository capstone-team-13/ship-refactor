using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerJoinStatusUI : MonoBehaviour
{
    [SerializeField] private int m_index = -1;
    [SerializeField] private Image m_bottomDecorationImage;

    [SerializeField] private Color m_playerWaitingColor;
    [SerializeField] private Color m_playerReadyColor;

    [SerializeField] private Animator m_playerAnimator;

    private Tween m_colorTween;
    private List<string> m_joinedAnimationTriggers = new();
    private List<string> m_exitedAnimationTriggers = new();

    private void Start()
    {
        if (m_playerAnimator == null)
        {
            Debug.LogError("Animator is not assigned!");
            return;
        }

        var parameters = m_playerAnimator.parameters;

        const string joinedAnimationPrefix = "$(Joined)";
        foreach (AnimatorControllerParameter parameter in parameters)
        {
            bool isTrigger = parameter.type == AnimatorControllerParameterType.Trigger;
            if (!isTrigger || !parameter.name.StartsWith(joinedAnimationPrefix)) continue;

            m_joinedAnimationTriggers.Add(parameter.name);
        }

        const string exitedAnimationPrefix = "$(Exited)";
        foreach (AnimatorControllerParameter parameter in parameters)
        {
            bool isTrigger = parameter.type == AnimatorControllerParameterType.Trigger;
            if (!isTrigger || !parameter.name.StartsWith(exitedAnimationPrefix)) continue;

            m_exitedAnimationTriggers.Add(parameter.name);
        }

        m_playerAnimator.SetBool("Mirror", m_index != 0);
    }

    private void OnEnable()
    {
        LevelManager.PlayerEventBus.SubscribeTo<PlayerJoinedEvent>(OnPlayerJoined);
        LevelManager.PlayerEventBus.SubscribeTo<PlayerExitedEvent>(OnPlayerExited);
    }

    private void OnDisable()
    {
        LevelManager.PlayerEventBus.UnsubscribeFrom<PlayerJoinedEvent>(OnPlayerJoined);
        LevelManager.PlayerEventBus.UnsubscribeFrom<PlayerExitedEvent>(OnPlayerExited);
    }

    private void OnPlayerJoined(ref PlayerJoinedEvent eventData, GameObject target, GameObject source)
    {
        if (eventData.Index != m_index) return;
        __M_AnimateBottomDecorationColor(m_playerReadyColor);

        int randomIndex = Random.Range(0, m_joinedAnimationTriggers.Count);
        m_playerAnimator.SetTrigger(m_joinedAnimationTriggers[randomIndex]);
    }

    private void OnPlayerExited(ref PlayerExitedEvent eventData, GameObject target, GameObject source)
    {
        if (eventData.Index != m_index) return;
        __M_AnimateBottomDecorationColor(m_playerWaitingColor);

        int randomIndex = Random.Range(0, m_exitedAnimationTriggers.Count);
        m_playerAnimator.SetTrigger(m_exitedAnimationTriggers[randomIndex]);
    }

    private void __M_AnimateBottomDecorationColor(Color targetColor, float duration = 0.375f,
        Ease easeType = Ease.InOutSine)
    {
        m_colorTween?.Kill();
        m_colorTween = m_bottomDecorationImage
            .DOColor(targetColor, duration)
            .SetEase(easeType);
    }

    private void __M_EnableAnimation()
    {
    }
}