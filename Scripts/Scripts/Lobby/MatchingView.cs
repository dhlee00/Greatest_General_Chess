using TMPro;
using UnityEngine;

public class MatchingView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI matchingTimeTxt;
    float matchingTimer = 0.0f;

    void OnEnable()
    {
        matchingTimer = 0.0f;
    }

    void Update()
    {
        matchingTimer += Time.deltaTime;

        matchingTimeTxt.text = $"{matchingTimer:F0}초";
    }
}
