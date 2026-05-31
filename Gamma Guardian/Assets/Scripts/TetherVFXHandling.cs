using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TetherVFXHandling : MonoBehaviour
{
    SpriteRenderer circle;

    Color originalColor;
    Color defaultColor;
    public Color activeColor;
    Color cooldownColor;
    Color cooldownFlashColor;

    bool inSequence;
    float cldwn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        circle = GetComponent<SpriteRenderer>();

        originalColor = circle.color;
        
        float H, S, V;
        Color.RGBToHSV(originalColor, out H, out S, out V);
        S *= .7f;
        V *= .9f;
        defaultColor = Color.HSVToRGB(H, S, V);
        defaultColor.a = originalColor.a * .8f;

        Color.RGBToHSV(originalColor, out H, out S, out V);
        H *= .8f;
        S *= .15f;
        V *= .6f;
        cooldownColor = Color.HSVToRGB(H, S, V);
        cooldownColor.a = originalColor.a * .7f;

        cooldownFlashColor = cooldownColor*.6f + originalColor * .4f;
        cooldownFlashColor.a *= 1.2f;

        circle.color = defaultColor;
        
        inSequence = false;
    }

    public void SetCircle(float rad)
    {
        Vector3 newScale = new Vector3(rad * 4, rad * 4, 1f);
        gameObject.transform.localScale = newScale;
    }

    public void SetCooldown(float cool)
    {
        cldwn = cool;
    }
    
    //helper method for color transitions
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

    public void SetHasInRange(bool anyInRange)
    {
        string msg = $"in seq {inSequence}";
        if (!inSequence)
        {
            msg += $" | in range {anyInRange}";

            Color target = (anyInRange) ? originalColor : defaultColor;

            StartCoroutine(ActiveTransition(circle.color, target, .15f));
        }

        Debug.Log(msg);
    }

    public void SetActive(bool active)
    {
        if (active)
        {
            inSequence = true;
            StartCoroutine(ActiveTransition(circle.color, activeColor, .2f));
        }
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
                //begins flashing with half time left
                temp -= Time.deltaTime;
                if (temp <= 0)
                {
                    yield return StartCoroutine(ActiveTransition(cooldownColor, cooldownFlashColor, .2f));
                    yield return new WaitForSeconds(.2f);
                    yield return StartCoroutine(ActiveTransition(cooldownFlashColor, cooldownColor, .2f));

                    cooldown -= .6f;
                    temp += .4f;
                }
            }
            yield return null;
        }

        yield return StartCoroutine(ActiveTransition(circle.color, defaultColor, ends));
        //Debug.Log($"(II) before: {last} -> after: {stopwatch} | {stopwatch-last} ");

        inSequence = false;
    }

}
