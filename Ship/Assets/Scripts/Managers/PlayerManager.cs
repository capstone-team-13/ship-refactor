using EE.Interactions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : SingletonBehaviour<PlayerManager>
{
    #region Editor API

    [SerializeField] private PlayerInput m_playerPrefab;

    [SerializeField] private Transform[] m_spawnPoints;

    #endregion

    #region Unity Callbacks

    [UsedImplicitly]
    private void OnValidate()
    {
        if (m_playerPrefab == null)
        {
            string errorMessage = $"{nameof(m_playerPrefab)} is not assigned in the Inspector. " +
                                  "Please assign a valid PlayerInput prefab to proceed.";
            Debug.LogError(errorMessage);
            throw new UnityException(errorMessage);
        }

        if (m_spawnPoints == null || m_spawnPoints.Length == 0)
        {
            string errorMessage = "Spawn points are not set or are empty. " +
                                  "Please assign at least one valid spawn point in the Inspector to proceed.";
            Debug.LogError(errorMessage);
            throw new UnityException(errorMessage);
        }
    }

    [UsedImplicitly]
    private void Start()
    {
        __M_SpawnPlayers();

        if (PlayerDeviceManager.Instance != null)
        {
            PlayerDeviceManager.Instance.DisableHandler();
        }
        else
        {
            string errorMessage = $"{nameof(PlayerDeviceManager)} instance is null. " +
                                  "Please ensure the game is started from the Start Scene to ensure all controllers are registered correctly.";
            throw new UnityException(errorMessage);
        }
    }

    #endregion

    #region Internal

    private const uint PLAYER_COUNT = 2;
    private PlayerInput[] m_players = new PlayerInput[PLAYER_COUNT];

    private void __M_SpawnPlayers()
    {
        int spawnPointId = 0;
        for (int id = 0; id < PLAYER_COUNT; id++)
        {
            PlayerInput player = __M_CreatePlayer(id, spawnPointId);
            if (player == null) continue;

            __M_AssignPlayerName(player, id);
            __M_AssignToPlayerArray(player, id);

            spawnPointId = __M_GetNextSpawnPointId(spawnPointId);
        }
    }

    private PlayerInput __M_CreatePlayer(int id, int spawnPointId)
    {
        PlayerInput player = Instantiate(m_playerPrefab, m_spawnPoints[spawnPointId].position, Quaternion.identity);
        if (player == null)
        {
            Debug.LogError($"Failed to instantiate player prefab for player #{id + 1}.");
        }

        return player;
    }

    private static void __M_AssignPlayerName(PlayerInput player, int id)
    {
        player.name = "Player #" + (id + 1);
    }

    private void __M_AssignToPlayerArray(PlayerInput player, int id)
    {
        if (m_players.Length > id)
        {
            m_players[id] = player;
        }
        else
        {
            Debug.LogError($"Index {id} out of bounds for m_players array.");
        }
    }

    private int __M_GetNextSpawnPointId(int currentSpawnPointId)
    {
        return (currentSpawnPointId + 1) % m_spawnPoints.Length;
    }

    #endregion
}