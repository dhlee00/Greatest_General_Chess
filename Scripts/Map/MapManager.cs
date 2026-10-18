using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    // 유닛 매니저
    [SerializeField] UnitManager m_Unit;

    // 타일 프리팹
    [SerializeField] GameObject tilePrefab;

    // 타일 리스트
    Dictionary<Vector2Int, Tile> tileList = new Dictionary<Vector2Int, Tile>();
    public Dictionary<Vector2Int, Tile> TileList => tileList;

    // 유닛 리스트
    Dictionary<string, Unit> AllyUnitList => m_Unit.AllyUnitList;
    Dictionary<string, Unit> EnemyUnitList => m_Unit.EnemyUnitList;

    // 하이라이트를 활성화 시킬 타일 리스트
    Vector2Int selectTile;
    List<Vector2Int> moveTileList = new List<Vector2Int>();
    List<Vector2Int> attackTileList = new List<Vector2Int>();

    // 길 찾기 알고리즘
    FindPath findPath;
    public FindPath FindPath => findPath;

    #region 맵 생성
    // 맵 데이터 파싱
    Dictionary<Vector2Int, TileData> ParseMapData()
    {
        Dictionary<Vector2Int, TileData> result = new Dictionary<Vector2Int, TileData>();

        // 저장 된 맵 데이터 파일 불러오기
        string tsvData = string.Empty;
        using (StreamReader sr = new StreamReader(new FileStream("mapData.tsv", FileMode.Open)))
        {
            tsvData = sr.ReadToEnd();
            sr.Close();
        }
        
        // 줄 단위로 저장
        string[] lines = tsvData.Split('\n');

        // 줄 개수만큼 반복
        for (int y = 0; y < lines.Length; y++)
        {
            // 셀 단위로 저장
            string[] cells = lines[y].Split('\t');

            // 줄 하나의 셀 개수만큼 반복
            for (int x = 0; x < cells.Length; x++)
            {
                // 공백 제거
                cells[x] = cells[x].Trim();

                // 빈 값이 아닌 경우
                if (cells[x] != "")
                {
                    // 환경, 층 값 분리
                    string[] value = cells[x].Split(',');

                    // 환경 값 저장
                    int environment = 0;
                    switch (value[0])
                    {
                        case "숲": environment = 0; break;
                        case "자갈": environment = 1; break;
                        case "암반": environment = 2; break;
                        case "땅": environment = 3; break;
                    }

                    // 층 값 저장
                    int floor = int.Parse(value[1]);

                    // 변경할 타일 좌표 계산
                    Vector2Int tileCoord = new Vector2Int(x, y);

                    // 변경 할 타일 데이터
                    TileData data = new TileData
                    {
                        Coord = new Vector2Int(x, y),
                        TileEv = (TileEnvironment)environment,
                        TileFloor = (TileFloor)floor
                    };

                    // 변경 목록에 추가
                    result.Add(tileCoord, data);
                }
            }
        }

        return result;
    }

    public void CreateMap()
    {
        Dictionary<Vector2Int, TileData> changeTiles = ParseMapData();

        // 열
        for (int x = 0; x < 15; x++)
        {
            // 행
            for (int y = 0; y < ((x % 2 == 0) ? 15 : 14); y++)
            {
                // 좌표 생성
                Vector2Int newCoord = new Vector2Int(x, y);

                // 타일 데이터 생성
                TileData newData = new TileData();

                // 변경 할 타일 목록에 있는 좌표의 타일이면
                if (changeTiles.ContainsKey(newCoord))
                {
                    // 타일 데이터 불러오기
                    newData.Coord = newCoord;
                    newData.TileEv = changeTiles[newCoord].TileEv;
                    newData.TileFloor = changeTiles[newCoord].TileFloor;
                }

                // 없다면 1층 땅으로 생성
                else
                {
                    newData.Coord = newCoord;
                    newData.TileEv = TileEnvironment.Ground;
                    newData.TileFloor = TileFloor.One;
                }

                // 타일 생성
                GameObject tile = Instantiate(tilePrefab, transform);

                // 위치 설정
                tile.transform.position = new Vector3
                (
                    // 열 간격만큼 X축 이동
                    x * 1.9f,
                    0,
                    // 행 간격만큼 Z축 이동 (홀수 열일 경우 계단식 배치 보정)
                    y * -2.2f + (x % 2) * -1.1f
                );

                // 생성된 타일 데이터 적용
                Tile tm = tile.GetComponent<Tile>();
                tm.SetTile(newData);

                // 딕셔너리에 추가
                tileList.Add(newCoord, tm);
            }
        }

        // 길 찾기 알고리즘 생성
        SetFindPath();
    }
    #endregion

    // 길 찾기 알고리즘 생성
    void SetFindPath()
    {
        // FindPath 초기화
        findPath = new FindPath(tileList);
    }

    #region 하이라이트
    // 기동 하이라이트
    public void ShowMoveHighlight(Unit myUnit, bool onoff)
    {
        // 활성화 시킨 경우
        if (onoff)
        {
            // 타일 목록 순회 (a* 알고리즘)
            foreach (Vector2Int coord in tileList.Keys)
            {
                // 암반 타일 + 시작지점 타일 + 다른 유닛이 있는 타일 무시
                if (tileList[coord].tileEv == TileEnvironment.Bedrock || tileList[coord].TileData.Coord == myUnit.Coord || (tileList[coord].Unit != null && !tileList[coord].Unit.IsConcealment))
                {
                    continue;
                }

                // 기동 가능한 거리의 타일들 리스트에 저장
                if (findPath.PathDis(myUnit.Tile.TileData.Coord, coord) <= myUnit.Movement)
                {
                    moveTileList.Add(coord);
                }
            }

            // 기동 가능한 타일
            foreach (Vector2Int coord in moveTileList)
            {
                tileList[coord].SetMoveHighlight(onoff);
            }
        }

        // 비활성 시킨 경우
        else
        {
            // 기동 가능한 타일
            foreach (Vector2Int coord in moveTileList)
            {
                tileList[coord].SetMoveHighlight(onoff);
            }

            // 목록 초기화
            moveTileList.Clear();
        }
    }

    // 공격 하이라이트
    public void ShowAttackHighlight(Unit myUnit, bool onoff)
    {
        // 활성화 시킨 경우
        if (onoff)
        {
            // 공격 사거리가 1인 경우
            if (myUnit.AttackRange <= 1)
            {
                // a* 알고리즘의 인접 타일 반환하는 함수로 주변 타일 불러옴
                foreach (Tile tile in FindPath.NeighborTiles(myUnit.Coord))
                {
                    Unit enemyUnit = tileList[tile.Coord].Unit;

                    // 주변 타일에 적 유닛이 있는지 확인 + 은폐한 적은 제외
                    if (enemyUnit != null && !enemyUnit.IsAlly && !enemyUnit.IsConcealment && (int)enemyUnit.Tile.TileData.TileFloor - (int)myUnit.Tile.TileData.TileFloor < 2)
                    {
                        // 적이 있다면 리스트에 저장
                        attackTileList.Add(tile.Coord);
                    }
                }
            }

            // 공격 사거리가 2 이상인 경우
            else
            {
                // 곡사 공격 방식 유닛
                if (myUnit.AttackType == AttackType.IndirectFire)
                {
                    Trebuchet trebuchet = myUnit as Trebuchet;

                    foreach (Vector2Int tileCoord in tileList.Keys)
                    {
                        // 암반 환경 타일과 자기 자신 타일 제외
                        if (tileList[tileCoord].tileEv == TileEnvironment.Bedrock || tileCoord == myUnit.Coord)
                        {
                            continue;
                        }

                        // 최소 사거리 보다 가까운 타일 제외
                        if (FindPath.GetHexDis(trebuchet.Coord, tileCoord) < trebuchet.MinAttackRange)
                        {
                            continue;
                        }

                        // 트레뷰셋이 서있는 타일보다 낮은 타일은 추가 사거리 적용
                        int bonus = (tileList[tileCoord].TileData.TileFloor < myUnit.Tile.TileData.TileFloor) ? (int)myUnit.Tile.TileData.TileFloor - 1 : 0;

                        // 공격 사거리 내의 타일 리스트에 추가
                        if (findPath.GetHexDis(myUnit.Coord, tileCoord) <= myUnit.AttackRange + bonus)
                        {
                            attackTileList.Add(tileCoord);
                        }
                    }
                }

                // 나머지
                else
                {
                    // 임시 대상 리스트
                    List<Vector2Int> tempTargetList = new List<Vector2Int>();

                    // 적 유닛 목록 순회
                    foreach (Unit enemyUnit in EnemyUnitList.Values)
                    {
                        // 은신한 적 + 공격자 보다 2층 높은 유닛은 하이라이트 표시하지 않음
                        if (enemyUnit.IsConcealment && (int)enemyUnit.Tile.TileData.TileFloor - (int)myUnit.Tile.TileData.TileFloor >= 2)
                        {
                            continue;
                        }

                        // 공격 사거리 내의 유닛
                        if (findPath.GetHexDis(myUnit.Coord, enemyUnit.Coord) <= myUnit.AttackRange)
                        {
                            // 레이 시작점
                            Vector3 rayOrigin = myUnit.Tile.transform.position;
                            rayOrigin.y = 0.1f;

                            // 레이 방향 (적 유닛 쪽)
                            Vector3 enemyPos = enemyUnit.Tile.transform.position;
                            enemyPos.y = rayOrigin.y;

                            Vector3 rayDir = enemyPos - rayOrigin;

                            // 레이 거리 (적 유닛까지)
                            float rayDis = Vector3.Distance(rayOrigin, enemyPos);

                            Debug.DrawRay(rayOrigin, rayDir, Color.red, 3.0f);
                            Debug.DrawRay(new Vector3(rayOrigin.x, rayOrigin.y + 1.6f, rayOrigin.z), rayDir, Color.green, 3.0f);

                            // 레이에 닿은 대상들 가져오기
                            RaycastHit[] hits = Physics.RaycastAll(rayOrigin, rayDir, rayDis, 1 << 11).OrderBy(h => h.distance).ToArray();

                            // 하나 이상 있는 경우
                            if (hits.Length > 0)
                            {
                                // 닿은 대상들 순회
                                foreach (RaycastHit hit in hits)
                                {
                                    Tile hitTile = hit.transform.parent.GetComponent<Tile>();

                                    if (hitTile == null) continue;

                                    // 시작타일 제외
                                    if (hitTile == myUnit.Tile) continue;

                                    // 공격 대상 타일에 도달한 경우 + 공격 대상이 은폐 중이 아닐 때
                                    if (hitTile == enemyUnit.Tile && !enemyUnit.IsConcealment)
                                    {
                                        tempTargetList.Add(enemyUnit.Coord);
                                        break;
                                    }

                                    // 중간에 다른 유닛이 있는지 확인
                                    if (hitTile.Unit != null) break;

                                    // 타일 층수
                                    int hitTileFloor = (int)hitTile.TileData.TileFloor;
                                    int myFloor = (int)myUnit.Tile.TileData.TileFloor;
                                    int enemyFloor = (int)enemyUnit.Tile.TileData.TileFloor;

                                    // 중간에 더 높은 층의 타일이 있는지 확인
                                    if (hitTileFloor > myFloor || hitTileFloor > enemyFloor)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // 임시 공격 대상들 중 주변의 공격자와 더 가까운 유닛이 있는 경우
                    foreach (Vector2Int coord in tempTargetList)
                    {
                        // 임시 공격 대상과 공격자의 거리
                        int targetDis = FindPath.GetHexDis(myUnit.Coord, coord);

                        bool addList = true;

                        // 임시 공격 대상자의 주변 타일
                        foreach (Tile neighborTile in FindPath.NeighborTiles(coord))
                        {
                            // 주변 타일에 유닛이 있고 그 유닛이 적인 경우
                            if (neighborTile.Unit != null && !neighborTile.Unit.IsAlly)
                            {
                                // 해당 유닛과 공격자의 거리 계산
                                int neighborDis = FindPath.GetHexDis(myUnit.Coord, neighborTile.Coord);

                                // 더 가까운 경우
                                if (neighborDis < targetDis)
                                {
                                    addList = false;
                                    break;
                                }
                            }
                        }

                        if (addList)
                        {
                            attackTileList.Add(coord);
                        }
                    }
                }
            }

            // 공격 사거리 내에 유닛이 있는 타일
            foreach (Vector2Int coord in attackTileList)
            {
                tileList[coord].SetAttackHighlight(onoff);
            }
        }

        // 비활성 시킨 경우
        else
        {
            // 공격 사거리 내에 유닛이 있는 타일
            foreach (Vector2Int coord in attackTileList)
            {
                tileList[coord].SetAttackHighlight(onoff);
            }

            // 목록 초기화
            attackTileList.Clear();
        }
    }
    #endregion
}
