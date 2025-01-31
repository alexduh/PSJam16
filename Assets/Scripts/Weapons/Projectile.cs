using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Projectile : MonoBehaviour
{
    float damage;
    [SerializeField] float knockbackVelocity = 40f;
    [SerializeField] GameObject particleTrail;
    public GameObject explosionPrefab;
    [SerializeField] GameObject destroyedParticles;
    Rigidbody2D rb;
    private void Awake()
    {
        rb = this.GetComponent<Rigidbody2D>();
    }

    // This method will be called whenever a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.gameObject.SetActive(false);
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnEnable()
    {
        // Subscribe to the sceneLoaded event when the script is enabled
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from the sceneLoaded event when the script is disabled
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //Use This Method to activate projectile
    public void ActivateProjectile(Vector3 spawnLocation, Vector2 projectileVelocity, float projectileDamage, float projectileLifeSpan, bool friendly)
    {
        if (friendly)
            tag = "PlayerOwned";
        else
            tag = "EnemyOwned";
        transform.position = spawnLocation;
        gameObject.SetActive(true);
        //particleTrail.SetActive(false); TODO: ADD PROJECTILE TRAIL IF APPLICABLE
        rb.linearVelocity = projectileVelocity;
        StartCoroutine(DisableAfter(projectileLifeSpan));
        damage = projectileDamage;
        tag = friendly ? "PlayerOwned" : "EnemyOwned";
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        CollisionBehavior(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //CollisionBehavior(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        //CollisionBehavior(collision);
    }


    void CollisionBehavior(Collider2D other)
    {

        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && tag == "EnemyOwned")
        {
            Player.Instance.TakeDamage();
            DeactivateProjectile();
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Enemy") && tag == "PlayerOwned")
        {
            other.gameObject.GetComponent<Enemy>().TakeDamage(damage);
            DeactivateProjectile();
        }
        else if (other.tag == "WorldOwned")
        {
            DeactivateProjectile();
        }
        else if (other.tag == "Destructible")
        {
            other.gameObject.GetComponent<DestructibleObject>().Damage(damage);
            Debug.Log("Hit");
            DeactivateProjectile();
        }
     
    }

    public void DeactivateProjectile()
    {
        rb.linearVelocity = Vector3.zero;
        //particleTrail.SetActive(false); TODO: ADD PROJECTILE TRAIL IF APPLICABLE
        //Instantiate(destroyedParticles, transform.position, transform.rotation); TODO: ADD DESTRUCTION PARTICLES IF APPLICABLE
        this.gameObject.SetActive(false);
    }

    public void CreateTrail()
    {
        
    }

    public IEnumerator DisableAfter(float timeLimit)
    {
        yield return new WaitForSeconds(timeLimit);
        DeactivateProjectile();
    }

    void AddForce(Collider2D other)
    {
        if (other.TryGetComponent<Rigidbody2D>(out Rigidbody2D enemyrb))
        {
            Vector3 force = rb.linearVelocity;
            force = force.normalized;
            enemyrb.AddForce(force * knockbackVelocity);
        }
    }
}
