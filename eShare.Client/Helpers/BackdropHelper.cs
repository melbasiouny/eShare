// Copyright (c) 2023 Mostafa Elbasiouny
//
// This software may be modified and distributed under the terms of the MIT license.
// See the LICENSE file for details.

using WinRT;
using Microsoft.UI.Xaml;
using Microsoft.UI.Composition.SystemBackdrops;
using System.Runtime.InteropServices;

namespace eShare.Client.Helpers;

internal class BackdropHelper
{
    MicaController controller;
    WindowsSystemDispatcherQueueHelper wsdqHelper;
    SystemBackdropConfiguration configurationSource;

    public void ActivateBackdrop()
    {
        if (MicaController.IsSupported())
        {
            wsdqHelper = new WindowsSystemDispatcherQueueHelper();
            wsdqHelper.EnsureWindowsSystemDispatcherQueueController();
            configurationSource = new SystemBackdropConfiguration();

            App.Window.Activated += OnActivated;
            App.Window.Closed += OnClosed;
            ((FrameworkElement)App.Window.Content).ActualThemeChanged += OnThemeChanged;

            configurationSource.IsInputActive = true;
            SetConfigurationSourceTheme();

            controller = new MicaController();
            controller.Kind = MicaKind.BaseAlt;
            controller.AddSystemBackdropTarget(App.Window.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
            controller.SetSystemBackdropConfiguration(configurationSource);
        }
    }

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        if (controller != null)
        {
            controller.Dispose();
            controller = null;
        }

        App.Window.Activated -= OnActivated;
        configurationSource = null;
    }

    private void OnThemeChanged(FrameworkElement sender, object args)
    {
        if (configurationSource != null) SetConfigurationSourceTheme();
    }

    private void SetConfigurationSourceTheme()
    {
        switch (((FrameworkElement)App.Window.Content).ActualTheme)
        {
            case ElementTheme.Dark: configurationSource.Theme = SystemBackdropTheme.Dark; break;
            case ElementTheme.Light: configurationSource.Theme = SystemBackdropTheme.Light; break;
            case ElementTheme.Default: configurationSource.Theme = SystemBackdropTheme.Default; break;
        }
    }

    internal class WindowsSystemDispatcherQueueHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        struct DispatcherQueueOptions
        {
            internal int dwSize;
            internal int threadType;
            internal int apartmentType;
        }

        [DllImport("CoreMessaging.dll")]
        private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);

        object dispatcherQueueController = null;

        public void EnsureWindowsSystemDispatcherQueueController()
        {
            if (Windows.System.DispatcherQueue.GetForCurrentThread() != null) return;

            if (dispatcherQueueController == null)
            {
                DispatcherQueueOptions options;
                options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
                options.threadType = 2;
                options.apartmentType = 2;

                CreateDispatcherQueueController(options, ref dispatcherQueueController);
            }
        }
    }
}
