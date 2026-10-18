using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    // 싱글톤
    public static SFXManager instance;

    // 오디오 클립
    public AudioClip[] audioClip;

    // 오디오 소스 저장할 딕셔너리
    public Dictionary<string, AudioSource> audioSource = new Dictionary<string, AudioSource>();

    void Awake()
    {
        // 인스턴스에 값이 없으면 자기자신 할당
        if (instance == null)
        {
            instance = this;

            // 사라지지 않음
            DontDestroyOnLoad(gameObject);
        }

        // 인스턴스에 값이 있으면 삭제
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        foreach (var clip in audioClip)
        {
            // 오디오 소스 생성
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.playOnAwake = false;
            source.volume = 0.5f;

            if (clip.name == "SFX_INF_MOVE" || clip.name == "SFX_MU_MOVE" || clip.name == "SFX_TRE_MOVE")
            {
                source.loop = true;
            }

            else
            {
                source.loop = false;
            }

            // 이름으로 딕셔너리, 리스트에 저장
            audioSource[clip.name] = source;
        }
    }

    // 효과음 재생
    public void PlaySFX(string name)
    {
        if (audioSource.TryGetValue(name, out var source))
        {
            source.Play();
        }
    }

    // 효과음 종료
    public void StopSFX(string name)
    {
        if (audioSource.TryGetValue(name, out var source))
        {
            source.Stop();
        }
    }
}
