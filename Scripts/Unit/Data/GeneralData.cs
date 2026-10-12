using UnityEngine;

[System.Serializable]
public class GeneralImage
{
    public Sprite profileImg;
    public Sprite introduceImg;
    public Sprite illustrationImg;
}

[CreateAssetMenu(fileName = "New General", menuName = "Game Data/Unit/General Data")]
public class GeneralData : UnitData
{
    [Header("")]

    // 장군 코드
    [SerializeField] string generalCode;
    public string GeneralCode => generalCode;

    // 장군 이름
    [SerializeField] string generalName;
    public string GeneralName => generalName;

    // 장군 전체 이름
    [SerializeField] string generalFullName;
    public string GeneralFullName => generalFullName;

    // 장군 프로필 이미지
    [SerializeField] GeneralImage generalImage;
    public GeneralImage GeneralImage => generalImage;

    // 장군 선전포고 멘트
    [SerializeField][TextArea(1, 3)] string generalMent;
    public string GeneralMent => generalMent;

    // 장군 일대기
    [SerializeField][TextArea(1, 10)] string generalBiography;
    public string GeneralBiography => generalBiography;

    [Header("")]

    // 스킬 데이터
    [SerializeField] SkillData[] skillData = new SkillData[2];
    public SkillData[] SkillData { get => skillData; set => skillData = value; }
}
