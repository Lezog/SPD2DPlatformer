using UnityEngine;

public class QuestBoard : MonoBehaviour
{
    [SerializeField] private GameObject text;
    public int killQuest;
    public int collectQuest;

    private void Start()
    {
        text.SetActive(false);
    }

    private void OnTriggerEnter2D (Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            text.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            text.SetActive(false);
        }
    }

    public void addKill()
    {
        killQuest++;
    }

    public void addCollect(int amount)
    {
        collectQuest = collectQuest + amount;
    }
}
