using JetBrains.Annotations;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
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
        float yaw = CalculateYaw();
        float pitch = CalculatePitch();

        float newPitch = ClampPitch(pitch);
        Vector3 newPosition = CalculatePosition(newPitch, yaw);

        UpdateTransform(newPosition);
    }

    [UsedImplicitly]
    private void OnEnable()
    {
        var target = m_target.gameObject;
        LevelManager.PlayerEventBus.SubscribeToTarget<PlayerLookEvent>(target, OnPlayerLooked);
        LevelManager.ShipEventBus.SubscribeToSource<RudderControlStartedEvent>(target, OnRudderControlStarted);
        LevelManager.ShipEventBus.SubscribeToSource<RudderControlEndedEvent>(target, OnRudderControlEnded);
    }

    [UsedImplicitly]
    private void OnDisable()
    {
        var target = m_target.gameObject;
        LevelManager.PlayerEventBus.UnsubscribeFromTarget<PlayerLookEvent>(target, OnPlayerLooked);
        LevelManager.ShipEventBus.UnsubscribeFromSource<RudderControlStartedEvent>(target, OnRudderControlStarted);
        LevelManager.ShipEventBus.UnsubscribeFromSource<RudderControlEndedEvent>(target, OnRudderControlEnded);
    }

    #endregion

    #region Event Handlers

    private void OnPlayerLooked(ref PlayerLookEvent eventData, GameObject target, GameObject source)
    {
        m_delta = eventData.Delta;
    }

    private void OnRudderControlStarted(ref RudderControlStartedEvent eventData, GameObject target, GameObject source)
    {
        Debug.Log("OnRudderControlStarted");
    }

    private void OnRudderControlEnded(ref RudderControlEndedEvent eventData, GameObject target, GameObject source)
    {
        Debug.Log("OnRudderControlEnded");
    }

    #endregion

    private float CalculateYaw()
    {
        return m_delta.x * m_rotationSpeed * Time.deltaTime;
    }

    private float CalculatePitch()
    {
        return -m_delta.y * m_rotationSpeed * Time.deltaTime;
    }

    private float ClampPitch(float pitch)
    {
        Vector3 currentAngles = transform.eulerAngles;
        float normalizedPitch = Mathf.DeltaAngle(0, currentAngles.x) + pitch;

        return Mathf.Clamp(normalizedPitch,
            m_defaultAngles.x - m_pitchLimit, m_defaultAngles.x + m_pitchLimit);
    }

    private Vector3 CalculatePosition(float pitch, float yaw)
    {
        Vector3 direction = transform.position - m_target.position;
        float distance = direction.magnitude;
        Quaternion rotation = Quaternion.Euler(pitch, transform.eulerAngles.y + yaw, 0);
        direction = rotation * Vector3.back * distance;
        return m_target.position + direction;
    }

    private void UpdateTransform(Vector3 newPosition)
    {
        transform.position = newPosition;
        transform.LookAt(m_target);
    }
}