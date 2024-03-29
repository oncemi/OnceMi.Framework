﻿using System.ComponentModel;

namespace OnceMi.Framework.Model.Common
{
    public enum ResultCode
    {
        #region 必须模块

        #region Others

        [Description("未知的系统错误")]
        Unknow = 30000,

        #endregion

        #region Account

        [Description("当前应用启用了IdentityServer认证中心，此功能被禁用")]
        ACT_FUNCTION_DISABLED_NOT_SUPPORT = 30001,

        [Description("用户名或密码错误")]
        ACT_USERNAME_OR_PASSWORD_ERROR = 30003,

        [Description("获取秘钥失败，无效的请求秘钥")]
        ACT_REFESH_TOKEN_FAILED = 30005,

        [Description("用户被禁用")]
        ACT_USER_DISABLED = 30007,

        [Description("Token已过期，请重新登录")]
        ACT_REFESH_TOKEN_TIMEOUT = 30009,

        [Description("用户未登录")]
        ACT_GET_USERID_FAILED = 30010,

        #endregion

        #region API

        [Description("父条目不存在")]
        API_PARENT_ENTRY_DOES_NOT_EXIST = 30101,

        [Description("添加的API版本必须和父节点版本相同")]
        API_MUST_THE_SAME_AS_PARENT = 30103,

        [Description("API已存在")]
        API_EXISTS = 30105,

        [Description("修改的条目不存在")]
        API_FOR_CURRENT_NOT_EXISTS = 30107,

        [Description("父条目不存在")]
        API_FOR_PARENT_NOT_EXISTS = 30109,

        [Description("没有要删除的条目")]
        API_FOR_DELETE_NOT_EISTS = 30111,

        [Description("从SwaggerGeneratorOptions中找不到对应的OpenApi选项")]
        API_CANNOT_FIND_OPEN_API_OPTION = 30113,

        [Description("找不到当前API的父节点")]
        API_CANNOT_FIND_CURRENTAPI_PARENTS = 30115,

        [Description("Api版本必须使用‘v’开头")]
        API_VERSION_NAME_MUST_STARTWITH_V = 30117,

        [Description("Api版本号必须为数字")]
        API_VERSION_MUST_NUMBER = 30119,

        [Description("不允许的操作类型")]
        API_NOT_SUPPORT_REQUEST_TYPE = 30121,

        [Description("请求路径不能为根目录")]
        API_REQUEST_PATH_CANNOT_ROOT = 30123,

        [Description("当Api不为个根节点（控制器）时，请求方式不能为空")]
        API_NOT_ROOT_REQUEST_METHOD_CANNOT_NULL = 30125,

        #endregion

        #region Dictionary

        [Description("查询条件Id和编码不能同时为空")]
        DIC_ID_AND_CODE_CANNOT_ALL_EMPTY = 30401,

        [Description("根据编码查询字典信息失败")]
        DIC_QUERY_BY_CODE_FAILED = 30403,

        [Description("查询字典信息失败")]
        DIC_QUERY_FAILED = 30405,

        [Description("父条目不存在")]
        DIC_PARENT_NOT_EXIST = 30407,

        [Description("当前子目录下Code已存在")]
        DIC_CODE_EXISTS_IN_PATH = 30409,

        [Description("当前子目录下Name已存在")]
        DIC_NAME_EXISTS_IN_PATH = 30411,

        [Description("修改的条目不存在")]
        DIC_UPDATE_ITEM_NOTEXISTS = 30413,

        [Description("没有要删除的条目")]
        DIC_DELETE_NOT_EISTS = 30415,

        #endregion

        #region File

        [Description("请求文件key不能为空")]
        FILE_REQUEST_KEY_CANNOT_NULL = 30501,

        [Description("对不起，您没有改文件的访问权限")]
        FILE_NO_DOWNLOAD_PERMISSION = 30503,

        [Description("上传的文件不能为空")]
        FILE_UPLOAD_IS_NULL = 30505,

        [Description("获取登录用户信息失败")]
        FILE_GET_USERINFO_FAILED = 30507,

        [Description("获取文件参数失败，找不到指定的文件路径")]
        FILE_CANNOT_FIND_FILE_PATH = 30509,

        [Description("该文件已过期")]
        FILE_EXPIRED = 30511,

        [Description("获取文件参数失败，找不到指定的文件存储位置")]
        FILE_CANNOT_FIND_LOCALTION = 30513,

        [Description("未知的文件存储类型")]
        FILE_UNKNOW_STORAGE_TYPE = 30515,

        [Description("文件上传失败")]
        FILE_UPLOAD_FAILED = 30517,

        [Description("文件流为空")]
        FILE_STREAM_IS_NULL = 30519,

        [Description("无法读取文件流")]
        FILE_STREAM_CANNOT_READ = 30521,

        [Description("无法获取历史文件名称")]
        FILE_OLDNAME_CANNOT_READ = 30523,

        [Description("上传路径不能为空")]
        FILE_UPLOAD_PATH_IS_NULL = 30525,

        [Description("获取文件类型失败")]
        FILE_CONTENTTYPE_CANNOT_GET = 30527,

        [Description("上传文件不存在")]
        FILE_UPLOAD_NOT_EXISTS = 30529,

        [Description("上传文件到对象储存失败")]
        FILE_UPLOAD_TO_OSS_FAILED = 30531,

        [Description("文件密钥验证失败")]
        FILE_KEY_DEC_FAILED = 30533,

        [Description("文件密钥验证失败")]
        FILE_KEY_FORMAT_FAILED = 30535,

        [Description("请求token不能为空")]
        FILE_TOKEN_IS_NULL = 30537,

        [Description("请求token验证失败")]
        FILE_TOKEN_INVALID = 30539,

        [Description("请求token已过期")]
        FILE_TOKEN_EXPIRED = 30539,

        [Description("不支持的文件访问权限")]
        FILE_PERMISSION_UNSUPPORT = 30541,

        #endregion

        #region JobGroup

        [Description("分组名称已存在")]
        JOBG_NAME_EXISTS = 30601,

        [Description("分组编码已存在")]
        JOBG_CODE_EXISTS = 30603,

        [Description("分组编码格式不正确，编码只能由字母、数组和下划线组成")]
        JOBG_CODE_FORMAT_ERROR = 30605,

        [Description("修改的分组不存在")]
        JOBG_NOT_EXISTS = 30607,

        [Description("没有要删除的条目")]
        JOBG_DELETE_NOT_EXISTS = 30609,

        [Description("分组正在使用，无法删除")]
        JOBG_IN_USED = 30611,

        #endregion

        #region Job

        [Description("Header必须是合法的Json字符串")]
        JOB_HEADER_MUST_JSON = 30801,

        [Description("请求参数必须是合法的Json字符串")]
        JOB_PARAMS_MUST_JSON = 30803,

        [Description("错误的Cron表达式")]
        JOB_CRON_ERROR = 30805,

        [Description("任务结束时间不能小于当前时间")]
        JOB_END_TIME_LOWER_THAN_NOW = 30807,

        [Description("任务结束时间必须大于任务开始时间")]
        JOB_END_TIME_LOWER_THAN_START = 30809,

        [Description("不允许的操作类型")]
        JOB_UNKNOW_OPERATION_TYPE = 30811,

        [Description("当前任务不存在")]
        JOB_NOT_EXISTS = 30813,

        [Description("重新加载任务过程中出现了错误，无法获取任务数据")]
        JOB_LOAD_ERROR = 30815,

        [Description("当前任务已停止")]
        JOB_STOPPED = 30817,

        [Description("任务已过期")]
        JOB_OUT_OF_DATE = 30819,

        [Description("所选分组不存在")]
        JOB_GROUP_NOT_EXISTS = 30821,

        [Description("保存任务信息到数据库失败")]
        JOB_DATA_SAVE_ERROR = 30823,

        [Description("修改的条目不存在")]
        JOB_UPDATE_ITEM_NOT_EXISTS = 30825,

        [Description("没有要删除的条目")]
        JOB_DELETE_ITEM_NOT_EXISTS = 30827,

        [Description("任务未停止")]
        JOB_IS_RUNNING = 30829,

        #endregion

        #region Menu

        [Description("获取用户角色失败")]
        MENU_GET_ROLE_FAILED = 30901,

        [Description("当才菜单类型为接口时，接口不能为空")]
        MENU_API_CANNOT_NULL = 30903,

        [Description("当才菜单类型为视图或分组时，视图不能为空")]
        MENU_VIEW_CANNOT_NULL = 30905,

        [Description("未知的菜单类型")]
        MENU_UNKNOW_TYPE = 30907,

        [Description("父目录不存在")]
        MENU_PARENTS_NOT_EXISTS = 30909,

        [Description("接口不能作为父目录")]
        MENU_API_CANNOT_AS_PARENT = 30911,

        [Description("指定的视图不存在")]
        MENU_VIEW_NOT_EXISTS = 30913,

        [Description("指定的Api不存在")]
        MENU_API_NOT_EXISTS = 30915,

        [Description("修改的条目不存在")]
        MENU_NOT_EXISTS = 30917,

        [Description("没有要删除的条目")]
        MENU_DELETE_NOT_EXISTS = 30919,

        #endregion

        #region Organize

        [Description("父条目不存在")]
        ORG_PARENTS_NOT_EXISTS = 31001,

        [Description("修改的条目不存在")]
        ORG_UPDATE_NOT_EXISTS = 31003,

        [Description("修改的条目不存在")]
        ORG_PARENT_CANNOT_SELF = 31005,

        [Description("没有要删除的条目")]
        ORG_DELETE_NOT_EXISTS = 31007,

        [Description("当前组织机构下包含未删除的角色组")]
        ORG_HAS_ROLES = 31009,

        [Description("当前组织机构下包含启用的用户")]
        ORG_HAS_USERS = 31011,

        [Description("所选部门负责人不正确")]
        ORG_SELECT_DL_ERROR = 31013,

        [Description("所选分管领导不正确")]
        ORG_SELECT_HL_ERROR = 31015,

        #endregion

        #region Permission

        [Description("获取用户角色失败")]
        PERM_GET_ROLE_FAILED = 31101,

        [Description("更新的角色不存在或已被禁用")]
        PERM_UPDATE_ROLE_NOT_EXISTS = 31103,

        #endregion

        #region Role

        [Description("获取用户角色失败")]
        ROLE_PARENTS_NOT_EXISTS = 31201,

        [Description("当前添加/修改的角色编码已存在")]
        ROLE_CODE_EXISTS = 31203,

        [Description("所选组织不存在或已被停用")]
        ROLE_ORGANIZES_NOT_EXISTS = 31205,

        [Description("修改的条目不存在")]
        ROLE_UPDATE_NOT_EXISTS = 31207,

        [Description("没有要删除的条目")]
        ROLE_DELETE_NOT_EXISTS = 31209,

        [Description("角色已经被分配至用户，无法删除")]
        ROLE_USERD = 31211,

        #endregion

        #region User

        [Description("用户名只能由数字和字母组成")]
        USER_NAME_INVALID = 31301,

        [Description("用户名已存在")]
        USER_NAME_EXISTS = 31303,

        [Description("当前电话号码已被注册")]
        USER_PHONE_EXISTS = 31305,

        [Description("当前邮箱已被注册")]
        USER_EMAIL_EXISTS = 31307,

        [Description("修改的用户不存在")]
        USER_UPDATE_NOT_EXISTS = 31309,

        [Description("旧密码错误")]
        USER_OLD_PWD_INVALID = 31311,

        [Description("没有要删除的条目")]
        USER_DELETE_NOT_EXISTS = 31313,

        [Description("未知的用户状态")]
        USER_UNKNOW_STATUS = 31315,

        #endregion

        #region View

        [Description("参数必须是合法的Json字符串")]
        VIEW_PARAMS_MUST_JSON = 31401,

        [Description("父条目不存在")]
        VIEW_PARENTS_NOT_EXISTS = 31403,

        [Description("修改的条目不存在")]
        VIEW_UPDATE_NOT_EXISTS = 31405,

        [Description("修改的条目不存在")]
        VIEW_PARENTS_CANNOT_SELF = 31407,

        [Description("没有要删除的条目")]
        VIEW_DELETE_NOT_EXISTS = 31409,

        #endregion

        #region Config

        [Description("参数KEY不能为空")]
        CONFIG_KEY_CANNOT_NULL = 31501,

        [Description("未找到配置项")]
        CONFIG_CANNOT_FING_KEY = 31501,

        #endregion

        #endregion

        #region 非必须模块

        #region AriticleCategory

        [Description("父亲条目不存在")]
        ARITICLECATEGORY_PARENTS_NOT_EXISTS = 40101,

        [Description("更新的条目不存在")]
        ARITICLECATEGORY_UPDATE_NOT_EXISTS = 40103,

        [Description("添加的分类名称已存在")]
        ARITICLECATEGORY_NAME_EXISTS = 40105,

        [Description("没有要删除的条目")]
        ARITICLECATEGORY_DELETE_NOT_EXISTS = 40107,

        [Description("当前分组为锁定分组，无法被删除")]
        ARITICLECATEGORY_IS_LOCKED = 40109,

        #endregion

        #region 文章模块

        [Description("文章分类不能为空")]
        ARTICLE_CATEGORY_CANNOT_NULL = 40001,

        [Description("数据库中无可用文章分类")]
        ARTICLE_NO_CATEGORY_IN_DB = 40003,

        [Description("所选文章分类不存在")]
        ARTICLE_CATEGORY_NOT_EXIST = 40005,

        [Description("所选文章分类已经被禁用")]
        ARTICLE_CATEGORY_DISABLED = 40007,

        [Description("数据库中无可用文章分类")]
        ARTICLE_ILLEGAL_TAG = 40009,

        [Description("标签长度不能大于20")]
        ARTICLE_TAG_TOO_LONG = 40011,

        [Description("查询的文章不存在")]
        ARTICLE_QUERY_NOT_EXIST = 40013,

        [Description("更新的文章不存在")]
        ARTICLE_UPDATE_NOT_EXIST = 40015,

        [Description("删除的文章不存在")]
        ARTICLE_DELETE_NOT_EXIST = 40017,

        [Description("更新文章时文章Id不能为空")]
        ARTICLE_ID_CANNOT_NULL = 40019,

        #endregion

        #endregion
    }
}
