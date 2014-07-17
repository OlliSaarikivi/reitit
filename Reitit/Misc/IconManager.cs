using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using Windows.UI.Xaml.Media;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Reitit
{
    public class IconManager : IEnumerable<Task<ImageSource>>
    {
        public const int DefaultFallbackIndex = 4;

        private static string[] _iconPaths =
        {
"/Icons/FavIcons/0.png",
"/Icons/FavIcons/1.png",
"/Icons/FavIcons/2.png",
"/Icons/FavIcons/3.png",
"/Icons/FavIcons/4.png",
"/Icons/FavIcons/5.png",
"/Icons/FavIcons/6.png",
"/Icons/FavIcons/7.png",
"/Icons/FavIcons/8.png",
"/Icons/FavIcons/9.png",
"/Icons/FavIcons/10.png",
"/Icons/FavIcons/11.png",
"/Icons/FavIcons/12.png",
"/Icons/FavIcons/13.png",
"/Icons/FavIcons/14.png",
"/Icons/FavIcons/15.png",
"/Icons/FavIcons/16.png",
"/Icons/FavIcons/17.png",
"/Icons/FavIcons/18.png",
"/Icons/FavIcons/19.png",
"/Icons/FavIcons/20.png",
"/Icons/FavIcons/21.png",
"/Icons/FavIcons/22.png",
"/Icons/FavIcons/23.png",
"/Icons/FavIcons/24.png",
"/Icons/FavIcons/25.png",
"/Icons/FavIcons/26.png",
"/Icons/FavIcons/27.png",
"/Icons/FavIcons/28.png",
"/Icons/FavIcons/29.png",
"/Icons/FavIcons/30.png",
"/Icons/FavIcons/31.png",
"/Icons/FavIcons/32.png",
"/Icons/FavIcons/33.png",
"/Icons/FavIcons/34.png",
"/Icons/FavIcons/35.png",
"/Icons/FavIcons/36.png",
"/Icons/FavIcons/37.png",
"/Icons/FavIcons/38.png",
        };

        private static string[] _tileIconPaths =
        {
"/Icons/FavIcons/T0.png",
"/Icons/FavIcons/T1.png",
"/Icons/FavIcons/T2.png",
"/Icons/FavIcons/T3.png",
"/Icons/FavIcons/T4.png",
"/Icons/FavIcons/T5.png",
"/Icons/FavIcons/T6.png",
"/Icons/FavIcons/T7.png",
"/Icons/FavIcons/T8.png",
"/Icons/FavIcons/T9.png",
"/Icons/FavIcons/T10.png",
"/Icons/FavIcons/T11.png",
"/Icons/FavIcons/T12.png",
"/Icons/FavIcons/T13.png",
"/Icons/FavIcons/T14.png",
"/Icons/FavIcons/T15.png",
"/Icons/FavIcons/T16.png",
"/Icons/FavIcons/T17.png",
"/Icons/FavIcons/T18.png",
"/Icons/FavIcons/T19.png",
"/Icons/FavIcons/T20.png",
"/Icons/FavIcons/T21.png",
"/Icons/FavIcons/T22.png",
"/Icons/FavIcons/T23.png",
"/Icons/FavIcons/T24.png",
"/Icons/FavIcons/T25.png",
"/Icons/FavIcons/T26.png",
"/Icons/FavIcons/T27.png",
"/Icons/FavIcons/T28.png",
"/Icons/FavIcons/T29.png",
"/Icons/FavIcons/T30.png",
"/Icons/FavIcons/T31.png",
"/Icons/FavIcons/T32.png",
"/Icons/FavIcons/T33.png",
"/Icons/FavIcons/T34.png",
"/Icons/FavIcons/T35.png",
"/Icons/FavIcons/T36.png",
"/Icons/FavIcons/T37.png",
"/Icons/FavIcons/T38.png",
        };

        private Dictionary<int, ImageSource> _sources = new Dictionary<int, ImageSource>();

        private static string GetCustomIconFileName(int index)
        {
            if (index < 0)
            {
                return "Icon-" + (-index).ToString("#") + ".bmp";
            }
            else
            {
                return "Icon-invalid-index-" + index.ToString("#") + ".bmp";
            }
        }

        private static int ReserveIndex()
        {
            return App.Current.Settings.CustomIconFreeIndex--;
        }

        public async Task<int> AddCustomIcon(Stream inStream)
        {
            int index = ReserveIndex();
            string fileName = GetCustomIconFileName(index);
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            Stream outStream;
            try
            {
                outStream = await file.OpenStreamForWriteAsync();

            }
            catch (IOException)
            {
                file.DeleteAsync();
                return DefaultFallbackIndex;
            }
            byte[] readBuffer = new byte[4096];
            int bytesRead = -1;
            while ((bytesRead = inStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
            {
                outStream.Write(readBuffer, 0, bytesRead);
            }
            return index;
        }

        public async Task RemoveCustomIcon(int index)
        {
            string fileName = GetCustomIconFileName(index);
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = null;
            try
            {
                file = await localFolder.GetFileAsync(fileName);
            }
            catch { }
            if (file != null) {
                await file.DeleteAsync();
            }
        }

        public async Task<ImageSource> GetImageSource(int index)
        {
            if (!_sources.ContainsKey(index))
            {
                if (index >= 0)
                {
                    _sources[index] = new BitmapImage(new Uri(_iconPaths[index], UriKind.Relative));
                }
                else
                {
                    string fileName = GetCustomIconFileName(index);
                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    Stream inStream = await localFolder.OpenStreamForReadAsync(fileName);

                    try
                    {
                        BitmapImage bmp = new BitmapImage();
                        await bmp.SetSourceAsync(inStream.AsRandomAccessStream());

                        _sources[index] = bmp;
                    }
                    catch (Exception)
                    {
                        _sources[index] = new BitmapImage(new Uri(_iconPaths[DefaultFallbackIndex], UriKind.Relative));
                    }
                }
            }

            return _sources[index];
        }

        public int Length
        {
            get
            {
                return _iconPaths.Length;
            }
        }

        public IEnumerator<Task<ImageSource>> GetEnumerator()
        {
            for (int i = 0; i < _iconPaths.Length; ++i)
            {
                yield return GetImageSource(i);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < _iconPaths.Length; ++i)
            {
                yield return GetImageSource(i);
            }
        }
    }
}
