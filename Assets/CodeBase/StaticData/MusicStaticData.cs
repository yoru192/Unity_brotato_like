using System;
using System.Collections.Generic;
using CodeBase.Infrastructure.Services.Music;
using UnityEngine;
using UnityEngine.Audio;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(fileName = "MusicStaticData", menuName = "StaticData/Music")]
    public class MusicStaticData : ScriptableObject
    {
        public AudioMixerGroup musicGroup;
        public List<MusicEntry> entries = new List<MusicEntry>();

        [Serializable]
        public class MusicEntry
        {
            public MusicTrackId id;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
            [Tooltip("Вимкни для джинглів (GameOver, Win)")]
            public bool loop = true;
        }
    }
}