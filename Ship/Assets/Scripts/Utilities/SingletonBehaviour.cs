using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    public static T Instance { get; protected set; }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            string errorMessage = $"An instance of the singleton {typeof(T).Name} already exists. " +
                                  $"Current GameObject: {gameObject.name}, " +
                                  $"Existing Instance GameObject: {Instance.gameObject.name}";
            Destroy(this);
            throw new System.Exception(errorMessage);
        }
        else
        {
            Instance = (T)this;
        }
    }
}