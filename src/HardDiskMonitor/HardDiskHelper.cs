using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardDiskMonitor
{
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
}
