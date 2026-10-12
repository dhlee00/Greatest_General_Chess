using System.Collections;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    [SerializeField] float lifeTime;

    [Header("If VFX End")]
    [SerializeField] bool destroy;

    void OnEnable()
    {
        StartCoroutine(WaitEndVFX());
    }

    IEnumerator WaitEndVFX()
    {
        yield return new WaitForSeconds(lifeTime);

        if (!destroy)
        {
            gameObject.SetActive(false);
        }

        else
        {
            Destroy(gameObject);
        }
    }
}
