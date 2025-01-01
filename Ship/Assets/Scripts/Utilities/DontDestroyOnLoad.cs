using UnityEngine;

public sealed class DontDestroyOnLoad : MonoBehaviour
{
    #region Unity Callbacks

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    #endregion
}