using System;
using System.Drawing;
using System.IO;
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
		private Graphics g, gf;
		private Bitmap b;
		private Pen p;
		private DateTime lastCrossUpdate = new DateTime (2001, 1, 1, 0, 0, 0);

		// Управление окном
		private const string HideWindowKey = "-h";
		private bool hideWindow = false;
		private NotifyIcon ni = new NotifyIcon ();

		// Возвращает текущий используемый экран
		private static Screen ActiveScreen
			{
			get
				{
				/*if (ScreenShooterSettings.ScreenNumber >= Screen.AllScreens.Length)
					ScreenShooterSettings.ScreenNumber = 0;*/

				return Screen.AllScreens[(int)ScreenShooterSettings.ScreenNumber];
				}
			}

		/// <summary>
		/// Главная форма программы
		/// </summary>
		/// <param name="Flags">Флаги запуска приложения</param>
		public ScreenShooterForm (string Flags)
			{
			InitializeComponent ();

			// Настройка
			this.Text = ProgramDescription.AssemblyTitle;
			if (!RDGenerics.AppHasAccessRights (false, true))
				this.Text += RDLocale.GetDefaultText (RDLDefaultTexts.Message_LimitedFunctionality);

			this.Left = this.Top = 0;
			this.Location = ActiveScreen.Bounds.Location;
			this.Width = ActiveScreen.Bounds.Width;
			this.Height = ActiveScreen.Bounds.Height;

			hideWindow = (Flags == HideWindowKey);

			// Настройка иконки в трее
			ni.Icon = ScreenShooterResources.ScreenShooterTray;
			ni.Text = ProgramDescription.AssemblyTitle;
			ni.Visible = true;

			ni.ContextMenuStrip = new ContextMenuStrip ();
			ni.ContextMenuStrip.ShowImageMargin = false;

			ni.ContextMenuStrip.Items.Add (RDLocale.GetText ("MenuSettings"),
				null, ChangeSettings);
			ni.ContextMenuStrip.Items.Add (RDLocale.GetDefaultText (RDLDefaultTexts.Control_InterfaceLanguage).Replace (":", ""),
				null, ChangeLanguage);
			ni.ContextMenuStrip.Items.Add (RDLocale.GetDefaultText (RDLDefaultTexts.Control_AppAbout),
				null, AppAbout);
			ni.ContextMenuStrip.Items.Add (RDLocale.GetDefaultText (RDLDefaultTexts.Button_Exit),
				null, CloseService);
			/*ni.ContextMenu.MenuItems[3].DefaultItem = true;*/

			ni.MouseDown += ReturnWindow;

			gf = Graphics.FromHwnd (this.Handle);
			p = new Pen (Color.FromArgb (255, 0, 0), 1.0f);
			}

		private void ScreenShooterForm_Shown (object sender, EventArgs e)
			{
			if (AboutForm.VeryFirstStart)
				RDInterface.LocalizedMessageBox (RDMessageTypes.Success_Left, "HelpKeysText");
			else if (hideWindow)
				this.Hide ();
			}

		private void ScreenShooterForm_FormClosing (object sender, FormClosingEventArgs e)
			{
			// Завершение
			if (ni != null)
				ni.Visible = false;
			if (gf != null)
				gf.Dispose ();
			if (p != null)
				p.Dispose ();
			}

		// Нажатие мыши
		private void MainForm_MouseDown (object sender, MouseEventArgs e)
			{
			// Сохранение изображения
			if (e.Button == MouseButtons.Right)
				{
				SaveImage (false);
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
			// Отрисовка перекрестия
			if (lastCrossUpdate.AddMilliseconds (10) < DateTime.Now)
				{
				lastCrossUpdate = DateTime.Now;
				gf.Clear (this.BackColor);

				if (e.Button != MouseButtons.Left)
					{
					gf.DrawLine (p, e.X, 0, e.X, this.Height);
					gf.DrawLine (p, 0, e.Y, this.Width, e.Y);

					return;
					}
				}

			// Нажатие кнопки
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
				SaveImage (false);
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
					RDInterface.LocalizedMessageBox (RDMessageTypes.Success_Left, "HelpKeysText");
					break;

				case Keys.F2:
					AppAbout (null, null);
					break;

				// Сохранение
				case Keys.Return:
					SaveImage (e.Shift);
					break;

				// Выход / скрытие окна
				case Keys.Escape:
				case Keys.H:
					this.Hide ();
					break;

				case Keys.X:
				case Keys.Q:
					this.Close ();
					break;

				// Настройки приложения
				case Keys.P:
				case Keys.S:
					ChangeSettings (null, null);
					break;

				// Сброс выделения
				case Keys.Space:
					if (MainSelection.Visible)
						{
						MainSelection.Visible = false;
						}
					else
						{
						if (!ActiveScreen.Primary)
							{
							RDInterface.LocalizedMessageBox (RDMessageTypes.Warning_Center,
								"NotAvailableForSecondaryScreen", 1500);
							}
						else if (GetPointedWindowBounds (MousePosition))
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
					ChangeLanguage (null, null);
					break;

				// Смена текущего экрана
				case Keys.Tab:
					/*ScreenShooterSettings.ScreenNumber =
						(uint)((ScreenShooterSettings.ScreenNumber + 1) % Screen.AllScreens.Length);*/
					ScreenShooterSettings.SetScreenNumber (ScreenShooterSettings.ScreenNumber + 1);

					this.Location = ActiveScreen.Bounds.Location;
					this.Width = ActiveScreen.Bounds.Width;
					this.Height = ActiveScreen.Bounds.Height;
					break;

				// Подмена картинки изображением, полученным от PrtScr
				case Keys.B:
					Image img = null;
					try
						{
						img = Clipboard.GetImage ();
						}
					catch { }
					if (img == null)
						{
						RDInterface.LocalizedMessageBox (RDMessageTypes.Warning_Center, "NoPictureInClipboard", 1500);
						return;
						}

					SaveBitmap ((Bitmap)img, e.Shift);
					break;
				}
			}

		// Метод извлекает изображение из указанной области и сохраняет его в файл
		private void SaveImage (bool AskTheName)
			{
			// Фиксация размера
			if (!MainSelection.Visible)
				{
				start.X = 0;
				start.Y = 0;
				end.X = ActiveScreen.Bounds.Width - 1;
				end.Y = ActiveScreen.Bounds.Height - 1;
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

			// Поправка на расположение на экране
			start.Offset (this.Location);
			end.Offset (this.Location);

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

			// Попытка сохранения
			SaveBitmap (b, AskTheName);
			}

		// Метод выполняет непосредственное сохранение изображения в файл
		private static void SaveBitmap (Bitmap Picture, bool AskTheName)
			{
			string path;
			if (AskTheName)
				{
				string name = RDInterface.LocalizedMessageBox ("EnterTheFileName", true, 256);
				if (string.IsNullOrWhiteSpace (name))
					return;

				char[] invChars = Path.GetInvalidFileNameChars ();
				for (int i = 0; i < invChars.Length; i++)
					name = name.Replace (invChars[i], '_');

				path = ScreenShooterSettings.NamedScreenshostPath + name +
					ScreenShooterSettings.ScreenshostFileExt;
				}
			else
				{
				path = ScreenShooterSettings.UnnamedScreenshotsPath + DateTime.Now.ToString ("yyyy-MM-dd HH-mm-ss") +
					ScreenShooterSettings.ScreenshostFileExt;
				}

			try
				{
				Picture.Save (path, ScreenShooterSettings.ScreenshotsFormat);

				RDInterface.LocalizedMessageBox (RDMessageTypes.Success_Center, "ImageSaved", 750);
				}
			catch
				{
				RDInterface.MessageBox (RDMessageTypes.Warning_Center,
					string.Format (RDLocale.GetDefaultText (RDLDefaultTexts.Message_SaveFailure_Fmt),
					Path.GetFileName (path)));
				}

			// Завершение
			Picture.Dispose ();
			}

		// Получение границ окна, на которое наведён курсор
		private bool GetPointedWindowBounds (Point Mouse)
			{
			// Получение дескриптора окна
			POINT p = new POINT (Mouse.X, Mouse.Y);
			/*IntPtr hWnd = IntPtr.Zero;*/
			IntPtr hWnd;

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
			/*RECT r = new RECT ();*/
			RECT r;
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

			if (r.Right > ActiveScreen.Bounds.Width)
				r.Right = ActiveScreen.Bounds.Width;
			if (r.Bottom > ActiveScreen.Bounds.Height)
				r.Bottom = ActiveScreen.Bounds.Height;

			MainSelection.Width = r.Right - r.Left;
			MainSelection.Height = r.Bottom - r.Top;
			end.X = r.Right - 1;
			end.Y = r.Bottom - 1;

			return true;
			}

		// Возврат окна приложения
		private void ReturnWindow (object sender, MouseEventArgs e)
			{
			if (e.Button != MouseButtons.Left)
				return;

			if (this.Visible)
				{
				this.Hide ();
				}
			else
				{
				this.Show ();

				this.TopMost = true;
				this.TopMost = false;
				this.WindowState = FormWindowState.Normal;
				}
			}

		// Закрытие службы
		private void CloseService (object sender, EventArgs e)
			{
			this.Close ();
			}

		// Изменение настроек
		private void ChangeSettings (object sender, EventArgs e)
			{
			ScreenShooterSettings s3 = new ScreenShooterSettings ();
			s3.Dispose ();
			}

		// Изменение языка интерфейса
		private void ChangeLanguage (object sender, EventArgs e)
			{
			RDInterface.MessageBox ();
			}

		// Информация о программе
		private void AppAbout (object sender, EventArgs e)
			{
			RDInterface.ShowAbout (false);
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
