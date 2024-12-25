using TMPro;
using UnityEngine;

public class TimeLimitUI : MonoBehaviour
{
    [SerializeField] private TMP_Text m_minuteText;

    [SerializeField] private TMP_Text m_secondText;

    private void OnEnable()
    {
        LevelManager.EventBus.SubscribeTo<LevelCountingDownEvent>(OnLevelCountingDown);
    }

    private void OnDisable()
    {
        LevelManager.EventBus.UnsubscribeFrom<LevelCountingDownEvent>(OnLevelCountingDown);
    }

    private void OnLevelCountingDown(ref LevelCountingDownEvent eventData)
    {
        var remainingTime = eventData.remainingTime;
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        m_minuteText.text = minutes.ToString("00");
        m_secondText.text = seconds.ToString(":00");
    }
}