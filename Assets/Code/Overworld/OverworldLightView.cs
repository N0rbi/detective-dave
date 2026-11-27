using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DetectiveDave.OverworldPlay
{
    public sealed class OverworldLightView : MonoBehaviour
    {
        Light2D[] lights;
        LightFade fade;
        Tween tween;
        float intensity;

        public void Configure(Light2D[] spriteLights, LightFade lightFade)
        {
            lights = spriteLights;
            fade = lightFade;
            SetIntensity(fade.FadeInFrom);
        }

        public void FadeIn() => FadeTo(fade.FadeInTo, fade.FadeInSeconds, null);

        public void FadeOut(Action done) => FadeTo(fade.FadeOutTo, fade.FadeOutSeconds, done);

        void FadeTo(float target, float seconds, Action done)
        {
            tween?.Kill();
            tween = DOTween.To(() => intensity, SetIntensity, target, seconds).SetEase(Ease.Linear).SetLink(gameObject).OnComplete(() => done?.Invoke());
        }

        void SetIntensity(float value)
        {
            intensity = value;
            foreach (var light in lights) light.intensity = value;
        }
    }
}
