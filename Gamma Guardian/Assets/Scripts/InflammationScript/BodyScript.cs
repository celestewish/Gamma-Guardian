using UnityEngine;

public class BodyScript : MonoBehaviour
{
    public float localInflammation = 0f;
    public float localMax = 50f;
    public float localRate = 3f;
    private int localAttachedCells = 0;
    private float decayRate = 1f;

    void Update()
    {
        localInflammation += localAttachedCells * localRate * Time.deltaTime;
        if (localAttachedCells == 0) localInflammation -= decayRate * Time.deltaTime;
        localInflammation = Mathf.Clamp(localInflammation, 0f, localMax);

        if (localInflammation >= localMax) Debug.Log($"{name} Destroyed!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ImmuneCell"))
        {
            localAttachedCells++;
            InflammationManager.Instance.AddAttachedCell();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("ImmuneCell"))
        {
            localAttachedCells--;
            InflammationManager.Instance.RemoveAttachedCell();
        }
    }
}
