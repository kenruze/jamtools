using UnityEngine;
using System.Collections;

public class EffectType : ScriptableObject
{
	public float lifeSpan = 1;

	public GameObject prefab;
	[Range (0, 1)]
	public float prefabQueue;
	public bool disableLoopingParticles = true;
	[Space (10)]
	public AudioClip sound;
	[Range (0, 1)]
	public float soundQueue;
	public EffectAudioGroupType audioGroupType;
	[Space (9)]
	[Range (0, 1)]
	public float volumeAdjust = 1;
	[Space (6)]
	[Range (-2, 2)]
	public float pitchOffsetMin;
	[Range (-2, 2)]
	public float pitchOffsetMax;
	[Space (6)]
	[Range (0, 1)]
	public float spatialBlend;
	[Range (0, 1)]
	public float spread;

	[Space (15)]
	public bool screenShake;
	public AnimationCurve screenShakeCurve;

	[Space (15)]
	public EffectType subEffect;
	[Range (0, 1)]
	public float subEffectQueue;

	[Space (15)]
	[Tooltip("off by default for danger of losing AudioSource on unrelated Destroy call")]
	public bool allowAudioSourceAttachToParent;
	[Tooltip("can be turned off to force world space sort of effect. overrides pooled instance setting when off")]
	public bool allowInstanceAttachToParent = true;
	[Tooltip("off by default for danger of losing pooled instance on unrelated Destroy call")]
	public bool allowPooledInstanceAttachToParent;

	public void PlayClipThroughAudioSource (AudioSource audioSource)
	{
		audioSource.clip = sound;
		audioSource.volume = volumeAdjust;
		audioSource.pitch = 1 + Random.Range (pitchOffsetMin, pitchOffsetMax);
		audioSource.spatialBlend = spatialBlend;
		audioSource.spread = spread;
		audioSource.Play ();
	}
}

public enum EffectAudioGroupType
{
	GeneralEffect,
	Footstep,
	Ambient,
}
