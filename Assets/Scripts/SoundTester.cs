using UnityEngine;
using System.Collections;

public class SoundTester : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Space))
		{
			AudioManager.Instance.PlaySound(AudioTrigger.SFX);
		}

		if(Input.GetKeyDown(KeyCode.Alpha1))
		{
			AudioManager.Instance.PlayBackgroundMusic(AudioTrigger.BackgroundMusic);
		}

		if(Input.GetKeyDown(KeyCode.Alpha2))
		{
			AudioManager.Instance.FadeInBackgroundMusic(AudioTrigger.BackgroundMusic,3);
		}

		if(Input.GetKeyDown(KeyCode.Alpha3))
		{
			AudioManager.Instance.FadeOutBackgroundMusic(3);
		}

		if(Input.GetKeyDown(KeyCode.Escape))
		{
			AudioManager.Instance.ToggleMute(!AudioManager.Instance.IsAudioMuted());
		}

		if(Input.GetKeyDown(KeyCode.LeftArrow))
		{
			AudioManager.Instance.GetBackgroundMusic().audioSource.panStereo -= 0.1f;
		}

		if(Input.GetKeyDown(KeyCode.RightArrow))
		{
			AudioManager.Instance.GetBackgroundMusic().audioSource.panStereo += 0.1f;
		}
	}
}
