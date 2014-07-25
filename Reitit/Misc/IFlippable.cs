using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Reitit
{
    public enum FlipPreference
    {
        East, West
    }

    public static class Flippable
    {
        public static bool GetParticipatesInFlipping(DependencyObject obj)
        {
            return (bool)obj.GetValue(ParticipatesInFlippingProperty);
        }

        public static void SetParticipatesInFlipping(DependencyObject obj, bool value)
        {
            obj.SetValue(ParticipatesInFlippingProperty, value);
        }
        public static readonly DependencyProperty ParticipatesInFlippingProperty =
            DependencyProperty.RegisterAttached("ParticipatesInFlipping", typeof(bool), typeof(Flippable), new PropertyMetadata(true));
    }

    interface IFlippable
    {
        int Importance { get; }
        void SetFlip(FlipPreference preference);
    }
}
