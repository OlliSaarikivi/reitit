using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Reitit
{
    [DataContract]
    public class ExtendedObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected PropertyChangedEventHandler PropertyChangedHandler
        {
            get
            {
                return PropertyChanged;
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event if needed.
        /// </summary>
        /// <remarks>If the propertyName parameter
        /// does not correspond to an existing property on the current class, an
        /// exception is thrown in DEBUG configuration only.</remarks>
        /// <param name="propertyName">(optional) The name of the property that
        /// changed.</param>
        [SuppressMessage(
            "Microsoft.Design",
            "CA1030:UseEventsWhereAppropriate",
            Justification = "This cannot be an event")]
        protected virtual void RaisePropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1030:UseEventsWhereAppropriate",
            Justification = "This cannot be an event")]
        [SuppressMessage(
            "Microsoft.Design",
            "CA1006:GenericMethodsShouldProvideTypeParameter",
            Justification = "This syntax is more convenient than other alternatives.")]
        protected virtual void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var propertyName = GetPropertyName(propertyExpression);
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "This syntax is more convenient than the alternatives."),
         SuppressMessage(
            "Microsoft.Design",
            "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This syntax is more convenient than the alternatives.")]
        protected static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }

            var body = propertyExpression.Body as MemberExpression;

            if (body == null)
            {
                throw new ArgumentException("Invalid argument", "propertyExpression");
            }

            var property = body.Member as PropertyInfo;

            if (property == null)
            {
                throw new ArgumentException("Argument is not a property", "propertyExpression");
            }

            return property.Name;
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This syntax is more convenient than the alternatives."),
         SuppressMessage(
            "Microsoft.Design",
            "CA1045:DoNotPassTypesByReference",
            MessageId = "1#",
            Justification = "This syntax is more convenient than the alternatives.")]
        protected bool Set<T>(
            Expression<Func<T>> propertyExpression,
            ref T field,
            T newValue)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            RaisePropertyChanged(propertyExpression);
            return true;
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1045:DoNotPassTypesByReference",
            MessageId = "1#",
            Justification = "This syntax is more convenient than the alternatives.")]
        protected bool Set<T>(
            string propertyName,
            ref T field,
            T newValue)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            RaisePropertyChanged(propertyName);
            return true;
        }

        public DerivedProperty<T> CreateDerivedProperty<T>(Expression<Func<T>> property, Expression<Func<T>> expression)
        {
            return DerivedProperty.Create(expression, () => RaisePropertyChanged(property));
        }

        public DerivedProperty<T> CreateDerivedProperty<T>(string property, Expression<Func<T>> expression)
        {
            return DerivedProperty.Create(expression, () => RaisePropertyChanged(property));
        }
    }
}
