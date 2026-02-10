using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает форму настроек программы
	/// </summary>
	public partial class ScreenShooterSettings: Form
		{
		/// <summary>
		/// Возвращает или задаёт индекс типа файла скриншота
		/// </summary>
		private static uint TypeIndex
			{
			get
				{
				return RDGenerics.GetSettings (typeIndexPar, 0);
				}
			set
				{
				RDGenerics.SetSettings (typeIndexPar, value);
				}
			}
		private const string typeIndexPar = "TypeIndex";

		/// <summary>
		/// Возвращает или задаёт индекс автоматического варианта хранения скриншотов
		/// </summary>
		private static uint UnnamedPathIndex
			{
			get
				{
				return RDGenerics.GetSettings (unnamedPathIndexPar, 0);
				}
			set
				{
				RDGenerics.SetSettings (unnamedPathIndexPar, value);
				}
			}
		private const string unnamedPathIndexPar = "PathIndex";

		/// <summary>
		/// Возвращает или задаёт индекс варианта хранения именованных скриншотов
		/// </summary>
		private static uint NamedPathIndex
			{
			get
				{
				return RDGenerics.GetSettings (namedPathIndexPar, 0);
				}
			set
				{
				RDGenerics.SetSettings (namedPathIndexPar, value);
				}
			}
		private const string namedPathIndexPar = "NamedPathIndex";

		/// <summary>
		/// Возвращает индекс текущего экрана
		/// </summary>
		public static uint ScreenNumber
			{
			get
				{
				return RDGenerics.GetSettings (currentScreenPar, 0);
				}
			}
		private const string currentScreenPar = "CurrentScreen";

		/// <summary>
		/// Метод сохраняет номер текущего экрана
		/// </summary>
		/// <param name="Number">Новый номер текущего экрана</param>
		public static void SetScreenNumber (uint Number)
			{
			uint number = Number;
			if (number >= Screen.AllScreens.Length)
				number = 0;

			RDGenerics.SetSettings (currentScreenPar, number);
			}

		/// <summary>
		/// Конструктор. Запускает настройку программы
		/// </summary>
		public ScreenShooterSettings ()
			{
			// Инициализация и локализация формы
			InitializeComponent ();

			SaveButton.Text = RDLocale.GetDefaultText (RDLDefaultTexts.Button_Save);
			AbortButton.Text = RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel);
			this.Text = RDGenerics.DefaultAssemblyVisibleName;
			RDGenerics.LoadWindowDimensions (this);

			for (int i = 0; i < 4; i++)
				{
				string text = RDLocale.GetText ("Path" + i.ToString ("D2"));
				NamedPathCombo.Items.Add (text);
				UnnamedPathCombo.Items.Add (text);
				}

			try
				{
				NamedPathCombo.SelectedIndex = (int)NamedPathIndex;
				}
			catch
				{
				NamedPathCombo.SelectedIndex = 0;
				}

			try
				{
				UnnamedPathCombo.SelectedIndex = (int)UnnamedPathIndex;
				}
			catch
				{
				UnnamedPathCombo.SelectedIndex = 0;
				}

			TypeCombo.Items.Add ("PNG");
			TypeCombo.Items.Add ("JPEG");
			TypeCombo.Items.Add ("GIF");
			TypeCombo.Items.Add ("BMP");
			try
				{
				TypeCombo.SelectedIndex = (int)TypeIndex;
				}
			catch
				{
				TypeCombo.SelectedIndex = 0;
				}

			NScreenshotsPath.Text = RDLocale.GetText ("NamedScreenshotsPath");
			UScreenshotsPath.Text = RDLocale.GetText ("UnnamedScreenshotsPath");
			ScreenshotsType.Text = RDLocale.GetText ("ScreenshotsType");

			// Запуск
			this.ShowDialog ();
			}

		// Отмена
		private void SaveAbort_Click (object sender, EventArgs e)
			{
			this.Close ();
			}

		// Сохранение
		private void SaveSettings_Click (object sender, EventArgs e)
			{
			// Сохранение настроек и создание требования к реинициализации значений
			NamedPathIndex = (uint)NamedPathCombo.SelectedIndex;
			namedScreenshotsPath = "";

			UnnamedPathIndex = (uint)UnnamedPathCombo.SelectedIndex;
			unnamedScreenshotsPath = "";

			TypeIndex = (uint)TypeCombo.SelectedIndex;
			screenshotsFormat = ImageFormat.Exif;

			this.Close ();
			}

		/// <summary>
		/// Возвращает путь для автоматического сохранения изображений
		/// </summary>
		public static string UnnamedScreenshotsPath
			{
			get
				{
				if (!string.IsNullOrWhiteSpace (unnamedScreenshotsPath))
					return unnamedScreenshotsPath;

				// Сборка пути
				switch (UnnamedPathIndex)
					{
					case 0:
					case 1:
					default:
						unnamedScreenshotsPath = Environment.GetFolderPath (Environment.SpecialFolder.Desktop);
						break;

					case 2:
					case 3:
						unnamedScreenshotsPath = Environment.GetFolderPath (Environment.SpecialFolder.MyPictures);
						break;
					}

				if (!unnamedScreenshotsPath.EndsWith ('\\'))
					unnamedScreenshotsPath += "\\";

				if (UnnamedPathIndex % 2 == 1)
					{
					unnamedScreenshotsPath += (ProgramDescription.AssemblyMainName + " - " +
						DateTime.Now.ToString ("yyyy-MM-dd") + "\\");

					// Попытка создания
					try
						{
						Directory.CreateDirectory (unnamedScreenshotsPath);
						}
					catch { }
					}

				return unnamedScreenshotsPath;
				}
			}
		private static string unnamedScreenshotsPath = "";

		/// <summary>
		/// Возвращает путь для сохранения изображений с названиями
		/// </summary>
		public static string NamedScreenshostPath
			{
			get
				{
				if (!string.IsNullOrWhiteSpace (namedScreenshotsPath))
					return namedScreenshotsPath;

				// Сборка пути
				switch (NamedPathIndex)
					{
					case 0:
					case 1:
					default:
						namedScreenshotsPath = Environment.GetFolderPath (Environment.SpecialFolder.Desktop);
						break;

					case 2:
					case 3:
						namedScreenshotsPath = Environment.GetFolderPath (Environment.SpecialFolder.MyPictures);
						break;
					}

				if (!namedScreenshotsPath.EndsWith ('\\'))
					namedScreenshotsPath += "\\";

				if (NamedPathIndex % 2 == 1)
					{
					namedScreenshotsPath += (ProgramDescription.AssemblyMainName + " - " +
						DateTime.Now.ToString ("yyyy-MM-dd") + "\\");

					// Попытка создания
					try
						{
						Directory.CreateDirectory (namedScreenshotsPath);
						}
					catch { }
					}

				return namedScreenshotsPath;
				}
			}
		private static string namedScreenshotsPath = "";

		/// <summary>
		/// Возвращает расширение файлов для сохранения изображений
		/// </summary>
		public static string ScreenshostFileExt
			{
			get
				{
				switch (TypeIndex)
					{
					case 0:
					default:
						return ".png";

					case 1:
						return ".jpeg";

					case 2:
						return ".gif";

					case 3:
						return ".bmp";
					}
				}
			}

		/// <summary>
		/// Возвращает тип сохраняемых изображений
		/// </summary>
		public static ImageFormat ScreenshotsFormat
			{
			get
				{
				if (screenshotsFormat != ImageFormat.Exif)
					return screenshotsFormat;

				// Сборка пути
				switch (TypeIndex)
					{
					case 0:
					default:
						screenshotsFormat = ImageFormat.Png;
						break;

					case 1:
						screenshotsFormat = ImageFormat.Jpeg;
						break;

					case 2:
						screenshotsFormat = ImageFormat.Gif;
						break;

					case 3:
						screenshotsFormat = ImageFormat.Bmp;
						break;
					}

				return screenshotsFormat;
				}
			}
		private static ImageFormat screenshotsFormat = ImageFormat.Exif;

		// Закрытие окна
		private void ScreenShooterSettings_FormClosing (object sender, FormClosingEventArgs e)
			{
			RDGenerics.SaveWindowDimensions (this);
			}
		}
	}
