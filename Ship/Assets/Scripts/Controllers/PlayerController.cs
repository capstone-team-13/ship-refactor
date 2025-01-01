using EE.Interactions;
using JetBrains.Annotations;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using System.Collections;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;

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
        public const string HEALING_BOOST = "Healing Boost";
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

    [Header("Interactions")] [SerializeField]
    private Interactor m_interactor;

    #endregion

    #region Unity Callbacks

    [UsedImplicitly]
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

        // TODO: Call when shooter swithed
        m_behaviourTree.graph.blackboard.SetVariableValue("Reloading Time", m_playerModel.Shooter.ReloadingTime);

        LevelManager.PlayerEventBus.SubscribeToTarget<PlayerReloadedEvent>(gameObject, OnPlayerReloaded);
        LevelManager.ShipEventBus.SubscribeToSource<RudderControlStartedEvent>(gameObject, OnRudderControlStarted);
        LevelManager.ShipEventBus.SubscribeToSource<RudderControlEndedEvent>(gameObject, OnRudderControlEnded);
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

        LevelManager.PlayerEventBus.UnsubscribeFromTarget<PlayerReloadedEvent>(gameObject, OnPlayerReloaded);
        LevelManager.ShipEventBus.UnsubscribeFromSource<RudderControlStartedEvent>(gameObject, OnRudderControlStarted);
        LevelManager.ShipEventBus.UnsubscribeFromSource<RudderControlEndedEvent>(gameObject, OnRudderControlEnded);
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

        IBlackboard blackboard = m_behaviourTree.graph.blackboard;
        blackboard.SetVariableValue("Velocity", newVelocity);

        if (m_controlledShip != null)
        {
            transform.position = m_rudderControlPoint.position;
            transform.rotation = m_rudderControlPoint.rotation;
        }
    }

    #endregion

    #region API

    public void Shoot()
    {
        m_playerModel.Shooter.Shoot();
    }

    #endregion

    #region Event Handlers

    // Handles the OnMovement event triggered by continuous input.
    // This method is invoked via SendMessage to process player movement input.
    [UsedImplicitly]
    public void OnMovement(InputValue value)
    {
        if (!__M_CanMove() || m_playerModel.IsDead) return;

        var rawInput = value.Get<Vector2>();

        if (m_controlledShip != null)
        {
            var deltaX = rawInput.x;
            gameObject.transform.position = m_rudderControlPoint.position;
            LevelManager.ShipEventBus.Raise(new ShipSteeredEvent(deltaX), m_controlledShip, gameObject);
        }
        else
        {
            Vector3 cameraForward = m_cameraTransform.forward;
            Vector3 cameraRight = m_cameraTransform.right;

            cameraForward.y = 0;
            cameraRight.y = 0;

            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 inputDirection = cameraRight * rawInput.x + cameraForward * rawInput.y;

            LevelManager.PlayerEventBus.Raise(new PlayerMoveEvent { Direction = inputDirection }, gameObject, null);
        }
    }

    // Handles the OnLook event triggered by continuous input.
    // This method is invoked via SendMessage to process player look input.
    [UsedImplicitly]
    public void OnLook(InputValue value)
    {
        var delta = value.Get<Vector2>();
        LevelManager.PlayerEventBus.Raise(new PlayerLookEvent { Delta = delta }, gameObject, null);
    }

    public void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (m_groundChecker.IsGrounded) m_playerModel.ResetJumpCount();
        if (__M_CanJump()) StartCoroutine(__M_Jump());
    }

    public void OnMeleePerformed(InputAction.CallbackContext context)
    {
        if (!__M_CanCast()) return;

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

    public void OnPickOrThrowPerformed(InputAction.CallbackContext context)
    {
        // if (m_pickedObject == null && __M_CanAct())
        // {
        //     Debug.Log("Picked");
        // }
        // else
        // {
        //     Debug.Log("Throw");
        // }

        if (!__M_CanAct()) return;
        m_interactor.TryInteract();
    }

    public void OnShieldStarted(InputAction.CallbackContext context)
    {
        if (!__M_CanAct()) return;
        m_playerModel.Speed *= 0.5f;
        m_behaviourTree.graph.blackboard.SetVariableValue("Shielding", true);
    }

    public void OnShieldCanceled(InputAction.CallbackContext context)
    {
        m_playerModel.Speed /= 0.5f;
        m_behaviourTree.graph.blackboard.SetVariableValue("Shielding", false);
    }

    public void OnShootStarted(InputAction.CallbackContext context)
    {
        if (!__M_CanAct()) return;

        ShootController shooter = m_playerModel.Shooter;
        IBlackboard blackboard = m_behaviourTree.graph.blackboard;

        blackboard.SetVariableValue(shooter.HasEnoughAmmo ? "Shooting" : "Reloading", true);
    }

    public void OnShootCanceled(InputAction.CallbackContext context)
    {
        m_behaviourTree.graph.blackboard.SetVariableValue("Shooting", false);
    }

    public void OnHealBoostPerformed(InputAction.CallbackContext context)
    {
        if (!__M_CanCast()) return;
        int modifierId = ModifierManager.Instance.GetModifierId(Casts.VITAL_SURGE);
        m_playerModel.TryCast(modifierId, m_playerModel);
    }

    private void OnPlayerReloaded(ref PlayerReloadedEvent eventData, GameObject target, GameObject source)
    {
        m_behaviourTree.graph.blackboard.SetVariableValue("Reloading", false);
    }

    private void OnRudderControlStarted(ref RudderControlStartedEvent eventData, GameObject target, GameObject source)
    {
        m_controlledShip = target;
        m_rudderControlPoint = eventData.RudderControlPoint;

        m_behaviourTree.graph.blackboard.SetVariableValue("Driving", true);
    }

    private void OnRudderControlEnded(ref RudderControlEndedEvent eventData, GameObject target, GameObject source)
    {
        m_controlledShip = null;
        m_rudderControlPoint = null;
        m_rigidbody.velocity = Vector3.zero;

        m_behaviourTree.graph.blackboard.SetVariableValue("Driving", false);
    }

    #endregion

    #region Internals

    private GameObject m_controlledShip;
    private Transform m_rudderControlPoint;

    private bool __M_CanJump()
    {
        return m_playerModel.JumpCount > 0 && __M_CanMove();
    }

    private bool __M_CanAct()
    {
        return m_playerModel.StatusEffectController.HasLegalAction(LegalAction.Act);
    }

    private bool __M_CanCast()
    {
        return m_playerModel.StatusEffectController.HasLegalAction(LegalAction.Cast);
    }

    private bool __M_CanMove()
    {
        return m_playerModel.StatusEffectController.HasLegalAction(LegalAction.Move);
    }

    private IEnumerator __M_Jump()
    {
        PlayerModel model = m_playerModel;
        model.HandleJump();

        // ResetState Y Velocity
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

    private IEnumerator __M_Melee()
    {
        m_rigidbody.velocity = Vector3.zero;
        m_behaviourTree.graph.blackboard.SetVariableValue("Melee Pressed", true);
        yield return new WaitForSeconds(0.25f);
        m_behaviourTree.graph.blackboard.SetVariableValue("Melee Pressed", false);
    }

    #endregion
}