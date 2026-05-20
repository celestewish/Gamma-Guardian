using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NewPlayerAbilityTest : MonoBehaviour
{
    Transform[] connectPoints;
    bool isActive = false;
    public float abilityTime = 5f;
    float countdown;
    public float abilityRange = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        countdown = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            foreach(Transform tr in connectPoints)
            {
                if(tr != null)
                {
                    Debug.DrawRay(transform.position, tr.position - transform.position, Color.blue);

                    Vector2 aPos = new Vector2(transform.position.x, transform.position.y);
                    Vector2 bPos = new Vector2(tr.position.x, tr.position.y);
                    float outOfOne = (countdown % .5f)/(.5f);
                    Vector2 point = aPos*(outOfOne) + bPos*(1-outOfOne);
                    
                    DebugDrawPolygon(point, 1, 6, Color.cyan);
                }
            }

            countdown-=Time.deltaTime;

            if (countdown <= 0)
            {
                isActive = false;
                Debug.Log("Ability countdown is over");
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isActive)
            {
                Debug.Log("Hit E");
                if (GetClosest())
                {
                    countdown = abilityTime;
                    isActive = true;
                }
                else
                {
                    Debug.Log("No Objects within range");
                }
            }
        }

        Vector2 pos = new Vector2(transform.position.x, transform.position.y);
        DebugDrawPolygon(pos, abilityRange, (int)20, Color.yellow);
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
