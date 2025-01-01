using System.Linq;
using UnityEngine;

namespace EE.Interactions
{
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private float m_radius = 5f;
        [SerializeField] private LayerMask m_interactableLayerMask;

        public bool TryInteract()
        {
            Vector3 position = transform.position;

            var hitColliders = Physics.OverlapSphere(position, m_radius, m_interactableLayerMask);

            IInteractable nearestComponent = hitColliders
                .Select(collider => collider.GetComponent<IInteractable>())
                .Where(component => component != null && component.CanInteract(this))
                .OrderBy(component => Vector3.Distance(position, component.Transform.position))
                .FirstOrDefault();

            if (nearestComponent == null) return false;
            nearestComponent.Interact(this);
            return true;
        }
    }
}