using JetBrains.Annotations;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform m_target;
    [SerializeField] private float m_rotationSpeed = 6f;
    [SerializeField] private float m_pitchLimit = 45f;

    private Vector3 m_defaultAngles;
    private Vector2 m_delta;

    #region Unity Callbacks

    private void Awake()
    {
        m_defaultAngles = transform.eulerAngles;
    }

    [UsedImplicitly]
    private void Update()
    {
        float yaw = m_delta.x * m_rotationSpeed * Time.deltaTime;
        float pitch = -m_delta.y * m_rotationSpeed * Time.deltaTime;

        Vector3 direction = transform.position - m_target.position;
        float distance = direction.magnitude;

        Vector3 currentAngles = transform.eulerAngles;
        float normalizedPitch = Mathf.DeltaAngle(0, currentAngles.x) + pitch;
        float newPitch = Mathf.Clamp(normalizedPitch,
            m_defaultAngles.x - m_pitchLimit, m_defaultAngles.x + m_pitchLimit);

        Quaternion rotation = Quaternion.Euler(newPitch, currentAngles.y + yaw, 0);
        direction = rotation * Vector3.back * distance;

        transform.position = m_target.position + direction;

        transform.LookAt(m_target);
    }

    [UsedImplicitly]
    private void OnEnable()
    {
        LevelManager.PlayerEventBus.SubscribeToTarget<PlayerLookEvent>(m_target.gameObject, OnPlayerLooked);
    }

    [UsedImplicitly]
    private void OnDisable()
    {
        LevelManager.PlayerEventBus.UnsubscribeFromTarget<PlayerLookEvent>(m_target.gameObject, OnPlayerLooked);
    }

    #endregion

    #region Event Handlers

    private void OnPlayerLooked(ref PlayerLookEvent eventData, GameObject target, GameObject source)
    {
        m_delta = eventData.Delta;
    }

    #endregion
}