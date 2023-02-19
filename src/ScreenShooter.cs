using System;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает основную программу
	/// </summary>
	public static class ScreenShooterProgram
		{
		/// <summary>
		/// Конструктор. Описывает точку входа приложения
		/// </summary>
		[STAThread]
		public static void Main ()
			{
			// Инициализация
			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);

			// Язык интерфейса и контроль XPR
			/*SupportedLanguages al = Localization.CurrentLanguage;*/
			if (!Localization.IsXPUNClassAcceptable)
				return;

			// Проверка запуска единственной копии
			if (!RDGenerics.IsThisInstanceUnique (Localization.IsCurrentLanguageRuRu))
				return;

			// Отображение справки и запроса на принятие Политики
			if (!RDGenerics.AcceptEULA ())
				return;
			RDGenerics.ShowAbout (true);

			// Запуск
			Application.Run (new ScreenShooterForm ());
			}
		}
	}
