using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectEmber.Rendering
{
    [Serializable]
    public sealed class PixelArtFrame
    {
        [SerializeField] private string spriteName;
        [SerializeField] private float duration;

        public string SpriteName
        {
            get => spriteName;
            set => spriteName = value;
        }

        public float Duration
        {
            get => duration;
            set => duration = Mathf.Max(0.01f, value);
        }

        public PixelArtFrame(string spriteName, float duration = 0.1f)
        {
            this.spriteName = spriteName;
            this.duration = duration;
        }
    }

    [Serializable]
    public sealed class PixelArtAnimation
    {
        [SerializeField] private string animationName;
        [SerializeField] private List<PixelArtFrame> frames;
        [SerializeField] private bool loop;
        [SerializeField] private float frameRate;

        public string AnimationName
        {
            get => animationName;
            set => animationName = value;
        }

        public List<PixelArtFrame> Frames
        {
            get => frames;
            set => frames = value ?? new List<PixelArtFrame>();
        }

        public bool Loop
        {
            get => loop;
            set => loop = value;
        }

        public float FrameRate
        {
            get => frameRate;
            set => frameRate = Mathf.Max(0.1f, value);
        }

        public PixelArtAnimation(string animationName, bool loop = true, float frameRate = 12f)
        {
            this.animationName = animationName;
            this.frames = new List<PixelArtFrame>();
            this.loop = loop;
            this.frameRate = frameRate;
        }

        public void AddFrame(string spriteName, float duration = 0f)
        {
            if (duration <= 0f)
            {
                duration = 1f / frameRate;
            }
            frames.Add(new PixelArtFrame(spriteName, duration));
        }
    }

    [CreateAssetMenu(fileName = "PixelArtCharacterData", menuName = "Project Ember/Pixel Art/Character Data")]
    public sealed class PixelArtCharacterData : ScriptableObject
    {
        [SerializeField] private string characterName;
        [SerializeField] private List<PixelArtAnimation> animations;
        [SerializeField] private string defaultAnimation;
        [SerializeField] private Vector2 pivotOffset;

        public string CharacterName
        {
            get => characterName;
            set => characterName = value;
        }

        public List<PixelArtAnimation> Animations
        {
            get => animations;
            set => animations = value ?? new List<PixelArtAnimation>();
        }

        public string DefaultAnimation
        {
            get => defaultAnimation;
            set => defaultAnimation = value;
        }

        public Vector2 PivotOffset
        {
            get => pivotOffset;
            set => pivotOffset = value;
        }

        public PixelArtAnimation GetAnimation(string animationName)
        {
            if (string.IsNullOrEmpty(animationName))
            {
                return null;
            }

            foreach (var anim in animations)
            {
                if (anim.AnimationName == animationName)
                {
                    return anim;
                }
            }

            return null;
        }

        public PixelArtAnimation GetDefaultAnimation()
        {
            return GetAnimation(defaultAnimation);
        }
    }

    [CreateAssetMenu(fileName = "PixelArtTreeData", menuName = "Project Ember/Pixel Art/Tree Data")]
    public sealed class PixelArtTreeData : ScriptableObject
    {
        [SerializeField] private string treeType;
        [SerializeField] private string trunkSprite;
        [SerializeField] private List<string> foliageSprites;
        [SerializeField] private Vector2 trunkOffset;
        [SerializeField] private Vector2 foliageBaseOffset;
        [SerializeField] private float foliageSpreadRadius;

        public string TreeType
        {
            get => treeType;
            set => treeType = value;
        }

        public string TrunkSprite
        {
            get => trunkSprite;
            set => trunkSprite = value;
        }

        public List<string> FoliageSprites
        {
            get => foliageSprites;
            set => foliageSprites = value ?? new List<string>();
        }

        public Vector2 TrunkOffset
        {
            get => trunkOffset;
            set => trunkOffset = value;
        }

        public Vector2 FoliageBaseOffset
        {
            get => foliageBaseOffset;
            set => foliageBaseOffset = value;
        }

        public float FoliageSpreadRadius
        {
            get => foliageSpreadRadius;
            set => foliageSpreadRadius = Mathf.Max(0f, value);
        }
    }

    [CreateAssetMenu(fileName = "PixelArtTileData", menuName = "Project Ember/Pixel Art/Tile Data")]
    public sealed class PixelArtTileData : ScriptableObject
    {
        [SerializeField] private string tileType;
        [SerializeField] private List<string> variantSprites;
        [SerializeField] private Vector2 tileSize;

        public string TileType
        {
            get => tileType;
            set => tileType = value;
        }

        public List<string> VariantSprites
        {
            get => variantSprites;
            set => variantSprites = value ?? new List<string>();
        }

        public Vector2 TileSize
        {
            get => tileSize;
            set => tileSize = value;
        }

        public string GetRandomVariant(System.Random random)
        {
            if (variantSprites.Count == 0)
            {
                return null;
            }

            return variantSprites[random.Next(variantSprites.Count)];
        }
    }
}
