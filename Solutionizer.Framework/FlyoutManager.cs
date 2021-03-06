﻿using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using MahApps.Metro.Controls;
using Solutionizer.Framework.Converters;

namespace Solutionizer.Framework {
    public interface IFlyoutManager {
        Task ShowFlyout(DialogViewModel viewModel);
        Task<TResult> ShowFlyout<TResult>(DialogViewModel<TResult> viewModel);
    }

    public class FlyoutManager : ObservableCollection<Flyout>, IFlyoutManager {
        public Task ShowFlyout(DialogViewModel viewModel) {
            ShowFlyoutInternal(viewModel);
            return viewModel.Task;
        }

        public Task<TResult> ShowFlyout<TResult>(DialogViewModel<TResult> viewModel) {
            ShowFlyoutInternal(viewModel);
            return viewModel.Task;
        }

        private void ShowFlyoutInternal(IDialogViewModel viewModel) {
            var view = (FrameworkElement)ViewLocator.GetViewForViewModel(viewModel);

            var flyout = view as Flyout ?? new Flyout { Content = view };
            flyout.IsOpen = false;
            flyout.Position = Position.Right;
            flyout.IsModal = true;
            view.HorizontalAlignment = HorizontalAlignment.Left;

            var widthBinding = new Binding {
                Path = new PropertyPath("ActualWidth"),
                Mode = BindingMode.OneWay,
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor) {
                    AncestorType = typeof(MetroWindow)
                },
                Converter = new AddDoubleConverter(),
                ConverterParameter = -150
            };
            flyout.SetBinding(FrameworkElement.WidthProperty, widthBinding);

            EventHandler closedHandler = null;
            closedHandler = (sender, args) => {
                viewModel.Closed -= closedHandler;
                flyout.IsOpen = false;
                DelayedRemove(flyout);
            };
            viewModel.Closed += closedHandler;

            Add(flyout);

            flyout.IsOpen = true;
        }

        private async void DelayedRemove(Flyout flyout) {
            if (flyout.IsOpen == false) {
                await Task.Delay(500);
                Remove(flyout);
            }
        }
    }
}