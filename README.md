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

<details><summary><b>육각 타일 기반 길찾기 알고리즘</b></summary>

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
  > 일반적인 타일 좌표는 육각 타일의 거리 계산이 어렵기 때문에 큐브 좌표계로 변환하는 로직을 구현했습니다. 이를 통해 타일 간의 거리를 쉽게 계산할 수 있습니다.
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
  > 변환된 큐브 좌표를 활용하여 두 육각 타일 사이의 최단 거리를 계산합니다. 세 축의 절대값 차이를 더한 뒤 2로 나누는 공식을 사용했습니다.
  <br>

* A* 알고리즘
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
  > 유닛의 현재 타일부터 이동할 타일까지의 최단 경로와 경로 내 타일들을 구하기 위해 A* 알고리즘을 구현했습니다.<br>
  매 루프마다 총 비용이 가장 낮은 노드를 우선 탐색하되 탐색 과정에서 이동 불가능한 암반 환경 타일이나<br>
  다른 유닛이 위치한 타일 등 일부 타일들을 필터링하여 유효한 경로만 도출하도록 설계했습니다.
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
  > 게임 규칙상 이동 비용이 추가적으로 발생하는 타일에 대해 추가 비용을 더하는 코드입니다.
  <br>

#### 설계 의도
> 큐브좌표계를 활용해 거리를 계산하고 특정 타일 회피를 고려한 타일과 타일 간의<br>
최단 경로를 효율적으로 찾기 위해 길찾기에 특화된 A* 알고리즘을 구현했습니다.<br>
이를 이용해 유닛이 이동 가능한 타일들을 나타내는 기능과 서서히 이동하는 연출 등 타일과 관련된 기능들을 쉽게 구현할 수 있었습니다.

---

</details>

<details><summary><b>Unity Netcode for GameObjects & Relay 기반 온라인 매치메이킹</b></summary>

#### 코드 [(전체 코드)](링크)

* 매칭 상태 업데이트 및 상대방 탐색
  ```csharp
  public async void StartMatch()
  {
      isMatching = true;

      Dictionary<string, string> option = new Dictionary<string, string>
      {
          { "Status", "Matching" },
          { "MatchingTime", DateTime.Now.Ticks.ToString() },
          { "SelectedGeneral", myGeneral }
      };

      // 서버에 내 매칭 상태 업데이트
      await UpdatePlayerData(option);

      while (isMatching)
      {
          mainLobby = await LoadMainLobby();

          // 매칭 중인 다른 유저 탐색
          var matchingUser = mainLobby.Players.FirstOrDefault(p => p.Id != playerId && p.Data != null && p.Data.ContainsKey("Status") && p.Data["Status"].Value == "Matching");

          if (matchingUser != null)
          {
              long myMatchingTime = long.Parse(mainLobby.Players.First(p => p.Id == playerId).Data["MatchingTime"].Value);
              long opponentMatchingTime = long.Parse(matchingUser.Data["MatchingTime"].Value);
              opponentGeneral = matchingUser.Data["SelectedGeneral"].Value;

              // 먼저 매칭을 시작한 유저가 방(Relay)을 생성하고 참여 코드 공유
              if (myMatchingTime < opponentMatchingTime || (myMatchingTime == opponentMatchingTime && string.Compare(playerId, matchingUser.Id) < 0))
              {
                  var joinCode = await RelayNetwork.instance.StartRelay();
                  await ShareCode(joinCode);
                  isMatching = false;
                  return;
              }

              // 늦게 시작한 유저는 상대방의 참여 코드로 방에 입장
              else
              {
                  if (await JoinRoom(matchingUser.Id))
                  {
                      isMatching = false;
                      return;
                  }
              }
          }

          else
          {
              await Task.Delay(500);
          }
      }
  }
  ```
  > 매칭을 시작한 시간과 나의 상태를 매칭 중인 상태로 업데이트 하고 또 다른 매칭 중인 유저를 탐색합니다.<br>
  상대방을 찾은 경우 상대방과 비교해서 매칭을 시작한 시간이 빠른 유저가 방을 생성하고 참여 코드를 공유합니다.<br>
  늦게 매칭을 시작한 유저는 참여 코드가 공유 되면 방에 참여합니다.
  <br>

* Unity Relay를 통한 호스트 세션 생성 및 참여 코드 발급
  ```csharp
  public async Task<string> StartRelay()
  {
      // Relay Allocation 생성 및 Join Code 발급
      Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
      string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

      var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

      // 호스트 릴레이 데이터 설정
      unityTransport.SetHostRelayData
      (
          allocation.RelayServer.IpV4,
          (ushort)allocation.RelayServer.Port,
          allocation.AllocationIdBytes,
          allocation.Key,
          allocation.ConnectionData
      );

      NetworkManager.Singleton.StartHost();
      SceneManager.sceneLoaded += GenerateGameManager;
      SceneManager.LoadScene("LoadingScene");

      return joinCode;
  }
  ```
  > Unity Relay를 통해 서버와 클라이언트의 역할을 동시에 하는 호스트로서 방을 생성하고 생성한 방에 입장하기 위한 참여 코드를 발급합니다.
  <br>

#### 설계 의도
> 큐브좌표계를 활용해 거리를 계산하고 특정 타일 회피를 고려한 타일과 타일 간의<br>
최단 경로를 효율적으로 찾기 위해 길찾기에 특화된 A* 알고리즘을 구현했습니다.<br>
이를 이용해 유닛이 이동 가능한 타일들을 나타내는 기능과 서서히 이동하는 연출 등 타일과 관련된 기능들을 쉽게 구현할 수 있었습니다.

</details>
