using System.Collections.Generic;
using System.Linq;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.Audio
{
    public class AudioService : IAudioService
    {
        private const int PoolSize = 8;

        private readonly Dictionary<AudioClipId, AudioEntry> _clips = new();
        private readonly List<AudioSource> _pool = new();
        private readonly Dictionary<AudioClipId, AudioSource> _loopSources = new();
        private GameObject _audioPoolGo;
        private float _masterVolume = 1f;

        public AudioService(AudioStaticData data)
        {
            foreach (var entry in data.entries)
                _clips[entry.id] = entry;
            
            _audioPoolGo = new GameObject("AudioPool");
            Object.DontDestroyOnLoad(_audioPoolGo);
            var go = _audioPoolGo;

            for (int i = 0; i < PoolSize; i++)
            {
                var source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                _pool.Add(source);
            }
        }

        public void PlaySfx(AudioClipId clipId)
        {
            if (!_clips.TryGetValue(clipId, out var entry)) return;

            AudioSource source = GetFreeSource();
            source.transform.position = Vector3.zero;
            source.spatialBlend = 0f;
            PlayOnSource(source, entry);
        }

        public void PlaySfxAt(AudioClipId clipId, Vector3 position)
        {
            if (!_clips.TryGetValue(clipId, out var entry)) return;

            AudioSource source = GetFreeSource();
            source.transform.position = position;
            source.spatialBlend = 1f;
            source.maxDistance = 20f;
            source.rolloffMode = AudioRolloffMode.Linear;
            PlayOnSource(source, entry);
        }

        public void PlayLoop(AudioClipId clipId)
        {
            if (!_clips.TryGetValue(clipId, out var entry)) return;

            if (_loopSources.TryGetValue(clipId, out var existing) && existing.isPlaying)
                return;

            var source = _audioPoolGo.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.loop = true;
            source.clip = entry.clip;
            source.volume = entry.volume * _masterVolume;
            source.pitch = 1f;
            source.Play();

            _loopSources[clipId] = source;
        }

        public void StopLoop(AudioClipId clipId)
        {
            if (!_loopSources.TryGetValue(clipId, out var source)) return;

            source.Stop();
            Object.Destroy(source);
            _loopSources.Remove(clipId);
        }

        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);

            foreach (var (clipId, source) in _loopSources)
            {
                if (_clips.TryGetValue(clipId, out var entry))
                    source.volume = entry.volume * _masterVolume;
            }
        }

        private void PlayOnSource(AudioSource source, AudioEntry entry)
        {
            source.clip = entry.clip;
            source.volume = entry.volume * _masterVolume;
            source.pitch = 1f + Random.Range(-entry.pitchVariance, entry.pitchVariance);
            source.Play();
        }

        private AudioSource GetFreeSource()
        {
            return _pool.FirstOrDefault(s => !s.isPlaying) ?? _pool[0];
        }
    }
}