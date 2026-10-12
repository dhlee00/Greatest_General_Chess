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

<details><summary><b>턴제 전투 시스템</b></summary>

<img width="654" height="368" alt="KakaoTalk_20261013_000215427_01" src="https://github.com/user-attachments/assets/84712be1-cc36-4af3-9064-a1fdd5d20d9d" />
<img width="654" height="368" alt="KakaoTalk_20261013_000215427" src="https://github.com/user-attachments/assets/038a5d19-d5e1-4c73-a0d8-c4d11883bb03" />


#### 코드 [(전체 코드)](Scripts/Battle/BattleManager.cs)

* 전투 메인 루프
  ```csharp
  void Update()
  {
      // 스페이스바, 마우스 클릭하면 진행
      if (bt.gameObject.activeSelf && !bt.isPrint && !selectView.activeSelf && !catchMotion &&
          skillEffect.childCount <= 0 && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
      {
          progressNum++;

          // 전투 돌입 / 승리 / 패배 등 공통 상태 처리
          if (state == BattleState.battleEnter) { BattleEnter(); }
          else if (state == BattleState.win) { Win(); }
          else if (state == BattleState.lose) { Lose(); }

          // 플레이어 턴일 때의 상태별 분기
          if (turn == Turn.player)
          {
              switch (state)
              {
                  case BattleState.selectAction: SelectAction(); break;
                  case BattleState.Attack: SkillAttack(); break;
                  case BattleState.useBall: UseBall(); break;
                  case BattleState.useMedi: UseMedi(); break;
                  case BattleState.switchPokemon: SwitchPokemon(); break;
                  case BattleState.run: Run(); break;
              }
          }

          // 적 턴일 때의 분기
          else if (turn == Turn.enemy)
          {
              if (state == BattleState.Attack) { SkillAttack(); }
          }
      }
  }
  ```
  > 플레이어의 턴과 적의 턴, 그리고 전투 상태(BattleState)에 따라 스페이스바 또는 마우스 클릭을 통해 전투를 진행하는 방식입니다.
  <br>

* 턴 전환 및 상태 초기화
  ```csharp
  // 상태 변경 메서드
  public void ChangeState(BattleState changeState)
  {
      state = changeState;
      progressNum = -1;
  }

  // 턴 변경 메서드
  public void ChangeTurn()
  {
      // 적 턴으로 변경
      if (turn == Turn.player)
      {
          turn = Turn.enemy;
          ChangeState(BattleState.Attack);
      }

      // 플레이어 턴으로 변경
      else if (turn == Turn.enemy)
      {
          turn = Turn.player;
          ChangeState(BattleState.selectAction);
      }
  }
  ```
  > 현재 플레이어의 턴인지 적의 턴인지 확인 후 턴을 변경합니다. 턴 변경 시 현재 전투 상태와 진행도를 초기화합니다.
  <br>
  
* 공격 처리
  ```csharp
  void SkillAttack()
  {
      int accuracy = Random.Range(1, 101);

      // 명중
      if (accuracy <= MyPokemon.pokemon[0].skill[skillNum].skill_Accuracy)
      {
          enemyAnim.Play("EnemyDamage");

          // 30% 확률로 급소공격
          int critical = Random.Range(1, 11);

          if (critical <= 3)
          {
              bt.PrintBattleInfo("급소에 맞았다!");
              TakeDamage(enemyPokemon, Damage(MyPokemon.pokemon[0], skillNum, enemyPokemon, true));
          }

          else
          {
              bt.PrintBattleInfo("공격이 명중했다!");
              TakeDamage(enemyPokemon, Damage(MyPokemon.pokemon[0], skillNum, enemyPokemon));
          }
      }

      // 빗나감
      else if (accuracy > MyPokemon.pokemon[0].skill[skillNum].skill_Accuracy)
      {
          bt.PrintBattleInfo("공격이 빗나갔다!");
      }
  }
  ```
  > 스킬의 명중률을 기반으로 1~100 사이의 난수를 통해 적중 여부를 판정합니다.<br>
  공격이 명중했을 경우 30% 확률로 급소 판정을 거쳐 추가 데미지를 부여합니다.
  <br>

* 데미지 산출 방식
  ```csharp
  float Damage(Pokemon attacker, int skillNum, Pokemon defender, bool critical = false)
  {
      // (공격력 * 기술 위력)의 85% - (방어력의 85%)
      float damage = ((attacker.attack * attacker.skill[skillNum].skill_Damage) * 0.85f) - (defender.defence * 0.85f);

      // 급소 공격 시 30% 추가
      if (critical) damage *= 1.3f;

      // 최종 데미지는 반올림
      damage = Mathf.RoundToInt(damage);

      // 최소 데미지 3
      return Mathf.Max(3, damage);
  }
  ```
  > 공격자의 공격력과 기술 위력을 곱한 값의 85%에서 피격자의 방어력의 85%를 차감하여 기본 데미지를 산출합니다.<br>
  급소 공격 성공 시 기본 데미지의 30%만큼 추가 보정이 적용되며 최종 데미지는 반올림 처리합니다.<br>
  또한 최소 3의 데미지가 보장되도록 설정했습니다.
  <br>

#### 설계 의도
> 전투 흐름에서 '현재 누구의 턴이고 어떤 행동을 하는지'와 '대사 출력과 연출'을 따로 관리해서 자연스럽게 전투가 흘러가도록 설계했습니다.<br>
데미지 공식은 지속적인 테스트를 통해 산출했고 방어형 포켓몬으로 전투를 진행할 때 낮은 데미지로 인해 전투가 지루해지는 느낌이 들지 않도록<br>
최소 데미지를 3으로 설정했습니다.

---

</details>

<details><summary><b>포획</b></summary>

<img width="654" height="368" alt="KakaoTalk_20261012_235034714_01" src="https://github.com/user-attachments/assets/3571fe05-139a-433b-a035-25348010c3ee" />
<img width="654" height="368" alt="KakaoTalk_20261012_235034714" src="https://github.com/user-attachments/assets/1a58f9af-031d-402d-9584-7f5af7757ea0" />

#### 코드 [(전체 코드)](Scripts/Battle/MonsterBall_Catch.cs)

* 체력 비례 포획 확률 산출
  ```csharp
  bool CanCatch()
  {
      float hpPercent = (BattleManager.instance.enemyPokemon.nowHP / BattleManager.instance.enemyPokemon.maxHP) * 100f;
      float catchProb;

      if (hpPercent <= 10f)
      {
          catchProb = 100f;
      }

      else if (hpPercent >= 90f)
      {
          catchProb = 0f;
      }

      else
      {
          catchProb = Mathf.Clamp01((90f - hpPercent) / 80f) * 100f;
      }

      int ran = Random.Range(1, 101);
      if (ran <= (int)catchProb)
      {
          return true;
      }

      else
      {
          return false;
      }
  }
  ```
  > 포켓몬의 체력의 비례 해 체력이 낮을 수록 포획 성공 확률이 올라가는 방식입니다.<br>
  포켓몬의 체력이 90% 이상 10% 이하일 때 각각 0%와 100%로 포획 확률을 고정합니다.
  <br>
  
#### 설계 의도
> 원작처럼 포켓몬의 체력에 비례해 포획률이 달라지게 설계했습니다. 또한 충분히 낮은 체력임에도 불구하고 낮은 확률로<br>
포획에 실패하는 것을 방지하기 위해 체력이 일정 비율 이하일 때 반드시 포획할 수 있도록 설계했습니다.
</details>
