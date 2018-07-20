using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Forms;
//
//Image screen = getScreen();                       // 截取屏幕  
//saveImage(screen, screen.Size, @"d:\截屏.jpg");   // 保存截屏 
//
namespace UART_Demo
{
    class ScreenImage
    {
        # region 图像处理功能函数

        /// <summary>
        /// 按指定尺寸对图像pic进行非拉伸缩放
        /// </summary>
        public static Bitmap shrinkTo(Image pic, Size S, Boolean cutting)
        {
            //创建图像
            Bitmap tmp = new Bitmap(S.Width, S.Height);     //按指定大小创建位图

            //绘制
            Graphics g = Graphics.FromImage(tmp);           //从位图创建Graphics对象
            g.Clear(Color.FromArgb(0, 0, 0, 0));            //清空

            Boolean mode = (float)pic.Width / S.Width > (float)pic.Height / S.Height;   //zoom缩放
            if (cutting) mode = !mode;                      //裁切缩放

            //计算Zoom绘制区域             
            if (mode)
                S.Height = (int)((float)pic.Height * S.Width / pic.Width);
            else
                S.Width = (int)((float)pic.Width * S.Height / pic.Height);
            Point P = new Point((tmp.Width - S.Width) / 2, (tmp.Height - S.Height) / 2);

            g.DrawImage(pic, new Rectangle(P, S));

            return tmp;     //返回构建的新图像
        }


        //保存图像pic到文件fileName中，指定图像保存格式
        public static void SaveToFile(Image pic, string fileName, bool replace, ImageFormat format)    //ImageFormat.Jpeg
        {
            //若图像已存在，则删除
            if (System.IO.File.Exists(fileName) && replace)
                System.IO.File.Delete(fileName);

            //若不存在则创建
            if (!System.IO.File.Exists(fileName))
            {
                if (format == null) format = getFormat(fileName);   //根据拓展名获取图像的对应存储类型

                if (format == ImageFormat.MemoryBmp) pic.Save(fileName);
                else pic.Save(fileName, format);                    //按给定格式保存图像
            }
        }

        //根据文件拓展名，获取对应的存储类型
        public static ImageFormat getFormat(string filePath)
        {
            ImageFormat format = ImageFormat.MemoryBmp;
            String Ext = System.IO.Path.GetExtension(filePath).ToLower();

            if (Ext.Equals(".png")) format = ImageFormat.Png;
            else if (Ext.Equals(".jpg") || Ext.Equals(".jpeg")) format = ImageFormat.Jpeg;
            else if (Ext.Equals(".bmp")) format = ImageFormat.Bmp;
            else if (Ext.Equals(".gif")) format = ImageFormat.Gif;
            else if (Ext.Equals(".ico")) format = ImageFormat.Icon;
            else if (Ext.Equals(".emf")) format = ImageFormat.Emf;
            else if (Ext.Equals(".exif")) format = ImageFormat.Exif;
            else if (Ext.Equals(".tiff")) format = ImageFormat.Tiff;
            else if (Ext.Equals(".wmf")) format = ImageFormat.Wmf;
            else if (Ext.Equals(".memorybmp")) format = ImageFormat.MemoryBmp;

            return format;
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorInfo(out CURSORINFO pci);

        private const Int32 CURSOR_SHOWING = 0x00000001;
        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public Int32 x;
            public Int32 y;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CURSORINFO
        {
            public Int32 cbSize;
            public Int32 flags;
            public IntPtr hCursor;
            public POINT ptScreenPos;
        }

        /// <summary>
        /// 截取屏幕指定区域为Image，保存到路径savePath下，haveCursor是否包含鼠标
        /// </summary>
        public static Image getScreen(int x = 0, int y = 0, int width = -1, int height = -1, String savePath = "", bool haveCursor = true)
        {
            if (width == -1) width = SystemInformation.VirtualScreen.Width;
            if (height == -1) height = SystemInformation.VirtualScreen.Height;

            Bitmap tmp = new Bitmap(width, height);                 //按指定大小创建位图
            Graphics g = Graphics.FromImage(tmp);                   //从位图创建Graphics对象
            g.CopyFromScreen(x, y, 0, 0, new Size(width, height));  //绘制

            // 绘制鼠标
            if (haveCursor)
            {
                try
                {
                    CURSORINFO pci;
                    pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
                    GetCursorInfo(out pci);
                    System.Windows.Forms.Cursor cur = new System.Windows.Forms.Cursor(pci.hCursor);
                    cur.Draw(g, new Rectangle(pci.ptScreenPos.x, pci.ptScreenPos.y, cur.Size.Width, cur.Size.Height));
                }
                catch (Exception) { }    // 若获取鼠标异常则不显示
            }

            //Size halfSize = new Size((int)(tmp.Size.Width * 0.8), (int)(tmp.Size.Height * 0.8));  // 按一半尺寸存储图像
            if (!savePath.Equals("")) saveImage(tmp, tmp.Size, savePath);       // 保存到指定的路径下

            return tmp;     //返回构建的新图像
        }

        /// <summary>
        /// 缩放icon为指定的尺寸，并保存到路径PathName
        /// </summary>
        public static void saveImage(Image image, Size size, String PathName)
        {
            Image tmp = shrinkTo(image, size, false);
            SaveToFile(tmp, PathName, true, null);
        }

        # endregion
    }
}
