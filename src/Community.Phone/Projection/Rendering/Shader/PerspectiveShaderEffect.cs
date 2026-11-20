// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;

namespace Microsoft.Phone.Controls
{
    internal sealed class PerspectiveShaderEffect : ShaderEffect
    {
        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }
        private static readonly DependencyProperty InputProperty =
            RegisterPixelShaderSamplerProperty("Input", typeof(PerspectiveShaderEffect), 0);

        public static readonly DependencyProperty InverseRow1Property =
             DependencyProperty.Register("InverseRow1", typeof(Point4D), typeof(PerspectiveShaderEffect), new UIPropertyMetadata(new Point4D(), PixelShaderConstantCallback(0)));

        public static readonly DependencyProperty InverseRow2Property =
            DependencyProperty.Register("InverseRow2", typeof(Point4D), typeof(PerspectiveShaderEffect), new UIPropertyMetadata(new Point4D(), PixelShaderConstantCallback(1)));

        public static readonly DependencyProperty InverseRow3Property =
            DependencyProperty.Register("InverseRow3", typeof(Point4D), typeof(PerspectiveShaderEffect), new UIPropertyMetadata(new Point4D(), PixelShaderConstantCallback(2)));

        public static readonly DependencyProperty ClipRow3Property =
            DependencyProperty.Register("ClipRow3", typeof(Point4D), typeof(PerspectiveShaderEffect), new UIPropertyMetadata(new Point4D(), PixelShaderConstantCallback(3)));

        public static readonly DependencyProperty OutputBoundsProperty =
            DependencyProperty.Register("OutputBounds", typeof(Point4D), typeof(PerspectiveShaderEffect), new UIPropertyMetadata(new Point4D(), PixelShaderConstantCallback(4)));

        public static readonly DependencyProperty SourceSizeProperty =
            DependencyProperty.Register("SourceSize", typeof(Point4D), typeof(PerspectiveShaderEffect), new UIPropertyMetadata(new Point4D(), PixelShaderConstantCallback(5)));

        [ThreadStatic]
        private static PixelShader _perspectiveShader;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="PerspectiveShaderEffect"/> class.
        /// </summary>
        public PerspectiveShaderEffect()
        {
            _perspectiveShader ??= new PixelShader
            {
                UriSource = new Uri("/Community.Phone;component/Projection/Rendering/Shader/PerspectiveShaderEffect.ps", UriKind.Relative)
            };
            PixelShader = _perspectiveShader;

            UpdateShaderValue(InputProperty);

            UpdateShaderValue(InverseRow1Property);
            UpdateShaderValue(InverseRow2Property);
            UpdateShaderValue(InverseRow3Property);
            UpdateShaderValue(ClipRow3Property);
            UpdateShaderValue(OutputBoundsProperty);
            UpdateShaderValue(SourceSizeProperty);
        }

        /// <summary>
        /// Updates every shader constant of a projection frame.
        /// </summary>
        internal void Update(HomographyMatrix inverse, HomographyMatrix forward, Rect outputBounds, Size sourceSize, bool enabled)
        {
            (double X, double Y, double Constant) sourceX = inverse.GetOutputCoefficients(0);
            (double X, double Y, double Constant) sourceY = inverse.GetOutputCoefficients(1);
            (double X, double Y, double Constant) weight = inverse.GetOutputCoefficients(2);
            (double X, double Y, double Constant) clip = forward.GetOutputCoefficients(2);

            SetValue(InverseRow1Property, new Point4D(sourceX.X, sourceX.Y, sourceX.Constant, 0));
            SetValue(InverseRow2Property, new Point4D(sourceY.X, sourceY.Y, sourceY.Constant, 0));
            SetValue(InverseRow3Property, new Point4D(weight.X, weight.Y, weight.Constant, 0));
            SetValue(ClipRow3Property, new Point4D(clip.X, clip.Y, clip.Constant, enabled ? 1.0 : 0.0));
            SetValue(OutputBoundsProperty, new Point4D(outputBounds.X, outputBounds.Y, outputBounds.Width, outputBounds.Height));
            SetValue(SourceSizeProperty, new Point4D(sourceSize.Width, sourceSize.Height, 0, 0));

            var left = Math.Max(0, -outputBounds.X);
            var top = Math.Max(0, -outputBounds.Y);
            var right = Math.Max(0, outputBounds.Right - sourceSize.Width);
            var bottom = Math.Max(0, outputBounds.Bottom - sourceSize.Height);

            if (PaddingLeft != left)
            { PaddingLeft = left; }

            if (PaddingTop != top)
            { PaddingTop = top; }

            if (PaddingRight != right)
            { PaddingRight = right; }

            if (PaddingBottom != bottom)
            { PaddingBottom = bottom; }
        }
    }
}
