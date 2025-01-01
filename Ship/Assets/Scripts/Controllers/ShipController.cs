using JetBrains.Annotations;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    [SerializeField] private ShipModel m_shipModel;
    [SerializeField] private SupportForce m_supportForce;
    [SerializeField] private Rigidbody m_rigidbody;

    private float m_deltaX;

    #region Unity Callbacks

    [UsedImplicitly]
    private void OnEnable()
    {
        LevelManager.ShipEventBus.SubscribeToTarget<ShipMoveEvent>(gameObject, OnShipMoved);
        LevelManager.ShipEventBus.SubscribeToTarget<ShipSteeredEvent>(gameObject, OnShipSteered);
    }


    [UsedImplicitly]
    private void OnDisable()
    {
        LevelManager.ShipEventBus.UnsubscribeFromTarget<ShipMoveEvent>(gameObject, OnShipMoved);
        LevelManager.ShipEventBus.UnsubscribeFromTarget<ShipSteeredEvent>(gameObject, OnShipSteered);
    }

    [UsedImplicitly]
    private void FixedUpdate()
    {
        ShipModel model = m_shipModel;
        Vector3 delta = model.Speed * Time.fixedDeltaTime * model.Direction;
        Vector3 targetPosition = transform.position + delta;
        targetPosition.y = 0;

        float yaw = m_deltaX * model.RotationSpeed * Time.fixedDeltaTime;
        Vector3 currentAngles = transform.eulerAngles;
        float newYaw = currentAngles.y + yaw;

        transform.rotation = Quaternion.Euler(currentAngles.x, newYaw, currentAngles.z);

        model.Direction = transform.forward.normalized;
        LevelManager.ShipEventBus.Raise(new ShipMoveEvent(targetPosition), gameObject, gameObject);
    }

    [UsedImplicitly]
    private void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        var playerRigidbody = collision.gameObject.GetComponent<Rigidbody>();
        if (playerRigidbody == null) return;

        Vector3 playerPosition = collision.transform.position;
        Vector3 platformMovement = m_rigidbody.velocity * Time.fixedDeltaTime;
        collision.transform.position = playerPosition + platformMovement;
    }

    #endregion

    #region Event Handlers

    private void OnShipMoved(ref ShipMoveEvent eventData, GameObject target, GameObject source)
    {
        m_supportForce.TargetPosition = eventData.TargetPosition;
    }

    private void OnShipSteered(ref ShipSteeredEvent eventData, GameObject target, GameObject source)
    {
        m_deltaX = eventData.DeltaX;
    }

    #endregion
}