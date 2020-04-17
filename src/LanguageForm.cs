using System.Drawing;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Форма обеспечивает возможность изменения языка интерфейса
	/// </summary>
	public partial class LanguageForm:Form
		{
		/// <summary>
		/// Конструктор. Запускает форму выбора размера
		/// </summary>
		/// <param name="CurrentInterfaceLanguage">Текущий язык интерфейса</param>
		public LanguageForm (SupportedLanguages CurrentInterfaceLanguage)
			{
			// Инициализация
			InitializeComponent ();

			LanguagesCombo.Items.AddRange (Localization.LanguagesNames);
			try
				{
				LanguagesCombo.SelectedIndex = (int)CurrentInterfaceLanguage;
				}
			catch
				{
				LanguagesCombo.SelectedIndex = 0;
				}

			this.Text = ProgramDescription.AssemblyTitle;
			Label01.Text = string.Format (Localization.GetText ("LanguageSelectorMessage", CurrentInterfaceLanguage), LanguagesCombo.Text);
			OKButton.Text = Localization.GetText ("NextButtonText", CurrentInterfaceLanguage);
			AbortButton.Text = Localization.GetText ("AbortButtonText", CurrentInterfaceLanguage);

			this.BackColor = ProgramDescription.MasterBackColor;
			Label01.ForeColor = LanguagesCombo.ForeColor = OKButton.ForeColor = AbortButton.ForeColor = ProgramDescription.MasterTextColor;
			LanguagesCombo.BackColor = OKButton.BackColor = AbortButton.BackColor = ProgramDescription.MasterButtonColor;

			// Запуск
			this.ShowDialog ();
			}

		// Выбор размера
		private void BOK_Click (object sender, System.EventArgs e)
			{
			Localization.CurrentLanguage = (SupportedLanguages)LanguagesCombo.SelectedIndex;
			this.Close ();
			}

		// Отмена
		private void BCancel_Click (object sender, System.EventArgs e)
			{
			this.Close ();
			}
		}
	}
