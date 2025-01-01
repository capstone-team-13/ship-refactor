using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    #region Editor API

    [SerializeField] private Transform m_rayOrigin;
    [SerializeField] private Vector3 m_rayDirection = Vector3.down;
    [SerializeField] private float m_rayLength;
    [SerializeField] private LayerMask m_groundLayer;

    #endregion

    #region API

    public bool IsGrounded => __M_IsGrounded();

    #endregion

    #region Unity Callbacks

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(m_rayOrigin.position, m_rayDirection * m_rayLength);
    }

    #endregion

    #region Internal

    private bool __M_IsGrounded()
    {
        return Physics.Raycast(m_rayOrigin.position, m_rayDirection, m_rayLength,
            m_groundLayer, QueryTriggerInteraction.Ignore);
    }

    #endregion
}