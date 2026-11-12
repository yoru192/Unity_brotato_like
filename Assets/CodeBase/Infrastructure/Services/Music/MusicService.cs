using System.Collections;
using System.Collections.Generic;
using CodeBase.StaticData;
using UnityEngine;
using UnityEngine.Audio;

namespace CodeBase.Infrastructure.Services.Music
{
    public class MusicService : IMusicService
    {
        private const float FadeDuration = 1.5f;

        private readonly Dictionary<MusicTrackId, MusicStaticData.MusicEntry> _entries = new();
        private readonly ICoroutineRunner _coroutineRunner;

        private AudioSource _active;
        private AudioSource _idle;
        private Coroutine _fade;
        private MusicTrackId? _currentId;

        public MusicService(MusicStaticData data, ICoroutineRunner coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;

            if (data == null)
            {
                Debug.LogError("MusicStaticData не знайдено. Музика вимкнена.");
                return;
            }

            foreach (MusicStaticData.MusicEntry entry in data.entries)
                _entries[entry.id] = entry;

            GameObject go = new GameObject("MusicPlayer");
            Object.DontDestroyOnLoad(go);
            _active = CreateSource(go, data.musicGroup);
            _idle = CreateSource(go, data.musicGroup);
        }

        public void Play(MusicTrackId id)
        {
            if (_active == null)
                return;

            if (!_entries.TryGetValue(id, out MusicStaticData.MusicEntry entry) || entry.clip == null)
            {
                Debug.LogWarning($"Музичний трек {id} не налаштований у MusicStaticData.");
                return;
            }

            if (_currentId == id && _active.isPlaying)
                return;

            _currentId = id;
            StopFade();

            // Якщо попередній перехід ще йшов — обриваємо той трек, що затухав.
            _idle.Stop();
            (_active, _idle) = (_idle, _active);

            _active.clip = entry.clip;
            _active.loop = entry.loop;
            _active.volume = 0f;
            _active.Play();

            _fade = _coroutineRunner.StartCoroutine(Crossfade(_active, _idle, entry.volume));
        }

        public void Stop()
        {
            if (_active == null)
                return;

            _currentId = null;
            StopFade();
            _idle.Stop();
            _fade = _coroutineRunner.StartCoroutine(Crossfade(null, _active, 0f));
        }

        // Unscaled час: у станах Shop/Campfire/Win/GameOver timeScale = 0,
        // і WaitForSeconds просто завис би.
        private IEnumerator Crossfade(AudioSource incoming, AudioSource outgoing, float targetVolume)
        {
            float outgoingStart = outgoing.volume;
            float elapsed = 0f;

            while (elapsed < FadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(elapsed / FadeDuration);

                if (incoming != null)
                    incoming.volume = Mathf.Lerp(0f, targetVolume, k);
                outgoing.volume = Mathf.Lerp(outgoingStart, 0f, k);

                yield return null;
            }

            outgoing.Stop();
            outgoing.volume = 0f;
            _fade = null;
        }

        private void StopFade()
        {
            if (_fade == null)
                return;

            _coroutineRunner.StopCoroutine(_fade);
            _fade = null;
        }

        private static AudioSource CreateSource(GameObject go, AudioMixerGroup group)
        {
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0f;
            source.outputAudioMixerGroup = group;
            return source;
        }
    }
}