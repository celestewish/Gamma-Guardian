using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerTetherAbility : MonoBehaviour
{
    [Header("Active Controlling")]
    bool isActive = false;
    public float abilityTime = 3f; //ability uptime
    float countdown; //keeps track of uptime
    public float cooldownTime; //base cooldown time
    float cooldown;
    public float abilityRange = 7f;
    public float forceVal = 45;
    public int maxTetherCount = 3;

        /* Variables for alt mode with repeated pulse effect; currently unused */
    //public enum AbilityMode { Force, PulseTethered /*, Pulse*/};
    //public AbilityMode mode;
    //private AbilityMode currentMode;
    //public int pulseCount = 3;
    //private float pulseDelay;
    //private float pulseTime=0f;

    [Header("Tether VFX")]
    List<Transform> connectPoints; //transforms for tethered enemies
    LineRenderer[] beams; //vfx for tethers
    public GameObject sprite; //prefab for animated "pulse" effect
    GameObject[] sprites; //
    public Material lineMat;

    void Start()
    {
        countdown = 0;
        cooldown = 0;
    }

    //initializes the tether vfx
    void MakeTetherLines()
    {
        beams = new LineRenderer[maxTetherCount];
        for (int i = 0; i < maxTetherCount; i++)
        {
            if (connectPoints[i] == null) break;

            connectPoints[i].gameObject.AddComponent<LineRenderer>();
            beams[i] = connectPoints[i].gameObject.GetComponent<LineRenderer>();
            beams[i].material = lineMat;
            beams[i].positionCount = 2;
            beams[i].startWidth = .25f;
            beams[i].endWidth = .1f;
            beams[i].startColor = new Color32(81, 202, 255, 255);
            beams[i].endColor = new Color32(25, 106, 220, 200);
        }

        sprites = new GameObject[maxTetherCount];
        for (int i = 0; i < maxTetherCount; i++)
        {
            sprites[i] = Instantiate(sprite);
        }
    }
    
    //reset after ability is over
    void EndForceAbility(bool canceled)
    {
        isActive = false;
        countdown = 0;
        cooldown = cooldownTime; //(canceled)? cooldownTime/2f: cooldownTime;

        int c = 0;
        string msg = "";

        foreach (Transform tf in connectPoints)
        {
            if(tf == null) break;

            Destroy(tf.gameObject.GetComponent<LineRenderer>());
            msg += (++c) + " ";
        }

        for(int i = 0; i<maxTetherCount; i++)
        {
            Destroy(sprites[i]);
        }
    }

    void FixedUpdate()
    {
        //Ability Handling
        if (isActive)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                EndForceAbility(true);
                Debug.Log("Ability was shut off.");
                return;
            }

            //Handles the physics for each tethered object and vfx for the tethers
            for(int i = 0; i<maxTetherCount; i++)
            {
                Transform tr = connectPoints[i];

                if (tr != null)
                {
                    //set endpoints for the tether
                    beams[i].SetPosition(0, transform.position);
                    beams[i].SetPosition(1, tr.position);

                    Vector3 vec = tr.position - transform.position; //direction of force
                    Rigidbody2D rb = tr.gameObject.GetComponent<Rigidbody2D>();

                    float distMult; //greater force for greater distance

                    if (vec.magnitude > abilityRange)
                        distMult = 2.5f;
                    else if (vec.magnitude > abilityRange * .75f)
                        distMult = 2f;
                    else if (vec.magnitude > abilityRange * .5f)
                        distMult = 1.5f;
                    else
                        distMult = 1f;
                    
                    rb.linearVelocity = new Vector2(0f, 0f);
                    rb.AddForce(-vec.normalized * forceVal * 5f * distMult, ForceMode2D.Force);

                    /*
                    switch (currentMode)
                    {
                        case AbilityMode.Force:
                            rb.linearVelocity = new Vector2(0f, 0f);
                            rb.AddForce(-vec.normalized * forceVal * 5f * distMult, ForceMode2D.Force);
                            break;
                        case AbilityMode.PulseTethered:
                            if (pulseTime >= pulseDelay)
                            {
                                rb.linearVelocity = new Vector2(0f, 0f);
                                float typeMult = (rb.gameObject.tag == "Cytokines") ? .2f : 1.25f;
                                rb.AddForce(-vec.normalized * forceVal * typeMult * distMult, ForceMode2D.Impulse);
                                pMsg += "\npulsed | " + pulseTime;
                            }
                            break;
                    }
                    //*/
                    //Debug.DrawRay(tr.position, -vec.normalized, Color.orange);

                    Vector2 aPos = new Vector2(transform.position.x, transform.position.y);
                    Vector2 bPos = new Vector2(tr.position.x, tr.position.y);
                    float outOftwo = (countdown % .25f) / (.25f);
                    Vector2 point2 = aPos * (1f - outOftwo) + bPos * outOftwo; //weighted midpoint, iterates over time
                    sprites[i].transform.position = point2;
                }
            }

            /*
            if(currentMode == AbilityMode.PulseTethered)
            {
                if (pulseTime >= pulseDelay)
                    pulseTime -= pulseDelay;

                if(pMsg != "Pulses:")
                    Debug.Log(pMsg);
            }
            //*/

            countdown -=Time.deltaTime;

            if (countdown <= 0)
            {
                EndForceAbility(false);
                Debug.Log("Ability is over");
            }
        }
        else if(cooldown > 0)
        {
            cooldown -= Time.deltaTime;
        }
        //End of Ability Handling

        //Ability Input
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isActive)
            {
                if(cooldown > 0)
                {
                    Debug.Log("Still cooling down");
                    return;
                }

                string msg = "Hit Ability button (E)";

                //currentMode = mode;

                /*
                if(currentMode == AbilityMode.PulseTethered)
                {
                    pulseDelay = abilityTime / pulseCount;
                    pulseTime = pulseDelay;
                }
                //*/

                msg += "\nMode: Force";

                if (GetClosest())
                {
                    countdown = abilityTime;
                    isActive = true;
                    msg += $"\nForce: {forceVal} | Range: {abilityRange} | Duration: {abilityTime}";
                    //if (currentMode == AbilityMode.PulseTethered)
                    //msg += $" | Pulse Count: {pulseCount} --> delay of {pulseDelay}\n";

                    Debug.Log(msg);
                }
                else
                {
                    Debug.Log("No Objects within range");
                }
            }
        }

        //Vector2 pos = new Vector2(transform.position.x, transform.position.y);
        //DebugDrawPolygon(pos, abilityRange, (int)20, Color.cyan);
    }

    /* Gets all cytokines and bacteria within {abilityRange} and keeps up to the closest {maxTetherCount} of them; 
     * Returns false if nothing is within range.
     */
    bool GetClosest()
    {
        GameObject[] cytokines = GameObject.FindGameObjectsWithTag("Cytokines");
        GameObject[] bacterias = GameObject.FindGameObjectsWithTag("Bacteria");

        List<GameObject> objList = new List<GameObject>();

        foreach(GameObject cyto in cytokines)
        {
            float distance = Vector3.Distance(transform.position, cyto.transform.position);
            if (distance <= abilityRange)
            {
                objList.Add(cyto);
            }
        }
        foreach(GameObject bact in bacterias)
        {
            float distance = Vector3.Distance(transform.position, bact.transform.position);
            if (distance <= abilityRange)
            {
                objList.Add(bact);
            }
        }

        if (objList.Count == 0)
        {
            return false;
        }

        connectPoints = new List<Transform>();
        List<float> temp = new List<float>();

        for (int i = 0; i < maxTetherCount; i++)
        {
            temp.Add(999f);
            connectPoints.Add(null);
        }

        foreach(GameObject obj in objList)
        {
            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist >= temp[maxTetherCount - 1]) 
                continue;

            int index = temp.BinarySearch(dist);
            if (index < 0) index = ~index;

            Debug.Log(obj.name + " at "+index);

            temp.Insert(index, dist); 
            temp.RemoveAt(maxTetherCount);
            connectPoints.Insert(index, obj.transform); 
            connectPoints.RemoveAt(maxTetherCount);
        }

        MakeTetherLines();

        return true;
    }

    //Helper method for debugging
    //https://docs.unity3d.com/ScriptReference/Mathf.Cos.html
    /*
    void DebugDrawPolygon(Vector2 center, float radius, int numSides, Color clr)
    {
        Vector2 startCorner = new Vector2(radius, 0) + center;

        Vector2 previousCorner = startCorner;

        for (int i = 1; i < numSides; i++)
        {
            float cornerAngle = 2f * Mathf.PI / (float)numSides * i;

            Vector2 currentCorner = new Vector2(Mathf.Cos(cornerAngle) * radius, Mathf.Sin(cornerAngle) * radius) + center;

            Debug.DrawLine(currentCorner, previousCorner, clr);

            previousCorner = currentCorner;
        }

        Debug.DrawLine(startCorner, previousCorner, clr);
    }
    //*/
}
