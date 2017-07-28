using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace PowerBIExtractor
{
    public class JsonUtil
    {
        public static void RemoveJsonProperties(JToken token, string[] propertiesToRemove)
        {
            if (token.Type == JTokenType.Object)
            {
                foreach (JProperty property in token.Children<JProperty>().ToList())
                {
                    bool removed = false;

                    if (propertiesToRemove.Contains(property.Name))
                    {
                        property.Remove();
                        removed = true;
                    }

                    if (!removed)
                    {
                        RemoveJsonProperties(property.Value, propertiesToRemove);
                    }
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                foreach (JToken child in token.Children())
                {
                    RemoveJsonProperties(child, propertiesToRemove);
                }
            }
        }

        public static void ExpandJsonProperties(JToken token, string[] propertiesToExpand)
        {
            if (token.Type == JTokenType.Object)
            {
                foreach (JProperty property in token.Children<JProperty>().ToList())
                {
                    bool processed = false;

                    if (propertiesToExpand.Contains(property.Name))
                    {
                        string jsonStringProperty = property.Value.ToString();
                        if (jsonStringProperty.StartsWith("{"))
                        {
                            JToken expandedProperties = JObject.Parse(jsonStringProperty);
                            property.Value = expandedProperties;
                            processed = true;
                        }
                        else if (jsonStringProperty.StartsWith("["))
                        {
                            JToken expandedProperties = JArray.Parse(jsonStringProperty);
                            property.Value = expandedProperties;
                            processed = true;
                        }
                    }

                    if (!processed)
                    {
                        ExpandJsonProperties(property.Value, propertiesToExpand);
                    }
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                foreach (JToken child in token.Children())
                {
                    ExpandJsonProperties(child, propertiesToExpand);
                }
            }
        }

        public static void SortJsonProperties(JToken token, string[] propertiesToSortBy)
        {
            if (propertiesToSortBy == null) return;

            if (token.Type == JTokenType.Array)
            {
                var array = token as JArray;
               
                sortArray(array, propertiesToSortBy);
                
                foreach (JToken child in token.Children())
                {
                    SortJsonProperties(child, propertiesToSortBy);
                }
            }
            else if (token.Type == JTokenType.Object)
            {
                foreach (JProperty property in token.Children<JProperty>().ToList())
                {
                    SortJsonProperties(property.Value, propertiesToSortBy);
                }
            }
        }

        private static void sortArray(JArray array, string[] propertiesToSortBy)
        {
            var firstObject = array.First as JToken;

            //check if the first object has values to check
            if (!firstObject.HasValues) return;

            //check if first object is an object
            if (firstObject.Type != JTokenType.Object) return;

            //check that the object has all the sort parameters
            foreach (var propertyName in propertiesToSortBy)
            {
                if (firstObject[propertyName] == null) return;
            }

            IOrderedEnumerable<JToken> ordered = array.OrderBy(s => s[propertiesToSortBy[0]]);
            for (int i = 1; i < propertiesToSortBy.Length; i++)
            {
                string propertyName = propertiesToSortBy[i];
                ordered = ordered.ThenBy(s => s[propertyName]);
            }

            //update the order
            var newArray = new JArray(ordered);
            array.Clear();
            foreach (JToken item in newArray.Children())
            {
                array.Add(item);
            }

        }


        public static void CollapseJsonProperties(JToken token, string[] propertiesToCollapse)
        {
            if (token.Type == JTokenType.Object)
            {
                foreach (JProperty property in token.Children<JProperty>().ToList())
                {
                    bool processed = false;

                    if (propertiesToCollapse.Contains(property.Name))
                    {
                        string jsonStringProperty = property.Value.ToString();
                        if (property.Value.Type == JTokenType.Object || property.Value.Type == JTokenType.Array)
                        {
                            var jsonString = JsonConvert.SerializeObject(property.Value, Formatting.None);
                            property.Value = jsonString;
                            processed = true;
                        }
                    }

                    if (!processed)
                    {
                        CollapseJsonProperties(property.Value, propertiesToCollapse);
                    }
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                foreach (JToken child in token.Children())
                {
                    CollapseJsonProperties(child, propertiesToCollapse);
                }
            }
        }

        public static int CountProperties(JToken token, string[] propertiesToCount = null)
        {
            List<JToken> matchingProperties = new List<JToken>();
            resursiveGetProperties(token, propertiesToCount, matchingProperties);
            return matchingProperties.Count;
        }

        private static void resursiveGetProperties(JToken token, string[] propertiesToCount, List<JToken> matchingProperties)
        {
            if (token.Type == JTokenType.Array)
            {
                foreach (JToken child in token.Children())
                {
                    resursiveGetProperties(child, propertiesToCount, matchingProperties);
                }
            }
            else if (token.Type == JTokenType.Object)
            {
                foreach (JProperty property in token.Children<JProperty>().ToList())
                {
                    if (propertiesToCount == null)
                        matchingProperties.Add(property);
                    else if (propertiesToCount.Contains(property.Name))
                        matchingProperties.Add(property);

                    resursiveGetProperties(property.Value, propertiesToCount, matchingProperties);
                }
            }
        }

        public static JObject SortPropertiesAlphabetically(JObject original)
        {
            var result = new JObject();

            foreach (var property in original.Properties().ToList().OrderBy(p => p.Name))
            {
                var value = property.Value as JObject;

                if (value != null)
                {
                    value = SortPropertiesAlphabetically(value);
                    result.Add(property.Name, value);
                }
                else
                {
                    result.Add(property.Name, property.Value);
                }
            }

            return result;
        }
    
    }

    
}
