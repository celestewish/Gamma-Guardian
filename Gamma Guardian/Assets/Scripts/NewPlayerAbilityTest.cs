using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NewPlayerAbilityTest : MonoBehaviour
{
    bool isActive = false;
    public float abilityTime = 5f;
    float countdown;
    public float abilityRange = 10f;

    public float forceVal;

    public AbilityMode mode;
    private AbilityMode currentMode;
    public int pulseCount = 3;
    private float pulseDelay;
    private float pulseTime=0f;

    public enum AbilityMode {Force, PulseTethered /*, Pulse*/};

    public Material lineMat;

    Transform[] connectPoints;
    LineRenderer[] beams;
    public GameObject sprite;
    GameObject[] sprites;

    void Start()
    {
        countdown = 0;
    }

    void MakeTetherLines()
    {
        beams = new LineRenderer[3];
        for (int i = 0; i < 3; i++)
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

        sprites = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            sprites[i] = Instantiate(sprite);
        }
    }

    void EndForceAbility()
    {
        isActive = false;
        countdown = 0;

        int c = 0;
        string msg = "";

        foreach (Transform tf in connectPoints)
        {
            if(tf == null) break;

            Destroy(tf.gameObject.GetComponent<LineRenderer>());
            msg += (++c) + " ";
        }

        for(int i = 0; i<3; i++)
        {
            Destroy(sprites[i]);
        }

        Debug.Log("first line: " + ((beams[0] == null) ? "Null" : "Not Null"));
    }

    void FixedUpdate()
    {
        //Force Handling
        if (isActive)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                EndForceAbility();
                Debug.Log("Ability was shut off.");
                return;
            }

            string pMsg = "Pulses:";
            if(currentMode == AbilityMode.PulseTethered)
            {
                pulseTime += Time.deltaTime;
            }

            for(int i = 0; i<3; i++)
            {
                Transform tr = connectPoints[i];

                if (tr != null)
                {
                    beams[i].SetPosition(0, transform.position);
                    beams[i].SetPosition(1, tr.position);

                    Vector3 vec = tr.position - transform.position;
                    Debug.DrawRay(transform.position, vec, Color.purple);
                    //Debug.Log("magn: " + vec.magnitude);
                    Rigidbody2D rb = tr.gameObject.GetComponent<Rigidbody2D>();

                    float distMult;

                    if (vec.magnitude > abilityRange)
                        distMult = 2.5f;
                    else if (vec.magnitude > abilityRange * .5f)
                        distMult = 1.5f;
                    else
                        distMult = .5f;

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
                    Debug.DrawRay(tr.position, -vec.normalized, Color.orange);

                    Vector2 aPos = new Vector2(transform.position.x, transform.position.y);
                    Vector2 bPos = new Vector2(tr.position.x, tr.position.y);
                    float outOfOne = (countdown % .5f) / (.5f);
                    Vector2 point = aPos * (1f - outOfOne) + bPos * outOfOne;
                    DebugDrawPolygon(point, .6f, 8, Color.magenta);

                    //Vector3 loc = spot.position * temp + transform.position * (1 - temp);
                    float outOftwo = (countdown % .25f) / (.25f);
                    Vector2 point2 = aPos * (1f - outOftwo) + bPos * outOftwo;
                    sprites[i].transform.position = point2;
                }
            }

            if(currentMode == AbilityMode.PulseTethered)
            {

                //Debug.Log("pulseTime: " + pulseTime);

                if (pulseTime >= pulseDelay)
                    pulseTime -= pulseDelay;

                if(pMsg != "Pulses:")
                    Debug.Log(pMsg);
            }

            countdown -=Time.deltaTime;

            if (countdown <= 0)
            {
                EndForceAbility();
                Debug.Log("Ability countdown is over");
            }
        }
        //End of Force Handling

        //Ability Input
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isActive)
            {
                string msg = "Hit Ability button (E)";

                currentMode = mode;
                if(currentMode == AbilityMode.PulseTethered)
                {
                    pulseDelay = abilityTime / pulseCount;
                    pulseTime = pulseDelay;
                }

                msg += "\nMode: " + currentMode;

                if (GetClosest())
                {
                    countdown = abilityTime;
                    isActive = true;
                    msg += $"\nRange: {abilityRange} | Duration: {abilityTime}";
                    if (currentMode == AbilityMode.PulseTethered)
                        msg += $" | Pulse Count: {pulseCount} --> delay of {pulseDelay}\n";

                    Debug.Log(msg);
                }
                else
                {
                    Debug.Log("No Objects within range");
                }
            }
        }

        Vector2 pos = new Vector2(transform.position.x, transform.position.y);
        //DebugDrawPolygon(pos, abilityRange, (int)20, Color.cyan);
    }

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

        if(objList.Count == 0)
        {
            return false;
        }

        connectPoints = new Transform[3];
        float[] temp = new float[3];
        for (int i = 0; i < 3; i++)
            temp[i] = 999f;

        foreach(GameObject obj in objList)
        {
            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist <= temp[0])
            {
                temp[2] = temp[1]; connectPoints[2] = connectPoints[1];
                temp[1] = temp[0]; connectPoints[1] = connectPoints[0];
                temp[0] = dist; connectPoints[0] = obj.transform;
            }
            else if (dist <= temp[1])
            {
                temp[2] = temp[1]; connectPoints[2] = connectPoints[1];
                temp[1] = dist; connectPoints[1] = obj.transform;
            }
            else if (dist < temp[2])
            {
                temp[2] = dist; connectPoints[2] = obj.transform;
            }
        }

        MakeTetherLines();

        return true;
    }

    //https://docs.unity3d.com/ScriptReference/Mathf.Cos.html
    void DebugDrawPolygon(Vector2 center, float radius, int numSides, Color clr)
    {
        // The corner that is used to start the polygon (parallel to the X axis).
        Vector2 startCorner = new Vector2(radius, 0) + center;

        // The "previous" corner point, initialised to the starting corner.
        Vector2 previousCorner = startCorner;

        // For each corner after the starting corner...
        for (int i = 1; i < numSides; i++)
        {
            // Calculate the angle of the corner in radians.
            float cornerAngle = 2f * Mathf.PI / (float)numSides * i;

            // Get the X and Y coordinates of the corner point.
            Vector2 currentCorner = new Vector2(Mathf.Cos(cornerAngle) * radius, Mathf.Sin(cornerAngle) * radius) + center;

            // Draw a side of the polygon by connecting the current corner to the previous one.
            Debug.DrawLine(currentCorner, previousCorner, clr);

            // Having used the current corner, it now becomes the previous corner.
            previousCorner = currentCorner;
        }

        // Draw the final side by connecting the last corner to the starting corner.
        Debug.DrawLine(startCorner, previousCorner, clr);
    }
}
