
SampleCardCenter
卡中心接口模块样例程序

本程序示范了如何编写一个和卡中心进行通讯的接口程序。程序是一个 Console 程序。

本程序是一个 .NET Remoting Server，dp2library 模块在需要进行读者信息同步的时候，会用 .NET Remoting 方式请求本程序。这时 dp2library 模块相当于本程序的 Client 角色，本程序是 Server 角色。

此外，dp2circulation(内务)前端在需要对读者进行扣款的时候，也可能直接请求本程序。