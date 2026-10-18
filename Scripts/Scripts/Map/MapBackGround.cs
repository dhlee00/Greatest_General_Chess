using UnityEngine;

public class MapBackGround : MonoBehaviour
{
    [SerializeField] GameObject bgParent;
    [SerializeField] GameObject bgTile;

    [SerializeField] Vector2Int coord;

    [SerializeField] Material bgMaterial;

    void Start()
    {
        for (int x = 0; x <= coord.x; x++)
        {
            GameObject parent = Instantiate(bgParent, transform);
            parent.name = $"X_{x}";

            for (int y = 0; y <= ((x % 2 == 0) ? coord.y : coord.y - 1); y++)
            {
                if ((14 <= x && x <= 28) && (10 <= y && y <= ((x % 2 == 0) ? 24 : 23))) continue;

                // 타일 생성
                GameObject tile = Instantiate(bgTile, parent.transform);

                tile.name = $"Y_{y}";

                tile.transform.localPosition = new Vector3
                (
                    // 열 간격만큼 X축 이동
                    x * 1.9f,
                    0,
                    // 행 간격만큼 Z축 이동 (홀수 열일 경우 계단식 배치 보정)
                    y * -2.2f + (x % 2) * -1.1f
                );

                MeshRenderer renderer = tile.GetComponentInChildren<MeshRenderer>();

                renderer.material = bgMaterial;

                // 랜덤 회전값
                int ran = Random.Range(0, 6);
                switch (ran)
                {
                    case 0: renderer.transform.rotation = Quaternion.Euler(0, 30, 0); break;
                    case 1: renderer.transform.rotation = Quaternion.Euler(0, 90, 0); break;
                    case 2: renderer.transform.rotation = Quaternion.Euler(0, 150, 0); break;
                    case 3: renderer.transform.rotation = Quaternion.Euler(0, 210, 0); break;
                    case 4: renderer.transform.rotation = Quaternion.Euler(0, 270, 0); break;
                    case 5: renderer.transform.rotation = Quaternion.Euler(0, 330, 0); break;
                }
            }
        }
    }
}
