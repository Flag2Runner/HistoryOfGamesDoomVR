using UnityEngine;
using System.Collections; // Needed for Coroutines!

public class EnemyCharacter : CharacterBase
{
    [Header("Enemy Rewards")]
    public int scoreValue = 100;
    public bool countsAsKill = true;

    // --- ADD THESE VARIABLES ---
    private Renderer[] modelRenderers;
    private Color[] originalColors;

    protected override void Start()
    {
        base.Start();
        if (entityType == EntityType.Prop) countsAsKill = false;

        // 1. Find all the 3D meshes on this enemy/prop
        modelRenderers = GetComponentsInChildren<Renderer>();

        // 2. Save their original colors so we can change them back!
        originalColors = new Color[modelRenderers.Length];
        for (int i = 0; i < modelRenderers.Length; i++)
        {
            if (modelRenderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] = modelRenderers[i].material.color;
            }
        }
    }

    // --- OVERRIDE TAKE DAMAGE TO ADD THE FLASH ---
    public override void TakeDamage(int damageAmount)
    {
        base.TakeDamage(damageAmount); // Do the math

        // If it didn't just die, flash red!
        if (currentHealth > 0)
        {
            StartCoroutine(FlashRedRoutine());
        }
    }

    private IEnumerator FlashRedRoutine()
    {
        // Turn everything red
        for (int i = 0; i < modelRenderers.Length; i++)
        {
            if (modelRenderers[i] != null && modelRenderers[i].material.HasProperty("_Color"))
            {
                modelRenderers[i].material.color = Color.red;
            }
        }

        // Wait a tiny fraction of a second
        yield return new WaitForSeconds(0.1f);

        // Turn everything back to normal
        for (int i = 0; i < modelRenderers.Length; i++)
        {
            if (modelRenderers[i] != null && modelRenderers[i].material.HasProperty("_Color"))
            {
                modelRenderers[i].material.color = originalColors[i];
            }
        }
    }

    protected override void Die()
    {
        if (PlayerCharacter.Instance != null)
        {
            PlayerCharacter.Instance.currentScore += scoreValue;
            if (countsAsKill) PlayerCharacter.Instance.enemiesKilled++;
        }
        base.Die();
    }
}