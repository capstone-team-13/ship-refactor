using JetBrains.Annotations;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private ProjectileModel m_model;
    [SerializeField] private Rigidbody m_rigidbody;

    public ProjectileType ProjectileType => m_model.Type;

    public void ResetState()
    {
        m_model.TimeElapsed = 0.0f;

        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        m_rigidbody.velocity = Vector3.zero;
        m_rigidbody.angularVelocity = Vector3.zero;
    }

    [UsedImplicitly]
    private void Update()
    {
        ProjectileModel model = m_model;
        model.TimeElapsed += Time.deltaTime;
        if (model.Lifetime <= model.TimeElapsed) ProjectileManager.Return(this);
    }

    [UsedImplicitly]
    private void OnCollisionEnter(Collision collision)
    {
    }

    public void Launch(Vector3 force)
    {
        m_rigidbody.AddForce(force, ForceMode.Impulse);
    }
}