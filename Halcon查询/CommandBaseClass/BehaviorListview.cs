using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using MyCommands;

namespace Halcon查询.CommandBaseClass
{
    public class ListViewDoubleClickBehavior:Behavior<System.Windows.Controls.ListView>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(CommandsBaseClass),
                typeof(ListViewDoubleClickBehavior));

        public CommandsBaseClass Command
        {
            get => (CommandsBaseClass)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseDoubleClick += OnMouseDoubleClick;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseDoubleClick -= OnMouseDoubleClick;
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AssociatedObject.SelectedItem != null && Command?.CanExecute(AssociatedObject.SelectedItem) == true)
            {
                Command.Execute(AssociatedObject.SelectedItems[0]);

            }
        }
    }
}
