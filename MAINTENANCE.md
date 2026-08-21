# SecurityCameraTG 维护说明

## 工程入口

- Unity 版本：2018.4.0f1
- 正式构建场景：`MainPreview`、`AsyncLoadScene`、`startscene`、`Empty`
- Unity 生成目录（`Library`、`Temp`、`obj`、`Logs`）不进入 Git。
- 大型原生插件、字体和视频资源由 Git LFS 管理。

## 运行配置

部署前修改：

`Assets/StreamingAssets/Configurations/AppRuntimeSettings.json`

主要配置项：

- `apiScheme` / `apiHost`：管理后端地址。
- `mqttHost` / `mqttPort` / `mqttUserName` / `mqttPassword`：MQTT 连接参数。
- `faceServiceUrl`：人脸算法服务地址。
- `sendAuthorizationHeader`：后端确认令牌格式后再启用。
- `authorizationHeader` / `authorizationScheme`：认证头名称和令牌前缀。
- `enableDebugLogs`：现场排障时临时开启，生产环境保持关闭。
- `enableLegacyMqttReceiver`：旧测试订阅器，默认关闭。
- `videoPreloadCount` / `videoPreloadPerFrame`：视频窗口对象池的总预热量和每帧预热量。

不要把客户现场的正式密码提交到 Git。交付时应由部署脚本或现场配置覆盖 MQTT 凭据。

## 权限边界

前端入口权限只负责界面可见性和操作体验，不能作为安全边界。后端必须对用户、角色、设备配置、录像回放、事件和人员管理接口逐项鉴权。

登录返回的 `auth` 已保存在进程内存中；将 `sendAuthorizationHeader` 设为 `true` 后，统一网络层会按配置发送认证头。启用前必须与后端确认是 Bearer Token、原始 Token，还是其他格式。

## Unity 升级

不要直接覆盖升级当前交付工程。应在独立分支中按以下顺序执行：

1. 盘点海康/大华 SDK、AVPro、UMP、Vuplex 和 MQTT 插件的目标 Unity 兼容版本。
2. 固化登录、四个构建场景、64 路预览、回放、门禁事件和用户权限的回归用例。
3. 先升级第三方插件，再升级 Unity；每个阶段单独提交并生成可运行包。
4. 使用 Unity Profiler 对主线程、GC、渲染和视频解码分别建立升级前后基线。
