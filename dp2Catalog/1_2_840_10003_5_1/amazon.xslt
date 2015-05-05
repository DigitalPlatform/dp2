<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:dprms="http://dp2003.com/dprms"
    xmlns:usmarc="http://www.loc.gov/MARC21/slim"
    xmlns:unimarc="http://dp2003.com/UNIMARC"
    xmlns:amazon="http://webservices.amazon.com/AWSECommerceService/2011-08-01"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt"
	xmlns:msfunction="http://www.mycompany.org/ns/function"
    exclude-result-prefixes="" extension-element-prefixes="unimarc msxsl msfunction dprms amazon">
  <xsl:output method="xml" version="1.0" indent="yes"/>

  <msxsl:script implements-prefix="msfunction" language="C#">

    <![CDATA[     

    public string convertISBN(string str)
    {
        return str;//补上为ISBN添加连线的函数
    }
    public string convertPrice(string strPrice)
    {
        Int64 v = 0;
        Int64.TryParse(strPrice, out v);
		    string result = String.Format("{0:F2}",v/100);
        return result;
    }


        string[] tables = {"Abkhaz,abk",
"Achinese,ace",
"Acoli,ach",
"Adangme,ada",
"Afro,afa",
"Afrihili,afh",
"Aritifical,afh",
"Afrikaans,afr",
"Akan,aka",
"Akkadian,akk",
"Albanian,alb",
"Aleut,ale",
"Algonquian,alg",
"Amharic,amh",
"Apache,apa",
"Arabic,ara",
"Aramaic,arc",
"Armenian,arm",
"Arapaho,arp",
"Artificial,art",
"Arawak,arw",
"Assamese,asm",
"Athapascan,ath",
"Australian,aus",
"Avaric,ava",
"Avestan,ave",
"Awadhi,awa",
"Aymara,aym",
"Azerbaijani,aze",
"Banda,bad",
"Bamileke,bai",
"Bashkir,bak",
"Baluchi,bal",
"Bambara,bam",
"Balinese,ban",
"Basque,baq",
"Basa,bas",
"Baltic,bat",
"Beja,bej",
"Belarusian,bel",
"Bemba,bem",
"Bengali,ben",
"Berber,ber",
"Bhojpuri,bho",
"Braj,bra",
"Breton,bre",
"Buginese,bug",
"Bulgarian,bul",
"Burmese,bur",
"Caddo,cad",
"Central,cai",
"Carib,car",
"Catalan,cat",
"Caucasian,cau",
"Cebuano,ceb",
"Celtic,cel",
"Chamorro,cha",
"Chibcha,chb",
"Chechen,che",
"Chagatai,chg",
"Chinese,chi",
"Truk,chk",
"Chinook,chn",
"Choctaw,cho",
"Cherokee,chr",
"Church,chu",
"Chuvash,chv",
"Cheyenne,chy",
"Chamic,cmc",
"Coptic,cop",
"Cornish,cor",
"Cree,cre",
"Creoles,crp",
"Cushitic,cus",
"Czech,cze",
"Dakota,dak",
"Danish,dan",
"Delaware,del",
"Dinka,din",
"Dogri,doi",
"Dravidian,dra",
"Duala,dua",
"Dutch,dut",
"Dyula,dyu",
"Efik,efi",
"Egyptian,egy",
"Ekajuk,eka",
"Elamite,elx",
"English,eng",
"Esperanto,epo",
"Estonian,est",
"Ewe,ewe",
"Fang,fan",
"Faroese,fao",
"Fanti,fat",
"Fijian,fij",
"Finnish,fin",
"Finno-Ugrian,fiu",
"Fon,fon",
"French,fre",
"Frisian,fry",
"Fula,ful",
"Ga,gaa",
"Gayo,gay",
"Germanic,gem",
"Georgian,geo",
"German,ger",
"Ethiopic,gez",
"Gilbertese,gil",
"Gaelic,gla",
"Irish,gle",
"Manx,glv",
"Gondi,gon",
"Gothic,got",
"Grebo,grb",
"Greek,gre",
"Guarani,grn",
"Gujarati,guj",
"Haida,hai",
"Hausa,hau",
"Hawaiian,haw",
"Hebrew,heb",
"Herero,her",
"Hiligaynon,hil",
"Himachali,him",
"Hindi,hin",
"Hiri,hmo",
"Hungarian,hun",
"Hnpa,hup",
"Iban,iba",
"Igbo,ibo",
"Icelandic,ice",
"Ijo,ijo",
"Iloko,ilo",
"Interligua,ina",
"Indic,inc",
"Indonesian,ind",
"Indo-European,ine",
"Iranian,ira",
"Iroquoian,iro",
"Italian,ita",
"Javanese,jav",
"Japanese,jpn",
"Judeo-Persian,jpr",
"Judeo-Arabic,jrb",
"Kara-Kalpak,kaa",
"Kabyle,kab",
"Kachin,kac",
"Kamba,kam",
"Kannada,kan",
"Karen,kar",
"Kashmiri,kas",
"Kanuri,kau",
"Kazakh,kaz",
"Khasi,kha",
"Khoisan,khi",
"Khmer,khm",
"Khotanese,kho",
"Kikuyu,kik",
"Kinyarwanda,kin",
"Kyrgyz,kir",
"Konkani,kok",
"Kongo,kon",
"Korean,kor",
"Kpelle,kpe",
"Kru,kro",
"Kurukh,kru",
"Kurdish,kur",
"Kutenai,kut",
"Ladino,lad",
"Lahndi,lah",
"Lamba,lam",
"Lao,lao",
"Latin,lat",
"Latvian,lav",
"Lithuanian,lit",
"Luba-Katanga,lub",
"Ganda,lug",
"Luiseno,lui",
"Lunda,lun",
"Luo,luo",
"Macedonian,mac",
"Madurese,mad",
"Magahi,mag",
"Marshall,mah",
"Maithili,mai",
"Makasar,mak",
"Malayalam,mal",
"Mandingo,man",
"Maori,mao",
"Austronesian,map",
"Marathi,mar",
"Masai,mas",
"Malay,may",
"Mende,men",
"Micmac,mic",
"Minangkabau,min",
"Miscellaneous,mis",
"Malagasy,mlg",
"Maltese,mlt",
"Manipuri,mni",
"Manobolanguages,mno",
"Mohawk,moh",
"Moldavian,mol",
"Mongolian,mon",
"Multiple,mul",
"Munda,mun",
"Creek,mus",
"Marwari,mwr",
"Mayan,myn",
"North,nai",
"Navajo,nav",
"Ndebele,nbl",
"Ndonga,ndo",
"Nepali,nep",
"Newari,new",
"Niger-Kordofanian,nic",
"Niuean,niu",
"Norwegian,nor",
"Northern,nso",
"Nubian,nub",
"Nyanja,nya",
"Nyamwezi,nym",
"Nyoro,nyo",
"Ojibwa,oji",
"Oriya,ori",
"Oromo,orm",
"Osage,osa",
"Ossetic,oss",
"Ottoman,ota",
"Otomian,oto",
"Papuan-Australian,paa",
"Pangasinan,pag",
"Pahlavi,pal",
"Pampanga,pam",
"Panjabi,pan",
"Papiamento,pap",
"Palauan,pau",
"Persian,per",
"Philippine,phi",
"Pali,pli",
"Polish,pol",
"Ponape,pon",
"Portuguese,por",
"Prakrit,pra",
"Provencal,pro",
"Pushto,pus",
"Quechua,que",
"Rajasthani,raj",
"Rarotongan,rar",
"Romance,roa",
"Raeto-Romance,roh",
"Romany,rom",
"Romanian,rum",
"Rundi,run",
"Russian,rus",
"Sandawe,sad",
"Sango,sag",
"South,sai",
"Salishan,sal",
"Samaritan,sam",
"Sanskrit,san",
"Serbo-Croatian,scc",
"Scots,sco",
"Selkup,sel",
"Semitic,sem",
"Sign,sgn",
"Shan,shn",
"Sidamo,sid",
"Sinhalese,sin",
"Siouan,sio",
"Sino-Tibetan,sit",
"Slavic,sla",
"Slovak,slo",
"Slovenian,slv",
"Sami,smi",
"Samoan,smo",
"Shona,sna",
"Sindhi,snd",
"Sogdian,sog",
"Somali,som",
"Songhai,son",
"Sotho,sot",
"Spanish,spa",
"Serer,srr",
"Nilo-Saharan,ssa",
"Swazi,ssw",
"Sukuma,suk",
"Susu,sus",
"Sumerian,sux",
"Swahili,swa",
"Swedish,swe",
"Syriac,syr",
"Tahitian,tah",
"Tamil,tam",
"Tatar,tat",
"Telugu,tel",
"Temne,tem",
"Terena,ter",
"Tajik,tgk",
"Tagalog,tgl",
"Thai,tha",
"Tibetan,tib",
"Tigre,tig",
"Tigrinya,tir",
"Tiv,tiv",
"Tlingit,tli",
"Tonga,tog",
"Tok,tpi",
"Tsimshian,tsi",
"Tswana,tsn",
"Tsonga,tso",
"Turkmen,tuk",
"Tumbuka,tum",
"Turkish,tur",
"Altaic,tut",
"Twi,twi",
"Ugaritic,uga",
"Uighur,uig",
"Ukrainian,ukr",
"Umbundu,umb",
"Undetermined,und",
"Urdu,urd",
"Uzbek,uzb",
"Vai,vai",
"Venda,ven",
"Vietnamese,vie",
"Votic,vot",
"Wakashan,wak",
"Walamo,wal",
"Washo,was",
"Welsh,wel",
"Sorbian,wen",
"Wolof,wol",
"Xhosa,xho",
"Yao,yao",
"Yap,yap",
"Yiddish,yid",
"Yoruba,yor",
"Yupik,ypk",
"Zapotec,zap",
"Zenaga,zen",
"Zulu,zul",
"Zuni,zun"
};
        Hashtable table = new Hashtable();
        public string convertLanguage(string strInput)
        {
            if (table.Count == 0)
            {
                foreach (string line in tables)
                {
                    int nRet = line.IndexOf(',');
                    if (nRet != -1)
                    {
                        string strKey = line.Substring(0, nRet);
                        string strValue = line.Substring(nRet + 1);
                        if (!table.ContainsKey(strKey.ToLower()))
                            table.Add(strKey.ToLower(), strValue);
                    }
                }
            }

            string input = strInput.ToLower();
            string strResult = "";
            if (table.ContainsKey(input))
                strResult = (string)table[input];
            else
                strResult = strInput;

            return strResult;
        }


	]]>

  </msxsl:script>

  <xsl:template match="/">
    <!--为了生成系统的XML备份格式数据，可加dprms:collection封装元素
    <xsl:element name="dprms:collection">
      <xsl:apply-templates select="amazon:Item"/>
    </xsl:element>
    -->
    <xsl:apply-templates select="amazon:Item"/>
  </xsl:template>

  <xsl:template match="amazon:Item">
    <!--优选ISBN，无，则选EAN-->
    <xsl:variable name="ISBN">
      <xsl:choose>
        <xsl:when test="amazon:ItemAttributes/amazon:ISBN!=''">
          <xsl:value-of select="amazon:ItemAttributes/amazon:ISBN"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="amazon:ItemAttributes/amazon:EAN"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--源数据中的价格是以分为单位的，所以用了自定义的msfunction:convertPrice()函数转换-->
    <xsl:variable name="availability" select="msfunction:convertPrice(amazon:ItemAttributes/amazon:ListPrice/amazon:Amount)" />

    <!--套中所含册数-->
    <xsl:variable name="sets" select="amazon:ItemAttributes/amazon:NumberOfItems" />

    <xsl:variable name="binding" select="amazon:ItemAttributes/amazon:Binding" />
    <xsl:element name="record" namespace="http://dp2003.com/UNIMARC">
      <xsl:element name="leader" namespace="http://dp2003.com/UNIMARC">
        <!--采用默认的头标值，以适应MARC必备头标字段的要求-->
        <xsl:value-of select="'?????nam0 22????? n 45  '"/>
      </xsl:element>
      <xsl:element name="controlfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="tag">
          <xsl:value-of select="'001'"/>
        </xsl:attribute>
        <!--采用'ASIN:'前缀串ASIN值作为001必备字段内容-->
        <xsl:value-of select="concat('ASIN:',amazon:ASIN)"/>
      </xsl:element>

      <xsl:element name="datafield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="tag">
          <xsl:value-of select="'010'"/>
        </xsl:attribute>
        <xsl:attribute name="ind1">
          <xsl:value-of select="' '"/>
        </xsl:attribute>
        <xsl:attribute name="ind2">
          <xsl:value-of select="' '"/>
        </xsl:attribute>
        <!--如果ISBN值不为空，则生成$a子字段。-->
        <!--从数据清爽角度考虑，通过亚马逊元素是否有值，只将有值的元素对应转换成MARC字段或子字段。-->
        <!--而如果从简化样式转换代码，可以将所需亚马逊元素一一绑定转换，没有值则生成空的字段或子字段也不失为一种好方式。-->
        <xsl:if test="$ISBN!=''">
          <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
            <xsl:attribute name="code">
              <xsl:value-of select="'a'"/>
            </xsl:attribute>
            <!--ISBN，唯一，所以直接生成010$a子字段值，如果需要转换为带连线格式，需要用扩展函数转换-->
            <xsl:value-of select="msfunction:convertISBN($ISBN)"/>
          </xsl:element>
        </xsl:if>
        <xsl:if test="$binding!=''">
          <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
            <xsl:attribute name="code">
              <xsl:value-of select="'b'"/>
            </xsl:attribute>
            <xsl:value-of select="$binding"/>
          </xsl:element>
        </xsl:if>
        <xsl:if test="$availability!=''">
          <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
            <xsl:attribute name="code">
              <xsl:value-of select="'d'"/>
            </xsl:attribute>
            <xsl:value-of select="concat(amazon:ItemAttributes/amazon:ListPrice/amazon:CurrencyCode,$availability)"/>
            <!--假如amazon:NumberOfItems值大于1，则在价格后串“(全X册)”字符串-->
            <xsl:if test="$sets>1">
              <xsl:value-of select="concat('(全',$sets,'册)')"/>
            </xsl:if>
          </xsl:element>
        </xsl:if>
      </xsl:element>

      <!--amazon:Author元素不带role属性，所以可以视为7字头责任者，但无法区分团体还是个人，所以统一转换为个人责任者-->
      <xsl:apply-templates select="amazon:ItemAttributes"/>

      <!--将图片链接转换成856字段-->
      <!--由于图片信息在amazon:Item子节点和孙节点中都有可能体现，且值是一样的，所以需要用xsl:choose元素判断以二选一。-->
      <!--为了体现大、中、小尺寸图片信息，在$x(内部注释)字段中，规范化表达。-->
      <xsl:choose>
        <xsl:when test="amazon:SmallImage/amazon:URL!=''">
          <!--如果amazon:Item/amazon:SmallImage/amazon:URL有值，则调用amazon:Item/amazon:SmallImage模板-->
          <xsl:apply-templates select="amazon:SmallImage"/>
        </xsl:when>
        <xsl:otherwise>
          <!--如果amazon:Item/amazon:SmallImage/amazon:URL无值，则调用amazon:ImageSets/amazon:ImageSet/amazon:SmallImage模板-->
          <xsl:apply-templates select="amazon:ImageSets" mode="SmallImage"/>
        </xsl:otherwise>
      </xsl:choose>

      <xsl:choose>
        <xsl:when test="amazon:MediumImage/amazon:URL!=''">
          <!--如果amazon:Item/amazon:MediumImage/amazon:URL有值，则调用amazon:Item/amazon:MediumImage模板-->
          <xsl:apply-templates select="amazon:MediumImage"/>
        </xsl:when>
        <xsl:otherwise>
          <!--如果amazon:Item/amazon:MediumImage/amazon:URL无值，则调用amazon:ImageSets/amazon:ImageSet/amazon:MediumImage模板-->
          <xsl:apply-templates select="amazon:ImageSets" mode="MediumImage"/>
        </xsl:otherwise>
      </xsl:choose>

      <xsl:choose>
        <xsl:when test="amazon:LargeImage/amazon:URL!=''">
          <!--如果amazon:Item/amazon:LargeImage/amazon:URL有值，则调用amazon:Item/amazon:LargeImage模板-->
          <xsl:apply-templates select="amazon:LargeImage"/>
        </xsl:when>
        <xsl:otherwise>
          <!--如果amazon:Item/amazon:LargeImage/amazon:URL无值，则调用amazon:ImageSets/amazon:ImageSet/amazon:LargeImage模板-->
          <xsl:apply-templates select="amazon:ImageSets" mode="LargeImage"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:element>
  </xsl:template>

  <xsl:template match="amazon:ItemAttributes">

    <!--MARC规定101字段不可重复，那么，先生成一个101字段-->
    <xsl:element name="datafield" namespace="http://dp2003.com/UNIMARC">
      <!--亚马逊的schema中没有具体细节，目前看到的数据实例中对语种的Type值有"Published"(大多数)、"Original Language"、"unknown"及空值。
      但这些不同Type属性对应的语种又都是一样的（估计是亚马逊数据录入人员没图书馆专业，所以稀里糊涂弄的）。
      假如这些信息规范，可以据此判断指示符1的值，及具体的子字段。
      现在，优先采用一个language即可，如果重复生成，估计产生多个$a内容都是一样的，不好看
      -->
      <xsl:attribute name="tag">
        <xsl:value-of select="'101'"/>
      </xsl:attribute>
      <xsl:attribute name="ind1">
        <xsl:value-of select="'0'"/>
      </xsl:attribute>
      <xsl:attribute name="ind2">
        <xsl:value-of select="' '"/>
      </xsl:attribute>
      <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="code">
          <xsl:value-of select="'a'"/>
        </xsl:attribute>
        <!--将语种生成一个变量，后面好对它进行语种代码替换加工-->
        <xsl:variable name="language">
          <xsl:choose>
            <xsl:when test="amazon:Languages/amazon:Language/amazon:Type='published'">
              <xsl:value-of select="amazon:Languages/amazon:Language/amazon:Name"/>
            </xsl:when>
            <xsl:when test="amazon:Languages/amazon:Language/amazon:Type='Original Language'">
              <xsl:value-of select="amazon:Languages/amazon:Language/amazon:Name"/>
            </xsl:when>
            <xsl:when test="amazon:Languages/amazon:Language/amazon:Type=''">
              <xsl:value-of select="amazon:Languages/amazon:Language/amazon:Name"/>
            </xsl:when>
            <xsl:when test="amazon:Languages/amazon:Language/amazon:Type='unknown'">
              <xsl:value-of select="amazon:Languages/amazon:Language/amazon:Name"/>
            </xsl:when>
          </xsl:choose>
        </xsl:variable>
        <xsl:value-of select="msfunction:convertLanguage($language)"/>
      </xsl:element>
    </xsl:element>

    <xsl:element name="datafield" namespace="http://dp2003.com/UNIMARC">
      <xsl:attribute name="tag">
        <xsl:value-of select="'200'"/>
      </xsl:attribute>
      <xsl:attribute name="ind1">
        <xsl:value-of select="'1'"/>
      </xsl:attribute>
      <xsl:attribute name="ind2">
        <xsl:value-of select="' '"/>
      </xsl:attribute>
      <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="code">
          <xsl:value-of select="'a'"/>
        </xsl:attribute>
        <xsl:value-of select="amazon:Title"/>
      </xsl:element>
      <!--amazon:Creator可重复，统一转换到200$f字段中-->
      <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="code">
          <xsl:value-of select="'f'"/>
        </xsl:attribute>
        <xsl:for-each select="amazon:Creator">
          <!--如果不是第一个，则前面串冒号分隔-->
          <xsl:choose>
            <xsl:when test="position() = 1">
              <!--amazon:Creator元素Role属性值用空格分隔，串在后面。-->
              <xsl:value-of select="concat(.,' ',@Role)" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="concat('; ',concat(.,' ',@Role))" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
    <!--唯一-->
    <xsl:apply-templates select="amazon:Edition"/>
    <!--因不可重复，所以只生成一个210字段-->
    <!--出版发行项，涉及到多个元素(都是唯一不可重复)体现出版者信息，优选amazon:Publisher-->
    <xsl:element name="datafield" namespace="http://dp2003.com/UNIMARC">
      <xsl:attribute name="tag">
        <xsl:value-of select="'210'"/>
      </xsl:attribute>
      <xsl:attribute name="ind1">
        <xsl:value-of select="' '"/>
      </xsl:attribute>
      <xsl:attribute name="ind2">
        <xsl:value-of select="' '"/>
      </xsl:attribute>
      <xsl:choose>
        <xsl:when test="amazon:Publisher!=''">
          <xsl:apply-templates select="amazon:Publisher"/>
        </xsl:when>
        <xsl:otherwise>
          <!--再优选制造商-->
          <xsl:choose>
            <xsl:when test="amazon:Manufacturer!=''">
              <xsl:apply-templates select="amazon:Manufacturer"/>
            </xsl:when>
            <xsl:otherwise>
              <!--再选工作室-->
              <xsl:apply-templates select="amazon:Studio"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:apply-templates select="amazon:PublicationDate"/>
    </xsl:element>
    <xsl:element name="datafield" namespace="http://dp2003.com/UNIMARC">
      <xsl:attribute name="tag">
        <xsl:value-of select="'215'"/>
      </xsl:attribute>
      <xsl:attribute name="ind1">
        <xsl:value-of select="' '"/>
      </xsl:attribute>
      <xsl:attribute name="ind2">
        <xsl:value-of select="' '"/>
      </xsl:attribute>
      <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="code">
          <xsl:value-of select="'a'"/>
        </xsl:attribute>
        <xsl:value-of select="concat(amazon:NumberOfPages,'页')"/>
      </xsl:element>
      <!--载体尺寸，在amazon:ItemDimensions和amazon:PackageDimensions元素中都有可能体现，优选amazon:ItemDimensions-->
      <xsl:choose>
        <xsl:when test="amazon:ItemDimensions/*/text()!=''">
          <!--ItemDimensions，尺寸，唯一，-->
          <xsl:apply-templates select="amazon:ItemDimensions"/>
          <!--附件信息-->
          <xsl:apply-templates select="../amazon:Accessories"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="amazon:PackageDimensions"/>
          <xsl:apply-templates select="../amazon:Accessories"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:element>
    <!--唯一-->
    <xsl:apply-templates select="../amazon:EditorialReviews/amazon:EditorialReview"/>
    <!--唯一-->
    <xsl:apply-templates select="../amazon:Subjects"/>
    <!--amazon:Author元素不带role属性，所以可以视为7字头责任者，但无法区分团体还是个人，所以统一转换为个人责任者-->
    <xsl:apply-templates select="amazon:Author"/>
    <!--amazon:Creator元素带Role属性，所以可以视为责任说明，转换为200$f者。-->
    <!--亚马逊元素中，amazon:Title是唯一的，而amazon:Creator是可选的，所以，转换一个200字段，重复生成200$f子字段-->
    <!--多个亚马逊元素涉及到载体形态项，其中，amazon:NumberOfPages是唯一的，所以，转换一个215字段-->

    <!--唯一-->
    <xsl:apply-templates select="../amazon:CustomerReviews/amazon:IFrameURL"/>


  </xsl:template>

  <xsl:template match="amazon:PublicationDate">
    <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
      <xsl:attribute name="code">
        <xsl:value-of select="'d'"/>
      </xsl:attribute>
      <!--亚马逊值格式为YYYY-MM-DD，原样输出，如欲转换，可另加字符串转换函数-->
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="amazon:PublicationDate">
    <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
      <xsl:attribute name="code">
        <xsl:value-of select="'d'"/>
      </xsl:attribute>
      <!--亚马逊值格式为YYYY-MM-DD，原样输出，如欲转换，可另加字符串转换函数-->
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="amazon:Manufacturer">
    <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
      <xsl:attribute name="code">
        <xsl:value-of select="'c'"/>
      </xsl:attribute>
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="amazon:Publisher">
    <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
      <xsl:attribute name="code">
        <xsl:value-of select="'c'"/>
      </xsl:attribute>
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="amazon:Studio">
    <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
      <xsl:attribute name="code">
        <xsl:value-of select="'c'"/>
      </xsl:attribute>
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="amazon:EditorialReview">
    <!--amazon:EditorialReview,内容摘要，创建330-->
    <xsl:element name="datafield" namespace="http://dp2003.com/UNIMARC">
      <xsl:attribute name="tag">
        <xsl:value-of select="'330'"/>
      </xsl:attribute>
      <xsl:attribute name="ind1">
        <xsl:value-of select="' '"/>
      </xsl:attribute>
      <xsl:attribute name="ind2">
        <xsl:value-of select="' '"/>
      </xsl:attribute>
      <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="code">
          <xsl:value-of select="'a'"/>
        </xsl:attribute>
        <xsl:value-of select="amazon:Content"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>


  <xsl:template match="amazon:CustomerReviews/amazon:IFrameURL">
    <!--amazon:IFrameURL,评论，创建856-->
    <xsl:element name="datafield" namespace="http://dp2003.com/UNIMARC">
      <xsl:attribute name="tag">
        <xsl:value-of select="'856'"/>
      </xsl:attribute>
      <xsl:attribute name="ind1">
        <xsl:value-of select="'4'"/>
      </xsl:attribute>
      <xsl:attribute name="ind2">
        <xsl:value-of select="' '"/>
      </xsl:attribute>
      <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="code">
          <xsl:value-of select="'3'"/>
        </xsl:attribute>
        <xsl:value-of select="'Book review'"/>
      </xsl:element>
      <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="code">
          <xsl:value-of select="'u'"/>
        </xsl:attribute>
        <xsl:value-of select="."/>
      </xsl:element>
      <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="code">
          <xsl:value-of select="'x'"/>
        </xsl:attribute>
        <!--用了type这个规范前缀来声明此856字段用途是评论（BookReview）-->
        <xsl:value-of select="concat('type:BookReview;source:Amazon:',../../amazon:ASIN)"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>


  <xsl:template match="amazon:Subjects">
    <xsl:apply-templates select="amazon:Subject"/>
  </xsl:template>

  <xsl:template match="amazon:Subject">
    <!--amazon:Subject可重复，重复生成610字段，非受控主题词-->
    <xsl:element name="datafield" namespace="http://dp2003.com/UNIMARC">
      <xsl:attribute name="tag">
        <xsl:value-of select="'610'"/>
      </xsl:attribute>
      <xsl:attribute name="ind1">
        <xsl:value-of select="'0'"/>
      </xsl:attribute>
      <xsl:attribute name="ind2">
        <xsl:value-of select="' '"/>
      </xsl:attribute>
      <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="code">
          <xsl:value-of select="'a'"/>
        </xsl:attribute>
        <xsl:value-of select="."/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="amazon:Accessories">
    <xsl:apply-templates select="amazon:Accessory"/>
  </xsl:template>

  <xsl:template match="amazon:Accessory">
    <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
      <xsl:attribute name="code">
        <xsl:value-of select="'e'"/>
      </xsl:attribute>
      <xsl:value-of select="amazon:Title"/>
    </xsl:element>
  </xsl:template>


  <xsl:template match="amazon:PackageDimensions">
    <!--生成几个变量，供后面调用-->
    <xsl:variable name="Height">
      <xsl:value-of select="concat('高: ',amazon:Height,'(',amazon:Height/@Units,')')"/>
    </xsl:variable>
    <xsl:variable name="Length">
      <xsl:value-of select="concat('长: ',amazon:Length,'(',amazon:Length/@Units,')')"/>
    </xsl:variable>
    <xsl:variable name="Width">
      <xsl:value-of select="concat('宽: ',amazon:Width,'(',amazon:Width/@Units,')')"/>
    </xsl:variable>
    <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
      <xsl:attribute name="code">
        <xsl:value-of select="'d'"/>
      </xsl:attribute>
      <!--把亚马逊尺寸信息用格式化方式串起来，嫌多，可以减少些。-->
      <xsl:value-of select="concat($Height,'; ',$Length,'; ',$Width)"/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="amazon:ItemDimensions">
    <!--生成几个变量，供后面调用-->
    <xsl:variable name="Height">
      <xsl:value-of select="concat('高: ',amazon:Height,'(',amazon:Height/@Units,')')"/>
    </xsl:variable>
    <xsl:variable name="Length">
      <xsl:value-of select="concat('长: ',amazon:Length,'(',amazon:Length/@Units,')')"/>
    </xsl:variable>
    <xsl:variable name="Width">
      <xsl:value-of select="concat('宽: ',amazon:Width,'(',amazon:Width/@Units,')')"/>
    </xsl:variable>
    <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
      <xsl:attribute name="code">
        <xsl:value-of select="'d'"/>
      </xsl:attribute>
      <!--把亚马逊尺寸信息用格式化方式串起来，嫌多，可以减少些。-->
      <xsl:value-of select="concat($Height,'; ',$Length,'; ',$Width)"/>
    </xsl:element>
  </xsl:template>


  <xsl:template match="amazon:Edition">
    <xsl:element name="datafield" namespace="http://dp2003.com/UNIMARC">
      <xsl:attribute name="tag">
        <xsl:value-of select="'205'"/>
      </xsl:attribute>
      <xsl:attribute name="ind1">
        <xsl:value-of select="' '"/>
      </xsl:attribute>
      <xsl:attribute name="ind2">
        <xsl:value-of select="' '"/>
      </xsl:attribute>
      <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="code">
          <xsl:value-of select="'a'"/>
        </xsl:attribute>
        <xsl:value-of select="."/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="amazon:Author">
    <xsl:element name="datafield" namespace="http://dp2003.com/UNIMARC">
      <xsl:attribute name="tag">
        <xsl:value-of select="'701'"/>
      </xsl:attribute>
      <xsl:attribute name="ind1">
        <xsl:value-of select="' '"/>
      </xsl:attribute>
      <xsl:attribute name="ind2">
        <xsl:value-of select="'0'"/>
      </xsl:attribute>
      <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="code">
          <xsl:value-of select="'a'"/>
        </xsl:attribute>
        <xsl:value-of select="."/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="amazon:SmallImage">
    <xsl:element name="datafield" namespace="http://dp2003.com/UNIMARC">
      <xsl:attribute name="tag">
        <xsl:value-of select="'856'"/>
      </xsl:attribute>
      <xsl:attribute name="ind1">
        <xsl:value-of select="'4'"/>
      </xsl:attribute>
      <xsl:attribute name="ind2">
        <xsl:value-of select="'2'"/>
      </xsl:attribute>
      <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="code">
          <xsl:value-of select="'3'"/>
        </xsl:attribute>
        <xsl:value-of select="'Cover image'"/>
      </xsl:element>
      <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="code">
          <xsl:value-of select="'u'"/>
        </xsl:attribute>
        <xsl:value-of select="amazon:URL"/>
      </xsl:element>
      <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="code">
          <xsl:value-of select="'x'"/>
        </xsl:attribute>
        <xsl:value-of select="concat('type:FrontCover.SmallImage',';size:',amazon:Width,'X',amazon:Height,amazon:Height/@Units,';source:Amazon:',../amazon:ASIN)"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="amazon:MediumImage">
    <xsl:element name="datafield" namespace="http://dp2003.com/UNIMARC">
      <xsl:attribute name="tag">
        <xsl:value-of select="'856'"/>
      </xsl:attribute>
      <xsl:attribute name="ind1">
        <xsl:value-of select="'4'"/>
      </xsl:attribute>
      <xsl:attribute name="ind2">
        <xsl:value-of select="'2'"/>
      </xsl:attribute>
      <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="code">
          <xsl:value-of select="'3'"/>
        </xsl:attribute>
        <xsl:value-of select="'Cover image'"/>
      </xsl:element>
      <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="code">
          <xsl:value-of select="'u'"/>
        </xsl:attribute>
        <xsl:value-of select="amazon:URL"/>
      </xsl:element>
      <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="code">
          <xsl:value-of select="'x'"/>
        </xsl:attribute>
        <xsl:value-of select="concat('type:FrontCover.MediumImage',';size:',amazon:Width,'X',amazon:Height,amazon:Height/@Units,';source:Amazon:',../amazon:ASIN)"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="amazon:LargeImage">
    <xsl:element name="datafield" namespace="http://dp2003.com/UNIMARC">
      <xsl:attribute name="tag">
        <xsl:value-of select="'856'"/>
      </xsl:attribute>
      <xsl:attribute name="ind1">
        <xsl:value-of select="'4'"/>
      </xsl:attribute>
      <xsl:attribute name="ind2">
        <xsl:value-of select="'2'"/>
      </xsl:attribute>
      <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="code">
          <xsl:value-of select="'3'"/>
        </xsl:attribute>
        <xsl:value-of select="'Cover image'"/>
      </xsl:element>
      <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="code">
          <xsl:value-of select="'u'"/>
        </xsl:attribute>
        <xsl:value-of select="amazon:URL"/>
      </xsl:element>
      <xsl:element name="subfield" namespace="http://dp2003.com/UNIMARC">
        <xsl:attribute name="code">
          <xsl:value-of select="'x'"/>
        </xsl:attribute>
        <xsl:value-of select="concat('type:FrontCover.LargeImage',';size:',amazon:Width,'X',amazon:Height,amazon:Height/@Units,';source:Amazon:',../amazon:ASIN)"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>
  
  <xsl:template match="amazon:ImageSets" mode="SmallImage">
    <!--由于amazon:ImageSet也是可重复的，所以仍通过调用模板方式起到生成重复字段的效果-->
    <xsl:apply-templates select="amazon:ImageSet" mode="SmallImage"/>
  </xsl:template>

  <xsl:template match="amazon:ImageSet" mode="SmallImage">
    <xsl:apply-templates select="amazon:SmallImage"/>
  </xsl:template>

  <xsl:template match="amazon:ImageSets" mode="MediumImage">
    <!--由于amazon:ImageSet也是可重复的，所以仍通过调用模板方式起到生成重复字段的效果-->
    <xsl:apply-templates select="amazon:ImageSet" mode="MediumImage"/>
  </xsl:template>

  <xsl:template match="amazon:ImageSet" mode="MediumImage">
    <xsl:apply-templates select="amazon:MediumImage"/>
  </xsl:template>

  <xsl:template match="amazon:ImageSets" mode="LargeImage">
    <!--由于amazon:ImageSet也是可重复的，所以仍通过调用模板方式起到生成重复字段的效果-->
    <xsl:apply-templates select="amazon:ImageSet" mode="LargeImage"/>
  </xsl:template>

  <xsl:template match="amazon:ImageSet" mode="LargeImage">
    <xsl:apply-templates select="amazon:LargeImage"/>
  </xsl:template>


</xsl:stylesheet>
