using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assignment3BAD.Database;
using Assignment3BAD.Models;
using Microsoft.EntityFrameworkCore;


// Method to seed initial data into the database
namespace Assignment3BAD.Seeding
{
    public static class SeedDummyData
    {
        public async static Task seedDb(MyDBContext db)
        {

            // Seed Cooks Name: {Name},   PhoneNumber: {PhoneNumber},   PhysicalAddress: {PhysicalAddress}
            var cook1 = new Cook { Name = "Anna Kitchen", PhoneNumber = "+45 31234567", PhysicalAddress = "Nørrebrogade 20, 2000 København S"/*, CPR = "010191-6789"*/, PassedCourse = true };
            var cook2 = new Cook { Name = "Lars Kitchen", PhoneNumber = "42345678", PhysicalAddress = "Kubyvænget 22, 8000, Aarhus C", /*CPR = "080289-2345" ,*/ PassedCourse = false };
            var cook3 = new Cook { Name = "Noah Kitchen", PhoneNumber = "+45 71555080", PhysicalAddress = "Finlandsgade 17, 8200 Aarhus N", /*CPR = "030586-6572"*/ PassedCourse = true };
            var cook4 = new Cook { Name = "Helle Kitchen", PhoneNumber = "+45 12345678", PhysicalAddress = "Munkegade 118, 8200 Aarhus N", /*CPR = "060494-1234"*/ PassedCourse = false };
            if (!db.Cooks.Any())
            {
                db.Cooks.Add(cook1);
                db.Cooks.Add(cook2);
                db.Cooks.Add(cook3);
                db.Cooks.Add(cook4);
            }



            //await db.SaveChangesAsync();

            // Seed Dishes "Name: {Name}, Price: {Price}, StartTime: {StartTime},   EndTime: {EndTime}
            var dish1 = new Dish { Name = "Spaghetti Carbonara",Quantity=2, Price = 105m, StartTime = DateTime.ParseExact("28102024 11:30", "ddMMyyyy HH:mm", null), /*new TimeSpan(11, 30, 0),*/ EndTime = DateTime.ParseExact("28102024 12:00", "ddMMyyyy HH:mm", null) /*new TimeSpan(12, 00, 0),*/, Cook = cook1 };
            var dish2 = new Dish { Name = "Hawaiian Pizza", Quantity=3,Price = 85m, StartTime = DateTime.ParseExact("28102024 12:00", "ddMMyyyy HH:mm", null) /*new TimeSpan(12, 00, 0),*/, EndTime = DateTime.ParseExact("28102024 12:30", "ddMMyyyy HH:mm", null) /*new TimeSpan(12, 30, 0),*/, Cook = cook2 };
            var dish3 = new Dish { Name = "Grilled Chicken Sandwich",Quantity=5, Price = 45m, StartTime = DateTime.ParseExact("28102024 10:00", "ddMMyyyy HH:mm", null) /*new TimeSpan(10, 00, 0),*/, EndTime = DateTime.ParseExact("28102024 11:00", "ddMMyyyy HH:mm", null) /*new TimeSpan(11, 00, 0),*/, Cook = cook3 };
            var dish4 = new Dish { Name = "Sushi", Quantity=10,Price = 300m, StartTime = DateTime.ParseExact("28102024 09:25", "ddMMyyyy HH:mm", null) /*new TimeSpan(9, 25, 0),*/, EndTime = DateTime.ParseExact("28102024 10:00", "ddMMyyyy HH:mm", null) /*new TimeSpan(10, 00, 0),*/, Cook = cook4 };

            if (!db.Dishes.Any())
            {
                db.Dishes.Add(dish1);
                db.Dishes.Add(dish2);
                db.Dishes.Add(dish3);
                db.Dishes.Add(dish4);
                Console.WriteLine("Dishes added");

            }
            // await db.SaveChangesAsync();

            // Seed Customers Name: {Name},   PhoneNumber: {PhoneNumber},   PaymentOptions: {PaymentOptions}
            var customer1 = new Customer { Name = "Hafsa Mohamed", PhysicalAddress = "Elmevje 2, 2000 København", PhoneNumber = "+45 75678901", PaymentOptions = "Card" };
            var customer2 = new Customer { Name = "Avantika Dhanabal", PhysicalAddress = "Nørrebro 11, 8000 Aarhus C", PhoneNumber = "+45 86789012", PaymentOptions = "Mobile Pay" };
            var customer3 = new Customer { Name = "Peter Larsen", PhysicalAddress = "Finlandsgade 20, 8200 Aarhus N", PhoneNumber = "+45 67901234", PaymentOptions = "Card" };
            var customer4 = new Customer { Name = "Sophie Jensen", PhysicalAddress = "Kulbyvænget 10, 8200 AArhus N", PhoneNumber = "+45 78901234", PaymentOptions = "Mobile Pay" };

            if (!db.Customers.Any())
            {
                db.Customers.Add(customer1);
                db.Customers.Add(customer2);
                db.Customers.Add(customer3);
                db.Customers.Add(customer4);
                Console.WriteLine("Customers added");

            }
            // await db.SaveChangesAsync();



            // Seed Cyclists BikeType: {BikeType},   PhoneNumber: {PhoneNumber}, Month: {Month},  MonthlyHours: {MonthlyHours},  MonthlyEarning: {MonthlyEarning}
            var cyclist1 = new Cyclist { BikeType = "Electric", PhoneNumber = "+45 53456789" /*Month = "June", MonthlyHours = "45", MonthlyEarning = "4520"*/};
            var cyclist2 = new Cyclist { BikeType = "Hybrid", PhoneNumber = "+45 64567890" /*Month = "August", MonthlyHours = "150", MonthlyEarning = "3000" */};
            var cyclist3 = new Cyclist { BikeType = "Electric", PhoneNumber = "+45 75890123" /*Month = "July", MonthlyHours = "50", MonthlyEarning = "5019"*/};
            var cyclist4 = new Cyclist { BikeType = "Hybrid", PhoneNumber = "+45 86791234" /*Month = "September", MonthlyHours = "49", MonthlyEarning = "4910"*/};

            if (!db.Cyclists.Any(c => c.PhoneNumber == cyclist1.PhoneNumber))
            {
                db.Cyclists.Add(cyclist1);
            }

            if (!db.Cyclists.Any(c => c.PhoneNumber == cyclist2.PhoneNumber))
            {
                db.Cyclists.Add(cyclist2);
            }

            if (!db.Cyclists.Any(c => c.PhoneNumber == cyclist3.PhoneNumber))
            {
                db.Cyclists.Add(cyclist3);
            }

            if (!db.Cyclists.Any(c => c.PhoneNumber == cyclist4.PhoneNumber))
            {
                db.Cyclists.Add(cyclist4);
            }

            await db.SaveChangesAsync();
            Console.WriteLine("Cyclists added");


            // if (!db.Cyclists.Any())
            // {
            //     db.Cyclists.Add(cyclist1);
            //     db.Cyclists.Add(cyclist2);
            //     db.Cyclists.Add(cyclist3);
            //     db.Cyclists.Add(cyclist4);

            //     await db.SaveChangesAsync();
            //     Console.WriteLine("Cyclists added");

            // }
            // await db.SaveChangesAsync();


            // Seed Orders OrderTimer: {OrderTime}
            var order1 = new Order { OrderTime = DateTime.ParseExact("28102024 11:00", "ddMMyyyy HH:mm", null), /* new TimeSpan(11, 00, 0),*/ Dish = dish1, Customer = customer1 };
            var order2 = new Order { OrderTime = DateTime.ParseExact("28102024 11:25", "ddMMyyyy HH:mm", null), /*new TimeSpan(11, 25, 0),*/ Dish = dish2, Customer = customer2 };
            var order3 = new Order { OrderTime = DateTime.ParseExact("28102024 19:30", "ddMMyyyy HH:mm", null), /* new TimeSpan(19, 30, 0),*/ Dish = dish3, Customer = customer3 };
            var order4 = new Order { OrderTime = DateTime.ParseExact("28102024 09:00", "ddMMyyyy HH:mm", null), /*new TimeSpan(9, 00, 0),*/ Dish = dish4, Customer = customer4 };

            if (!db.Orders.Any())
            {
                db.Orders.Add(order1);
                db.Orders.Add(order2);
                db.Orders.Add(order3);
                db.Orders.Add(order4);
                Console.WriteLine("Orders added");

            }
            // await db.SaveChangesAsync();

            // seed DishOrder: Name, quantity, DishID, OrderID
            var dishorder1 = new DishOrder { Quantity = 3, Dish = dish1, Order = order1 };
            var dishorder2 = new DishOrder { Quantity = 2, Dish = dish2, Order = order1 };
            var dishorder3 = new DishOrder { Quantity = 1, Dish = dish3, Order = order2 };
            var dishorder4 = new DishOrder { Quantity = 4, Dish = dish1, Order = order2 };

            if (!db.DishOrders.Any())
            {
                db.DishOrders.Add(dishorder1);
                db.DishOrders.Add(dishorder2);
                db.DishOrders.Add(dishorder3);
                db.DishOrders.Add(dishorder4);
                Console.WriteLine("DishOrders added.");

            }

            // Seed CyclistStats for each cyclist
            var cyclistStats1 = new CyclistStats
            {
                Month = "January",
                MonthlyHours = "120",
                MonthlyEarning = "3600",
                Cyclist = cyclist1
            };

            var cyclistStats2 = new CyclistStats
            {
                Month = "February",
                MonthlyHours = "300",
                MonthlyEarning = "2000",
                Cyclist = cyclist1
            };

            var cyclistStats3 = new CyclistStats
            {
                Month = "March",
                MonthlyHours = "500",
                MonthlyEarning = "4000",
                Cyclist = cyclist2
            };

            var cyclistStats4 = new CyclistStats
            {
                Month = "January",
                MonthlyHours = "120",
                MonthlyEarning = "3600",
                Cyclist = cyclist2
            };

            var cyclistStats5 = new CyclistStats
            {
                Month = "February",
                MonthlyHours = "300",
                MonthlyEarning = "2000",
                Cyclist = cyclist3
            };

            var cyclistStats6 = new CyclistStats
            {
                Month = "March",
                MonthlyHours = "500",
                MonthlyEarning = "4000",
                Cyclist = cyclist3
            };

            var cyclistStats7 = new CyclistStats
            {
                Month = "January",
                MonthlyHours = "120",
                MonthlyEarning = "3600",
                Cyclist = cyclist4
            };

            var cyclistStats8 = new CyclistStats
            {
                Month = "February",
                MonthlyHours = "300",
                MonthlyEarning = "2000",
                Cyclist = cyclist4
            };

            if (!db.CyclistStats.Any())
            {
                db.CyclistStats.Add(cyclistStats1);
                db.CyclistStats.Add(cyclistStats2);
                db.CyclistStats.Add(cyclistStats3);
                db.CyclistStats.Add(cyclistStats4);
                db.CyclistStats.Add(cyclistStats5);
                db.CyclistStats.Add(cyclistStats6);
                db.CyclistStats.Add(cyclistStats7);
                db.CyclistStats.Add(cyclistStats8);
                Console.WriteLine("CyclistStats added");
            }

            await db.SaveChangesAsync(); // Ensure you save changes to the database

            // Seed Trips and their Stops
            var trip1 = new Trip
            {
                Cyclist = cyclist1,
                Order = order1,
                Stops = new List<TripStop>
        {
            new TripStop
            {
                Address = "68 Randersvej Aarhus N 8200",
                Time = DateTime.ParseExact("28102024 12:30", "ddMMyyyy HH:mm", null),
                StopType = "Delivery"
            },
            new TripStop
            {
                Address = "12 Ny Munkegade Aarhus C 8200",
                Time = DateTime.ParseExact("28102024 12:45", "ddMMyyyy HH:mm", null),
                StopType = "Pickup"
            },
            new TripStop
            {
                Address = "5 Viby Torv Aarhus V 8000",
                Time = DateTime.ParseExact("28102024 13:00", "ddMMyyyy HH:mm", null),
                StopType = "Delivery"
            }
        }
            };


            var trip2 = new Trip
            {
                Cyclist = cyclist2,
                Order = order2,
                Stops = new List<TripStop>
        {
            new TripStop
            {
                Address = "66 Bauvej Aarhus V 8361",
                Time = DateTime.ParseExact("28102024 12:55", "ddMMyyyy HH:mm", null),
                StopType = "Pickup"
            },
            new TripStop
            {
                Address = "25 Godsbanen Aarhus C 8000",
                Time = DateTime.ParseExact("28102024 13:15", "ddMMyyyy HH:mm", null),
                StopType = "Delivery"
            }
        }
            };

            var trip3 = new Trip
            {
                Cyclist = cyclist3,
                Order = order3,
                Stops = new List<TripStop>
        {
            new TripStop
            {
                Address = "1 Danske Slagterier Aarhus C 8000",
                Time = DateTime.ParseExact("28102024 12:00", "ddMMyyyy HH:mm", null),
                StopType = "Delivery"
            },
            new TripStop
            {
                Address = "56 Frederiks Allé Aarhus C 8000",
                Time = DateTime.ParseExact("28102024 12:15", "ddMMyyyy HH:mm", null),
                StopType = "Pickup"
            }
        }
            };

            var trip4 = new Trip
            {
                Cyclist = cyclist4,
                Order = order4,
                Stops = new List<TripStop>
        {
            new TripStop
            {
                Address = "9 Gendarmstien Aarhus N 8000",
                Time = DateTime.ParseExact("28102024 11:00", "ddMMyyyy HH:mm", null),
                StopType = "Pickup"
            },
            new TripStop
            {
                Address = "13 Østbanetorvet Aarhus N 8000",
                Time = DateTime.ParseExact("28102024 11:30", "ddMMyyyy HH:mm", null),
                StopType = "Delivery"
            }
        }
            };


            if (!db.Trips.Any())
            {
                db.Trips.Add(trip1);
                db.Trips.Add(trip2);
                db.Trips.Add(trip3);
                db.Trips.Add(trip4);
                Console.WriteLine("Trips and TripStops added");

            }


            // // Seed Trips Address: {Address}, DeliverType: {DeliveryType},   Time: {Time}
            // var trip1 = new Trip { Address = "68 Randersvej Aarhus N 8200", /*DeliveryType = "delivery"*/ Time = DateTime.ParseExact("28102024 12:30", "ddMMyyyy HH:mm", null) /*new TimeSpan (12, 30, 0),*/, Cyclist = cyclist1, Order = order1};
            // var trip2 = new Trip { Address = "66 Bauvej Aarhus V 8361", /*DeliveryType = "pickup"*/Time = DateTime.ParseExact("28102024 12:55", "ddMMyyyy HH:mm", null) /* new TimeSpan(12, 55, 0),*/, Cyclist = cyclist2,Order = order2 };
            // var trip3 = new Trip { Address = "1 Dansladge Aarhus C 8000", /*DeliveryType = "delivery"*/Time = DateTime.ParseExact("28102024 12:00", "ddMMyyyy HH:mm", null) /*new TimeSpan(12, 00, 0)*/, Cyclist = cyclist3, Order = order3 };
            // var trip4 = new Trip { Address = "9 Gendengade Aarhus N 8000", /*DeliveryType = "pickup"*/Time = DateTime.ParseExact("28102024 11:00", "ddMMyyyy HH:mm", null) /* new TimeSpan(11, 00, 0)*/, Cyclist = cyclist4, Order = order4 };

            // if (!db.Trips.Any())
            // {
            //     db.Trips.Add(trip1);
            //     db.Trips.Add(trip2);
            //     db.Trips.Add(trip3);
            //     db.Trips.Add(trip4);
            //     Console.WriteLine("Trips added");

            // }
            // // await db.SaveChangesAsync();

            // var delivery1 = new Delivery
            // {
            //     TripTypes = new List<string> { "Pickup", "Delivery", "Pickup" },
            //     Trip = trip1
            // };

            // var delivery2 = new Delivery
            // {
            //     TripTypes = new List<string> { "Pickup", "Delivery", "Pickup" },
            //     Trip = trip2
            // };

            // var delivery3 = new Delivery
            // {
            //     TripTypes = new List<string> { "Pickup", "Delivery", "Pickup" },
            //     Trip = trip3
            // };

            // var delivery4 = new Delivery
            // {
            //     TripTypes = new List<string> { "Pickup", "Delivery", "Pickup" },
            //     Trip = trip4
            // };

            // // Save to database if no DeliveryTypes exist
            // if (!db.Deliveries.Any()) 
            // {
            //     db.Deliveries.Add(delivery1);
            //     db.Deliveries.Add(delivery2);
            //     db.Deliveries.Add(delivery3);
            //     db.Deliveries.Add(delivery4);
            //      Console.WriteLine("DeliveryTypes added");
            // }

            //await db.SaveChangesAsync();






            //await db.SaveChangesAsync(); // Ensure you save changes to the database


            // Seed Ratings DeliveryRating: {DeliveryRating},   FoodRating: {FoodRating}
            var rating1 = new RatingSystem { DeliveryRating = 4, FoodRating = 3, Cook = cook1, Customer = customer1, Cyclist = cyclist1 };
            var rating2 = new RatingSystem { DeliveryRating = 3, FoodRating = 5, Cook = cook2, Customer = customer2, Cyclist = cyclist2 };
            var rating3 = new RatingSystem { DeliveryRating = 1, FoodRating = 1, Cook = cook3, Customer = customer3, Cyclist = cyclist3 };
            var rating4 = new RatingSystem { DeliveryRating = 5, FoodRating = 4, Cook = cook4, Customer = customer4, Cyclist = cyclist4 };

            if (!db.Ratings.Any())
            {
                db.Ratings.Add(rating1);
                db.Ratings.Add(rating2);
                db.Ratings.Add(rating3);
                db.Ratings.Add(rating4);
                Console.WriteLine("Rating added");

            }

            await db.SaveChangesAsync();


        }
    }
}