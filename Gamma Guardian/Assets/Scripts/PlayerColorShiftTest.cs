using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerColorShiftTest : MonoBehaviour
{
    public GameObject obj;
    private Material mat;
    //float target; //target hue val

    public Slider slid;

    //float diff; //hue val difference
    //float diffTemp; 
    //float dir; //change direction

    void Start()
    {
        mat = obj.GetComponent<Image>().material;
        //target = mat.GetFloat("_Hue");
        //Debug.Log("hue shift: "+target);

        //diff = dir = 0;

        mat.SetFloat("_Hue", slid.value);
    }

    void Update()
    {
        //if(mat.GetFloat("_Hue") != target)
        //{
        //    //controls change speed
        //    if(diffTemp > .3f * diff)
        //    {
        //        mat.SetFloat("_Hue", mat.GetFloat("_Hue") + dir);
        //        diffTemp -= Mathf.Abs(dir);
        //    }
        //    else //if(diffTemp > .1f * diff)
        //    {
        //        mat.SetFloat("_Hue", mat.GetFloat("_Hue") + .5f * dir);
        //        diffTemp -= .5f * Mathf.Abs(dir);
        //    }

        //    //resets hue val to be with 0-360
        //    if( (mat.GetFloat("_Hue")+360) % 360 == target)
        //    {
        //        Debug.Log("diff: " + diffTemp);
        //        target = (mat.GetFloat("_Hue") + 360) % 360;
        //        mat.SetFloat("_Hue", target);
        //    }
        //}
    }

    public void ColorSlider()
    {
        Debug.Log(slid.value);
        mat.SetFloat("_Hue", slid.value);
        //Debug.Log($"old: {target} | new: {newShiftVal}");
        //target = newShiftVal;
        
        //diff = target - mat.GetFloat("_Hue");
        //dir = Mathf.Sign(diff);
        //diff = Mathf.Abs(diff);

        //if ( (360 - diff) < diff)
        //{
        //    diff = 360 - diff;
        //    dir = -dir; //swaps to other direction if there's less distance
        //}

        //Debug.Log($" difference: {diff} | direction: {dir}");
        //diffTemp = diff;
    }
}
