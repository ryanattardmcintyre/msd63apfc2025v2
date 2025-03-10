using Newtonsoft.Json;
using PFCWebApplication.Models;
using StackExchange.Redis;

namespace PFCWebApplication.Repositories
{
    //1. Redis cache library StackExchange.Redis
    //2. Newtonsoft.Json
    public class RedisRepository
    {
        IDatabase db;
        public RedisRepository(string endPoint, string user, string password) {
          
            ConfigurationOptions conf = new ConfigurationOptions
            {
                EndPoints = { endPoint },
                User = user,
                Password = password
            };

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(conf);
            db = redis.GetDatabase();
        }

        public void Add(Menu m)
        {
            var myList = GetAll(); //getting the list of menus already stored in cache
            myList.Add(m);
            var myMenus = JsonConvert.SerializeObject(myList);

            db.StringSet("menu", myMenus); //overwriting the menu key in the cache
        }
        public List<Menu> GetAll()
        {
            List<Menu> myList = new List<Menu>();
            var myMenus = db.StringGet("menu");
            if (myMenus.HasValue)
            {
                //deserialize means = convert from a string into a List<Menu>
                myList = JsonConvert.DeserializeObject<List<Menu>>(myMenus);
            }
            return myList.OrderBy(x=>x.Order).ToList();
        }
    }
}
