using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour {

	public Animator animator;

	private int levelToLoad;

    private void Start()
    {
		if (SceneManager.GetActiveScene().name == "SplashScreen")
		{
			Invoke("loginCheck",3);
		}
    }

	private void loginCheck()
    {
		FindObjectOfType<LoginCheck>().LogInCheck();
	}
	public void FadeToNextLevel ()
	{
		FadeToLevel(SceneManager.GetActiveScene().buildIndex + 1);
	}

	public void FadeToLevel (int levelIndex)
	{
		levelToLoad = levelIndex;
		animator.SetTrigger("FadeOut");
	}

	public void OnFadeComplete ()
	{
		SceneManager.LoadScene(levelToLoad);
	}
}
