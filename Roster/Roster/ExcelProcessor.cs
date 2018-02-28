using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace Roster
{
    public class ProgressUpdatedArgs : EventArgs
    {
        private string message;
        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public ProgressUpdatedArgs(string Message)
        {
            this.Message = Message;
        }
    }
    public class ProcessingCompleteArgs : EventArgs
    {
        private List<Employee> entries;
        public List<Employee> Entries
        {
            get { return entries; }
            set { entries = value; }
        }

        public ProcessingCompleteArgs(List<Employee> Entries)
        {
            this.Entries = Entries;
        }
    }

    public delegate void ProgressUpdatedHandler(object sender, ProgressUpdatedArgs e);
    public delegate void ProcessingCompleteHandler(object sender, ProcessingCompleteArgs e);

    public class ExcelProcessor
    {
        public event ProgressUpdatedHandler ProgressUpdated;
        public event ProcessingCompleteHandler ProcessingComplete;

        protected virtual void OnProgressUpdated(string message)
        {
            if (ProgressUpdated != null)
            {
                ProgressUpdated(this, new ProgressUpdatedArgs(message));
            }
        }

        protected virtual void OnProcessingComplete(ProcessingCompleteArgs e)
        {
            if (ProcessingComplete != null)
            {
                ProcessingComplete(this, e);
            }
        }

        public async void ImportFromExcel(string PhoneDirectory, List<string> L4S)
        {
            List<Employee> Entries = new List<Employee>();

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
                    OnProgressUpdated($"{i.ToString("000")}/{range.Rows.Count - 1}: {entry.FullName}");
                }
            });
            Entries.Sort();

            #region Close Excel
            book.Close(0);
            application.Quit();
            Marshal.ReleaseComObject(application);
            #endregion

            OnProcessingComplete(new ProcessingCompleteArgs(Entries));
        }
    }
}
