using System;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace Winforms.DataBindings.Extensions
{
    public static class ControlExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TControlType"></typeparam>
        /// <typeparam name="TDataSourceItem"></typeparam>
        /// <param name="control">The control you are binding</param>
        /// <param name="controlProperty">The property on the control you are binding to</param>
        /// <param name="dataSource">The obect that contains the data</param>
        /// <param name="dataSourceProperty">The property on the object you are binding to</param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static Binding Bind<TControlType, TDataSourceItem>(this IBindableComponent control,
            Expression<Func<TControlType, object>> controlProperty, object dataSource,
            Expression<Func<TDataSourceItem, object>> dataSourceProperty, DataSourceUpdateMode mode)
        {
            return control.DataBindings.Add(PropertyName.For(controlProperty), dataSource,
                PropertyName.For(dataSourceProperty), true, mode, string.Empty);
        }

        public static Binding Bind<TControlType, TDataSourceItem>(this Control control,
            Expression<Func<TControlType, object>> controlProperty, object dataSource,
            Expression<Func<TDataSourceItem, object>> dataSourceProperty)
        {
            return control.DataBindings.Add(PropertyName.For(controlProperty), dataSource,
                PropertyName.For(dataSourceProperty));
        }

        /// <summary>Databinding with strongly typed object names</summary>
        /// <param name="control">The Control you are binding to</param>
        /// <param name="controlProperty">The property on the control you are binding to</param>
        /// <param name="dataSource">The object you are binding to</param>
        /// <param name="dataSourceProperty">The property on the object you are binding to</param>
        public static Binding Bind<TDataSourceItem>(this Control control, Expression<Func<Control, object>> controlProperty,
            object dataSource, Expression<Func<TDataSourceItem, object>> dataSourceProperty)
        {
            return control.DataBindings.Add(PropertyName.For(controlProperty), dataSource,
                PropertyName.For(dataSourceProperty));
        }

        public static Binding Bind<TDataSourceItem>(this Control control, Expression<Func<Control, object>> controlProperty,
            object dataSource, Expression<Func<TDataSourceItem, object>> dataSourceProperty, bool formattingEnabled = false)
        {
            return control.DataBindings.Add(PropertyName.For(controlProperty), dataSource,
                PropertyName.For(dataSourceProperty), formattingEnabled);
        }

        public static Binding Bind<TDataSourceItem>(this Control control, Expression<Func<Control, object>> controlProperty,
            object dataSource, Expression<Func<TDataSourceItem, object>> dataSourceProperty, bool formattingEnabled,
            DataSourceUpdateMode updateMode)
        {
            var exists = control.DataBindings[PropertyName.For(controlProperty)];
            if (exists == null)
            {
                return control.DataBindings.Add(PropertyName.For(controlProperty), dataSource,
                    PropertyName.For(dataSourceProperty), formattingEnabled, updateMode);
            }

            return exists;
        }

        public static Binding Bind<TDataSourceItem>(this Control control, Expression<Func<Control, object>> controlProperty,
            object dataSource, Expression<Func<TDataSourceItem, object>> dataSourceProperty, bool formattingEnabled,
            DataSourceUpdateMode updateMode, object nullValue)
        {
            return control.DataBindings.Add(PropertyName.For(controlProperty), dataSource,
                PropertyName.For(dataSourceProperty), formattingEnabled, updateMode, nullValue);
        }

        public static Binding Bind<TDataSourceItem>(this Control control, Expression<Func<Control, object>> controlProperty,
            object dataSource, Expression<Func<TDataSourceItem, object>> dataSourceProperty, bool formattingEnabled,
            DataSourceUpdateMode updateMode, object nullValue, string formatString)
        {
            return control.DataBindings.Add(PropertyName.For(controlProperty), dataSource,
                PropertyName.For(dataSourceProperty), formattingEnabled, updateMode, nullValue, formatString);
        }

        public static Binding Bind<TDataSourceItem>(this Control control, Expression<Func<Control, object>> controlProperty,
            object dataSource, Expression<Func<TDataSourceItem, object>> dataSourceProperty, bool formattingEnabled,
            DataSourceUpdateMode updateMode, object nullValue, string formatString, IFormatProvider formatInfo)
        {
            return control.DataBindings.Add(PropertyName.For(controlProperty), dataSource,
                PropertyName.For(dataSourceProperty), formattingEnabled, updateMode, nullValue, formatString, formatInfo);
        }

        public static class PropertyName
        {
            public static string For<T>(Expression<Func<T, object>> property)
            {
                var member = property.Body as MemberExpression;
                if (null == member)
                {
                    var unary = property.Body as UnaryExpression;
                    if (null != unary) member = unary.Operand as MemberExpression;
                }
                return member?.Member.Name ?? string.Empty;
            }
        }
    }
}

