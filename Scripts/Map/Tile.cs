using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// 타일의 환경
public enum TileEnvironment
{
    Bush,         // 풀숲
    GravelField,  // 자갈
    Bedrock,      // 암반
    Ground        // 땅
}

// 타일의 층
public enum TileFloor
{
    One = 1,  // 1층
    Two,      // 2층
    Three     // 3층
}

// 타일 데이터
[System.Serializable]
public class TileData
{
    [SerializeField] Vector3 tilePos;
    [SerializeField] Vector2Int coord;
    [SerializeField] TileEnvironment tileEv;
    [SerializeField] TileFloor tileFloor;

    public Vector3 TilePos { get => tilePos; set => tilePos = value; }
    public Vector2Int Coord { get => coord; set => coord = value; }
    public TileEnvironment TileEv { get => tileEv; set => tileEv = value; }
    public TileFloor TileFloor { get => tileFloor; set => tileFloor = value; }
}

public class Tile : MonoBehaviour
{
    // 타일 정보
    [SerializeField] TileData tileData;
    public TileData TileData => tileData;

    // 현재 타일에 있는 유닛
    [SerializeField] Unit unit;
    public Unit Unit { get => unit; set => unit = value; }

    // 환경 매쉬
    [SerializeField] Transform evMesh;

    // 땅 매쉬
    [SerializeField] Transform groundMesh;

    // 환경 매쉬에 적용할 메테리얼
    [SerializeField] Material[] evMaterial;

    // 하이라이트
    [SerializeField] Image moveHighlight;
    public Image MoveHighlight => moveHighlight;
    
    [SerializeField] Image attackHighlight;
    public Image AttackHighlight => attackHighlight;

    // 타일 환경
    public TileEnvironment tileEv => tileData.TileEv;

    // 타일 위치
    public Vector2Int Coord => tileData.Coord;

    // 타일 높이
    [SerializeField] float tileHeigh;
    public float TileHeigh => tileHeigh;

    // 타일 설정
    public void SetTile(TileData data)
    {
        // 타일 데이터 받아오기
        tileData = data;

        // 타일 이름 설정
        gameObject.name = $"{tileData.TileEv}_{(int)tileData.TileFloor}F";

        // 환경이 땅인 경우
        if (tileData.TileEv == TileEnvironment.Ground)
        {
            // 기본(0.8) 타일 크기에 타일의 층 만큼 환경 매쉬(0.4) 타일 크기 더하기
            groundMesh.localScale = new Vector3(1, groundMesh.localScale.y + ((int)tileData.TileFloor * evMesh.localScale.y), 1);

            // 타일 높이 저장
            tileHeigh = groundMesh.localScale.y;
        }

        // 환경이 땅이 아닌 경우
        else
        {
            // 타일의 층 만큼 높이 조절
            groundMesh.localScale = new Vector3(1, groundMesh.localScale.y * (int)tileData.TileFloor, 1);

            // 환경 매쉬 활성화
            evMesh.gameObject.SetActive(true);

            // 환경이 땅이 아닌 경우 환경 매쉬 랜더러 불러옴
            MeshRenderer evMeshRen = evMesh.GetComponent<MeshRenderer>();

            // 환경 매쉬 리소스 적용
            evMeshRen.sharedMaterial = evMaterial[(int)tileData.TileEv];

            // 타일의 층 만큼 환경 매쉬 위로 이동
            evMesh.position = new Vector3(evMesh.position.x, 0.5f * (int)tileData.TileFloor, evMesh.position.z);

            // 타일 높이 저장
            tileHeigh = groundMesh.localScale.y + evMesh.localScale.y;
        }

        // 타일 위치 저장
        tileData.TilePos = new Vector3(transform.position.x, tileHeigh / 2.0f + 0.25f, transform.position.z);

        // 하이라이트 위치 조정
        Vector3 hPos = new Vector3(transform.position.x, tileHeigh / 2.0f + 0.01f, transform.position.z);
        attackHighlight.transform.position = hPos;
        moveHighlight.transform.position = hPos;

        if (!NetworkManager.Singleton.IsServer)
        {
            moveHighlight.transform.rotation = Quaternion.Euler(90, 180, 0);
        }
    }

    // 기동 하이라이트 활성
    public void SetMoveHighlight(bool onoff)
    {
        moveHighlight.gameObject.SetActive(onoff);
    }

    // 공격 하이라이트 활성
    public void SetAttackHighlight(bool onoff)
    {
        attackHighlight.gameObject.SetActive(onoff);
    }
}
