using UnityEngine;

public class TutorialMedicineScript : MonoBehaviour
{
    public TutorialCytokine tutorialCytokine;
    public TutorialBacteria tutorialBacteria;
    //public TutorialManager tutorialManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Action()
    {
        if (tutorialCytokine != null && tutorialCytokine.gameObject != null)
        {
            tutorialCytokine.Deactivate();
            return;
        }

        if (tutorialBacteria != null)
        {
            tutorialBacteria.Deactivate();
        }
    }

}
