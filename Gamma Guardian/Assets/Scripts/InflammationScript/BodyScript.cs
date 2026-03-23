using UnityEngine;

public class BodyScript : MonoBehaviour
{
    public float localInflammation = 0f;
    public float localMax = 50f;
    public float localRate = 3f;
    private int localAttachedCells = 0;
    private float decayRate = 1f;
    private bool canInflame = true;

    void Update()
    {
        if (!canInflame)
        {
            localInflammation -= decayRate * 2f * Time.deltaTime;
            localInflammation = Mathf.Max(0f, localInflammation);
            return;
        }

        localInflammation += localAttachedCells * localRate * Time.deltaTime;
        if (localAttachedCells == 0) localInflammation -= decayRate * Time.deltaTime;
        localInflammation = Mathf.Clamp(localInflammation, 0f, localMax);

        if (localInflammation >= localMax) Debug.Log($"{name} Destroyed!");

        if (!GameManager.Instance.levelRunning)
        {
            DisableInflame();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ImmuneCell") && canInflame)
        {
            localAttachedCells++;
            InflammationManager.Instance.AddAttachedCell();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("ImmuneCell") && canInflame)
        {
            localAttachedCells--;
            InflammationManager.Instance.RemoveAttachedCell();
        }
    }

    public void DisableInflame()
    {
        canInflame = false;
        localAttachedCells = 0;
        InflammationManager.Instance.RemoveAttachedCell();
        InflammationManager.Instance.inflammationRatePerCell = 0f;
    }
}
