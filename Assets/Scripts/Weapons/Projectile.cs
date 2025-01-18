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
    public void ActivateProjectile(Vector3 spawnLocation, Vector2 projectileVelocity, float projectileDamage, float projectileLifeSpan)
    {
        transform.position = spawnLocation;
        gameObject.SetActive(true);
        //particleTrail.SetActive(false); TODO: ADD PROJECTILE TRAIL IF APPLICABLE
        rb.linearVelocity = projectileVelocity;
        StartCoroutine(DisableAfter(projectileLifeSpan));
        damage = projectileDamage;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        CollisionBehavior(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        CollisionBehavior(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        CollisionBehavior(collision);
    }


    void CollisionBehavior(Collider2D other)
    {
        if (false)
        {
            //ToDo: Hit player/enemy/hitbox behavior
        }
        else
        {
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
