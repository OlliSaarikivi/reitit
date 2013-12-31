using GalaSoft.MvvmLight;
using Oat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Reitit
{
    class ExtendedObservableObject : ObservableObject
    {
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
