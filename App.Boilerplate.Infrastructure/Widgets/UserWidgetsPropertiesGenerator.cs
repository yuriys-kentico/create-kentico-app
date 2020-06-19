using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using App.Boilerplate.Core.Widgets;
using App.Boilerplate.Infrastructure.Widgets;

using CMS;
using CMS.Core;

using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;

[assembly: RegisterImplementation(typeof(IUserWidgetsPropertiesGenerator), typeof(UserWidgetsPropertiesGenerator), Priority = RegistrationPriority.Default)]

namespace App.Boilerplate.Infrastructure.Widgets
{
    internal class UserWidgetsPropertiesGenerator : IUserWidgetsPropertiesGenerator
    {
        private readonly AssemblyName dynamicAssemblyName;
        private readonly ModuleBuilder dynamicModule;

        public UserWidgetsPropertiesGenerator()
        {
            dynamicAssemblyName = new AssemblyName("UserWidgets");

            dynamicModule = AppDomain.CurrentDomain
                .DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run)
                .DefineDynamicModule("UserWidgetsModule");
        }

        public Type GetPropertiesType(string identifier, IList<UserWidgetProperty> properties)
        {
            var dynamicType = dynamicModule.DefineType(identifier, TypeAttributes.Public);

            dynamicType.AddInterfaceImplementation(typeof(IWidgetProperties));

            foreach (var userWidgetProperty in properties)
            {
                var propertyName = userWidgetProperty.Name;
                var propertyType = Type.GetType(userWidgetProperty.TypeName, true, true);

                var field = dynamicType.DefineField($"{propertyName}Field", propertyType, FieldAttributes.Private);

                var getSetAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

                var property = dynamicType.DefineProperty(propertyName, PropertyAttributes.None, propertyType, null);

                var getProperty = dynamicType.DefineMethod($"get_{propertyName}", getSetAttributes, propertyType, Type.EmptyTypes);
                var getPropertyIlGenerator = getProperty.GetILGenerator();
                getPropertyIlGenerator.Emit(OpCodes.Ldarg_0);
                getPropertyIlGenerator.Emit(OpCodes.Ldfld, field);
                getPropertyIlGenerator.Emit(OpCodes.Ret);
                property.SetGetMethod(getProperty);

                var setProperty = dynamicType.DefineMethod($"set_{propertyName}", getSetAttributes, null, new Type[] { propertyType });
                var setPropertyIlGenerator = setProperty.GetILGenerator();
                setPropertyIlGenerator.Emit(OpCodes.Ldarg_0);
                setPropertyIlGenerator.Emit(OpCodes.Ldarg_1);
                setPropertyIlGenerator.Emit(OpCodes.Stfld, field);
                setPropertyIlGenerator.Emit(OpCodes.Ret);
                property.SetSetMethod(setProperty);

                var editingComponentAttributeConstructor = typeof(EditingComponentAttribute).GetConstructor(new Type[] { typeof(string) });

                var editingComponentAttributeProperties = new Dictionary<PropertyInfo, object>
                {
                    {  typeof(EditingComponentAttribute).GetProperty(nameof(UserWidgetProperty.Label)), userWidgetProperty.Label },
                    {  typeof(EditingComponentAttribute).GetProperty(nameof(UserWidgetProperty.DefaultValue)), userWidgetProperty.DefaultValue },
                    {  typeof(EditingComponentAttribute).GetProperty(nameof(UserWidgetProperty.ExplanationText)), userWidgetProperty.ExplanationText },
                    {  typeof(EditingComponentAttribute).GetProperty(nameof(UserWidgetProperty.Tooltip)), userWidgetProperty.Tooltip },
                    {  typeof(EditingComponentAttribute).GetProperty(nameof(UserWidgetProperty.Order)), userWidgetProperty.Order }
                };

                property.SetCustomAttribute(new CustomAttributeBuilder(
                    editingComponentAttributeConstructor,
                    new[] { userWidgetProperty.FormComponentIdentifier },
                    editingComponentAttributeProperties.Keys.ToArray(),
                    editingComponentAttributeProperties.Values.ToArray()
                ));
            }

            var generatedType = dynamicType.CreateType();

            return generatedType;
        }
    }
}