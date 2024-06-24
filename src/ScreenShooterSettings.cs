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
				/*if (typeIndex < uint.MaxValue)
					return typeIndex;

				try
					{
					typeIndex = uint.Parse (RDGenerics.GetAppSettingsValue ("TypeIndex"));
					}
				catch
					{
					typeIndex = 0;
					}

				return typeIndex;*/
				return RDGenerics.GetSettings (typeIndexPar, 0);
				}
			set
				{
				/*typeIndex = value;
				RDGenerics.SetAppSettingsValue ("TypeIndex", typeIndex.ToString ());*/
				RDGenerics.SetSettings (typeIndexPar, value);
				}
			}
		/*private static uint typeIndex = uint.MaxValue;
		*/
		private const string typeIndexPar = "TypeIndex";

		/// <summary>
		/// Возвращает или задаёт индекс варианта хранения скриншотов
		/// </summary>
		private static uint PathIndex
			{
			get
				{
				/*if (pathIndex < uint.MaxValue)
					return pathIndex;

				try
					{
					pathIndex = uint.Parse (RDGenerics.GetAppSettingsValue ("PathIndex"));
					}
				catch
					{
					pathIndex = 0;
					}

				return pathIndex;*/
				return RDGenerics.GetSettings (pathIndexPar, 0);
				}
			set
				{
				/*pathIndex = value;
				RDGenerics.SetAppSettingsValue ("PathIndex", pathIndex.ToString ());*/
				RDGenerics.SetSettings (pathIndexPar, value);
				}
			}
		/*private static uint pathIndex = uint.MaxValue;
		*/
		private const string pathIndexPar = "PathIndex";

		/// <summary>
		/// Конструктор. Запускает настройку программы
		/// </summary>
		public ScreenShooterSettings ()
			{
			// Инициализация и локализация формы
			InitializeComponent ();

			SaveButton.Text = RDLocale.GetDefaultText (RDLDefaultTexts.Button_Save);
			AbortButton.Text = RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel);
			this.Text = ProgramDescription.AssemblyTitle;

			for (int i = 0; i < 4; i++)
				PathCombo.Items.Add (RDLocale.GetText ("Path" + i.ToString ("D2")));
			try
				{
				PathCombo.SelectedIndex = (int)PathIndex;
				}
			catch
				{
				PathCombo.SelectedIndex = 0;
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

			ScreenshotsPath.Text = RDLocale.GetText ("ScreenshotsPath");
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
			PathIndex = (uint)PathCombo.SelectedIndex;
			screenshostPath = "";
			TypeIndex = (uint)TypeCombo.SelectedIndex;
			screenshotsFormat = ImageFormat.Exif;

			this.Close ();
			}

		/// <summary>
		/// Возвращает путь для сохранения изображений
		/// </summary>
		public static string ScreenshostPath
			{
			get
				{
				if (!string.IsNullOrWhiteSpace (screenshostPath))
					return screenshostPath;

				// Сборка пути
				switch (PathIndex)
					{
					case 0:
					case 1:
					default:
						screenshostPath = Environment.GetFolderPath (Environment.SpecialFolder.Desktop);
						break;

					case 2:
					case 3:
						screenshostPath = Environment.GetFolderPath (Environment.SpecialFolder.MyPictures);
						break;
					}

				if (!screenshostPath.EndsWith ("\\"))
					screenshostPath += "\\";
				if (PathIndex % 2 == 1)
					screenshostPath += (ProgramDescription.AssemblyMainName + " - " +
						DateTime.Now.ToString ("yyyy-MM-dd") + "\\");

				// Попытка создания
				try
					{
					Directory.CreateDirectory (screenshostPath);
					}
				catch { }

				return screenshostPath;
				}
			}
		private static string screenshostPath = "";

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
		}
	}
