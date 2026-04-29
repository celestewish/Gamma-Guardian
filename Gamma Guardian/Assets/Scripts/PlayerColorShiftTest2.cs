using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerColorShiftTest2 : MonoBehaviour
{
    public GameObject obj;
    private Material mat;
    //float target; //target hue val

    public Slider body;
    public Slider gun;
    public Slider badge;

    private float[][] presets;

    void Awake()
    {
        presets = new float[][]
        {
            new float[]{4, 5}, new float[]{5, 6}
        };
    }

    void Start()
    {

        mat = obj.GetComponent<Image>().material;
        //target = mat.GetFloat("_Hue");
        //Debug.Log("hue shift: "+target);

        SliderChange("body");
        SliderChange("gun");
        SliderChange("badge");
    }

    public void SliderChange(string nm)
    {
        switch (nm)
        {
            case "gun":
                mat.SetFloat("_GunHue", (gun.value*5) );
                break;
            case "body":
                mat.SetFloat("_BodyHue", body.value * 5);
                break;
            case "badge":
                mat.SetFloat("_BadgeHue", badge.value * 5);
                break;
        }
    }

    public void SliderCheck(string nm)
    {
        switch (nm)
        {
            case "gun":
                Debug.Log("gun val = " + (gun.value * 5));
                break;
            case "body":
                Debug.Log("body val = " + (body.value * 5));
                break;
            case "badge":
                ;
                Debug.Log("badge val = " + (badge.value * 5));
                break;
        }
    }
}
