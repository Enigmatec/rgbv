using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Service.Models;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Api.Controllers
{
    /// <summary>
    /// Base Controller for setting up Api response object
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        public string IpAddress => Request.Headers.TryGetValue("X-Forwarded-For", out StringValues value) ? value : HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? null;
        /// <summary>
        /// All none media responses are processed here
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        protected IActionResult ApiResult<T>(AppResult<T> result)
        {
            if (result.HasError || result.StatusCode == StatusCodes.Status400BadRequest)
            {
                return BadRequest(new ApiResult<T>
                {
                    Code = result.StatusCode,
                    Errors = result.Errors,
                    Message = result.Message,
                    Data = result.Data
                });
            }

            if (result.StatusCode == StatusCodes.Status404NotFound)
            {
                return NotFound(
                  new ApiResult<T>
                  {
                      Code = result.StatusCode,
                      Message = result.Message
                  }
                    );
            }
            if (result.StatusCode == StatusCodes.Status401Unauthorized)
            {
                return Unauthorized(
                  new ApiResult<T>
                  {
                      Code = result.StatusCode,
                      Message = result.Message
                  }
                    );
            }

            return Ok(new ApiResult<T>
            {
                Code = result.StatusCode,
                Data = result.Data,
                Message = result.Message,
            });
        }

        /// <summary>
        /// Media related responses are processed heres
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected IActionResult ApiResult(AppResult<MemoryStream> result)
        {
            if (result.HasError)
            {
                return BadRequest(new ApiResult<MemoryStream>
                {
                    Code = result.StatusCode,
                    Errors = result.Errors,
                    Message = result.Message
                });
            }
            if (result.StatusCode == StatusCodes.Status404NotFound)
            {
                return NotFound(
                  new ApiResult<MemoryStream>
                  {
                      Code = result.StatusCode,
                      Message = result.Message
                  }
                    );
            }
            if (result.StatusCode == StatusCodes.Status401Unauthorized)
            {
                return Unauthorized(
                  new ApiResult<MemoryStream>
                  {
                      Code = result.StatusCode,
                      Message = result.Message
                  }
                    );
            }

            if (result.StatusCode == StatusCodes.Status400BadRequest)
            {
                return BadRequest(
                    new ApiResult<MemoryStream>
                    {
                        Code = result.StatusCode,
                        Message = result.Message
                    }
                );
            }

            if (result.Message.EndsWith(".sav"))
            {
                return File(result.Data, "application/x-spss-sav", result.Message);
            }

            return File(result.Data, "application/vnd.openxmlformats", result.Message);
        }

        /// <summary>
        /// For responses from services that have Status, Message and Data
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        [NonAction]
        protected IActionResult Result<TData>(Result<TData> response)
        {
            if (response.IsFailure) return BadRequest(new ApiResult<TData>
            {
                Code = (int)HttpStatusCode.BadRequest,
                Message = response.Error
            });

            return Ok(new ApiResult<TData>
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Success",
                Data = response.Value
            });
        }

        /// <summary>
        /// For responses from services that have Status and Message
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        [NonAction]
        protected IActionResult Result(Result response)
        {
            if (response.IsFailure) return BadRequest(new
            {
                Code = (int)HttpStatusCode.BadRequest,
                Message = response.Error
            });

            return Ok(new
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Success",
            });
        }

        #region | Result Utils |

        [NonAction]
        public IActionResult Response<T>(Result<T> result)
        {
            if (result.IsFailure) return BadRequest(new MessageResult(result.IsSuccess, result.Error));
            return Ok(new DataResult<T>(result.IsSuccess, "Success", result.Value));
        }

        [NonAction]
        public IActionResult Response(Result result)
        {
            if (result.IsFailure) return BadRequest(new MessageResult(result.IsSuccess, result.Error));
            return Ok(new MessageResult(result.IsSuccess, "Success"));
        }

        /// <summary>
        /// For responses from services that have Status, Message and Data
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        [NonAction]
        protected IActionResult Result<TData>((bool status, string message, TData data) p)
        {
            if (!p.status) return BadRequest(new MessageResult(p.status, p.message));

            return Ok(new DataResult<TData>(p.status, p.message, p.data));
        }

        /// <summary>
        /// For responses from services that have Status and Data
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        [NonAction]
        protected IActionResult Result<TData>((bool status, TData data) p)
        {
            if (!p.status) return BadRequest(new DataResult<TData>(p.status, p.data));

            return Ok(new DataResult<TData>(p.status, p.data));
        }

        /// <summary>
        /// For responses from services that have Status and Message
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        [NonAction]
        protected IActionResult Result((bool status, string message) p)
        {
            if (!p.status) return BadRequest(new MessageResult(p.status, p.message));

            return Ok(new MessageResult(p.status, p.message));
        }

        /// <summary>
        /// Use to return results that have data, message and status
        /// </summary>
        /// <typeparam name="T">Type of data to return</typeparam>
        public class DataResult<T> : MessageResult
        {
            public DataResult(bool status, string message, T data) : base(status, message)
            {
                Data = data;
            }

            public DataResult(bool status, T data) : base(status)
            {
                Data = data;
            }

            public T Data { get; set; }
        }

        /// <summary>
        /// Use to return results that have message and status only
        /// </summary>
        public class MessageResult
        {
            public MessageResult(bool status, string message)
            {
                Status = status;
                Message = message;
            }

            public MessageResult(bool status)
            {
                Status = status;
            }

            public bool Status { get; set; }

            public string Message { get; set; }
        }

        #endregion | Result Utils |
    }

    ///Default controller class for swagger URL
    public class HomeController : Controller
    {
        /// <summary>
        /// Directs to the home page of swagger doc
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return Redirect("/swagger/index.html");
        }
    }

    /// <summary>
    /// Api Result class providing a consistent response object for UI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ApiResult<T>
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; }
    }
}