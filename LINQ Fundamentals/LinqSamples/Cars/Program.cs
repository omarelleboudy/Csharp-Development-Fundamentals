using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Cars
{
    class Program
    {
        static void Main(string[] args)
        {

            // This line is only used in a demo, if data model changes, wipe and start clear. NEVER USED IN REAL WORK.
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<CarDb>());

            InsertData();
            QueryData();
            /*
             *          These are for the XML Demo. Their functions are down there.
                        CreateXml();
                        QueryXml();
            */
            /* Code for joins example    
             * var query =
                 from car in cars
                 join manufacturer in manufacturers
                     on new { car.Manufacturer, car.Year } 
                         equals
                         new {Manufacturer = manufacturer.Name,manufacturer.Year}
                 orderby car.Combined descending, car.Name ascending
                 select new
                 {
                     // Anonymous Type
                     manufacturer.Headquarters,
                     car.Name,
                     car.Combined
                 };
             // Extension Method 
             var query2 =
                 cars.Join(manufacturers,
                 c => new { c.Manufacturer, c.Year },
                 m => new { Manufacturer = m.Name, m.Year },
                 (c, m) => new
                 {
                     m.Headquarters,
                     c.Name,
                     c.Combined
                 })
                 .OrderByDescending(c => c.Combined)
                 .ThenBy(c => c.Name);  
            var result = cars.Any(c => c.Manufacturer == "Ford");
            foreach (var car in query2.Take(10))
            {
                Console.WriteLine($"{car.Headquarters} {car.Name} : {car.Combined}");
            }
            Console.WriteLine(result);
            ***********************************************************/
            /*   // Group Join and aggregation examples
               var query =
                   from car in cars
                   group car by car.Manufacturer into carGroup
                   select new
                   {
                       Name = carGroup.Key,
                       Max = carGroup.Max(c => c.Combined),
                       Min = carGroup.Min(c => c.Combined),
                       Avg = carGroup.Average(c => c.Combined)
                   } into result
                   orderby result.Max descending
                   select result;

               var query2 =
                   cars.GroupBy(c => c.Manufacturer)
                       .Select(g =>
                       {
                           var results = g.Aggregate(new CarStatistics(),
                               (acc, c) => acc.Accumulate(c),
                               acc => acc.Compute());
                           return new
                           {
                               Name = g.Key,
                               Avg = results.Avg,
                               Max = results.Max,
                               Min = results.Min
                           };
                       })
                       .OrderByDescending(r => r.Max);
               foreach (var result in query)
               {
                   Console.WriteLine($"{result.Name}");
                   Console.WriteLine($"\t Max: {result.Max}");
                   Console.WriteLine($"\t Min: {result.Min}");
                   Console.WriteLine($"\t Avg: {result.Avg}");
               } */
        }

        private static void QueryData()
        {
            var db = new CarDb();
            db.Database.Log = Console.WriteLine;
            var query = from car in db.Cars
                        orderby car.Combined descending, car.Name ascending
                        select car;

            var query2 = db.Cars.Where(c => c.Manufacturer == "BMW")
                .OrderByDescending(c => c.Combined)
                .ThenBy(c => c.Name)
                .Take(10)
                .ToList();

            var query3 =
                db.Cars.GroupBy(c => c.Manufacturer)
                       .Select(g => new
                       {
                           Name = g.Key,
                           Cars = g.OrderByDescending(c => c.Combined).Take(2)
                       });
            var query4 =
                from car in db.Cars
                group car by car.Manufacturer into manufacturer
                select new
                {
                    Name = manufacturer.Key,
                    Cars = (from car in manufacturer
                            orderby car.Combined descending
                            select car).Take(2)
                };

            foreach (var group in query4)
            {
                Console.WriteLine(group.Name);
                foreach (var car in group.Cars)
                {
                    Console.WriteLine($"\t{car.Name} : {car.Combined}");
                }
            }

        }

        private static void InsertData()
        {
            var cars = ProcessCars("fuel.csv");
            var db = new CarDb();
 
            if (!db.Cars.Any())
            {
                foreach (var car in cars)
                {
                    db.Cars.Add(car);
                }
                db.SaveChanges();
            }
        }

        private static void QueryXml()
        {

            var ns = (XNamespace)"http://pluralsight.com/cars/2020";
            var ex = (XNamespace)"http://pluralsight.com/cars/2020/ex";
            var document = XDocument.Load("fuel.xml");

            var query =
                from element in document.Element(ns + "Cars")?.Elements(ex + "Car")
                                                             ?? Enumerable.Empty<XElement>()
                where element.Attribute("Manufacturer")?.Value == "BMW"
                select element.Attribute("Name").Value;

            foreach (var name in query)
            {
                Console.WriteLine(name);
            }
        }

        private static void CreateXml()
        {
            var records = ProcessCars("fuel.csv");

            var ns = (XNamespace)"http://pluralsight.com/cars/2020";
            var ex = (XNamespace)"http://pluralsight.com/cars/2020/ex";
            var document = new XDocument();
            var cars = new XElement(ns + "Cars",

                from record in records
                select new XElement(ex + "Car",
                    new XAttribute("Name", record.Name),
                    new XAttribute("Combined", record.Combined),
                    new XAttribute("Manufacturer", record.Manufacturer))
                );
            cars.Add(new XAttribute(XNamespace.Xmlns + "ex", ex));
            document.Add(cars);
            document.Save("fuel.xml");
        }

        public class CarStatistics
        {
            public int Max { get; set; }
            public int Min { get; set; }
            public double Avg { get; set; }
            public double Total { get; set; }
            public double Count { get; set; }

            public CarStatistics()
            {
                Max = Int32.MinValue;
                Min = Int32.MaxValue;
            }

            public CarStatistics Accumulate(Car car)
            {

                Total += car.Combined;
                Count++;
                Max = Math.Max(Max, car.Combined);
                Min = Math.Max(Min, car.Combined);
                return this;
            }

            public CarStatistics Compute()
            {
                Avg = Total / Count;
                return this;
            }
        }

        private static List<Car> ProcessCars(string path)
        {
            var query =
                File.ReadAllLines(path)
                .Skip(1)
                .Where(l => l.Length > 1)
                .ToCar();

            //from line in File.ReadAllLines(path).Skip(1)
            //where line.Length > 1
            //select Car.ParseFromCsv(line);

            return query.ToList();
        }
        private static List<Manufacturer> ProcessManufacturers(string path)
        {
            var query =
                File.ReadAllLines(path)
                .Where(l => l.Length > 1)
                .Select(l =>
                {
                    var columns = l.Split(',');
                    return new Manufacturer
                    {
                        Name = columns[0],
                        Headquarters = columns[1],
                        Year = int.Parse(columns[2])
                    };
                });


            return query.ToList();
        }

    }

    public static class CarExtensions
    {
        public static IEnumerable<Car> ToCar(this IEnumerable<string> source)
        {
            foreach (var line in source)
            {
                var columns = line.Split(',');

                yield return new Car
                {
                    Year = int.Parse(columns[0]),
                    Manufacturer = columns[1],
                    Name = columns[2],
                    Displacement = double.Parse(columns[3]),
                    Cylinders = int.Parse(columns[4]),
                    City = int.Parse(columns[5]),
                    Highway = int.Parse(columns[6]),
                    Combined = int.Parse(columns[7])
                };
            }
        }
    }
}
