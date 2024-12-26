using JetBrains.Annotations;
using ModiBuff.Core;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;

[RequireComponent(typeof(PlayerModel))]
public class PlayerController : MonoBehaviour
{
    private static class PlayerActions
    {
        public const string MOVEMENT = "Movement";
        public const string LOOK = "Look";
        public const string MELEE = "Melee";
        public const string JUMP = "Jump";
        public const string PICK_OR_THROW = "Pick or Throw";
        public const string SHIELD = "Shield";
        public const string DIVINE_BOON = "Divine Boon";
        public const string SHOOT = "Shoot";
        public const string HEALING_BOOST = "Energy Boost";
        public const string MANA_TRANSFER = "Energy Transfer";
    }

    #region Editor API

    [Header("Player Components")] [SerializeField]
    private PlayerModel m_playerModel;

    [SerializeField] private PlayerInput m_playerInput;
    [SerializeField] private Rigidbody m_rigidbody;

    [SerializeField] private BehaviourTreeOwner m_behaviourTree;

    [Header("Camera and Transform")] [SerializeField]
    private Transform m_cameraTransform;

    [Header("Ground Check")] [SerializeField]
    private GroundChecker m_groundChecker;

    private GameObject m_pickedObject;

    #endregion

    #region Unity Callbacks

    private void OnEnable()
    {
        if (m_playerInput != null)
        {
            m_playerInput.actions[PlayerActions.JUMP].performed += OnJumpPerformed;

            m_playerInput.actions[PlayerActions.MELEE].performed += OnMeleePerformed;

            m_playerInput.actions[PlayerActions.PICK_OR_THROW].performed += OnPickOrThrowPerformed;

            m_playerInput.actions[PlayerActions.SHIELD].started += OnShieldStarted;
            m_playerInput.actions[PlayerActions.SHIELD].canceled += OnShieldCanceled;

            m_playerInput.actions[PlayerActions.SHOOT].started += OnShootStarted;
            m_playerInput.actions[PlayerActions.SHOOT].canceled += OnShootCanceled;

            m_playerInput.actions[PlayerActions.HEALING_BOOST].performed += OnHealBoostPerformed;
        }
    }

    [UsedImplicitly]
    private void OnDisable()
    {
        if (m_playerInput != null)
        {
            m_playerInput.actions[PlayerActions.JUMP].performed -= OnJumpPerformed;

            m_playerInput.actions[PlayerActions.MELEE].performed -= OnMeleePerformed;

            m_playerInput.actions[PlayerActions.PICK_OR_THROW].performed -= OnPickOrThrowPerformed;

            m_playerInput.actions[PlayerActions.SHIELD].started -= OnShieldStarted;
            m_playerInput.actions[PlayerActions.SHIELD].canceled -= OnShieldCanceled;

            m_playerInput.actions[PlayerActions.SHOOT].started -= OnShootStarted;
            m_playerInput.actions[PlayerActions.SHOOT].canceled -= OnShootCanceled;

            m_playerInput.actions[PlayerActions.HEALING_BOOST].performed -= OnHealBoostPerformed;
        }
    }

    [UsedImplicitly]
    private void OnValidate()
    {
        if (m_playerInput == null)
        {
            Debug.LogError("PlayerInput reference is null. Please assign it in the inspector.");
            return;
        }

        var actionMapNames = m_playerInput.actions.Select(a => a.name).ToList();
        var invalidActions = typeof(PlayerActions).GetFields()
            .Where(field => field.IsLiteral && !field.IsInitOnly)
            .Select(field => field.GetValue(null)?.ToString())
            .Where(actionName => !string.IsNullOrEmpty(actionName) && !actionMapNames.Contains(actionName))
            .ToList();

        if (invalidActions.Any())
        {
            var builder = new System.Text.StringBuilder();
            builder.AppendLine(
                $"GameObject '{gameObject.name}' in script '{GetType().Name}' has the following invalid actions in PlayerActions:");

            foreach (string actionName in invalidActions) builder.AppendLine($"- {actionName}");

            builder.AppendLine("\nPlease check for typos or consider commenting out unused constants.");
            Debug.LogError(builder.ToString());
        }
    }

    [UsedImplicitly]
    private void Update()
    {
        PlayerModel model = m_playerModel;
        if (model.IsDead) return;
        Vector3 newVelocity = model.Direction * model.Speed;
        newVelocity.y = m_rigidbody.velocity.y;

        // m_rigidbody.velocity = newVelocity;

        IBlackboard blackboard = m_behaviourTree.graph.blackboard;
        blackboard.SetVariableValue("Velocity", newVelocity);
    }

    #endregion

    #region Event Handlers

    // Handles the OnMovement event triggered by continuous input.
    // This method is invoked via SendMessage to process player movement input.
    [UsedImplicitly]
    public void OnMovement(InputValue value)
    {
        if (m_playerModel.IsDead) return;

        var rawInput = value.Get<Vector2>();

        Vector3 cameraForward = m_cameraTransform.forward;
        Vector3 cameraRight = m_cameraTransform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 inputDirection = cameraRight * rawInput.x + cameraForward * rawInput.y;

        LevelManager.PlayerEventBus.Raise(new PlayerMoveEvent { Direction = inputDirection }, gameObject, null);
    }

    // Handles the OnLook event triggered by continuous input.
    // This method is invoked via SendMessage to process player look input.
    [UsedImplicitly]
    public void OnLook(InputValue value)
    {
        var delta = value.Get<Vector2>();
        LevelManager.PlayerEventBus.Raise(new PlayerLookEvent { Delta = delta }, gameObject, null);
    }

    [UsedImplicitly]
    public void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (m_groundChecker.IsGrounded) m_playerModel.ResetJumpCount();
        if (__M_CanJump()) StartCoroutine(__M_Jump());
    }

    public void OnMeleePerformed(InputAction.CallbackContext context)
    {
        StartCoroutine(__M_Melee());

        LayerMask entityLayer = LevelManager.Instance.EntityLayer;
        int modifierId = ModifierManager.Instance.GetModifierId(Casts.MELEE);
        var colliders = Physics.OverlapSphere(transform.position, m_playerModel.MeleeRadius, entityLayer);

        foreach (Collider myCollider in colliders)
        {
            if (myCollider.CompareTag("Player"))
                continue;

            if (myCollider.TryGetComponent(out IModifierOwner modifierOwner))
                m_playerModel.TryCast(modifierId, modifierOwner);
        }
    }

    private IEnumerator __M_Melee()
    {
        m_rigidbody.velocity = Vector3.zero;
        m_behaviourTree.graph.blackboard.SetVariableValue("Melee Pressed", true);
        yield return new WaitForSeconds(0.25f);
        m_behaviourTree.graph.blackboard.SetVariableValue("Melee Pressed", false);
    }

    public void OnPickOrThrowPerformed(InputAction.CallbackContext context)
    {
        if (m_pickedObject == null)
        {
            Debug.Log("Picked");
        }
        else
        {
            Debug.Log("Throw");
        }
    }

    public void OnShieldStarted(InputAction.CallbackContext context)
    {
        m_behaviourTree.graph.blackboard.SetVariableValue("Shielding", true);
    }

    public void OnShieldCanceled(InputAction.CallbackContext context)
    {
        m_behaviourTree.graph.blackboard.SetVariableValue("Shielding", false);
    }

    public void OnShootStarted(InputAction.CallbackContext context)
    {
        m_behaviourTree.graph.blackboard.SetVariableValue("Shooting", true);
    }

    public void OnShootCanceled(InputAction.CallbackContext context)
    {
        m_behaviourTree.graph.blackboard.SetVariableValue("Shooting", false);
    }

    public void OnHealBoostPerformed(InputAction.CallbackContext context)
    {
        int modifierId = ModifierManager.Instance.GetModifierId(Casts.HEAL);
        m_playerModel.TryCast(modifierId, m_playerModel);
    }

    private bool __M_CanJump()
    {
        return m_playerModel.JumpCount > 0;
    }

    private IEnumerator __M_Jump()
    {
        PlayerModel model = m_playerModel;
        model.HandleJump();

        // Reset Y Velocity
        Vector3 velocity = m_rigidbody.velocity;
        velocity.y = 0;

        IBlackboard blackboard = m_behaviourTree.graph.blackboard;
        blackboard.SetVariableValue("Velocity", velocity);
        blackboard.SetVariableValue("Jump Pressed", model.JumpPressed);
        blackboard.SetVariableValue("Jump Force", model.JumpForce);

        LevelManager.PlayerEventBus.Raise(new PlayerJumpEvent(), gameObject, null);

        yield return new WaitForEndOfFrame();

        --model.JumpCount;
        model.JumpPressed = false;
        m_behaviourTree.graph.blackboard.SetVariableValue("Jump Pressed", model.JumpPressed);
    }

    #endregion
}