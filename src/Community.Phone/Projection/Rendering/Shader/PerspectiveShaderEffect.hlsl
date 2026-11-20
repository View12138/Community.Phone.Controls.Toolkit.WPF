// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
// Perspective projection of a planar element through a single homography.
//
// The element is rasterized by WPF into the effect's implicit input, which
// covers the element bounds expanded by the effect padding. The shader walks
// backwards from every output pixel: the pixel is mapped into the element's
// own (unprojected) coordinate space with the inverse homography, the sample is
// rejected when it falls outside the source rectangle or behind the camera, and
// the input texture is sampled in the padded texture space.
//
// Compile with:
//   fxc /T ps_2_0 /E main /Fo PerspectiveShaderEffect.ps PerspectiveShaderEffect.hlsl

sampler2D implicitInput : register(s0);

// Inverse homography, one row per register (row-vector convention).
float4 InverseRow1 : register(c0);
float4 InverseRow2 : register(c1);
float4 InverseRow3 : register(c2);

// Third row of the forward homography (xyz) and an enable flag (w). A point is
// in front of the camera while the homogeneous weight is positive.
float4 ClipRow3 : register(c3);

// Padding offset (x, y) and the padded output size (z, w), in DIPs.
float4 OutputBounds : register(c4);

// Size of the unprojected element (x, y).
float4 SourceSize : register(c5);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    // OutputBounds.xy is the offset of the padded surface relative to the
    // element origin, so it is negative for a surface that starts before the
    // element. Adding it turns a padded surface coordinate into an
    // element-local coordinate.
    float2 padded = uv * OutputBounds.zw;
    float3 position = float3(padded + OutputBounds.xy, 1.0);

    float weight = dot(InverseRow3.xyz, position);
    float safeWeight = (abs(weight) < 1e-6) ? 1e-6 : weight;

    float2 source = float2(dot(InverseRow1.xyz, position), dot(InverseRow2.xyz, position)) / safeWeight;

    float inside =
        step(0.0, source.x) * step(source.x, SourceSize.x) *
        step(0.0, source.y) * step(source.y, SourceSize.y) *
        step(1e-9, dot(ClipRow3.xyz, float3(source, 1.0))) *
        ClipRow3.w;

    // The input texture spans the padded surface, so the sample has to be moved
    // back into padded space before it is normalized.
    float2 sourceUv = (source - OutputBounds.xy) / OutputBounds.zw;
    return tex2D(implicitInput, sourceUv) * inside;
}
