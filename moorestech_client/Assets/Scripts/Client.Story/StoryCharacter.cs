using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Client.Story
{
    public class StoryCharacter : MonoBehaviour
    {
        [SerializeField] private AudioSource voiceAudioSource;
        [SerializeField] private SkinnedMeshRenderer faceSkinnedMeshRenderer;
        [SerializeField] private Animator animator;

        public void Initialize(Transform parent, string name)
        {
            gameObject.name = name + " (StoryCharacter)";
            transform.SetParent(parent);
        }

        public void SetTransform(Vector3 position, Vector3 rotation)
        {
            transform.position = position;
            transform.eulerAngles = rotation;
        }

        public void PlayAnimation(string animationName)
        {
            animator.Play(animationName);
        }

        public void PlayVoice(AudioClip voiceClip)
        {
            voiceAudioSource.clip = voiceClip;
            voiceAudioSource.Play();
        }

        public void SetEmotion(EmotionType emotion, float duration)
        {
            var blendShapeData = ToBlendShapeData(emotion);

            // Tween BlendShape
            foreach (var (key, value) in blendShapeData)
            {
                DOTween.To(
                    () => faceSkinnedMeshRenderer.GetBlendShapeWeight(key),
                    x => faceSkinnedMeshRenderer.SetBlendShapeWeight(key, x),
                    value,
                    duration);
            }

            #region Internal

            Dictionary<int, float> ToBlendShapeData(EmotionType emotionType)
            {
                return emotionType switch
                {
                    EmotionType.Normal => new Dictionary<int, float> { { 11, 0f } },
                    EmotionType.Happy => new Dictionary<int, float> { { 11, 100 } },
                };
            }

            #endregion
        }
    }
}