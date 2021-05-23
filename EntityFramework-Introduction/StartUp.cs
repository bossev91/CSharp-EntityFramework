using System;
using SoftUni.Data;
using SoftUni.Models;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace SoftUni
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            var softUniContext = new SoftUniContext();

            var result = DeleteProjectById(softUniContext);

            Console.WriteLine(result);
        }
        public static string RemoveTown(SoftUniContext context)
        {
            var townName = "Seattle";

            var townToDelete = context
                .Towns
                .Where(t => t.Name == townName)
                .FirstOrDefault();

            var addresses = context
                .Addresses
                .Where(a => a.TownId == townToDelete.TownId)
                .ToList();

            foreach (var adr in addresses)
            {
                var employees = context
                    .Employees
                    .Where(e => e.AddressId == adr.AddressId)
                    .ToList();

                foreach (var emp in employees)
                {
                    emp.AddressId = null;
                }

                context.Addresses.Remove(adr);
            }

            context.Towns.Remove(townToDelete);

            context.SaveChanges();

            return $"{addresses.Count()} addresses in {townName} were deleted";
        }

        public static string DeleteProjectById(SoftUniContext context)
        {
            var employees = context.Employees
                .Include(x => x.EmployeesProjects)
                .Where(x => x.EmployeesProjects.Any(p => p.ProjectId == 2))
                .ToList();

            foreach (var employee in employees)
            {
               // Console.WriteLine($"{employee.FirstName} {employee.LastName}");
                foreach (var item in employee.EmployeesProjects)
                {
                    if(item.ProjectId == -1)
                    {
                        item.ProjectId = 2;
                    }
                  //  Console.WriteLine($" Project id is: {item.ProjectId}");
                }
            }

            var project = context.Projects.Find(2);
            context.Projects.Remove(project);


            //context.SaveChanges();

            return null;

           // var projects = context.Projects
        }

        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            var employees = context.Employees.Select(x => new
            {
                x.FirstName,
                x.LastName,
                x.JobTitle,
                x.Salary
            })
                .Where(x => x.FirstName.StartsWith("Sa"))
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToList();

            var sb = new StringBuilder();
            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle} - (${employee.Salary:F2})");
            }
            return sb.ToString().TrimEnd();
        }
        public static string GetLatestProjects(SoftUniContext context)
        {
            var topProjects = context.Projects.Select(x => new
            {
                x.Name,
                x.Description,
                x.StartDate
            })
                .OrderBy(x => x.Name)
                .Take(10)
                .ToList();
            var sb = new StringBuilder();
            foreach (var item in topProjects)
            {
                string startDate = item.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                sb.AppendLine(item.Name);
                sb.AppendLine(item.Description);
                sb.AppendLine(startDate);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            var employees = context.Employees
                .Include(x => x.EmployeesProjects)
                .ThenInclude(x => x.Project)
                .Include(x => x.Manager)
                .Where(x => x.EmployeesProjects.Any(x => x.Project.StartDate.Year >= 2001 && x.Project.EndDate.Value.Year <= 2003))
                .Take(10)
                .ToList();

            var sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} - Manager: {employee.Manager.FirstName} {employee.Manager.LastName}");


                foreach (var item in employee.EmployeesProjects)
                {
                    string finish = item.Project.EndDate.HasValue ? item.Project.EndDate.Value.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture) : "not finished";
                    sb.AppendLine($"--{item.Project.Name} - {item.Project.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)} - {finish}");
                }
            }

            return sb.ToString().TrimEnd();
        }


        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            var departments = context.Departments
                .Where(x => x.Employees.Count > 5)
                .Select(d => new
                {
                    d.Name,
                    d.Manager.FirstName,
                    d.Manager.LastName,
                    d.Employees.Count,
                    d.DepartmentId,
                    Employeess = context.Employees.Select(e => new
                    {
                        e.FirstName,
                        e.LastName,
                        e.JobTitle,
                        e.DepartmentId
                    })
                .OrderBy(n => n.FirstName)
                .ThenBy(n => n.LastName)
                .ToList()
                })
                .OrderBy(x => x.Count)
                .ThenBy(n => n.Name)
                .ToList();

            var sb = new StringBuilder();
            foreach (var item in departments)
            {
                sb.AppendLine($"{item.Name} - {item.FirstName} {item.LastName}");
                foreach (var emp in item.Employeess)
                {
                    if (emp.DepartmentId == item.DepartmentId)
                    {
                        sb.AppendLine($"{emp.FirstName} {emp.LastName} - {emp.JobTitle}");
                    }
                }
            }
            string result = sb.ToString().TrimEnd();
            return result;
        }

        public static string IncreaseSalaries(SoftUniContext context)
        {
            var departaments = new string[] { "Marketing", "Engineering", "Tool Design", "Information Services" };
            var employees = context.Employees
                    .Where(d => departaments.Contains(d.Department.Name))
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName)
                    .ToList();
            var sb = new StringBuilder();
            foreach (var employee in employees)
            {
                employee.Salary *= 1.12m;
                sb.AppendLine($"{employee.FirstName} {employee.LastName} (${employee.Salary:f2})");
            }
            string result = sb.ToString().TrimEnd();
            return result;
        }

        public static string GetEmployee147(SoftUniContext context)
        {
            var employee = context.Employees.Select(x => new
            {
                x.EmployeeId,
                x.FirstName,
                x.LastName,
                Jobb = x.JobTitle,
                Proj = x.EmployeesProjects.OrderBy(x => x.Project.Name)
                .Select(p => new
                {
                    p.Project.Name
                })
            })
                .FirstOrDefault(e => e.EmployeeId == 147);

            var sb = new StringBuilder();
            sb.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.Jobb}");
            foreach (var item in employee.Proj)
            {
                sb.AppendLine($"{item.Name}");
            }
            string result = sb.ToString().TrimEnd();
            return result;
        }

        public static string GetAddressesByTown(SoftUniContext context)
        {
            var addresses = context.Addresses.Select(x => new
            {
                at = x.AddressText,
                tn = x.Town.Name,
                x.Employees.Count
            })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.tn)
                .ThenBy(x => x.at)
                .Take(10)
                .ToList();

            var sb = new StringBuilder();

            foreach (var addres in addresses)
            {
                sb.AppendLine($"{addres.at}, {addres.tn} - {addres.Count} employees");
            }
            var result = sb.ToString().TrimEnd();
            return result;
        }
        // TODO: Have to complete task 7 with the dates.
        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            var address = new Address
            {
                AddressText = "Vitoshka 15",
                TownId = 4
            };
            context.Addresses.Add(address);
            context.SaveChanges();

            var nakov = context.Employees.FirstOrDefault(x => x.LastName == "Nakov");
            nakov.AddressId = address.AddressId;
            context.SaveChanges();

            var addresses = context.Employees
                .OrderByDescending(x => x.AddressId)
                .Select(x => new
                {
                    x.Address.AddressText,
                    x.Address.AddressId
                })
                .Take(10)
                .ToList();

            var sb = new StringBuilder();
            foreach (var curaddress in addresses)
            {
                sb.AppendLine(curaddress.AddressText);
            }

            var result = sb.ToString().TrimEnd();
            return result;
        }

        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {

            var employees = context.Employees
                .Where(x => x.Department.Name == "Research and Development")
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    x.Salary,
                    x.Department.Name
                })
                .OrderBy(x => x.Salary)
                .ThenByDescending(x => x.FirstName);

            var sb = new StringBuilder();
            foreach (var emp in employees)
            {
                sb.AppendLine($"{emp.FirstName} {emp.LastName} from Research and Development - ${emp.Salary:f2}");
            }

            var result = sb.ToString().TrimEnd();
            return result;
        }

        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            var employees = context.Employees
                .Select(x => new { x.FirstName, x.Salary })
                .OrderBy(x => x.FirstName)
                .Where(x => x.Salary > 50000)
                .ToList();

            var sb = new StringBuilder();

            foreach (var emp in employees)
            {
                sb.AppendLine($"{emp.FirstName} - {emp.Salary:F2}");
            }

            var result = sb.ToString().TrimEnd();
            return result;
        }

        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            var employees = context.Employees
                .Select(x => new { x.EmployeeId, x.FirstName, x.LastName, x.MiddleName, x.JobTitle, x.Salary })
                .OrderBy(x => x.EmployeeId)
                .ToList();

            var sb = new StringBuilder();

            foreach (var emp in employees)
            {
                sb.AppendLine($"{emp.FirstName} {emp.LastName} {emp.MiddleName} {emp.JobTitle} {emp.Salary:F2}");
            }

            var result = sb.ToString().TrimEnd();
            return result;
        }
    }
}
