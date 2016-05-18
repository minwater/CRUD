using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRUD;

namespace UsingExample
{
    class Program
    {
        static void Main(string[] args)
        {
            VehicleClass vehicle = new VehicleClass();
            vehicle.name = "Porsche";
            vehicle.num = 123;
            vehicle.owner = "Rob";
            EngineClass engine = new EngineClass();
            engine.manufacturer = "Audi";
            engine.uniqNum = "sdfsdf1213";
            vehicle.engine = engine;

            Storage storage = new Storage(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "temp"));
            Console.WriteLine("Нажмите цифру 1, чтобы создать хранилище. Чтобы использовать созданное, нажмите любую другую кнопку");
            if (Console.ReadLine().Equals("1"))
            {
                Entity engineEntity2 = storage.CreateEntity("engine", new List<string>() { "manufacturer", "uniqNum" }, new Dictionary<string, Entity>());
                Entity vehicleEntity2 = storage.CreateEntity("vehicle", new List<string>() { "name", "num", "owner" }, new Dictionary<string, Entity>() { { "engine", engineEntity2 } });
                EntityRecord record = vehicleEntity2.GetRecord(vehicle);
                storage.SaveRecord(record);
            }

            Entity engineEntity = storage.GetEntity("engine");
            Entity vehicleEntity = storage.GetEntity("vehicle");

            EngineClass e2 = new EngineClass();
                        VehicleClass v2 = new VehicleClass();
            v2.engine = e2;
            vehicleEntity.SetFields(storage.Find(vehicleEntity, "name", "Porsche"), v2);

            VehicleClass v3 = new VehicleClass();
            EngineClass e3 = new EngineClass();
            v3.engine = e3;
            vehicleEntity.SetFields(storage.Find(vehicleEntity, "engine.manufacturer", "Audi"), v3);

            VehicleClass v4 = new VehicleClass();
            EngineClass e4 = new EngineClass();
            v4.engine = e4;
            vehicleEntity.SetFields(storage.Select(vehicleEntity, "engine.manufacturer", (string s) => { return s.Equals("Audi"); }).First(), v4);



            Console.WriteLine(v2.name + " " + v2.num + " " + v2.owner + " " + v2.engine.manufacturer + " " + v2.engine.uniqNum);
            Console.WriteLine(v3.name + " " + v3.num + " " + v3.owner + " " + v3.engine.manufacturer + " " + v3.engine.uniqNum);
            Console.WriteLine(v4.name + " " + v4.num + " " + v4.owner + " " + v4.engine.manufacturer + " " + v2.engine.uniqNum);
        }
    }
}
