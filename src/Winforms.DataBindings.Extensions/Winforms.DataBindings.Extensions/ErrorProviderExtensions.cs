using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Winforms.DataBindings.Extensions
{
    public static class ErrorProviderExtensions
    {
        public static void RegisterFormBindings(this ErrorProvider errorProvider,Form form)
        {
            RegisterBindingSourceValidationsRecurvice(form, errorProvider);
        }

        private static void RegisterBindingSourceValidationsRecurvice(
            Control control, ErrorProvider provider)
        {
            foreach (Control childControl in control.Controls)
            {
                RegisterBindingSourceValidationsForControl(childControl, provider);

                RegisterBindingSourceValidationsRecurvice(childControl, provider);
            }
        }

        private static void RegisterBindingSourceValidationsForControl(
            Control control, ErrorProvider errorProvider)
        {
            AddDataAnnotationsValidations(control, errorProvider);
        }

        private static void AddMaximumStringLengthToDataViewBoundTextBox(Control control)
        {
            var textBox = control as TextBox;

            if (textBox == null || textBox.DataBindings.Count == 0)
            {
                return;
            }

            var maximumTextLength = (
                from dataBinding in textBox.DataBindings.Cast<Binding>()
                where StringComparer.OrdinalIgnoreCase.Equals(dataBinding.PropertyName, "Text")
                let bindingSource = (BindingSource)dataBinding.DataSource
                where bindingSource.SyncRoot is DataView
                let view = (DataView)bindingSource.SyncRoot
                let bindingField = dataBinding.BindingMemberInfo.BindingField
                let maxLength = view.Table.Columns[bindingField].MaxLength
                where maxLength > 0
                select maxLength)
                .SingleOrDefault();

            if (maximumTextLength > 0)
            {
                textBox.MaxLength = maximumTextLength;
            }
        }

        private static void AddDataAnnotationsValidations(Control control,
            ErrorProvider errorProvider)
        {
            foreach (Binding dataBinding in control.DataBindings)
            {
                var type = dataBinding.DataSource.GetType();
                var controlProperty = control.GetType().GetProperty(dataBinding.PropertyName);
                var boundPropertyName = dataBinding.BindingMemberInfo.BindingMember;

                RegisterValidator(control, controlProperty,
                    type, boundPropertyName,
                    () => dataBinding.DataSource, errorProvider);

            }
        }

        private static void SetMaximumTextLength(TextBox textBoxToValidate,
            Type modelType, string modelPropertyName)
        {
            var propertyChain = GetPropertyChain(modelType, modelPropertyName).ToArray();

            ApplyMaximumStringLength(textBoxToValidate, propertyChain.Last());
        }

        private static void ApplyMaximumStringLength(TextBox textBoxToValidate,
            PropertyInfo property)
        {
            var maximumLength = (
                from attribute in property.GetCustomAttributes(
                    typeof(StringLengthAttribute), true)
                    .OfType<StringLengthAttribute>()
                select attribute.MaximumLength)
                .FirstOrDefault();

            if (maximumLength > 0)
            {
                textBoxToValidate.MaxLength = maximumLength;
            }
        }

        private static Type GetEnumerableElementType(
            this BindingSource bindingSource)
        {
            return (
                from intf in bindingSource.DataSource.GetType()
                    .GetInterfaces()
                where intf.IsGenericType
                where intf.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                let type = intf.GetGenericArguments().Single()
                where type != typeof(object)
                select type)
                .SingleOrDefault();
        }

        public static void RegisterValidator(Control controlToValidate,
            PropertyInfo controlProperty,
            Type modelType, string modelPropertyName,
            Func<object> instanceSelector, ErrorProvider errorProvider)
        {
            controlToValidate.CausesValidation = true;

            var propertyChain = GetPropertyChain(modelType, modelPropertyName).ToArray();

            var targetProperty = propertyChain.Last();

            var validator = new ControlValidator
            {
                ControlToValidate = controlToValidate,
                ControlProperty = controlProperty,
                PropertyChain = propertyChain,
                InstanceSelector = instanceSelector,
                ErrorProvider = errorProvider,
                ValidationAttributes =
                    targetProperty.GetCustomAttributes<ValidationAttribute>().ToArray(),
                Converter = TypeDescriptor.GetConverter(targetProperty.PropertyType),
            };

            if (validator.ValidationAttributes.Any())
            {
                controlToValidate.CausesValidation = true;

                if (validator.Converter == null)
                {
                    throw GetTypeConverterMissingExcpetion(targetProperty);
                }

                controlToValidate.Validating += (s, e) =>
                {
                    e.Cancel = !validator.Validate();
                };
            }
        }

        private static Exception GetTypeConverterMissingExcpetion(
            PropertyInfo modelProperty)
        {
            return new InvalidOperationException(
                $"Property '{modelProperty.Name}' declared on type {modelProperty.DeclaringType} cannot be used for validation. " +
                $"There is no TypeConverter for type {modelProperty.PropertyType}.");
        }

        private static IEnumerable<PropertyInfo> GetPropertyChain(
            Type modelType, string modelPropertyName)
        {
            foreach (var propertyName in modelPropertyName.Split('.'))
            {
                var property = modelType.GetProperty(propertyName);

                if (property == null)
                {
                    throw new InvalidOperationException(
                        $"Property with name '{propertyName}' could not be found on type {modelType.FullName}.");
                }

                modelType = property.PropertyType;

                yield return property;
            }
        }

        private class ControlValidator
        {
            public PropertyInfo[] PropertyChain { get; set; }
            public ValidationAttribute[] ValidationAttributes { get; set; }
            public TypeConverter Converter { get; set; }
            public Func<object> InstanceSelector { get; set; }
            public ErrorProvider ErrorProvider { get; set; }
            public Control ControlToValidate { get; set; }
            public PropertyInfo ControlProperty { get; set; }

            public bool Validate
                ()
            {
                var pair = this.GetModelPropertyChain().Last();

                var value = this.GetValueToValidate();

                object convertedValue;

                if (!this.TryConvertValue(value, out convertedValue))
                {
                    this.ErrorProvider.SetError(this.ControlToValidate,
                        "Value is invalid.");
                    return false;
                }

                var errorMessage = this.GetValidationErrorOrNull(pair, convertedValue);

                this.ErrorProvider.SetError(this.ControlToValidate, errorMessage);

                return string.IsNullOrWhiteSpace(errorMessage);
            }

            private IEnumerable<ObjectPropertyPair> GetModelPropertyChain()
            {
                var model = this.InstanceSelector();

                foreach (var property in this.PropertyChain)
                {
                    yield return new ObjectPropertyPair(model, property);

                    model = model == null ? null : property.GetValue(model);
                }
            }

            private object GetValueToValidate()
            {
                return this.ControlProperty.GetValue(this.ControlToValidate);
            }

            private string GetValidationErrorOrNull(ObjectPropertyPair pair, object value)
            {
                var context = new ValidationContext(pair.Object) { MemberName = pair.Property.Name };

                try
                {
                    Validator.ValidateValue(value, context, this.ValidationAttributes);
                    return null;
                }
                catch (ValidationException ex)
                {
                    return ex.Message;
                }
            }

            private bool TryConvertValue(object rawValue, out object convertedValue)
            {
                if (rawValue != null &&
                    rawValue.GetType() == this.PropertyChain.Last().PropertyType)
                {
                    convertedValue = rawValue;
                    return true;
                }

                try
                {
                    convertedValue = this.Converter.ConvertFrom(rawValue);
                    return true;
                }
                catch (Exception)
                {
                    convertedValue = null;
                    return false;
                }
            }

            private class ObjectPropertyPair
            {
                public readonly object Object;
                public readonly PropertyInfo Property;

                public ObjectPropertyPair(object @object, PropertyInfo property)
                {
                    this.Object = @object;
                    this.Property = property;
                }
            }
        }
    }
}