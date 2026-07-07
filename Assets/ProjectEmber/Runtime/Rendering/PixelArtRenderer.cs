using System;
using System.Collections;
using UnityEngine;

namespace ProjectEmber.Rendering
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PixelArtRenderer : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private PixelArtCharacterData characterData;
        private PixelArtAnimation currentAnimation;
        private int currentFrameIndex;
        private float frameTimer;
        private bool isPlaying;

        public Sprite CurrentSprite => spriteRenderer != null ? spriteRenderer.sprite : null;
        public bool IsPlaying => isPlaying;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            
            spriteRenderer.sortingOrder = 0;
        }

        public void SetStaticSprite(string spriteName)
        {
            StopAnimation();
            var sprite = PixelArtSpriteManager.Instance.GetSprite(spriteName);
            spriteRenderer.sprite = sprite;
        }

        public void SetCharacterData(PixelArtCharacterData data)
        {
            characterData = data;
            if (data != null)
            {
                PlayAnimation(data.DefaultAnimation);
            }
        }

        public void PlayAnimation(string animationName)
        {
            if (characterData == null)
            {
                Debug.LogWarning($"[PixelArtRenderer] Cannot play animation '{animationName}': no character data assigned on '{gameObject.name}'.");
                return;
            }

            var animation = characterData.GetAnimation(animationName);
            if (animation == null)
            {
                Debug.LogWarning($"[PixelArtRenderer] Animation '{animationName}' not found in character data on '{gameObject.name}'.");
                return;
            }

            if (animation.Frames == null || animation.Frames.Count == 0)
            {
                Debug.LogWarning($"[PixelArtRenderer] Animation '{animationName}' has no frames on '{gameObject.name}'.");
                return;
            }

            currentAnimation = animation;
            currentFrameIndex = 0;
            frameTimer = 0f;
            isPlaying = true;
            UpdateFrame();
        }

        public void StopAnimation()
        {
            isPlaying = false;
            currentAnimation = null;
        }

        private void Update()
        {
            if (!isPlaying || currentAnimation == null)
            {
                return;
            }

            if (currentAnimation.Frames == null || currentAnimation.Frames.Count == 0)
            {
                isPlaying = false;
                return;
            }

            if (currentFrameIndex < 0 || currentFrameIndex >= currentAnimation.Frames.Count)
            {
                currentFrameIndex = 0;
            }

            frameTimer += Time.deltaTime;
            var currentFrame = currentAnimation.Frames[currentFrameIndex];

            if (frameTimer >= currentFrame.Duration)
            {
                frameTimer = 0f;
                currentFrameIndex++;

                if (currentFrameIndex >= currentAnimation.Frames.Count)
                {
                    if (currentAnimation.Loop)
                    {
                        currentFrameIndex = 0;
                    }
                    else
                    {
                        currentFrameIndex = currentAnimation.Frames.Count - 1;
                        isPlaying = false;
                    }
                }

                UpdateFrame();
            }
        }

        private void UpdateFrame()
        {
            if (currentAnimation == null || currentFrameIndex >= currentAnimation.Frames.Count)
            {
                return;
            }

            var frame = currentAnimation.Frames[currentFrameIndex];
            var sprite = PixelArtSpriteManager.Instance.GetSprite(frame.SpriteName);
            spriteRenderer.sprite = sprite;
        }

        public void SetSortingOrder(int order)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = order;
            }
        }

        public void SetColor(Color color)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }

        public void FlipX(bool flip)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = flip;
            }
        }
    }
}
