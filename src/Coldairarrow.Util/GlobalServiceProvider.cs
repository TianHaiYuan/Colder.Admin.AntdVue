using System;

namespace Coldairarrow.Util
{
    /// <summary>
    /// 全局服务提供者
    /// </summary>
    public static class GlobalServiceProvider
    {
        /// <summary>
        /// 服务提供者实例
        /// </summary>
        public static IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// 设置服务提供者
        /// </summary>
        /// <param name="serviceProvider">服务提供者</param>
        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
    }
}

