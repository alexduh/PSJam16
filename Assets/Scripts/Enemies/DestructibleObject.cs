using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    [SerializeField] float health = 6;
    [SerializeField] SpriteRenderer spriteRenderer;

    [SerializeField] Sprite damagedSprite;
    [SerializeField] Sprite destroyedSprite;
    [SerializeField] BoxCollider2D boxCollider;





    public void Damage(float damage)
    {
        health -= damage;

        if (health <= 0)
            SetDestroyed();
        else if (health <= 5)
            SetDamaged();
    }

    private void SetDestroyed()
    {
        spriteRenderer.sprite = destroyedSprite;
        boxCollider.enabled = false;

    }

    private void SetDamaged()
    {
        spriteRenderer.sprite = damagedSprite;
    }
}
