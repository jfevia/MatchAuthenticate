using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Point = System.Drawing.Point;

namespace MachtAuthenticate.Localization.TypeConverters
{

    #region Uses

    #endregion

    /// <summary>
    ///     A type converter class for Bitmap resources that are used in WPF.
    /// </summary>
    public class BitmapSourceTypeConverter : TypeConverter
    {
        /// <summary>
        ///     Returns whether this converter can convert an object of the given type to the type of this converter, using the
        ///     specified context.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="sourceType">A <see cref="Type" /> that represents the type you want to convert from.</param>
        /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(Bitmap);
        }

        /// <summary>
        ///     Returns whether this converter can convert the object to the specified type, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="destinationType">A <see cref="Type" /> that represents the type you want to convert to.</param>
        /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(Bitmap);
        }

        /// <summary>
        ///     Converts the given object to the type of this converter, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="culture">The <see cref="CultureInfo" /> to use as the current culture.</param>
        /// <param name="value">The <see cref="Object" /> to convert.</param>
        /// <returns>An <see cref="Object" /> that represents the converted value.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var bitmap = value as Bitmap;

            if (bitmap == null)
                return null;

            var bmpPt = bitmap.GetHbitmap();

            // create the bitmapSource
            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                bmpPt,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            // freeze the bitmap to avoid hooking events to the bitmap
            bitmapSource.Freeze();

            // free memory
            DeleteObject(bmpPt);

            // return bitmapSource
            return bitmapSource;
        }

        /// <summary>
        ///     Frees memory of a pointer.
        /// </summary>
        /// <param name="o">Object to remove from memory.</param>
        /// <returns>0 if the removing was success, otherwise another number.</returns>
        [DllImport("gdi32.dll")]
        private static extern int DeleteObject(IntPtr o);

        /// <summary>
        ///     Converts the given value object to the specified type, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="culture">The <see cref="CultureInfo" /> to use as the current culture.</param>
        /// <param name="value">The <see cref="Object" /> to convert.</param>
        /// <param name="destinationType">The <see cref="Type" /> to convert the value parameter to.</param>
        /// <returns>An <see cref="Object" /> that represents the converted value.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var source = value as BitmapSource;

            if (source == null)
                return null;

            var bmp = new Bitmap(
                source.PixelWidth,
                source.PixelHeight,
                PixelFormat.Format32bppPArgb);

            var data = bmp.LockBits(
                new Rectangle(Point.Empty, bmp.Size),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppPArgb);

            source.CopyPixels(
                Int32Rect.Empty,
                data.Scan0,
                data.Height * data.Stride,
                data.Stride);

            bmp.UnlockBits(data);

            return bmp;
        }
    }
}