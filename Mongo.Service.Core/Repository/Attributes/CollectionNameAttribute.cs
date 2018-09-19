using System;

namespace Mongo.Service.Core.Repository.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CollectionNameAttribute : Attribute
    {
        public CollectionNameAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }
    }
}