using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class Bloodstream : MonoBehaviour
{
    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        ps.Play();
    }
}
