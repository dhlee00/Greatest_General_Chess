using TMPro;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    // 배경음
    [Header("BGM")]
    [SerializeField] Slider bgmSlider;
    [SerializeField] TextMeshProUGUI bgmVolumeTxt;

    [Header("SFX")]
    [SerializeField] Slider sfxSlider;
    [SerializeField] TextMeshProUGUI sfxVolumeTxt;
    float lastValue;

    // 배경음 조절
    public void SettingBGM()
    {
        foreach (AudioSource audio in BGMManager.instance.audioSource.Values)
        {
            audio.volume = bgmSlider.value;
            bgmVolumeTxt.text = $"{(bgmSlider.value * 100).ToString("F0")}%";
        }
    }

    // 효과음 조절
    public void SettingSFX()
    {
        foreach (AudioSource audio in SFXManager.instance.audioSource.Values)
        {
            audio.volume = sfxSlider.value;
            sfxVolumeTxt.text = $"{(sfxSlider.value * 100).ToString("F0")}%";
        }

        // 볼륨 값이 10% 씩 변할 때 마다 효과음 재생
        if (Mathf.Abs(lastValue - Mathf.Ceil(sfxSlider.value * 10)) >= 1.0f)
        {
            lastValue = Mathf.Ceil(sfxSlider.value * 10);

            SFXManager.instance.PlaySFX("SFX_ST_ATTACK");
        }
    }

    public void SettingBtn(GameObject settingView)
    {
        settingView.SetActive(!settingView.activeSelf);
    }

    public async void EndGameBtn()
    {
        if (!string.IsNullOrEmpty(LobbyManager.instance.MainLobby.Id) && !string.IsNullOrEmpty(LobbyManager.instance.PlayerId))
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(LobbyManager.instance.MainLobby.Id, LobbyManager.instance.PlayerId);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e.Message);
            }
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(); // 실제 빌드된 게임 종료
#endif
    }
}
