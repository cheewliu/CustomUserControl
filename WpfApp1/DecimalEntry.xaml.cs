using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

// ****************************** PLEASE NOTE ****************************************
// This file is essentially a duplicate of NumericEntry.xaml.cs (with 'double' replaced
// with 'decimal').  Any changes in one should be made in both!
// TODO: refactor NumericEntry and DecimalEntry to extract common code (and reduce maintenance issues)
// ***********************************************************************************
namespace Generic
{
    /// <summary>
    /// DecimalEntry is an aggregate container for a numeric entry field (derived from
    /// TextBox) and associated buttons (min, max, up, down, etc).
    /// 
    /// Please realize that almost every interaction with the Focus of the controls is
    /// critical for proper operation (i.e. don't change anything without exhaustive
    /// testing).
    /// </summary>
    public partial class DecimalEntry : UserControl, INotifyPropertyChanged
    {
        public DecimalEntry()
        {
            InitializeComponent();
            mEntryTextBox.SetDecimalEntry(this);
            UnitList = new List<UnitMultiplier>();
        }

        #region Event Handlers (most simply delegated to mEntryTextBox)
        private void OnMinButtonClick(object sender, RoutedEventArgs e)
        {
            mEntryTextBox.ProcessButton(SpecialButtons.MinButton);
        }

        private void OnDownButtonClick(object sender, RoutedEventArgs e)
        {
            if (UpDownButtonIncrement == 0)
            {
                // Dynamic increment depends on cursor position
                mEntryTextBox.ProcessButton(SpecialButtons.DownButton);
            }
            else
            {
                // Fixed increment
                // NOTE: normally the entry (text) is converted to Decimal (i.e. no loss in precision) but to use
                //       an absolute increment (and not have to rewrite all the number processing) we need to use
                //       'Value' here (a decimal) which opens up the possibility of rounding.  Try to minimize this
                //       by performing all computations as Decimal.
                Value = Math.Max(Value - UpDownButtonIncrement, Min);
            }
        }

        private void OnZeroButtonClick(object sender, RoutedEventArgs e)
        {
            mEntryTextBox.ProcessButton(SpecialButtons.ZeroButton);
        }

        private void OnUpButtonClick(object sender, RoutedEventArgs e)
        {
            if (UpDownButtonIncrement == 0)
            {
                // Dynamic increment depends on cursor position
                mEntryTextBox.ProcessButton(SpecialButtons.UpButton);
            }
            else
            {
                // Fixed increment
                // NOTE: normally the entry (text) is converted to Decimal (i.e. no loss in precision) but to use
                //       an absolute increment (and not have to rewrite all the number processing) we need to use
                //       'Value' here (a decimal) which opens up the possibility of rounding.  Try to minimize this
                //       by performing all computations as Decimal.
                Value = Math.Min(Value + UpDownButtonIncrement, Max);
            }
        }

        private void OnMaxButtonClick(object sender, RoutedEventArgs e)
        {
            mEntryTextBox.ProcessButton(SpecialButtons.MaxButton);
        }

        /// <summary>
        /// Intercept the mouse wheel and interpret as up/down buttons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                if (MouseWheelIncrement == 0)
                {
                    // Dynamic increment depends on cursor position
                    mEntryTextBox.ProcessButton(SpecialButtons.DownButton);
                }
                else
                {
                    // Fixed increment
                    // NOTE: normally the entry (text) is converted to Decimal (i.e. no loss in precision) but to use
                    //       an absolute increment (and not have to rewrite all the number processing) we need to use
                    //       'Value' here (a decimal) which opens up the possibility of rounding.  Try to minimize this
                    //       by performing all computations as Decimal.
                    Value = Math.Max(Value - MouseWheelIncrement, Min);
                }
                e.Handled = true;
            }
            if (e.Delta > 0)
            {
                if (MouseWheelIncrement == 0)
                {
                    // Dynamic increment depends on cursor position
                    mEntryTextBox.ProcessButton(SpecialButtons.UpButton);
                }
                else
                {
                    // Fixed increment
                    // NOTE: normally the entry (text) is converted to Decimal (i.e. no loss in precision) but to use
                    //       an absolute increment (and not have to rewrite all the number processing) we need to use
                    //       'Value' here (a decimal) which opens up the possibility of rounding.  Try to minimize this
                    //       by performing all computations as Decimal.
                    Value = Math.Min(Value + MouseWheelIncrement, Max);
                }
                e.Handled = true;
            }
        }

        /// <summary>
        /// Intercept double click and select all of the text (since there is
        /// a space between the value and units/terminator, the default double
        /// click only selects a portion of the string).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // The source is a TextBoxView which is a private type -- hence the name check
            if (e.OriginalSource.GetType().Name.Contains("Text"))
            {
                mEntryTextBox.SelectAll();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Force the width of the sending control to be  .8 the height of the parent control.
        /// This is used to force a constant aspect ratio of the up/down buttons of the
        /// numeric control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;
            if (control != null && e.HeightChanged)
            {
                FrameworkElement parent = control.Parent as FrameworkElement;
                if (parent != null)
                {
                    control.Width = parent.ActualHeight / 1.25;
                }
            }
        }

        #endregion

        #region Appearance Properties (Button visibility, etc. Most simply delegated to mEntryTextBox)
        public bool HasMinButton
        {
            get { return mMinButton.Visibility == Visibility.Visible; }
            set { mMinButton.Visibility = (value) ? Visibility.Visible : Visibility.Collapsed; }
        }

        public bool HasMaxButton
        {
            get { return mMaxButton.Visibility == Visibility.Visible; }
            set { mMaxButton.Visibility = (value) ? Visibility.Visible : Visibility.Collapsed; }
        }

        public bool HasZeroButton
        {
            get { return mZeroButton.Visibility == Visibility.Visible; }
            set { mZeroButton.Visibility = (value) ? Visibility.Visible : Visibility.Collapsed; }
        }

        public bool HasSmallButtons
        {
            get { return mSmallButtons.Visibility == Visibility.Visible; }
            set
            {
                if (value)
                {
                    mSmallButtons.Visibility = Visibility.Visible;
                    mUpBtn.Visibility = Visibility.Collapsed;
                    mDownBtn.Visibility = Visibility.Collapsed;
                }
                else
                {
                    mSmallButtons.Visibility = Visibility.Collapsed;
                    mUpBtn.Visibility = Visibility.Visible;
                    mDownBtn.Visibility = Visibility.Visible;
                }
            }
        }
        #endregion

        #region Number Properties (Digits, Resolution, ...)

        #region UnitList property
        public List<UnitMultiplier> UnitList
        {
            get { return (List<UnitMultiplier>)GetValue(UnitListProperty); }
            set
            {
                if (!ReferenceEquals(value, GetValue(UnitListProperty)))
                {
                    SetValue(UnitListProperty, value); 
                }
            }
        }

        // Using a DependencyProperty as the backing store for Min.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitListProperty =
            DependencyProperty.Register("UnitList", typeof(List<UnitMultiplier>), typeof(DecimalEntry), new UIPropertyMetadata(null, UnitListChangeCallback));

        /// <summary>
        /// Callback for the a change to the UnitList property.  
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void UnitListChangeCallback(DependencyObject source,
        DependencyPropertyChangedEventArgs e)
        {
            List<UnitMultiplier> newValue = (List<UnitMultiplier>)e.NewValue;
            DecimalEntry entry = source as DecimalEntry;
            if (entry != null)
            {
                entry.mEntryTextBox.UnitList = newValue;
                entry.UnitList = newValue;
            }
        }
        #endregion UnitList property

        private string _label = "";
        public string label
        {
            get { return _label; }
            set
            {
                _label = value;
                OnPropertyChanged(nameof(label));
            }
        }

        private Visibility _labelVisibility = Visibility.Visible;
        public Visibility labelVisibility
        {
            get { return _labelVisibility; }
            set
            {
                _labelVisibility = value;
                OnPropertyChanged(nameof(labelVisibility));
            }
        }

        public bool ShowAllDigits
        {
            get { return mEntryTextBox.ShowAllDigits; }
            set { mEntryTextBox.ShowAllDigits = value; }
        }

        public int Digits
        {
            get { return mEntryTextBox.Digits; }
            set { mEntryTextBox.Digits = value; }
        }

        #region Resolution property
        public decimal Resolution
        {
            get { return (decimal)GetValue(ResolutionProperty); }
            set
            {
                if (value != (decimal)GetValue(ResolutionProperty))
                {
                    SetValue(ResolutionProperty, value);
                }
            }
        }

        // Using a DependencyProperty as the backing store for Resolution.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResolutionProperty =
            DependencyProperty.Register("Resolution", typeof(decimal), typeof(DecimalEntry), new UIPropertyMetadata(Decimal.MinValue, ResolutionChangeCallback));

        /// <summary>
        /// Callback for the a change to the Resolution property.  
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void ResolutionChangeCallback(DependencyObject source,
        DependencyPropertyChangedEventArgs e)
        {
            decimal newValue = (decimal)e.NewValue;
            DecimalEntry entry = source as DecimalEntry;
            if (entry != null)
            {
                entry.mEntryTextBox.Resolution = newValue;
                entry.Resolution = newValue;
            }
        }
        #endregion Resolution property
        
        /// <summary>
        /// UpDownButtonIncrement, when non-zero, specifies the increment used by the up/down spin
        /// buttons regardless of the cursor position in the text (does NOT affect keyboard
        /// up/down arrows or mouse wheel).  For example, if this value is 0.25 and the control is
        /// 1|2.35 (cursor at tens position) clicking the up arrow on the keyboard results
        /// in 2|2.35 but clicking the up spin button results in 1|2.60.
        /// 
        /// If 0, the increment is determined by the cursor position (the original and default
        /// behavior).
        /// 
        /// If non-zero, this value overrides PreferUnitStep and PreferredIncrement.
        /// </summary>
        public decimal UpDownButtonIncrement
        {
            get;
            set;
        }

        /// <summary>
        /// MouseWheelIncrement, when non-zero, specifies the increment used by the mouse
        /// wheel regardless of the cursor position in the text (does NOT affect keyboard
        /// up/down arrows or spin buttons).  For example, if this value is 0.25 and the control
        /// is 1|2.35 (cursor at tens position) clicking the up arrow on the keyboard results
        /// in 2|2.35 but clicking the up spin button results in 1|2.60
        /// 
        /// If 0, the increment is determined by the cursor position (the original and default
        /// behavior).
        /// 
        /// If non-zero, this value overrides PreferUnitStep and PreferredIncrement.
        /// </summary>
        public decimal MouseWheelIncrement
        {
            get;
            set;
        }

        /// <summary>
        /// PLEASE READ THIS ENTIRE COMMENT (there are non-obvious effects using this)
        /// 
        /// This value affects how the up/down increment is chosen when the cursor is not
        /// at a digit position (normally if the control has not received focus yet or if
        /// the cursor is positioned in the terminator).
        /// 
        /// This value overrides PreferUnitStep.
        /// 
        /// If the cursor is at a digit position, the value of PreferredIncrement is ignored and
        /// the increment is dynamically adjusted to increment the digit at the cursor position.
        /// For example, 1|000->2|000, 43|1.3->441.3, -3.5|4->-3.44.
        /// 
        /// When zero (the default) and the cursor is anywhere past the end of the number
        /// the increment applies to the least significant digit that is currently displayed.
        /// For example, 1000->1001, 23.1->23.2, -41.2345->-41.2344. This has been the default
        /// behavior for DecimalEntry since originally implemented. 
        /// 
        /// When non-zero and the cursor is at least one position past the last displayed digit,
        /// PreferredIncrement is used instead of the dynamically determined step.  Since
        /// this value is fixed, it may cause a very "large" or very "small" change in the
        /// value (e.g. 1 uV -> 100.001 mV,  100 V -> 100.1 V) so normally this should only
        /// be used with values limited to relatively small ranges.
        /// 
        /// NOW FOR THE CAVEATS:
        /// 1) In order to detect this case (the cursor "past" the number), this only works
        ///    if the number uses a terminator (otherwise the cursor position is always at
        ///    a valid digit position)
        /// 2) In order for the up/down buttons to work with a digit position the user has
        ///    selected, the digit position has to be retained even after the text box has
        ///    lost focus.  This means there's a "history" to how the control works:
        ///    a) If the control has never had focus, the cursor is not displayed, the
        ///       assumed cursor position is at the end of the text, and (if the number has
        ///       a terminator) the increment will apply to the unit position.
        ///    b) If the user sets focus to a digit position, set focus to another control, and
        ///       clicks the up (or down) button then the increment will apply to the digit at
        ///       the cursor position but since the control does not have focus the cursor is
        ///       not visible AND THE CONTROL APPEARS IDENTICAL TO THE PREVIOUS CASE.  Hence the
        ///       user cannot know simply by inspection whether the up/down keys will increment
        ///       the unit position or another position.
        /// </summary>
        public decimal PreferredIncrement
        {
            get { return mEntryTextBox.PreferredIncrement; }
            set { mEntryTextBox.PreferredIncrement = value; }
        }

        /// <summary>
        /// PLEASE READ THIS ENTIRE COMMENT (there are non-obvious effects using this)
        /// 
        /// This value affects how the up/down increment is chosen when the cursor is not
        /// at a digit position (normally if the control has not received focus yet or if
        /// the cursor is positioned in the terminator).
        /// 
        /// This value is ignored if PreferredIncrement is non-zero.
        /// 
        /// If the cursor is at a digit position, the value of PreferUnitStep is ignored and
        /// the increment is dynamically adjusted to increment the digit at the cursor position.
        /// For example, 1|000->2|000, 43|1.3->441.3, -3.5|4->-3.44.
        /// 
        /// When false (the default) and the cursor is anywhere past the end of the number
        /// the increment applies to the least significant digit that is currently displayed.
        /// For example, 1000->1001, 23.1->23.2, -41.2345->-41.2344. This has been the default
        /// behavior for DecimalEntry since originally implemented. 
        /// 
        /// When true and the cursor is at least one position past the last displayed digit,
        /// the increment is dynamically adjusted to increment the unit position regardless
        /// of the digits currently displayed.  For example, 1000->1001, 23.1->24.1,
        /// -41.2345->-40.2345.  NOW FOR THE CAVEATS:
        /// 1) In order to detect this case (the cursor "past" the number), this only works
        ///    if the number uses a terminator (otherwise the cursor position is always at
        ///    a valid digit position)
        /// 2) In order for the up/down buttons to work with a digit position the user has
        ///    selected, the digit position has to be retained even after the text box has
        ///    lost focus.  This means there's a "history" to how the control works:
        ///    a) If the control has never had focus, the cursor is not displayed, the
        ///       assumed cursor position is at the end of the text, and (if the number has
        ///       a terminator) the increment will apply to the unit position.
        ///    b) If the user sets focus to a digit position, set focus to another control, and
        ///       clicks the up (or down) button then the increment will apply to the digit at
        ///       the cursor position but since the control does not have focus the cursor is
        ///       not visible AND THE CONTROL APPEARS IDENTICAL TO THE PREVIOUS CASE.  Hence the
        ///       user cannot know simply by inspection whether the up/down keys will increment
        ///       the unit position or another position.
        /// </summary>
        public bool PreferUnitStep
        {
            get { return mEntryTextBox.PreferUnitStep; }
            set { mEntryTextBox.PreferUnitStep = value; }
        }

        public UnitMultiplier DisplayUnits
        {
            get { return mEntryTextBox.DisplayUnits; }
            set { mEntryTextBox.DisplayUnits = value; }
        }

        public bool IsPerDiv
        {
            get { return mEntryTextBox.IsPerDiv; }
            set { mEntryTextBox.IsPerDiv = value; }
        }
        #endregion

        #region TextBoxDouble compatability (Change Listener, Notification, ...)
        public const int EDIT_BEGIN = 0;
        public const int EDIT_END = 1;
        public delegate void EditChangedEventHandler(int editState);
        public event EditChangedEventHandler EditListener;
        private bool mHasChanged;

        public bool HasChanged
        {
            get
            {
                bool temp = mHasChanged;
                mHasChanged = false;
                return temp;
            }
        }

        public string FieldName { get; set; }
        #endregion

        #region Dependency Properties (Value, Min, Max)

        #region Value property and supporting methods
        // Using a DependencyProperty as the backing store for MyProperty.  
        // This enables binding to the Value property

        private bool mForceUpdate;

        /// <summary>
        /// Enable/disable updating the DependencyProperty when the user interacts with
        /// the control even though the value has not changed.  If false (the default)
        /// the DependencyProperty is called only when the value changes (e.g. 1->2).
        /// If true, the DependencyProperty is call when any event that *might* have
        /// changed the value occurs (lost focus, re-entry of text, etc.)
        /// </summary>
        public bool IsForceUpdate
        {
            get { return mForceUpdate; }
            set { mForceUpdate = value; }
        }

        /// <summary>
        /// Force a reformat of the text with the current value
        /// </summary>
        public void Refresh()
        {
            mEntryTextBox.FormatEntry(Value);
        }

        public decimal Value
        {
            get { return (decimal)GetValue(ValueProperty); }
            set
            {
                if (IsForceUpdate || value != (decimal)GetValue(ValueProperty))
                {
                    SetValue(ValueProperty, value);
                }
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(decimal), typeof(DecimalEntry), new UIPropertyMetadata((decimal)0.0, ValueChangeCallback, ValueCoerceCallback));

        /// <summary>
        /// Callback for the a change to the Value property.
        /// Call the format function for the displayed text
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void ValueChangeCallback(DependencyObject source,
        DependencyPropertyChangedEventArgs e)
        {
            decimal newValue = (decimal)e.NewValue;
            DecimalEntry entry = source as DecimalEntry;
            if (entry != null)
            {
                entry.mEntryTextBox.FormatEntry(newValue);
                entry.Value = newValue;
                // Announce change...
                entry.mHasChanged = true;
                if (entry.EditListener != null)
                {
                    entry.EditListener(EDIT_END);
                }
            }
        }
        /// <summary>
        /// Coerce callback for the Value property.  
        /// Forces the value to be in the specified range.
        /// NOTE: coercions done here do not result in calling the property bound to Value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static object ValueCoerceCallback(DependencyObject sender, object data)
        {
            decimal newValue = (decimal)data;

            DecimalEntry entry = sender as DecimalEntry;
            if (entry != null)
            {
                if (newValue > entry.Max)
                {
                    newValue = entry.Max;
                }
                else if (newValue < entry.Min)
                {
                    newValue = entry.Min;
                }
            }
            return newValue;
        }
        #endregion

        #region Min property
        public decimal Min
        {
            get { return (decimal)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Min.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinProperty =
            DependencyProperty.Register("Min", typeof(decimal), typeof(DecimalEntry), new UIPropertyMetadata(Decimal.MinValue, MinChangeCallback));

        /// <summary>
        /// Callback for the a change to the Min property.  The limiting performed here
        /// does result in the property bound to Value being updated...
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void MinChangeCallback(DependencyObject source,
        DependencyPropertyChangedEventArgs e)
        {
            decimal newValue = (decimal)e.NewValue;
            DecimalEntry entry = source as DecimalEntry;
            if (entry != null)
            {
                if (entry.Value < newValue)
                {
                    entry.Value = newValue;
                }
            }
        }
        #endregion

        #region Max property

        public decimal Max
        {
            get { return (decimal)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Max.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register("Max", typeof(decimal), typeof(DecimalEntry), new UIPropertyMetadata(Decimal.MaxValue, MaxChangeCallback));

        /// <summary>
        /// Callback for the a change to the Max property.  The limiting performed here
        /// does result in the property bound to Value being updated...
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void MaxChangeCallback(DependencyObject source,
        DependencyPropertyChangedEventArgs e)
        {
            decimal newValue = (decimal)e.NewValue;
            DecimalEntry entry = source as DecimalEntry;
            if (entry != null)
            {
                if (entry.Value > newValue)
                {
                    entry.Value = newValue;
                }
            }
        }
        #endregion

        #endregion

        private void MinMaxToolTipToolTipOpening( object sender, ToolTipEventArgs e )
        {
            var ctrl = sender as DecimalTextBox;
            if( ctrl != null )
            {
                string minToolTip = ctrl.FormatToolTipEntry( Min );
                string maxToolTip = ctrl.FormatToolTipEntry( Max );
                if( string.IsNullOrEmpty( minToolTip ) )
                {
                    if( string.IsNullOrEmpty( maxToolTip ) )
                    {
                        ctrl.ToolTip = null;
                    }
                    else
                    {
                        ctrl.ToolTip = string.Format( "Maximum = {0}",
                                                      maxToolTip );
                    }
                }
                else if( string.IsNullOrEmpty( maxToolTip ) )
                {
                    ctrl.ToolTip = string.Format( "Minimum = {0}",
                                                  minToolTip );
                }
                else
                {
                    ctrl.ToolTip = string.Format( "Minimum = {0} and Maximum = {1}",
                                                  minToolTip,
                                                  maxToolTip );
                }
            }
        }
        // For data binding - do not remove
        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }

    /// <summary>
    /// Extension of TextBox to implement a numeric entry text box that support units (suffixes,
    /// terminators) and spin buttons.
    /// 
    /// This class was derived from CNumPadEntryTextBox (authored by Bill Loughner for Resolution)
    /// </summary>
    public class DecimalTextBox : TextBox
    {
        #region Internal state
        private const int MAX_EXPONENT_LENGTH = 3; // sign and two digits
        private const int MAX_NUM_LENGTH = 50;
        private const char BACKSPACE_CHAR = '\b';
        private const char DELETE_CHAR = (char)0x7f;
        private readonly char DECIMAL_SEPARATOR = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];

        private int mInsertionPt;
        private bool mHasSign;
        private bool mHasDecimal;
        private int mDecimalIndex;
        private int mImpliedDecimalIndex;
        private int mExponentIndex;
        private bool mHasExponent;
        private bool mHasExponentSign;
        private bool mHasTerminator;
        private int mTerminatorIndex;
        private int mDigits = 12;
        private decimal mLastGoodValue;
        private bool mTrimTrailingZeros = true;
        private int mDecimalOffsetIndex;
        private UnitMultiplier mDisplayUnits = UnitMultiplier.UNIT_NONE;
        private List<UnitMultiplier> mUnitList = new List<UnitMultiplier>();
        private StringBuilder mEntryString;
        private DecimalEntry mDecimalEntry;
        #endregion

        public DecimalTextBox()
        {
            MaxLength = MAX_NUM_LENGTH;
            ShowAllDigits = false;
            mLastGoodValue = 0;
        }

        public void SetDecimalEntry(DecimalEntry control)
        {
            mDecimalEntry = control;
        }

        #region Event Handlers
        /// <summary>
        /// OnTextChanged is called when the user enters values directly from a keyboard (as opposed
        /// to pressing the buttons on the keypad) or when something is pasted into the control (which
        /// could be completely invalid for numeric entry).
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (SelectionLength == 0)
            {
                bool isFirstTime = mEntryString == null;
                mEntryString = new StringBuilder(Text);

                if (SelectionStart > 0)
                {
                    mInsertionPt = SelectionStart;
                }

                // Coerce to canonical form with cursor insertion point
                // preserved
                UpdateFormat();
                Text = mEntryString.ToString();
                if (isFirstTime)
                {
                    mInsertionPt = mEntryString.Length;
                }
                SelectionStart = mInsertionPt;
            }
        }

        /// <summary>
        /// When focus is lost there are 2 tasks
        /// 
        /// 1) Re-format the displayed text to its "final" form -- this is because there
        ///    may be trailing zeros (maintained for use by up/down processing)
        /// 
        /// 2) Clear the selection (base calling the base functionality)
        /// 
        /// IMPORTANT: the up/down buttons must not take focus away from the text box
        ///            else the selection is cleared and the user can't tell where
        ///            the up/down increment will be applied
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            // Update the value and displayed format...
            if (mDecimalEntry != null)
            {
                mDecimalEntry.Value = ParseEntry();
            }

            // Need to call base functionality to clear the selection (else all
            // the controls look like they are selected)
            base.OnLostFocus(e);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            // Update the value and displayed format...
            if (mDecimalEntry != null)
            {
                mDecimalEntry.Value = ParseEntry();
            }

            // Need to call base functionality to clear the selection (else all
            // the controls look like they are selected)
            base.OnLostKeyboardFocus(e);
        }
        
        /// <summary>
        /// On initial focus, select all of the text to make it easy to replace
        /// the entire entry.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            SelectAll();
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            SelectAll();
        }



        /// <summary>
        /// Force select all -- the text can have spaces in it, so the default
        /// double-click processing doesn't work as desired (it will select
        /// a single portion of the text delimited by the space)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            SelectAll();
        }

        /// <summary>
        /// If the control does not have focus, give it focus (which will cause a
        /// select all) and prevent further processing (which would set a 0 length
        /// selection starting at the mouse position).
        /// 
        /// If the control already has focus, do the normal processing (which sets
        /// a 0 length selection starting at the mouse position).
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // Find the TextBox
            DependencyObject parent = e.OriginalSource as UIElement;
            while (parent != null && !(parent is TextBox))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent != null)
            {
                var textBox = (TextBox)parent;
                if (!textBox.IsKeyboardFocusWithin)
                {
                    // If the text box is not yet focused, give it the focus and
                    // stop further processing of this click event.
                    textBox.Focus();
                    e.Handled = true;
                }
            }

            base.OnPreviewMouseLeftButtonDown(e);
        }

        /// <summary>
        /// Preserve the insertion point and process the key.  Some keys are
        /// ignored (e.g. space), some use the base/default functionality (e.g.
        /// alt,ctrl,...), and some are handled "locally" as part of numeric
        /// entry (e.g. digits)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            // preserve insertion point when keyboard key is pressed
            mInsertionPt = SelectionStart;

            char c;

            switch (e.Key)
            {
                case Key.Add:
                case Key.OemPlus:
                    c = '+';
                    break;
                case Key.Subtract:
                case Key.OemMinus:
                    c = '-';
                    break;
                case Key.OemPeriod:
                case Key.Decimal:
                    c = DECIMAL_SEPARATOR;
                    break;
                case Key.OemComma:
                    c = ',';
                    break;
                case Key.Enter:
                    if (mDecimalEntry != null)
                    {
                        mDecimalEntry.Value = ParseEntry();
                        e.Handled = true;
                    }
                    return;
                case Key.Escape:
                    // Esc allows the user to under a partial entry
                    FormatEntry(mLastGoodValue);
                    e.Handled = true;
                    return;
                case Key.NumPad0:
                    c = '0';
                    break;
                case Key.NumPad1:
                    c = '1';
                    break;
                case Key.NumPad2:
                    c = '2';
                    break;
                case Key.NumPad3:
                    c = '3';
                    break;
                case Key.NumPad4:
                    c = '4';
                    break;
                case Key.NumPad5:
                    c = '5';
                    break;
                case Key.NumPad6:
                    c = '6';
                    break;
                case Key.NumPad7:
                    c = '7';
                    break;
                case Key.NumPad8:
                    c = '8';
                    break;
                case Key.NumPad9:
                    c = '9';
                    break;
                case Key.Tab:
                case Key.LeftShift:
                case Key.RightShift:
                case Key.LeftAlt:
                case Key.RightAlt:
                case Key.LeftCtrl:
                case Key.RightCtrl:
                case Key.Left:
                case Key.Right:
                case Key.System:
                    // Don't process these here...
                    base.OnKeyDown(e);
                    return;
                default:
                    // We don't know what the possible terminators will be (the first
                    // letter of a unit/terminator will select it) so we have to 
                    // route almost all keys to the key handling...
                    c = (char)KeyInterop.VirtualKeyFromKey(e.Key);
                    break;
            }
            ProcessKeyPress(c);
            e.Handled = true;
        }

        /// <summary>
        /// Intercept/handle various keys that won't reach OnKeyDown.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            // preserve insertion point when keyboard key is pressed
            mInsertionPt = SelectionStart;

            switch (e.Key)
            {
                case Key.Down:
                    ProcessButton(SpecialButtons.DownButton);
                    e.Handled = true;
                    break;
                case Key.Up:
                    ProcessButton(SpecialButtons.UpButton);
                    e.Handled = true;
                    break;
                case Key.Back:
                    ProcessKeyPress(BACKSPACE_CHAR);
                    e.Handled = true;
                    break;
                case Key.Delete:
                    ProcessKeyPress(DELETE_CHAR);
                    e.Handled = true;
                    break;
                case Key.Space:
                    // Not allowed!  Ignore
                    e.Handled = true;
                    break;
            }

            base.OnPreviewKeyDown(e);
        }
        #endregion

        #region Process keys, buttons, events...
        /// <summary>
        /// Called internally in response to a key press ... 'key' has already
        /// been "translated" (i.e. it's alphanumeric or some control character).
        /// 
        /// The net result of this method will be the update of the displayed
        /// numeric value per the key pressed.
        /// </summary>
        /// <param name="key"></param>
        public void ProcessKeyPress(char key)
        {
            bool signalValueChange = false;

            ProcessSelection();

            AnalyzeEntryString();

            if (Char.IsDigit(key))
            {
                ProcessDigit(key);
            }
            else if (key == DECIMAL_SEPARATOR)
            {
                ProcessDecimal();
            }
            else if ((key == 'E') || (key == 'e'))
            {
                ProcessExponent();
            }
            else if ((key == '+') || (key == '-'))
            {
                ProcessSign(key);
            }
            else if (key == BACKSPACE_CHAR)
            {
                ProcessBackspace();
            }
            else if (key == DELETE_CHAR)
            {
                ProcessDelete();
            }
            else
            {
                string letter = new string(new[] { key });
                foreach (UnitMultiplier unit in mUnitList)
                {
                    if (unit.DisplayUnits.StartsWith(letter, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (mHasTerminator)
                        {
                            // Remove existing terminator
                            mEntryString.Remove(mTerminatorIndex, mEntryString.Length - mTerminatorIndex);
                        }
                        // Add the new terminator
                        mTerminatorIndex = mEntryString.Length;
                        mEntryString.Append(' ').Append(unit.DisplayUnits);
                        signalValueChange = true;
                        break;
                    }
                }
            }

            // The entry text box always gets the focus after a dialog button
            // or keyboard key press changes the entry string
            //Focus();

            UpdateFormat();

            if (mEntryString.Length <= MaxLength)
            {
                Text = mEntryString.ToString();
                SelectionStart = mInsertionPt;
            }

            if (signalValueChange && mDecimalEntry != null)
            {
                mDecimalEntry.Value = ParseEntry();
                // Editing is no longer in progress, but the IsEditing property is
                // cleared in FormatEntry which is called as a result of setting Value
                // (i.e. no need to call it hear)
            }
            else
            {
                // Indicate the editing is in progress (e.g. the displayed text does
                // not match the value the control is bound to).
                IsEditing = true;
            }
        }

        /// <summary>
        /// Analyze the current entry and "tag" various locations in the text
        /// to make handling easier in other routines.  The method identifies
        /// if the entry has sign, decimal point, exponent, exponent sign,
        /// terminator and where in the entry they are located.
        /// </summary>
        private void AnalyzeEntryString()
        {
            if (mEntryString.Length == 0)
            {
                mHasSign = false;
                mHasDecimal = false;
                mHasExponent = false;
                mHasExponentSign = false;
                mHasTerminator = false;
                mImpliedDecimalIndex = 0;
            }
            else
            {
                string entry = mEntryString.ToString();
                mHasSign = (mEntryString[0] == '+') || (mEntryString[0] == '-');
                mDecimalIndex = entry.LastIndexOf(DECIMAL_SEPARATOR);
                mHasDecimal = mDecimalIndex >= 0;
                mExponentIndex =
                   entry.LastIndexOf("E", StringComparison.InvariantCultureIgnoreCase);
                mHasExponent = mExponentIndex >= 0;
                if (mExponentIndex > 0)
                {
                    // see if the unit is deg (degree)
                    if (Char.IsLetter(entry[mExponentIndex - 1]) && Char.IsLetter(entry[mExponentIndex + 1]))
                    {
                        mHasExponent = false;
                    }
                }
                mHasExponentSign = mHasExponent &&
                   ((entry.LastIndexOf('+') > mExponentIndex) ||
                   (entry.LastIndexOf('-') > mExponentIndex));
                mTerminatorIndex = -1;
                UnitMultiplier one = null;
                foreach (UnitMultiplier unit in mUnitList)
                {
                    if (unit.Multiplier == 1)
                    {
                        one = unit;
                    }
                    else
                    {
                        mTerminatorIndex = entry.IndexOf(unit.DisplayUnits, StringComparison.InvariantCultureIgnoreCase);
                        if (mTerminatorIndex >= 0)
                        {
                            break;
                        }
                    }
                }
                if (mTerminatorIndex < 0 && one != null)
                {
                    mTerminatorIndex = entry.IndexOf(one.DisplayUnits, StringComparison.InvariantCultureIgnoreCase);
                }
                mHasTerminator = mTerminatorIndex >= 0;
                if (mHasTerminator && mTerminatorIndex > 0 && entry[mTerminatorIndex - 1] == ' ')
                {
                    mTerminatorIndex -= 1;
                }
                // Determine the implied decimal point -- used to maintain constant step sizes
                if (mHasDecimal)
                {
                    mImpliedDecimalIndex = mDecimalIndex;
                }
                else if (mHasExponent)
                {
                    mImpliedDecimalIndex = mExponentIndex;
                }
                else if (mHasTerminator)
                {
                    mImpliedDecimalIndex = mTerminatorIndex;
                }
                else
                {
                    mImpliedDecimalIndex = mEntryString.Length;
                }
            }
        }

        // Removes selected text
        private void ProcessSelection()
        {
            mInsertionPt = SelectionStart;
            mEntryString = new StringBuilder(Text);
            if (SelectionLength != 0)
            {
                mEntryString.Remove(SelectionStart, SelectionLength);
            }
        }

        // Add a digit to the mantissa or to the exponent
        // The maximum exponent length may not be exceeded
        private void ProcessDigit(char key)
        {
            // No digits allowed past the terminator
            if (mHasTerminator && mInsertionPt > mTerminatorIndex)
            {
                // So snap the insertion point to the end of the number
                mInsertionPt = mTerminatorIndex;
            }

            int length = (mHasTerminator) ? mTerminatorIndex : mEntryString.Length;
            if (!mHasExponent ||
               (mInsertionPt <= mExponentIndex) ||
               ((length - (mExponentIndex + 1)) < MAX_EXPONENT_LENGTH))
            {
                mEntryString.Insert(mInsertionPt, key);
                mInsertionPt += 1;
            }
        }

        private void ProcessDecimal()
        {
            // No decimal allowed past the terminator
            if (mHasTerminator && mInsertionPt > mTerminatorIndex)
            {
                // So snap the insertion point to the end of the number
                mInsertionPt = mTerminatorIndex;
            }

            // Only one decimal point allowed; must be placed before
            // the 'E' exponent character
            if (!mHasDecimal &&
                ((mInsertionPt <= mExponentIndex) || (!mHasExponent)))
            {
                mEntryString.Insert(mInsertionPt, DECIMAL_SEPARATOR);
                mDecimalIndex = mInsertionPt;
                mInsertionPt += 1;
                mHasDecimal = true;
            }
        }

        private void ProcessExponent()
        {
            // No exponent allowed past the terminator
            if (mHasTerminator && mInsertionPt > mTerminatorIndex)
            {
                // So snap the insertion point to the end of the number
                mInsertionPt = mTerminatorIndex;
            }
            if (!mHasExponent)
            {
                mEntryString.Insert(mInsertionPt, 'E');
                mExponentIndex = mInsertionPt;
                mHasExponent = true;
                mInsertionPt += 1;
            }
        }

        private void ProcessSign(char c)
        {
            if ((mInsertionPt <= mExponentIndex) || !mHasExponent)
            {
                // Set mantissa sign
                if (mHasSign)
                {
                    mEntryString[0] = c;
                }
                else
                {
                    mEntryString.Insert(0, c);
                    ++mInsertionPt;
                    mHasSign = true;
                }
            }
            else
            {
                // Set exponent sign
                if (mHasExponentSign)
                {
                    mEntryString[mExponentIndex + 1] = c;
                }
                else
                {
                    mEntryString.Insert(mExponentIndex + 1, c);
                    ++mInsertionPt;
                    mHasExponentSign = true;
                }
            }
        }

        private void ProcessBackspace()
        {
            // ProcessSelection takes care of backspace/delete of text that is selected
            // Otherwise delete character in front of insertion point
            if (SelectionLength == 0)
            {
                // Backspace over any part of the terminator removes the entire terminator
                if (mHasTerminator && mInsertionPt > mTerminatorIndex)
                {
                    mEntryString.Remove(mTerminatorIndex, mEntryString.Length - mTerminatorIndex);
                    mHasTerminator = false;
                    mTerminatorIndex = -1;
                    mInsertionPt = mEntryString.Length;
                }
                else if (mInsertionPt > 0)
                {
                    mEntryString.Remove(mInsertionPt - 1, 1);
                    mInsertionPt -= 1;
                }
            }
        }

        private void ProcessDelete()
        {
            // ProcessSelection takes care of backspace/delete of text that is selected
            // Otherwise delete character after the insertion point
            if (SelectionLength == 0)
            {
                // Backspace over any part of the terminator removes the entire terminator
                if (mHasTerminator && mInsertionPt >= mTerminatorIndex)
                {
                    mEntryString.Remove(mTerminatorIndex, mEntryString.Length - mTerminatorIndex);
                    mHasTerminator = false;
                    mTerminatorIndex = -1;
                    mInsertionPt = mEntryString.Length;
                }
                else if (mInsertionPt < mEntryString.Length)
                {
                    mEntryString.Remove(mInsertionPt, 1);
                }
            }
        }

        // Enforces canonical notation:
        //   * Mantissa always has a sign; default is '+'
        //   * A decimal point is always preceded by at least one digit,
        //      zero if necessary
        //   * Deleting the 'E' exponent character results in any
        //     exponent sign being deleted as well
        //   * An 'E' exponent character is always preceded by a
        //     a mantissa; default mantissa is +1.0.
        //   * An exponent always has a sign; default is '+'
        private void UpdateFormat()
        {
            AnalyzeEntryString();
            // Remove exponent sign if exponent 'E' was deleted
            if (mEntryString.ToString().LastIndexOf("E", StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                mHasExponent = false;
                int signIndex = mEntryString.ToString().LastIndexOf('-');
                if (signIndex > 0)
                {
                    mEntryString.Remove(signIndex, 1);
                    Text = mEntryString.ToString();
                    mInsertionPt = signIndex;
                }
                else
                {
                    signIndex = mEntryString.ToString().LastIndexOf('+');
                    if (signIndex > 0)
                    {
                        mEntryString.Remove(signIndex, 1);
                        Text = mEntryString.ToString();
                        mInsertionPt = signIndex;
                    }
                }
                mHasExponentSign = false;
            }

            // Add implied positive sign to exponent
            if (mHasExponent && !mHasExponentSign &&
               (mEntryString.Length >= (mExponentIndex + 2)))
            {
                mEntryString.Insert(mExponentIndex + 1, '+');
                mHasExponentSign = true;
                if (mInsertionPt > mExponentIndex)
                {
                    ++mInsertionPt;
                }
            }

            // Add default unity mantissa
            if (mHasExponent &&
               ((mExponentIndex == 0) ||
               (!Char.IsDigit(mEntryString[mExponentIndex - 1]) &&
               (mEntryString[mExponentIndex - 1] != DECIMAL_SEPARATOR))))
            {
                const string unityMantissa = "1.0";
                mEntryString.Insert(mExponentIndex, unityMantissa);
                if (mInsertionPt != 0)
                {
                    mInsertionPt += unityMantissa.Length;
                }
            }

            // Add leading zero to decimal point
            if (mHasDecimal)
            {
                if ((mDecimalIndex == 0) ||
                   (!Char.IsDigit(mEntryString[mDecimalIndex - 1])))
                {
                    mEntryString.Insert(mDecimalIndex, '0');
                    ++mInsertionPt;
                }
            }
        }

        /// <summary>
        /// Process a substring of the numeric entry by incrementing/decrementing a digit specified
        /// by the current insertion point (mInsertionPt).
        /// </summary>
        /// <param name="direction">increment (1) or decrement (-1)</param>
        /// <param name="startIndex">start of the substring to manipulate</param>
        /// <param name="numberLength">length of the substring to manipulate</param>
        /// <param name="hasSign">true if the substring includes a sign</param>
        /// <param name="hasDecimal">true if the substring includes a decimal point</param>
        /// <param name="decimalIndex">index of the decimal, only valid if hasDecimal==true</param>
        /// <param name="forceSign">true if the result should include a sign regardless of positive or negative value</param>
        /// <param name="smallestStep">the smallest step allowed.  use 1 for exponents and Resolution for mantissa</param>
        private void ProcessValue(int direction, int startIndex, int numberLength, bool hasSign, bool hasDecimal, int decimalIndex, bool forceSign, decimal smallestStep)
        {
            try
            {
                char[] buffer = new char[numberLength];
                mEntryString.CopyTo(startIndex, buffer, 0, numberLength);
                Decimal value = Decimal.Parse(new String(buffer));
                Decimal increment;
                if (hasSign && mInsertionPt == startIndex)
                {
                    mInsertionPt = startIndex + 1;
                }
                if (hasDecimal)
                {
                    if (mInsertionPt > decimalIndex)
                    {
                        increment = (Decimal)Math.Pow(10, decimalIndex - mInsertionPt + 1);
                    }
                    else if (mInsertionPt == decimalIndex)
                    {
                        increment = 1;
                    }
                    else
                    {
                        increment = (Decimal)Math.Pow(10, decimalIndex - mInsertionPt);
                    }
                }
                else
                {
                    increment = (Decimal)Math.Pow(10, startIndex + numberLength - mInsertionPt);
                }

                // increment has to be at least smallestStep
                if (increment < smallestStep)
                {
                    increment = smallestStep;
                }

                // For values that have a minimum > 0 (e.g. center frequency) we don't want
                // the increment to rail the value -- so instead dynamically adjust the increment
                if( mDecimalEntry.Min > 0 )
                {
                    if( ( value + direction * increment ) <= 0 )
                    {
                        // scale the increment by 10 ... i.e. rather than step from 1 GHz to 0 GHz
                        // step from 1 GHz to 900 MHz
                        increment /= 10;

                        // Special case if the cursor is at index 0 (to the left of the MSD)
                        if( ( value + direction * increment ) <= 0 )
                        {
                            // Still too big
                            increment /= 10;
                            mInsertionPt += 1;
                            mDecimalOffsetIndex -= 1;
                        }

                        // Adjust the "pointers" into the text
                        if( increment < 1 )
                        {
                            if( hasDecimal )
                            {
                                // String is getting 1 longer
                                mInsertionPt += 1;
                                if( mHasTerminator && mInsertionPt >= mTerminatorIndex )
                                {
                                    mTerminatorIndex += 1;
                                }
                            }
                            else
                            {
                                // String is getting 2 longer (decimal point and digit)
                                hasDecimal = true;
                                decimalIndex = mInsertionPt;
                                mInsertionPt += 1;
                                mDecimalOffsetIndex -= 2;
                                if( mHasTerminator )
                                {
                                    mTerminatorIndex += 2;
                                }
                            }
                        }
                        else
                        {
                            // String is getting shorter...
                            mDecimalOffsetIndex -= 1;
                            if( mHasTerminator )
                            {
                                mTerminatorIndex -= 1;
                            }
                        }
                    }
                }

                value += direction * increment;
                string temp;
                string format;
                if (hasDecimal && numberLength > decimalIndex)
                {
                    format = "{0:0." + new string('0', numberLength - decimalIndex - 1) + "}";
                }
                else
                {
                    format = (forceSign) ? "{0:+0;-0;+0}" : "{0}";
                }
                temp = String.Format(format, value);
                if (temp.Length > numberLength)
                {
                    mInsertionPt += 1;
                }
                else if (temp.Length < numberLength)
                {
                    mInsertionPt -= 1;
                }
                mEntryString.Remove(startIndex, numberLength);
                mEntryString.Insert(startIndex, temp);
            }
            catch
            {
                // silently ignore errors to allow user to fix them
            }
        }

        /// <summary>
        /// Process an up (direction=1) or down (direction=-1) directive (could be from the
        /// spin buttons or from the up/down keys).  The portion of the entry affected
        /// depends on the current insertion point, mInsertionPt (i.e. mantissa vs. exponent
        /// and which digit)
        /// </summary>
        /// <param name="direction"></param>
        /// <returns>true if the caller should update the insertion point (to keep increment constant)</returns>
        public bool ProcessUpDown(int direction)
        {
            int length = (mHasTerminator) ? mTerminatorIndex : mEntryString.Length;

            // If the cursor is "in" the terminator...
            if (mInsertionPt > length)
            {
                mDecimalOffsetIndex += mInsertionPt;
                if (PreferredIncrement != 0 && mDecimalEntry != null)
                {
                    // ... use a hard-coded increment
                    // NOTE: normally the entry (text) is converted to Decimal (i.e. no loss in precision) but to use
                    //       an absolute increment (and not have to rewrite all the number processing) we need to use
                    //       'Value' here (a decimal) which opens up the possibility of rounding.  Try to minimize this
                    //       by performing all computations as Decimal.
                    decimal temp = mDecimalEntry.Value + direction * PreferredIncrement;
                    temp = Math.Min(temp, mDecimalEntry.Max);
                    mDecimalEntry.Value = Math.Max(temp, mDecimalEntry.Min);
                    // Keep the insertion point at the end of the text so PreferredIncrement will continue to apply
                    mInsertionPt = mEntryString.Length;
                    return false;
                }
                if (PreferUnitStep)
                {
                    // ... set insertion point to the units position
                    mInsertionPt = mDecimalOffsetIndex;
                    mDecimalOffsetIndex = 0;
                }
                else
                {
                    // ... set insertion point to the least significant digit
                    mInsertionPt = length;
                    mDecimalOffsetIndex -= mInsertionPt;
                }
            }

            if (mHasExponent)
            {
                if (mInsertionPt > mExponentIndex)
                {
                    ProcessValue(direction, mExponentIndex + 1, length - mExponentIndex - 1, mHasExponentSign, false, -1, true, 1);
                }
                else
                {
                    ProcessValue(direction, 0, mExponentIndex, mHasSign, mHasDecimal, mDecimalIndex, false, ComputeEffectiveResolution());
                }
            }
            else
            {
                ProcessValue(direction, 0, length, mHasSign, mHasDecimal, mDecimalIndex, false, ComputeEffectiveResolution());
            }

            return true;
        }

        public void ProcessButton(SpecialButtons button)
        {
            bool trimTrailingZeros = true;
            bool adjustInsertionPoint = true;

            // Capture the current cursor position -- if any text is selected, use the
            // least significant digit of the selected text... But if the text has never
            // been selected, default to the LSD instead of the MSD
            if (SelectionLength > 0 || SelectionStart != 1 || IsFocused)
            {
                mInsertionPt = SelectionStart + SelectionLength;
            }
            else
            {
                mInsertionPt = Text.Length;
            }

            // Cache some characteristics of the current setting (used to preserve
            // the numeric value of the increment applied by up/down).
            //
            // mDecimalOffsetIndex isn't quite log10(increment) because the decimal
            // offsets the index if increment < 1 ... but this offset is accounted
            // for in AdjustInsertionPoint.
            UnitMultiplier previousUnits = mDisplayUnits;
            mDecimalOffsetIndex = mImpliedDecimalIndex - mInsertionPt;

            switch (button)
            {
                case SpecialButtons.ZeroButton:
                    FormatEntry(0);
                    break;
                case SpecialButtons.MinButton:
                    if (mDecimalEntry != null)
                    {
                        FormatEntry(mDecimalEntry.Min);
                    }
                    break;
                case SpecialButtons.MaxButton:
                    if (mDecimalEntry != null)
                    {
                        FormatEntry(mDecimalEntry.Max);
                    }
                    break;
                case SpecialButtons.UpButton:
                    adjustInsertionPoint = ProcessUpDown(+1);
                    trimTrailingZeros = false;
                    break;
                case SpecialButtons.DownButton:
                    adjustInsertionPoint = ProcessUpDown(-1);
                    trimTrailingZeros = false;
                    break;
            }

            UpdateFormat();

            if (mEntryString.Length <= MaxLength)
            {
                Text = mEntryString.ToString();
                SelectionStart = mInsertionPt;
            }

            // For up/down buttons we never want to trim trailing zeros
            // because that could eliminate the digit being manipulated
            // when it "passes through" zero.
            mTrimTrailingZeros = trimTrailingZeros;
            decimal temp = ParseEntry();
            if (mDecimalEntry != null)
            {
                mDecimalEntry.Value = temp;
            }
            mTrimTrailingZeros = true;

            // Adjust the cursor position to keep a constant increment
            if( adjustInsertionPoint )
            {
                AdjustInsertionPoint( mDecimalOffsetIndex, mDisplayUnits, previousUnits );
            }
        }

        /// <summary>
        /// Compute the desired cursor position to maintain the value of
        /// the increment applied by up/down keys...
        /// </summary>
        /// <param name="oldDecimalOffsetIndex"></param>
        /// <param name="newUnits"></param>
        /// <param name="oldUnits"></param>
        private void AdjustInsertionPoint(int oldDecimalOffsetIndex, UnitMultiplier newUnits, UnitMultiplier oldUnits)
        {
            // Change of scale...
            double multiplier = (newUnits == null) ? 1 : newUnits.Multiplier;
            multiplier /= (oldUnits == null) ? 1 : oldUnits.Multiplier;

            // Compute position relative to decimal
            int newDecimalOffsetIndex = oldDecimalOffsetIndex - (int)Math.Round(Math.Log10(multiplier));
            mInsertionPt = mImpliedDecimalIndex - newDecimalOffsetIndex;

            // If crossing the decimal point need to adjust insertion point by 1
            if (newDecimalOffsetIndex >= 0 && oldDecimalOffsetIndex < 0)
            {
                mInsertionPt -= 1;
            }
            else if (newDecimalOffsetIndex < 0 && oldDecimalOffsetIndex >= 0)
            {
                mInsertionPt += 1;
            }

            // If the cursor is just to the right of an actual decimal, move it
            // to the left of the decimal so it is obvious what the up/down
            // action will do
            if (mHasDecimal && mInsertionPt == mDecimalIndex + 1)
            {
                mInsertionPt = mDecimalIndex;
            }

            // Sanity checks...
            if (mInsertionPt < 0)
            {
                mInsertionPt = 0;
            }
            int numberLength = (mHasTerminator) ? mTerminatorIndex : mEntryString.Length;
            if (mInsertionPt > numberLength)
            {
                mInsertionPt = numberLength;
            }

            // Set the cursor position
            SelectionStart = mInsertionPt;
        }
        #endregion

        #region Parsing & Formatting
        /// <summary>
        /// Compute the effective resolution -- this is the user specified resolution
        /// scaled by the exponent and/or the terminator/unit multiplier.
        /// </summary>
        /// <returns></returns>
        private decimal ComputeEffectiveResolution()
        {
            // Default, no scaling
            decimal scale = 1;
            string entry = mEntryString.ToString();

            if (mHasExponent)
            {
                // Parse exponent
                int end = (mHasTerminator) ? mTerminatorIndex : mEntryString.Length;
                int exponent;
                if (Int32.TryParse(entry.Substring(mExponentIndex + 1, end - mExponentIndex - 1), out exponent))
                {
                    scale /= (decimal)Math.Pow(10, exponent);
                }
            }

            if (mHasTerminator)
            {
                string terminator = entry.Substring(mTerminatorIndex).Trim();
                if (IsPerDiv && terminator.EndsWith("/div", StringComparison.InvariantCultureIgnoreCase))
                {
                    terminator = terminator.Substring(0, terminator.Length - 4);
                }
                foreach (UnitMultiplier unit in mUnitList)
                {
                    if (unit.DisplayUnits.Equals(terminator, StringComparison.InvariantCultureIgnoreCase))
                    {
                        scale /= (decimal)unit.Multiplier;
                        break;
                    }
                }
            }

            return scale * Resolution;
        }

        /// <summary>
        /// Choose the best UnitMultiplier for the specified value from the
        /// list of units (mUnitList).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private UnitMultiplier ChooseBestUnit(decimal value)
        {
            // Trivial cases:
            if (mUnitList.Count == 0)
            {
                return UnitMultiplier.UNIT_NONE;
            }
            if (mUnitList.Count == 1)
            {
                return mUnitList[0];
            }
            // Not so trivial ... need to choose the best of multiple units.
            decimal absValue = Math.Abs(value);
            UnitMultiplier bestUnit = null;
            UnitMultiplier baseUnit = null;
            foreach (UnitMultiplier unit in mUnitList)
            {
                if (unit.Multiplier == 1)
                {
                    baseUnit = unit;
                }
                if (bestUnit == null)
                {
                    // If any units are supplied, we must pick one of them!
                    bestUnit = unit;
                }
                else if (absValue >= (decimal)unit.Multiplier &&
                         unit.Multiplier > bestUnit.Multiplier)
                {
                    // Better fit... Example
                    //     Value = 10e6    = 10 MHz
                    //     Unit.Multiplier = kHz
                    //     Best.Multiplier = Hz  (from previous loop)
                    bestUnit = unit;
                }
                else if (absValue < (decimal)bestUnit.Multiplier &&
                         unit.Multiplier < bestUnit.Multiplier )
                {
                    // Better fit... Example
                    //     Value = 10e6    = 10 MHz
                    //     Unit.Multiplier = kHz
                    //     Best.Multiplier = GHz  (from previous loop)
                    bestUnit = unit;
                }
            }
            if (value == 0 && baseUnit != null)
            {
                bestUnit = baseUnit;
            }
            if (bestUnit == null)
            {
                bestUnit = (baseUnit == null) ? UnitMultiplier.UNIT_NONE : baseUnit;
            }
            return bestUnit;
        }

        /// <summary>
        /// Format the specified value for display.  There are 3 cases for formatting:
        /// 
        /// 1) All requested digits are to be shown (ShowAllDigits == True)
        /// 2) Trailing zero suppression (ShowAllDigits == False && mTrimTrailingZeros == True)
        /// 3) Preserve displayed digits so up/down keys work nicely (ShowAllDigits == False && mTrimTrailingZeros == False)
        ///
        /// All 3 cases may be further limited by the Resolution
        /// </summary>
        /// <param name="value"></param>
        public void FormatEntry(decimal value)
        {
            // Find the appropriate unit
            UnitMultiplier units = ChooseBestUnit(value);

            int desiredDigits;
            char fillChar;
            if (ShowAllDigits)
            {
                // Case 1 -- always show all digits
                desiredDigits = Digits;
                fillChar = '0';
            }
            else if (mTrimTrailingZeros)
            {
                // Case 2 -- trailing zero suppression
                desiredDigits = Digits;
                fillChar = '#';
            }
            else
            {
                // Case 3 -- preserve the format the user has provided
                fillChar = '0';
                if (mHasExponent)
                {
                    desiredDigits = mExponentIndex - mImpliedDecimalIndex;
                }
                else if (mHasTerminator)
                {
                    desiredDigits = mTerminatorIndex - mImpliedDecimalIndex;
                }
                else
                {
                    desiredDigits = mEntryString.Length - mImpliedDecimalIndex;
                }
                // If there is an actual decimal point, adjust (cause the decimal
                // takes up a space)
                desiredDigits -= (mHasDecimal) ? 1 : 0;

                desiredDigits += (int)Math.Round(Math.Log10(units.Multiplier / mDisplayUnits.Multiplier));
            }

            decimal scaledValue = value / (decimal)units.Multiplier;
            //double absScaledValue = Math.Abs(scaledValue);
            //int digitsLeftOfDecimal = (absScaledValue < 1) ? 0 : ((int)Math.Log10(absScaledValue)) + 1;
            int digitsRightOfDecimal = desiredDigits; // Math.Min(desiredDigits - digitsLeftOfDecimal, Digits);

            // Does Resolution impact digits?
            if (Resolution > 0)
            {
                double desired = Math.Log10((double)((decimal)units.Multiplier / Resolution));
                // Use Ceiling because the resolution may not be a multiple of 10
                int maxDigits = (int)Math.Ceiling(desired);
                if (Math.Abs(desired-maxDigits)>0.1)
                {
                    // This takes care of things like Resolution = 0.25 (need 2 digits, not 1)
                    maxDigits += 1;
                }
                if (maxDigits < 0)
                {
                    digitsRightOfDecimal = 0;
                }
                else
                {
                    digitsRightOfDecimal = Math.Min(digitsRightOfDecimal, maxDigits);
                }
            }

            string format;
            if (digitsRightOfDecimal > 0)
            {
                format = "{0:0." + new string(fillChar, digitsRightOfDecimal) + "} {1}{2}";
            }
            else
            {
                format = "{0} {1}{2}";
            }
            string perDiv = IsPerDiv ? "/div" : "";

            mEntryString = new StringBuilder(String.Format(format, scaledValue, units.DisplayUnits, perDiv).Trim());

            AnalyzeEntryString();

            int maxInsertionPt = (mHasTerminator) ? mTerminatorIndex : mEntryString.Length;
            mInsertionPt = Math.Min(mInsertionPt, maxInsertionPt);

            UpdateFormat();

            // Copy results...
            mLastGoodValue = value;
            mDisplayUnits = units;
            Text = mEntryString.ToString();

            // Set the TextBox style so the user knows the value has been applied
            IsEditing = false;
        }

        public string FormatToolTipEntry( decimal value )
        {
            // Sanity check...
            if( //double.IsNaN( value ) ||
                //double.IsInfinity( value ) ||
                value == decimal.MaxValue || 
                value == decimal.MinValue )
            {
                return string.Empty;
            }

            // Find the appropriate unit
            UnitMultiplier units = ChooseBestUnit( value );

            int desiredDigits = Digits;
            const char fillChar = '#';

            decimal scaledValue = value / (decimal)units.Multiplier;
            int digitsRightOfDecimal = desiredDigits; // Math.Min(desiredDigits - digitsLeftOfDecimal, Digits);

            string format;
            if( digitsRightOfDecimal > 0 )
            {
                decimal absValue = Math.Abs( value );
                format = ( ( absValue > 0 && absValue < (decimal)0.001 ) || absValue >= 1000000 )
                             ? "{0:e2} {1}{2}"
                             : "{0:0." + new string( fillChar, digitsRightOfDecimal ) + "} {1}{2}";
            }
            else
            {
                format = "{0} {1}{2}";
            }
            string perDiv = IsPerDiv ? "/div" : "";
            StringBuilder sb =
                new StringBuilder( String.Format( format, scaledValue, units.DisplayUnits, perDiv ).Trim() );

            return sb.ToString();
        }
        /// <summary>
        /// Parse the current entry and "publish" the value via the Dependency Property, Value,
        /// in DecimalEntry
        /// </summary>
        /// <returns></returns>
        private decimal ParseEntry()
        {
            bool parsed = false;
            decimal value = 0;
            try
            {
                string entry = mEntryString.ToString();
                if (mHasTerminator)
                {
                    string terminator = entry.Substring(mTerminatorIndex).Trim();
                    if (IsPerDiv && terminator.EndsWith("/div", StringComparison.InvariantCultureIgnoreCase))
                    {
                        terminator = terminator.Substring(0, terminator.Length - 4);
                    }
                    foreach (UnitMultiplier unit in mUnitList)
                    {
                        if (unit.DisplayUnits.Equals(terminator, StringComparison.InvariantCultureIgnoreCase))
                        {
                            value = decimal.Parse(entry.Substring(0, mTerminatorIndex)) * (decimal)unit.Multiplier;
                            parsed = true;
                        }
                    }
                }
                if (!parsed)
                {
                    value = Decimal.Parse(entry);
                }
            }
            catch
            {
                // Error handling ... force back to last known good value
                value = mLastGoodValue;
            }

            // Adjust resolution (if specified)
            if (Resolution > 0)
            {
                value = Resolution * Math.Round(value / Resolution);
            }

            // Limits
            if (mDecimalEntry != null)
            {
                value = Math.Max(value, mDecimalEntry.Min);
                value = Math.Min(value, mDecimalEntry.Max);
            }

            if (value == mLastGoodValue)
            {
                // Since the value won't change, we can't count on event
                // handling/notification to update the displayed text. So
                // explicitly force a display update
                FormatEntry(value);
            }

            return value;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The list of UnitMulitpliers available for formatting the value.
        /// </summary>
        public List<UnitMultiplier> UnitList
        {
            get
            {
                return mUnitList;
            }
            set
            {
                mUnitList = value;
                if (mDecimalEntry != null)
                {
                    mDecimalEntry.Value = ParseEntry();
                }
            }
        }

        /// <summary>
        /// The number of digits to display past the decimal point.  If ShowAllDigits
        /// is true, this number of digits will always be displayed (subject to
        /// limitation by Resolution).  If ShowAllDigits is false, trailing zeros
        /// are trimmed -- but a maximum of Digits places will be displayed past
        /// the decimal (again, subject to the limitation of Resolution)
        /// </summary>
        public int Digits
        {
            get { return mDigits; }
            set
            {
                mDigits = value;
            }
        }

        /// <summary>
        /// If true, trailing zero will be included to display 'Digits' places
        /// past the decimal point.
        /// </summary>
        public bool ShowAllDigits { get; set; }

        /// <summary>
        /// If non-zero, the number of displayed digits will not extend past the
        /// specified resolution (e.g. with resolution of 1, 1234 ... with
        /// resolution of 0.01, 1234.56 ... etc)
        /// </summary>
        public decimal Resolution { get; set; }

        /// <summary>
        /// PLEASE READ THIS ENTIRE COMMENT (there are non-obvious effects using this)
        /// 
        /// This value affects how the up/down increment is chosen when the cursor is not
        /// at a digit position (normally if the control has not received focus yet or if
        /// the cursor is positioned in the terminator).
        /// 
        /// This value overrides PreferUnitStep.
        /// 
        /// If the cursor is at a digit position, the value of PreferredIncrement is ignored and
        /// the increment is dynamically adjusted to increment the digit at the cursor position.
        /// For example, 1|000->2|000, 43|1.3->441.3, -3.5|4->-3.44.
        /// 
        /// When zero (the default) and the cursor is anywhere past the end of the number
        /// the increment applies to the least significant digit that is currently displayed.
        /// For example, 1000->1001, 23.1->23.2, -41.2345->-41.2344. This has been the default
        /// behavior for DecimalEntry since originally implemented. 
        /// 
        /// When non-zero and the cursor is at least one position past the last displayed digit,
        /// PreferredIncrement is used instead of the dynamically determined step.  Since
        /// this value is fixed, it may cause a very "large" or very "small" change in the
        /// value (e.g. 1 uV -> 100.001 mV,  100 V -> 100.1 V) so normally this should only
        /// be used with values limited to relatively small ranges.
        /// 
        /// NOW FOR THE CAVEATS:
        /// 1) In order to detect this case (the cursor "past" the number), this only works
        ///    if the number uses a terminator (otherwise the cursor position is always at
        ///    a valid digit position)
        /// 2) In order for the up/down buttons to work with a digit position the user has
        ///    selected, the digit position has to be retained even after the text box has
        ///    lost focus.  This means there's a "history" to how the control works:
        ///    a) If the control has never had focus, the cursor is not displayed, the
        ///       assumed cursor position is at the end of the text, and (if the number has
        ///       a terminator) the increment will apply to the unit position.
        ///    b) If the user sets focus to a digit position, set focus to another control, and
        ///       clicks the up (or down) button then the increment will apply to the digit at
        ///       the cursor position but since the control does not have focus the cursor is
        ///       not visible AND THE CONTROL APPEARS IDENTICAL TO THE PREVIOUS CASE.  Hence the
        ///       user cannot know simply by inspection whether the up/down keys will increment
        ///       the unit position or another position.
        /// </summary>
        public decimal PreferredIncrement { get; set; }

        /// <summary>
        /// PLEASE READ THIS ENTIRE COMMENT (there are non-obvious effects using this)
        /// 
        /// This value affects how the up/down increment is chosen when the cursor is not
        /// at a digit position (normally if the control has not received focus yet or if
        /// the cursor is positioned in the terminator).
        /// 
        /// If the cursor is at a digit position, the value of PreferUnitStep is ignored and
        /// the increment is dynamically adjusted to increment the digit at the cursor position.
        /// For example, 1|000->2|000, 43|1.3->441.3, -3.5|4->-3.44. 
        /// 
        /// When false (the default) and the cursor is anywhere past the end of the number
        /// the increment applies to the least significant digit that is currently displayed.
        /// For example, 1000->1001, 23.1->23.2, -41.2345->-41.2344. This has been the default
        /// behavior for DecimalEntry since originally implemented. NOTICE that when the control
        /// loses focus, the user cannot see the cursor and hence the increments can not be
        /// determined by simple inspection.
        /// 
        /// When true and the cursor is at least one position past the last displayed digit,
        /// the increment is dynamically adjusted to increment the unit position regardless
        /// of the digits currently displayed.  For example, 1000->1001, 23.1->24.1,
        /// -41.2345->-40.2345.  NOW FOR THE CAVEATS:
        /// 1) In order to detect this case (the cursor "past" the number), this only works
        ///    if the number uses a terminator (otherwise the cursor position is always at
        ///    a valid digit position)
        /// 2) In order for the up/down buttons to work with a digit position the user has
        ///    selected, the digit position has to be retained even after the text box has
        ///    lost focus.  This means there's a "history" to how the control works:
        ///    a) If the control has never had focus, the cursor is not displayed, the
        ///       assumed cursor position is at the end of the text, and (if the number has
        ///       a terminator) the increment will apply to the unit position.
        ///    b) If the user sets focus to a digit position, set focus to another control, and
        ///       clicks the up (or down) button then the increment will apply to the digit at
        ///       the cursor position but since the control does not have focus the cursor is
        ///       not visible AND THE CONTROL APPEARS IDENTICAL TO THE PREVIOUS CASE.  Hence the
        ///       user cannot know simply by inspection whether the up/down keys will increment
        ///       the unit position or another position.
        /// </summary>
        public bool PreferUnitStep { get; set; }

        /// <summary>
        /// If true, "/div" will be appended to the units terminator (e.g. MHz/div)
        /// </summary>
        public bool IsPerDiv
        {
            get;
            set;
        }

        /// <summary>
        /// The current units (terminator) displayed.
        /// </summary>
        public UnitMultiplier DisplayUnits
        {
            get
            {
                return mDisplayUnits;
            }
            set
            {
                // At the moment, we don't actually allow a fixed display unit...
            }
        }

        /// <summary>
        /// Adjust the style of the textbox (FontStyle, Background, TBD) to indicate
        /// if editing is in progress (i.e. the text does not match the current value
        /// the control is bound to).
        /// </summary>
        private bool IsEditing
        {
            set
            {
                FontStyle = value ? FontStyles.Italic : FontStyles.Normal;
            }
        }
        #endregion
    }

    public class UnitMultiplier
    {
        private string mDisplayUnits;
        private double mUltiplier;


        public static readonly UnitMultiplier UNIT_NONE = new UnitMultiplier("", 1);

        public static readonly UnitMultiplier UNIT_HZ = new UnitMultiplier("Hz", 1);
        public static readonly UnitMultiplier UNIT_KHZ = new UnitMultiplier("kHz", 1.0e3);
        public static readonly UnitMultiplier UNIT_MHZ = new UnitMultiplier("MHz", 1.0e6);
        public static readonly UnitMultiplier UNIT_GHZ = new UnitMultiplier("GHz", 1.0e9);

        public static readonly UnitMultiplier UNIT_S = new UnitMultiplier("s", 1);
        public static readonly UnitMultiplier UNIT_MS = new UnitMultiplier("ms", 1.0e-3);
        public static readonly UnitMultiplier UNIT_US = new UnitMultiplier("us", 1.0e-6);
        public static readonly UnitMultiplier UNIT_NS = new UnitMultiplier("ns", 1.0e-9);
        public static readonly UnitMultiplier UNIT_PS = new UnitMultiplier("ps", 1.0e-12);

        public static readonly UnitMultiplier UNIT_KV = new UnitMultiplier("kV", 1.0e3);
        public static readonly UnitMultiplier UNIT_V = new UnitMultiplier("V", 1);
        public static readonly UnitMultiplier UNIT_MV = new UnitMultiplier("mV", 1.0e-3);
        public static readonly UnitMultiplier UNIT_UV = new UnitMultiplier("uV", 1.0e-6);
        public static readonly UnitMultiplier UNIT_NV = new UnitMultiplier("nV", 1.0e-9);
        public static readonly UnitMultiplier UNIT_PV = new UnitMultiplier("pV", 1.0e-12);

        public static readonly UnitMultiplier UNIT_DB = new UnitMultiplier("dB", 1);
        public static readonly UnitMultiplier UNIT_DBM = new UnitMultiplier("dBm", 1);

        public static readonly UnitMultiplier UNIT_DBi = new UnitMultiplier("dBi", 1);

        public static readonly UnitMultiplier UNIT_DBSM = new UnitMultiplier("dBsm", 1);

        public static readonly UnitMultiplier UNIT_METER = new UnitMultiplier("meter", 1);
        public static readonly UnitMultiplier UNIT_KILOMETER = new UnitMultiplier("kilometer", 1000);

        public static readonly UnitMultiplier UNIT_Special_KILOMETERPERHOUR = new UnitMultiplier("Km/h", 1);
        public static readonly UnitMultiplier UNIT_Special_GHZ = new UnitMultiplier("GHz", 1);

        public static readonly UnitMultiplier UNIT_Percentage = new UnitMultiplier("%", 1);

        public static readonly UnitMultiplier UNIT_KW = new UnitMultiplier("kWatt", 1.0e3);
        public static readonly UnitMultiplier UNIT_W = new UnitMultiplier("Watt", 1);
        public static readonly UnitMultiplier UNIT_MW = new UnitMultiplier("mWatt", 1.0e-3);
        public static readonly UnitMultiplier UNIT_UW = new UnitMultiplier("uWatt", 1.0e-6);
        public static readonly UnitMultiplier UNIT_NW = new UnitMultiplier("nWatt", 1.0e-9);
        public static readonly UnitMultiplier UNIT_PW = new UnitMultiplier("pWatt", 1.0e-12);
        public UnitMultiplier()
        {
            mDisplayUnits = "";
            mUltiplier = 1;
        }

        public UnitMultiplier(string theUnits, double theMultiplier)
        {
            mDisplayUnits = theUnits;
            mUltiplier = theMultiplier;
        }

        public UnitMultiplier(string theUnits)
        {
            mDisplayUnits = theUnits;
            mUltiplier = 1;
        }

        public string DisplayUnits
        {
            get { return mDisplayUnits; }
            set { mDisplayUnits = value; }
        }

        public double Multiplier
        {
            get { return mUltiplier; }
            set { mUltiplier = value; }
        }


    }

    public enum SpecialButtons
    {
        UpButton,
        DownButton,
        MinButton,
        MaxButton,
        ZeroButton
    }
}
