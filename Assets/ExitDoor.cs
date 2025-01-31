using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    public bool playerEscaped = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
           playerEscaped = true;
        }
    }
}
