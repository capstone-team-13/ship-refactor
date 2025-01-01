using UnityEngine;

public class ProjectileModel : MonoBehaviour
{
    public ProjectileType Type;

    public float Damage;

    public float Lifetime = 3f;
    public float TimeElapsed { get; set; }
}