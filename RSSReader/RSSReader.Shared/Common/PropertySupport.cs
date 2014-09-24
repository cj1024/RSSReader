using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RSSReader.Common
{
    public static class PropertySupport
    {
        /// <summary>
        /// Extracts the property name from a property expression.
        /// 
        /// </summary>
        /// <typeparam name="T">The object type containing the property specified in the expression.</typeparam><param name="propertyExpression">The property expression (e.g. p =&gt; p.PropertyName)</param>
        /// <returns>
        /// The name of the property.
        /// </returns>
        public static string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException("propertyExpression");
            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("NotMemberAccessExpression", "propertyExpression");
            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException("ExpressionNotProperty", "propertyExpression");
            }
            if (propertyInfo.GetMethod.IsStatic)
            {
                throw new ArgumentException("StaticExpression", "propertyExpression");
            }
            return memberExpression.Member.Name;
        }
    }

}
