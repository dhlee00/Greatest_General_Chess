using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class SkillUpdater : EditorWindow
{
    // 맵 디자인 구글 시트 주소
    string sheetUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vQksIif49bwK6XxJJICjCrhWh29rZ6BwcmGXbZy-7HyCoAZSfmW2HiYf0wcx7m943rulvPdw5pfQLRz/pub?output=tsv";

    // 저장 경로
    string folderPath = "Assets\\Resources\\GeneralData";

    [MenuItem("Tools/Skill Update")]
    public static void ShowWindow()
    {
        GetWindow<SkillUpdater>("Skill Update");
    }

    void OnGUI()
    {
        GUILayout.Label("Skill Update", EditorStyles.boldLabel);

        if (GUILayout.Button("Update General Skill"))
        {
            UpdateSkill();
        }
    }

    // 구글 시트에서 장군 스킬 정보 받아와서 적용
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

    // 스킬 리스트들 파싱해서 반환
    Dictionary<string, SkillData> SkillFromGoogleSheet(string tsvText)
    {
        Dictionary<string, SkillData> list = new Dictionary<string, SkillData>();
        StringReader reader = new StringReader(tsvText);

        // 줄 단위로 저장
        string[] lines = reader.ReadToEnd().Split('\n');

        // 줄 개수만큼 반복
        for (int y = 1; y < lines.Length; y++)  // 맨 윗줄 빼고
        {
            // 셀 단위로 저장
            string[] cells = lines[y].Split('\t');

            // 스킬 데이터 생성
            SkillData skillData = new SkillData();

            // 스킬 아이디
            skillData.SkillID = cells[0];

            // 스킬 이름
            skillData.SkillName = cells[1];

            // 발동 타이밍
            switch (cells[2])
            {
                case "게임 시작 시": skillData.ActivateTiming = ActivateTiming.GameStart; break;
                case "발동 시": skillData.ActivateTiming = ActivateTiming.Activate; break;
                case "수적 열세 시": skillData.ActivateTiming = ActivateTiming.OutNumbered; break;
            }

            // 스킬 대상
            switch (cells[3])
            {
                case "전역": skillData.SkillTarget = SkillTarget.Global; break;
                case "아군 유닛": skillData.SkillTarget = SkillTarget.AllyUnit; break;
                case "중장기병": skillData.SkillTarget = SkillTarget.HeavyCavalry; break;
                case "트레뷰셋": skillData.SkillTarget = SkillTarget.Trebuchet; break;
            }

            // 범위 값
            skillData.RangeValue = int.Parse(cells[4]);

            // 지속 턴
            skillData.DurationTurn = int.Parse(cells[5]);

            // 스킬 리스트 추가
            list.Add(skillData.SkillID, skillData);
        }

        return list;
    }

    // 장군 스킬 적용
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
}
#endif
