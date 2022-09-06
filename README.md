@[TOC]

##  效果图
![在这里插入图片描述](https://img-blog.csdnimg.cn/697508b201354e0da0c6f6b1b81daeab.png)

![在这里插入图片描述](https://img-blog.csdnimg.cn/c6a7f7b54b74461b9743a2afdf74237d.png)

## 功能
- 开机启动
	支持设置开机启动，下次工控机或者电脑启动后就能够自动检测
- 指定监控磁盘
	支持监控指定的盘符（目前不支持多盘符，源码在下面，你可以根据自己的需要调整代码）
- 设置阈值
	支持当磁盘使用容量达到一定数值后（百分比）进行系统通知
- 任务托盘
	支持托盘图标

- 更多功能你们在源码中进行添加吧，如果可以，推送源码，我们一起完善这个工具的功能。

源码地址：
	[https://github.com/iml6yu-tool/HardDiskMonitor](https://github.com/iml6yu-tool/HardDiskMonitor)

## 部分源码介绍
### HardDiskHelper 
- 磁盘监控帮助类

```csharp
public class HardDiskHelper
    {
        /// <summary>
        /// 获取硬盘已经使用百分比
        /// </summary>
        /// <param name="hardDiskName"></param>
        /// <returns></returns>
        public static float GetUsedPercnet(string hardDiskName)
        {
            var total = GetHardDiskSpace(hardDiskName);
            var free =  GetHardDiskFreeSpace(hardDiskName);
            var result = (total- free) * 1f / total;
            return result;
        }

        /// <summary>
        /// 获取指定驱动器的空间总大小(单位为GB) 
        /// </summary>
        /// <param name="hardDiskName">只需输入代表驱动器的字母即可</param>
        /// <returns></returns>
        public static long GetHardDiskSpace(string hardDiskName)
        {
            long totalSize = new long(); 
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (drive.Name == hardDiskName)
                {
                    totalSize = drive.TotalSize / (1024 * 1024 * 1024);
                }
            }
            return totalSize;
        }


        /// <summary>
        /// 获取指定驱动器的剩余空间总大小(单位为GB)
        /// </summary>
        /// <param name="hardDiskName">只需输入代表驱动器的字母即可</param>
        /// <returns></returns>
        public static long GetHardDiskFreeSpace(string hardDiskName)
        {
            long freeSpace = new long(); 
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (drive.Name == hardDiskName)
                {
                    freeSpace = drive.TotalFreeSpace / (1024 * 1024 * 1024);
                }
            }
            return freeSpace;
        }

    }
```

### SettingHelper 
- 开机启动帮助类

```csharp
 public class SettingHelper
    {
        /// <summary>
        /// 将本程序设为开启自启
        /// </summary>
        /// <param name="onOff">自启开关</param>
        /// <returns></returns>
        public static bool SetMeStart(bool onOff)
        {
            bool isOk = false;
            string appName = Process.GetCurrentProcess().MainModule.ModuleName;
            string appPath = Process.GetCurrentProcess().MainModule.FileName;
            isOk = SetAutoStart(onOff, appName, appPath);
            return isOk;
        }

        /// <summary>
        /// 将应用程序设为或不设为开机启动
        /// </summary>
        /// <param name="onOff">自启开关</param>
        /// <param name="appName">应用程序名</param>
        /// <param name="appPath">应用程序完全路径</param>
        public static bool SetAutoStart(bool onOff, string appName, string appPath)
        {
            bool isOk = true;
            //如果从没有设为开机启动设置到要设为开机启动
            if (!IsExistKey(appName) && onOff)
            {
                isOk = SelfRunning(onOff, appName, @appPath);
            }
            //如果从设为开机启动设置到不要设为开机启动
            else if (IsExistKey(appName) && !onOff)
            {
                isOk = SelfRunning(onOff, appName, @appPath);
            }
            return isOk;
        }

        /// <summary>
        /// 判断注册键值对是否存在，即是否处于开机启动状态
        /// </summary>
        /// <param name="keyName">键值名</param>
        /// <returns></returns>
        public static bool IsExistKey(string keyName)
        {
            try
            {
                bool _exist = false;
                RegistryKey local = Registry.LocalMachine;
                RegistryKey runs = local.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (runs == null)
                {
                    RegistryKey key2 = local.CreateSubKey("SOFTWARE");
                    RegistryKey key3 = key2.CreateSubKey("Microsoft");
                    RegistryKey key4 = key3.CreateSubKey("Windows");
                    RegistryKey key5 = key4.CreateSubKey("CurrentVersion");
                    RegistryKey key6 = key5.CreateSubKey("Run");
                    runs = key6;
                }
                string[] runsName = runs.GetValueNames();
                foreach (string strName in runsName)
                {
                    if (strName.ToUpper() == keyName.ToUpper())
                    {
                        _exist = true;
                        return _exist;
                    }
                }
                return _exist;

            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 写入或删除注册表键值对,即设为开机启动或开机不启动
        /// </summary>
        /// <param name="isStart">是否开机启动</param>
        /// <param name="exeName">应用程序名</param>
        /// <param name="path">应用程序路径带程序名</param>
        /// <returns></returns>
        private static bool SelfRunning(bool isStart, string exeName, string path)
        {
            try
            {
                RegistryKey local = Registry.LocalMachine;
                RegistryKey key = local.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (key == null)
                {
                    local.CreateSubKey("SOFTWARE//Microsoft//Windows//CurrentVersion//Run");
                }
                //若开机自启动则添加键值对
                if (isStart)
                {
                    key.SetValue(exeName, path);
                    key.Close();
                }
                else//否则删除键值对
                {
                    string[] keyNames = key.GetValueNames();
                    foreach (string keyName in keyNames)
                    {
                        if (keyName.ToUpper() == exeName.ToUpper())
                        {
                            key.DeleteValue(exeName);
                            key.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string ss = ex.Message;
                return false;
                //throw;
            }

            return true;
        }
    }
```

## 最后
这是一个用了2个小时写完的小工具，很多命名都不符合规则，但是着急使用，所以如果当你在看源代码的时候出现混沌的情况，请关注公众号回复 "联系作者" ，能够得到更多的介绍和说明

![在这里插入图片描述](https://img-blog.csdnimg.cn/40ab195c066c46e7b849252d67aab45a.png)
