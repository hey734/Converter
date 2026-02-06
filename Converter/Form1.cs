using System;
using System.Drawing; // Для картинок
using System.IO; // Для работы с файлами
using System.Windows.Forms;
using PdfSharp.Pdf; // Для создания PDF-документов
using PdfSharp.Drawing; // Для рисования текста в PDF
using PdfSharp.Fonts; // Для шрифтов в PDF
using NAudio.Lame; // Для MP3
using NAudio.Wave; // Для Аудио

namespace Converter
{
    public partial class Form1 : Form
    {
        string selectedFilePath = ""; // Сюда запомним путь к файлу

        public Form1()
        {
            InitializeComponent();
            if (GlobalFontSettings.FontResolver == null)
            {
                GlobalFontSettings.FontResolver = new MyFontResolver();
            }
        }

        // 1. Логика кнопки "Выбрать файл"
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFilePath = openFileDialog.FileName;
                label1.Text = "Выбран: " + Path.GetFileName(selectedFilePath);
            }
        }

        // 2. Логика кнопки TXT <-> PDF
        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath)) { MessageBox.Show("Сначала выбери файл!"); return; }

            try
            {
                string text = File.ReadAllText(selectedFilePath);
                string outputPath = Path.ChangeExtension(selectedFilePath, ".pdf");

                PdfDocument document = new PdfDocument();
                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);

                XFont font = new XFont("Arial", 12, XFontStyleEx.Regular);

                XRect layoutRect = new XRect(XUnit.FromPoint(40), XUnit.FromPoint(40),
                                             page.Width - XUnit.FromPoint(80), page.Height - XUnit.FromPoint(80));

                gfx.DrawString(text, font, XBrushes.Black, layoutRect, XStringFormats.TopLeft);

                document.Save(outputPath);
                MessageBox.Show("Успех! PDF создан: " + Path.GetFileName(outputPath));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
        public class MyFontResolver : IFontResolver // Этот класс говорит PDF-библиотеке, где искать шрифты
        {
            public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic) 
            {
                // Здесь мы говорим, что для всех запросов шрифта "Arial" возвращаем наш файл "Arial.ttf"
                if (familyName.Equals("Arial", StringComparison.OrdinalIgnoreCase))
                {
                    return new FontResolverInfo("Arial.ttf");
                }
                return null;
            }

            public byte[] GetFont(string faceName) 
            {
                // Читаем файл шрифта напрямую из папки Windows
                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), faceName);
                return File.ReadAllBytes(fontPath);
            }
        }

        // 3. Логика кнопки PNG -> JPG
        private void button3_Click(object sender, EventArgs e) 
        {
            if (string.IsNullOrEmpty(selectedFilePath)) { MessageBox.Show("Сначала выбери файл!"); return; }

            try
            {
                using (System.Drawing.Image image = System.Drawing.Image.FromFile(selectedFilePath))
                {
                    string outputFolder = Path.GetDirectoryName(selectedFilePath);
                    string fileName = Path.GetFileNameWithoutExtension(selectedFilePath);
                    string outputPath = Path.Combine(outputFolder, fileName + ".jpg");

                    // Сохраняем в формате JPEG
                    image.Save(outputPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    MessageBox.Show("Готово! Картинка сохранена как JPG.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка (это точно картинка?): " + ex.Message);
            }
        }

        // 4. Логика кнопки FLAC -> MP3
        private void button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath)) { MessageBox.Show("Сначала выбери файл!"); return; }

            if (!selectedFilePath.EndsWith(".flac"))
            {
                MessageBox.Show("Пожалуйста, выберите файл .flac");
                return;
            }

            try
            {
                string outputFolder = Path.GetDirectoryName(selectedFilePath);
                string fileName = Path.GetFileNameWithoutExtension(selectedFilePath);
                string outputPath = Path.Combine(outputFolder, fileName + ".mp3");

                // Читаем FLAC и пишем MP3
                using (var reader = new NAudio.Wave.AudioFileReader(selectedFilePath))
                using (var writer = new LameMP3FileWriter(outputPath, reader.WaveFormat, LAMEPreset.STANDARD))
                {
                    reader.CopyTo(writer);
                }

                MessageBox.Show("Готово! Аудио сконвертировано.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка аудио: " + ex.Message + "\nУбедитесь, что файл libmp3lame.dll есть в папке с программой (обычно NAudio его подтягивает).");
            }
        }
    }
}