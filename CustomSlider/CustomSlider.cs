using System;
using Xamarin.Forms;

namespace CustomControls
{
    public class CustomSlider : AbsoluteLayout
    {
        public static readonly BindableProperty ThumbProperty =
            BindableProperty.Create(
                "Thumb", typeof(View), typeof(CustomSlider),
                defaultValue: default(View), propertyChanged: OnThumbChanged);

        public View Thumb
        {
            get { return (View)GetValue(ThumbProperty); }
            set { SetValue(ThumbProperty, value); }
        }

        private static void OnThumbChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((CustomSlider)bindable).OnThumbChangedImpl((View)oldValue, (View)newValue);
        }

        protected virtual void OnThumbChangedImpl(View oldValue, View newValue)
        {
            OnSizeChanged(this, EventArgs.Empty);
        }

        public static readonly BindableProperty TrackBarProperty =
            BindableProperty.Create(
                "TrackBar", typeof(View), typeof(CustomSlider),
                defaultValue: default(View), propertyChanged: OnTrackBarChanged);

        public View TrackBar
        {
            get { return (View)GetValue(TrackBarProperty); }
            set { SetValue(TrackBarProperty, value); }
        }

        private static void OnTrackBarChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((CustomSlider)bindable).OnTrackBarChangedImpl((View)oldValue, (View)newValue);
        }

        protected virtual void OnTrackBarChangedImpl(View oldValue, View newValue)
        {
            OnSizeChanged(this, EventArgs.Empty);
        }

        public static readonly BindableProperty StateLabelsProperty =
            BindableProperty.Create(
                "StateLabels", typeof(View), typeof(CustomSlider),
                defaultValue: default(View));

        public View StateLabels
        {
            get { return (View)GetValue(StateLabelsProperty); }
            set { SetValue(StateLabelsProperty, value); }
        }

        public bool IsToggled { get; private set; }

        private static void OnStateLabelsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((CustomSlider)bindable).OnStateLabelsChangedImpl((View)oldValue, (View)newValue);
        }

        protected virtual void OnStateLabelsChangedImpl(View oldValue, View newValue)
        {
            OnSizeChanged(this, EventArgs.Empty);
        }

        private PanGestureRecognizer _panGesture = new PanGestureRecognizer();
        private TapGestureRecognizer _leftTapGesture = new TapGestureRecognizer();
        private TapGestureRecognizer _rightTapGesture = new TapGestureRecognizer();
        private TapGestureRecognizer _switchTapGesture = new TapGestureRecognizer();
        private StackLayout _gestureListener;

        public CustomSlider()
        {
            SizeChanged += OnSizeChanged;

            _panGesture.PanUpdated += OnPanGestureUpdated;
            _switchTapGesture.Tapped += SwitchTapped;

            _gestureListener = new StackLayout { Orientation = StackOrientation.Horizontal };
            _gestureListener.GestureRecognizers.Add(_panGesture);
            _gestureListener.GestureRecognizers.Add(_switchTapGesture);
        }

        public event EventHandler SlideCompleted;

        private double posX;

        private const double _fadeEffect = 0.5;
        private const uint _animLength = 50;
        async void OnPanGestureUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (Thumb == null | TrackBar == null)
                return;

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    break;

                case GestureStatus.Running:
                    var velocityX = e.TotalX;
                    if ((posX + velocityX) > (Width - Thumb.Width - Thumb.Margin.Right))
                    {
                        Thumb.TranslationX = Width - Thumb.Width - 2 * Thumb.Margin.Right;
                    }
                    else if (posX + velocityX <= 0)
                    {
                        Thumb.TranslationX = 0;
                    }
                    else
                    {
                        Thumb.TranslationX = posX + velocityX;
                    }

                    break;

                case GestureStatus.Completed:
                    if ((Thumb.TranslationX + Thumb.Width / 2) > Width / 2)
                    {
                        await Thumb.TranslateTo(Width / 2, 0, _animLength * 2, Easing.CubicIn);
                        IsToggled = true;
                    }
                    else
                    {
                        await Thumb.TranslateTo(0, 0, _animLength * 2, Easing.CubicIn);
                        IsToggled = false;
                    }

                    posX = Thumb.TranslationX;

                    if (posX >= (Width - Thumb.Width - 10))
                        SlideCompleted?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }

        private async void SwitchTapped(object sender, EventArgs e)
        {
            if (IsToggled)
            {
                await Thumb.TranslateTo(0, 0, _animLength * 2, Easing.CubicIn);
            }
            else
            {
                await Thumb.TranslateTo(Width / 2, 0, _animLength * 2, Easing.CubicIn);
            }
            IsToggled = !IsToggled;
            posX = Thumb.TranslationX;
        }

        void OnSizeChanged(object sender, EventArgs e)
        {
            if (Width == 0 || Height == 0)
                return;
            if (Thumb == null || TrackBar == null || StateLabels == null)
                return;


            Children.Clear();

            SetLayoutFlags(TrackBar, AbsoluteLayoutFlags.SizeProportional);
            SetLayoutBounds(TrackBar, new Rectangle(0, 0, 1, 1));
            Children.Add(TrackBar);

            var thumbXPosition = IsToggled ? (Width/2) : 0;
            SetLayoutFlags(Thumb, AbsoluteLayoutFlags.YProportional);
            var rectangle = new Rectangle(thumbXPosition, 0.5, this.Width / 2, Thumb.Height);
            SetLayoutBounds(Thumb, rectangle);
            Children.Add(Thumb);

            SetLayoutFlags(StateLabels, AbsoluteLayoutFlags.SizeProportional);
            SetLayoutBounds(StateLabels, new Rectangle(0, 0, 1, 1));
            Children.Add(StateLabels);

            SetLayoutFlags(_gestureListener, AbsoluteLayoutFlags.SizeProportional);
            SetLayoutBounds(_gestureListener, new Rectangle(0, 0, 1, 1));
            Children.Add(_gestureListener);
        }
    }
}
