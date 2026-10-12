# Greatest General Chess

역사 속 위대한 장군들을 지휘하여 승리를 쟁취하는 전략 보드게임입니다.

<img width="1920" height="1080" alt="스크린샷 2026-10-08 105601" src="https://github.com/user-attachments/assets/86cdc006-3993-4f94-85ca-174b1fb0ec2f" />

---

📺 [시연 영상 (YouTube)](링크)<br>
🎮 [빌드 파일 다운로드 (Google Drive)](https://drive.google.com/file/d/14FHR_amLAltqyWwiunJTnI22-_Ok8Aci/view?usp=drive_link)<br>
📄 [기획서](https://github.com/dhlee00/Greatest_General_Chess/blob/main/GGC%20%EB%A9%94%EC%9D%B8%20%EA%B8%B0%ED%9A%8D%EC%84%9C.pdf)

---

## 프로젝트 개요

- **개발 형태:** 팀 프로젝트 (총 5명)
- **담당 역할:** 메인 프로그래머
- **개발 기간:** 2025.11 ~ 2026.07 (약 9개월)
- **사용 엔진 및 언어:** Unity, C#

---

## 주요 기능

<details><summary><b>육각 타일 기반 A* 알고리즘</b></summary>

#### 코드 [(전체 코드)](링크)

* 큐브 좌표계 변환
  ```csharp
  Vector3Int ConvertCube(Vector2Int coord)
  {
      int x = coord.x;
      int z = coord.y - (coord.x - (coord.x & 1)) / 2;
      int y = -(x + z);

      return new Vector3Int(x, y, z);
  }
  ```
  > 설명
  <br>

* 육각형 타일맵에서 두 좌표 간의 거리 계산
  ```csharp
  public int GetHexDis(Vector2Int currentCoord, Vector2Int targetCoord)
  {
      Vector3Int currentCube = ConvertCube(currentCoord);
      Vector3Int targetCube = ConvertCube(targetCoord);

      return (Mathf.Abs(currentCube.x - targetCube.x) + 
              Mathf.Abs(currentCube.y - targetCube.y) + 
              Mathf.Abs(currentCube.z - targetCube.z)) / 2;
  }
  ```
  > 설명
  <br>

* 육각 타일 기반 A* 알고리즘
  ```csharp
  public List<Vector2Int> Path(Vector2Int startTile, Vector2Int endTile)
  {
      openList.Clear();
      closeList.Clear();

      // 시작 노드 초기화
      Node startNode = new Node(startTile, 0, GetHexDis(startTile, endTile));
      openList.Add(startTile, startNode);

      while (openList.Count > 0)
      {
          // F비용이 가장 낮고, 같을 경우 H비용이 낮은 노드 선택
          Node nowNode = openList.Values.OrderBy(n => n.fCost).ThenBy(n => n.hCost).First();

          openList.Remove(nowNode.coord);
          closeList.Add(nowNode.coord, nowNode);

          // 목표 도달 시 경로 복원 후 반환
          if (nowNode.coord == endTile)
          {
              return RetracePath(startTile, nowNode);
          }

          // 인접 노드 탐색
          foreach (Tile tile in NeighborTiles(nowNode.coord))
          {
              // 닫힌 목록에 있거나, 암반 지형이거나, 다른 유닛이 있는 경우 스킵
              if (closeList.ContainsKey(tile.Coord) || 
                  tileList[tile.Coord].tileEv == TileEnvironment.Bedrock || 
                  (tileList[tile.Coord].Unit != null && !(tile.Coord == endTile)))
              {
                  continue;
              }

              int moveCost = GetMoveCost(nowNode.coord, tile.Coord);

              if (openList.ContainsKey(tile.Coord))
              {
                  Node neighborNode = openList[tile.Coord];
                 
                  // 더 적은 기동력 비용으로 도달 가능한 경우 갱신
                  if (nowNode.gCost + moveCost < neighborNode.gCost)
                  {
                      neighborNode.gCost = nowNode.gCost + moveCost;
                      neighborNode.fCost = neighborNode.gCost + neighborNode.hCost;
                      neighborNode.parentCoord = nowNode.coord;
                      openList[tile.Coord] = neighborNode;
                  }
              }

              else
              {
                  Node neighborNode = new Node(tile.Coord, nowNode.gCost + moveCost, GetHexDis(tile.Coord, endTile), nowNode.coord);
                  openList.Add(tile.Coord, neighborNode);
              }
          }
      } 
      return new List<Vector2Int>();
  }
  ```
  > 설명
  <br>

* 기동력 비용 계산
  ```csharp
  int GetMoveCost(Vector2Int nowNode, Vector2Int toGoNode)
  {
      int cost = 1;
      Tile fromTile = tileList[nowNode];
      Tile toTile = tileList[toGoNode];

      // 타일 층수 차이에 따른 추가 비용
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
  ```
  > 설명
  <br>

#### 설계 의도
> 여기에 쓰기

---
