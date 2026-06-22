using UnityEngine;

namespace CodeBase.Infrastructure.Services.Audio
{
    public interface IAudioService : IService
    {
        void PlaySfx(AudioClipId clipId);
        void PlaySfxAt(AudioClipId clipId, Vector3 position);
        void SetMasterVolume(float volume);
        void PlayLoop(AudioClipId clipId);
        void StopLoop(AudioClipId clipId);
    }

    public enum AudioClipId
    {
        SwordSwing,
        ArrowShoot,
        ShopItemPick,
        CampfireButtonPick,
        Campfire
    }
}