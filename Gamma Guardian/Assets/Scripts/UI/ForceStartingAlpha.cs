using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ForceStartingAlpha : MonoBehaviour
{
    [SerializeField] private float startingAlpha = 0.75f;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Color c = sr.color;
        c.a = startingAlpha;
        sr.color = c;
    }
}