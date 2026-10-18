using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 노드
struct Node
{
    public Vector2Int coord;  // 노드 좌표
    public int gCost;  // 출발 노드에서 현재 노드까지의 실제 이동 비용
    public int hCost;  // 현재 노드에서 목표 노드까지의 예상 이동 비용
    public int fCost;  // g비용과 h비용을 더한 총 예상 비용

    public Vector2Int parentCoord;  // 경로를 추적하기 위한 바로 이전 노드의 좌표

    // 노드 생성자
    public Node(Vector2Int coord, int gCost, int hCost, Vector2Int parentCoord = default)
    {
        this.coord = coord;
        this.gCost = gCost;
        this.hCost = hCost;
        this.fCost = gCost + hCost;
        this.parentCoord = parentCoord;
    }
}

public class FindPath
{
    // 타일 리스트
    Dictionary<Vector2Int, Tile> tileList;

    // 타일 리스트 받기
    public FindPath(Dictionary<Vector2Int, Tile> list)
    {
        tileList = list;
    }

    #region A* 알고리즘
    // 탐색할 노드들을 저장하는 열린 목록
    Dictionary<Vector2Int, Node> openList = new Dictionary<Vector2Int, Node>();

    // 탐색을 완료한 노드들을 저장하는 닫힌 목록
    Dictionary<Vector2Int, Node> closeList = new Dictionary<Vector2Int, Node>();

    // 최단 경로 이동 비용 계산
    public int PathDis(Vector2Int start, Vector2Int end)
    {
        // 최단 경로 타일 리스트
        List<Vector2Int> pathTiles = Path(start, end);

        // 경로가 없거나 자기 자신이면 이동 불가
        if (pathTiles.Count <= 1) return 999;

        int totalCost = 0;

        // 경로상의 타일들을 순회
        for (int i = 1; i < pathTiles.Count; i++)
        {
            // 이전 타일에서 다음 타일로 갈 때 드는 비용을 추가
            totalCost += GetMoveCost(pathTiles[i - 1], pathTiles[i]);
        }

        // 총 기동력 소모값 반환
        return totalCost;
    }

    // 길 찾기
    public List<Vector2Int> Path(Vector2Int startTile, Vector2Int endTile)
    {
        // 목록 초기화
        openList.Clear();
        closeList.Clear();

        // 시작 노드 초기화
        Node startNode = new Node(startTile, 0, GetHexDis(startTile, endTile));

        // 열린 목록에 추가
        openList.Add(startTile, startNode);

        // 열린 목록이 빌 때까지 알고리즘 반복
        while (openList.Count > 0)
        {
            // 열린 목록에서 f비용이 가장 낮은 노드를 선택 (같으면 h비용이 더 낮은 노드 (더 가까운 노드)를 선택)
            Node nowNode = openList.Values.OrderBy(n => n.fCost).ThenBy(n => n.hCost).First();

            // 현재 노드를 열린 목록에서 제거하고 닫힌 목록에 추가
            openList.Remove(nowNode.coord);
            closeList.Add(nowNode.coord, nowNode);

            // 목표 도달 확인
            if (nowNode.coord == endTile)
            {
                // 목표에 도달했으면 경로를 복원하여 반환
                return RetracePath(startTile, nowNode);
            }

            // 인접 노드 탐색
            foreach (Tile tile in NeighborTiles(nowNode.coord))
            {
                // 이미 closeList에 있는 경우 + 이동 불가능한 지형인 경우 + 다른 유닛이 있는 타일인 경우 스킵
                if (closeList.ContainsKey(tile.Coord) || tileList[tile.Coord].tileEv == TileEnvironment.Bedrock || (tileList[tile.Coord].Unit != null && !(tile.Coord == endTile)))
                {
                    continue;
                }

                // 이웃 노드가 열린 목록에 이미 있는 경우
                if (openList.ContainsKey(tile.Coord))
                {
                    // 열린 목록에서 기록된 정보 가져오기
                    Node neighborNode = openList[tile.Coord];

                    // 인접 타일의 실제 이동 비용
                    int moveCost = GetMoveCost(nowNode.coord, neighborNode.coord);

                    // 새로 찾은 경로가 기존 경로보다 기동력이 덜 든 경우 정보 갱신
                    if (nowNode.gCost + moveCost < neighborNode.gCost)
                    {
                        neighborNode.gCost = nowNode.gCost + moveCost;
                        neighborNode.fCost = neighborNode.gCost + neighborNode.hCost;
                        neighborNode.parentCoord = nowNode.coord;

                        openList[tile.Coord] = neighborNode;
                    }
                }

                // 이웃 노드가 열린 목록에 없는 경우 (새롭게 발견된 노드)
                else
                {
                    // 목적지까지의 직선거리(예상 비용) 계산
                    int hCost = GetHexDis(tile.Coord, endTile);

                    // 현재 위치한 타일에서 이 타일로 갈 때 드는 비용 계산
                    int moveCost = GetMoveCost(nowNode.coord, tile.Coord);

                    // 새로운 노드 데이터 생성
                    Node neighborNode = new Node
                    (
                        tile.Coord,
                        nowNode.gCost + moveCost,
                        hCost,
                        nowNode.coord
                    );

                    // 열린 목록에 추가
                    openList.Add(tile.Coord, neighborNode);
                }
            }
        }

        return new List<Vector2Int>();
    }

    // 경로 복원 함수
    List<Vector2Int> RetracePath(Vector2Int startCoord, Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;

        // 시작 노드에 도달할 때까지 부모 노드를 따라 거꾸로 이동
        while (currentNode.coord != startCoord)
        {
            // 현재 노드를 경로에 추가 (역순으로 추가)
            path.Add(currentNode.coord);

            if (closeList.ContainsKey(currentNode.parentCoord))
            {
                // 부모 노드 정보를 닫힌 목록에서 가져와 다음 노드로 설정
                currentNode = closeList[currentNode.parentCoord];
            }
        }

        // 마지막에 출발지점 추가
        path.Add(startCoord);

        // 경로가 끝부터 저장되었으므로 역순으로 변경
        path.Reverse();

        // 복원된 최종 경로 반환
        return path;
    }
    #endregion

    // 현재 타일의 인접한 6개 타일의 좌표를 계산하고 맵 내에 존재하는지 확인
    public List<Tile> NeighborTiles(Vector2Int nowTile)
    {
        List<Tile> resultNeighbors = new List<Tile>();

        // Odd-q 좌표계에서 인접 타일 6개의 상대 좌표
        Vector2Int[] neighborTiles = new Vector2Int[]
        {
            new Vector2Int(nowTile.x, nowTile.y - 1),  // 상
            new Vector2Int(nowTile.x, nowTile.y + 1),  // 하
            new Vector2Int(nowTile.x - 1, nowTile.x % 2 == 0 ? nowTile.y - 1 : nowTile.y),  // 좌상
            new Vector2Int(nowTile.x - 1, nowTile.x % 2 == 0 ? nowTile.y : nowTile.y + 1),  // 좌하
            new Vector2Int(nowTile.x + 1, nowTile.x % 2 == 0 ? nowTile.y - 1 : nowTile.y),  // 우상
            new Vector2Int(nowTile.x + 1, nowTile.x % 2 == 0 ? nowTile.y : nowTile.y + 1)   // 우하
        };

        // 인접 타일이 실제로 맵에 존재하는지 확인
        foreach (Vector2Int coord in neighborTiles)
        {
            if (tileList.ContainsKey(coord))
            {
                resultNeighbors.Add(tileList[coord]);
            }
        }

        return resultNeighbors;
    }

    // 큐브 좌표계 변환
    Vector3Int ConvertCube(Vector2Int coord)
    {
        int x = coord.x;
        int z = coord.y - (coord.x - (coord.x & 1)) / 2;
        int y = -(x + z);

        return new Vector3Int(x, y, z);
    }

    // 육각형 타일맵에서 두 좌표 간의 거리 계산
    public int GetHexDis(Vector2Int currentCoord, Vector2Int targetCoord)
    {
        // 일반 좌표를 큐브 좌표계로 변환
        Vector3Int currentCube = ConvertCube(currentCoord);
        Vector3Int targetCube = ConvertCube(targetCoord);

        return (Mathf.Abs(currentCube.x - targetCube.x) + Mathf.Abs(currentCube.y - targetCube.y) + Mathf.Abs(currentCube.z - targetCube.z)) / 2;
    }

    // 기동력 소모 비용 계산
    int GetMoveCost(Vector2Int nowNode, Vector2Int toGoNode)
    {
        int cost = 1;

        Tile fromTile = tileList[nowNode];
        Tile toTile = tileList[toGoNode];

        // 층수 차이에 따른 추가 비용
        if (toTile.TileData.TileFloor > fromTile.TileData.TileFloor)
        {
            cost += 1;
        }

        // 험지 타일 추가 비용
        if (toTile.tileEv == TileEnvironment.GravelField)
        {
            cost += 1;
        }

        return cost;
    }
}
