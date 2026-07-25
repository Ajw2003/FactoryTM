using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
// GEMINI_MODIFICATION_TEST

using System.Collections;
using Code.Scripts.EventSystems;
using Singleton;
using UnityEngine;
using UnityEngine.Audio;

namespace Code.Scripts.Audio
{
    public class AudioManager : SingletonBase<AudioManager>
    {
        [field: SerializeField] public  AudioSource sfxSource;

        [SerializeField] private AudioSource ambientSource;

        [SerializeField] private AudioSource musicSource;

        [SerializeField] public float pitchVariation = 0.01f;

        [SerializeField]protected bool PersistBetweenScenes;

        private AudioMixer _mixer;

        private void Start()
        {
          persistBetweenScenes = PersistBetweenScenes;
            sfxSource = GetComponent<AudioSource>();
                        EventManager.Instance?.Subscribe(this, (AudioClipEvent e) => PlayClip(e.Clip, e.Channel, e.Volume, e.Duration));
        }

        void PlayClip(AudioClip clip, AudioChannel channel, float volume, float duration)
        {
            
            AudioSource targetSource = GetAudioSource(channel);
            if (targetSource == null || clip == null)
            {
                Debug.LogWarning("Source or clip is null");
                return;
            }
            
            targetSource.clip = clip;
            if (volume > 0.05)
            {
                targetSource.volume = volume;
            }

            if (duration >= 0.01)
            {
                var stopClipRoutine = StopClipRoutine(channel, duration);
            }
            targetSource.PlayOneShot(clip); 
        }

        private IEnumerator StopClipRoutine(AudioChannel channel, float duration)
        {
            yield return new WaitForSeconds(duration);
            StopClip(channel);
            yield return null;
        }

        private AudioSource GetAudioSource(AudioChannel channel)
        {
            switch (channel)
            {
                case AudioChannel.Music: return musicSource;
                case AudioChannel.Sfx: return sfxSource;
                case AudioChannel.Ambient: return ambientSource;
                case AudioChannel.Null : return null;
                default: return null;
            }
        }

        public void StopClip(AudioChannel channel)
        {
            AudioSource targetSource = GetAudioSource(channel);
            if (targetSource == null)
            {
                Debug.LogWarning("Source is null");
                return;
            }
            targetSource.Stop();
            
        }
    }
}
