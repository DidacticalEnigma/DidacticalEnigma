using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DidacticalEnigma.Utils
{
    class ControlRenderCache<TKey>
    {
        private readonly int width;
        private readonly int height;
        private readonly Func<TKey, FrameworkElement> factory;

        private readonly ConcurrentDictionary<TKey, BitmapSource> underlying = new ConcurrentDictionary<TKey, BitmapSource>();

        public BitmapSource Get(TKey key)
        {
            return underlying.GetOrAdd(key, c =>
            {
                var b = factory(c);
                b.Width = width;
                b.Height = height;
                b.Measure(new Size(width, height));
                b.Arrange(new Rect(new Size(width, height)));
                var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(b);
                bitmap.Freeze();
                return bitmap;
            });
        }

        public ControlRenderCache(int width, int height, Func<TKey, FrameworkElement> factory)
        {
            this.width = width;
            this.height = height;
            this.factory = factory;
        }
    }
}
