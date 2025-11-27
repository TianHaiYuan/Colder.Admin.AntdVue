using QRCoder;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.Versioning;

namespace Coldairarrow.Util
{
    /// <summary>
    /// 二维码生成帮助类
    /// </summary>
    public class QRCodeHelper
    {
        #region 生成二维码

        /// <summary>
        /// 生成二维码，默认边长为250px
        /// </summary>
        /// <param name="content">二维码内容</param>
        /// <returns></returns>
        [SupportedOSPlatform("windows")]
        public static Image BuildQRCode(string content)
        {
            return BuildQRCode(content, 250, Color.White, Color.Black);
        }

        /// <summary>
        /// 生成二维码,自定义边长
        /// </summary>
        /// <param name="content">二维码内容</param>
        /// <param name="imgSize">二维码边长px</param>
        /// <returns></returns>
        [SupportedOSPlatform("windows")]
        public static Image BuildQRCode(string content, int imgSize)
        {
            return BuildQRCode(content, imgSize, Color.White, Color.Black);
        }

        /// <summary>
        /// 生成二维码
        /// 注：自定义边长以及颜色
        /// </summary>
        /// <param name="content">二维码内容</param>
        /// <param name="imgSize">二维码边长px</param>
        /// <param name="background">二维码底色</param>
        /// <param name="foreground">二维码前景色</param>
        /// <returns></returns>
        [SupportedOSPlatform("windows")]
        public static Image BuildQRCode(string content, int imgSize, Color background, Color foreground)
        {
            return BuildQRCode_Logo(content, imgSize, background, foreground, null);
        }

        /// <summary>
        /// 生成二维码并添加Logo
        /// 注：默认生成边长为250px的二维码
        /// </summary>
        /// <param name="content">二维码内容</param>
        /// <param name="logo">logo图片</param>
        /// <returns></returns>
        [SupportedOSPlatform("windows")]
        public static Image BuildQRCode_Logo(string content, Bitmap logo)
        {
            return BuildQRCode_Logo(content, 250, Color.White, Color.Black, logo);
        }

        /// <summary>
        /// 生成二维码并添加Logo
        /// 注：自定义边长
        /// </summary>
        /// <param name="content">二维码内容</param>
        /// <param name="imgSize">二维码边长px</param>
        /// <param name="logo">logo图片</param>
        /// <returns></returns>
        [SupportedOSPlatform("windows")]
        public static Image BuildQRCode_Logo(string content, int imgSize, Bitmap logo)
        {
            return BuildQRCode_Logo(content, imgSize, Color.White, Color.Black, logo);
        }

        /// <summary>
        /// 生成二维码并添加Logo
        /// 注：自定义边长以及颜色
        /// </summary>
        /// <param name="content">二维码内容</param>
        /// <param name="imgSize">二维码边长px</param>
        /// <param name="background">二维码底色</param>
        /// <param name="foreground">二维码前景色</param>
        /// <param name="logo">logo图片</param>
        /// <returns></returns>
        [SupportedOSPlatform("windows")]
        public static Image BuildQRCode_Logo(string content, int imgSize, Color background, Color foreground, Bitmap logo)
        {
            // 使用 PngByteQRCode 生成二维码字节数组，然后转换为 Bitmap
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            var ppm = imgSize / qrCodeData.ModuleMatrix.Count;
            var qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qrCode.GetGraphic(ppm);

            using (var ms = new MemoryStream(qrCodeBytes))
            {
                Bitmap qrCodeImage = new Bitmap(ms);

                // 如果有 logo，将其绘制到二维码中心
                if (logo != null)
                {
                    using (Graphics g = Graphics.FromImage(qrCodeImage))
                    {
                        int logoSize = qrCodeImage.Width / 5;
                        int logoX = (qrCodeImage.Width - logoSize) / 2;
                        int logoY = (qrCodeImage.Height - logoSize) / 2;
                        g.DrawImage(logo, logoX, logoY, logoSize, logoSize);
                    }
                }

                return qrCodeImage;
            }
        }

        /// <summary>
        /// 生成二维码PNG字节数组（跨平台）
        /// </summary>
        /// <param name="content">二维码内容</param>
        /// <param name="pixelsPerModule">每个模块的像素数</param>
        /// <returns>PNG格式的字节数组</returns>
        public static byte[] BuildQRCodeBytes(string content, int pixelsPerModule = 20)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(pixelsPerModule);
        }

        #endregion

        #region 生成条形码

        /// <summary>
        /// 生成条形码
        /// 注：默认宽150px,高50px
        /// </summary>
        /// <param name="content">条形码内容</param>
        /// <returns></returns>
        [SupportedOSPlatform("windows")]
        public static Image BuildBarCode(string content)
        {
            return BuildBarCode(content, 150, 50);
        }

        /// <summary>
        /// 生成条形码
        /// 注：自定义尺寸
        /// </summary>
        /// <param name="content">条形码内容</param>
        /// <param name="width">宽度px</param>
        /// <param name="height">高度px</param>
        /// <returns></returns>
        [SupportedOSPlatform("windows")]
        public static Image BuildBarCode(string content, int width, int height)
        {
            throw new Exception("暂不支持!");
        }

        #endregion

        #region 读取码内容

        /// <summary>
        /// 从二维码读取内容
        /// </summary>
        /// <param name="image">二维码</param>
        /// <returns></returns>
        [SupportedOSPlatform("windows")]
        public static string ReadContent(Bitmap image)
        {
            throw new Exception("暂不支持!");
        }

        #endregion
    }
}
