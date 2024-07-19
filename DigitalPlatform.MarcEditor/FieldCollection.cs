using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

using DigitalPlatform.Text;

namespace DigitalPlatform.Marc
{
    // 
    /// <summary>
    /// �ֶμ���
    /// </summary>
    public class FieldCollection : List<Field>
    {
        /// <summary>
        /// �������ļ�¼����
        /// </summary>
        internal Record record = null;

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="comparer">����ӿ�</param>
        public void Sort(IComparer comparer)
        {

        }

        // ժҪ:
        //     ��ȡ������ָ����������Ԫ�ء�
        //
        // ����:
        //   index:
        //     Ҫ��û����õ�Ԫ�ش��㿪ʼ��������
        //
        // ���ؽ��:
        //     ָ����������Ԫ�ء�
        //
        // �쳣:
        //   System.ArgumentOutOfRangeException:
        //     index С�� 0��- �� -index ���ڻ���� System.Collections.Generic.List<T>.Count��
        /// <summary>
        /// ��ȡ������ָ���������� Field ����
        /// </summary>
        /// <param name="nIndex">Ҫ��û����õ�Ԫ�ش��㿪ʼ��������</param>
        /// <returns>ָ���������� Field ����</returns>
        public new Field this[int nIndex]
        {
            get
            {
                if (nIndex < 0 || nIndex >= this.Count)
                {
                    Debug.Assert(false, "�±�Խ��");
                    throw new Exception("�±�Խ�硣");
                }
                return (Field)base[nIndex];
            }
            set
            {
                base[nIndex] = value;
            }
        }

        /// <summary>
        /// ��ȡָ���ֶ��������ɸ��ֶ��е�ĳһ��
        /// </summary>
        /// <param name="strFieldName">�ֶ���</param>
        /// <param name="nIndex">�����ֶ����������ֶ��еĵڼ���</param>
        /// <returns>Field ����</returns>
        public Field this[string strFieldName,
            int nIndex]
        {
            get
            {
                return this.GetOneField(strFieldName, nIndex);
            }
        }

        /// <summary>
        /// ��ȡָ���ֶ��������ɸ��ֶ��е�ĳһ��
        /// </summary>
        /// <param name="strFieldName">�ֶ���</param>
        /// <param name="nIndex">�����ֶ����������ֶ��еĵڼ���</param>
        /// <param name="strIndicatorMatch">�ֶ�ָʾ��ɸѡ������ȱʡΪ "**"</param>
        /// <returns>Field ����</returns>
        public Field GetOneField(string strFieldName,
            int nIndex,
            string strIndicatorMatch = "**")
        {
            FieldCollection fields = this.GetFields(strFieldName, strIndicatorMatch);
            if (nIndex < 0 || nIndex >= fields.Count)
                return null;

            return fields[nIndex];
        }

        /// <summary>
        /// ��÷������������ɸ��ֶζ���
        /// </summary>
        /// <param name="strFieldName">�ֶ���</param>
        /// <param name="strIndicatorMatch">�ֶ�ָʾ��ɸѡ������ȱʡΪ "**"</param>
        /// <returns>�ֶζ��󼯺�</returns>
        public FieldCollection GetFields(string strFieldName,
            string strIndicatorMatch = "**")
        {
            FieldCollection fields = new FieldCollection();
            foreach (Field field in this)
            {
                if (field.m_strName == strFieldName)
                {
                    if (strIndicatorMatch != "**"
                        && string.IsNullOrEmpty(field.m_strIndicator) == false
                        && MarcUtil.MatchIndicator(strIndicatorMatch, field.m_strIndicator) == false)
                        continue;
                    // ����Ҫ�ر�ע��
                    fields.Add(field);
                }
            }
            return fields;
        }

        // ȡָ�����Ƶ�һ���ֶεĵ�һ�����ֶ�
        /// <summary>
        /// ȡָ���ֶ����ĵ�һ���ֶεĵ�һ�����ֶ�
        /// </summary>
        /// <param name="strFieldName">�ֶ���</param>
        /// <param name="strSubfieldName">���ֶ���</param>
        /// <returns>���ֶ�ֵ</returns>
        public string GetFirstSubfield(string strFieldName,
            string strSubfieldName)
        {
            Field field = this.GetOneField(strFieldName, 0);
            if (field == null)
                return "";
            Subfield subfield = field.Subfields.GetSubfield(strSubfieldName, 0);
            if (subfield == null)
                return "";

            return subfield.Value;
        }

        // 2011/8/9
        // ȡָ�����Ƶ�һ���ֶεĵ�һ�����ֶΡ����������ֶε�ָʾ������ɸѡ
        /// <summary>
        /// ȡָ�����Ƶ�һ���ֶεĵ�һ�����ֶΡ����������ֶε�ָʾ������ɸѡ
        /// </summary>
        /// <param name="strFieldName">�ֶ���</param>
        /// <param name="strSubfieldName">���ֶ���</param>
        /// <param name="strIndicatorMatch">�ֶ�ָʾ��ɸѡ������ȱʡΪ "**"</param>
        /// <returns>���ֶ�ֵ</returns>
        public string GetFirstSubfield(string strFieldName,
            string strSubfieldName,
            string strIndicatorMatch)
        {
            Field field = this.GetOneField(strFieldName, 0);
            if (field == null)
                return "";
            Subfield subfield = field.Subfields.GetSubfield(strSubfieldName, 0);
            if (subfield == null)
                return "";

            return subfield.Value;
        }

        // 2011/8/10
        // ȡָ�������ֶε����ֶΡ����������ֶε�ָʾ������ɸѡ
        // parameters:
        //      strFieldName    3�ַ����ֶ���
        //      strSubfieldName ����Ϊһ�����߶���ַ���ÿ���ַ�����һ�����ֶ���
        /// <summary>
        /// ȡָ�������ֶε����ֶΡ����������ֶε�ָʾ������ɸѡ
        /// </summary>
        /// <param name="strFieldName">�ֶ���</param>
        /// <param name="strSubfieldName">���ֶ���������Ϊһ�����߶���ַ���ÿ���ַ�����һ�����ֶ���</param>
        /// <param name="strIndicatorMatch">�ֶ�ָʾ��ɸѡ������ȱʡΪ "**"</param>
        /// <returns>���ֶ�ֵ����</returns>
        public List<string> GetSubfields(string strFieldName,
            string strSubfieldName,
            string strIndicatorMatch)
        {
            List<string> results = new List<string>();
            FieldCollection fields = this.GetFields(strFieldName, strIndicatorMatch);
            foreach (Field field in fields)
            {
                foreach (Subfield subfield in field.Subfields)
                {
                    if (strSubfieldName.IndexOf(subfield.Name) != -1)
                        results.Add(subfield.Value);
                }
            }
            return results;
        }

        public Field SetFirstSubfield(string strFieldName,
    string strSubfieldName,
    string strSubfieldValue)
        {
            return SetFirstSubfield(strFieldName,
            strSubfieldName,
            strSubfieldValue,
            out _);
        }

        /// <summary>
        /// ����ָ���ֶ����ĵ�һ���ֶ��ڵ�ָ�����ֶ����ĵ�һ�����ֶε�ֵ��
        /// ����������������ֶκ����ֶΣ���εڴ���
        /// </summary>
        /// <param name="strFieldName">�ֶ���</param>
        /// <param name="strSubfieldName">���ֶ���</param>
        /// <param name="strSubfieldValue">���ֶ�ֵ</param>
        /// <param name="old_field"></param>
        public Field SetFirstSubfield(string strFieldName,
            string strSubfieldName,
            string strSubfieldValue,
            out Field old_field)
        {
            old_field = null;
            Field field = this.GetOneField(strFieldName, 0);
            if (field == null)
            {
                field = this.Add(strFieldName,
                    "  ",
                    "",
                    true);
            }
            else
            {
                old_field = field.Clone();
            }

            if (field == null)
                throw new Exception("�����ܵ����");

            Subfield subfield = field.Subfields.GetSubfield(strSubfieldName, 0);
            if (subfield != null)
                subfield.Value = strSubfieldValue;
            else
            {
                subfield = new Subfield();
                subfield.m_strName = strSubfieldName;
                subfield.m_strValue = strSubfieldValue;
                field.Subfields.Add(subfield, true);
            }

            return field;
        }

        internal MarcEditor MarcEditor
        {
            get
            {
                return this.record.marcEditor;
            }
        }

        //--------------------------׷���ֶ�-------------------

        // ׷��һ�����ֶΣ�ֻ���ڲ�ʹ�ã�
        // ��Ϊ�ú���ֻ�����ڴ���󣬲��漰���������
        // ������ʷ: ��
        // parameters:
        //		strName	���ֶ�����
        //		strIndicator	���ֶ�ָʾ��
        //		strValue	���ֶ�ֵ ���ֶ�ָʾ���Ѿ�ת��Ϊ�ڲ���̬
        //		bFireTextChanged	�Ƿ񴥷�TextChanged�¼�
        //		bInOrder	�Ƿ���ӵ�ָ��λ�� true�ӵ�ָ��λ�ã�false�ӵ�ĩβ
        // return:
        //		void
        internal Field AddInternal(string strName,
            string strIndicator,
            string strValue,
            bool bFireTextChanged,
            bool bInOrder,
            out int nOutputPosition)
        {
            nOutputPosition = -1;

#if DEBUG
            if (strValue.IndexOf((char)31) != -1)
                Debug.Assert(false, "AddInternal()������strValue�����в�Ӧ����ASCII 31");
#endif

            string strCaption = this.record.marcEditor.GetLabel(strName);

            Field field = new Field(this);
            field.m_strNameCaption = strCaption;
            field.m_strName = strName;
            field.m_strIndicator = strIndicator;
            field.m_strValue = strValue;
            if (this.Count == 0)
            {
                field.m_strName = "###";
                field.m_strIndicator = "";
                field.m_strValue = field.m_strValue.PadRight(24, '?');
            }

            field.CalculateHeight(null, false);
            if (bInOrder == false)
            {
                base.Add(field);
            }
            else
            {
                //�ȶ�λ������insert
                int nPosition = this.GetPosition(field.Name);
                this.InsertInternal(nPosition + 1,
                    field);
                nOutputPosition = nPosition;
            }

            if (bFireTextChanged == true)
            {
                // �ĵ������ı�
                this.MarcEditor.FireTextChanged();
            }

            return field;
        }


        // ����
        // ׷��һ�����ֶΣ����ⲿʹ��
        // �ú������������ڴ���󣬻�������������
        // ������ʷ: ��
        // parameters:
        //		strName	���ֶ�����
        //		strIndicator	���ֶ�ָʾ��
        //		strValue	���ֶ�ֵ(������ܰ���������ֶ�ָʾ��ASCII31)
        /// <summary>
        /// ����һ���ֶζ���
        /// </summary>
        /// <param name="strName">�ֶ���</param>
        /// <param name="strIndicator">�ֶ�ָʾ��</param>
        /// <param name="strValue">�ֶ�����</param>
        /// <param name="bInOrder">�Ƿ�Ҫ�����ֶ���˳����뵽�ʵ�λ��</param>
        /// <returns>�������ֶζ���</returns>
        public Field Add(string strName,
            string strIndicator,
            string strValue,
            bool bInOrder)
        {
            strValue = strValue.Replace(Record.SUBFLD, Record.KERNEL_SUBFLD);

            int nOutputPosition = -1;
            Field field = this.AddInternal(strName,
                strIndicator,
                strValue,
                true,
                bInOrder,
                out nOutputPosition);


            // ����ʧЧ����

            InvalidateRect iRect = new InvalidateRect();
            iRect.bAll = false;
            if (nOutputPosition == -1)
            {
                iRect.rect = this.MarcEditor.GetItemBounds(this.Count - 1,
                    1,
                    BoundsPortion.FieldAndBottom);
            }
            else
            {
                if (this.MarcEditor.FocusedFieldIndex > nOutputPosition)
                {
                    this.MarcEditor.SelectedFieldIndices[0] = (int)this.MarcEditor.SelectedFieldIndices[0] + 1;
                }
                iRect.rect = this.MarcEditor.GetItemBounds(nOutputPosition,
                    -1,
                    BoundsPortion.FieldAndBottom);
            }

            // �����ֶ����ͣ��轹��λ��
            if (field.m_strName == this.MarcEditor.DefaultFieldName)
                this.MarcEditor.SetActiveField(field, 1);
            else if (Record.IsControlFieldName(field.m_strName) == true)
                this.MarcEditor.SetActiveField(field, 3);
            else
                this.MarcEditor.SetActiveField(field, 2);

            //this.marcEditor.ActiveField(field,3);

            this.MarcEditor.AfterDocumentChanged(ScrollBarMember.Both,
                iRect);

            return field;
        }

        // �����ֶ������ж�λ
        private int GetPosition(string strFieldName)
        {
            if (this.Count == 0)
                return 0;

            int nBaseIndex = 0;
            for (int i = 0; i < this.Count; i++)
            {
                Field field = this[i];
                if (String.Compare(field.Name, strFieldName) <= 0)  // < 2009/7/3 changed
                {
                    nBaseIndex = i;
                }
            }

            return nBaseIndex;
        }

        //--------------------------ǰ���ֶ�-------------------

        // ǰ��һ�����ֶΣ�ֻ���ڲ�ʹ�ã�
        // ��Ϊ�ú���ֻ�����ڴ���󣬲��漰���������
        // ������ʷ: ��
        // parameters:
        //		nIndex	�ο���λ��
        //		strName	���ֶ�����
        //		strIndicator	���ֶ�ָʾ��
        //		strValue	���ֶ�ֵ
        // return:
        //		void
        internal Field InsertInternal(int nIndex,
            Field field)
        {
            if (this.Count == 0)
                throw new Exception("��ǰ MARC ��¼û��ͷ���������ȴ���ͷ����");

            Debug.Assert(nIndex <= this.Count, "nIndex ["+nIndex.ToString()+"] ���Ϸ�");

            string strCaption = this.MarcEditor.GetLabel(field.m_strName);

            field.m_strNameCaption = strCaption;
            field.container = this;
            field.CalculateHeight(null, false);
            if (this.Count == 0)
            {
                field.m_strName = "###";
                field.m_strIndicator = "";
                field.m_strValue = field.m_strValue.PadRight(24, '?');
            }

            base.Insert(nIndex, field);

            if (this.MarcEditor.curEdit != null)
                this.MarcEditor.curEdit.ContentIsNull = true;    // ��ֹ�������ʱ�ͻ��ڴ� 2009/7/3

            // �ĵ������ı�
            this.MarcEditor.FireTextChanged();

            return field;
        }

        // ���ݻ��ڸ�ʽ�ı�ʾ����ֶε��ַ�������nIndexλ��ǰ��������ֶ�
        // ������ʷ: ��
        // parameters:
        //		nIndex	λ��
        //		strFieldMarc	marc�ַ���
        //		nNewFieldCount	out�����������˼����ֶ�
        internal void InsertInternal(int nIndex,
            string strFieldsMarc,
            out int nNewFieldsCount)
        {
            nNewFieldsCount = 0;

            // ���ҵ��м����ֶ�
            strFieldsMarc = strFieldsMarc.Replace(Record.SUBFLD, Record.KERNEL_SUBFLD);
            List<string> fields = Record.GetFields(strFieldsMarc);
            if (fields == null || fields.Count == 0)
                return;

            nNewFieldsCount = fields.Count;

            this.InsertInternal(nIndex, fields);
        }

        // ������ʷ: ��
        internal void InsertInternal(int nIndex,
            List<string> fields)
        {
            if (fields == null || fields.Count == 0)
                return;

            // 2007/7/17
            // ��Сedit�ؼ�����
            this.MarcEditor.HideTextBox();

            // 2014/7/10
            if (this.MarcEditor.curEdit != null)
                this.MarcEditor.curEdit.ContentIsNull = true;    // ��ֹ�������ʱ�ͻ��ڴ�

            // �������ĵ�һ��Ԫ���漰��ͷ������Ҫ��������Ĵ���
            // 2009/3/5
            if (nIndex == 0)
            {
                Debug.Assert(fields.Count > 0, "");
                if (fields[0].Length > 24)
                {
                    string strValue = fields[0];
                    string strHeader = strValue.Substring(0, 24);
                    string strOther = strValue.Substring(24);
                    fields[0] = strHeader;
                    fields.Insert(1, strOther);
                }
            }

            // �Ѷ���ֶμӽ�ȥһ����ʱ������
            // List<Field> aField = new List<Field>();
            int nTempIndex = nIndex;
            for (int i = 0; i < fields.Count; i++)
            {
                Field field = new Field(this);
                base.Insert(nTempIndex, field);
                nTempIndex++;
                // aField.Add(field);
                field.SetFieldMarc(fields[i], false);

                // if (this.Count == 0 && i == 0)
                if (nTempIndex == 1)
                {
                    field.m_strName = "###";
                    field.m_strIndicator = "";
                    field.m_strValue = field.m_strValue.PadRight(24, '?');
                }
            }

            /*
            // �����˼�¼��
            this.InsertRange(nIndex, aField);
             * */

            int nTailIndex = -1;
            if (this.MarcEditor.SelectedFieldIndices.Count > 0)
                nTailIndex = this.MarcEditor.SelectedFieldIndices[this.MarcEditor.SelectedFieldIndices.Count - 1];

            // 2007/7/17
            // �����ֶ��±�Ҳ���ƶ�
            if (nTailIndex != -1)
            {
                if (nIndex <= nTailIndex)
                {
                    // this.MarcEditor.FocusedFieldIndex += fields.Count;

                    // 2014/7/10
                    for (int i = 0; i < this.MarcEditor.SelectedFieldIndices.Count; i++)
                    {
                        this.MarcEditor.SelectedFieldIndices[i] += fields.Count;
                    }
                }
            }


            // �ĵ������仯
            this.MarcEditor.FireTextChanged();

            // 2007/7/17
            // �����С�༭��λ�ÿ��ܱ��ƶ���
            this.MarcEditor.SetEditPos();

#if NO
            // �������ݸ���Сedit�ؼ���
            this.MarcEditor.ItemTextToEditControl();
#endif
        }

        // ǰ��һ�����ֶΣ��ɹ��ڲ����ⲿʹ�ã�
        // �ú������������ڴ���󣬻�������������
        // ������ʷ: ��
        // parameters:
        //		nIndex	�ο���λ��
        //		strName	���ֶ�����
        //		strIndicator	���ֶ�ָʾ��
        //		strValue	���ֶ�ֵ(������ܰ���������ֶ�ָʾ��ASCII31)
        // ˵��:�����ڲ���InsertField(nIndex,field)�汾
        /// <summary>
        /// ��ָ��λ�ò���һ���µ��ֶζ���
        /// </summary>
        /// <param name="nIndex">����λ��</param>
        /// <param name="strName">�ֶ���</param>
        /// <param name="strIndicator">�ֶ�ָʾ��</param>
        /// <param name="strValue">�ֶ�����</param>
        /// <returns>�������ֶζ���</returns>
        public Field Insert(int nIndex,
            string strName,
            string strIndicator,
            string strValue)
        {
            strValue = strValue.Replace(Record.SUBFLD, Record.KERNEL_SUBFLD);

            Field field = new Field(this);
            field.m_strName = strName.PadLeft(3, '*');
            field.m_strIndicator = strIndicator;
            field.m_strValue = strValue;

            this.Insert(nIndex,
                field);

            return field;
        }

        // ǰ��һ���ֶ�
        // ������ʷ: ��
        /// <summary>
        /// ����һ���ֶζ���
        /// </summary>
        /// <param name="nIndex">λ��</param>
        /// <param name="field">�ֶζ���</param>
        public /*override*/ new void Insert(int nIndex,
            Field field)
        {
            Debug.Assert(nIndex <= this.Count, "nIndex�������Ϸ�");
            // Debug.Assert(oValue is Field, "����ΪField����");
            // Field field = (Field)oValue;

            // �����ݻ�ԭ���ѵ�ǰ������Ϊ�գ�ʡ��Activeʱ���±���
            this.MarcEditor.ClearSelectFieldIndices();

            this.InsertInternal(nIndex,
                field);

            // �����ֶ����ͣ��轹��λ��
            if (field.m_strName == this.MarcEditor.DefaultFieldName)
                this.MarcEditor.SetActiveField(field, 1);
            else if (Record.IsControlFieldName(field.m_strName) == true)
                this.MarcEditor.SetActiveField(field, 3);
            else
                this.MarcEditor.SetActiveField(field, 2);

            // ʧЧ��Χ
            int nStartIndex = 0;
            if (nIndex > 0)
                nStartIndex = nIndex - 1;
            InvalidateRect iRect = new InvalidateRect();
            iRect.bAll = false;
            iRect.rect = this.MarcEditor.GetItemBounds(nStartIndex,
                -1,
                BoundsPortion.FieldAndBottom);
            this.MarcEditor.AfterDocumentChanged(ScrollBarMember.Both,
                iRect);
        }

        //--------------------------����ֶ�-------------------

        // ���һ�����ֶΣ��ɹ��ڲ����ⲿʹ�ã�
        // �ú������������ڴ���󣬻�������������
        // ������ʷ: ��
        // parameters:
        //		nIndex	�ο���λ��
        //		strName	���ֶ�����
        //		strIndicator	���ֶ�ָʾ��
        //		strValue	���ֶ�ֵ(������ܰ���������ֶ�ָʾ��ASCII31)
        // ע��: �������Զ����ָʾ���Ķ�λ����
        /// <summary>
        /// ����һ���µ��ֶζ����ڲο�λ�õĺ���
        /// </summary>
        /// <param name="nIndex">����λ�á��ֶζ��󽫲��뵽���λ�õĺ���</param>
        /// <param name="strName">�ֶ���</param>
        /// <param name="strIndicator">�ֶ�ָʾ��</param>
        /// <param name="strValue">�ֶ�����</param>
        /// <returns>�²�����ֶζ���</returns>
        public Field InsertAfter(int nIndex,
            string strName,
            string strIndicator,
            string strValue)
        {
            strValue = strValue.Replace(Record.SUBFLD, Record.KERNEL_SUBFLD);

            Field field = new Field();
            field.m_strName = strName;
            field.m_strIndicator = strIndicator;
            field.m_strValue = strValue;

            this.InsertAfter(nIndex,
                field);

            return field;
        }

        // ����ֶ�
        // ������ʷ: ��
        /// <summary>
        /// ����һ���µ��ֶζ����ڲο�λ�õĺ���
        /// </summary>
        /// <param name="nIndex">����λ�á��ֶζ��󽫲��뵽���λ�õĺ���</param>
        /// <param name="field">�ֶζ���</param>
        public void InsertAfter(int nIndex,
            Field field)
        {
            Debug.Assert(nIndex < this.Count, "InsertAfterField(),nIndex�������Ϸ�");

            // �ڴ�����һ��
            this.InsertInternal(nIndex + 1,
                field);

            // �����ֶ����ͣ��轹��λ��
            if (field.m_strName == this.MarcEditor.DefaultFieldName)
                this.MarcEditor.SetActiveField(field, 1);
            else if (Record.IsControlFieldName(field.m_strName) == true)
                this.MarcEditor.SetActiveField(field, 3);
            else
                this.MarcEditor.SetActiveField(field, 2);

            // ʧЧ�ӵ�ǰ�����ֶε�ĩβ������
            InvalidateRect iRect = new InvalidateRect();
            iRect.bAll = false;
            iRect.rect = this.MarcEditor.GetItemBounds(nIndex + 1,
                -1,
                BoundsPortion.FieldAndBottom);
            this.MarcEditor.AfterDocumentChanged(ScrollBarMember.Horz,
                iRect);
        }

        //--------------------------ɾ���ֶ�-------------------

        // ɾ��һ���ֶΣ�ֻ���ڲ�ʹ�ã�
        // ������ʷ: ��
        // ��Ϊ�ú���ֻ�����ڴ���󣬲��漰���������
        // parameters:
        //		nFieldIndex	�ֶ�������
        // return:
        //		void
        internal Field RemoveAtInternal(int nFieldIndex)
        {
            var old_field = this[nFieldIndex];
            base.RemoveAt(nFieldIndex);

            // �ĵ������ı�
            this.MarcEditor.FireTextChanged();

            return old_field;
        }

        // ɾ��һ���ֶΣ��ɹ��ڲ����ⲿʹ�ã�
        // �ú������������ڴ���󣬻�������������
        // ������ʷ: ��
        // parameters:
        //		nFieldIndex	�ֶ�������
        /// <summary>
        /// ��ָ��λ��ɾ��һ���ֶζ���
        /// </summary>
        /// <param name="nFieldIndex">λ��</param>
        public /*override*/ new void RemoveAt(int nFieldIndex)
        {
            this.MarcEditor.Flush();
            this.RemoveAtInternal(nFieldIndex);

            // ��Сedit�ؼ�����
            this.MarcEditor.SelectedFieldIndices.Remove(nFieldIndex);
            this.MarcEditor.HideTextBox();

            if (nFieldIndex < this.Count)
                this.MarcEditor.SetActiveField(nFieldIndex, this.MarcEditor.m_nFocusCol);

            // Ӧ��ʧЧ�����������������Ż�
            InvalidateRect iRect = new InvalidateRect();
            iRect.bAll = false;
            //iRect.rect = 
            this.MarcEditor.AfterDocumentChanged(ScrollBarMember.Vert,
                iRect);
        }

        // ������ʷ: ��
        /// <summary>
        /// ɾ�������ֶζ���
        /// </summary>
        /// <param name="fieldIndices">λ���±�����</param>
        public void RemoveAt(int[] fieldIndices)
        {
            // ���ѡ�ж���
            this.MarcEditor.ClearSelectFieldIndices();

            int nMixIndex = 1000;
            for (int i = 0; i < fieldIndices.Length; i++)
            {
                int nIndex = fieldIndices[i];
                if (nIndex < nMixIndex)
                    nMixIndex = nIndex;
                this[nIndex] = null;
            }

            for (int i = 0; i < this.Count; i++)
            {
                if (this[i] == null)
                {
                    this.RemoveAtInternal(i);
                    i--;
                }
            }

            if (nMixIndex < this.Count)
                this.MarcEditor.SetActiveField(nMixIndex, this.MarcEditor.m_nFocusCol);

            // Ӧ��ʧЧ�����������������Ż�
            InvalidateRect iRect = new InvalidateRect();
            iRect.bAll = true;
            this.MarcEditor.AfterDocumentChanged(ScrollBarMember.Vert,
                iRect);
        }

        #region ���������ϣ��������κ��¼�

        internal void _removeAt(int field_index)
        {
            base.RemoveAt(field_index);
        }

        internal void _insert(int field_index, Field field)
        {
            base.Insert(field_index, field);
        }

        #endregion
    }

    /// <summary>
    /// �ֶ�����ӿ�
    /// </summary>
    internal class FieldComparer : IComparer<Field>
    {
        int IComparer<Field>.Compare(Field x, Field y)
        {
            if (x.Name == y.Name)
                return 0;

            if (x.Name == "hdr")
                return -1;
            if (y.Name == "hdr")
                return 1;

            // �ѷ���'-'�滻Ϊ'/'�������ͱ�'0'��С
            return string.Compare(x.Name.Replace("-", "/"), y.Name.Replace("-", "/"));
        }
    }
}
