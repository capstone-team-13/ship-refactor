using JetBrains.Annotations;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
    #region Editor API

    [SerializeField] private Transform m_target;

    [SerializeField] private Vector3 m_offset;
    [SerializeField] private float m_followSpeed = 5f;
    [SerializeField] private float m_dampingTime = 0.1f;

    public Transform Target
    {
        get => m_target;
        set => m_target = value;
    }

    public Vector3 FacingDirection => m_target.forward;

    #endregion

    private Vector3 m_velocity;

    [UsedImplicitly]
    private void Update()
    {
        Vector3 targetPosition = m_target.position + m_offset;
        transform.position =
            Vector3.SmoothDamp(transform.position, targetPosition, ref m_velocity, m_dampingTime, m_followSpeed);
    }
}