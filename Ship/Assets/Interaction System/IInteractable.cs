using UnityEngine;

namespace EE.Interactions
{
    public interface IInteractable
    {
        Transform Transform { get; }
        public bool CanInteract(Interactor interactor);
        public void Interact(Interactor interactor);
    }
}