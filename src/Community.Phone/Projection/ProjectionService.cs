// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Runtime.CompilerServices;
using System.Windows;

namespace Microsoft.Phone.Controls;

public enum EffectConflictBehavior { Replace, Preserve }

public enum HitTestPolicy { Layout, Disabled }

public static class ProjectionService
{
    extension(DependencyObject element)
    {
        public Projection Projection
        {
            get => (Projection)element.GetValue(ProjectionProperty);
            set => element.SetValue(ProjectionProperty, value);
        }

        public bool PreferShader
        {
            get => (bool)element.GetValue(PreferShaderProperty);
            set => element.SetValue(PreferShaderProperty, value);
        }
        public double PerspectiveDepth
        {
            get => (double)element.GetValue(PerspectiveDepthProperty);
            set => element.SetValue(PerspectiveDepthProperty, value);
        }
        public HitTestPolicy HitTestPolicy
        {
            get => (HitTestPolicy)element.GetValue(HitTestPolicyProperty);
            set => element.SetValue(HitTestPolicyProperty, value);
        }
        public EffectConflictBehavior EffectConflictBehavior
        {
            get => (EffectConflictBehavior)element.GetValue(EffectConflictBehaviorProperty);
            set => element.SetValue(EffectConflictBehaviorProperty, value);
        }
    }
    private static readonly ConditionalWeakTable<UIElement, ShaderProjectionRenderer> States =
        new ConditionalWeakTable<UIElement, ShaderProjectionRenderer>();

    public static Projection GetProjection(DependencyObject element) => (Projection)element.GetValue(ProjectionProperty);
    public static void SetProjection(DependencyObject element, Projection value) => element.SetValue(ProjectionProperty, value);
    public static readonly DependencyProperty ProjectionProperty =
        DependencyProperty.RegisterAttached("Projection", typeof(Projection), typeof(ProjectionService), new PropertyMetadata(null, (d, e) =>
        {
            if (d is UIElement element)
            {
                if (e.NewValue == null && States.TryGetValue(element, out ShaderProjectionRenderer renderer))
                {
                    States.Remove(element);
                    renderer.Release();
                }
                else
                {
                    var current = States.GetValue(element, (element) => new ShaderProjectionRenderer(element));
                    current.Invalidate();
                }
            }
        }));

    public static bool GetPreferShader(DependencyObject element) => (bool)element.GetValue(PreferShaderProperty);
    public static void SetPreferShader(DependencyObject element, bool value) => element.SetValue(PreferShaderProperty, value);
    public static readonly DependencyProperty PreferShaderProperty =
        DependencyProperty.RegisterAttached("PreferShader", typeof(bool), typeof(ProjectionService), new PropertyMetadata(true));

    public static double GetPerspectiveDepth(DependencyObject element) => (double)element.GetValue(PerspectiveDepthProperty);
    public static void SetPerspectiveDepth(DependencyObject element, double value) => element.SetValue(PerspectiveDepthProperty, value);
    public static readonly DependencyProperty PerspectiveDepthProperty =
        DependencyProperty.RegisterAttached("PerspectiveDepth", typeof(double), typeof(ProjectionService), new PropertyMetadata(ProjectionMath.DefaultPerspectiveDepth));

    public static HitTestPolicy GetHitTestPolicy(DependencyObject element) => (HitTestPolicy)element.GetValue(HitTestPolicyProperty);
    public static void SetHitTestPolicy(DependencyObject element, HitTestPolicy value) => element.SetValue(HitTestPolicyProperty, value);
    public static readonly DependencyProperty HitTestPolicyProperty =
        DependencyProperty.RegisterAttached("HitTestPolicy", typeof(HitTestPolicy), typeof(ProjectionService), new PropertyMetadata(HitTestPolicy.Layout));


    public static EffectConflictBehavior GetEffectConflictBehavior(DependencyObject obj) => (EffectConflictBehavior)obj.GetValue(EffectConflictBehaviorProperty);
    public static void SetEffectConflictBehavior(DependencyObject obj, EffectConflictBehavior value) => obj.SetValue(EffectConflictBehaviorProperty, value);
    public static readonly DependencyProperty EffectConflictBehaviorProperty =
        DependencyProperty.RegisterAttached("EffectConflictBehavior", typeof(EffectConflictBehavior), typeof(ProjectionService), new PropertyMetadata(EffectConflictBehavior.Replace));

}
