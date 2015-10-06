using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JamTools
{
	public class EffectOutlet : MonoBehaviour
	{
		//USAGE: drop one of these in the scene
		//pass a reference to an EffectType asset into EffectOutlet.Get.PlayEffect

		static EffectOutlet instance;
		public static EffectOutlet Get{ get { return instance; } }

		public bool logsOn = true;
		[EnumList(typeof(EffectAudioGroupType))]
		public List<EffectAudioSourcePool> effectAudioSourceGroups = new List<EffectAudioSourcePool>();
		public List<EffectInstancePool> effectInstancePools = new List<EffectInstancePool> ();

		ScreenShakeCamera screenShakeCamera;

		void Awake()
		{
			instance = this;
		}

		void Start()
		{
			for (int i = 0; i < effectInstancePools.Count; i++)
			{
				effectInstancePools [i].InstantiatePool (this);
			}

			//if the audio sources for effect audio groups are not set up they can have a default source
			//set up the groups to manage priority on which effects can interrupt eachother
			AudioSource groupNotSetUpSource = null;
			int groups = System.Enum.GetValues (typeof(EffectAudioGroupType)).Length;
			for (int i = 0; i < groups; i++)
			{
				if (effectAudioSourceGroups.Count <= i)
					effectAudioSourceGroups.Add (new EffectAudioSourcePool ());
				if (effectAudioSourceGroups[i].pool == null)
					effectAudioSourceGroups [i].pool = new List<AudioSource> ();
				if (effectAudioSourceGroups [i].pool.Count <= 0)
				{
					if (groupNotSetUpSource == null)
					{
						if (logsOn)
						{
							Debug.Log ("creating default effect AudioSource");
						}
						groupNotSetUpSource = new GameObject ("defaultEffectAudioSource").AddComponent<AudioSource> ();
						groupNotSetUpSource.transform.parent = transform;
						groupNotSetUpSource.transform.localPosition = Vector3.zero;
					}
					effectAudioSourceGroups [i].pool.Add (groupNotSetUpSource);
				}
			}

			screenShakeCamera = FindObjectOfType<ScreenShakeCamera> ();
		}

		public void PlayEffect(EffectType effect, Vector3 position, Quaternion orientation = default(Quaternion), Transform parent = null)
		{
			for (int i = 0; i < effectInstancePools.Count; i++) {
				if (effectInstancePools [i].effect == effect)
				{
					StartCoroutine(PlayEffectTimeline (effect, position, orientation, parent, effectInstancePools [i]));
					return;
				}
			}
			StartCoroutine(PlayEffectTimeline (effect, position, orientation, parent));
		}

		IEnumerator PlayEffectTimeline(EffectType effect, Vector3 position, Quaternion orientation = default(Quaternion), Transform parent = null, EffectInstancePool pool = null)
		{
			var frame = new WaitForEndOfFrame ();
			bool instanceFired = effect.prefab == null;
			bool soundFired = effect.sound == null;
			bool subEffectFired = effect.subEffect == null;

			if (effect.screenShake && screenShakeCamera != null)
			{
				screenShakeCamera.AddShake (effect.screenShakeCurve);
			}

			GameObject effectInstance = null;

			for (float t = 0; t <= 1; t+=Time.deltaTime/effect.lifeSpan)
			{
				if (!instanceFired && t > effect.prefabQueue)
				{
					instanceFired = true;
					if (pool == null)
					{
						if (logsOn)
							Debug.Log ("instantiating effect");
						effectInstance = Instantiate (effect.prefab, position, orientation) as GameObject;
						effectInstance.transform.parent = ( !effect.allowInstanceAttachToParent || parent == null) ? transform : parent;
						if(effect.disableLoopingParticles)
						{
							var particles = effectInstance.GetComponentInChildren<ParticleSystem> ();
							if (particles != null)
								particles.loop = false;
						}
					}
					else
					{
						effectInstance = pool.FireNext (position, orientation, parent);
					}
				}
				if (!soundFired && t >= effect.soundQueue)
				{
					if (logsOn)
						Debug.Log ("playing sound");
					soundFired = true;
					AudioSource audioSource = effectAudioSourceGroups[(int)effect.audioGroupType].TakeNext ();
					audioSource.transform.position = position;
					if (effect.allowAudioSourceAttachToParent && parent != null)
						audioSource.transform.parent = parent;
					else
						audioSource.transform.parent = transform;
					effect.PlayClipThroughAudioSource (audioSource);
				}
				if (!subEffectFired && t > effect.subEffectQueue)
				{
					subEffectFired = true;
					PlayEffect (effect.subEffect, position, orientation, parent);
				}
					
				yield return frame;
			}

			if (effectInstance != null)
			{
				if (pool == null)
					Destroy (effectInstance);
				else
					effectInstance.SetActive (false);
			}
		}
	}

	[System.Serializable]
	public class EffectAudioSourcePool
	{
		public List<AudioSource> pool;
		internal int next;

		public AudioSource TakeNext()
		{
			if (pool.Count > 0 && next < pool.Count)
			{
				var nextEntry = pool [next];
				next++;
				if (next >= pool.Count)
					next = 0;
				return nextEntry;
			}
			else
			{
				next = 0;
			}
			return null;
		}
	}

	[System.Serializable]
	public class EffectInstancePool
	{
		public EffectType effect;
		public int capacity = 10;
		Pool<GameObject> pool;
		EffectOutlet owner;

		public void InstantiatePool(EffectOutlet owner)
		{
			this.owner = owner;
			pool = new Pool<GameObject> ();
			pool.pool = new List<GameObject> ();
			for (int i = 0; i < capacity; i++)
			{
				GameObject effectClone = GameObject.Instantiate (effect.prefab) as GameObject;
				effectClone.transform.parent = owner.transform;
				if(effect.disableLoopingParticles)
				{
					var particles = effectClone.GetComponentInChildren<ParticleSystem> ();
					if (particles != null)
						particles.loop = false;
				}
				effectClone.SetActive (false);
				pool.pool.Add (effectClone);
			}
		}

		public GameObject FireNext(Vector3 position, Quaternion orientation, Transform parent)
		{
			if (owner.logsOn)
				Debug.Log ("activating pooled instance");
			GameObject nextEffectInstance = pool.TakeNext ();
			nextEffectInstance.transform.position = position;
			nextEffectInstance.transform.rotation = orientation;
			if( effect.allowInstanceAttachToParent && effect.allowPooledInstanceAttachToParent && parent != null)
				nextEffectInstance.transform.parent = parent;
			else
				nextEffectInstance.transform.parent = owner.transform;
			var particles = nextEffectInstance.GetComponentInChildren<ParticleSystem> ();
			if (particles != null)
			{
				particles.time = 0;
				particles.Play ();
			}
			nextEffectInstance.SetActive (true);

			return nextEffectInstance;
		}
	}

	public class Pool<T>
	{
		public List<T> pool;
		public int next;

		public T TakeNext()
		{
			if (pool.Count > 0 && next < pool.Count)
			{
				T nextEntry = pool [next];
				next++;
				if (next >= pool.Count)
					next = 0;
				return nextEntry;
			}
			else
			{
				next = 0;
			}
			return default(T);
		}
	}
}