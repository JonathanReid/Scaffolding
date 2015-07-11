using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {

	public AudioMixer mixer;
	public int InitialCapacity = 15;
	public float GlobalVolume = 1;
	private Stack<SoundObject> _availableSounds;
	private List<SoundObject> _playingSounds;
	private AudioConfig _config;
	private Dictionary<AudioVO, AudioClip> _loadedAudio;

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

		_availableSounds = new Stack<SoundObject>();
		_playingSounds = new List<SoundObject>();
		_loadedAudio = new Dictionary<AudioVO, AudioClip>(0);

		CreatePooledObjects();

		foreach(AudioGroupVO vo in _config.SFXGroups)
		{
			foreach(AudioVO v in vo.Clips)
			{
				v.Playing = false;
			}
		}

		if(_config.PlayBackgroundMusicOnLoad)
		{
			PlayBackgroundMusic(AudioTrigger.BackgroundMusic);
		}
	}

	private void OnDestroy()
	{
		_loadedAudio = new Dictionary<AudioVO, AudioClip>();
		for(int i = 0; i < _availableSounds.Count; ++i)
		{
			SoundObject obj = _availableSounds.Pop();
			obj = null;
		}

		for(int i = 0; i < _playingSounds.Count; ++i)
		{
			_playingSounds[i] = null;
		}
	}

	private SoundObject GetAvailableSoundObject()
	{
		SoundObject sound = null;
		if(_availableSounds.Count > 0)
		{
			sound = _availableSounds.Pop();
		}

		if(sound == null)
		{
			sound = new SoundObject();
			sound.Setup(this);
		}

		_playingSounds.Add(sound);

		return sound;
	}

	private SoundObject GetPlayingSoundObject(AudioTrigger trigger)
	{
		foreach(SoundObject obj in _playingSounds)
		{
			if(obj.CurrentTrigger == trigger)
			{
				return obj;
			}
		}
		return null;
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

	private void CreatePooledObjects()
	{
		for(int i = 0; i < InitialCapacity; i++)
		{
			SoundObject obj = new SoundObject();
			obj.Setup(this);
			_availableSounds.Push(obj);
		}
	}

	public void AudioCompletedPlaying(SoundObject obj)
	{
		_playingSounds.Remove(obj);
		_availableSounds.Push(obj);
	}

	public void Play(AudioTrigger trigger)
	{
		AudioVO vo = GetSoundForTrigger(trigger);
		SoundObject sound = GetAvailableSoundObject();
		sound.Play(vo,_loadedAudio[vo]);
	}

	public void PlayLooped(AudioTrigger trigger)
	{
		AudioVO vo = GetSoundForTrigger(trigger);
		SoundObject sound = GetAvailableSoundObject();
		vo.Loop = true;
		sound.Play(vo,_loadedAudio[vo]);
	}
	
	public void PlayClipFadeIn(AudioTrigger trigger, float duration)
	{
		FadeInClip(trigger,duration,false);
	}

	public void StopClipFadeOut(AudioTrigger trigger, float duration)
	{
		FadeOutClip(trigger, duration);
	}

	public void PlayBackgroundMusic(AudioTrigger trigger)
	{
		AudioVO vo = GetSoundForTrigger(trigger);
		if(!vo.Playing)
		{
			PlayLooped(trigger);
		}
	}

	public void FadeInBackgroundMusic(AudioTrigger trigger, float duration)
	{
		FadeInClip(trigger,duration,true);
	}

	public void FadeOutBackgroundMusic(AudioTrigger trigger, float duration)
	{
		FadeOutClip(trigger, duration);
	}

	private void FadeInClip(AudioTrigger trigger, float duration, bool loop)
	{
		AudioVO vo = GetSoundForTrigger(trigger);
		if(!vo.Playing)
		{
			SoundObject sound = GetAvailableSoundObject();
			vo.Loop = loop;
			StartCoroutine(sound.FadeIn(vo,_loadedAudio[vo],duration));
		}
	}

	private void FadeOutClip(AudioTrigger trigger, float duration)
	{
		SoundObject sound = GetPlayingSoundObject(trigger);
		if(sound != null)
		{
			StartCoroutine(sound.FadeOut(duration));
		}
	}

	public void CrossFadeBackgroundMusic()
	{

	}

	public void LoopClip()
	{

	}

	public void ChangeClipVolume()
	{

	}

	public void ChangeGlobalVolume()
	{

	}

	private float _previousGlobalVolume;
	private bool _muted;
	public void ToggleMute(bool mute)
	{
		if(mute)
		{
			_previousGlobalVolume = _config.GlobalVolume;
			_config.GlobalVolume = 0;
		}
		else
		{
			_config.GlobalVolume = _previousGlobalVolume;
		}
		_muted = mute;
	}

	public bool IsAudioMuted()
	{
		return _muted;
	}

	public void FadeGlobalVolume()
	{

	}

	public void Stop(AudioTrigger trigger)
	{
		SoundObject sound = GetPlayingSoundObject(trigger);
		if(sound != null)
		{
			sound.Stop();
		}
	}
	
	private void Update()
	{
		for(int i = 0; i < _playingSounds.Count; ++i)
		{
			if(_playingSounds[i].AudioCompleted())
			{
				_playingSounds[i].Stop();
			}
			else
			{
				_playingSounds[i].AdjustVolume(_config.GlobalVolume);
			}
		}
	}
}
