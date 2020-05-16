﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hos.ScheduleMaster.Core;
using Hos.ScheduleMaster.Core.Common;
using Hos.ScheduleMaster.Core.Dto;
using Hos.ScheduleMaster.Core.Interface;
using Hos.ScheduleMaster.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hos.ScheduleMaster.Web.ApiControllers
{
    [Route("api/[controller]/[action]")]
    public class TaskController : ApiController
    {
        [Autowired]
        public IScheduleService _scheduleService { get; set; }

        /// <summary>
        /// 查询分页数据
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        public object QueryList(string name = "")
        {
            var pager = new ListPager<ScheduleInfo>(PageIndex, PageSize);
            if (!string.IsNullOrEmpty(name))
            {
                pager.AddFilter(m => m.Title.Contains(name));
            }
            pager = _scheduleService.QueryPager(pager, null);
            var result = new
            {
                total = pager.Total,
                rows = pager.Rows
            };
            return result;
        }

        /// <summary>
        /// 查询详细信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ServiceResponseMessage QueryTaskDetail(Guid id)
        {
            var entity = _scheduleService.QueryById(id);
            return ApiResponse(ResultStatus.Success, "请求数据成功", entity);
        }


        /// <summary>
        /// 创建任务并启动
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        [HttpPost]
        public ServiceResponseMessage Create([FromForm]ScheduleInfo task)
        {
            if (!ModelState.IsValid)
            {
                return ApiResponse(ResultStatus.ParamError, "非法提交参数");
            }
            ScheduleEntity main = new ScheduleEntity
            {
                MetaType = task.MetaType,
                CreateTime = DateTime.Now,
                CronExpression = task.CronExpression,
                EndDate = task.EndDate,
                Remark = task.Remark,
                StartDate = task.StartDate,
                Title = task.Title,
                Status = (int)ScheduleStatus.Stop,
                CustomParamsJson = task.CustomParamsJson,
                RunLoop = task.RunLoop,
                TotalRunCount = 0,
                CreateUserName = CurrentUserName
            };
            if (task.MetaType == (int)ScheduleMetaType.Assembly)
            {
                main.AssemblyName = task.AssemblyName;
                main.ClassName = task.ClassName;
            }
            ScheduleHttpOptionEntity httpOption = null;
            if (task.MetaType == (int)ScheduleMetaType.Http)
            {
                httpOption = new ScheduleHttpOptionEntity
                {
                    RequestUrl = task.HttpRequestUrl,
                    Method = task.HttpMethod,
                    ContentType = task.HttpContentType,
                    Headers = task.HttpHeaders,
                    Body = task.HttpBody
                };
            }
            var result = _scheduleService.Add(main, httpOption, task.Keepers, task.Nexts, task.Executors);
            if (result.Status == ResultStatus.Success)
            {
                if (task.RunNow)
                {
                    var start = _scheduleService.Start(main);
                    return ApiResponse(ResultStatus.Success, "任务创建成功！启动状态为：" + (start.Status == ResultStatus.Success ? "成功" : "失败"), main.Id);
                }
            }
            return result;
        }

        /// <summary>
        /// 编辑任务信息
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        [HttpPost]
        //[ApiParamValidation]
        public ServiceResponseMessage Edit([FromForm]ScheduleInfo task)
        {
            var result = _scheduleService.Edit(task);
            return result;
        }

        /// <summary>
        /// 启动一个任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ServiceResponseMessage Start([FromQuery]Guid id)
        {
            var task = _scheduleService.QueryById(id);
            var result = _scheduleService.Start(task);
            return result;
        }

        /// <summary>
        /// 暂停一个任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ServiceResponseMessage Pause([FromQuery]Guid id)
        {
            var result = _scheduleService.Pause(id);
            return result;
        }

        /// <summary>
        /// 立即运行一次
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ServiceResponseMessage RunOnce([FromQuery]Guid id)
        {
            var result = _scheduleService.RunOnce(id);
            return result;
        }

        /// <summary>
        /// 恢复一个任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ServiceResponseMessage Resume([FromQuery]Guid id)
        {
            var result = _scheduleService.Resume(id);
            return result;
        }

        /// <summary>
        /// 停止一个任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ServiceResponseMessage Stop([FromQuery]Guid id)
        {
            var result = _scheduleService.Stop(id);
            return result;
        }
    }
}
