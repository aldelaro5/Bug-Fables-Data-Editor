﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BugFablesEntityEditor.Views
{
  public class WindowsTitleBar : UserControl
  {
    private Button minimizeButton;
    private Button maximizeButton;
    private Path maximizeIcon;
    private ToolTip maximizeToolTip;
    private Button closeButton;
    private Image windowIcon;

    public WindowsTitleBar()
    {
      this.InitializeComponent();

      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
      {
        this.IsVisible = false;
      }
      else
      {
        minimizeButton = this.FindControl<Button>("MinimizeButton");
        maximizeButton = this.FindControl<Button>("MaximizeButton");
        maximizeIcon = this.FindControl<Path>("MaximizeIcon");
        maximizeToolTip = this.FindControl<ToolTip>("MaximizeToolTip");
        closeButton = this.FindControl<Button>("CloseButton");
        windowIcon = this.FindControl<Image>("WindowIcon");

        minimizeButton.Click += MinimizeWindow;
        maximizeButton.Click += MaximizeWindow;
        closeButton.Click += CloseWindow;
        windowIcon.DoubleTapped += CloseWindow;

        SubscribeToWindowState();
      }
    }

    private void CloseWindow(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
      Window hostWindow = (Window)this.VisualRoot;
      hostWindow.Close();
    }

    private void MaximizeWindow(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
      Window hostWindow = (Window)this.VisualRoot;

      if (hostWindow.WindowState == WindowState.Normal)
      {
        hostWindow.WindowState = WindowState.Maximized;
      }
      else
      {
        hostWindow.WindowState = WindowState.Normal;
      }
    }

    private void MinimizeWindow(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
      Window hostWindow = (Window)this.VisualRoot;
      hostWindow.WindowState = WindowState.Minimized;
    }

    private async void SubscribeToWindowState()
    {
      Window hostWindow = (Window)this.VisualRoot;

      while (hostWindow == null)
      {
        hostWindow = (Window)this.VisualRoot;
        await Task.Delay(50);
      }

      hostWindow.GetObservable(Window.WindowStateProperty).Subscribe(s =>
      {
        if (s != WindowState.Maximized)
        {
          maximizeIcon.Data = Avalonia.Media.Geometry.Parse("M2048 2048v-2048h-2048v2048h2048zM1843 1843h-1638v-1638h1638v1638z");
          hostWindow.Padding = new Thickness(0, 0, 0, 0);
          maximizeToolTip.Content = "Maximize";
        }
        if (s == WindowState.Maximized)
        {
          maximizeIcon.Data = Avalonia.Media.Geometry.Parse("M2048 1638h-410v410h-1638v-1638h410v-410h1638v1638zm-614-1024h-1229v1229h1229v-1229zm409-409h-1229v205h1024v1024h205v-1229z");
          hostWindow.Padding = new Thickness(7, 7, 7, 7);
          maximizeToolTip.Content = "Restore Down";
        }
      });
    }

    private void InitializeComponent()
    {
      AvaloniaXamlLoader.Load(this);
    }
  }
}
