using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerTetherAbility : MonoBehaviour
{
    [Header("Active Controlling")]
    bool isActive = false;
    public float abilityTime = 3f; //ability uptime
    [HideInInspector] public float countdown; //keeps track of uptime
    public float cooldownTime = 3f; //base cooldown time
    [HideInInspector] public float cooldown;
    public float abilityRange = 7f;
    public float forceVal = 50f;
    public int maxTetherCount = 3;
    public List<string> includeTags;
    
    List<GameObject> objList;

    /* Variables for alt mode with repeated pulse effect; currently unused */
    //public enum AbilityMode { Force, PulseTethered /*, Pulse*/};
    //public AbilityMode mode;
    //private AbilityMode currentMode;
    //public int pulseCount = 3;
    //private float pulseDelay;
    //private float pulseTime=0f;

    [Header("Tether VFX")]
    public GameObject sprite; //prefab for animated "pulse" effect
    public Material lineMat;

    [HideInInspector] public List<Transform> connectPoints; //transforms for tethered enemies
    [HideInInspector] public LineRenderer[] beams; //vfx for tethers
    [HideInInspector] public GameObject[] sprites; //circle objects that animate along the tethers

    void Start()
    {
        countdown = 0;
        cooldown = 0;

        gameObject.SendMessage("SetCircle", abilityRange);
        gameObject.SendMessage("SetCooldown", cooldownTime);
        objList = new List<GameObject>();
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

        gameObject.SendMessage("SetActive", false);
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
                        distMult = 2.25f;
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
                    gameObject.SendMessage("SetActive", true);
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
    }

    /* Gets up to the closest {maxTetherCount} of bacteria and cytokines objects and triggers the tether making; 
     * Returns true if there are enemies within range; if not, prematurely returns false 
     */
    bool GetClosest()
    {
        Debug.Log("Trying GetClosest | count in range: "+objList.Count);
        if (objList.Count == 0)
        {
            return false;
        }

        connectPoints = new List<Transform>();
        List<float> temp = new List<float>(); //keeps track of distances to be compared to

        for (int i = 0; i < maxTetherCount; i++)
        {
            temp.Add(999f);
            connectPoints.Add(null);
        }

        foreach(GameObject obj in objList)
        {
            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist >= temp[maxTetherCount - 1]) //if dist is more than the greatest currently stored distance
                continue;

            int index = temp.BinarySearch(dist);
            if (index < 0) index = ~index;

            //inserts the new object appropriately and cuts off the end
            temp.Insert(index, dist); 
            temp.RemoveAt(maxTetherCount);
            connectPoints.Insert(index, obj.transform); 
            connectPoints.RemoveAt(maxTetherCount);
        }

        MakeTetherLines();

        return true;
    }

    /* Maintains the list of enemies within the range*/
    void OnTriggerEnter2D(Collider2D coll)
    {
        GameObject go = coll.gameObject;
        if (!includeTags.Contains(go.tag)) return;

        objList.Add(go);
        //Debug.Log($"{go.name} entered | count = {objList.Count}");
        if (objList.Count == 1)
            this.gameObject.SendMessage("SetHasInRange", true);
    }
    void OnTriggerExit2D(Collider2D coll)
    {
        GameObject go = coll.gameObject;
        if (!includeTags.Contains(go.tag)) return;

        objList.Remove(go);
        //Debug.Log($"{go.name} exited | count = {objList.Count}");
        if (objList.Count == 0)
            this.gameObject.SendMessage("SetHasInRange", false);
    }
}
