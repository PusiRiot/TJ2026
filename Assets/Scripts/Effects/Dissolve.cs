using System.Collections;
using UnityEngine;

public class Dissolve : MonoBehaviour
{

    public Material materialDissolve;
    public float dissolveSpeed = 0.2f;

    public void Start()
    {
        StartCoroutine(DissolveEffect());
    }

    IEnumerator DissolveEffect()
    {
        float cutoff = 0f;
        while (cutoff < 1f)
        {
            cutoff += Time.deltaTime * dissolveSpeed; // Adjust the speed of the dissolve effect here
            
            materialDissolve.SetFloat("_CutoffHeight", (cutoff));
            yield return null;
        }
            
        yield return new WaitForSeconds(1f);

        while (cutoff > 0f)
        {
            cutoff -= Time.deltaTime * dissolveSpeed; // Adjust the speed of the dissolve effect here
            materialDissolve.SetFloat("_CutoffHeight", cutoff);
            yield return null;
        }

    }
}