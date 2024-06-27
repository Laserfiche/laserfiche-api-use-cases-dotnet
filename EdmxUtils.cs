// Copyright Laserfiche.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Laserfiche.Repository.Api.Client.Sample.ServiceApp
{
    internal static class EdmxUtils
    {
        static public Dictionary<string, Entity> EdmxToEntityDictionary(XDocument edmx)
        {
            XElement schema = edmx.Descendants().Where(x => x.Name.LocalName == "Schema").FirstOrDefault();
            int v = 0;
            string z = v.GetType().ToString();
            XNamespace ns = schema.GetDefaultNamespace();
            Dictionary<string, Entity> entityTypes = schema.Descendants(ns + "EntityType")
                .Select(x => new Entity()
                {
                    name = x.Attribute("Name")?.Value,
                    key = x.Descendants(ns + "PropertyRef").FirstOrDefault()?.Attribute("Name")?.Value,
                    properties = x.Elements(ns + "Property").Select(y => new Property()
                    {
                        name = y.Attribute("Name")?.Value,
                        _type = Type.GetType("System." + ((string)y.Attribute("Type")).Split(new char[] { '.' }).Last()),
                        nullable = (y.Attribute("Nullable") == null) ? (Boolean?)null : ((string)y.Attribute("Nullable") == "false") ? false : true
                    }).ToList()
                })
                .ToDictionary(x => x.name, x => x);

            return entityTypes;
        }

    }
    public class Entity
    {
        public string name { get; set; }
        public string key { get; set; }
        public List<Property> properties { get; set; }
    }
    public class Property
    {
        public string name { get; set; }
        public Type _type { get; set; }
        public Boolean? nullable { get; set; }
    }

}
