// Parts of this file licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

using App.Boilerplate.Core.Routing;

namespace App.Boilerplate.Infrastructure.Routing
{
    internal class PageTreeModelService : IPageTreeModelService
    {
        public IDictionary<Type, Delegate> ActivatorsDictionary { get; set; } = new ConcurrentDictionary<Type, Delegate>();

        public object GetModel(Type modelType, IDictionary<string, object> parameters)
        {
            var constructors = modelType.GetConstructors();

            if (constructors.Length != 1)
            {
                throw new Exception($"Type '{modelType}' does not have exactly one constructor.");
            }

            var constructor = constructors[0];
            var constructorParameterInfos = constructor.GetParameters();

            var parametersValues = new object[constructorParameterInfos.Length];
            for (int i = 0; i < constructorParameterInfos.Length; i++)
            {
                var parameterInfo = constructorParameterInfos[i];

                if (!parameters.TryGetValue(parameterInfo.Name, out var parameter) && !parameterInfo.HasDefaultValue)
                {
                    throw new ArgumentException(
                        $"Parameter named '{parameterInfo.Name}' of type '{parameterInfo.ParameterType}' is not available.",
                        nameof(parameters)
                        );
                }

                if (parameter != null && !parameterInfo.ParameterType.IsInstanceOfType(parameter))
                {
                    throw new ArgumentException(
                        $"Parameter named '{parameterInfo.Name}' of type '{parameterInfo.ParameterType}' does not match the available parameter's type, '{parameter.GetType()}'.",
                        nameof(parameters)
                        );
                }

                parametersValues[i] = parameter;
            }

            if (!ActivatorsDictionary.TryGetValue(modelType, out var activator))
            {
                activator = GetActivator(modelType);

                ActivatorsDictionary.Add(modelType, activator);
            }

            return activator.DynamicInvoke(modelType, parametersValues);
        }

        private static Delegate GetActivator(Type modelType)
        {
            var modelTypeParameter = Expression.Parameter(typeof(Type), "modelType");
            var parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

            var constructor = modelType.GetConstructors()[0];
            var constructorParameterInfos = constructor.GetParameters();

            var constructorParameters = new List<Expression>();

            for (int i = 0; i < constructorParameterInfos.Length; i++)
            {
                var constructorParameterInfo = constructorParameterInfos[i];
                var constructorParameterValue = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));

                constructorParameters.Add(Expression.Convert(constructorParameterValue, constructorParameterInfo.ParameterType));
            }

            var activation = Expression.New(constructor, constructorParameters);

            var lambda = Expression.Lambda(
                activation,
                modelTypeParameter,
                parametersParameter
                );

            return lambda.Compile();
        }
    }
}