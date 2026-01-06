using UnityEngine;

/// <summary>
/// Interface for any object that can receive damage.
/// Implemented by PlayerController, EnemyController, and destructible objects.
/// 
/// Reference: KB Section V.A - Combat System
/// </summary>
public interface IDamageable
{
    void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
    void TakeDamage(float amount);
    bool IsAlive { get; }
    float CurrentHealth { get; }
    float MaxHealth { get; }
}
