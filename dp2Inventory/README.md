# dp2Inventory -- 盘点模块

支持 SIP2 协议和 dp2library 协议。

## dp2library 协议

在 dp2library 协议下，可以从 dp2library 服务器获取册记录，并将盘点后修改的字段更新到 dp2library 服务器，不会产生信息孤岛问题。

## SIP2 协议全功能/半功能方式

SIP2 协议下，可以支持全功能和半功能两种方式：

* 全功能：从 SIP2 服务器获取册记录，盘点后把永久架位和当前架位字段更新到 SIP2 服务器。更新使用了扩充的 SIP2 接口 ItemStatusUpdate(19/20)

* 半功能：从 SIP2 服务器获取册记录，但并不把修改了永久架位和当前架位的册记录更新到 SIP2 服务器，而是保存到本地数据库中

全功能的 SIP2 服务器，例子之一是 dp2 系统的 dp2capo 服务器。半功能的 SIP2 服务器的例子是大多数市面上的 SIP2 服务器

ItemStatusUpdate 接口的详细信息可参考：
https://github.com/DigitalPlatform/dp2/issues/756

## 册记录上传接口

支持用上传接口把盘点后的册记录信息上传到外部 Web Server。

接口定义可参考如下 Project:
https://github.com/renyh/InventoryAPI

这是一个示范接口的样例 Web Server，可使用它的 swagger.json 文件来获得接口定义

上传接口尤其对前述 SIP2 半功能方式有用，因为半功能状态下，SIP2 服务器一端无法获得更新的信息，可以通过建立一个容纳上传信息的 Web Server 来进行补充。
当然，SIP2 服务器能支持全功能方式是最好的，可以避免产生信息孤岛问题。

## 手持盘点设备的 B/L 模式切换(标签类型切换)

实际上 B/L 模式切换就是在 dp2Inventory 开发过程中，由开发小组建议后新增的一种设备模式

B/L 模式切换可以避免读图书标签的时候被偶然读入的层架标干扰；和避免读层架标的时候被偶然读入的图书标签所干扰