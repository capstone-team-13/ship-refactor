using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class CountDownTimer : MonoBehaviour
{
    [SerializeField] [Tooltip("In minutes")]
    private float m_time;

    [Space(4)] public UnityEvent<float> OnCountdownUpdated;
    [Space(4)] public UnityEvent OnCountdownFinished;

    private float m_remainingTime;
    private bool m_isCountingDown;

    private void Awake()
    {
        // Convert user input in minute to second
        m_time = m_time * 60;
    }

    [UsedImplicitly]
    private void Update()
    {
        if (!m_isCountingDown || !(m_remainingTime > 0)) return;
        m_remainingTime -= Time.deltaTime;

        OnCountdownUpdated?.Invoke(m_remainingTime);

        if (!(m_remainingTime <= 0)) return;

        m_remainingTime = 0;
        m_isCountingDown = false;
        OnCountdownFinished?.Invoke();
    }

    public void StartCountdown()
    {
        m_remainingTime = m_time;
        m_isCountingDown = true;
    }

    public void StopCountdown()
    {
        m_isCountingDown = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="time">in seconds</param>
    public void AddTime(float time)
    {
        m_time += time;
    }

    public float GetRemainingTime()
    {
        return m_remainingTime;
    }
}