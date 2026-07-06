using System.Collections.Generic;
using UnityEngine;

namespace ProjectEmber.Rendering
{
    public sealed class PixelArtSpriteManager : MonoBehaviour
    {
        private static PixelArtSpriteManager instance;
        private readonly Dictionary<string, Sprite> spriteCache = new();

        public static PixelArtSpriteManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("PixelArtSpriteManager");
                    instance = go.AddComponent<PixelArtSpriteManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        public Sprite GetSprite(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                return null;
            }

            if (spriteCache.TryGetValue(spriteName, out var cachedSprite))
            {
                return cachedSprite;
            }

            var loadedSprite = Resources.Load<Sprite>($"PixelArt/{spriteName}");
            if (loadedSprite != null)
            {
                spriteCache[spriteName] = loadedSprite;
            }

            return loadedSprite;
        }

        public void RegisterSprite(string key, Sprite sprite)
        {
            if (!string.IsNullOrEmpty(key) && sprite != null)
            {
                spriteCache[key] = sprite;
            }
        }

        public void ClearCache()
        {
            spriteCache.Clear();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
