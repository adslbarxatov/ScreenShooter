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
		private Graphics g;
		private Bitmap b;

		// Управление окном
		private const string HideWindowKey = "-h";
		private bool hideWindow = false;
		private NotifyIcon ni = new NotifyIcon ();

		/// <summary>
		/// Главная форма программы
		/// </summary>
		/// <param name="Flags">Флаги запуска приложения</param>
		public ScreenShooterForm (string Flags)
			{
			InitializeComponent ();

			// Настройка
			this.Text = ProgramDescription.AssemblyTitle;
			if (!RDGenerics.IsRegistryAccessible)
				this.Text += RDLocale.GetDefaultText (RDLDefaultTexts.Message_LimitedFunctionality);

			this.Left = this.Top = 0;
			this.Width = Screen.PrimaryScreen.Bounds.Width;
			this.Height = Screen.PrimaryScreen.Bounds.Height;

			hideWindow = (Flags == HideWindowKey);

			// Настройка иконки в трее
			ni.Icon = Properties.ScreenShooter.ScreenShooterTray;
			ni.Text = ProgramDescription.AssemblyTitle;
			ni.Visible = true;

			ni.ContextMenu = new ContextMenu ();

			ni.ContextMenu.MenuItems.Add (new MenuItem (RDLocale.GetText ("MenuSettings"),
				ChangeSettings));
			ni.ContextMenu.MenuItems.Add (new MenuItem (RDLocale.GetDefaultText
				(RDLDefaultTexts.Control_InterfaceLanguage).Replace (":", ""), ChangeLanguage));
			ni.ContextMenu.MenuItems.Add (new MenuItem (RDLocale.GetDefaultText
				(RDLDefaultTexts.Control_AppAbout), AppAbout));
			ni.ContextMenu.MenuItems.Add (new MenuItem (RDLocale.GetDefaultText (RDLDefaultTexts.Button_Exit),
				CloseService));
			ni.ContextMenu.MenuItems[3].DefaultItem = true;

			ni.MouseDown += ReturnWindow;
			}

		private void ScreenShooterForm_Shown (object sender, EventArgs e)
			{
			if (AboutForm.VeryFirstStart)
				RDGenerics.LocalizedMessageBox (RDMessageTypes.Success_Left, "HelpKeysText");
			else if (hideWindow)
				this.Hide ();
			}

		private void ScreenShooterForm_FormClosing (object sender, FormClosingEventArgs e)
			{
			// Завершение
			if (ni != null)
				ni.Visible = false;
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
					AppAbout (null, null);
					break;

				// Сохранение
				case Keys.Return:
					SaveImage ();
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
					ChangeLanguage (null, null);
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

			// Попытка сохранения
			string path = ScreenShooterSettings.ScreenshostPath + DateTime.Now.ToString ("yyyy-MM-dd HH-mm-ss") +
				ScreenShooterSettings.ScreenshostFileExt;
			try
				{
				b.Save (path, ScreenShooterSettings.ScreenshotsFormat);

				RDGenerics.MessageBox (RDMessageTypes.Success_Center, RDLocale.GetText ("ImageSaved"), 750);
				}
			catch
				{
				RDGenerics.MessageBox (RDMessageTypes.Warning_Center,
					string.Format (RDLocale.GetDefaultText (RDLDefaultTexts.Message_SaveFailure_Fmt),
					Path.GetFileName (path)));
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
			RDGenerics.MessageBox ();
			}

		// Информация о программе
		private void AppAbout (object sender, EventArgs e)
			{
			RDGenerics.ShowAbout (false);
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
