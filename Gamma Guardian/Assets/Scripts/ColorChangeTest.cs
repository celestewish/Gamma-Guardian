using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ColorChangeTest : MonoBehaviour
{
    private Material mat;
    float target;

    float diff;
    float dir;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mat = GetComponent<Image>().material;
        target = mat.GetFloat("_Hue");
        Debug.Log("hue shift: "+target);

        diff = dir = 0;
    }

    void Update()
    {
        if(mat.GetFloat("_Hue") != target)
        {
            if(diff > 50)
            {
                mat.SetFloat("_Hue", mat.GetFloat("_Hue") + dir);
            }
            else if(diff > 20)
            {
                mat.SetFloat("_Hue", mat.GetFloat("_Hue") + .5f * dir);
            }
            else
            {
                mat.SetFloat("_Hue", mat.GetFloat("_Hue") + .25f * dir);
            }

            if( (mat.GetFloat("_Hue")+360) % 360 == target)
            {
                target = (mat.GetFloat("_Hue") + 360) % 360;
                mat.SetFloat("_Hue", target);
            }
        }
    }

    public void SetShift(int shift)
    {
        Debug.Log($"old: {target} | new: {shift}");
        target = shift;

        diff = target - mat.GetFloat("_Hue");
        dir = Mathf.Sign(diff);
        diff = Mathf.Abs(diff);

        if ( (360 - diff) < diff)
        {
            diff = 360 - diff;
            dir = -dir;
        }

        Debug.Log($" difference: {diff} ");
        //Debug.Log($" difference: {diff} | direction: {dir}");
    }
}
