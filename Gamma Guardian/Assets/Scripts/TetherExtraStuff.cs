using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TetherExtraStuff : MonoBehaviour
{
    public SpriteRenderer circle;

    Color originalColor;
    public Color activeColor;
    Color cooldownColor;
    Color cooldownFlashColor;
    public Color failedColor;

    bool inSequence=false;
    [HideInInspector]
    public float cldwn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalColor = circle.color;
        
        float H, S, V;
        Color.RGBToHSV(originalColor, out H, out S, out V);
        S *= .3f;
        V *= .8f;
        cooldownColor = Color.HSVToRGB(H, S, V);
        cooldownColor.a = originalColor.a * .8f;

        cooldownFlashColor = cooldownColor*.5f + originalColor * .5f;
        cooldownFlashColor.a *= 1.2f;

        //SetActive(true);
    }

    /*
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(1f);
        SetActive(true);
        yield return new WaitForSeconds(1f);
        SetActive(false);
    }
    //*/

    IEnumerator ActiveTransition(Color start, Color end, float dur)
    {
        float clock = dur;

        while (clock > 0f)
        {
            float prog = (clock / dur);
            Color temp = prog * start + (1 - prog) * end;
            circle.color = temp;
            clock -= Time.deltaTime;
            yield return null;
        }
    }

    public void SetActive(bool active)
    {
        if (active)
            StartCoroutine(ActiveTransition(originalColor, activeColor, .2f));
        else
        {
            StartCoroutine(CooldownSequence(cldwn));
        }
    }

    IEnumerator CooldownSequence(float cooldown)
    {
        float ends = .5f;
        cooldown -= 2f * ends;

        float half = cooldown / 2f;
        float temp = 0f;

        yield return StartCoroutine(ActiveTransition(activeColor, cooldownColor, ends));
        cooldown -= ends;

        while (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
            if (cooldown <= half)
            {
                temp -= Time.deltaTime;
                if (temp <= 0)
                {
                    yield return StartCoroutine(ActiveTransition(cooldownColor, cooldownFlashColor, .2f));
                    yield return new WaitForSeconds(.2f);
                    yield return StartCoroutine(ActiveTransition(cooldownFlashColor, cooldownColor, .2f));

                    cooldown -= .6f;
                    temp += .4f;
                    //yield return null;
                    //continue;
                }
            }
            yield return null;
        }

        yield return StartCoroutine(ActiveTransition(circle.color, originalColor, ends));
        //Debug.Log($"(II) before: {last} -> after: {stopwatch} | {stopwatch-last} ");

        inSequence = false;
    }

}
