using OffineRegistry;
namespace Offine.Test;

internal class Program
{
    static void Main(string[] args)
    {
        var systemconfig = @"./SYSTEM";
        RegistryKey.InitOffreg.InitLibrary(@"C:\Windows\System32\offreg.dll");
        using var hive3 = RegistryKey.CreateHive(systemconfig);
        using var key2 = hive3.CreateSubKey(@"xbb1.5\dsfsd");
        
        key2.SetValue("hixxxxhi", "ok2sadsadsaddssssss00");
        Console.WriteLine(key2.GetValue("hixxxxhi"));
        var newk = key2.CreateSubKey(@"xxx\sssssss\ssssssfsf\sdfsdfwefs\sadfsdfsdfsef");
        Console.WriteLine(newk.EnumerateSubKeys());
        Console.WriteLine(hive3.IsExistSubKey(@"xbb1.5\dsfsd\xxx\sssssss\ssssssfsf\sdfsdfwefs\sadfsdfsdfsef"));
        key2.SaveHive();

        //key.Close();
        /*
         * 
         *Hive 不支持修改后保存到源文件
         *
         *LOG模式
         *1. IF FILE not EXIST HIVE
         *2. Create HIVE -> CHANGE SOME
         *3. SAVE TO HIVE.LOG
         *   close
         *
         *第二次打开:
         *1. if exist hive.log rename to hive (old hive will be deleted now)
         *2. delete now hive.log
         *3. open hive(real is old hive.log)
         *4. some change
         *5. save to hive.log
         *   close
         *6. goto restart:
         *
         *rules:
         *
         * open -> hive
         * save -> hive.log
         * rename hive.log -> hive
         * 
         * if not exist hive -> first save -> hive
         *
         *自删除模式:
         *每次save前delete自身即可
         */
    }
}