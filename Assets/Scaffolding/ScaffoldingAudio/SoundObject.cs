using UnityEngine;
using System.Collections;
using System;

public class SoundObject {

	public AudioTrigger CurrentTrigger;

	private AudioManager _audioManager;
	private AudioSource _audioSource;
	private Action _completeCallback;
	private float _elapsedTime;
	private AudioVO _currentVO;
	private float _adjustedVolume = 1;
	private bool _breakFade;

	public void Setup(AudioManager manager)
	{
		_audioManager = manager;
		_audioSource = _audioManager.gameObject.AddComponent<AudioSource>();
		_audioSource.playOnAwake = false;
	}

	private void ResetObject(AudioVO vo)
	{
		CurrentTrigger = vo.Trigger;
		_currentVO = vo;
		_breakFade = false;
		_adjustedVolume = 1;
		_elapsedTime = 0;
		vo.DefinedVariation = UnityEngine.Random.Range(-vo.Variation, vo.Variation);
		vo.GlobalVolume = (vo.ClipVolume+vo.DefinedVariation);
		vo.CombinedVolume = vo.GlobalVolume;
	}

	public void Play(AudioVO vo, AudioClip clip)
	{
		ResetObject(vo);

		_audioSource.clip = clip;
		_audioSource.volume = vo.CombinedVolume;
		_audioSource.loop = vo.Loop;
		vo.Playing = true;
		_audioSource.Play();
	}

	public IEnumerator FadeIn(AudioVO vo, AudioClip clip, float duration)
	{
		Play(vo,clip);
		_adjustedVolume = 0;
		while( _adjustedVolume < 1 && _elapsedTime < clip.length && !_breakFade)
		{
			_adjustedVolume += Time.deltaTime / duration;
			yield return new WaitForEndOfFrame();
		}
	}

	public IEnumerator FadeOut(float duration)
	{
		_breakFade = true;
		while( _adjustedVolume > 0 && _elapsedTime < _audioSource.clip.length )
		{
			_adjustedVolume -= Time.deltaTime / duration;
			yield return new WaitForEndOfFrame();
		}

		Stop();
	}

	public void Stop()
	{
		_audioSource.Stop();
		_audioManager.AudioCompletedPlaying(this);
		_currentVO.Playing = false;
	}

	public bool AudioCompleted()
	{
		_elapsedTime += Time.deltaTime;
		return _elapsedTime > _audioSource.clip.length && !_currentVO.Loop;
	}

	public void AdjustVolume(float volume)
	{
		_currentVO.GlobalVolume = (_currentVO.ClipVolume+_currentVO.DefinedVariation);
		_currentVO.CombinedVolume = _currentVO.GlobalVolume * volume;
		_currentVO.CombinedVolume *= _adjustedVolume;
		_audioSource.volume = _currentVO.CombinedVolume;
	}
}
