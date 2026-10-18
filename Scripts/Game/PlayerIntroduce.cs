using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerIntroduce : MonoBehaviour
{
    // UI 매니저 (또는 UIManager)
    [SerializeField] UIManager m_UI;

    [Header("")]

    [SerializeField] RectTransform p1Side;  // 플레이어 1 소개창
    [SerializeField] RectTransform p2Side;  // 플레이어 2 소개창
    [SerializeField] RectTransform vsTxt;   // VS 텍스트
    Image backGround;

    [Header("")]

    [SerializeField] Image p1_GeneralImg;
    [SerializeField] TextMeshProUGUI p1_GeneralNameTxt;
    [SerializeField] TextMeshProUGUI p1_MentTxt;

    [Header("")]

    [SerializeField] Image p2_GeneralImg;
    [SerializeField] TextMeshProUGUI p2_GeneralNameTxt;
    [SerializeField] TextMeshProUGUI p2_MentTxt;

    [Header("")]

    [SerializeField] float pSideIntroDuration;  // 플레이어 정보 연출 시간
    [SerializeField] float vsTxtIntroDuration;  // vs 텍스트 연출 시간
    [SerializeField] float waitTime;            // 대기 시간
    [SerializeField] float endDuration;         // 소개 종료 연출 시간

    void Start()
    {
        backGround = GetComponent<Image>();
    }

    // 소개
    public IEnumerator Introduction(GeneralData p1_General, GeneralData p2_General)
    {
        p1_GeneralImg.sprite = p1_General.GeneralImage.introduceImg;
        p1_GeneralNameTxt.text = p1_General.GeneralName;
        p1_MentTxt.text = $"\"{p1_General.GeneralMent}\"";

        p2_GeneralImg.sprite = p2_General.GeneralImage.introduceImg;
        p2_GeneralNameTxt.text = p2_General.GeneralName;
        p2_MentTxt.text = $"\"{p2_General.GeneralMent}\"";

        float time = 0f;

        // 각 플레이어 정보 초기 위치
        Vector2 p1Start = p1Side.anchoredPosition;
        Vector2 p2Start = p2Side.anchoredPosition;

        // 각 플레이어 정보 도착 위치
        Vector2 p1End = new Vector2(-415.84f, p1Start.y);
        Vector2 p2End = new Vector2(415.84f, p2Start.y);

        while (time < pSideIntroDuration)
        {
            time += Time.deltaTime;
            float percent = time / pSideIntroDuration;

            // 도착 위치로 연출 시간 동안 이동
            p1Side.anchoredPosition = Vector2.Lerp(p1Start, p1End, percent);
            p2Side.anchoredPosition = Vector2.Lerp(p2Start, p2End, percent);

            yield return null;
        }

        // 목적지 보정 (또는 최종 위치 보정)
        p1Side.anchoredPosition = p1End;
        p2Side.anchoredPosition = p2End;

        // 잠시 대기
        yield return new WaitForSeconds(0.3f);

        time = 0f;

        while (time < vsTxtIntroDuration)
        {
            time += Time.deltaTime;
            float percent = time / vsTxtIntroDuration;

            // 도착 위치로 연출 시간 동안 이동 (또는 스케일 조절 연출)
            vsTxt.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, percent);

            yield return null;
        }

        // 목적지 보정
        vsTxt.localScale = Vector3.one;

        // 연출 후 대기
        yield return new WaitForSeconds(waitTime);

        // 소개 종료
        backGround.color = new Color(0, 0, 0, 0);

        time = 0f;

        while (time < endDuration)
        {
            time += Time.deltaTime;
            float percent = time / endDuration;

            p1Side.anchoredPosition = Vector2.Lerp(p1End, p1Start, percent);
            p2Side.anchoredPosition = Vector2.Lerp(p2End, p2Start, percent);

            vsTxt.gameObject.SetActive(false);

            yield return null;
        }

        yield return null;

        m_UI.PrintStep(GameReadyStep.UnutPosSetting);

        // 자기 자신 비활성화
        gameObject.SetActive(false);
    }
}
