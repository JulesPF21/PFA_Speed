using UnityEngine;
using UnityEngine.UI;

public class Collectible : MonoBehaviour
{
    [SerializeField] private Image objet;
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            objet.gameObject.SetActive(true);
            Destroy(this.gameObject);
        }

    }
}
