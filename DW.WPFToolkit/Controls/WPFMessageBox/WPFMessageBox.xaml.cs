﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using DW.WPFToolkit.Interactivity;

namespace DW.WPFToolkit.Controls
{
    public partial class WPFMessageBox
    {
        public WPFMessageBox()
        {
            InitializeComponent();
            DataContext = this;

            AddHandler(WPFMessageBoxButtonsPanel.ClickEvent, (RoutedEventHandler)OnButtonClick);
            AddHandler(WPFMessageBoxButtonsPanel.HelpRequestEvent, (RoutedEventHandler)OnHelpRequestClick);
        }

        private bool _closeByButtons;

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var panel = (WPFMessageBoxButtonsPanel)e.OriginalSource;
            Result = panel.Result;

            _closeByButtons = true;
            DialogResult = true;
        }

        private void OnHelpRequestClick(object sender, RoutedEventArgs e)
        {
            if (Options.HelpRequestCallback != null)
                Options.HelpRequestCallback();
        }

        public string Message { get; set; }
        public WPFMessageBoxImage Image { get; set; }
        public WPFMessageBoxButtons Buttons { get; set; }
        public WPFMessageBoxResult DefaultButton { get; set; }
        public WPFMessageBoxResult Result { get; set; }
        public WPFMessageBoxOptions Options { get; set; }
        
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            if (!Options.ShowSystemMenu)
                WindowTitleBar.DisableSystemMenu(this);
            else
                if (Options.Icon != null)
                    Icon = Options.Icon;

            WindowTitleBar.DisableMinimizeButton(this);
            WindowTitleBar.DisableMaximizeButton(this);
            if (Buttons == WPFMessageBoxButtons.YesNo || Buttons == WPFMessageBoxButtons.AbortRetryIgnore)
                WindowTitleBar.DisableCloseButton(this);
        }

        protected override void OnContentRendered(EventArgs e)
        {
            PART_ButtonPanel.Measure(new Size(double.MaxValue, double.MaxValue));
            var panelWidth = PART_ButtonPanel.DesiredSize.Width;
            if (!double.IsNaN(panelWidth) && panelWidth > MaxWidth)
                MaxWidth = panelWidth + 40;
            base.OnContentRendered(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_closeByButtons && (Buttons == WPFMessageBoxButtons.YesNo || Buttons == WPFMessageBoxButtons.AbortRetryIgnore))
                e.Cancel = true;

            base.OnClosing(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);

            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control && Options.MessageCopyFormatter != null)
                Options.MessageCopyFormatter.Copy(Title, Message, Buttons, Image, Options.Strings);

            if (e.Key != Key.Escape)
                return;

            if (Buttons == WPFMessageBoxButtons.AbortRetryIgnore || Buttons == WPFMessageBoxButtons.YesNo)
                return;

            Close();
        }

        public static WPFMessageBoxResult Show(Window owner,
                                               string messageBoxText,
                                               string caption,
                                               WPFMessageBoxButtons buttons,
                                               WPFMessageBoxImage icon,
                                               WPFMessageBoxResult defaultButton,
                                               WPFMessageBoxOptions options)
        {
            if (options == null)
                options = new WPFMessageBoxOptions();

            var box = new WPFMessageBox();
            box.Owner = owner;
            box.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (box.Owner != null)
                box.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (options.StartupLocation != null)
                box.WindowStartupLocation = options.StartupLocation.Value;
            box.SizeToContent = SizeToContent.WidthAndHeight;
            box.SnapsToDevicePixels = true;
            box.ShowInTaskbar = options.ShowInTaskbar;
            box.ResizeMode = options.ResizeMode;

            //box.Left
            //box.Top
            box.MinWidth = 249;
            box.MaxWidth = 494;
            box.Width = 349;
            box.MinHeight = 172;
            //box.MaxHeight
            box.Height = 172;
            //box.Style

            box.Message = messageBoxText;
            box.Title = caption ?? string.Empty;
            box.Buttons = buttons;
            box.Image = icon;
            box.DefaultButton = defaultButton;
            box.Options = options;
            var dialogResult = box.ShowDialog();
            if (dialogResult != true)
            {
                if (buttons == WPFMessageBoxButtons.OK)
                    return WPFMessageBoxResult.OK;
                return WPFMessageBoxResult.Cancel;
            }
            return box.Result;
        }
    }
}