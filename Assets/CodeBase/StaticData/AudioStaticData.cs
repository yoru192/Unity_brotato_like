using System;
using System.Collections.Generic;
using CodeBase.Infrastructure.Services.Audio;
using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(menuName = "StaticData/Audio")]
    public class AudioStaticData : ScriptableObject
    {
        public List<AudioEntry> entries = new();
    }

    [Serializable]
    public class AudioEntry
    {
        public AudioClipId id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0f, 0.3f)] public float pitchVariance = 0.1f;
    }
}