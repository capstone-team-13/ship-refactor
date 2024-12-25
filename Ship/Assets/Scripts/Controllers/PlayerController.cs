using System.Collections;
using JetBrains.Annotations;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerModel))]
public class PlayerController : MonoBehaviour
{
    private static class PlayerActions
    {
        public const string MOVEMENT = "Movement";
        public const string LOOK = "Look";
        public const string MELEE = "Melee";
        public const string JUMP = "Jump";
        public const string PICK_OR_THROW = "Pick/Throw";
        public const string SHIELD = "Shield";
        public const string DIVINE_BOON = "Divine Boon";
        public const string SHOOT = "Shoot";
        public const string ENERGY_BOOST = "Energy Boost";
        public const string ENERGY_TRANSFER = "Energy Transfer";
    }

    #region Editor API

    [Header("Player Components")] [SerializeField]
    private PlayerModel m_playerModel;

    [SerializeField] private PlayerInput m_playerInput;
    [SerializeField] private Rigidbody m_rigidbody;

    [Header("Camera and Transform")] [SerializeField]
    private Transform m_cameraTransform;

    [Header("Ground Check")] [SerializeField]
    private GroundChecker m_groundChecker;

    #endregion

    #region Unity Callbacks

    private void OnEnable()
    {
        if (m_playerInput != null)
        {
            m_playerInput.actions[PlayerActions.JUMP].performed += OnJumpPerformed;
        }
    }

    [UsedImplicitly]
    private void OnDisable()
    {
        if (m_playerInput != null)
        {
            m_playerInput.actions[PlayerActions.JUMP].performed -= OnJumpPerformed;
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
    private void FixedUpdate()
    {
        // Handles player movement directly in FixedUpdate for consistent physics updates.
        // This bypasses the Behavior Tree as movement needs to be updated regularly regardless of AI logic.
        PlayerModel model = m_playerModel;
        if (model.IsDead) return;
        Vector3 newVelocity = model.Direction * model.Speed;
        newVelocity.y = m_rigidbody.velocity.y;
        m_rigidbody.velocity = newVelocity;
    }

    #endregion

    #region Event Handlers

    // Handles the OnMovement event triggered by continuous input.
    // This method is invoked via SendMessage to process player movement input.
    [UsedImplicitly]
    public void OnMovement(InputValue value)
    {
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
        m_rigidbody.velocity = velocity;
        m_rigidbody.AddForce(model.JumpForce, ForceMode.Impulse);

        LevelManager.PlayerEventBus.Raise(new PlayerJumpEvent(), gameObject, null);

        yield return new WaitForEndOfFrame();

        --model.JumpCount;
        model.JumpPressed = false;
    }

    #endregion
}