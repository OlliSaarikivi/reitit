using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Toolkit;

namespace Reitit
{
    public partial class ReititMap : UserControl
    {
        private readonly int _screenHeight;

        public ReititMap()
        {
            InitializeComponent();
            var content = Application.Current.Host.Content;
            _screenHeight = (int)Math.Ceiling((double)(content.ActualHeight * content.ScaleFactor) / 100);
        }

        public void UpdateMapTransform(double contentHeight)
        {
            if (Map != null)
            {
                Map.TransformCenter = new Point(0.5, 1 - (double)(_screenHeight + contentHeight) / (2 * _screenHeight));
                Map.SetView(Map.Center.Copy().DisplaceFrom(Map.Center), Map.ZoomLevel, MapAnimationKind.None);
            }
        }
    }
}
