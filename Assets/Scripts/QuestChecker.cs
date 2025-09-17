using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestChecker : MonoBehaviour
{
    [SerializeField] private QuestBoard quests;
    [SerializeField] private GameObject lamp;
    [SerializeField] private GameObject textFrame;
    [SerializeField] private GameObject textNotCompleted;
    [SerializeField] private GameObject textCompleted;
    [SerializeField] private string loadLevel;
    private Animator animator;
    private bool isCompleted;
    private bool inTrigger;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) && isCompleted && inTrigger)
        {
            SceneFade.Instance.FadeToScene(loadLevel);
        }
    }
    private void FixedUpdate()
    {
        if ( quests.killQuest >= 5 && quests.collectQuest >= 5)
        {
            isCompleted = true;
            animator = lamp.GetComponent<Animator>();
            animator.SetBool("Activated", isCompleted); 
        }
    }

    private void Start()
    {
        textFrame.SetActive(false);
        textNotCompleted.SetActive(false);
        textCompleted.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        inTrigger = true;
        if (other.CompareTag("Player"))
        {
            textFrame.SetActive(true);
            if (!isCompleted)
            {
                textNotCompleted.SetActive(true);
            }
            else
            {
                textCompleted.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        inTrigger = false;
        if (other.CompareTag("Player"))
        {
            textFrame.SetActive(false);
            textNotCompleted.SetActive(false);
            textCompleted.SetActive(false);
        }
    }

}
