using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Начальная форма программы
	/// </summary>
	public partial class ScreenShooterForm: Form
		{
		// Параметры
		private Point start, end;
		private Graphics g;
		private Bitmap b;

		/// <summary>
		/// Главная форма программы
		/// </summary>
		public ScreenShooterForm ()
			{
			InitializeComponent ();

			// Настройка
			this.Text = ProgramDescription.AssemblyTitle;
			if (!RDGenerics.IsRegistryAccessible)
				this.Text += Localization.GetDefaultText (LzDefaultTextValues.Message_LimitedFunctionality);

			this.Left = this.Top = 0;
			this.Width = Screen.PrimaryScreen.Bounds.Width;
			this.Height = Screen.PrimaryScreen.Bounds.Height;

			Localize ();
			}

		// Метод выполняет локализацию приложения
		private void Localize ()
			{
			SFDialog.Title = Localization.GetText ("SaveImageTitle");
			SFDialog.Filter = Localization.GetText ("SaveImageFilter");
			}

		// Нажатие мыши
		private void MainForm_MouseDown (object sender, MouseEventArgs e)
			{
			// Сохранение изображения
			if (e.Button == MouseButtons.Right)
				{
				SaveImage ();
				return;
				}

			// Обработка выделения области
			if (e.Button != MouseButtons.Left)
				return;

			if (!MainSelection.Visible)
				MainSelection.Visible = true;

			// Фиксация начальной точки
			MainSelection.Location = start = e.Location;
			}

		// Движение мыши
		private void MainForm_MouseMove (object sender, MouseEventArgs e)
			{
			if (e.Button != MouseButtons.Left)
				return;

			// Обновление рамки выделения
			if (e.X >= start.X)
				{
				MainSelection.Left = start.X;
				MainSelection.Width = e.X - start.X + 1;
				}
			else
				{
				MainSelection.Left = e.X;
				MainSelection.Width = start.X - e.X + 1;
				}

			if (e.Y >= start.Y)
				{
				MainSelection.Top = start.Y;
				MainSelection.Height = e.Y - start.Y + 1;
				}
			else
				{
				MainSelection.Top = e.Y;
				MainSelection.Height = start.Y - e.Y + 1;
				}

			// Отображение координат и размеров
			MainSelection.Text = "(" + MainSelection.Left.ToString () + "; " + MainSelection.Top.ToString () + ") (" +
				MainSelection.Width.ToString () + " x " + MainSelection.Height.ToString () + ")";
			}

		// Завершение выделения
		private void MainForm_MouseUp (object sender, MouseEventArgs e)
			{
			end = e.Location;
			}

		// Команды рамки выделения
		private void MainSelection_MouseDown (object sender, MouseEventArgs e)
			{
			if (e.Button == MouseButtons.Right)
				{
				SaveImage ();
				return;
				}

			if (MainSelection.Visible)
				MainSelection.Visible = false;
			}

		// Обработка клавиатуры
		private void MainForm_KeyDown (object sender, KeyEventArgs e)
			{
			switch (e.KeyCode)
				{
				// Справка
				case Keys.F1:
				case Keys.OemQuestion:
					RDGenerics.LocalizedMessageBox (RDMessageTypes.Success_Left, "HelpKeysText");
					break;

				case Keys.F2:
					RDGenerics.ShowAbout (false);
					break;

				// Сохранение
				case Keys.Return:
					SaveImage ();
					break;

				// Выход
				case Keys.Escape:
				case Keys.X:
				case Keys.Q:
					this.Close ();
					break;

				// Сброс выделения
				case Keys.Space:
					if (MainSelection.Visible)
						{
						MainSelection.Visible = false;
						}
					else
						{
						if (GetPointedWindowBounds (MousePosition.X, MousePosition.Y))
							{
							MainSelection.Text = "(" + MainSelection.Left.ToString () + "; " +
								MainSelection.Top.ToString () + ") (" + MainSelection.Width.ToString () +
								" x " + MainSelection.Height.ToString () + ")";
							MainSelection.Visible = true;
							}
						}
					break;

				// Смена языка интерфейса
				case Keys.L:
					if (RDGenerics.MessageBox ())
						Localize ();
					break;
				}
			}

		// Сохранение изображения
		private void SaveImage ()
			{
			// Фиксация размера
			if (!MainSelection.Visible)
				{
				start.X = 0;
				start.Y = 0;
				end.X = Screen.PrimaryScreen.Bounds.Width - 1;
				end.Y = Screen.PrimaryScreen.Bounds.Height - 1;
				}

			// На случай зеркального выделения
			if (start.X > end.X)
				{
				int x = start.X;
				start.X = end.X;
				end.X = x;
				}
			if (start.Y > end.Y)
				{
				int y = start.Y;
				start.Y = end.Y;
				end.Y = y;
				}

			// Получение дескриптора и снимка экрана
			if (b != null)
				b.Dispose ();
			b = new Bitmap (end.X - start.X + 1, end.Y - start.Y + 1);
			g = Graphics.FromImage (b);

			// На Windows 10 следует убирать окно ScreenShooter на задний план.
			// Иначе оно попадает на снимок
			this.SendToBack ();
			g.CopyFromScreen (start.X, start.Y, 0, 0, b.Size);
			this.BringToFront ();

			g.Dispose ();

			// Запрос имени файла (делается после снимка, чтобы не перекрывать экран)
			SFDialog.ShowDialog ();
			}

		private void SFDialog_FileOk (object sender, CancelEventArgs e)
			{
			// Обработка параметров
			try
				{
				switch (SFDialog.FilterIndex)
					{
					case 1:
						b.Save (SFDialog.FileName, ImageFormat.Png);
						break;

					case 2:
						b.Save (SFDialog.FileName, ImageFormat.Jpeg);
						break;

					case 3:
						b.Save (SFDialog.FileName, ImageFormat.Bmp);
						break;

					case 4:
						b.Save (SFDialog.FileName, ImageFormat.Gif);
						break;
					}
				}
			catch
				{
				RDGenerics.MessageBox (RDMessageTypes.Warning_Center,
					Localization.GetFileProcessingMessage (SFDialog.FileName,
					LzFileProcessingMessageTypes.Save_Failure));
				}

			// Завершение
			b.Dispose ();
			}

		// Получение границ окна, на которое наведён курсор
		private bool GetPointedWindowBounds (int X, int Y)
			{
			// Получение дескриптора окна
			POINT p = new POINT (X, Y);
			IntPtr hWnd = IntPtr.Zero;

			try
				{
				// Подмена текущего окна
				this.Hide ();
				hWnd = WindowFromPoint (p);
				this.Show ();

				if (hWnd == IntPtr.Zero)
					throw new Exception ();
				}
			catch
				{
				return false;
				}

			// Получение границ окна
			RECT r = new RECT ();
			try
				{
				if (!GetWindowRect (hWnd, out r))
					throw new Exception ();
				}
			catch
				{
				return false;
				}

			// Преобразование параметров и возврат
			MainSelection.Left = start.X = (r.Left < 0) ? 0 : r.Left;
			MainSelection.Top = start.Y = (r.Top < 0) ? 0 : r.Top;

			if (r.Right > Screen.PrimaryScreen.Bounds.Width)
				r.Right = Screen.PrimaryScreen.Bounds.Width;
			if (r.Bottom > Screen.PrimaryScreen.Bounds.Height)
				r.Bottom = Screen.PrimaryScreen.Bounds.Height;

			MainSelection.Width = r.Right - r.Left;
			MainSelection.Height = r.Bottom - r.Top;
			end.X = r.Right - 1;
			end.Y = r.Bottom - 1;

			return true;
			}

		// Описания, необходимые для получения границ окна
		[DllImport ("User32.dll")]
		private static extern bool GetWindowRect (IntPtr hWnd, out RECT Rectangle);

		private struct RECT
			{
			public Int32 Left;
			public Int32 Top;
			public Int32 Right;
			public Int32 Bottom;
			}

		[DllImport ("User32.dll")]
		private static extern IntPtr WindowFromPoint (POINT MousePoint);

		private struct POINT
			{
			public Int32 X;
			public Int32 Y;

			public POINT (Int32 XV, Int32 YV)
				{
				X = XV;
				Y = YV;
				}
			}
		}
	}
