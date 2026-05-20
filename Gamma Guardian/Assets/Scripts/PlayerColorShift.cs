using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class PlayerColorShift : MonoBehaviour
{
    public GameObject obj;
    private Material mat;
    float target; //target hue val

    float diff; //hue val difference
    float diffTemp; 
    float dir; //change direction

    float timeDelta;

    void Start()
    {
        mat = obj.GetComponent<Image>().material;

        if (PlayerPrefs.HasKey("GuardianHue"))
        {
            target = PlayerPrefs.GetFloat("GuardianHue");
            mat.SetFloat("_Hue", target);
        }
        else
        {
            target = 0f;
            mat.SetFloat("_Hue", target);
            PlayerPrefs.SetFloat("GuardianHue", target);
        }

        Debug.Log("hue shift: "+target);

        diff = dir = 0;

        timeDelta = GameObject.Find("Game Manager").GetComponent<GameManager>().timeDelta;
    }

    void Update()
    {
        if(mat.GetFloat("_Hue") != target)
        {
            //controls change speed
            if(diffTemp > .3f * diff)
            {
                mat.SetFloat("_Hue", mat.GetFloat("_Hue") + 60 * dir * timeDelta); //* timeDelta
                diffTemp -= Mathf.Abs(dir);
            }
            else if(diffTemp > .1f * diff)
            {
                mat.SetFloat("_Hue", mat.GetFloat("_Hue") + 30 * dir * timeDelta);
                diffTemp -= .5f * Mathf.Abs(dir);
            }

            float temp = (mat.GetFloat("_Hue") + 360) % 360;
            if (Mathf.Abs(temp - target) <= 5)
            {
                Debug.Log("diff: " + diffTemp);
                mat.SetFloat("_Hue", (int)target);
                PlayerPrefs.SetFloat("GuardianHue", mat.GetFloat("_Hue"));
            }

            //resets hue val to be with 0-360
            //if ( (mat.GetFloat("_Hue")+360) % 360 == target)
            //{
                
            //    target = (mat.GetFloat("_Hue") + 360) % 360;
            //    mat.SetFloat("_Hue", (int)target);
            //}
        }
    }

    public void SetShift(int newShiftVal)
    {
        Debug.Log($"old: {target} | new: {newShiftVal}");
        target = newShiftVal;
        
        diff = target - mat.GetFloat("_Hue");
        dir = Mathf.Sign(diff);
        diff = Mathf.Abs(diff);

        if ( (360 - diff) < diff)
        {
            diff = 360 - diff;
            dir = -dir; //swaps to other direction if there's less distance
        }

        Debug.Log($" difference: {diff} | direction: {dir}");
        diffTemp = diff;
    }

    public void PlaySound()
    {
        GetComponent<AudioSource>().Play();
    }
}
