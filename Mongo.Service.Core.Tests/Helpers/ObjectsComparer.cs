using Newtonsoft.Json;

namespace Mongo.Service.Core.Tests.Helpers
{
    public static class ObjectsComparer
    {
        public static bool AreEqual(object obj1, object obj2)
        {
            var sObj1 = JsonConvert.SerializeObject(obj1);
            var sObj2 = JsonConvert.SerializeObject(obj2);
            return sObj1.Equals(sObj2);
        }
    }
}