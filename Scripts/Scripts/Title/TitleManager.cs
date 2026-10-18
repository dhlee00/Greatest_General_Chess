using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    // 진행도 바 이미지
    [SerializeField] Image progressFilled;

    // 게임 시작 버튼
    [SerializeField] GameObject startBtn;

    // 대기 문구
    [SerializeField] GameObject waitTxt;

    void Start()
    {
        StartCoroutine(ShowProgress());
        StartCoroutine(EndLoading());
    }

    // 진행도 출력
    IEnumerator ShowProgress()
    {
        // Unity 사용자 인증
        LobbyManager.instance.InitializeServices();

        float time = 0.0f;

        // 진행 시간동안 진행
        while (time < 0.5f)
        {
            float fill = 0.99f / 0.5f;

            // 진행도 증가
            progressFilled.fillAmount += fill * Time.deltaTime;

            time += Time.deltaTime;
            yield return null;
        }
    }

    // 로딩 종료
    IEnumerator EndLoading()
    {
        while (true)
        {
            // 로그인 완료한 경우
            if (LobbyManager.instance.IsLogin)
            {
                // 진행도 출력 종료
                StopCoroutine(ShowProgress());

                // 진행도 100%으로 설정
                progressFilled.fillAmount = 1.0f;

                // 대기 시간만큼 잠시 대기
                yield return new WaitForSeconds(0.3f);

                // 진행도 비활성화
                progressFilled.transform.parent.gameObject.SetActive(false);

                // 대기 후 게임 시작 버튼 활성화
                startBtn.SetActive(true);
                BGMManager.instance.PlayBGM("Title");

                yield break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    // 게임 시작 버튼
    public void StartBtn(GameObject btn)
    {
        btn.SetActive(false);

        waitTxt.SetActive(true);

        // 로비 참여
        _ = LobbyManager.instance.JoinLobby();
    }
}
