using UnityEngine;
using Unity.Netcode;

public class CameraManager : MonoBehaviour
{
    // 플레이어 1의 시점
    Vector3 player_1_View = new Vector3(13.3f, 38, -57.4f);
    
    // 플레이어 2의 시점
    Vector3 player_2_View = new Vector3(13.3f, 38, 26.6f);
    
    // 카메라가 바라볼 지점
    Vector3 targetPoint = new Vector3(13.3f, 1, -15.4f);

    void Start()
    {
        // 호스트인 경우 플레이어 1 시점 적용
        if (NetworkManager.Singleton.IsHost)
        {
            transform.position = player_1_View;
        }

        // 클라이언트인 경우 플레이어 2 시점 적용
        else
        {
            transform.position = player_2_View;
        }

        transform.LookAt(targetPoint);
    }
}
