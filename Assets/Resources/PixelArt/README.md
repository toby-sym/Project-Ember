# Procedural Pixel Art System

Project Ember now features a **procedural pixel art generation system** that creates DS-era Pokemon-style pixel art textures at runtime from vector data. No external sprite assets are required - everything is generated procedurally!

## How It Works

The system converts vector shapes into pixel art textures through:
1. **Rasterization** - Vector shapes are rasterized onto a low-resolution texture grid (32x32 by default)
2. **Dithering** - Bayer matrix dithering adds authentic pixel art texture
3. **Palette Quantization** - Colors are quantized to limited palettes for retro feel
4. **Detail Generation** - Procedural noise and patterns add variation to tiles, characters, and trees

## Advanced Rendering Features

The game now includes high-end rendering capabilities:

### Anti-Aliasing & Quality
- **MSAA 4X** - Multisample anti-aliasing for smooth edges
- **Render Scale 1.2X** - Supersampling for crisp visuals
- **FSR Upscaling** - AMD FidelityFX Super Resolution support
- **HDR Rendering** - High dynamic range for better color depth

### Lighting & Shadows
- **Soft Shadows** - High-quality soft shadow mapping
- **4 Cascades** - Cascaded shadow maps for distant shadows
- **4096 Resolution** - High-resolution shadow maps
- **Additional Lights** - Up to 8 dynamic lights per object
- **Reflection Probes** - Real-time reflections

### Post-Processing
- **Color Grading** - ACES tonemapping and color correction
- **Bloom** - Glow effects for bright areas
- **TAA** - Temporal anti-aliasing for motion
- **Depth of Field** - Cinematic focus effects

## Pixel Art Shader Features

The custom `ProjectEmber/Pixel Art` shader includes:
- **Pixel Snapping** - Maintains crisp pixel edges
- **Bayer Dithering** - Authentic retro dithering patterns
- **Palette Quantization** - Limits colors to retro palettes
- **Outline Detection** - Automatic outline generation
- **Shadow/Highlight** - Subtle depth effects

## Texture Generation

### Characters
- Procedurally generated body parts (torso, head, hair, arms, legs)
- Randomized colors for hair, clothing, and skin tones
- Pixel noise for texture variation

### Trees
- Procedural trunk and foliage generation
- Randomized bark and leaf colors
- Organic blob patterns for foliage clusters

### Ground Tiles
- Grass with detail variation
- Dirt with small rocks
- Water with wave patterns
- Stone with cracks and highlights

## Toggle Between Modes

Switch between pixel art and geometric rendering:

```csharp
// Characters
character.UsePixelArt = true;  // or false

// Trees
ProceduralTreeFactory.CreateTree(name, seed, parent, usePixelArt: true);

// World
chunkManager.UsePixelArt = true;
```

## Performance Considerations

- Pixel art textures are generated at 32x32 resolution by default
- Textures are cached per object to avoid regeneration
- Point filtering ensures crisp pixel art without blur
- MSAA and supersampling can be disabled for performance

## Customization

Adjust pixel art generation parameters:
- **Resolution** - Change texture resolution in `ProceduralPixelArtGenerator`
- **Dither Strength** - Adjust via shader `_DitherStrength` parameter
- **Palette Colors** - Set custom palettes via shader `_PaletteColors` array
- **Noise Intensity** - Modify noise generation in texture methods
