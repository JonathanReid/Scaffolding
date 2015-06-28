using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System;
using UnityEngine.Audio;

public class AudioConfig : ScriptableObjectCreator<AudioConfig> {

	public List<AudioGroupVO> SFXGroups;
	public float GlobalVolume = 1;
	public AudioMixer Mixer;
}

[Serializable]
public class AudioGroupVO
{
	public AudioTrigger Trigger;
	public AudioChannel Channel;
	public bool Expanded = true;
	public List<AudioVO> Clips;
}

[Serializable]
public class AudioVO
{
	public AudioTrigger Trigger;
	public string Clip;
	public float ClipVolume = 1;
	public float GlobalVolume;
	public float CombinedVolume;
	public float Variation;
	public bool PlayOnAwake;
	public bool Loop;
	public bool Playing = false;
	public float DefinedVariation;
}