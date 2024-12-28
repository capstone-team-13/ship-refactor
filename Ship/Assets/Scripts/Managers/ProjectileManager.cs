using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectileManager : SingletonBehaviour<ProjectileManager>
{
    [SerializeField] private List<ProjectileController> m_projectilePrefabs;
    private Dictionary<ProjectileType, ObjectPool<ProjectileController>> m_projectilePools = new();

    [Header("Projectile Pool")] [SerializeField]
    private int m_poolCapacity = 20;

    [SerializeField] private int m_poolMaxSize = 100;

    protected override void Awake()
    {
        base.Awake();

        foreach (ProjectileController projectileController in m_projectilePrefabs)
        {
            ProjectileController prefab = projectileController;
            m_projectilePools[projectileController.ProjectileType] = new ObjectPool<ProjectileController>(
                createFunc: () => __M_Create(prefab),
                actionOnGet: obj => obj.gameObject.SetActive(true),
                actionOnRelease: obj => obj.gameObject.SetActive(false),
                actionOnDestroy: Destroy,
                defaultCapacity: m_poolCapacity,
                maxSize: m_poolMaxSize
            );
        }
    }

    /// <summary>
    /// Rents (gets) a bullet instance of the specified type from the object pool.
    /// </summary>
    /// <param name="projectileType">The type of the bullet to rent.</param>
    /// <returns>A GameObject representing the bullet instance, or null if the type is not found.</returns>
    public static ProjectileController Rent(ProjectileType projectileType)
    {
        if (Instance.m_projectilePools.TryGetValue(projectileType, out var pool))
        {
            return pool.Get();
        }

        Debug.LogError($"Bullet type {projectileType} not found in the prefab configuration!");
        return null;
    }

    /// <summary>
    /// Returns a projectile instance of the specified type to the object pool.
    /// </summary>
    /// <param name="projectile">The projectile to return to the pool.</param>
    public static void Return(ProjectileController projectile)
    {
        if (Instance.m_projectilePools.TryGetValue(projectile.ProjectileType, out var pool))
        {
            projectile.ResetState();
            pool.Release(projectile);
        }
        else
        {
            Debug.LogError($"Projectile type {projectile.ProjectileType} not found in the object pool configuration!");
            Destroy(projectile);
        }
    }

    private static ProjectileController __M_Create(ProjectileController prefab)
    {
        return Instantiate(prefab);
    }
}