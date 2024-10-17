using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : RecycleObject
{
    Player player;
    public float maxHealth = 100.0f;

    private float health = 100.0f;

    public float Health
    {
        get => health;
        set
        {
            if (health != value)
            {
                health = value;
                if (health < 0)
                {
                    health = Mathf.Clamp(health, 0, maxHealth);
                    Die();
                }
            }
        }
    }

    protected override void OnReset()
    {
        player = GameManager.Instance.Player;

        player.onHit += OnHit;
    }

    void Die()
    {
        player.onHit -= OnHit;
        Debug.Log($"{gameObject.name} Die");
    }

    void OnHit(RaycastHit hitInfo)
    {
        if (hitInfo.transform == transform)
        {
            float randomDamage = Random.Range(20, 30);
            Health -= randomDamage;
            Debug.Log($"{gameObject.name}, {randomDamage}, {Health}");
        }
    }
}
