// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.





using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AdaptiveCards.Rendering.Avalonia
{
    public static class AdaptiveColumnRenderer
    {
        public static Control Render(AdaptiveColumn column, AdaptiveRenderContext context)
        {
            var uiContainer = new Grid();
            // uiContainer.Style = context.GetStyle("Adaptive.Column");

            bool? previousContextRtl = context.Rtl;
            bool? currentRtl = previousContextRtl;

            bool updatedRtl = false;
            if (column.Rtl.HasValue)
            {
                currentRtl = column.Rtl;
                context.Rtl = currentRtl;
                updatedRtl = true;
            }

            if (currentRtl.HasValue)
            {
                uiContainer.FlowDirection = currentRtl.Value ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            }

            // Keep track of ContainerStyle.ForegroundColors before Container is rendered
            var parentRenderArgs = context.RenderArgs;
            // This is the renderArgs that will be passed down to the children
            var childRenderArgs = new AdaptiveRenderArgs(parentRenderArgs);

            Border border = new Border();
            border.Child = uiContainer;

            bool inheritsStyleFromParent = !column.Style.HasValue;
            bool columnHasPadding = false;

            if (!inheritsStyleFromParent)
            {
                columnHasPadding = AdaptiveContainerRenderer.ApplyPadding(border, uiContainer, column, parentRenderArgs, context);

                // Apply background color
                ContainerStyleConfig containerStyle = context.Config.ContainerStyles.GetContainerStyleConfig(column.Style);
                border.Background = context.GetColorBrush(containerStyle.BackgroundColor);

                childRenderArgs.ForegroundColors = containerStyle.ForegroundColors;
            }

            childRenderArgs.ParentStyle = (inheritsStyleFromParent) ? parentRenderArgs.ParentStyle : column.Style.Value;

            // If the column has no padding or has padding and doesn't bleed, then the children can bleed
            // to the side the column would have bled
            if (columnHasPadding)
            {
                childRenderArgs.BleedDirection = BleedDirection.BleedAll;
            }

            // If either this column or an ancestor had padding, then the children will have an ancestor with padding
            childRenderArgs.HasParentWithPadding = (columnHasPadding || parentRenderArgs.HasParentWithPadding);
            context.RenderArgs = childRenderArgs;

            AdaptiveContainerRenderer.AddContainerElements(uiContainer, column.Items, context);

            RendererUtil.ApplyVerticalContentAlignment(border, column.VerticalContentAlignment);

            uiContainer.MinHeight = column.PixelMinHeight;

            // Revert context's value to that of outside the Column
            context.RenderArgs = parentRenderArgs;

            if (updatedRtl)
            {
                context.Rtl = previousContextRtl;
            }

            if (column.BackgroundImage != null)
            {
                uiContainer.SetBackgroundSource(column.BackgroundImage, context);
                if (column.Items.Count == 0)
                {
                    // if we have background source and no children we need to have at least padding to show background image 
                    uiContainer.Children.Add(new Grid() { Margin = new Thickness(context.Config.Spacing.Padding) });
                }
            }

            return RendererUtil.ApplySelectAction(border, column, context);
        }
    }
}
