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

#### 코드 [(전체 코드)](Scripts/Map/FindPath.cs)

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

#### 코드 [(전체 코드(매칭 관련))](Scripts/Network/LobbyManager.cs) [(전체 코드(네트워크 관련))](Scripts/Network/RelayNetwork.cs)

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
> 매칭되었을 때 서로 방을 생성해 방을 생성한 사람만 있고 들어가려는 사람은 없는 경우가 발생하지 않도록<br>
매칭 상태를 업데이트 할 때 매칭을 시작한 시간도 같이 업데이트해 비교할 수 있도록 설계했습니다.

---

</details>

<details><summary><b>턴 제어 및 RPC 기반 동기화</b></summary>

#### 코드 [(전체 코드)](Scripts/Game/GameManager.cs)

* 턴 제어
  ```csharp
  public void ChangeTurn(bool extraTurn, ulong senderId)
  {
      turnCount++;
      m_UI.PrintPrintTurn(turnCount);
    
      // 플레이어 상태 및 스킬 쿨타임 초기화
      m_Player.CheckSkillTurn();
      m_Player.SkillCoolDown();
      m_Player.ResetState();
    
      actionPoint = 2; // 행동력 초기화

      // 서버 권한으로 턴 제한 시간 및 턴 주체 변경
      if (IsServer)
      {
          turnTimer.Value = turnTime;
          isP1Turn.Value = !isP1Turn.Value;
      }

      if (extraTurn)
      {
          StartCoroutine(TakeTurn(senderId));
      }
  }
  ```
  > 서버이자 클라이언트의 역할을 하는 호스트인 유저가 턴을 제어합니다. 호스트인 유저는 자신의 턴이 종료되면 바로 메서드를 실행하고<br>
  호스트가 아닌 유저는 자신의 턴이 종료될 때 호스트인 유저에게 알려 턴 변경 메서드를 실행해줄 것을 요청합니다.
  <br>

* RPC 기반 동기화
  ```csharp
  [Rpc(SendTo.NotMe)]
  public void UnitMoveRpc(string unitID, Vector2Int destinationCoord)
  {
      Unit unit = m_Unit.EnemyUnitList[unitID];

      if (m_Player.EnemyControlUnit != unit)
      {
          m_Player.EnemyControlUnit = unit;
          unit.Selected(true);
      }
 
      Tile destination = m_Map.TileList[destinationCoord];
      StartCoroutine(unit.Move(destination));
  }

  [Rpc(SendTo.NotMe)]
  public void UnitAttackRpc(string unitID, Vector2Int tileCoord, float damageValue, bool _isMiss, bool _isNegation)
  {
      Unit attacker = m_Unit.EnemyUnitList[unitID];

      if (m_Player.EnemyControlUnit != attacker)
      {
          m_Player.EnemyControlUnit = attacker;
          attacker.Selected(true);
      }

      Tile targetTile = m_Map.TileList[tileCoord];

      Damage damage = new Damage
      {
          value = damageValue,
          isMiss = _isMiss,
          isNegation = _isNegation
      };

      StartCoroutine(attacker.Attack(targetTile, damage));
  }
  ```
  > RPC를 통해 서버에게 상대방의 게임 화면에 유닛 조작 상태를 동기화해줄 것을 요청합니다.<br>
  유닛 이동의 경우 이동하려는 유닛과 목적지 타일의 정보를 함께 넘겨주고<br>
  유닛 공격의 경우 공격하려는 유닛과 공격하려는 타일 좌표, 데미지 수치, 빗나감 여부, 무효화 여부를 함께 넘겨줍니다.
  <br>

#### 설계 의도
> 온라인 상에서 실시간으로 진행되는 게임인만큼 양쪽 플레이어에게 정확한 현재 턴과 제한 시간 등의 정보를 넘겨주기 위해<br>
서버 역할을 하는 호스트가 혼자 턴을 제어하도록 설계했습니다.<br>
유닛의 이동, 공격을 포함한 모든 동기화는 최소한의 비용으로 동기화 시키기 위해 꼭 필요한 정보만 넘기고<br>
연출은 각각의 컴퓨터에서 진행되도록 설계했습니다.

---

</details>

<details><summary><b>스킬 시스템</b></summary>

#### 코드 [(전체 코드(스킬 관련))](Scripts/Game/GlobalSkill.cs) [(전체 코드(스킬 정보 업데이트 관련))](Scripts/Game/SkillUpdater.cs)

* 대리자 기반 스킬 적용
  ```csharp
  public static void CheckDamageNegation(General general, SkillData skillData, int value, bool onoff)
  {
      foreach (Unit target in general.UnitList.Values)
      {
          if (onoff)
          {
              target.CheckDamageNegation += (damage) =>
              {
                  float ran = Random.Range(0f, 1f);

                  if ((ran * 100) <= value)
                  {
                      damage.value.value = 0.0f;
                      damage.value.isNegation = true;
                  }
              };

              target.AppliedSkill.Add(skillData);
          }

          else
          {
              target.CheckDamageNegation = null;
          }
      }
  }
  ```
  > **위 코드는 유닛이 피해를 받을 때 일정 확률로 피해를 무효화 시키는 능력을 부여해주는 스킬의 코드입니다.**<br>
  스킬을 활성화하면 각 유닛들이 가지고 있는 대리자에 스킬의 효과를 람다식으로 구독합니다.<br>
  (위 스킬로 예를 들면 확률 계산을 통해 데미지 무효화 여부를 결정하는 효과를 부여합니다.)<br>
  모든 스킬들은 장군이 사망하면 비활성화 되므로 장군들은 사망 시 유닛들의 스킬 효과 대리자 리스트에 있던 스킬 효과를 초기화합니다.
  <br>

* 구글 시트에서 스킬 정보 받아오는 메서드
  ```csharp
  void UpdateSkill()
  {
      string tsvData;

      try
      {
          using (WebClient client = new WebClient())
          {
              // 구글 시트 내용을 Tsv 형식으로 다운로드
              tsvData = client.DownloadString(sheetUrl);
          }
      }
      catch (WebException e)
      {
          Debug.LogError($"데이터 다운로드 실패: {e.Message}");
          return;
      }

      // 스킬 목록 불러오기
      Dictionary<string, SkillData> skillList = SkillFromGoogleSheet(tsvData);

      // 스킬 적용
      SetSkill(skillList);
  }
  ```
  > 구글 스프레드 시트의 스킬 정보들을 Tsv 형식으로 불러와 파싱합니다.<br>
  그 후 ScriptableObject 형태의 장군 데이터 중 스킬 정보에 반영해 갱신합니다.
  <br>

* 스킬 정보 업데이트
  ```csharp
  void SetSkill(Dictionary<string, SkillData> skillList)
  {
      // 폴더 내에 모든 장군 데이터 불러오기
      string[] guids = AssetDatabase.FindAssets("t:GeneralData", new[] { folderPath });

      foreach (string guid in guids)
      {
          // 실제 에셋 경로를 얻고 로드
          string assetPath = AssetDatabase.GUIDToAssetPath(guid);
          GeneralData generalData = AssetDatabase.LoadAssetAtPath<GeneralData>(assetPath);

          // 스킬 ID에 맞는 스킬 적용
          generalData.SkillData[0] = skillList[generalData.SkillData[0].SkillID];
          generalData.SkillData[1] = skillList[generalData.SkillData[1].SkillID];

          // 수정 된 상태 알림
          EditorUtility.SetDirty(generalData);
      }

      // 변경 사항 저장 + 최신화
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();
      Debug.Log("장군 스킬 업데이트 완료");
  }
  ```
  > 장군 데이터들을 저장해둔 폴더에서 각 장군마다 가진 스킬들의 정보를 구글 스프레드 시트에 저장된 정보와 동일하게 갱신합니다.<br>
  각 장군마다 가진 스킬의 ID만 직접 입력하면 나머지 정보들은 자동으로 갱신됩니다.
  <br>

#### 설계 의도
> 장군은 장군 본인도 스킬을 사용하면서 **스킬을 부여해주는 역할**이라는 점과 유닛은 **부여 받은 스킬을 사용하는 역할**이라는 점을 명확하게 구분하고 싶었습니다. 이를 위해 유닛들에게 스킬 효과를 담을 대리자를 선언하고 장군은 그 대리자에 스킬 효과를 구독, 유닛은 대리자를 실행하는 방식으로 설계했습니다.<br>
스킬의 데이터는 구글 스프레드 시트를 통해 관리하고 있었습니다. 하지만 기획이 변경될 때 마다 유니티 내에서 일일이 직접 데이터를 수정하는 것이 번거로웠습니다. 이를 해결하기 위해 버튼 클릭만으로 편하게 최신화 할 수 있도록 외부 확장 툴을 구현했습니다.

</details>
