using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace dp2Circulation
{
    public class ErrorInfoCollection
    {
    }

    [ComVisible(true), ClassInterface(ClassInterfaceType.None)]
    public class Reflector :
        System.Reflection.IReflect
    {
        object _target;

        protected Reflector() { }

        public Reflector(object target)
        {
            Debug.Assert(target != null);
            _target = target;
        }

        public object Target
        {
            get { return _target; }
        }

        #region IReflect

        public FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            return this._target.GetType().GetField(name, bindingAttr);
        }

        public FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            return this._target.GetType().GetFields(bindingAttr);
        }

        public MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
        {
            return this._target.GetType().GetMember(name, bindingAttr);
        }

        public MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            return this._target.GetType().GetMembers(bindingAttr);
        }

        public MethodInfo GetMethod(string name, BindingFlags bindingAttr)
        {
            return this._target.GetType().GetMethod(name, bindingAttr);
        }

        public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
        {
            return this._target.GetType().GetMethod(name, bindingAttr, binder, types, modifiers);
        }

        public MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            return this._target.GetType().GetMethods(bindingAttr);
        }

        public PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            return _target.GetType().GetProperties(bindingAttr);
        }

        public PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            return this._target.GetType().GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
        }

        public PropertyInfo GetProperty(string name, BindingFlags bindingAttr)
        {
            return this._target.GetType().GetProperty(name, bindingAttr);
        }

        public object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters)
        {
            if (target == this)
            {
                if (name.CompareTo("[DISPID=0]") == 0)
                {
                    if (invokeAttr.HasFlag(BindingFlags.InvokeMethod))
                        return this._target;
                    else if (invokeAttr.HasFlag(BindingFlags.GetProperty) && args.Length == 0)
                        return this._target.ToString();
                }
                else
                {
                    return this._target.GetType().InvokeMember(name, invokeAttr, binder, _target, args, modifiers, culture, namedParameters);
                }
            }
            throw new ArgumentException();
        }

        public Type UnderlyingSystemType
        {
            get { return this._target.GetType().UnderlyingSystemType; }
        }

        #endregion
    }
}
