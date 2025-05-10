using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GoogleLogin : MonoBehaviour
{
    [SerializeField] string userID;

	public Image image;
    public Text text;
    private bool loginAttempted = false; // 로그인 시도 여부를 저장하는 변수 추가

    public void SignIn()
    {
#if UNITY_EDITOR
		NetworkMaanger.instance.ServerLogin(UserPlatform.Android, AuthType.GooglePlay, userID, (res) => {

            DataManager.instance.sessionKey = res.data.session;

            if (res.result == ServerResult.Success)
            {
				//로그인 성공 씬 변경 
				ChangedLobbyScene(res.data);
			}
            else if (res.result == ServerResult.RequireNickname)
            {
                EventManager.instance.SendEvent(e_Event.NicknameUpdate);
            }
            else
            {
				Debug.Log("네트워크 통신 실패,,");
			}
		});
#else
        if (!loginAttempted)
        {
            // 첫 로그인 시도 시 자동 로그인
            PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
        }
        else
        {
            // 로그인 실패 후 다시 시도할 경우 강제 로그인 UI 띄우기
            PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
        }
#endif
	}

	internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            loginAttempted = false; // 로그인 성공하면 다시 자동 로그인 가능하도록 초기화
            string userId = PlayGamesPlatform.Instance.GetUserId();
            Debug.Log($"로그인 성공! userID: {userId}");

            image.color = Color.green;
            text.text = $"로그인 성공! userID: {userId}";
            
            NetworkMaanger.instance.ServerLogin(UserPlatform.Android, AuthType.GooglePlay, userId, (res) => {

				DataManager.instance.sessionKey = res.data.session;

				if (res.result == ServerResult.Success)
				{
                    //로그인 성공 씬 변경 
                    ChangedLobbyScene(res.data);
				}
				else if (res.result == ServerResult.RequireNickname)
				{
					EventManager.instance.SendEvent(e_Event.NicknameUpdate);
				}
				else
				{
					Debug.Log("네트워크 통신 실패,,");
				}
			});    
        }
        else
        {
            Debug.LogError($"로그인 실패! 오류 코드: {status}");

            image.color = Color.red;
            text.text = $"로그인 실패: {status}";

            // 로그인 실패 시 이후부터 `ManuallyAuthenticate()`를 사용하도록 설정
            loginAttempted = true;
            // Disable your integration with Play Games Services or show a login button
            // to ask users to sign-in. Clicking it should call
            // PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication).
        }

    }

    private void ChangedLobbyScene(LoginData data)
    {
        DataManager.instance.user_data = data;
        SceneManager.LoadScene("GameLobbyScene");
	}
}
