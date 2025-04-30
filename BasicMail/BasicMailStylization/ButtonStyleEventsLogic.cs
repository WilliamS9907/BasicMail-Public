using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace BasicMailStylization
{
    public partial class ButtonStyleEventsLogic
    {
        Button refreshButton = new Button();
        Border highlightRect = new Border();
        TextBlock? textBlockOfButtonDown = new TextBlock();
        RotateTransform? transformOfImageOfRefreshButton;
        Boolean roundedTickSet = false;
        Boolean refreshTickSet = false;
        Boolean refreshLoopAnim = false;
        DispatcherTimer genericHighlightAnimTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMicroseconds(1000)
        };
        public static DispatcherTimer refreshAnimTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMicroseconds(1000)
        };

        #region Mouse Hightlight
        public void RoundedMouseEnter(object sender, MouseEventArgs e)
        {
            highlightRect = GetChildOfGridOfType<Border>((Grid)((Button)sender).Content, typeof(Border));

            if (roundedTickSet == false)
            {
                genericHighlightAnimTimer.Tick += IncreaseOpacityOfHighlightRect!;

                roundedTickSet = true;
            }

            genericHighlightAnimTimer.Start();
        }

        public void RoundedMouseLeave(object sender, MouseEventArgs e)
        {
            genericHighlightAnimTimer.Stop();
            highlightRect.Opacity = 0.0;
        }

        public void StandardMouseEnter(object sender, MouseEventArgs e)
        {
            highlightRect = GetChildOfGridOfType<Border>((Grid)((Button)sender).Content, typeof(Border));
            textBlockOfButtonDown = GetChildOfGridOfType<TextBlock>((Grid)((Button)sender).Content, typeof(TextBlock));

            if (textBlockOfButtonDown != null)
            {
                IncreaseButtonTextBlockFontSize();
            }

            IncreaseButtonWidthAndHeight((Button)sender);
            highlightRect.Opacity = 0.25;
        }

        public void StandardMouseLeave(object sender, MouseEventArgs e)
        {
            if (textBlockOfButtonDown != null)
            {
                DecreaseButtonTextBlockFontSize();
            }

            DecreaseButtonWidthAndHeight((Button)sender);
            highlightRect.Opacity = 0.0;
        }

        public void ToolbarMouseEnter(object sender, MouseEventArgs e)
        {
            highlightRect = GetChildOfGridOfType<Border>((Grid)((Button)sender).Content, typeof(Border));

            highlightRect.Opacity = 0.25;
        }

        public void ToolbarMouseLeave(object sender, MouseEventArgs e)
        {
            highlightRect.Opacity = 0.0;
        }

        public void RefreshMouseEnter(object sender, MouseEventArgs e)
        {
            refreshButton = (Button)sender;
            highlightRect = GetChildOfGridOfType<Border>((Grid)refreshButton.Content, typeof(Border));
            transformOfImageOfRefreshButton = (GetChildOfGridOfType<Image>((Grid)refreshButton.Content, typeof(Image)).RenderTransform as RotateTransform)!;

            if (refreshTickSet == false)
            {
                refreshAnimTimer.Tick += RotateRefreshButton!;

                refreshTickSet = true;
            }

            refreshAnimTimer.Start();

            highlightRect.Opacity = 0.25;
        }

        public void RefreshMouseLeave(object sender, MouseEventArgs e)
        {
            if (!refreshLoopAnim)
            {
                StopRotating();
            }

            highlightRect.Opacity = 0.0;
        }

        private void IncreaseOpacityOfHighlightRect(Object sender, EventArgs e)
        {
            switch (highlightRect.Opacity)
            {
                case var opacity when (highlightRect.Opacity >= 0.25):
                    genericHighlightAnimTimer.Stop();
                    break;

                default:
                    highlightRect.Opacity = highlightRect.Opacity + 0.03;
                    break;
            }
        }
        #endregion Mouse Highlight

        #region Animations
        private void GenericPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            textBlockOfButtonDown = GetChildOfGridOfType<TextBlock>((Grid)((Button)sender).Content, typeof(TextBlock));

            if (textBlockOfButtonDown != null)
            {
                DecreaseButtonTextBlockFontSize();
            }

            DecreaseButtonWidthAndHeight((Button)sender);
        }

        private void GenericPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (textBlockOfButtonDown != null)
            {
                IncreaseButtonTextBlockFontSize();
            }

            IncreaseButtonWidthAndHeight((Button)sender);
        }

        private void RefreshPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (textBlockOfButtonDown != null)
            {
                IncreaseButtonTextBlockFontSize();
            }

            IncreaseButtonWidthAndHeight((Button)sender);

            refreshLoopAnim = true;

            refreshAnimTimer.Start();
        }

        private void RotateRefreshButton(object sender, EventArgs e)
        {
            if (transformOfImageOfRefreshButton!.Angle <= -180
                && !refreshLoopAnim)
            {
                refreshAnimTimer.Stop();
            }
            else
            {
                if (refreshLoopAnim
                    && refreshButton.IsEnabled)
                {
                    StopRotating();
                }

                transformOfImageOfRefreshButton.Angle = transformOfImageOfRefreshButton.Angle - 10;
            }
        }

        private void StopRotating()
        {
            refreshLoopAnim = false;
            transformOfImageOfRefreshButton!.Angle = 0.0;
            refreshAnimTimer.Stop();
        }
        #endregion Animations

        #region Common Element Modification(s)
        private void IncreaseButtonWidthAndHeight(Button sender)
        {
            sender.Width = sender.Width + 1;
            sender.Height = sender.Height + 1;
        }

        private void DecreaseButtonWidthAndHeight(Button sender)
        {
            sender.Width = sender.Width - 1;
            sender.Height = sender.Height - 1;
        }

        private void IncreaseButtonTextBlockFontSize()
        {
            textBlockOfButtonDown!.FontSize = textBlockOfButtonDown.FontSize + 1;
        }

        private void DecreaseButtonTextBlockFontSize()
        {
            textBlockOfButtonDown!.FontSize = textBlockOfButtonDown.FontSize - 1;
        }
        #endregion Common Element Modification(s)

        #region Element Search
        private T GetChildOfGridOfType<T>(Grid container, Type objectType)
        {
            return container.Children
                            .Cast<FrameworkElement>()
                            .Where(x => x.GetType() == objectType)
                            .Cast<T>()
                            .FirstOrDefault()!;
        }
        #endregion Element Search
    }
}
