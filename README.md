# OffineRegistry
---

## 这个库修改自offreg


这个库在新版本中追随原生体验, 因此重写了相关方法;

//1.0.4版本增加了InitOffreg, 你必须先调用他加载offreg.dll, 然后才可以调用加载hive

例如: RegistryKey.InitOffreg.InitLibrary(@"X:\Windows\System32\offreg.dll");

//如果你不想判断subkey是否存在, 请直接createsubkey, 因为创建前会先尝试打开

//1.0.4支持多路径了: 

例如: key2.CreateSubKey(@"xxx\sssssss\ssssssfsf\sdfsdfwefs\sadfsdfsdfsef");



---

使用方法与offreg一样, 值得注意的是, SaveHive方法中的版本非常依赖offreg.dll的版本:

e.g. offreg version = 6.2 (win7)

此时调用 savehive(10, 0); 将会抛出异常(因为这是win10的版本号), 正确做法是执行前复制一份目标操作注册表单元'System32\offreg.dll', 这样可以确保顺利保存

(x-已经重写save方法)[另外保存hive不能存在已有文件,原地保存可以先删除(打开时全部载入内存, 不占用文件)后保存同名文件即可]
新版本会自动删除, 如果保存失败直接退出将永久丢失文件, 请确认保存成功再退出, 因为hive数据库是全部载入内存, 只要内存还在就可以保存到别的地方
