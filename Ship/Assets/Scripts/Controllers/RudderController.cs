using EE.Interactions;
using UnityEngine;

public class RudderController : MonoBehaviour, IInteractable
{
    private Interactor m_currentInteractor;

    [SerializeField] private GameObject m_ship;
    [SerializeField] private Transform m_controlPoint;

    #region API

    public Transform Transform => transform;

    public bool CanInteract(Interactor interactor)
    {
        // toggle to exit current interaction
        if (interactor == m_currentInteractor)
        {
            m_currentInteractor = null;
            LevelManager.ShipEventBus.Raise(new RudderControlEndedEvent(), m_ship, interactor.gameObject);
            return false;
        }

        Vector3 interactorDirection = (interactor.transform.position - transform.position).normalized;
        float dotProduct = Vector3.Dot(transform.forward, interactorDirection);

        bool isBehind = dotProduct < 0;
        bool notInUsed = m_currentInteractor == null;

        return notInUsed && isBehind;
    }

    public void Interact(Interactor interactor)
    {
        m_currentInteractor = interactor;
        LevelManager.ShipEventBus.Raise(new RudderControlStartedEvent(m_controlPoint), m_ship,
            interactor.gameObject);
    }

    #endregion
}