using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class BGMManager : MonoBehaviour
{
    // 싱글톤
    public static BGMManager instance;

    // 오디오 클립
    public AudioClip[] audioClip;

    // 오디오 소스 저장할 딕셔너리
    public Dictionary<string, AudioSource> audioSource = new Dictionary<string, AudioSource>();

    // 현재 재생 중인 배경음 이름
    [SerializeField] string nowPlayingBGM;

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
            source.loop = true;
            source.volume = 0.5f;

            // 이름으로 딕셔너리, 리스트에 저장
            audioSource[clip.name] = source;
        }
    }

    // 배경음 재생
    public void PlayBGM(string name)
    {
        if (!nowPlayingBGM.IsNullOrEmpty())
        {
            if (audioSource.TryGetValue(nowPlayingBGM, out var nowSource))
            {
                nowSource.Stop();
            }
        }

        if (audioSource.TryGetValue(name, out var newSource))
        {
            newSource.Play();
            nowPlayingBGM = name;
        }
    }
}
