using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace Roster
{
    public class PhoneDirectory
    {
        public DateTime LastUpdated { get; set; }
        public List<Employee> Employees { get; set; }
        public List<string> L4S;

        public PhoneDirectory() { }

        public PhoneDirectory(string jsonfile)
        { 
            L4S = File.ReadAllLines(@"C:\Users\mcoupland\Documents\Visual Studio 2017\Projects\Roster\Roster\L4S.txt").ToList<string>();            
            DeserializeDirectory(@"C:\Users\mcoupland\Documents\Visual Studio 2017\Projects\Roster\Roster\Roster.json");
        }

        public void DeserializeDirectory(string jsonfile)
        {
            PhoneDirectory phonedirectory = new PhoneDirectory();
            using (StreamReader file = File.OpenText(jsonfile))
            {
                JsonSerializer serializer = new JsonSerializer();
                phonedirectory = (PhoneDirectory)serializer.Deserialize(file, typeof(PhoneDirectory));
                LastUpdated = DateTime.Now;
                Employees = phonedirectory.Employees;
            }
            foreach (Employee employee in Employees)
            {
                if (L4S.Contains(employee.FullName))
                {
                    employee.IsL4 = true;
                }
            }
        }
    }
}
