using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    // 맵 데이터 로더
    [SerializeField] MapDataLoader mapDataLoader;

    // 플레이어 초상화
    [SerializeField] Image player_1_Img;
    [SerializeField] Image player_2_Img;

    // 플레이어 이름
    [SerializeField] Text player_1_Name;
    [SerializeField] Text player_2_Name;

    // 진행도 바 이미지
    [SerializeField] Image progressFilled;

    // 진행도 텍스트
    [SerializeField] Text progressTxt;

    // 진행 시간
    [SerializeField] float progressDuration;

    // 대기 시간
    [SerializeField] float waitTime;

    void Start()
    {
        StartCoroutine(mapDataLoader.MapFromGoogleSheet());
        StartCoroutine(ShowProgress());
        StartCoroutine(EndLoading());
    }

    // 진행도 출력
    IEnumerator ShowProgress()
    {
        float time = 0.0f;

        // 진행 시간동안 진행
        while (time < progressDuration)
        {
            float fill = 0.99f / progressDuration;

            // 진행도 증가
            progressFilled.fillAmount += fill * Time.deltaTime;
            progressTxt.text = progressFilled.fillAmount.ToString("P0");

            time += Time.deltaTime;
            yield return null;
        }
    }

    // 로딩 종료
    IEnumerator EndLoading()
    {
        while (true)
        {
            // 맵 데이터를 불러온 경우
            if (mapDataLoader.LoadComplete)
            {
                // 진행도 출력 종료
                StopCoroutine(ShowProgress());

                // 만약 클라이언트가 먼저 로딩이 끝난 경우 대기
                if (!NetworkManager.Singleton.IsServer)
                {
                    while (!RelayNetwork.instance.IsHostLoadComplete)
                    {
                        Debug.Log("호스트 아직 로딩 중");

                        // 잠시 대기
                        yield return new WaitForSeconds(0.5f);
                    }
                }

                // 진행도 100%으로 설정
                progressFilled.fillAmount = 1.0f;
                progressTxt.text = progressFilled.fillAmount.ToString("P0");

                // 대기 시간만큼 잠시 대기
                yield return new WaitForSeconds(waitTime);

                // 대기 후 게임씬 로드
                SceneManager.LoadScene("2.GameScene");
                BGMManager.instance.PlayBGM("InGame");

                yield break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
}
