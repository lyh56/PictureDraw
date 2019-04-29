using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PictureDraw
{
    /// <summary>
    /// DrawContainer.xaml 的交互逻辑
    /// </summary>
    public partial class DrawContainer : UserControl
    {
        private string[] _files;
        private VisualCollection _child;
        private Dictionary<string, DrawingVisual> visualCache = new Dictionary<string, DrawingVisual>();
        private int renderIndex = 0;
        public DrawContainer()
        {
            InitializeComponent();
            _child = new VisualCollection(this);
            CompositionTarget.Rendering += CompositionTarget_Rendering;
            this.Loaded += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(this.ImagesPosition))
                {
                    this.LoadFiles();
                }
            };
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (_files != null && _files.Length > 0 && visualCache != null)
            {
                if (renderIndex >= _files.Length)
                {
                    renderIndex = 0;
                }

                string fileName = System.IO.Path.GetFileName(_files[renderIndex]);
                if (visualCache.ContainsKey(fileName))
                {
                    _child.Clear();
                    _child.Add(visualCache[fileName]);
                    GC.Collect();
                    renderIndex++;
                }
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _child == null ? null : _child[index];
        }

        protected override int VisualChildrenCount => _child == null ? 0 : _child.Count;

        public string ImagesPosition
        {
            get { return (string)GetValue(ImagesPositionProperty); }
            set { SetValue(ImagesPositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImagesPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImagesPositionProperty =
            DependencyProperty.Register("ImagesPosition", typeof(string), typeof(DrawContainer), new PropertyMetadata(ImagesPositionPropertyChangedCallback));


        public static void ImagesPositionPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as DrawContainer).LoadFiles();
        }

        private void LoadFiles()
        {
            if (!this.IsLoaded) return;
            if (!string.IsNullOrEmpty(this.ImagesPosition) && System.IO.Directory.Exists(this.ImagesPosition))
            {
                _files = Directory.GetFiles(this.ImagesPosition);
                foreach (var file in _files)
                {
                    if (!visualCache.ContainsKey(file))
                    {
                        var dv = CreateDrawingVisual(file);
                        visualCache.Add(System.IO.Path.GetFileName(file), dv);
                    }
                }
            }
        }

        private DrawingVisual CreateDrawingVisual(string fileName)
        {
            var dv = new DrawingVisual();
            using (var drawingContext = dv.RenderOpen())
            {
                var imageSource = new BitmapImage(new Uri(fileName, UriKind.Absolute));
                drawingContext.DrawImage(imageSource, new Rect(new Point(0, 0), this.RenderSize));
            }
            
            return dv;
        }


    }
}
