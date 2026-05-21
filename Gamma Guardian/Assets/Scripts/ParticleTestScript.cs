using UnityEngine;
using System.Collections;

public class ParticleTestScript : MonoBehaviour
{
    public Material testMat;
    ParticleSystem ps;

    public Transform spot;

    public float duration;
    private float upTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(ps == null)
            gameObject.AddComponent<ParticleSystem>();
        ps = gameObject.GetComponent<ParticleSystem>();
        
        var main = ps.main;
        main.startLifetime = 1f;
        main.startSpeed = 30f;
        main.startColor = new Color(0f, .8588f, 1f, .7843f);
        main.maxParticles = 1;

        var em = ps.emission;
        em.rateOverTime = 3f;

        var sh = ps.shape;
        sh.shapeType = ParticleSystemShapeType.Sprite;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.x = 50f;
        vel.y = -15f;

        var re = GetComponent<ParticleSystemRenderer>();
        re.material = testMat;
    }

    void Update()
    {
        Vector3 dir = spot.position-transform.position;
        var vel = ps.velocityOverLifetime;
        vel.x = dir.x;
        vel.y = dir.y;
    }
}
