﻿using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fluent
{
    /// <summary>
    /// This interface must be implemented for controls
    /// which are intended to insert to quick access toolbar
    /// </summary>
    public interface IQuickAccessItemProvider
    {
        /// <summary>
        /// Gets control which represents quick access toolbar item.
        /// This item MUST be syncronized with the original 
        /// and send command to original one control
        /// </summary>
        /// <returns>Control which represents quick access toolbar item</returns>
        UIElement GetQuickAccessToolbarItem();
    }

    /// <summary>
    /// The class responds to mine controls for QuickAccessToolbar
    /// </summary>
    public static class QuickAccessItemsProvider
    {
        #region Public Methods

        /// <summary>
        /// Determines whether the given control can provide a quick access toolbar item
        /// </summary>
        /// <param name="element">Control</param>
        /// <returns>True if this control is able to provide
        /// a quick access toolbar item, false otherwise</returns>
        public static bool IsSupported(UIElement element)
        {
            if (element is IQuickAccessItemProvider) return true;
            if ((element is Button) ||
                (element is CheckBox) ||
                (element is RadioButton) ||
                (element is ComboBox) ||
                (element is TextBox)) return true;
            else return false;
        }

        /// <summary>
        /// Gets control which represents quick access toolbar item
        /// </summary>
        /// <param name="element">Host control</param>
        /// <returns>Control which represents quick access toolbar item</returns>
        public static UIElement GetQuickAccessItem(UIElement element)
        {
            UIElement result = null;

            // If control supports the interface just return what it provides            
            if (element is IQuickAccessItemProvider) result = (element as IQuickAccessItemProvider).GetQuickAccessToolbarItem();

            // Predefined controls            
            else if (element is TextBox) result = GetTextBoxQuickAccessItem(element as TextBox);
            else if (element is ComboBox) result = GetComboBoxQuickAccessItem(element as ComboBox);
            else if (element is Button) result = GetButtonQuickAccessItem(element as Button);
            else if (element is ToggleButton) result = GetToggleButtonQuickAccessItem(element as ToggleButton);

            // The control isn't supported
            if (element == null) throw new ArgumentException("The contol " + element.GetType().Name + " is not able to provide a quick access toolbar item");
                        
            return result;
        }
        
        /// <summary>
        /// Finds the top supported control and gets quick access item from it
        /// </summary>
        /// <param name="visual">Visual</param>
        /// <param name="point">Point</param>
        /// <returns>Point</returns>
        public static UIElement PickQuickAccessItem(Visual visual, Point point)
        {
            UIElement element = FindSupportedControl(visual, point);
            if (element != null) return GetQuickAccessItem(element);
            else return null;
        }

        /// <summary>
        /// Finds the top supported control
        /// </summary>
        /// <param name="visual">Visual</param>
        /// <param name="point">Point</param>
        /// <returns>Point</returns>
        public static UIElement FindSupportedControl(Visual visual, Point point)
        {
            HitTestResult result = VisualTreeHelper.HitTest(visual, point);
            UIElement element = result.VisualHit as UIElement;
            while (element != null)
            {
                if(IsSupported(element)) return element;
                element = VisualTreeHelper.GetParent(element) as UIElement;
            }
            return null;
        }

        #endregion

        #region Buttons

        static Dictionary<Button, Button> cachedQuickAccessButtons = new Dictionary<Button, Button>();
        static UIElement GetButtonQuickAccessItem(Button button)
        {
            if (cachedQuickAccessButtons.ContainsKey(button)) return cachedQuickAccessButtons[button];

            Button item = new Button();
            item.Focusable = false;


            // Copy common properties
            BindControlProperties(button, item);
            // Copy ScreenTip data
            BindScreenTip(button, item);
            // Bind small icon
            // Copy small icon
            if (RibbonControl.GetSmallIcon(button) != null)
                Bind(button, item, "(Fluent:RibbonControl.SmallIcon)",
                    RibbonControl.SmallIconProperty, BindingMode.OneWay);

            // TODO: check, maybe copy style is not required for quick access toolbar items
            Bind(button, item, "Style", Button.StyleProperty, BindingMode.OneWay);
            RibbonControl.SetSize(item, RibbonControlSize.Small);


            // Syncronization
            item.Click += OnButtonClick;

            cachedQuickAccessButtons.Add(button, item);
            return item;
        }

        static void OnButtonClick(object sender, RoutedEventArgs e)
        {
            // Redirect to the host control
            cachedQuickAccessButtons.Where(x=>x.Value == sender).First().Key.RaiseEvent(e);
        }

        #endregion

        #region CheckBoxes & RadioButtons (ToggleButtons)

        static Dictionary<ToggleButton, ToggleButton> cachedQuickAccessToggleButtons = new Dictionary<ToggleButton, ToggleButton>();
        static UIElement GetToggleButtonQuickAccessItem(ToggleButton toggleButton)
        {
            if (cachedQuickAccessToggleButtons.ContainsKey(toggleButton)) return cachedQuickAccessToggleButtons[toggleButton];

            ToggleButton item = new ToggleButton();
            item.Focusable = false;
            item.Content = (toggleButton.Content != null) ? toggleButton.Content.ToString() : null;
            
            // Copy common properties
            BindControlProperties(toggleButton, item);
            // Copy ScreenTip data
            BindScreenTip(toggleButton, item);
            // Copy small icon
            if (RibbonControl.GetSmallIcon(toggleButton) != null) 
                Bind(toggleButton, item, "(Fluent:RibbonControl.SmallIcon)", 
                    RibbonControl.SmallIconProperty, BindingMode.OneWay);
            
            // TODO: check, maybe copy style is not required for quick access toolbar items
            Bind(toggleButton, item, "Style", ToggleButton.StyleProperty, BindingMode.OneWay);

            // Syncronization            
            Bind(toggleButton, item, "IsChecked", ToggleButton.IsCheckedProperty, BindingMode.TwoWay);

            cachedQuickAccessToggleButtons.Add(toggleButton, item);
            return item;
        }

        #endregion

        #region ComboBoxes

        static Dictionary<ComboBox, ComboBox> cachedQuickAccessComboBoxes = new Dictionary<ComboBox, ComboBox>();
        static UIElement GetComboBoxQuickAccessItem(ComboBox comboBox)
        {
            if (cachedQuickAccessComboBoxes.ContainsKey(comboBox)) return cachedQuickAccessComboBoxes[comboBox];

            ComboBox item = new ComboBox();
            item.Focusable = false;
            

            // Copy common properties
            BindControlProperties(comboBox, item);
            // Copy ScreenTip data
            BindScreenTip(comboBox, item);
            // Copy small icon
            if (RibbonControl.GetSmallIcon(comboBox) != null)
                Bind(comboBox, item, "(Fluent:RibbonControl.SmallIcon)",
                    RibbonControl.SmallIconProperty, BindingMode.OneWay);


            Bind(comboBox, item, "Width", ComboBox.WidthProperty, BindingMode.OneWay);
            Bind(comboBox, item, "Heigth", ComboBox.HeightProperty, BindingMode.OneWay);
            Bind(comboBox, item, "MaxDropDownHeight", ComboBox.MaxDropDownHeightProperty, BindingMode.OneWay);
            Bind(comboBox, item, "StaysOpenOnEdit", ComboBox.StaysOpenOnEditProperty, BindingMode.OneWay);
            

            Bind(comboBox, item, "IsEditable", ComboBox.IsEditableProperty, BindingMode.OneWay);
            Bind(comboBox, item, "IsReadOnly", ComboBox.IsReadOnlyProperty, BindingMode.TwoWay);
            Bind(comboBox, item, "Text", ComboBox.TextProperty, BindingMode.TwoWay);

            item.ItemsSource = comboBox.Items;            
            Bind(comboBox, item, "Items", ComboBox.ItemsSourceProperty, BindingMode.OneWay);

            cachedQuickAccessComboBoxes.Add(comboBox, item);
            return item;
        }

        #endregion

        #region TextBoxes

        static Dictionary<TextBox, TextBox> cachedQuickAccessTextBoxes = new Dictionary<TextBox, TextBox>();
        static UIElement GetTextBoxQuickAccessItem(TextBox textBox)
        {
            if (cachedQuickAccessTextBoxes.ContainsKey(textBox)) return cachedQuickAccessTextBoxes[textBox];

            TextBox item = new TextBox();
            //item.Focusable = false;
            Bind(textBox, item, "Width", Control.WidthProperty, BindingMode.OneWay);
            Bind(textBox, item, "Heigth", Control.HeightProperty, BindingMode.OneWay);

            // Copy common properties
            BindControlProperties(textBox, item);
            // Copy ScreenTip data
            BindScreenTip(textBox, item);
            // Copy small icon
            if (RibbonControl.GetSmallIcon(textBox) != null)
                Bind(textBox, item, "(Fluent:RibbonControl.SmallIcon)",
                    RibbonControl.SmallIconProperty, BindingMode.OneWay);
                        
            Bind(textBox, item, "IsReadOnly", TextBox.IsReadOnlyProperty, BindingMode.OneWay);
            //Bind(textBox, item, "Text", TextBox.TextProperty, BindingMode.OneWay);
            Bind(textBox, item, "CharacterCasing", TextBox.CharacterCasingProperty, BindingMode.OneWay);
            Bind(textBox, item, "MaxLength", TextBox.MaxLengthProperty, BindingMode.OneWay);
            Bind(textBox, item, "MaxLines", TextBox.MaxLinesProperty, BindingMode.OneWay);
            Bind(textBox, item, "MinLines", TextBox.MinLinesProperty, BindingMode.OneWay);
            Bind(textBox, item, "TextAlignment", TextBox.TextAlignmentProperty, BindingMode.OneWay);
            Bind(textBox, item, "TextDecorations", TextBox.TextDecorationsProperty, BindingMode.OneWay);
            Bind(textBox, item, "TextWrapping", TextBox.TextWrappingProperty, BindingMode.OneWay);

            // Binding for Text we have to do manually, 
            // because the binding doesn't work properly 
            // if focus will be remain in one of the controls
            item.Text = textBox.Text;
            textBox.TextChanged += delegate { item.Text = textBox.Text; };
            item.TextChanged += delegate { textBox.Text = item.Text; };

            cachedQuickAccessTextBoxes.Add(textBox, item);
            return item;
        }

        #endregion
        
        #region Common Stuff

        // Copies ScreenTip data
        static void BindScreenTip(FrameworkElement source, FrameworkElement target)
        {            
            Bind(source, target, "(Fluent:ScreenTip.Title)", ScreenTip.TitleProperty, BindingMode.OneWay);
            Bind(source, target, "(Fluent:ScreenTip.Text)", ScreenTip.TextProperty, BindingMode.OneWay);
            Bind(source, target, "(Fluent:ScreenTip.Image)", ScreenTip.ImageProperty, BindingMode.OneWay);
            Bind(source, target, "(Fluent:ScreenTip.DisableReason)", ScreenTip.DisableReasonProperty, BindingMode.OneWay);
            Bind(source, target, "(Fluent:ScreenTip.HelpTopic)", ScreenTip.HelpTopicProperty, BindingMode.OneWay);
            Bind(source, target, "(Fluent:ScreenTip.Width)", ScreenTip.WidthProperty, BindingMode.OneWay);
            Bind(source, target, "ToolTip", FrameworkElement.ToolTipProperty, BindingMode.OneWay);
        }


        static void BindControlProperties(Control source, Control target)
        {
            if (source is ButtonBase)
            {
                Bind(source, target, "Command", ButtonBase.CommandProperty, BindingMode.OneWay);
                Bind(source, target, "CommandParameter", ButtonBase.CommandParameterProperty, BindingMode.OneWay);
                Bind(source, target, "CommandTarget", ButtonBase.CommandTargetProperty, BindingMode.OneWay);
                Bind(source, target, "Command", ButtonBase.CommandProperty, BindingMode.OneWay);
            }

            Bind(source, target, "ContextMenu", Control.ContextMenuProperty, BindingMode.OneWay);

            Bind(source, target, "FontFamily", Control.FontFamilyProperty, BindingMode.OneWay);
            Bind(source, target, "FontSize", Control.FontSizeProperty, BindingMode.OneWay);
            Bind(source, target, "FontStretch", Control.FontStretchProperty, BindingMode.OneWay);
            Bind(source, target, "FontStyle", Control.FontStyleProperty, BindingMode.OneWay);
            Bind(source, target, "FontWeight", Control.FontWeightProperty, BindingMode.OneWay);

            Bind(source, target, "Foreground", Control.ForegroundProperty, BindingMode.OneWay);
            Bind(source, target, "IsEnabled", Control.IsEnabledProperty, BindingMode.OneWay);
            Bind(source, target, "Opacity", Control.OpacityProperty, BindingMode.OneWay);
            Bind(source, target, "SnapsToDevicePixels", Control.SnapsToDevicePixelsProperty, BindingMode.OneWay);
            Bind(source, target, "Visibility", Control.VisibilityProperty, BindingMode.OneWay);            
        }

        #endregion

        #region Binding

        static void Bind(FrameworkElement source, FrameworkElement target, string path, DependencyProperty property)
        {
            Bind(source, target, path, property, BindingMode.OneWay);
        }

        static void Bind(FrameworkElement source, FrameworkElement target, string path, DependencyProperty property, BindingMode mode)
        {
            Binding binding = new Binding();
            binding.Path = new PropertyPath(path);
            binding.Source = source;
            binding.Mode = mode;
            target.SetBinding(property, binding);
        }

        #endregion
    }
}
