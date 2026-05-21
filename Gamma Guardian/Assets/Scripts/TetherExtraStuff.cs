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

    public Color color1;
    public Color color2;

    bool inSequence=false;
    public float cooldownTest;

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

        cooldownFlashColor = cooldownColor*.7f + originalColor * .3f;
        cooldownFlashColor.a *= 1.2f;

        //SetActive(true);
    }

    float stopwatch = 0f;

    void Update()
    {
        if(!inSequence && Input.GetKeyDown(KeyCode.P))
        {
            inSequence = true;
            StartCooldown(cooldownTest);
        }

        if (inSequence)
        {
            stopwatch += Time.deltaTime;
        }
        else
        {
            if(stopwatch > 0f)
                Debug.Log("stopwatch: "+stopwatch);
            stopwatch = 0f;
        }
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

    public void SetActive(bool active)
    {
        if (active)
            StartCoroutine(ActiveTransition(originalColor, activeColor, .2f));
        else
            StartCoroutine(ActiveTransition(activeColor, originalColor, .2f));
    }

    IEnumerator ActiveTransition(Color start, Color end, float dur)
    {
        float clock = dur;

        while(clock > 0f)
        {
            float prog = (clock / dur);
            Color temp = prog * start + (1 - prog) * end;
            circle.color = temp;
            clock-=Time.deltaTime;
            yield return null;
        }
    }

    public void StartCooldown(float cooldown)
    {
        StartCoroutine(CooldownSequence(cooldown));
    }

    IEnumerator CooldownSequence(float cooldown)
    {
        float ends = .5f;
        cooldown -= 2f * ends;

        float half = cooldown / 2f;
        float temp = 0f;

        yield return StartCoroutine(ActiveTransition(originalColor, cooldownColor, ends));
        cooldown -= ends;

        while (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
            if (cooldown <= half)
            {
                temp -= Time.deltaTime;
                if (temp <= 0)
                {
                    float st = stopwatch;
                    yield return StartCoroutine(ActiveTransition(cooldownColor, cooldownFlashColor, .2f));
                    Debug.Log($"(EE) before: {st} -> after: {stopwatch} | {stopwatch - st} ");
                    yield return new WaitForSeconds(.2f);
                    yield return StartCoroutine(ActiveTransition(cooldownFlashColor, cooldownColor, .2f));
                    Debug.Log($"(I) before: {st} -> after: {stopwatch} | {stopwatch - st} ");

                    cooldown -= .6f;
                    temp += .4f;
                    //yield return null;
                    //continue;
                }
            }
            yield return null;
        }

        float last = stopwatch;
        yield return StartCoroutine(ActiveTransition(circle.color, originalColor, ends));
        Debug.Log($"(II) before: {last} -> after: {stopwatch} | {stopwatch-last} ");

        inSequence = false;
    }
}
