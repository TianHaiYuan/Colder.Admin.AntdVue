import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'
import Components from 'unplugin-vue-components/vite'
import AutoImport from 'unplugin-auto-import/vite'
import { AntDesignVueResolver } from 'unplugin-vue-components/resolvers'
import { resolve } from 'path'

export default defineConfig(({ mode }) => {
  // 加载所有 VUE_APP_ 开头的环境变量
  const env = loadEnv(mode, process.cwd(), '')

  // 定义 process.env 对象，每个值需要是合法的 JS 表达式字符串
  const processEnv = {
    NODE_ENV: mode === 'production' ? 'production' : 'development',
    VUE_APP_PREVIEW: env.VUE_APP_PREVIEW || 'false',
    VUE_APP_ProjectName: env.VUE_APP_ProjectName || 'Colder框架',
    VUE_APP_DesktopPath: env.VUE_APP_DesktopPath || '/Home/Introduce',
    VUE_APP_PublishRootUrl: env.VUE_APP_PublishRootUrl || 'http://localhost:5000',
    VUE_APP_LocalRootUrl: env.VUE_APP_LocalRootUrl || 'http://localhost:5000',
    VUE_APP_ApiTimeout: env.VUE_APP_ApiTimeout || '10000',
    VUE_APP_DevPort: env.VUE_APP_DevPort || '5001',
    VUE_APP_AppId: env.VUE_APP_AppId || 'PcAdmin',
    VUE_APP_AppSecret: env.VUE_APP_AppSecret || 'wtMaiTRPTT3hrf5e',
  }

  return {
    plugins: [
      vue(),
      AutoImport({
        imports: ['vue', 'vue-router', 'pinia'],
        dts: 'src/auto-imports.d.ts',
      }),
      Components({
        resolvers: [
          AntDesignVueResolver({
            importStyle: false, // css in js
          }),
        ],
        dts: 'src/components.d.ts',
      }),
    ],
    define: {
      // 兼容使用 process.env 的代码，整个对象 stringify
      'process.env': JSON.stringify(processEnv),
    },
    resolve: {
      alias: {
        '@': resolve(__dirname, 'src'),
        '@$': resolve(__dirname, 'src'),
      },
      extensions: ['.mjs', '.js', '.ts', '.jsx', '.tsx', '.json', '.vue'],
    },
    css: {
      preprocessorOptions: {
        less: {
          javascriptEnabled: true,
          modifyVars: {
            // 自定义主题变量
            // 'primary-color': '#1890ff',
          },
        },
      },
    },
    server: {
      port: parseInt(env.VUE_APP_DevPort) || 5001,
      host: true,
    },
    build: {
      sourcemap: false,
      chunkSizeWarningLimit: 2000,
    },
  }
})

