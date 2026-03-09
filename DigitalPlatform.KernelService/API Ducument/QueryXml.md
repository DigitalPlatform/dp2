# XML 检索式格式

dp2kernel(数据库内核)使用了一种专门的 XML 格式的检索语言来描述检索请求。它可以表达复杂的逻辑检索请求。

在 dp2rms 前端的检索窗中，可以直接输入这种 XML 格式的检索式进行检索。


## 文档主体结构

最重要的元素是 `<item>` 元素，它定义了一个最基本的检索单元。`<item>` 元素之间可以用`<operator>` 元素连接起来，表示逻辑运算。为了将某个范围内的能够形成检索结果的一个局部组合成一个单元以便参加更大规模的逻辑运算，可以使用 `<group>` 元素把 `<item>` 和 `<operator>` 元素包装起来。
同级的 `<group>` 元素和 `<item>` 元素之间，以及 `<group>` 元素和另一个或多个 `<group>` 元素之间，可以用 `<operator>` 元素连接起来。
`<group>` 元素有时候像普通算式中的括号，能控制逻辑运算的优先级次和执行顺序。

例1：
```xml
<item>…</item>
```
注: 省略号表示印刷中被略去的细节结构。以上代码为示范用，尚不能实际运行
这是一个最简单的结构，表示只有一个基本单元的检索。

例2：
```xml
<group>
        <item>…</item>
        <operator value="AND" />
        <item>…</item>
</group>
```
这表示两个基本单元之间进行AND(与)运算的逻辑检索。因为XML文档必须有一个唯一的根元素，所以这里使用了<group>元素来充当根元素，它同时也是 `<item>` 元素和 `<operator>` 元素的容器。

例3：
```xml
<group>
    <group>
        <item>…</item>
        <operator value="AND" />
        <item>…</item>
    </group>
    <operator value="OR" />
    <group>
        <item>…</item>
        <operator value="AND" />
        <item>…</item>
    </group>
</group>
```
这表示内层两个 `<group>` 结构之间进行OR(或)运算的逻辑检索。涉及的每个 `<group>` 结构其下级又包含了一层逻辑运算结果。一共有四个基本单元参与逻辑运算。

## <item> -- 基本检索单元

`<item>` 元素用于构造一个基本的检索单元。
`<item>` 元素下级通过 `<word>`、`<match>`、`<relation>`、`<dataType>`、`<order>`、`<originOrder>`、`<maxCount>`元素表达了检索参数。

例4：
```
<target list="中文图书:题名">
    <item>
        <word>中国</word>
        <match>left</match>
        <relation>=</relation>
        <dataType>string</dataType>
    </item>
    <lang>zh</lang>
</target>
```

<item>元素检索所针对的目标，是由<target>元素定义的。<target>元素在整个XML结构中的位置比较自由，它可以包围一个或者多个<item>元素。从<item>元素向父元素的方向寻找，第一个遇到的<target>元素就是对这个<item>元素起作用的元素。这样(允许自由位置的)设计的目的是便于在有多个<item>元素的情况下，允许共享节省<target>元素。

## <target> -- 检索目标

<target>元素定义了检索目标。<target>元素本身可以充当容器元素，它里面可以包含<item>、<operator>、<group>等元素。
<target>元素只有一个属性list。list属性定义了检索目标的列表，格式类似：
`中文图书:题名,责任者;中文采购:责任者`
规则就是用分号连接属于不同数据库的段落；每个段落内，冒号前是数据库名，冒号后是一个或者多个检索途径名。检索途径之间用逗号连接。

多个<item>元素以逻辑运算符OR连接起来的逻辑运算结构，如果它们之间检索词和匹配方式等参数都相同，那可以尽量将它们合并为一个<item>元素，将这些原本分离的<item>元素的检索目标合并为一个检索目标字符串即可。

例如:
```xml
<group>
    <target list="中文图书:题名">
        <item>
            <word>中国</word>
            <match>left</match>
            <relation>=</relation>
            <dataType>string</dataType>
        </item>
    </target>
    <operator value="OR" />
    <target list="中文图书:责任者">
        <item>
            <word>中国</word>
            <match>left</match>
            <relation>=</relation>
            <dataType>string</dataType>
        </item>
    </target>
</group>
```

可以合并为:
```xml
<group>
    <target list="中文图书:题名,责任者">
        <item>
            <word>中国</word>
            <match>left</match>
            <relation>=</relation>
            <dataType>string</dataType>
        </item>
    </target>
</group>
```

## <word> -- 检索词
<word>元素内的正文部分定义了检索词。

## <match> -- 匹配方式
<match>元素内的正文部分定义了匹配方式。
有以下几种可能：
left middle right exact
分别代表“前方一致”“中间一致”"“后方一致”“精确一致”。

## <relation> -- 关系
<relation>元素内的正文部分定义了对检索词进行比较时所要求的大小关系。
可用值的解释如下：
值	替代形式	说明
>	G	大于
>=	GE	大于等于
<	L	小于
<=	LE	小于等于
=	E	等于
!=	NE	不等于
range		范围

## <dataType> -- 数据类型
<dataType>元素内的正文部分定义了检索词的数据类型。
为number string 之一，分别代表“数值”“字符串”。
数据类型会影响到检索的匹配方式和效果。例如，对于时间字符串的检索点，如果采用number类型进行检索，就能准确比较时间值；而如果采用string类型进行检索，则只能把待匹配的字符串当作普通字符串进行比较，那么有可能无法正确命中。

## <order> -- 输出顺序
<order>元素内的正文部分定义了检索结果输出到结果集时的排序方式。
为ASC DESC 之一，分别代表“升序”“降序”。
如果没有使用<order>元素，则表示在输出检索结果的时候不专门进行排序。

<originOrder> -- 原始顺序
<originOrder>元素内的正文部分定义了检索结果输出到结果集时的排序方式。
为ASC/ DESC 之一，分别代表“升序"“降序"。
如果没有使用<originOrder>元素，则表示在输出检索结果的时候不专门进行排序。

TODO: order 和 orginOrder 有什么区别?

## <maxCount> -- 最大命中条数
<maxCount>元素内的正文部分定义了检索命中的最大条数。如果为-1，表示不限制最大命中条数。
本元素缺省时，表示不限制最大命中条数。

## <operator> -- 逻辑运算符
<operator>元素通过其属性value定义了逻辑运算符。
value属性值为OR AND SUB之一，分别代表或、与、减运算。
减运算的意思是从前一个集合中除去和后一个集合的交叉部分后剩下的部分是运算结果。

## <lang> -- 语言
<lang>元素内的正文部分定义了语言代码。
本元素缺省时，表示语言代码为"zh"。
这个元素可以出现在检索式XML结构的任何位置。

## <option> -- 选项
<option>元素定义了选项信息。这个元素可以出现在检索式XML结构的任何位置。
<option>元素的warning属性定义了警告级别数字。缺省为1。
警告级别数字的含义为：0 -- 严厉，遇到检索式信息有矛盾之处，直接出错返回；1 -- 宽容，当检索式信息出现矛盾时，系统自动做出变通修改，尽量继续进行检索。

## 记录ID的范围检索
检索词中可以使用’-’表示一个范围；<relation>元素指定range关系；<dataType>元素指定为number数据类型，如下：
<target list="中文图书:__id">
    <item>
        <word>1000-2000</word>
        <match>exact</match>
        <relation>range</relation>
        <dataType>number</dataType>
    </item>
</target>
这样就可以针对数据库的"__id"途径进行范围检索了。"__id"表示记录ID检索途径，无需在数据库的keys文件中定义，这个检索途径是数据库自含的。

还可以进行大于、小于等数量关系的检索：
<target list="中文图书:__id">
    <item>
        <word>200</word>
        <match>exact</match>
        <relation>&gt;=</relation>
        <dataType>number</dataType>
    </item>
</target>
上例中的<relation>元素中指定了>=这一数量比较关系，将检索出记录ID号大于或等于200的全部记录。


## 时间检索
如果检索途径不存在会怎么样。返回“未命中"，而不是报错。这一点在调试中要引起注意。这是为了兼容性的目的。可以通过前端获得数据库的检索途径名，来预先筛选发现。
warninglevel的具体例子

