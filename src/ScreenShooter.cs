﻿using System;
using System.Threading;
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

			// Проверка запуска единственной копии
			bool result;
			Mutex instance = new Mutex (true, ProgramDescription.AssemblyTitle, out result);
			if (!result)
				{
				MessageBox.Show (string.Format (Localization.GetText ("AlreadyStarted", Localization.CurrentLanguage),
					ProgramDescription.AssemblyTitle),
					ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
				}

			// Отображение справки и запроса на принятие Политики
			if (!ProgramDescription.AcceptEULA ())
				return;
			ProgramDescription.ShowAbout (true);

			// Запуск
			Application.Run (new ScreenShooterForm ());
			}
		}
	}
