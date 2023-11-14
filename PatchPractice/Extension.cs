using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.Json;

namespace PatchPractice
{
    public static class Extension
    {
        #region ToDictionary Using Expression Trees

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

        #endregion ToDictionary Using Expression Trees

        //public static Dictionary<string, object> ToDictionary<T>(this T? model)
        //{
        //    var dictionary = new Dictionary<string, object>();
        //    if (model == null) { return dictionary; }

        //    var serializedModel = JsonSerializer.Serialize(model);
        //    return JsonSerializer.Deserialize<Dictionary<string, object>>(serializedModel) ?? dictionary;
        //}

        public static Dictionary<string, object> ToDictionary<T>(this T? model)
        {
            var dictionary = new Dictionary<string, object>();
            if (model == null) { return dictionary; }

            var properties = model.GetType().GetProperties();

            foreach (var property in properties)
            {
                if (!property.IsDefined(typeof(PatchNotAllowedAttribute), true))
                {
                    var name = property.Name;
                    var value = property.GetValue(model);

                    dictionary.Add(name, value);
                }
            }

            return dictionary;
        }

        public static List<Operation<T>> ToOperations<T>(this IDictionary<string, object?> properties) where T : class
        {
            List<Operation<T>> operations = new List<Operation<T>>();
            if (properties == null || properties.Count == 0) { return operations; }

            foreach (KeyValuePair<string, object?> item in properties)
            {
                if (item.Key == null) { continue; }
                //if (item.Value == null) { continue; }

                operations.Add(new() { op = OperationType.Replace.ToString(), path = $"/{item.Key}", value = item.Value?.ToString() });
            }

            return operations;
        }

        public static JsonPatchDocument<T> ToJsonPatchDocument<T>(this List<Operation<T>> operations) where T : class
        {
            IContractResolver contractResolver = new DefaultContractResolver();
            return new JsonPatchDocument<T>(operations, contractResolver);
        }

        public static JsonPatchDocument<TResponse> ToJsonPatchDocument<TModel, TResponse>(this TModel? obj) where TResponse : class where TModel : class
        {
            if (obj == null) { return new JsonPatchDocument<TResponse>(); }

            #region Becnhmark

            //Stopwatch sw1 = new();
            //Stopwatch sw2 = new();

            //sw1.Start();
            //var jsonPatchDocument1 = obj.ToDictionary().ToOperations<TResponse>().ToJsonPatchDocument();
            //sw1.Stop();
            //Console.WriteLine("sw1 Ticks: " + sw1.ElapsedTicks);
            //Console.WriteLine("sw1 ms: " + sw1.ElapsedMilliseconds);
            //Console.WriteLine();

            //sw2.Start();
            //var jsonPatchDocumentWithExpressionTree = obj.ToDictionaryWithExpressionTree().ToOperations<TResponse>().ToJsonPatchDocument();
            //sw2.Stop();
            //Console.WriteLine("sw2 Ticks: " + sw2.ElapsedTicks);
            //Console.WriteLine("sw2 ms: " + sw2.ElapsedMilliseconds);
            //Console.WriteLine();

            #endregion Becnhmark

            JsonPatchDocument<TResponse> jsonPatchDocument = obj.ToDictionary().ToOperations<TResponse>().ToJsonPatchDocument();
            //var jsonPatchDocumentWithExpressionTree = obj.ToDictionaryWithExpressionTree().ToOperations<TResponse>().ToJsonPatchDocument();
            return jsonPatchDocument;
        }

        public static JsonPatchDocument<TModel> ToJsonPatchDocument<TModel>(this TModel? obj) where TModel : class
        {
            if (obj == null) { return new JsonPatchDocument<TModel>(); }
            JsonPatchDocument<TModel> jsonPatchDocument = obj.ToDictionary().ToOperations<TModel>().ToJsonPatchDocument();
            //var jsonPatchDocumentWithExpressionTree = obj.ToDictionaryWithExpressionTree().ToOperations<TModel>().ToJsonPatchDocument();
            return jsonPatchDocument;
        }

        public static JsonPatchDocument<TModel> ToJsonPatchDocument<TModel>(this IDictionary<string, object?>? obj) where TModel : class
        {
            if (obj == null) { return new JsonPatchDocument<TModel>(); }

            var properties = typeof(TModel).GetProperties();//.Where(x => obj.Any(y=>y.Key.ToLower() == x.Name.ToLower()));
            var dictionary = new Dictionary<string, object?>();

            foreach (var property in properties)
            {
                if (!obj.Any(x => x.Key.Equals(property.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                if (!(property.IsDefined(typeof(PatchNotAllowedAttribute), false)))
                {
                    var value = obj.FirstOrDefault(x => x.Key.Equals(property.Name, StringComparison.OrdinalIgnoreCase));
                    dictionary.Add(value.Key, value.Value);
                }
            }

            JsonPatchDocument<TModel> jsonPatchDocument = dictionary.ToOperations<TModel>().ToJsonPatchDocument();
            return jsonPatchDocument;
        }



        ////////////////

        private static Expression<Func<T, object?>> CreatePropertyExpression1<T>(string propertyName)
        {
            // Create parameter expression for the instance of the class
            ParameterExpression param = Expression.Parameter(typeof(T), "x");

            // Create property access expression
            MemberExpression member = Expression.Property(param, propertyName);

            // Create lambda expression to represent the property access
            Expression<Func<T, object?>> lambda = Expression.Lambda<Func<T, object?>>(Expression.Convert(member, typeof(object)), param);

            return lambda;
        }

        private static Expression<Func<T, object?>> CreatePropertyExpression2<T>(string propertyName)
        {
            // Create parameter expression for the instance of the class
            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

            // Create property access expression
            MemberExpression property = Expression.Property(parameter, propertyName);

            // Create lambda expression to represent the property access
            UnaryExpression conversion = Expression.Convert(property, typeof(object));
            Expression<Func<T, object?>> lambda = Expression.Lambda<Func<T, object?>>(conversion, parameter);

            return lambda;
        }

        public static Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> GenerateSetPropertyExpressionChain<TModel, TEntity>(TModel model) where TModel : PersonPatchVM where TEntity : Person
        {
            Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyExpressionChain = calls => calls;

            Type entityType = typeof(TEntity);
            Type modelType = typeof(TModel);

            //Dictionary<string, object> modelProperties = model.ToDictionary();

            //foreach (KeyValuePair<string, object?> item in modelProperties)
            //{
            //    if (item.Key == null) { continue; }
            //    //if (item.Value == null) { continue; }

            //    Func<TEntity, object?> propertyExpression = CreatePropertyExpression2<TEntity>(item.Key).Compile();

            //    setPropertyExpressionChain = AppendSetProperty(setPropertyExpressionChain, s => s.SetProperty(propertyExpression, item.Value.ToString()));
            //}


            //var modelProperties = modelType.GetProperties();
            //foreach (var prop in modelProperties)
            //{
            //    string propName = prop.Name;
            //    object? propValue = prop.GetValue(model);

            //    Expression<Func<TEntity, object?>> propertyExpression = CreatePropertyExpression1<TEntity>(propName);

            //    setPropertyExpressionChain = AppendSetProperty(setPropertyExpressionChain, s => s.SetProperty(propertyExpression.Compile(), propValue));
            //}


            if (model.Name is not null)
            {
                Expression<Func<Person, string>> lambdaExpression = CreatePropertyExpression<Person, string>("Name");
                setPropertyExpressionChain = AppendSetProperty(setPropertyExpressionChain, s => s.SetProperty(lambdaExpression.Compile(), model.Name));
            }

            if (model.Age is not null)
            {
                setPropertyExpressionChain = AppendSetProperty(setPropertyExpressionChain, s => s.SetProperty(e => e.Age, model.Age));
            }

            if (model.Gender is not null)
            {
                setPropertyExpressionChain = AppendSetProperty(setPropertyExpressionChain, s => s.SetProperty(e => e.Gender, model.Gender));
            }



            return setPropertyExpressionChain;
        }

        public static Expression<Func<TSource, TProperty>> CreatePropertyExpression<TSource, TProperty>(string propertyName)
        {
            // Create a parameter expression for the source object (e)
            ParameterExpression parameter = Expression.Parameter(typeof(TSource), "e");

            // Create a property access expression
            MemberExpression property = Expression.Property(parameter, propertyName);

            // Create a lambda expression to represent the property access
            Expression<Func<TSource, TProperty>> lambda = Expression.Lambda<Func<TSource, TProperty>>(property, parameter);
            return lambda;

            //// Compile the lambda expression to a delegate (Func<TSource, TProperty>)
            //Func<TSource, TProperty> propertyExpression = lambda.Compile();

            //return propertyExpression;
        }

        private static Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> AppendSetProperty<TEntity>(
            Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> left,
            Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> right
            )
        {
            var replace = new ReplacingExpressionVisitor(right.Parameters, new[] { left.Body });
            var combined = replace.Visit(right.Body);
            return Expression.Lambda<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>(combined, left.Parameters);
        }

    }
}
