using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.RFID.UI
{
    public class ViewModel : CustomTypeDescriptor
    {
        private LogicChipItem _instance;
        private ICustomTypeDescriptor _originalDescriptor;
        public ViewModel(LogicChipItem instance,
            ICustomTypeDescriptor originalDescriptor)
            : base(originalDescriptor)
        {
            _instance = instance;
            _originalDescriptor = originalDescriptor;
            // PropertyAttributeReplacements = new Dictionary<Type, IList<Attribute>>();
        }

        public static ViewModel DressUp(LogicChipItem instance)
        {
            return new ViewModel(instance, TypeDescriptor.GetProvider(instance).GetTypeDescriptor(instance));
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            var properties = base.GetProperties(attributes).Cast<PropertyDescriptor>();

            var bettered = properties.Select(pd =>
            {
                // 找元素名
                if (Element.TryGetOidByName(pd.Name, out ElementOID oid) == false)
                    return pd;

                bool is_readonly = false;
                Element element = _instance.FindElement(oid);
                if (element != null && element.Locked)
                    is_readonly = true;

                // 从 .Name 找到 readonly 的应有值
                {
                    List<Attribute> attribs = new List<Attribute>();
                    foreach (Attribute attr in pd.Attributes)
                    {
                        if (attr is ReadOnlyAttribute)
                            attribs.Add(new ReadOnlyAttribute(is_readonly));
                        else
                            attribs.Add(attr);
                    }
                    return TypeDescriptor.CreateProperty(typeof(LogicChipItem), pd, attribs.ToArray());
                }
#if NO
                if (PropertyAttributeReplacements.ContainsKey(pd.PropertyType))
                {
                    return TypeDescriptor.CreateProperty(typeof(T), pd, PropertyAttributeReplacements[pd.PropertyType].ToArray());
                }
                else
                {
                    return pd;
                }
#endif
            });
            return new PropertyDescriptorCollection(bettered.ToArray());
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(null);
        }
    }

}
