# OnceMi.Framework
基于.NET 5和Vue开发的企业级前后端分离权限管理开发框架（后台管理系统），具有组织管理、角色管理、用户管理、菜单管理、授权管理、计划任务、文件管理等功能。支持国内外多种流行数据库，支持IdentityServer4统一认证。  

### 特色
- 前端界面美观大方，支持主题切换，夜间模式等，前端基于[vue-antd-admin](https://github.com/iczer/vue-antd-admin "vue-antd-admin")开发
- 采用FreeSql，支持Sqlite/MySQL/PostgreSQL/SQLServer/Oracle等多种流行数据库(Oracle未测试)
- 基于仓储模式开发
- 支持本地认证和IdentityServer4统一认证（可随意切换）
- 基于角色的权限控制
- 后端不做过多封装，小白也能轻松上手
- 多层开发，结构清晰，封装完善，易于扩展
- 支持AOP面向切面开发
- 支持AOP数据库事务，AOP缓存管理（缓存管理仅清除，可自行实现完整的缓存管理）
- 支持分布式Redis
- 支持Redis和RabbitMQ消息队列，且设计了简单快捷的订阅和发布机制
- 支持任务调度，作业管理（基于Quartz.net）。即使把本框架仅作为一个定时任务管理器，也是很不错的。
- 支持健康检查
- 支持Service层和Repository自动注入
- 支持自动依赖注入
- 统一文件管理，支持上传文件至本地和OSS（支持Minio，腾讯云，阿里云，基于[OnceMi.AspNetCore.OSS](https://github.com/oncemi/OnceMi.AspNetCore.OSS "OnceMi.AspNetCore.OSS")）
- 使用Automapper处理对象映射
- 支持组织管理、角色管理、用户管理、菜单管理、授权管理等基本功能
- 得益于.NET Core的跨平台特性，支持Linux、Windows、OSX。你甚至可以将此框架运行在树莓派上面。

### 预览
Demo：https://ofw.demo.oncemi.com/  
用户名：test  
密码：123456  
  
Swagger UI：https://ofw-api.demo.oncemi.com/sys/swagger-ui/index.html  
HealthCheck UI：https://ofw-api.demo.oncemi.com/sys/health-ui  
IdentityServer4认证中心：https://ids4.demo.oncemi.com/  

### 文档地址
文档地址：https://doc.oncemi.com/web/#/5  
一个高质量的开源项目不仅仅体现在代码和设计上面，也体现在配套的文档中。详细的文档才能让使用者知其然再知其所以然，如果有描述模糊的地方，还请提交iusse，我们将尽快更新。  

### 项目结构
![](https://raw.githubusercontent.com/oncemi/OnceMi.Framework/main/docs/imgs/01.png)  

### 支持
鼓励自己手动解决问题:+1:，如果是本项目问题，还请麻烦提交Issue（能提交pull就更好啦:smile:），大家一起交流进步。  
QQ交流群：460481440  

### 捐赠
<center class="half">
    <img src="https://raw.githubusercontent.com/oncemi/OnceMi.Framework/main/docs/imgs/02.png" width = "630" height = "300" alt="图片名称" align=center />
</center>
 
