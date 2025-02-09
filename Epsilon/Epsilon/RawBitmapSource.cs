using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Epsilon
{
	public class RawBitmapSource : BitmapSource
	{
		private byte[] _buffer;
		private int _pixelWidth;
		private int _pixelHeight;
		public bool channelR = true;
		public bool channelG = true;
		public bool channelB = true;
		public bool channelA = true;
		public bool linkColorChannels;

		public RawBitmapSource(byte[] buffer, int pixelWidth) {
			this._buffer = buffer;
			this._pixelWidth = pixelWidth;
			this._pixelHeight = buffer.Length / ( 4 * pixelWidth );
		}

		public override void CopyPixels(Int32Rect sourceRect, Array pixels, int stride, int offset) {
			byte[] destination = (byte[])pixels;
			int dstOffset = offset;

			for (int y = sourceRect.Y; y < sourceRect.Y + sourceRect.Height; y++) {
				for (int x = sourceRect.X; x < sourceRect.X + sourceRect.Width; x++) {
					int srcOffset = stride * y + 4 * x;

					destination[dstOffset++] = channelB ? _buffer[srcOffset] : (byte)0;
					destination[dstOffset++] = channelG ? _buffer[srcOffset + 1] : (byte)0;
					destination[dstOffset++] = channelR ? _buffer[srcOffset + 2] : (byte)0;
					destination[dstOffset++] = channelA ? _buffer[srcOffset + 3] : (byte)255;
				}
			}
		}

		protected override Freezable CreateInstanceCore() {
			return new RawBitmapSource(_buffer, _pixelWidth);
		}

		public override event EventHandler<DownloadProgressEventArgs> DownloadProgress;
		public override event EventHandler DownloadCompleted;
		public override event EventHandler<ExceptionEventArgs> DownloadFailed;
		public override event EventHandler<ExceptionEventArgs> DecodeFailed;

		public override double DpiX {
			get { return 96; }
		}

		public override double DpiY {
			get { return 96; }
		}

		public override System.Windows.Media.PixelFormat Format {
			get { return PixelFormats.Bgra32; }
		}

		public override int PixelWidth {
			get { return _pixelWidth; }
		}

		public override int PixelHeight {
			get { return _pixelHeight; }
		}

		public override double Width {
			get { return _pixelWidth; }
		}

		public override double Height {
			get { return _pixelHeight; }
		}
	}
}
