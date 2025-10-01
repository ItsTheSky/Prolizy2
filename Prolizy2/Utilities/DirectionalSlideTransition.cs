using System.Collections.Generic;
using Avalonia.Controls;

namespace Prolizy2.Utilities;

using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

public class DirectionalSlideTransition : IPageTransition
{
    public enum SlideDirection
    {
        LeftToRight,  // Retour arrière (page vient de la gauche)
        RightToLeft   // Forward (page vient de la droite)
    }
    
    public SlideDirection Direction { get; set; }
    public TimeSpan Duration { get; set; }
    public TimeSpan FadeDuration { get; set; }
    public IEasing? Easing { get; set; }
    
    public DirectionalSlideTransition(
        SlideDirection direction,
        TimeSpan duration,
        TimeSpan fadeDuration,
        IEasing? easing = null)
    {
        Direction = direction;
        Duration = duration;
        FadeDuration = fadeDuration;
        Easing = easing ?? new CubicEaseOut();
    }
    
    public async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
            
        var tasks = new List<Task>();
        
        // Slide animation
        if (from is Control fromControl && to is Control toControl)
        {
            var fromVisual = ElementComposition.GetElementVisual(fromControl);
            var toVisual = ElementComposition.GetElementVisual(toControl);
            
            if (fromVisual != null && toVisual != null)
            {
                var compositor = fromVisual.Compositor;
                var parent = toControl.GetVisualParent();
                var width = parent?.Bounds.Width ?? 0;
                
                // Déterminer la direction du slide
                float slideMultiplier = Direction == SlideDirection.RightToLeft ? 1 : -1;
                
                // Animation de la page sortante (from)
                var fromAnimation = compositor!.CreateVector3KeyFrameAnimation();
                fromAnimation.Target = "Offset";
                fromAnimation.InsertKeyFrame(0f, new Vector3(0, 0, 0), Easing);
                fromAnimation.InsertKeyFrame(1f, new Vector3((float)(-width * slideMultiplier), 0, 0), Easing);
                fromAnimation.Duration = Duration;
                
                // Animation de la page entrante (to)
                var toAnimation = compositor.CreateVector3KeyFrameAnimation();
                toAnimation.Target = "Offset";
                toAnimation.InsertKeyFrame(0f, new Vector3((float)(width * slideMultiplier), 0, 0), Easing);
                toAnimation.InsertKeyFrame(1f, new Vector3(0, 0, 0), Easing);
                toAnimation.Duration = Duration;
                
                fromVisual.StartAnimation("Offset", fromAnimation);
                toVisual.StartAnimation("Offset", toAnimation);
                
                tasks.Add(Task.Delay(Duration, cancellationToken));
            }
        }
        
        // Fade animation
        if (from != null)
        {
            tasks.Add(FadeOut(from, FadeDuration, cancellationToken));
        }
        
        if (to != null)
        {
            tasks.Add(FadeIn(to, FadeDuration, cancellationToken));
        }
        
        await Task.WhenAll(tasks);
    }
    
    private async Task FadeOut(Visual visual, TimeSpan duration, CancellationToken cancellationToken)
    {
        if (visual is Control control)
        {
            var compositionVisual = ElementComposition.GetElementVisual(control);
            if (compositionVisual != null)
            {
                var animation = compositionVisual.Compositor!.CreateScalarKeyFrameAnimation();
                animation.Target = "Opacity";
                animation.InsertKeyFrame(0f, 1f);
                animation.InsertKeyFrame(1f, 0f);
                animation.Duration = duration;
                
                compositionVisual.StartAnimation("Opacity", animation);
                await Task.Delay(duration, cancellationToken);
            }
        }
    }
    
    private async Task FadeIn(Visual visual, TimeSpan duration, CancellationToken cancellationToken)
    {
        if (visual is Control control)
        {
            var compositionVisual = ElementComposition.GetElementVisual(control);
            if (compositionVisual != null)
            {
                var animation = compositionVisual.Compositor!.CreateScalarKeyFrameAnimation();
                animation.Target = "Opacity";
                animation.InsertKeyFrame(0f, 0f);
                animation.InsertKeyFrame(1f, 1f);
                animation.Duration = duration;
                
                compositionVisual.StartAnimation("Opacity", animation);
                await Task.Delay(duration, cancellationToken);
            }
        }
    }
}