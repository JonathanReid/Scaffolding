using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

[RequireComponent(typeof(SoundKit))]
public class AudioManager : MonoBehaviour {

	private AudioConfig _config;
	private Dictionary<AudioVO, AudioClip> _loadedAudio;
	private SoundKit _soundKit;

	private static AudioManager _instance;
	public static AudioManager Instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = FindObjectOfType<AudioManager>();
				if(_instance == null)
				{
					GameObject go = new GameObject();
					_instance = go.AddComponent<AudioManager>();
					go.name = "AudioManager";
				}
			}
			return _instance;
		}
	}

	private void Awake()
	{
		_config = AudioConfig.Get ("SCAudioConfig.asset","ScaffoldingAudio/Resources");

		_loadedAudio = new Dictionary<AudioVO, AudioClip>(0);
		_soundKit = SoundKit.instance;

		foreach(AudioGroupVO vo in _config.SFXGroups)
		{
			foreach(AudioVO v in vo.Clips)
			{
				v.Playing = false;
			}
		}

		PlayBackgroundMusic(AudioTrigger.BackgroundMusic);
	}

	private void OnDestroy()
	{
		_loadedAudio = new Dictionary<AudioVO, AudioClip>();
	}

	private AudioVO GetSoundForTrigger(AudioTrigger trigger)
	{
		AudioVO audio = null;
		foreach(AudioGroupVO vo in _config.SFXGroups)
		{
			if(vo.Trigger == trigger)
			{
				audio = vo.Clips[Random.Range(0,vo.Clips.Count)];
				break;
			}
		}

		if(!_loadedAudio.ContainsKey(audio))
		{
			_loadedAudio.Add(audio,Resources.Load<AudioClip>(audio.Clip));
		}

		return audio;
	}

	public SoundKit.SKSound PlaySound(AudioTrigger trigger)
	{
		AudioVO vo = GetSoundForTrigger(trigger);
		return _soundKit.playSound(_loadedAudio[vo],vo.ClipVolume + Random.Range(-vo.Variation,vo.Variation));
	}

	public SoundKit.SKSound PlaySoundLooped(AudioTrigger trigger)
	{
		AudioVO vo = GetSoundForTrigger(trigger);
		return _soundKit.playSoundLooped(_loadedAudio[vo]);
	}

	public void PlayBackgroundMusic(AudioTrigger trigger)
	{
		AudioVO vo = GetSoundForTrigger(trigger);
		_soundKit.playBackgroundMusic(_loadedAudio[vo],vo.ClipVolume);
	}

	public SoundKit.SKSound GetBackgroundMusic()
	{
		return _soundKit.backgroundSound;
	}

	public void PlayOneShot(AudioTrigger trigger)
	{
		AudioVO vo = GetSoundForTrigger(trigger);
		_soundKit.playOneShot(_loadedAudio[vo],vo.ClipVolume);
	}

	public SoundKit.SKSound PlayPannedSound(AudioTrigger trigger)
	{
		AudioVO vo = GetSoundForTrigger(trigger);
		return _soundKit.playPannedSound(_loadedAudio[vo],vo.Pan);
	}

	public SoundKit.SKSound PlayPitchedSound(AudioTrigger trigger)
	{
		AudioVO vo = GetSoundForTrigger(trigger);
		return _soundKit.playPitchedSound(_loadedAudio[vo],vo.Pitch);
	}

	public void FadeOutBackgroundMusic(float duration)
	{
		_soundKit.backgroundSound.fadeOutAndStop(duration);
	}

	public void FadeInBackgroundMusic(AudioTrigger trigger, float duration)
	{
		AudioVO vo = GetSoundForTrigger(trigger);
		_soundKit.playBackgroundMusic(_loadedAudio[vo],0);
		_soundKit.backgroundSound.fadeInMusic(duration, vo.ClipVolume);
	}

	public void ToggleMute(bool toggle)
	{
		AudioListener.pause = !AudioListener.pause;
	}

	public bool IsAudioMuted()
	{
		return AudioListener.pause;
	}

	private void Update()
	{
		AudioListener.volume = _config.GlobalVolume;
	}
}
