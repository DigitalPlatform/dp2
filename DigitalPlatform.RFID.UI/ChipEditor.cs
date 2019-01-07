using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace DigitalPlatform.RFID.UI
{
    public partial class ChipEditor : UserControl
    {
        LogicChipItem _chip = null;

        public event PropertyValueChangedEventHandler PropertyValueChanged = null;

        public LogicChipItem LogicChipItem
        {
            get
            {
                return _chip;
            }
            set
            {
                _chip = value;
                propertyGrid1.SelectedObject = value;
            }
        }

        public ChipEditor()
        {
            InitializeComponent();
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            this.PropertyValueChanged?.Invoke(s, e);
        }

#if NO
        public void SetContent(LogicChipItem chip)
        {
            _chip = chip;

            if (chip != null)
            {
                // item.PropertyChanged += Item_PropertyChanged;
                propertyGrid1.SelectedObject = chip;
            }
            else
                propertyGrid1.SelectedObject = null;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ChipItem item = sender as ChipItem;
            // TODO: 更新 LogicChip
            // 进一步反馈到调用 Dialog 的窗口，更新列表显示。比如添加星号表示修改过
        }
#endif

#if NO
        static void SetField(ChipItem item,
            LogicChip chip,
            string fieldName)
        {
            ElementOID oid = Element.GetOidByName(fieldName);
            Element element = chip.FindElement(oid);
            if (element != null)
            {
                var info = item.GetType().GetProperty(fieldName);
                info.SetValue(item, element.Text);

                item.SetLocked(fieldName, element == null ? false : element.Locked);
            }
        }

        public LogicChip GetContent()
        {
            ChipItem item = (ChipItem)propertyGrid1.SelectedObject;

            if (_chip == null)
                _chip = new LogicChip();

            if (item != null)
            {
                GetField(item, "PrimaryItemIdentifier");
                GetField(item, "OwnerInstitution");
                GetField(item, "SetInformation");
            }
            return _chip;
        }

        void GetField(ChipItem item,
            string fieldName)
        {
            ElementOID oid = Element.GetOidByName(fieldName);
            Element element = _chip.FindElement(oid);
            if (element != null && element.Locked)
                return;

            var info = item.GetType().GetProperty(fieldName);
            _chip.SetElement(oid, (string)info.GetValue(item));
        }

#endif
    }
}
