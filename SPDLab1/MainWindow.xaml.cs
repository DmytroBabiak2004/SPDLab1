using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SPDLab1
{
    public partial class MainWindow : Window
    {
        private long modulus = 1023; // 2^10 - 1
        private long multiplier = 32; // 2^5
        private long increment = 0;
        private long seed = 2;
        private int count = 100;

        private List<long> generatedNumbers = new List<long>();
        private List<long> allNumbers = new List<long>();
        private int period = 0;
        private bool isGenerating = false;

        public MainWindow()
        {
            InitializeComponent();
            ResetToDefaults();
            UpdateUI();
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateParameters())
                return;

            isGenerating = true;
            ErrorMessageText.Text = "";
            generatedNumbers.Clear();
            allNumbers.Clear();
            period = 0;
            UpdateUI();

            try
            {
                var seen = new HashSet<long>();
                long x = seed;
                allNumbers.Add(x);
                seen.Add(x);

                for (int i = 0; i < count; i++)
                {
                    x = (multiplier * x + increment) % modulus;
                    generatedNumbers.Add(x);

                    if (allNumbers.Count <= 10000)
                    {
                        allNumbers.Add(x);
                    }
                }

                period = FindPeriod();
                UpdateUI();
            }
            catch (Exception ex)
            {
                ErrorMessageText.Text = $"Помилка при генерації: {ex.Message}";
            }
            finally
            {
                isGenerating = false;
                UpdateUI();
            }
        }

        private int FindPeriod()
        {
            var seen = new Dictionary<long, int>();
            long x = seed;

            for (int i = 0; i < Math.Min(modulus, 1000000); i++)
            {
                if (seen.ContainsKey(x))
                {
                    return i - seen[x];
                }

                seen[x] = i;
                x = (multiplier * x + increment) % modulus;
            }

            return -1;
        }

        private bool ValidateParameters()
        {
            if (!long.TryParse(ModulusTextBox.Text, out modulus) || modulus <= 0)
            {
                ErrorMessageText.Text = "Модуль повинен бути більше 0";
                return false;
            }

            if (!long.TryParse(MultiplierTextBox.Text, out multiplier) || multiplier < 0 || multiplier >= modulus)
            {
                ErrorMessageText.Text = $"Множник повинен бути в межах [0, {modulus - 1}]";
                return false;
            }

            if (!long.TryParse(IncrementTextBox.Text, out increment) || increment < 0 || increment >= modulus)
            {
                ErrorMessageText.Text = $"Приріст повинен бути в межах [0, {modulus - 1}]";
                return false;
            }

            if (!long.TryParse(SeedTextBox.Text, out seed) || seed < 0 || seed >= modulus)
            {
                ErrorMessageText.Text = $"Початкове число повинно бути в межах [0, {modulus - 1}]";
                return false;
            }

            if (!int.TryParse(CountTextBox.Text, out count) || count <= 0 || count > 90000)
            {
                ErrorMessageText.Text = "Кількість чисел повинна бути від 1 до 90000";
                return false;
            }

            return true;
        }

        private void SaveToFile_Click(object sender, RoutedEventArgs e)
        {
            if (generatedNumbers.Count == 0)
                return;

            try
            {
                var content = new StringBuilder();
                content.AppendLine($"Генератор псевдовипадкових чисел - Лінійне порівняння");
                content.AppendLine($"Дата генерації: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                content.AppendLine();
                content.AppendLine($"Параметри:");
                content.AppendLine($"Модуль (m): {modulus}");
                content.AppendLine($"Множник (a): {multiplier}");
                content.AppendLine($"Приріст (c): {increment}");
                content.AppendLine($"Початкове число (X₀): {seed}");
                content.AppendLine($"Кількість чисел: {count}");
                content.AppendLine($"Період: {period}");
                content.AppendLine();
                content.AppendLine("Згенеровані числа:");

                for (int i = 0; i < generatedNumbers.Count; i++)
                {
                    content.AppendLine($"{i + 1}: {generatedNumbers[i]}");
                }

                var fileName = $"lcg_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
                File.WriteAllText(filePath, content.ToString());
                MessageBox.Show($"Файл збережено: {filePath}", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessageText.Text = $"Помилка при збереженні файлу: {ex.Message}";
            }
        }

        private void LoadPreset_Click(object sender, RoutedEventArgs e)
        {
            modulus = 2147483647;
            multiplier = 16807;
            increment = 0;
            seed = 1;
            count = 100;

            ErrorMessageText.Text = "";
            generatedNumbers.Clear();
            allNumbers.Clear();
            period = 0;
            UpdateUI();
        }

        private void ResetToDefaults_Click(object sender, RoutedEventArgs e)
        {
            ResetToDefaults();
            UpdateUI();
        }

        private void ResetToDefaults()
        {
            modulus = 1023;
            multiplier = 32;
            increment = 0;
            seed = 2;
            count = 100;

            ErrorMessageText.Text = "";
            generatedNumbers.Clear();
            allNumbers.Clear();
            period = 0;
        }

        private void UpdateUI()
        {
            ModulusTextBox.Text = modulus.ToString();
            MultiplierTextBox.Text = multiplier.ToString();
            IncrementTextBox.Text = increment.ToString();
            SeedTextBox.Text = seed.ToString();
            CountTextBox.Text = count.ToString();
            SequenceListBox.ItemsSource = null;
            SequenceListBox.ItemsSource = generatedNumbers;
            PeriodInfoText.Text = $"Період: {period}, Елементів: {generatedNumbers.Count}";
            GenerateButton.IsEnabled = !isGenerating;
            SaveToFileButton.IsEnabled = !isGenerating && generatedNumbers.Count > 0;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

    }
}