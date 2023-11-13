using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.Json;

namespace PatchPractice
{
    public static class Extension
    {
        #region ToDictionaryWithExpressionTree From ChatGPT
        public static Dictionary<string, object> ToDictionaryWithExpressionTree<T>(this T? obj) where T : class
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            // Get all properties of the class
            var properties = typeof(T).GetProperties();

            // Create parameter expression for the instance of the class
            ParameterExpression param = Expression.Parameter(typeof(T), "x");

            foreach (var property in properties)
            {
                // Create property access expression
                MemberExpression member = Expression.Property(param, property);

                // Create lambda expression to represent the property access
                Expression<Func<T, object>> lambda = Expression.Lambda<Func<T, object>>(Expression.Convert(member, typeof(object)), param);

                // Compile the lambda expression to a delegate
                Func<T, object> func = lambda.Compile();

                // Add property name and value to the dictionary
                dictionary.Add(property.Name, func(obj));
            }

            return dictionary;
        }
        #endregion ToDictionaryWithExpressionTree From ChatGPT


        #region ToDictionaryWithExpressionTree From Bing

        //public static Dictionary<string, object> ToDictionaryWithExpressionTree<T>(this T? obj) where T : class
        //{
        //    var dictionary = new Dictionary<string, object>();
        //    if (obj == null) { return dictionary; }

        //    Expression<Func<T, Dictionary<string, object>>> expr = ObjectToDictionaryCompiledLinq<T>();
        //    return expr.Compile()(obj);
        //}

        //private static Expression<Func<T, Dictionary<string, object>>> ObjectToDictionaryCompiledLinq<T>()
        //{
        //    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //    var propertyInfos = properties as PropertyInfo[] ?? properties.ToArray();

        //    var dictionary = Expression.Parameter(typeof(Dictionary<string, object>), "dictionary");
        //    var source = Expression.Parameter(typeof(T), "source");

        //    var body = new List<Expression> { Expression.Assign(dictionary, Expression.New(typeof(Dictionary<string, object>))) };

        //    body.AddRange(propertyInfos.Select(property => Expression.Call(dictionary, typeof(Dictionary<string, object>).GetMethod("Add"), Expression.Constant(property.Name), Expression.Convert(Expression.Property(source, property), typeof(object)))));

        //    body.Add(dictionary);

        //    return Expression.Lambda<Func<T, Dictionary<string, object>>>(Expression.Block(new[] { dictionary }, body), source);
        //}

        #endregion ToDictionaryWithExpressionTree From Bing

        public static Dictionary<string, object> ToDictionary<T>(this T? model)
        {
            var dictionary = new Dictionary<string, object>();
            if (model == null) { return dictionary; }

            var serializedModel = JsonSerializer.Serialize(model);
            return JsonSerializer.Deserialize<Dictionary<string, object>>(serializedModel) ?? dictionary;
        }

        public static List<Operation<T>> ToOperations<T>(this Dictionary<string, object> properties) where T : class
        {
            List<Operation<T>> operations = new List<Operation<T>>();
            if (properties == null || properties.Count == 0) { return operations; }

            foreach (KeyValuePair<string, object> item in properties)
            {
                if (item.Key == null || item.Value == null) { continue; }

                operations.Add(new() { op = OperationType.Replace.ToString(), path = $"/{item.Key}", value = item.Value.ToString() });
            }

            return operations;
        }

        public static JsonPatchDocument<T> ToJsonPatchDocument<T>(this List<Operation<T>> operations) where T : class
        {
            IContractResolver contractResolver = new DefaultContractResolver();
            return new JsonPatchDocument<T>(operations, contractResolver);
        }

        public static JsonPatchDocument<T> ToJsonPatchDocument<T>(this T? obj) where T : class
        {
            if (obj == null) { return new JsonPatchDocument<T>(); }
            JsonPatchDocument<T> jsonPatchDocument;

            #region Becnhmark

            Stopwatch sw1 = new();
            Stopwatch sw2 = new();

            sw1.Start();
            var jsonPatchDocument1 = obj.ToDictionary().ToOperations<T>().ToJsonPatchDocument();
            sw1.Stop();
            Console.WriteLine("sw1 Ticks: " + sw1.ElapsedTicks);
            Console.WriteLine("sw1 ms: " + sw1.ElapsedMilliseconds);
            Console.WriteLine();

            sw2.Start();
            var jsonPatchDocumentWithExpressionTree = obj.ToDictionaryWithExpressionTree().ToOperations<T>().ToJsonPatchDocument();
            sw2.Stop();
            Console.WriteLine("sw2 Ticks: " + sw2.ElapsedTicks);
            Console.WriteLine("sw2 ms: " + sw2.ElapsedMilliseconds);
            Console.WriteLine();

            #endregion Becnhmark


            jsonPatchDocument = jsonPatchDocument1;
            return jsonPatchDocument;
        }
    }
}
