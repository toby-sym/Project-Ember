using System.Collections.Generic;
using UnityEngine;

namespace ProjectEmber.Rendering
{
    [CreateAssetMenu(
        fileName = "VectorSpriteData",
        menuName = "Project Ember/Rendering/Vector Sprite Data")]
    public sealed class VectorSpriteData : ScriptableObject
    {
        [SerializeField] private List<VectorLayer> layers = new();

        public List<VectorLayer> Layers => layers;
    }
}
