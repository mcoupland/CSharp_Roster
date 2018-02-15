using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using System.ComponentModel;

namespace Roster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(int hWnd, out int lpdwProcessId);
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private string PhoneDirectory = @"S:\EmployeeDirectory\PhoneDirectory.xls";
        private List<Employee> Entries = new List<Employee>();
        List<string> L4S = File.ReadAllLines(@"C:\Users\mcoupland\Documents\Visual Studio 2017\Projects\Roster\Roster\L4S.txt").ToList<string>();

        public MainWindow()
        {
            InitializeComponent();
            ContentRendered += MainWindow_ContentRendered;
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            if(!File.Exists(PhoneDirectory))
            {
                MessageBox.Show($"Source file is stored at {PhoneDirectory}.  Please map this folder to the appropriate drive and restart the application.", "Could not find source file.");
                this.Close();
                return;
            }
            ImportFromExcel();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs ea)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            string search = Search.Text;
            search = search.Replace(',', ' ');
            search = search.Replace("\"", "");
            List<string> tokens = search.Split(' ').ToList<string>();
            List<Employee> entries = new List<Employee>();
            foreach (string token in tokens)
            {
                if (token.Length > 1)
                {
                    entries.AddRange(Entries.Where(x => x.FullName.ToLower().Contains(token.Trim().ToLower())).ToList<Employee>());
                    entries.AddRange(Entries.Where(x => x.ManagerFullName.ToLower().Contains(token.Trim().ToLower())).ToList<Employee>());
                }
            }
            AddEntriesToGrid(entries.Distinct().ToList<Employee>());
            AddHeadersToGrid();
            Mouse.OverrideCursor = Cursors.Arrow;
        }
        
        private void AddHeadersToGrid()
        {
            AddValueToGrid("Employee", 0, 0, OutputGrid, null, false);
            AddValueToGrid("Manager", 0, 1, OutputGrid, null, false);
            AddValueToGrid("Division", 0, 2, OutputGrid, null, false);
            AddValueToGrid("Department", 0, 3, OutputGrid, null, false);
        }

        private async void ImportFromExcel()
        {
            Mouse.OverrideCursor = Cursors.AppStarting;            
            #region Prepare Excel
            Excel.Application application = new Excel.Application() { Visible = false };
            application.UserControl = false;
            application.DisplayAlerts = false;
            Excel.Workbook book = application.Workbooks.Open(PhoneDirectory);
            Excel.Worksheet sheet = book.Sheets[1];
            Excel.Range range = sheet.UsedRange;
            #endregion

            await Task.Run(() =>
            {
                for (int i = 1; i < range.Rows.Count; i++)
                {
                    Employee entry = new Employee();
                    entry.LastName = range.Cells[i, 1].Value2;
                    entry.FirstName = range.Cells[i, 2].Value2;
                    if(entry.LastName == "Kemp")
                    {
                        int x = 0;
                    }
                    entry.MiddleName = range.Cells[i, 3].Value2;
                    entry.NickName = range.Cells[i, 5].Value2;
                    entry.ManagerLastName = range.Cells[i, 14].Value2;
                    entry.ManagerFirstName = range.Cells[i, 15].Value2;
                    entry.ManagerMiddleName = range.Cells[i, 16].Value2;
                    entry.Division = range.Cells[i, 8].Value2;
                    entry.Department = range.Cells[i, 9].Value2;
                    if (L4S.Contains(entry.FullName))
                    {
                        entry.IsL4 = true;
                    }
                    Entries.Add(entry);
                    UpdateUI(entry.FullName, i, range.Rows.Count - 1);
                }
            });
            Entries.Sort();

            #region Close Excel
            book.Close(0);
            application.Quit();
            Marshal.ReleaseComObject(application);
            #endregion

            AddEntriesToGrid(Entries);
            AddHeadersToGrid();
            Search.Focus();
            LoadingRect.Visibility = Visibility.Collapsed;
            Loading.Visibility = Visibility.Collapsed;
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void AddEntriesToGrid(List<Employee> entries)
        {
            OutputGrid.Children.Clear();
            if (entries == null || entries.Count <= 0) { return; }
            
            for (int i = 0; i < entries.Count; i++)
            {
                int rowindex = i + 1;
                if (entries[i].IsL4)
                {
                    string name = $"{entries[i].FullName} (L4)";
                    AddValueToGrid(name, rowindex, 0, OutputGrid, entries[i], true);
                }
                else
                {
                    AddValueToGrid(entries[i].FullName, rowindex, 0, OutputGrid, entries[i], false, true);
                }
                string managername = entries[i].ManagerFullName;
                bool isl4 = false;
                foreach (string l4 in L4S)
                {
                    if(l4.Contains(entries[i].ManagerFullName))
                    {
                        managername = $"{entries[i].ManagerFullName} (L4)";
                        isl4 = true;
                        break;
                    }
                }
                AddValueToGrid(managername, rowindex, 1, OutputGrid, entries[i], isl4, true);
                AddValueToGrid(entries[i].Division, rowindex, 2, OutputGrid, entries[i], false);
                AddValueToGrid(entries[i].Department, rowindex, 3, OutputGrid, entries[i], false);
            }
        }

        private void AddValueToGrid(string value, int rowindex, int columnindex, Grid grid, Employee entry, bool isl4, bool cansearch = false)
        {
            Brush background = rowindex % 2 == 0 ? Brushes.Transparent : Brushes.White;
            Brush foreground = isl4 ? Brushes.Red : Brushes.Black;
            FontWeight fontweight = isl4 ? FontWeights.Bold : FontWeights.Normal;
            TextBox box = new TextBox
            {
                Text = value,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Padding = new Thickness(10),
                IsReadOnly = true,
                Background = background,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = foreground,
                FontWeight = fontweight,
                Tag = entry
            };
            box.SetValue(Grid.ColumnProperty, columnindex);
            box.SetValue(Grid.RowProperty, rowindex);
            RowDefinition row = new RowDefinition();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.Children.Add(box);

            if (cansearch)
            {
                box.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(
                    delegate (Object o, MouseButtonEventArgs e)
                    {
                        Search.Text = box.Text;
                        SearchButton_Click(null, null);
                    }
                );
                box.MouseEnter += Box_MouseEnter;
                box.MouseLeave += Box_MouseLeave;
            }
        }

        private void Box_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void Box_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        public void UpdateUI(string name, int count, int total)
        {
            synchronizationContext.Post(new SendOrPostCallback(o =>
            {
                Loading.HorizontalContentAlignment = HorizontalAlignment.Left;
                Loading.Content = $"{count.ToString("000")}/{total}: {name}";
            }), name);
            
        }
    }
}
