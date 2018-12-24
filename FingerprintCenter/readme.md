# FingerprintCenter - 指纹中心

本模块是 dp2 系统指纹识别的接口模块。适配中控指纹仪各型号，包括：zk-4500, zk-7500, Live 20R, Live 10R。

本模块能直接和 dp2library 通讯，从 dp2library 服务器获取读者指纹信息，并创建指纹高速缓存，实现指纹识别功能，将识别到的读者证条码号发送给当前焦点窗口。

本模块还可与 dp2circulation (内务)模块协同，为之提供读者指纹登记功能。

本模块可取代早先的 zkfingerprint 模块。

关于本模块的使用，可参考：
https://github.com/DigitalPlatform/dp2/issues/222
