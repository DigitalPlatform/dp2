<?xml version="1.0" encoding="utf-8" ?>
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

   
    public string convertPrice(string strPrice)
    {
        Int64 v = 0;
        Int64.TryParse(strPrice, out v);
		    string result = String.Format("{0:F2}",v/100);
        return result;
    }

        string[] tables = 
        {
          "Chinese,chi",
          "French,fre",
          "Greek,gre",
          "Japanese,jpn",
          "Korean,kor",
          "Portuguese,por",
          "Russian,rus",
          "Spanish,spa"
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
    <!--先将亚马逊不可重复元素值，生成一系列的变量待用-->
    <!--ISBN变量，优选ISBN元素，无，则选EAN元素-->
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
    <!--availability变量，源数据中的价格是以分为单位的，所以用了自定义的msfunction:convertPrice()函数转换。再串其货币前缀-->
    <xsl:variable name="availability" select="concat(amazon:ItemAttributes/amazon:ListPrice/amazon:CurrencyCode,msfunction:convertPrice(amazon:ItemAttributes/amazon:ListPrice/amazon:Amount))" />
    <!--ASIN变量-->
    <xsl:variable name="ASIN" select="amazon:ASIN" />
    <!--binding变量-->
    <xsl:variable name="binding" select="amazon:ItemAttributes/amazon:Binding" />
    <!--author变量-->
    <xsl:variable name="author" select="amazon:ItemAttributes/amazon:Author" />
    <!--edition变量-->
    <xsl:variable name="edition" select="amazon:ItemAttributes/amazon:Edition" />
    <!--itemDimensions变量，它由几个子元素值串起来组成-->
    <xsl:variable name="itemDimensions" select="concat('Height:',amazon:ItemAttributes/amazon:ItemDimensions/amazon:Height,';','Length:',amazon:ItemAttributes/amazon:ItemDimensions/amazon:Length,';','Weight:',amazon:ItemAttributes/amazon:ItemDimensions/amazon:Weight,';','Width:',amazon:ItemAttributes/amazon:ItemDimensions/amazon:Width)" />
    <!--publisher变量，优选Publisher元素-->
    <xsl:variable name="publisher">
      <xsl:choose>
        <xsl:when test="amazon:ItemAttributes/amazon:Publisher!=''">
          <xsl:value-of select="amazon:ItemAttributes/amazon:Publisher"/>
        </xsl:when>
        <xsl:otherwise>
          <!--再优选制造商-->
          <xsl:choose>
            <xsl:when test="amazon:ItemAttributes/amazon:Manufacturer!=''">
              <xsl:value-of select="amazon:ItemAttributes/amazon:Manufacturer"/>
            </xsl:when>
            <xsl:otherwise>
              <!--再选工作室-->
              <xsl:value-of select="amazon:ItemAttributes/amazon:Studio"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>



    <!--生成usmarc:record元素，在其下方开始生成子元素-->
    <xsl:element name="record" namespace="http://www.loc.gov/MARC21/slim">
      <!--采用默认的头标值，以适应MARC必备头标字段的要求-->
      <xsl:element name="leader" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:value-of select="'?????nam a22?????3u 45  '"/>
      </xsl:element>
      <!--采用'ASIN:'前缀串ASIN值作为001必备字段内容-->
      <xsl:element name="controlfield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="tag">
          <xsl:value-of select="'001'"/>
        </xsl:attribute>
        <xsl:value-of select="concat('ASIN:',amazon:ASIN)"/>
      </xsl:element>
      <!--生成020字段。亚马逊元素对应用ISBN、Binding和Availability。由于这些元素得归于同一字段中，所以先生成020字段-->
      <xsl:element name="datafield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="tag">
          <xsl:value-of select="'020'"/>
        </xsl:attribute>
        <xsl:attribute name="ind1">
          <xsl:value-of select="' '"/>
        </xsl:attribute>
        <xsl:attribute name="ind2">
          <xsl:value-of select="' '"/>
        </xsl:attribute>
        <!--而如果从简化样式转换代码，可以将所需亚马逊元素一一绑定转换，没有值则生成空的字段或子字段也不失为一种好方式。则无须判断是否有值-->
        <xsl:if test="$ISBN!=''">
          <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
            <xsl:attribute name="code">
              <xsl:value-of select="'a'"/>
            </xsl:attribute>
            <!--如果有装祯内容，则把内容用括号括起置于ISBN值之后-->
            <xsl:choose>
              <xsl:when test="$binding!=''">
                <xsl:choose>
                  <!--如果有获得方式与价格内容，则在$a数据后串' :'-->
                  <xsl:when test="$availability!=''">
                    <xsl:value-of select="concat($ISBN,'(',$binding,') :')"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="concat($ISBN,'(',$binding,')')"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:otherwise>
                <!--如果有获得方式与价格内容，则在$a数据后串' :'-->
                <xsl:choose>
                  <xsl:when test="$availability!=''">
                    <xsl:value-of select="concat($ISBN,' :')"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <!--仅ISBN，则只输出ISBN内容-->
                    <xsl:value-of select="$ISBN"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:element>
        </xsl:if>
        <!--由于有价格，所以生成$c子字段及值-->
        <xsl:if test="$availability!=''">
          <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
            <xsl:attribute name="code">
              <xsl:value-of select="'c'"/>
            </xsl:attribute>
            <xsl:value-of select="$availability"/>
          </xsl:element>
        </xsl:if>
      </xsl:element>

      <!--MARC21规定041字段不可重复，那么，先生成一个041字段，如果遇多个language，则重复其Sa-->
      <xsl:element name="datafield" namespace="http://www.loc.gov/MARC21/slim">
        <!--亚马逊的schema中没有具体细节，目前看到的数据实例中对语种的Type值有"Published"(大多数)、"Original Language"、"unknown"及空值。
      但这些不同Type属性对应的语种又都是一样的（估计是亚马逊数据录入人员没图书馆专业，所以稀里糊涂弄的）。
      假如这些信息规范，可以据此判断指示符1的值，及具体的子字段。
      现在，仍采用见重复的language则重复生成$a方式——估计产生多个$a内容都是一样的，不好看。如以后客户有意见，则采用后面注释的那种只选一个值的方式。
      -->
        <xsl:attribute name="tag">
          <xsl:value-of select="'041'"/>
        </xsl:attribute>
        <xsl:attribute name="ind1">
          <xsl:value-of select="'0'"/>
        </xsl:attribute>
        <xsl:attribute name="ind2">
          <xsl:value-of select="' '"/>
        </xsl:attribute>
        <!--循环多个Language元素，以生成重复的$a子元素-->
        <xsl:for-each select="amazon:ItemAttributes/amazon:Languages/amazon:Language">
          <!--先产生lang变量，并用自定义函数对它进行三位语种代码加工-->
          <xsl:variable name="lang">
            <xsl:value-of select="msfunction:convertLanguage(amazon:Name)"/>
          </xsl:variable>
          <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
            <xsl:attribute name="code">
              <xsl:value-of select="'a'"/>
            </xsl:attribute>
            <xsl:value-of select="$lang"/>
          </xsl:element>
        </xsl:for-each>

        <!--只选一个language的方式，先注释掉，备查
      <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="code">
          <xsl:value-of select="'a'"/>
        </xsl:attribute>
        <xsl:variable name="lang">
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
        <xsl:value-of select="msfunction:convertLanguage($lang)"/>
      </xsl:element>
    -->
      </xsl:element>

      <!--生成245字段，由于亚马逊元数据中，仅有一个Title元素对应转换到245$a，所以数据后不考虑添加对应后续子字段的ISBD分隔标识符-->
      <xsl:element name="datafield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="tag">
          <xsl:value-of select="'245'"/>
        </xsl:attribute>
        <xsl:attribute name="ind1">
          <xsl:value-of select="'1'"/>
        </xsl:attribute>
        <xsl:attribute name="ind2">
          <xsl:value-of select="'0'"/>
        </xsl:attribute>
        <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
          <xsl:attribute name="code">
            <xsl:value-of select="'a'"/>
          </xsl:attribute>
          <xsl:value-of select="amazon:ItemAttributes/amazon:Title"/>
        </xsl:element>
      </xsl:element>
      <!--生成250版本信息，不重复-->
      <xsl:element name="datafield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="tag">
          <xsl:value-of select="'250'"/>
        </xsl:attribute>
        <xsl:attribute name="ind1">
          <xsl:value-of select="' '"/>
        </xsl:attribute>
        <xsl:attribute name="ind2">
          <xsl:value-of select="' '"/>
        </xsl:attribute>
        <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
          <xsl:attribute name="code">
            <xsl:value-of select="'a'"/>
          </xsl:attribute>
          <xsl:value-of select="amazon:ItemAttributes/amazon:Edition"/>
        </xsl:element>
      </xsl:element>
      <!--生成一个260字段，不重复-->
      <!--出版发行项，涉及到多个元素(都是唯一不可重复)体现出版者信息，优选amazon:Publisher-->
      <xsl:element name="datafield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="tag">
          <xsl:value-of select="'260'"/>
        </xsl:attribute>
        <xsl:attribute name="ind1">
          <xsl:value-of select="' '"/>
        </xsl:attribute>
        <xsl:attribute name="ind2">
          <xsl:value-of select="' '"/>
        </xsl:attribute>
        <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
          <xsl:attribute name="code">
            <xsl:value-of select="'a'"/>
          </xsl:attribute>
          <xsl:value-of select="' : '"/>
        </xsl:element>
        <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
          <xsl:attribute name="code">
            <xsl:value-of select="'b'"/>
          </xsl:attribute>
          <xsl:value-of select="$publisher"/>
        </xsl:element>
        <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
          <xsl:attribute name="code">
            <xsl:value-of select="'c'"/>
          </xsl:attribute>
          <!--亚马逊值格式为YYYY-MM-DD，原样输出，如欲转换，可另加字符串转换函数-->
          <xsl:value-of select="amazon:ItemAttributes/amazon:PublicationDate"/>
        </xsl:element>
      </xsl:element>

      <!--生成一个300字段-->
      <xsl:element name="datafield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="tag">
          <xsl:value-of select="'300'"/>
        </xsl:attribute>
        <xsl:attribute name="ind1">
          <xsl:value-of select="' '"/>
        </xsl:attribute>
        <xsl:attribute name="ind2">
          <xsl:value-of select="' '"/>
        </xsl:attribute>
        <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
          <xsl:attribute name="code">
            <xsl:value-of select="'a'"/>
          </xsl:attribute>
          <xsl:value-of select="concat(amazon:ItemAttributes/amazon:NumberOfPages,' p.')"/>
        </xsl:element>
        <!--载体尺寸，在amazon:ItemDimensions和amazon:PackageDimensions元素中都有可能体现，优选amazon:ItemDimensions-->
        <xsl:choose>
          <xsl:when test="amazon:ItemAttributes/amazon:ItemDimensions/*/text()!=''">
            <!--ItemDimensions，尺寸，唯一，-->
            <xsl:apply-templates select="amazon:ItemAttributes/amazon:ItemDimensions"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="amazon:ItemAttributes/amazon:PackageDimensions"/>
          </xsl:otherwise>
        </xsl:choose>
        <!--附件，如果有值的话。-->
        <xsl:choose>
          <xsl:when test="amazon:Accessories/amazon:Accessory/amazon:Title!=''">
            <xsl:apply-templates select="amazon:Accessories"/>
          </xsl:when>
        </xsl:choose>
      </xsl:element>
      <!--调用内容提要摘要模板-->
      <xsl:apply-templates select="amazon:EditorialReviews/amazon:EditorialReview"/>
      <!--生成一个653关键词字段，并根据重复的Subject元素生成重复的$a子字段-->
      <!--amazon:Subject可重复，重复生成653$a字段，非受控主题词-->
      <xsl:element name="datafield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="tag">
          <xsl:value-of select="'653'"/>
        </xsl:attribute>
        <xsl:attribute name="ind1">
          <xsl:value-of select="' '"/>
        </xsl:attribute>
        <xsl:attribute name="ind2">
          <xsl:value-of select="' '"/>
        </xsl:attribute>
        <xsl:for-each select="amazon:Subjects/amazon:Subject">
          <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
            <xsl:attribute name="code">
              <xsl:value-of select="'a'"/>
            </xsl:attribute>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
      <!--亚马逊元素Author和Creator体现了责任者信息，但Author不可重复，且实例中，常跟Creator元素值一样，无法区分团体还是个人，所以统一转换为个人责任者-->
      <xsl:apply-templates select="amazon:ItemAttributes/amazon:Author"/>

      <!--评注生成856-->
      <xsl:apply-templates select="amazon:CustomerReviews/amazon:IFrameURL"/>
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

  <xsl:template match="amazon:EditorialReview">
    <!--amazon:EditorialReview,内容摘要，创建520-->
    <xsl:element name="datafield" namespace="http://www.loc.gov/MARC21/slim">
      <xsl:attribute name="tag">
        <xsl:value-of select="'520'"/>
      </xsl:attribute>
      <xsl:attribute name="ind1">
        <xsl:value-of select="' '"/>
      </xsl:attribute>
      <xsl:attribute name="ind2">
        <xsl:value-of select="' '"/>
      </xsl:attribute>
      <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="code">
          <xsl:value-of select="'a'"/>
        </xsl:attribute>
        <xsl:value-of select="amazon:Content"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>


  <xsl:template match="amazon:CustomerReviews/amazon:IFrameURL">
    <!--amazon:IFrameURL,评论，创建856-->
    <xsl:element name="datafield" namespace="http://www.loc.gov/MARC21/slim">
      <xsl:attribute name="tag">
        <xsl:value-of select="'856'"/>
      </xsl:attribute>
      <xsl:attribute name="ind1">
        <xsl:value-of select="'4'"/>
      </xsl:attribute>
      <xsl:attribute name="ind2">
        <xsl:value-of select="' '"/>
      </xsl:attribute>
      <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="code">
          <xsl:value-of select="'3'"/>
        </xsl:attribute>
        <xsl:value-of select="'Book review'"/>
      </xsl:element>
      <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="code">
          <xsl:value-of select="'u'"/>
        </xsl:attribute>
        <xsl:value-of select="."/>
      </xsl:element>
      <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="code">
          <xsl:value-of select="'x'"/>
        </xsl:attribute>
        <!--用了type这个规范前缀来声明此856字段用途是评论（BookReview）-->
        <xsl:value-of select="concat('type:BookReview;source:Amazon:',../../amazon:ASIN)"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="amazon:Accessories">
    <xsl:apply-templates select="amazon:Accessory"/>
  </xsl:template>

  <xsl:template match="amazon:Accessory">
    <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
      <xsl:attribute name="code">
        <xsl:value-of select="'e'"/>
      </xsl:attribute>
      <xsl:value-of select="amazon:Title"/>
    </xsl:element>
  </xsl:template>


  <xsl:template match="amazon:PackageDimensions">
    <!--生成几个变量，供后面调用-->
    <xsl:variable name="Height">
      <xsl:value-of select="concat(amazon:Height,'(',amazon:Height/@Units,')')"/>
    </xsl:variable>
    <xsl:variable name="Length">
      <xsl:value-of select="concat(amazon:Length,'(',amazon:Length/@Units,')')"/>
    </xsl:variable>
    <xsl:variable name="Width">
      <xsl:value-of select="concat(amazon:Width,'(',amazon:Width/@Units,')')"/>
    </xsl:variable>
    <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
      <xsl:attribute name="code">
        <xsl:value-of select="'c'"/>
      </xsl:attribute>
      <!--仅输出高度，是英寸为单位，如果要转换为CM，再说。值后串' + '-->
      <xsl:value-of select="concat($Height,' + ')"/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="amazon:ItemDimensions">
    <!--生成几个变量，供后面调用-->
    <xsl:variable name="Height">
      <xsl:value-of select="concat(amazon:Height,'(',amazon:Height/@Units,')')"/>
    </xsl:variable>
    <xsl:variable name="Length">
      <xsl:value-of select="concat(amazon:Length,'(',amazon:Length/@Units,')')"/>
    </xsl:variable>
    <xsl:variable name="Width">
      <xsl:value-of select="concat(amazon:Width,'(',amazon:Width/@Units,')')"/>
    </xsl:variable>
    <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
      <xsl:attribute name="code">
        <xsl:value-of select="'c'"/>
      </xsl:attribute>
      <!--仅输出高度，是英寸为单位，如果要转换为CM，再说。-->
      <xsl:value-of select="concat($Height,' + ')"/>
    </xsl:element>
  </xsl:template>


  <xsl:template match="amazon:Author">
    <xsl:choose>
      <xsl:when test="self::amazon:Author!=''">
        <xsl:element name="datafield" namespace="http://www.loc.gov/MARC21/slim">
          <xsl:attribute name="tag">
            <xsl:value-of select="'700'"/>
          </xsl:attribute>
          <xsl:attribute name="ind1">
            <xsl:value-of select="'1'"/>
          </xsl:attribute>
          <xsl:attribute name="ind2">
            <xsl:value-of select="' '"/>
          </xsl:attribute>
          <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
            <xsl:attribute name="code">
              <xsl:value-of select="'a'"/>
            </xsl:attribute>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <!--调模板，实现生成重复字段效果-->
        <xsl:apply-templates select="../amazon:Creator"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="amazon:Creator">
    <xsl:element name="datafield" namespace="http://www.loc.gov/MARC21/slim">
      <xsl:attribute name="tag">
        <xsl:value-of select="'700'"/>
      </xsl:attribute>
      <xsl:attribute name="ind1">
        <xsl:value-of select="'1'"/>
      </xsl:attribute>
      <xsl:attribute name="ind2">
        <xsl:value-of select="' '"/>
      </xsl:attribute>
      <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="code">
          <xsl:value-of select="'a'"/>
        </xsl:attribute>
        <xsl:value-of select="."/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="amazon:SmallImage">
    <xsl:element name="datafield" namespace="http://www.loc.gov/MARC21/slim">
      <xsl:attribute name="tag">
        <xsl:value-of select="'856'"/>
      </xsl:attribute>
      <xsl:attribute name="ind1">
        <xsl:value-of select="'4'"/>
      </xsl:attribute>
      <xsl:attribute name="ind2">
        <xsl:value-of select="'2'"/>
      </xsl:attribute>
      <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="code">
          <xsl:value-of select="'3'"/>
        </xsl:attribute>
        <xsl:value-of select="'Cover image'"/>
      </xsl:element>
      <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="code">
          <xsl:value-of select="'u'"/>
        </xsl:attribute>
        <xsl:value-of select="amazon:URL"/>
      </xsl:element>
      <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="code">
          <xsl:value-of select="'x'"/>
        </xsl:attribute>
        <xsl:value-of select="concat('type:FrontCover.SmallImage',';size:',amazon:Width,'X',amazon:Height,amazon:Height/@Units,';source:Amazon:',../amazon:ASIN)"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="amazon:MediumImage">
    <xsl:element name="datafield" namespace="http://www.loc.gov/MARC21/slim">
      <xsl:attribute name="tag">
        <xsl:value-of select="'856'"/>
      </xsl:attribute>
      <xsl:attribute name="ind1">
        <xsl:value-of select="'4'"/>
      </xsl:attribute>
      <xsl:attribute name="ind2">
        <xsl:value-of select="'2'"/>
      </xsl:attribute>
      <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="code">
          <xsl:value-of select="'3'"/>
        </xsl:attribute>
        <xsl:value-of select="'Cover image'"/>
      </xsl:element>
      <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="code">
          <xsl:value-of select="'u'"/>
        </xsl:attribute>
        <xsl:value-of select="amazon:URL"/>
      </xsl:element>
      <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="code">
          <xsl:value-of select="'x'"/>
        </xsl:attribute>
        <xsl:value-of select="concat('type:FrontCover.MediumImage',';size:',amazon:Width,'X',amazon:Height,amazon:Height/@Units,';source:Amazon:',../amazon:ASIN)"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="amazon:LargeImage">
    <xsl:element name="datafield" namespace="http://www.loc.gov/MARC21/slim">
      <xsl:attribute name="tag">
        <xsl:value-of select="'856'"/>
      </xsl:attribute>
      <xsl:attribute name="ind1">
        <xsl:value-of select="'4'"/>
      </xsl:attribute>
      <xsl:attribute name="ind2">
        <xsl:value-of select="'2'"/>
      </xsl:attribute>
      <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="code">
          <xsl:value-of select="'3'"/>
        </xsl:attribute>
        <xsl:value-of select="'Cover image'"/>
      </xsl:element>
      <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
        <xsl:attribute name="code">
          <xsl:value-of select="'u'"/>
        </xsl:attribute>
        <xsl:value-of select="amazon:URL"/>
      </xsl:element>
      <xsl:element name="subfield" namespace="http://www.loc.gov/MARC21/slim">
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
