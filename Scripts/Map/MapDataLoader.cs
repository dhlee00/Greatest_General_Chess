using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class MapDataLoader : MonoBehaviour
{
    // 맵 디자인 구글 시트 주소
    string url = "https://docs.google.com/spreadsheets/d/e/2PACX-1vSVaGIYlQPJEbx0G7XWetXOFEqYopdOdcWGIhNttH96JMzymhc6GBWqvzFO7YiruGQYSnNUT6YaNLiL/pub?output=tsv";

    // 데이터 로드 완료
    bool loadComplete = false;
    public bool LoadComplete => loadComplete;

    // 구글 시트로 맵 데이터 불러오기
    public IEnumerator MapFromGoogleSheet()
    {
        // 구글 시트 주소로 정보 요청
        UnityWebRequest request = UnityWebRequest.Get(url);

        // 정보 올 때까지 대기
        yield return request.SendWebRequest();

        // 제대로 가져온 경우
        if (request.result == UnityWebRequest.Result.Success)
        {
            // 저장
            using (StreamWriter sw = new StreamWriter(new FileStream("mapData.tsv", FileMode.Create)))
            {
                sw.Write(request.downloadHandler.text);
                sw.Close();
            }

            loadComplete = true;
        }

        else
        {
            Debug.LogError(request.error);
        }
    }
}
