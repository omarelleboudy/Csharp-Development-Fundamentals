using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Features.Linq;
namespace Features
{
    class Program
    {
        static void Main(string[] args)
        {
            Func<int, int> square = x => x * x;
            Func<int, int, int> add = (x, y) => x + y;
            Console.WriteLine(square(3));
            Action<int> write = x => Console.WriteLine(x);
            write(add(5, 5));
            IEnumerable<Employee> developers = new Employee[]
            {
                new Employee {Id = 1, Name = "Scott"},
                new Employee {Id = 2, Name = "Chris"}
            };

            IEnumerable<Employee> sales = new List<Employee>()
            {
               new Employee {Id = 3, Name = "Alex"}
            };
            var query = developers.Where(e => e.Name.Length == 5)
                                                .OrderBy(e => e.Name);
            var query2 = from developer in developers
                         where developer.Name.Length == 5
                         orderby developer.Name
                         select developer;
            foreach (var employee in query2)
            {
                Console.WriteLine(employee.Name);
            }

        }
    }
}
