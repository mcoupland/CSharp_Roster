using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace Roster
{
    public class Employee : IComparable<Employee>
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string FullName
        {            
            get
            {
                string nickname = string.IsNullOrEmpty(NickName) ? "" : $", \"{NickName}\"";
                string middlename = string.IsNullOrEmpty(MiddleName) ? "" : $" {MiddleName}";
                return $"{FirstName}{middlename} {LastName}{nickname}";
            }
            set { }
        }
        public string ManagerFirstName { get; set; }
        public string ManagerMiddleName { get; set; }
        public string ManagerLastName { get; set; }
        public string ManagerFullName
        {
            get
            {
                string middlename = string.IsNullOrEmpty(ManagerMiddleName) ? "" : $" {ManagerMiddleName}";
                return $"{ManagerFirstName}{middlename} {ManagerLastName}";
            }
            set { }
        }
        public string Division { get; set; }
        public string Department { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsL4;

        public Employee() { }

        public Employee(string jsonfile)
        {

        }

        public void DeserializeEmployee(string jsonfile)
        {
            Employee employee = new Employee();
            using (StreamReader file = File.OpenText(jsonfile))
            {
                JsonSerializer serializer = new JsonSerializer();
                employee = (Employee)serializer.Deserialize(file, typeof(Employee));
                LastUpdated = DateTime.Now;
                FirstName = employee.FirstName;
                MiddleName = employee.MiddleName;
                LastName = employee.LastName;
                NickName = employee.NickName;
                FullName = employee.FullName;
                ManagerFirstName = employee.ManagerFirstName;
                ManagerMiddleName = employee.ManagerMiddleName;
                ManagerLastName = employee.ManagerLastName;
                ManagerFullName = employee.ManagerFullName;
                Division = employee.Division;
                Department = employee.Department;
                LastUpdated = employee.LastUpdated;
            }
        }
        public int CompareTo(Employee other)
        {
            return FullName.CompareTo(other.FullName);
        }
    }
}
