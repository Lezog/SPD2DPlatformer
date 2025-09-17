using Unity.VisualScripting;
using UnityEngine;

public class GemPickup : MonoBehaviour
{
    [SerializeField] private int gemAmount = 1;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatHeight = 0.2f;
    [SerializeField] private GameObject collectionParticles;
    private QuestBoard quests;
    private Vector3 startPos;

    private void Start()
    {
        quests = GameObject.FindGameObjectWithTag("QuestBoard").GetComponent<QuestBoard>();
        startPos = transform.position;
    }

    private void Update()
    { 
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();
            if (player != null)
            {
                Instantiate(collectionParticles, transform.position, Quaternion.identity);
                quests.addCollect(gemAmount);
                player.GiveGem(gemAmount);
                Destroy(gameObject);
            }
        }
    }

}
