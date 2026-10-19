using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Phone.Shell;
using Xunit;

namespace Community.Phone.Controls.Toolkit.WPF.Tests
{
    public class ShellTests
    {
        [Fact]
        public void ApplicationBarButtonsRaiseClicksAndExecuteCommands()
        {
            ApplicationBar bar = new ApplicationBar();
            ApplicationBarIconButton button = new ApplicationBarIconButton { Text = "save" };
            ApplicationBarMenuItem menu = new ApplicationBarMenuItem { Text = "settings" };
            bar.Buttons.Add(button);
            bar.MenuItems.Add(menu);

            int clicks = 0;
            int commands = 0;
            object commandParameter = null;
            button.Click += delegate { clicks++; };
            button.Command = new TestCommand(
                parameter =>
                {
                    commands++;
                    commandParameter = parameter;
                    return true;
                },
                _ => true);
            button.CommandParameter = "parameter";
            menu.Click += delegate { clicks++; };

            button.RaiseClick();
            menu.RaiseClick();

            Assert.Equal(2, clicks);
            Assert.Equal(1, commands);
            Assert.Equal("parameter", commandParameter);
            Assert.Single(bar.Buttons);
            Assert.Single(bar.MenuItems);
        }

        [Fact]
        public void ApplicationBarDisabledItemsDoNotRaiseEvents()
        {
            ApplicationBarIconButton button = new ApplicationBarIconButton { IsEnabled = false };
            ApplicationBarMenuItem menu = new ApplicationBarMenuItem { IsEnabled = false };
            int events = 0;
            button.Click += delegate { events++; };
            menu.Click += delegate { events++; };

            button.RaiseClick();
            menu.RaiseClick();

            Assert.Equal(0, events);
        }

        [Fact]
        public void SystemTrayStatePublishesPropertyChanges()
        {
            SystemTrayState state = SystemTrayState.Current;
            int changes = 0;
            PropertyChangedEventHandler handler = delegate { changes++; };
            state.PropertyChanged += handler;
            try
            {
                state.Opacity = 0.5;
                state.BackgroundColor = Colors.Red;
                state.ForegroundColor = Colors.Yellow;
                state.IsVisible = true;
                state.ProgressIndicator.IsVisible = true;
                state.ProgressIndicator.Text = "Loading";

                Assert.Equal(0.5, state.Opacity);
                Assert.Equal(Colors.Red, state.BackgroundColor);
                Assert.Equal(Colors.Yellow, state.ForegroundColor);
                Assert.True(state.IsVisible);
                Assert.True(state.ProgressIndicator.IsVisible);
                Assert.True(changes >= 6);
            }
            finally
            {
                state.PropertyChanged -= handler;
                state.IsVisible = false;
                state.Opacity = 1.0;
                state.BackgroundColor = Colors.Black;
                state.ForegroundColor = Colors.White;
                state.ProgressIndicator.IsVisible = false;
                state.ProgressIndicator.Text = string.Empty;
            }
        }

        private sealed class TestCommand : ICommand
        {
            private readonly Func<object, bool> _execute;
            private readonly Func<object, bool> _canExecute;

            public TestCommand(Func<object, bool> execute, Func<object, bool> canExecute)
            {
                _execute = execute;
                _canExecute = canExecute;
            }

            public event EventHandler CanExecuteChanged
            {
                add { }
                remove { }
            }

            public bool CanExecute(object parameter)
            {
                return _canExecute(parameter);
            }

            public void Execute(object parameter)
            {
                _execute(parameter);
            }
        }
    }
}
