using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDeviceManager : SingletonBehaviour<PlayerDeviceManager>
{
    #region Editor API

    [SerializeField] private InputActionAsset m_inputActionAsset;
    [SerializeField] private string m_joinActionName = "Join";
    [SerializeField] private string m_exitActionName = "Exit";

    #endregion

    #region Unity Events

    [UsedImplicitly]
    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < MAX_PLAYER_COUNT; i++) m_connectedDevices.Add(null);
    }

    [UsedImplicitly]
    private void OnEnable()
    {
        EnableHandlers();
    }

    [UsedImplicitly]
    private void OnDisable()
    {
        DisableHandler();
    }

    #endregion

    #region API

    public InputDevice GetDeviceByPlayerId(int playerId)
    {
        return playerId is < 0 or >= MAX_PLAYER_COUNT ? null : m_connectedDevices[playerId];
    }

    /// <summary>
    /// Used In Scene Loader @ Start Scene
    /// </summary>
    /// <exception cref="System.Exception"></exception>
    [UsedImplicitly]
    public void EnsureAllDevicesConnected()
    {
        int index = m_connectedDevices.IndexOf(null);
        if (index == -1) return;
        int connectedCount = m_connectedDevices.FindAll(device => device != null).Count;
        throw new System.Exception(
            $"Player count mismatch: Expected {m_connectedDevices.Count}, but only {connectedCount} devices are connected.");
    }

    public void EnableHandlers()
    {
        (InputAction joinAction, InputAction exitAction) = __M_FindActions();
        if (joinAction != null)
        {
            joinAction.performed += OnJoin;
            joinAction.Enable();
        }

        if (exitAction != null)
        {
            exitAction.performed += OnExit;
            exitAction.Enable();
        }
    }

    public void DisableHandler()
    {
        (InputAction joinAction, InputAction exitAction) = __M_FindActions();

        if (joinAction != null)
        {
            joinAction.performed -= OnJoin;
            joinAction.Disable();
        }

        if (exitAction != null)
        {
            exitAction.performed -= OnExit;
            exitAction.Disable();
        }
    }

    #endregion

    #region Event Handlers

    private void OnExit(InputAction.CallbackContext context)
    {
        InputDevice device = context.control.device;

        int index = m_connectedDevices.IndexOf(device);
        if (index == -1) return;

        m_connectedDevices[index] = null;

        LevelManager.PlayerEventBus.Raise(new PlayerExitedEvent(index, device), null, gameObject);

        Debug.Log($"{device.name} Left! (ID: {index})");
    }

    private void OnJoin(InputAction.CallbackContext context)
    {
        InputDevice device = context.control.device;

        if (m_connectedDevices.Contains(device)) return;

        int index = m_connectedDevices.IndexOf(null);
        if (index == -1)
        {
            Debug.Log("No available slots for new devices!");
            return;
        }

        m_connectedDevices[index] = device;

        LevelManager.PlayerEventBus.Raise(new PlayerJoinedEvent(index, device), null, gameObject);

        Debug.Log($"{device.name} Joined! (ID: {index})");
    }

    #endregion

    #region Internal

    private const int MAX_PLAYER_COUNT = 2;
    private List<InputDevice> m_connectedDevices = new(MAX_PLAYER_COUNT);

    private (InputAction joinAction, InputAction exitAction) __M_FindActions()
    {
        InputAction joinAction = m_inputActionAsset.FindAction(m_joinActionName);
        InputAction exitAction = m_inputActionAsset.FindAction(m_exitActionName);

        if (joinAction == null)
        {
            throw new System.Exception($"Join action '{m_joinActionName}' not found in InputActionAsset.");
        }

        if (exitAction == null)
        {
            throw new System.Exception($"Exit action '{m_exitActionName}' not found in InputActionAsset.");
        }

        return (joinAction, exitAction);
    }

    #endregion
}