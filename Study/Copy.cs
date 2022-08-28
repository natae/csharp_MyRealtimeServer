using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Study
{
    internal class Copy
    {
        public void Start()
        {
            var city1 = new City();

            // ShallowCopy with Substitution
            city1.Init();

            string a = null;
            var city2 = city1;
            city2.Name = "Busan";
            city2.Budget = 90;
            city2.CityInfo.CityId = 2;

            city1.Print();
            city2.Print();

            // ShallowCopy with MemberwiseClone
            city1.Init();

            var city3 = city1.ShallowCopyWithMemberwiseClone();
            city3.Name = "Jeju";
            city3.Budget = 75;
            city3.CityInfo.CityId = 3;

            city1.Print();
            city3.Print();

            // DeepCopy with Bson Serialization
            city1.Init();

            var city4 = city1.DeepCopyWithBson();
            city4.Name = "Anyang";
            city4.Budget = 60;
            city4.CityInfo.CityId = 4;

            city1.Print();
            city4.Print();
        }
    }

    public class City
    {
        public string Name;
        public int Budget;
        public CityInfo CityInfo;

        public void Init()
        {
            Name = "Seoul";
            Budget = 100;
            CityInfo = new CityInfo
            {
                CityId = 1,
            };
        }

        public City ShallowCopyWithMemberwiseClone()
        {
            return (City)MemberwiseClone();
        }

        public City DeepCopyWithBson()
        {
            var serializer = new JsonSerializer();

            var memoryStreamForWrite = new MemoryStream();
            using (var writer = new BsonDataWriter(memoryStreamForWrite))
            {
                serializer.Serialize(writer, this);
            }

            var memoryStreamForRead = new MemoryStream(memoryStreamForWrite.ToArray());
            using (var reader = new BsonDataReader(memoryStreamForRead))
            {
                return serializer.Deserialize<City>(reader);
            }
        }
        public void Print()
        {
            Console.WriteLine($"Name: {Name}");
            Console.WriteLine($"Budget: {Budget}");
            Console.WriteLine($"CityInfo.CityId: {CityInfo.CityId}");
            Console.WriteLine("---------------------------------");
        }
    }
    public class CityInfo
    {
        public int CityId;
    }
}
