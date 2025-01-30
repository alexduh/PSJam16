using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    [SerializeField] float health = 10;

    public void Damage(float damage)
    {
        health -= damage;
        if (health <= 0)
            Destruction();
    }

    private void Destruction()
    {
        this.gameObject.SetActive(false);
    }
}
