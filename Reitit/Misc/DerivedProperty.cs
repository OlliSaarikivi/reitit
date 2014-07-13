using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Reitit
{
    internal class PropertyFinder<T> : ExpressionVisitor
    {
        private DerivedProperty<T> _property;

        public PropertyFinder(DerivedProperty<T> property)
        {
            _property = property;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member is PropertyInfo)
            {
                var notifier = Expression.Lambda<Func<object>>(node.Expression).Compile()() as INotifyPropertyChanged;
                if (notifier != null)
                {
                    _property.AddDependency(notifier, node.Member.Name);
                }
            }
            return base.VisitMember(node);
        }
    }

    public static class DerivedProperty
    {
        public static DerivedProperty<R> Create<R>(Expression<Func<R>> expression, Action notify)
        {
            var property = new DerivedProperty<R>(expression.Compile(), notify);
            var finder = new PropertyFinder<R>(property);
            finder.Visit(expression);
            return property;
        }
    }

    public class DerivedProperty<T>
    {
        private Func<T> _getter;
        private Action _notify;
        private Dictionary<INotifyPropertyChanged, HashSet<string>> _dependencies
            = new Dictionary<INotifyPropertyChanged, HashSet<string>>();

        public DerivedProperty(Func<T> getter, Action notify)
        {
            _getter = getter;
            _notify = notify;
        }

        public T Get()
        {
            return _getter();
        }

        public void AddDependency(INotifyPropertyChanged notifier, string propertyName)
        {
            HashSet<String> propertyNames;
            if (!_dependencies.TryGetValue(notifier, out propertyNames))
            {
                propertyNames = new HashSet<string>();
                _dependencies[notifier] = propertyNames;
                notifier.PropertyChanged += (s, e) =>
                {
                    if (string.IsNullOrEmpty(e.PropertyName) || propertyNames.Contains(e.PropertyName))
                    {
                        _notify();
                    }
                };
            }
            propertyNames.Add(propertyName);
        }

    }
}
