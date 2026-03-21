using System;
using UnityEngine;

namespace BottleFillShaderGraph.Demo
{
    public class BottleFillDemoController : MonoBehaviour
    {
    [Header("Target")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private bool usePropertyBlock = true;

    [Header("Shader Properties")]
    [SerializeField] private string fillAmountProperty = "_FillAmount";
    [SerializeField] private string spriteHeightProperty = "_SpriteHeight";
    [SerializeField] private string bandScaleProperty = "_BandScale";
    [SerializeField] private string bandCountProperty = "_BandCount";
    [SerializeField] private string[] bandColorProperties =
    {
        "_BandColor1",
        "_BandColor2",
        "_BandColor3",
        "_BandColor4"
    };

    [Header("Values")]
    [SerializeField] private bool autoSpriteHeight = true;
    [SerializeField] private float spriteHeight = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float fillAmount = 0.5f;
    [SerializeField] private float bandScale = 1f;
    [SerializeField] private bool applyEveryFrame = true;

    [Header("Animation")]
    [SerializeField] private bool animateFill = true;
    [SerializeField] private float fillSpeed = 0.35f;
    [SerializeField] private Vector2 fillRange = new Vector2(0f, 1f);

    [Header("Palette")]
    [SerializeField] private Color[] palette =
    {
        Color.red,
        Color.yellow,
        Color.blue,
        Color.green
    };
    [SerializeField] private bool shuffleOnPlay;

    private MaterialPropertyBlock propertyBlock;
    private SpriteRenderer spriteRenderer;
    private bool hasPaletteWarning;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        spriteRenderer = targetRenderer as SpriteRenderer;
        propertyBlock = new MaterialPropertyBlock();
        if (shuffleOnPlay)
        {
            ShufflePalette();
        }

        ApplyNow();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        if (animateFill)
        {
            var t = Mathf.PingPong(Time.time * fillSpeed, 1f);
            fillAmount = Mathf.Lerp(fillRange.x, fillRange.y, t);
        }

        if (!applyEveryFrame && !animateFill)
        {
            return;
        }

        ApplyNow();
    }

    private void OnValidate()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
    }

    private void OnDisable()
    {
        ClearPropertyBlock();
    }

    public void ApplyNow()
    {
        if (targetRenderer == null)
        {
            return;
        }
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        var height = autoSpriteHeight ? ComputeSpriteHeight() : spriteHeight;
        var colors = ResolvePalette();
        var bandCount = ResolveBandCount();

        if (usePropertyBlock)
        {
            targetRenderer.GetPropertyBlock(propertyBlock);
            SetProperty(propertyBlock, fillAmountProperty, fillAmount);
            SetProperty(propertyBlock, spriteHeightProperty, height);
            SetProperty(propertyBlock, bandScaleProperty, bandScale);
            SetProperty(propertyBlock, bandCountProperty, bandCount);
            SetColors(propertyBlock, colors);
            targetRenderer.SetPropertyBlock(propertyBlock);
            return;
        }

        var material = Application.isPlaying ? targetRenderer.material : targetRenderer.sharedMaterial;
        if (material == null)
        {
            return;
        }

        SetProperty(material, fillAmountProperty, fillAmount);
        SetProperty(material, spriteHeightProperty, height);
        SetProperty(material, bandScaleProperty, bandScale);
        SetProperty(material, bandCountProperty, bandCount);
        SetColors(material, colors);
    }

    private float ComputeSpriteHeight()
    {
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            return spriteRenderer.sprite.bounds.size.y * spriteRenderer.transform.lossyScale.y;
        }

        return targetRenderer.bounds.size.y;
    }

    private Color[] ResolvePalette()
    {
        const int bandCount = 4;
        var resolved = new Color[bandCount];
        if (palette == null || palette.Length == 0)
        {
            for (var i = 0; i < resolved.Length; i++)
            {
                resolved[i] = Color.white;
            }
            return resolved;
        }

        if (palette.Length > bandCount && !hasPaletteWarning)
        {
            Debug.LogWarning(
                $"{name}: Palette has {palette.Length} colors; only the first {bandCount} are used.");
            hasPaletteWarning = true;
        }

        for (var i = 0; i < resolved.Length; i++)
        {
            resolved[i] = palette[Mathf.Min(i, palette.Length - 1)];
        }

        return resolved;
    }

    private float ResolveBandCount()
    {
        var count = palette != null ? palette.Length : 0;
        if (count <= 0)
        {
            count = 1;
        }
        return Mathf.Clamp(count, 1, 4);
    }

    private void SetColors(MaterialPropertyBlock block, Color[] colors)
    {
        for (var i = 0; i < bandColorProperties.Length && i < colors.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(bandColorProperties[i]))
            {
                block.SetColor(bandColorProperties[i], colors[i]);
            }
        }
    }

    private void SetColors(Material material, Color[] colors)
    {
        for (var i = 0; i < bandColorProperties.Length && i < colors.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(bandColorProperties[i]))
            {
                material.SetColor(bandColorProperties[i], colors[i]);
            }
        }
    }

    private void SetProperty(MaterialPropertyBlock block, string name, float value)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            block.SetFloat(name, value);
        }
    }

    private void SetProperty(Material material, string name, float value)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            material.SetFloat(name, value);
        }
    }

    private void ShufflePalette()
    {
        if (palette == null || palette.Length <= 1)
        {
            return;
        }

        var rng = new System.Random();
        for (var i = palette.Length - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (palette[i], palette[j]) = (palette[j], palette[i]);
        }
    }

    [ContextMenu("Clear Property Block")]
    private void ClearPropertyBlock()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }
        if (targetRenderer == null)
        {
            return;
        }

        targetRenderer.SetPropertyBlock(null);
    }
    }
}
