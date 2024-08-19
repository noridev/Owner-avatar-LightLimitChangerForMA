﻿namespace io.github.azukimochi;

[Serializable]
public sealed class LightingSettings : ISettings
{
    /// <summary>
    /// 明るさの下限
    /// </summary>
    [RangeParameter(nameof(MinLightRange))]
    public Parameter<float> MinLight = new(0.05f) { Range = new(0, 1) };

    /// <summary>
    /// 明るさの上限
    /// </summary>
    [RangeParameter(nameof(MaxLightRange))]
    public Parameter<float> MaxLight = new(1f) { Range = new(0, 10) };

    /// <summary>
    /// 明るさの下限の範囲
    /// </summary>
    [MinMaxSlider(0, 1)]
    public Vector2 MinLightRange = new(0, 1);

    /// <summary>
    /// 明るさの上限の範囲
    /// </summary>
    [MinMaxSlider(0, 10)]
    public Vector2 MaxLightRange = new(0, 1);

    /// <summary>
    /// 光の色の無視具合
    /// </summary>
    [ShaderFeature(SupportedShaders.LilToon)]
    public Parameter<float> Monochrome = new(0) { Range = new(0, 1) };

    /// <summary>
    /// 光の無視具合
    /// </summary>
    [ShaderFeature(SupportedShaders.LilToon)]
    public Parameter<float> Unlit = new(0) { Range = new(0, 1) };
}
