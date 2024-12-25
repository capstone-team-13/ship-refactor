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
        int spawnPointId = 0;
        for (int id = 0; id < PLAYER_COUNT; id++)
        {
            PlayerInput player = Instantiate(m_playerPrefab, m_spawnPoints[spawnPointId].position,
                Quaternion.identity);
            if (player == null)
            {
                Debug.LogError($"Failed to instantiate player prefab for player #{id + 1}.");
                continue;
            }

            player.name = "Player #" + (id + 1);

            if (m_players.Length > id) m_players[id] = player;
            else Debug.LogError($"Index {id} out of bounds for m_players array.");

            m_players[id] = player;

            spawnPointId = ++spawnPointId % m_spawnPoints.Length;
        }

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

    #endregion
}