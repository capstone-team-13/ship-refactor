using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SupportForce : MonoBehaviour
{
    // TODO: Refactor to frequency
    [Header("Configs")] [SerializeField] private float m_springConstant;
    [SerializeField] private float m_dampingConstant;
    [SerializeField] private Vector3 m_targetPosition;

    [Header("Refs")] [SerializeField] private Rigidbody m_rigidBody;

    public Vector3 TargetPosition
    {
        get => m_targetPosition;
        set => m_targetPosition = value;
    }

    #region Unity Callback

    [UsedImplicitly]
    private void FixedUpdate()
    {
        Vector3 supportForce = _M_SolveSupportForce();
        m_rigidBody.AddForce(supportForce, ForceMode.Force);
    }

    #endregion

    #region Internal

    private Vector3 _M_SolveSupportForce()
    {
        Vector3 currentPosition = m_rigidBody.transform.position;
        Vector3 currentVelocity = m_rigidBody.velocity;

        Vector3 springForce = m_springConstant * (m_targetPosition - currentPosition);
        Vector3 convergenceForce = m_dampingConstant * (Vector3.zero - currentVelocity);

        Vector3 force = springForce + convergenceForce;
        return force;
    }

    #endregion
}