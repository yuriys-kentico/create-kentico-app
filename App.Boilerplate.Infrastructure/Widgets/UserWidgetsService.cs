using System;
using System.Collections.Generic;
using System.Reflection;

using App.Boilerplate.Core.Widgets;
using App.Boilerplate.Infrastructure.Widgets;

using CMS;
using CMS.Core;

using Kentico.PageBuilder.Web.Mvc;

[assembly: RegisterImplementation(typeof(IUserWidgetsService), typeof(UserWidgetsService), Priority = RegistrationPriority.Default)]

namespace App.Boilerplate.Infrastructure.Widgets
{
    internal class UserWidgetsService : IUserWidgetsService
    {
        private readonly IUserWidgetsRepository userWidgetRepository;

        private Type widgetComponentDefinitionStore;
        private MethodInfo widgetComponentDefinitionStoreAddMethod;
        private object widgetComponentDefinitionStoreInstance;
        private object widgetComponentDefinitionStoreInstanceRegister;
        private Dictionary<string, WidgetDefinition> widgetComponentDefinitionStoreInstanceRegisterRegister;

        private readonly ConstructorInfo widgetDefinitionConstructor = typeof(WidgetDefinition)
            .GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(string), typeof(Type), typeof(string), typeof(string), typeof(string), typeof(Type), typeof(string) },
                null
            );

        private Type WidgetComponentDefinitionStore
        {
            get => widgetComponentDefinitionStore ??= typeof(ComponentDefinition).Assembly
                .GetType("Kentico.Content.Web.Mvc.ComponentDefinitionStore`1")
                .MakeGenericType(typeof(WidgetDefinition));
        }

        private MethodInfo WidgetComponentDefinitionStoreAddMethod
        {
            get => widgetComponentDefinitionStoreAddMethod ??= WidgetComponentDefinitionStore
                    .GetMethod("Add");
        }

        private Action<string, Type, string, string, string, Type, string> WidgetComponentDefinitionStoreAdd
            => (string identifier, Type controllerType, string name, string description, string icon, Type propertiesType, string customViewPath)
            => WidgetComponentDefinitionStoreAddMethod
                    .Invoke(
                        WidgetComponentDefinitionStoreInstance,
                        new[] {
                            widgetDefinitionConstructor.Invoke(
                                new object[] {
                                    identifier,
                                    controllerType,
                                    name,
                                    description,
                                    icon,
                                    propertiesType,
                                    customViewPath
                                }
                            )
                        }
                    );

        private object WidgetComponentDefinitionStoreInstance
        {
            get => widgetComponentDefinitionStoreInstance ??= WidgetComponentDefinitionStore
                .GetProperty("Instance")
                .GetValue(null);
        }

        private object WidgetComponentDefinitionStoreInstanceRegister
        {
            get => widgetComponentDefinitionStoreInstanceRegister ??= WidgetComponentDefinitionStore
                .GetField("components", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(widgetComponentDefinitionStoreInstance);
        }

        private Dictionary<string, WidgetDefinition> WidgetComponentDefinitionStoreInstanceRegisterRegister
        {
            get => widgetComponentDefinitionStoreInstanceRegisterRegister ??= WidgetComponentDefinitionStoreInstanceRegister
                .GetType()
                .GetField("register", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(widgetComponentDefinitionStoreInstanceRegister)
                as Dictionary<string, WidgetDefinition>;
        }

        public UserWidgetsService(IUserWidgetsRepository userWidgetRepository)
        {
            this.userWidgetRepository = userWidgetRepository;
        }

        public void RegisterAll()
        {
            foreach (var userWidgetEntry in userWidgetRepository.UserWidgets)
            {
                var userWidget = userWidgetEntry.Value;

                WidgetComponentDefinitionStoreAdd(
                    userWidgetEntry.Key,
                    null,
                    userWidget.Name,
                    userWidget.Description,
                    userWidget.Icon,
                    userWidget.Type,
                    null
                    );
            }
        }

        public void Add(string identifier)
        {
            if (!userWidgetRepository.UserWidgets.TryGetValue(identifier, out var userWidget))
            {
                throw new ArgumentException("Identifier not found.", nameof(identifier));
            }

            WidgetComponentDefinitionStoreAdd(
                identifier,
                null,
                userWidget.Name,
                userWidget.Description,
                userWidget.Icon,
                userWidget.Type,
                null
                );
        }

        public void Remove(string identifier)
        {
            WidgetComponentDefinitionStoreInstanceRegisterRegister.Remove(identifier);
        }
    }
}